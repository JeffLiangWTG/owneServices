using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DataTools.DbBackupAndRestore.Business;
using log4net;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI.Testing
{
	sealed class DbRestoreControlTest : TestCase
	{
		#region Backup File Selection

		public void TestOnBackupFileSelectedTickBoxesDontChange()
		{
			var restoreControl = new DbRestoreControlForTest();
			restoreControl.InitialiseRestoreManagerWithMock();
			var checkboxTickedValues = new List<bool> { true, false };

			foreach (var checkboxTicked in checkboxTickedValues)
			{
				restoreControl.DifferentialBackupsCheckBoxExposed.Checked = checkboxTicked;
				restoreControl.OnBackupFileSelectedExposed("test.bak");
				AssertEquals(checkboxTicked, restoreControl.DifferentialBackupsCheckBoxExposed.Checked);
				AssertEquals((int)DbRestoreOption.RestoreWithRecovery, restoreControl.DbRestoreOptionComboBoxExposed.SelectedIndex);
			}
		}

		public void TestOnBackupFileSelectedDatabaseNotSet()
		{
			var restoreControl = new DbRestoreControlForTest();
			restoreControl.InitialiseRestoreManagerWithMock();

			restoreControl.DbRestoreDatabaseNameTextBoxExposed.Text = "";
			restoreControl.OnBackupFileSelectedExposed("");

			AssertEquals("Database name must be set", restoreControl.RestoreManagerExposed.InfoMessages.Last());
		}

		public void TestOnBackupFileSelectedServerNotSet()
		{
			var restoreControl = new DbRestoreControlForTest();
			restoreControl.InitialiseRestoreManagerWithMock();

			restoreControl.DbRestoreServerTextBoxExposed.Text = "";
			restoreControl.DbRestoreDatabaseNameTextBoxExposed.Text = "Odyssey";
			restoreControl.OnBackupFileSelectedExposed("");

			AssertEquals("Server name must be set", restoreControl.RestoreManagerExposed.InfoMessages.Last());
		}

		public void TestOnBackupFileSelectedEmptyFilePath()
		{
			var restoreControl = new DbRestoreControlForTest();
			restoreControl.InitialiseRestoreManagerWithMock();

			restoreControl.DbRestoreServerTextBoxExposed.Text = "TestServer";
			restoreControl.DbRestoreDatabaseNameTextBoxExposed.Text = "Odyssey";
			restoreControl.OnBackupFileSelectedExposed("");

			AssertEquals("Backup file path must be set", restoreControl.RestoreManagerExposed.InfoMessages.Last());
		}

		public void TestOnBackupFileSelectedInvalidFilePath()
		{
			var restoreControl = new DbRestoreControlForTest();
			restoreControl.InitialiseRestoreManagerWithMock();

			restoreControl.DbRestoreServerTextBoxExposed.Text = "TestServer";
			restoreControl.DbRestoreDatabaseNameTextBoxExposed.Text = "Odyssey";
			restoreControl.OnBackupFileSelectedExposed("test");

			AssertEquals("\"test\" is not a valid file", restoreControl.RestoreManagerExposed.InfoMessages.Last());
		}

		public void TestOnBackupFileSelectedNormalBAK()
		{
			var restoreControl = new DbRestoreControlForTest();
			restoreControl.InitialiseRestoreManagerWithMock();

			restoreControl.DbRestoreServerTextBoxExposed.Text = "TestServer";
			restoreControl.DbRestoreDatabaseNameTextBoxExposed.Text = "Odyssey";
			restoreControl.OnBackupFileSelectedExposed("test.bak");

			AssertEquals("Examining and preparing files to restore", restoreControl.RestoreManagerExposed.InfoMessages.Last());
		}

		public void TestOnBackupFileSelectedNormalDBK()
		{
			var restoreControl = new DbRestoreControlForTest();
			restoreControl.InitialiseRestoreManagerWithMock();

			restoreControl.DbRestoreServerTextBoxExposed.Text = "TestServer";
			restoreControl.DbRestoreDatabaseNameTextBoxExposed.Text = "Odyssey";
			restoreControl.OnBackupFileSelectedExposed("test.dbk");

			AssertEquals("Examining and preparing files to restore", restoreControl.RestoreManagerExposed.InfoMessages.Last());
		}

		public void TestOnBackupFileSelectedRefDbFBK()
		{
			var restoreControl = new DbRestoreControlForTest();
			restoreControl.InitialiseRestoreManagerWithMock();

			restoreControl.DbRestoreServerTextBoxExposed.Text = "TestServer";
			restoreControl.DbRestoreDatabaseNameTextBoxExposed.Text = "Odyssey";
			restoreControl.OnBackupFileSelectedExposed("CW1Bkp_T-20190207-170619_S-SYDCO-WBSU-1_D-Odyssey_RefDb_Cmr_AU.bak");

			AssertEquals("Examining and preparing files to restore", restoreControl.RestoreManagerExposed.InfoMessages.Last());
		}

		#endregion

		#region Availability Group Selection

		public void TestAddDbToAvailabilityGroup_IsDisabledByDefault()
		{
			// Arrange
			var restoreControl = new DbRestoreControlForTest();
			restoreControl.InitialiseRestoreManagerWithMock();

			// Act
			// Assert
			AssertEquals("Add to Availability Group option must be disabled when server does not belong on any availability group", restoreControl.AddDbToAvailabilityGroupCheckBoxExposed.Enabled, false);
		}

		public void TestAddDbToAvailabilityGroup_AvailabilityGroupIsSpecified()
		{
			// Arrange
			var restoreControl = new DbRestoreControlForTest();
			restoreControl.InitialiseRestoreManagerWithMock();

			// Act
			restoreControl.AvailabilityGroupComboBoxExposed.Enabled = true;
			restoreControl.AddDbToAvailabilityGroupCheckBoxExposed.Checked = true;
			var validationResult = restoreControl.ValidateAvailabilityGroupSelectionExposed();

			// Assert
			AssertEquals("Validation should return false when the availability group is not selected", false, validationResult);
			AssertEquals("Availability Group must be selected", restoreControl.RestoreManagerExposed.InfoMessages.Last());
		}

		public void TestAddDbToAvailabilityGroup_RestoreWithNoRecoveryIsNotSupported()
		{
			// Arrange
			var restoreControl = new DbRestoreControlForTest();
			restoreControl.InitialiseRestoreManagerWithMock();

			// Act
			restoreControl.AvailabilityGroupComboBoxExposed.Enabled = true;
			restoreControl.AddDbToAvailabilityGroupCheckBoxExposed.Checked = true;
			restoreControl.AvailabilityGroupComboBoxExposed.Items.Add("testGroup");
			restoreControl.AvailabilityGroupComboBoxExposed.SelectedIndex = 1;
			restoreControl.DbRestoreOptionComboBoxExposed.SelectedIndex = (int)DbRestoreOption.RestoreWithNoRecovery;
			var validationResult = restoreControl.ValidateAvailabilityGroupSelectionExposed();

			// Assert
			AssertEquals("Validation should return false when RESTORE WITH NORECOVERY is selected", false, validationResult);
			AssertEquals("Restore WITH NORECOVERY is not supported when adding database to availability group", restoreControl.RestoreManagerExposed.InfoMessages.Last());
		}

		public void TestAddDbToAvailabilityGroup_BiServersAreOnPrimaryServer()
		{
			// Arrange
			var restoreControl = new DbRestoreControlForTest();
			restoreControl.InitialiseRestoreManagerWithMock();

			// Act
			restoreControl.DbRestoreServerTextBoxExposed.Text = "MainServer";
			restoreControl.AvailabilityGroupComboBoxExposed.Enabled = true;
			restoreControl.AddDbToAvailabilityGroupCheckBoxExposed.Checked = true;
			restoreControl.AvailabilityGroupComboBoxExposed.Items.Add("testGroup");
			restoreControl.AvailabilityGroupComboBoxExposed.SelectedIndex = 0;
			restoreControl.RestoreBiDatabasesCheckBoxExposed.Checked = true;

			restoreControl.RestoreManagerExposed.IsPrimaryReplica = false;
			restoreControl.DbRestoreServerTextBoxExposed.Enabled = true;
			restoreControl.DbRestoreAuditServerTextBoxExposed.Enabled = true;
			restoreControl.DbRestoreDwServerTextBoxExposed.Enabled = true;

			var validationResult = restoreControl.ValidateAvailabilityGroupSelectionExposed();

			// Assert
			AssertEquals($"Validation checks on BI servers should be skipped when server name is not specified", true, validationResult);

			restoreControl.DbRestoreAuditServerTextBoxExposed.Text = "AuditServer";
			validationResult = restoreControl.ValidateAvailabilityGroupSelectionExposed();
			AssertEquals($"Validation should return false when the Audit server is not a primary server on the availability group", false, validationResult);
			AssertEquals($"Audit Server '{restoreControl.DbRestoreAuditServerTextBoxExposed.Text}' must match the main server '{restoreControl.DbRestoreServerTextBoxExposed.Text}' and also be a primary replica in the Availability Group '{restoreControl.AvailabilityGroupComboBoxExposed.Text}'.", restoreControl.RestoreManagerExposed.InfoMessages.Last());

			restoreControl.RestoreManagerExposed.IsPrimaryReplica = true;
			validationResult = restoreControl.ValidateAvailabilityGroupSelectionExposed();
			AssertEquals("Validation should return false when the Audit server does not match the Main server", false, validationResult);
			AssertEquals($"Audit Server '{restoreControl.DbRestoreAuditServerTextBoxExposed.Text}' must match the main server '{restoreControl.DbRestoreServerTextBoxExposed.Text}' and also be a primary replica in the Availability Group '{restoreControl.AvailabilityGroupComboBoxExposed.Text}'.", restoreControl.RestoreManagerExposed.InfoMessages.Last());

			restoreControl.DbRestoreAuditServerTextBoxExposed.Text = "MainServer";
			validationResult = restoreControl.ValidateAvailabilityGroupSelectionExposed();
			AssertEquals("Validation should return true when the Audit server is a primary server on the availability group", true, validationResult);

			restoreControl.DbRestoreAuditServerTextBoxExposed.Text = "";
			restoreControl.DbRestoreDwServerTextBoxExposed.Text = "EdwServer";
			restoreControl.RestoreManagerExposed.IsPrimaryReplica = false;
			validationResult = restoreControl.ValidateAvailabilityGroupSelectionExposed();
			AssertEquals("Validation should return false when the Data Warehouse server is not a primary server on the availability group", false, validationResult);
			AssertEquals($"Data Warehouse Server '{restoreControl.DbRestoreDwServerTextBoxExposed.Text}' must match the main server '{restoreControl.DbRestoreServerTextBoxExposed.Text}' and also be a primary replica in the Availability Group '{restoreControl.AvailabilityGroupComboBoxExposed.Text}'.", restoreControl.RestoreManagerExposed.InfoMessages.Last());

			restoreControl.RestoreManagerExposed.IsPrimaryReplica = true;
			validationResult = restoreControl.ValidateAvailabilityGroupSelectionExposed();
			AssertEquals("Validation should return false when the Data Warehouse server does not match the Main server", false, validationResult);
			AssertEquals($"Data Warehouse Server '{restoreControl.DbRestoreDwServerTextBoxExposed.Text}' must match the main server '{restoreControl.DbRestoreServerTextBoxExposed.Text}' and also be a primary replica in the Availability Group '{restoreControl.AvailabilityGroupComboBoxExposed.Text}'.", restoreControl.RestoreManagerExposed.InfoMessages.Last());

			restoreControl.DbRestoreDwServerTextBoxExposed.Text = "MainServer";
			validationResult = restoreControl.ValidateAvailabilityGroupSelectionExposed();
			AssertEquals("Validation should return true when the Data Warehouse server is a primary server on the availability group", true, validationResult);
		}

		#endregion

		public void TestDatabaseIsInRestorableState()
		{
			var databaseStates = Enum.GetValues(typeof(DatabaseStatus))
				.Cast<DatabaseStatus>()
				.Where(status => status != DatabaseStatus.Online &&
								status != DatabaseStatus.Standby &&
								status != DatabaseStatus.EmergencyMode &&
								status != DatabaseStatus.Restoring &&
								status != DatabaseStatus.DoesNotExist)
				.ToArray();

			foreach (var status in databaseStates)
			{
				TestDatabaseState(status);
			}
		}

		void TestDatabaseState(DatabaseStatus status)
		{
			// Arrange
			var restoreControl = new DbRestoreControlForTest();
			restoreControl.InitialiseRestoreManagerWithMock();

			var serverName = "TestServer";
			var targetDbName = "Odyssey";

			restoreControl.DbRestoreServerTextBoxExposed.Text = serverName;
			restoreControl.DbRestoreDatabaseNameTextBoxExposed.Text = targetDbName;

			restoreControl.RestoreManagerExposed.DbStatus = status;

			// Act
			restoreControl.CheckBiServerExposed();

			// Assert
			AssertEquals($"The database 'Odyssey' is not in a state that allows for restoration: {status}. Please check its status.", restoreControl.RestoreManagerExposed.InfoMessages.Last());
		}

		public void TestDatabaseInAvailabilityGroup_SecondaryNodeRestoreFails()
		{
			// Arrange
			var restoreControl = new DbRestoreControlForTest();
			restoreControl.InitialiseRestoreManagerWithMock();

			var serverName = "TestServer";
			var targetDbName = "Odyssey";

			restoreControl.DbRestoreServerTextBoxExposed.Text = serverName;
			restoreControl.DbRestoreDatabaseNameTextBoxExposed.Text = targetDbName;
			restoreControl.RestoreManagerExposed.IsPrimaryReplica = false;

			// Act
			restoreControl.CheckBiServerExposed();

			// Assert
			AssertEquals(
				$"The database '{targetDbName}' is part of an Always On availability group. " +
				"Please ensure that the destination server is currently hosting the primary replica before proceeding.",
				restoreControl.RestoreManagerExposed.InfoMessages.Last()
			);
		}

		public void TestBrowseWindowCloseBackUpFileSelected()
		{
			// Arrange
			var browseResult = new DbServerBrowser.BrowseResult();
			using var browseForm = new DbServerBrowseFormForTesting(browseResult, Db.ServerName, false);
			browseResult.FullPath = "testPath";

			// Act
			browseForm.Show();
			browseForm.SetTextbox("blah");
			browseForm.BeginInvoke(new Action(() => browseForm.Close()));

			// Assert
			AssertEquals("blah", browseResult.FullPath);
		}

		[ExpectNoExceptions]
		public void TestCheckBiServerShowWarningWhenDbIsUnreachable()
		{
			// Arrange
			const string serverName = "NotReachableServer";
			const string targetDbName = "NotReachableDb";
			const string expectedInfo = "Failed to connect to database. It might be triggered by incorrect Server/Database value. If problem persists, please contact your administrator.";

			var originalLogger = Logger.Instance.logger;
			var logMock = new Mock<ILog>();
			using var restoreControl = new DbRestoreControlForTest();
			using var disposableAction = new DisposableAction(() => Logger.Instance.logger = originalLogger);
			Logger.Instance.logger = logMock.Object;
			restoreControl.DbRestoreServerTextBoxExposed.Text = serverName;
			restoreControl.DbRestoreDatabaseNameTextBoxExposed.Text = targetDbName;

			// Act
			restoreControl.CheckBiServerExposed();

			// Assert
			logMock.Verify(x => x.Info(It.Is<string>(y => y.Contains($"Event: Information\t{expectedInfo}"))));
		}

		public void TestResetDbFilesClearsListViewAndRestoreDbFiles()
		{
			// Arrange
			var restoreControl = new DbRestoreControlForTest();
			restoreControl.InitialiseRestoreManagerWithMock();
			restoreControl.DbRestoreListViewExposed.Rows.Add("InitialData");
			restoreControl.RestoreDbFilesExposed = new DbFileInfoCollection { new DbFileInfo("AAA", "BBB", "D") };

			// Act
			restoreControl.ResetDbFilesExposed();

			// Assert
			Assert("DbRestoreListView should be cleared", restoreControl.DbRestoreListViewExposed.Rows.Count == 0);
			AssertNull("restoreDbFiles should be null", restoreControl.RestoreDbFilesExposed);
		}

		public void TestRefreshListViewClearsDbRestoreListViewIfRestoreDbFilesNull()
		{
			// Arrange
			var restoreControl = new DbRestoreControlForTest();
			restoreControl.RestoreDbFilesExposed = null;
			restoreControl.DbRestoreListViewExposed.Rows.Add("InitialData");

			// Act
			restoreControl.RefreshListViewExposed();

			// Assert
			Assert("DbRestoreListView should be cleared", restoreControl.DbRestoreListViewExposed.Rows.Count == 0);
		}

		public void TestRefreshListViewAddsVisibleFilesToDbRestoreListView()
		{
			// Arrange
			var restoreControl = new DbRestoreControlForTest();
			restoreControl.RestoreDbFilesExposed = new DbFileInfoCollection
			{
				new DbFileInfo("AAA", "BBB", "D"),
				new DbFileInfo("CCC", "DDD", "L")
			};
			restoreControl.DbRestoreListViewExposed.Rows.Add("InitialData");

			// Act
			restoreControl.RefreshListViewExposed();

			// Assert
			Assert("DbRestoreListView should have 2 rows", restoreControl.DbRestoreListViewExposed.Rows.Count == 2);
			Assert("DbRestoreListView should have correct logical name", restoreControl.DbRestoreListViewExposed.Rows[0].Cells[0].Value.ToString() == "AAA");
			Assert("DbRestoreListView should have correct logical name", restoreControl.DbRestoreListViewExposed.Rows[1].Cells[0].Value.ToString() == "CCC");
		}

		public void TestUpdateDbFilesFolderPathView()
		{
			// Arrange
			var restoreControl = new DbRestoreControlForTest();
			restoreControl.RestoreDbFilesExposed = new DbFileInfoCollection
			{
				new DbFileInfo("AAA", "BBB", "D"),
				new DbFileInfo("CCC", "DDD", "L")
			};
			restoreControl.DbRestoreListViewExposed.Rows.Add("AAA", "AAA");

			// Act
			restoreControl.UpdateDbFilesFolderPathViewExposed();

			// Assert
			Assert("DbRestoreListView should have correct folder path view", restoreControl.RestoreDbFilesExposed[0].FolderPathView == "AAA");
		}

		sealed class DbServerBrowseFormForTesting : DbServerBrowseForm
		{
			public DbServerBrowseFormForTesting(DbServerBrowser.BrowseResult browseResult, string dbServer, bool isFolderOnlyBrowse) : base(browseResult, dbServer, isFolderOnlyBrowse)
			{
			}

			public void SetTextbox(string value)
			{
				if (InvokeRequired)
				{
					BeginInvoke(new Action<string>(SetTextbox), new object[] { value });
					return;
				}
				SelectedPathTextBox.Text = value;
			}
		}
	}
}
