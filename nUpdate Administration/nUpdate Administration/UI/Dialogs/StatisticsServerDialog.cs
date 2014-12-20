﻿// Author: Dominic Beger (Trade/ProgTrade)
// License: Creative Commons Attribution NoDerivs (CC-ND)
// Created: 02-08-2014 20:10

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using nUpdate.Administration.UI.Controls;
using nUpdate.Administration.UI.Popups;

namespace nUpdate.Administration.UI.Dialogs
{
    public partial class StatisticsServerDialog : BaseDialog
    {
        /// <summary>
        ///     The url of the SQL-connection.
        /// </summary>
        public string SqlWebUrl { get; set; }

        /// <summary>
        ///     The name of the SQL-database to use.
        /// </summary>
        public string SqlDatabaseName { get; set; }

        /// <summary>
        ///     The username for the SQL-login.
        /// </summary>
        public string SqlUsername { get; set; }

        public StatisticsServerDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Represents the content of the file for the statistic servers.
        /// </summary>
        public string FileContent { get; set; }

        /// <summary>
        ///     Sets if the dialog should react on key inputs, e. g. when a server should be selected.
        /// </summary>
        public bool ReactsOnKeyDown { get; set; }

        /// <summary>
        ///     Initializes the statistic servers.
        /// </summary>
        private bool InitializeServers()
        {
            if (serverList.Items.Count > 0)
                serverList.Items.Clear();

            string[] serverData;

            try
            {
                FileContent = File.ReadAllText(Program.StatisticServersFilePath);
                if (String.IsNullOrEmpty(FileContent))
                    return true;
                // Stop the execution as no items are there, but return "true" as there are no errors with that.

                serverData = FileContent.Split(new[] {'\n'});
            }
            catch (Exception ex)
            {
                Popup.ShowPopup(this, SystemIcons.Error, "Error while loading the servers.",
                    ex, PopupButtons.Ok);
                return false;
            }

            int currentIndex = 0;
            try
            {
                foreach (string server in serverData)
                {
                    string[] serverDetails = server.Split(new[] {','});
                    var item = new ServerListItem();
                    item.ItemImage = imageList1.Images[0];
                    item.HeaderText = serverDetails[0];
                    item.ItemText = String.Format("Web-URL: \"{0}\" - Database: \"{1}\"",
                        serverDetails[1], serverDetails[0]);
                    serverList.Items.Add(item);

                    currentIndex += 1; // Increase the index value for the current item.
                }
            }
            catch (Exception ex)
            {
                Popup.ShowPopup(this, SystemIcons.Error, String.Format("Error while loading server \"{0}\"",
                    serverData[currentIndex]), ex, PopupButtons.Ok);
                return false;
            }

            return true;
        }

        private void StatisticsServerDialog_Load(object sender, EventArgs e)
        {
            if (!InitializeServers())
            {
                Close();
                return;
            }

            if (ReactsOnKeyDown)
            {
                Popup.ShowPopup(this, SystemIcons.Information, "Selecting a statistics-server.",
                    "To select a statistics server, select one in the list and press \"Enter\".", PopupButtons.Ok);
            }
        }

        private void addServerButton_Click(object sender, EventArgs e)
        {
            var statisticsServerAddDialog = new StatisticsServerAddDialog();
            if (statisticsServerAddDialog.ShowDialog() != DialogResult.OK) 
                return;

            SqlDatabaseName = statisticsServerAddDialog.DatabaseName;
            SqlWebUrl = statisticsServerAddDialog.WebUrl;
            SqlUsername = statisticsServerAddDialog.Username;

            try
            {
                var builder = new StringBuilder(FileContent);
                if (!String.IsNullOrEmpty(builder.ToString()))
                    builder.Append(String.Format("\n{0},{1},{2}", SqlDatabaseName,
                        SqlWebUrl, SqlUsername));
                else
                    builder.Append(String.Format("{0},{1},{2}", SqlDatabaseName,
                        SqlWebUrl, SqlUsername));

                File.WriteAllText(Program.StatisticServersFilePath, builder.ToString());
            }
            catch (Exception ex)
            {
                Popup.ShowPopup(this, SystemIcons.Error, "Error while saving the server.",
                    ex, PopupButtons.Ok);
                return;
            }

            InitializeServers(); // Re-initialize the servers again
        }

        private void deleteServerButton_Click(object sender, EventArgs e)
        {
            if (serverList.SelectedItem != null)
            {
                if (
                    Popup.ShowPopup(this, SystemIcons.Warning, "Delete this server?",
                        "Are you sure that you want to delete this server from the server list?", PopupButtons.YesNo) ==
                    DialogResult.Yes)
                {
                    string[] servers = FileContent.Split('\n');
                    List<string> serversList = servers.ToList();
                    serversList.RemoveAt(serverList.SelectedIndex); // Remove from list

                    try
                    {
                        var builder = new StringBuilder();
                        foreach (string server in serversList)
                        {
                            if (!String.IsNullOrEmpty(builder.ToString()))
                                builder.Append(String.Format("\n{0}", server));
                            else
                                builder.Append(server);
                        }

                        File.WriteAllText(Program.StatisticServersFilePath, builder.ToString());
                    }
                    catch (Exception ex)
                    {
                        Popup.ShowPopup(this, SystemIcons.Error, "Error while saving the server.",
                            ex, PopupButtons.Ok);
                        return;
                    }

                    InitializeServers(); // Re-initialize the servers
                }
            }
        }

        private void StatisticsServerDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (serverList.SelectedItem == null) 
                return;

            if (e.KeyCode != Keys.Enter || !ReactsOnKeyDown) 
                return;

            string[] selectedStatisticServer =
                FileContent.Split('\n')[serverList.SelectedIndex].Split(',');
            SqlDatabaseName = selectedStatisticServer[0];
            SqlWebUrl = selectedStatisticServer[1];
            SqlUsername = selectedStatisticServer[2];

            DialogResult = DialogResult.OK;
        }
    }
}