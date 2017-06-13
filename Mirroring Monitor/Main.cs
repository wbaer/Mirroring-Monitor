//-----------------------------------------------------------------------
// <copyright file="Main.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// <author>Bill Baer</author>
//-----------------------------------------------------------------------

namespace MS.Mirroring.Monitor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    /// <summary>
    /// Contains methods to support monitoring and failover
    /// of databases configured for database mirroring.
    /// </summary>
    public partial class Main : Form
    {
        /// <summary>
        /// Initializes a new instance of the Main class.
        /// </summary>
        public Main()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Queries the specified connection and returns a list
        /// of databases configured for database mirroring.
        /// </summary>
        public void QueryBuilder()
        {
            string connStr = this.txtConnectionString.Text;

            SqlConnection conn = new SqlConnection(connStr);        
            SqlCommand comm = new SqlCommand(
                @"
                SELECT sys.databases.name 'DatabaseName', 'MirrorRole' =
                CASE
                WHEN sys.database_mirroring.mirroring_role = 1 THEN 'Principal'
                WHEN sys.database_mirroring.mirroring_role = 2 THEN 'Mirror'
                ELSE 'Not Mirrored'
                END
                FROM sys.databases, sys.database_mirroring
                WHERE sys.databases.database_id = sys.database_mirroring.database_id
                ORDER BY MirrorRole
                ", 
                conn);
            try
            {
                conn.Open();
                SqlDataAdapter sqlAdapter = new SqlDataAdapter(comm);
                DataSet dataSet = new DataSet(); 
                sqlAdapter.Fill(dataSet);

                this.dataGridView1.DataSource = dataSet.Tables[0].DefaultView;
            } 
            catch (SqlException ex)
            {
                DialogResult message = MessageBox.Show("An error has occurred while establishing a connection to the server." + "\n\n" + ex.Message, "Exception", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                switch (message)
                {
                    case DialogResult.Retry:
                        this.QueryBuilder();
                        break;
                    case DialogResult.Cancel: 
                        break;
                }
            }
        }

        /// <summary>
        /// Executes the Transact-SQL statement specified in the
        /// QueryBuilder method.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void Run_Click(object sender, EventArgs e)
        {
            this.QueryBuilder();
            this.statusMessages.Text = "Transact-SQL executed.";
        }

        /// <summary>
        /// Informs all message pumps that they must terminate,
        /// and then closes all application windows after the
        /// messages have been processed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void ExitMenu_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
