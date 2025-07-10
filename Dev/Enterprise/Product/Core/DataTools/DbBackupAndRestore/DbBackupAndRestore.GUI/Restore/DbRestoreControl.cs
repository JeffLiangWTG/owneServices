using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DataTools.DbBackupAndRestore.Business;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	public partial class DbRestoreControl : TaskPageControl
	{
		public DbRestoreControl()
		{
			InitializeComponent();
			InitialiseListControls();
			InitialiseRestoreManager();
			InitialiseDefaultValues();
		}

		void InitialiseListControls()
		{
			DbRestoreOptionComboBox.SelectedIndex = (int)DbRestoreOption.RestoreWithRecovery;
		}

		protected virtual void InitialiseRestoreManager()
		{
			restoreManager = DbRestoreManagerFactory.Create(
				this,
				DbTools_OnTaskStarted,
				DbTools_OnSubtaskStarted,
				DbTools_OnTaskCompleted,
				DbTools_OnTaskFailed,
				DbTools_OnShowInfoMessage,
				DbTools_OnConfirmationPrompt,
				RestoreManager_OnPromptForReleaseKey
				);
		}

		void InitialiseDefaultValues()
		{
			DbRestoreServerTextBox.Text = Defaults.Instance.ServerName;
			DbRestoreDatabaseNameTextBox.Text = Defaults.Instance.DatabaseName;
			DbRestoreFilePathTextBox.Text = Defaults.Instance.BackupFileName;
			restoreOption = Defaults.Instance.RestoreOption;
			DifferentialBackupsCheckBox.Checked = Defaults.Instance.RestoreDifferentialBackups;
			TransactionLogBackupsCheckBox.Checked = Defaults.Instance.RestoreTransactionLogBackups;

			DbRestoreAuditServerTextBox.Text = Defaults.Instance.AuditServerName;
			AuditDbRestoreFilePathTextBox.Text = Defaults.Instance.AuditBackupFileName;

			DbRestoreDwServerTextBox.Text = Defaults.Instance.DataWarehouseServerName;
			EdwDbRestoreFilePathTextBox.Text = Defaults.Instance.EdwBackupFileName;

			AddDbToAvailabilityGroupCheckBox.Checked = Defaults.Instance.AddDbToAvailabilityGroup;
			AvailabilityGroupComboBox.Items.Insert(0, $"{Defaults.Instance.AvailabilityGroup}");
			AvailabilityGroupComboBox.SelectedIndex = 0;
		}

		#region Restore Database

		protected DbFileInfoCollection restoreDbFiles;
		DbRestoreOption restoreOption;
		string availabilityGroup;
		string serverName;

		protected IDbRestoreManager restoreManager;

		#region Examine Backup File

		void ExamineFileAndPrepareToRestore()
		{
			restoreManager.LogMessage("Examining and preparing files to restore");
			OutputTextBox.Text = "";
			PopulateBackupFileListView();

			if (DbRestoreListView.Rows.Count > 0)
			{
				EnableRestoreMode();
			}
		}

		void PopulateBackupFileListView()
		{
			restoreManager.RefreshExtendedProperties(DbRestoreServerTextBox.Text);

			string auditServer = null;
			string auditDbFilePath = null;
			string dwServer = null;
			string edwDbFilePath = null;

			if (RestoreBiDatabasesCheckBox.Checked)
			{
				auditServer = DbRestoreAuditServerTextBox.Text;
				auditDbFilePath = AuditDbRestoreFilePathTextBox.Text;
				dwServer = DbRestoreDwServerTextBox.Text;
				edwDbFilePath = EdwDbRestoreFilePathTextBox.Text;
			}

			restoreDbFiles = restoreManager.GetBackupDbFileInfoCollection(DbRestoreServerTextBox.Text, DbRestoreFilePathTextBox.Text, auditServer, auditDbFilePath, dwServer, edwDbFilePath);
			bool hasExtendedPropForAllFiles;
			restoreDbFiles = restoreManager.ApplyExtendedProperties(restoreDbFiles, out hasExtendedPropForAllFiles);

			if (restoreDbFiles.Count == 0)
			{
				return;
			}

			DbRestoreListView.Rows.Clear();

			restoreDbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, RestoreOperationalDatabasesCheckBox.Checked);
			restoreDbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, RestoreOperationalDatabasesCheckBox.Checked);
			restoreDbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, RestoreReferenceFilesDatabasesCheckBox.Checked);
			restoreDbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeAuditDB, RestoreBiDatabasesCheckBox.Checked);
			restoreDbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEdwDB, RestoreBiDatabasesCheckBox.Checked);
			restoreDbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeSingleSharedRefDB, false);

			RefreshListView();
		}

		#endregion

		#region Availability Group Selection

		protected void PopulateAvailabilityGroupsComboBox()
		{
			if (string.IsNullOrEmpty(DbRestoreServerTextBox.Text))
			{
				return;
			}

			OutputTextBox.Text = "";
			AvailabilityGroupComboBox.Items.Clear();

			var availabilityGroups = restoreManager.GetAvailabilityGroupsList(DbRestoreServerTextBox.Text)
				.Where(groupName => restoreManager.IsPrimaryReplicaOnAvailabilityGroup(DbRestoreServerTextBox.Text, groupName))
				.ToList();

			if (availabilityGroups.Count > 0)
			{
				AvailabilityGroupComboBox.Items.AddRange(availabilityGroups.ToArray());

				SetDefaultAvailabilityGroup();
			}
			else
			{
				AddDbToAvailabilityGroupCheckBox.Enabled = false;
				AddDbToAvailabilityGroupCheckBox.Checked = false;
			}
		}

		void SetDefaultAvailabilityGroup()
		{
			if (string.IsNullOrEmpty(DbRestoreServerTextBox.Text) || AvailabilityGroupComboBox.Items.Count == 0)
			{
				return;
			}

			AvailabilityGroupComboBox.SelectedIndex = -1;
			AvailabilityGroupComboBox.Enabled = true;
			AddDbToAvailabilityGroupCheckBox.Enabled = true;

			if (!string.IsNullOrEmpty(DbRestoreDatabaseNameTextBox.Text))
			{
				var groupName = restoreManager.GetAvailabilityGroupName(DbRestoreServerTextBox.Text, DbRestoreDatabaseNameTextBox.Text);
				if (!string.IsNullOrEmpty(groupName))
				{
					AvailabilityGroupComboBox.Enabled = false;
					SelectAvailabilityGroup(groupName);
				}
			}

			if (restoreManager.IsServerAvailabilityGroupListener(DbRestoreServerTextBox.Text))
			{
				AvailabilityGroupComboBox.SelectedIndex = 0;
				AvailabilityGroupComboBox.Enabled = false;
			}
		}

		void SelectAvailabilityGroup(string groupName)
		{
			for (var i = 0; i < AvailabilityGroupComboBox.Items.Count; i++)
			{
				if (AvailabilityGroupComboBox.Items[i].ToString().Equals(groupName, StringComparison.OrdinalIgnoreCase))
				{
					AvailabilityGroupComboBox.SelectedIndex = i;
					break;
				}
			}
		}

		void DisableAvailabilityGroupComboBox()
		{
			AddDbToAvailabilityGroupCheckBox.Checked = false;
			AddDbToAvailabilityGroupCheckBox.Enabled = false;
			AvailabilityGroupComboBox.Enabled = false;
			AvailabilityGroupComboBox.Items.Clear();
		}

		#endregion

		void EnableRestoreMode()
		{
			DbRestoreExamineFileButton.Enabled = false;
			DbRestoreListView.Enabled = true;
			DbRestoreOptionComboBox.Enabled = true;
			DbRestoreButton.Enabled = true;
		}

		void DisableRestoreMode()
		{
			DbRestoreExamineFileButton.Enabled =
				   !string.IsNullOrWhiteSpace(DbRestoreServerTextBox.Text)
				&& !string.IsNullOrWhiteSpace(DbRestoreDatabaseNameTextBox.Text)
				&& !string.IsNullOrWhiteSpace(DbRestoreFilePathTextBox.Text);

			DbRestoreListView.Rows.Clear();
			DbRestoreListView.Enabled = false;
			DbRestoreOptionComboBox.Enabled = false;
			DbRestoreButton.Enabled = false;
		}

		void SetRestoreExtraDbsCheckboxesAccessability()
		{
			var shouldEnableRestoreExtraDbsCheckboxes = ShouldEnableRestoreExtraDbsCheckboxes();
			RestoreOperationalDatabasesCheckBox.Enabled = shouldEnableRestoreExtraDbsCheckboxes;
			RestoreOperationalDatabasesCheckBox.Checked = shouldEnableRestoreExtraDbsCheckboxes;
			RestoreReferenceFilesDatabasesCheckBox.Enabled = shouldEnableRestoreExtraDbsCheckboxes;
			RestoreReferenceFilesDatabasesCheckBox.Checked = shouldEnableRestoreExtraDbsCheckboxes;
			RestoreBiDatabasesCheckBox.Enabled = shouldEnableRestoreExtraDbsCheckboxes;
		}

		bool ShouldEnableRestoreExtraDbsCheckboxes()
		{
			return Utilities.DetermineFileValidity(DbRestoreFilePathTextBox.Text.Trim()) != FileValidity.NotValid;
		}

		void RestoreDatabases()
		{
			restoreOption = (DbRestoreOption)DbRestoreOptionComboBox.SelectedIndex;
			availabilityGroup = AvailabilityGroupComboBox.Text;
			serverName = DbRestoreServerTextBox.Text;
			if (restoreManager.IsServerAvailabilityGroupListener(DbRestoreServerTextBox.Text))
			{
				serverName = restoreManager.GetPrimaryReplicaOnAvailabilityGroup(DbRestoreServerTextBox.Text, availabilityGroup);
			}

			StartTaskOnASeparateThread(OnRestoreDatabasesTaskThreadStart);
		}

		void OnRestoreDatabasesTaskThreadStart()
		{
			try
			{
				var mainServerConfiguration = new DbServerConfiguration(serverName, DbRestoreFilePathTextBox.Text);
				var auditServerConfiguration = new AuditDbServerConfiguration(RestoreBiDatabasesCheckBox.Checked ? DbRestoreAuditServerTextBox.Text : "", AuditDbRestoreFilePathTextBox.Text);
				var edwServerConfiguration = new EdwDbServerConfiguration(RestoreBiDatabasesCheckBox.Checked ? DbRestoreDwServerTextBox.Text : "", EdwDbRestoreFilePathTextBox.Text);
				var dbRestoreSettings = new DbRestoreSettings(DbRestoreDatabaseNameTextBox.Text, restoreDbFiles, restoreOption, RestoreOperationalDatabasesCheckBox.Checked, DifferentialBackupsCheckBox.Checked, TransactionLogBackupsCheckBox.Checked, AddDbToAvailabilityGroupCheckBox.Checked, availabilityGroup);
				restoreManager.RestoreDatabases(mainServerConfiguration, auditServerConfiguration, edwServerConfiguration, dbRestoreSettings);
			}
			finally
			{
				Invoke(new ThreadStart(OnTaskThreadFinished));
			}
		}

		#endregion

		#region Event Handlers

		protected void RestoreBrowseButton_Click(object sender, EventArgs e)
		{
			var browseResult = new DbServerBrowser.BrowseResult();
			browseResult.FullPath = DbRestoreFilePathTextBox.Text;
			var browseForm = new DbServerBrowseForm(browseResult, DbRestoreServerTextBox.Text, false);
			browseForm.StartPosition = FormStartPosition.CenterScreen;

			if (browseForm.ShowDialog() == DialogResult.OK)
			{
				ResetDbFiles();
				DbRestoreFilePathTextBox.Text = browseResult.FullPath;
			}
		}

		protected void ResetDbFiles()
		{
			DbRestoreListView.Rows.Clear();
			restoreDbFiles = null;
		}

		void DbRestoreServerTextBox_TextChanged(object sender, EventArgs e)
		{
			DisableRestoreMode();
			RestoreBrowseButton.Enabled = !string.IsNullOrWhiteSpace(DbRestoreServerTextBox.Text);
			RefreshAvailabilityGroupsButton.Enabled = !string.IsNullOrWhiteSpace(DbRestoreServerTextBox.Text);
			DbRestoreDatabaseNameTextBox.Clear();
			DbRestoreDatabaseNameTextBox.Enabled = false;
			DisableAvailabilityGroupComboBox();
		}

		void DbRestoreDatabaseNameTextBox_TextChanged(object sender, EventArgs e)
		{
			DisableRestoreMode();
		}

		void DbRestoreFilePathTextBox_TextChanged(object sender, EventArgs e)
		{
			DisableRestoreMode();
			SetRestoreExtraDbsCheckboxesAccessability();

			if (!string.IsNullOrWhiteSpace(DbRestoreFilePathTextBox.Text))
			{
				if (string.Equals(DbRestoreAuditServerTextBox.Text, DbRestoreServerTextBox.Text, StringComparison.OrdinalIgnoreCase))
				{
					AuditDbRestoreFilePathTextBox.Text = new Regex(@"\.bak$", RegexOptions.IgnoreCase).Replace(DbRestoreFilePathTextBox.Text, "_Audit.bak");
				}
				if (string.Equals(DbRestoreDwServerTextBox.Text, DbRestoreServerTextBox.Text, StringComparison.OrdinalIgnoreCase))
				{
					EdwDbRestoreFilePathTextBox.Text = new Regex(@"\.bak$", RegexOptions.IgnoreCase).Replace(DbRestoreFilePathTextBox.Text, "_EDW.bak");
				}
			}
		}

		void DbRestoreExamineFileButton_Click(object sender, EventArgs e)
		{
			Parent.Parent.Enabled = false;

			try
			{
				OnBackupFileSelected(DbRestoreFilePathTextBox.Text.Trim());
			}
			finally
			{
				Parent.Parent.Enabled = true;
			}
		}

		protected void OnBackupFileSelected(string path)
		{
			if (string.IsNullOrWhiteSpace(DbRestoreDatabaseNameTextBox.Text))
			{
				restoreManager.LogMessage("Database name must be set");
				return;
			}

			if (string.IsNullOrWhiteSpace(DbRestoreServerTextBox.Text))
			{
				restoreManager.LogMessage("Server name must be set");
				return;
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				restoreManager.LogMessage("Backup file path must be set");
				return;
			}

			var fileValidity = Utilities.DetermineFileValidity(path);
			switch (fileValidity)
			{
				case FileValidity.NotValid:
					restoreManager.LogMessage(string.Format(CultureInfo.InvariantCulture, "\"{0}\" is not a valid file", path));
					return;
				case FileValidity.ValidDepStandard:
				case FileValidity.ValidDepDBK:
				case FileValidity.ValidDepFBK:
					restoreManager.LogMessage(string.Format(CultureInfo.InvariantCulture, "\"{0}\" dependent database backup file naming scheme", path));
					break;
				default:
					break;
			}

			DbRestoreOptionComboBox.SelectedIndex = (int)DbRestoreOption.RestoreWithRecovery;
			ExamineFileAndPrepareToRestore();
		}

		static void RestoreManager_OnPromptForReleaseKey(SessionInfo sessionInfo)
		{
			var releaseForm = new DbOverwriteReleaseForm(sessionInfo);
			releaseForm.ShowDialog();
		}

		void DbRestoreButton_Click(object sender, EventArgs e)
		{
			if (ValidateAvailabilityGroupSelection() && ValidateTargetDatabaseStatus())
			{
				UpdateDbFilesFolderPathView();
				RefreshListView();
				RestoreDatabases();
			}
		}

		protected bool ValidateAvailabilityGroupSelection()
		{
			if (AddDbToAvailabilityGroupCheckBox.Checked)
			{
				if (string.IsNullOrWhiteSpace(AvailabilityGroupComboBox.Text))
				{
					restoreManager.LogMessage("Availability Group must be selected");
					return false;
				}

				if (RestoreBiDatabasesCheckBox.Checked)
				{
					if (!string.IsNullOrEmpty(DbRestoreAuditServerTextBox.Text) &&
						!string.Equals(DbRestoreAuditServerTextBox.Text, DbRestoreServerTextBox.Text, StringComparison.OrdinalIgnoreCase))
					{
						restoreManager.LogMessage($"Audit Server '{DbRestoreAuditServerTextBox.Text}' must match the main server '{DbRestoreServerTextBox.Text}' and also be a primary replica in the Availability Group '{AvailabilityGroupComboBox.Text}'.");
						return false;
					}
					if (!string.IsNullOrEmpty(DbRestoreDwServerTextBox.Text) &&
						!string.Equals(DbRestoreDwServerTextBox.Text, DbRestoreServerTextBox.Text, StringComparison.OrdinalIgnoreCase))
					{
						restoreManager.LogMessage($"Data Warehouse Server '{DbRestoreDwServerTextBox.Text}' must match the main server '{DbRestoreServerTextBox.Text}' and also be a primary replica in the Availability Group '{AvailabilityGroupComboBox.Text}'.");
						return false;
					}
				}

				if (DbRestoreOptionComboBox.SelectedIndex == (int)DbRestoreOption.RestoreWithNoRecovery)
				{
					restoreManager.LogMessage($"Restore WITH NORECOVERY is not supported when adding database to availability group");
					return false;
				}
			}

			return true;
		}

		bool ValidateTargetDatabaseStatus()
		{
			var groupName = restoreManager.GetAvailabilityGroupName(DbRestoreServerTextBox.Text, DbRestoreDatabaseNameTextBox.Text);
			if (restoreManager.IsDbPartOfAlwaysOn(DbRestoreServerTextBox.Text, DbRestoreDatabaseNameTextBox.Text) &&
				!restoreManager.IsPrimaryReplicaOnAvailabilityGroup(DbRestoreServerTextBox.Text, groupName))
			{
				restoreManager.LogMessage(
					$"The database '{DbRestoreDatabaseNameTextBox.Text}' is part of an Always On availability group. " +
					"Please ensure that the destination server is currently hosting the primary replica before proceeding."
				);
				return false;
			}

			var dbStatus = restoreManager.GetDatabaseStatus(DbRestoreServerTextBox.Text, DbRestoreDatabaseNameTextBox.Text);
			if (!IsDatabaseInRestorableState(dbStatus))
			{
				restoreManager.LogMessage($"The database '{DbRestoreDatabaseNameTextBox.Text}' is not in a state that allows for restoration: {dbStatus}. Please check its status.");
				return false;
			}
			return true;
		}

		void DbRestoreServerTextBox_Leave(object sender, EventArgs e)
		{
			DbRestoreServerTextBox.Text = Utilities.GetLocalMachineNameForLocalhost(DbRestoreServerTextBox.Text.Trim());
		}

		void RestoreOperationalDatabasesCheckBox_Click(object sender, EventArgs e)
		{
			if (restoreDbFiles != null)
			{
				restoreDbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, RestoreOperationalDatabasesCheckBox.Checked);
				restoreDbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, RestoreOperationalDatabasesCheckBox.Checked);
			}

			RefreshListView();
		}

		void RestoreReferenceFilesDatabasesCheckBox_Click(object sender, EventArgs e)
		{
			restoreDbFiles?.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, RestoreReferenceFilesDatabasesCheckBox.Checked);
			RefreshListView();
		}

		void RestoreBiDatabasesCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			DisableRestoreMode();

			DbRestoreAuditServerTextBox.Enabled = RestoreBiDatabasesCheckBox.Checked;
			AuditDbRestoreFilePathTextBox.Enabled = RestoreBiDatabasesCheckBox.Checked;
			AuditRestoreBrowseButton.Enabled = RestoreBiDatabasesCheckBox.Checked;
			DbRestoreDwServerTextBox.Enabled = RestoreBiDatabasesCheckBox.Checked;
			EdwDbRestoreFilePathTextBox.Enabled = RestoreBiDatabasesCheckBox.Checked;
			EdwRestoreBrowseButton.Enabled = RestoreBiDatabasesCheckBox.Checked;

			if (RestoreBiDatabasesCheckBox.Checked)
			{
				if (string.IsNullOrWhiteSpace(DbRestoreAuditServerTextBox.Text))
				{
					DbRestoreAuditServerTextBox.Text = DbRestoreServerTextBox.Text;
				}
				DbRestoreDwServerTextBox.Enabled = RestoreBiDatabasesCheckBox.Checked;
				if (string.IsNullOrWhiteSpace(DbRestoreDwServerTextBox.Text))
				{
					DbRestoreDwServerTextBox.Text = DbRestoreServerTextBox.Text;
				}
			}
		}

		protected void RefreshListView()
		{
			DbRestoreListView.Rows.Clear();

			if (restoreDbFiles == null)
			{
				return;
			}

			foreach (DbFileInfo dbFile in restoreDbFiles)
			{
				if (dbFile.Visible)
				{
					DbRestoreListView.Rows.Add(dbFile.LogicalName, dbFile.FolderPathView);
				}
			}
		}

		protected void UpdateDbFilesFolderPathView()
		{
			for (var i = 0; i < DbRestoreListView.Rows.Count; i++)
			{
				var row = DbRestoreListView.Rows[i];
				var cells = row.Cells;
				var folder = cells[1].Value?.ToString().Trim();
				var name = cells[0].Value?.ToString();

				for (var j = 0; j < restoreDbFiles.Count; j++)
				{
					if (name == restoreDbFiles[j].LogicalName && folder != restoreDbFiles[j].FolderPathView)
					{
						restoreDbFiles[j].FolderPathView = folder;
					}
				}
			}
		}

		void DbRestoreDatabaseNameTextBox_Leave(object sender, EventArgs e)
		{
			DbRestoreDatabaseNameTextBox.Text = DbRestoreDatabaseNameTextBox.Text.Trim();
			SetDefaultAvailabilityGroup();
			CheckBiServer();
		}

		void DbRestoreFilePathTextBox_Leave(object sender, EventArgs e)
		{
			DbRestoreFilePathTextBox.Text = DbRestoreFilePathTextBox.Text.Trim();
		}

		void DbRestoreDwServerTextBox_Leave(object sender, EventArgs e)
		{
			DbRestoreDwServerTextBox.Text = Utilities.GetLocalMachineNameForLocalhost(DbRestoreDwServerTextBox.Text.Trim());
		}

		void DbRestoreDwServerTextBox_TextChanged(object sender, EventArgs e)
		{
			DisableRestoreMode();
			if (string.Equals(DbRestoreDwServerTextBox.Text, DbRestoreServerTextBox.Text, StringComparison.OrdinalIgnoreCase))
			{
				EdwDbRestoreFilePathTextBox.Text = new Regex(@"\.bak$", RegexOptions.IgnoreCase).Replace(DbRestoreFilePathTextBox.Text, "_EDW.bak");
			}

			RestoreBiDatabasesCheckBox.Checked = !string.IsNullOrWhiteSpace(DbRestoreAuditServerTextBox.Text) || !string.IsNullOrWhiteSpace(DbRestoreDwServerTextBox.Text);
			EdwDbRestoreFilePathTextBox.Enabled = !string.IsNullOrWhiteSpace(DbRestoreDwServerTextBox.Text);
			EdwRestoreBrowseButton.Enabled = !string.IsNullOrWhiteSpace(DbRestoreDwServerTextBox.Text);
		}

		void EdwRestoreBrowseButton_Click(object sender, EventArgs e)
		{
			var browseResult = new DbServerBrowser.BrowseResult();
			browseResult.FullPath = EdwDbRestoreFilePathTextBox.Text;
			var browseForm = new DbServerBrowseForm(browseResult, DbRestoreDwServerTextBox.Text, false);
			browseForm.StartPosition = FormStartPosition.CenterScreen;

			if (browseForm.ShowDialog() == DialogResult.OK)
			{
				EdwDbRestoreFilePathTextBox.Text = browseResult.FullPath;
			}
		}

		protected void CheckBiServer()
		{
			var serverName = DbRestoreServerTextBox.Text;
			var targetDbName = DbRestoreDatabaseNameTextBox.Text;
			var auditServerName = DbRestoreAuditServerTextBox.Text;
			var dwServerName = DbRestoreDwServerTextBox.Text;

			if (!string.IsNullOrWhiteSpace(serverName)
				&& !string.IsNullOrWhiteSpace(targetDbName)
				&& string.IsNullOrWhiteSpace(dwServerName)
				&& string.IsNullOrWhiteSpace(auditServerName))
			{
				try
				{
					var dbStatus = restoreManager.GetDatabaseStatus(serverName, targetDbName);
					restoreManager.LogMessage("Current database status: " + dbStatus);
					if (dbStatus == DatabaseStatus.Restoring)
					{
						restoreManager.LogMessage("You must apply a differential backup to complete the restoration");
						return;
					}

					if (!ValidateTargetDatabaseStatus())
					{
						return;
					}

					var auditServer = restoreManager.GetAuditServer(serverName, targetDbName);
					DbRestoreAuditServerTextBox.Enabled = true;
					DbRestoreAuditServerTextBox.Text = auditServer;

					var dwServer = restoreManager.GetDataWarehouseServer(serverName, targetDbName);
					DbRestoreDwServerTextBox.Enabled = true;
					DbRestoreDwServerTextBox.Text = dwServer;

					RestoreBiDatabasesCheckBox.Enabled = true;
				}
				catch (SqlException sqlEx) when(new DbErrorMatch(sqlEx).IsInfrastructureDbError)
				{
					restoreManager.LogMessage($"Failed to connect to database. It might be triggered by incorrect Server/Database value. If problem persists, please contact your administrator.\r\n\r\nDetail Message: {sqlEx.Message}");
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					restoreManager.LogMessage($"Failed to update server and database information because of the error.\r\n{e.ToString()}");
				}
			}
		}

		bool IsDatabaseInRestorableState(DatabaseStatus dbStatus) => dbStatus == DatabaseStatus.Online ||
			dbStatus == DatabaseStatus.Standby ||
			dbStatus == DatabaseStatus.EmergencyMode ||
			dbStatus == DatabaseStatus.Restoring ||
			dbStatus == DatabaseStatus.DoesNotExist;

		void DbRestoreAuditServerTextBox_Leave(object sender, EventArgs e)
		{
			DbRestoreAuditServerTextBox.Text = Utilities.GetLocalMachineNameForLocalhost(DbRestoreAuditServerTextBox.Text.Trim());
		}

		void DbRestoreAuditServerTextBox_TextChanged(object sender, EventArgs e)
		{
			DisableRestoreMode();
			if (string.Equals(DbRestoreAuditServerTextBox.Text, DbRestoreServerTextBox.Text, StringComparison.OrdinalIgnoreCase))
			{
				AuditDbRestoreFilePathTextBox.Text = new Regex(@"\.bak$", RegexOptions.IgnoreCase).Replace(DbRestoreFilePathTextBox.Text, "_Audit.bak");
			}

			RestoreBiDatabasesCheckBox.Checked = !string.IsNullOrWhiteSpace(DbRestoreAuditServerTextBox.Text) || !string.IsNullOrWhiteSpace(DbRestoreDwServerTextBox.Text);
			AuditDbRestoreFilePathTextBox.Enabled = !string.IsNullOrWhiteSpace(DbRestoreAuditServerTextBox.Text);
			AuditRestoreBrowseButton.Enabled = !string.IsNullOrWhiteSpace(DbRestoreAuditServerTextBox.Text);
		}

		void AuditRestoreBrowseButton_Click(object sender, EventArgs e)
		{
			var browseResult = new DbServerBrowser.BrowseResult();
			browseResult.FullPath = AuditDbRestoreFilePathTextBox.Text;
			var browseForm = new DbServerBrowseForm(browseResult, DbRestoreAuditServerTextBox.Text, false);
			browseForm.StartPosition = FormStartPosition.CenterScreen;

			if (browseForm.ShowDialog() == DialogResult.OK)
			{
				AuditDbRestoreFilePathTextBox.Text = browseResult.FullPath;
			}
		}

		void AuditDbRestoreFilePathTextBox_TextChanged(object sender, EventArgs e)
		{
			DisableRestoreMode();
		}

		void EdwDbRestoreFilePathTextBox_TextChanged(object sender, EventArgs e)
		{
			DisableRestoreMode();
		}

		void RefreshAvailabilityGroupsButton_Click(object sender, EventArgs e)
		{
			this.Parent.Parent.Enabled = false;

			try
			{
				PopulateAvailabilityGroupsComboBox();
				DbRestoreDatabaseNameTextBox.Enabled = true;
			}
			finally
			{
				this.Parent.Parent.Enabled = true;
			}
		}

		#endregion

		void AddDbToAvailabilityGroupCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (!AddDbToAvailabilityGroupCheckBox.Checked)
			{
				AvailabilityGroupComboBox.Enabled = false;
			}
		}
	}
}
