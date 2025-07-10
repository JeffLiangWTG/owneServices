using System;
using System.IO;
using System.Security.AccessControl;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.LogShipping.Setup;
using NUnit.Framework;

namespace Enterprise.LogShipping.Testing
{
	sealed class ScreenNavigatorTest : LogShippingTestFixture
	{
		public void TestStep3PrimaryServer()
		{
			using (DbTestHelper dbTestHelper = new DbTestHelper())
			{
				ScreenNavigatorForTesting navigator = new ScreenNavigatorForTesting { TestPrimaryAndSecondaryServersDiffer = false };
				navigator.CurrentScreen_Exposed = ScreenEnum.PrimaryServer;
				navigator.SetupInfo.SecondaryServer = new SqlServerInfoForTesting(dbTestHelper.TestConnection.ServerNameWithoutInstance, dbTestHelper.TestConnection.ServerInstanceName, dbTestHelper.TestConnection.ServerVersionNumber.ToString());

				//Check when server was not entered
				SqlServerInfo primaryServer = null;
				string expectedErrorMessage = "Error: Server name was not entered.";
				Assert(!navigator.MoveNext(primaryServer));
				AssertEquals("Server name was not entered.", expectedErrorMessage, navigator.Message);
				AssertNull("PrimaryServer", navigator.SetupInfo.PrimaryServer);

				AssertEquals("Current screen was not changed", ScreenEnum.PrimaryServer, navigator.CurrentScreen_Exposed);

				//Check primary server with other version than secondary server
				string otherVersion = "9.0.0.0";
				primaryServer = new SqlServerInfoForTesting(dbTestHelper.TestConnection.ServerNameWithoutInstance, dbTestHelper.TestConnection.ServerInstanceName, otherVersion);
				expectedErrorMessage = "Error: Primary server version should be same as secondary server version.";
				Assert(!navigator.MoveNext(primaryServer));
				AssertEquals("Secondary and Primary servers should have same major versions", expectedErrorMessage, navigator.Message);
				AssertNull("PrimaryServer", navigator.SetupInfo.PrimaryServer);

				AssertEquals("Current screen was not changed", ScreenEnum.PrimaryServer, navigator.CurrentScreen_Exposed);

				//Check server using sql server string name
				string assertHeaderMessage = "Should move next successfully (Check server and get Enterprise databases), but failed with message:\r\n";

				bool moveNextOk = navigator.MoveNext(Db.ServerName);
				Assert(assertHeaderMessage + navigator.Message, moveNextOk);

				AssertNotNull("PrimaryServer", navigator.SetupInfo.PrimaryServer);
				Assert("Should be at least current enterprise database", navigator.PrimaryServerEnterpriseDatabases.Length > 0);
				AssertEquals("Next step", ScreenEnum.PrimaryDatabase, navigator.CurrentScreen_Exposed);

				navigator.MoveBack();
				AssertEquals("Previous step", ScreenEnum.PrimaryServer, navigator.CurrentScreen_Exposed);

				//Check server using SqlServerInfo
				primaryServer = navigator.SetupInfo.SecondaryServer;
				Assert("Check server and get Enterprise databases", navigator.MoveNext(primaryServer));
				AssertNotNull("PrimaryServer", navigator.SetupInfo.PrimaryServer);
				Assert("Should be at least current enterprise database", navigator.PrimaryServerEnterpriseDatabases.Length > 0);
				AssertEquals("Next step", ScreenEnum.PrimaryDatabase, navigator.CurrentScreen_Exposed);

				navigator.MoveBack();
				AssertEquals("Previous step", ScreenEnum.PrimaryServer, navigator.CurrentScreen_Exposed);
			}
		}

		public void TestStep3PrimaryServerNotTheSameAsSecondaryServer()
		{
			using (DbTestHelper dbTestHelper = new DbTestHelper())
			{
				ScreenNavigatorForTesting navigator = new ScreenNavigatorForTesting { TestPrimaryAndSecondaryServersDiffer = true };
				navigator.CurrentScreen_Exposed = ScreenEnum.PrimaryServer;
				navigator.SetupInfo.SecondaryServer = new SqlServerInfoForTesting(dbTestHelper.TestConnection.ServerNameWithoutInstance, dbTestHelper.TestConnection.ServerInstanceName, dbTestHelper.TestConnection.ServerVersionNumber.ToString());
				SqlServerInfoForTesting primaryServer = new SqlServerInfoForTesting(dbTestHelper.TestConnection.ServerNameWithoutInstance, dbTestHelper.TestConnection.ServerInstanceName, dbTestHelper.TestConnection.ServerVersionNumber.ToString());
				Assert(!navigator.MoveNext(primaryServer));
				AssertEquals("Error: Primary and secondary servers cannot be the same server.", navigator.Message);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStep4PrimaryDatabase()
		{
			using (var dbTestHelper = new DbTestHelper())
			{
				var navigator = new ScreenNavigatorForTesting();
				navigator.CurrentScreen_Exposed = ScreenEnum.PrimaryDatabase;
				navigator.SetupInfo.SecondaryServer = new SqlServerInfoForTesting(dbTestHelper.TestConnection.ServerNameWithoutInstance, dbTestHelper.TestConnection.ServerInstanceName, dbTestHelper.TestConnection.ServerVersionNumber.ToString());
				navigator.SetupInfo.PrimaryServer = navigator.SetupInfo.SecondaryServer;
				var testInfo = new LogShippingInfo();
				FillSetupInfoWithTestData(testInfo);
				navigator.ExistingSecondaryDatabases_Exposed = new[] { testInfo };

				//When database was not selected
				var primaryDatabase = string.Empty;
				var expectedErrorMessage = "Error: Database was not selected.";
				Assert(!navigator.MoveNext(primaryDatabase));
				AssertEquals("Database was not selected", expectedErrorMessage, navigator.Message);
				AssertNull("PrimaryDatabase", navigator.SetupInfo.MainDatabase);

				AssertEquals("Current screen was not changed", ScreenEnum.PrimaryDatabase, navigator.CurrentScreen_Exposed);

				//When secondary database is already configured
				primaryDatabase = navigator.ExistingLogShippingConfigurations[0].MainDatabase.DatabaseName;
				expectedErrorMessage = string.Format("Error: Secondary database - [{0}] is already configured for selected primary database.",
					navigator.ExistingLogShippingConfigurations[0].MainDatabase.SecondaryDatabaseName);
				Assert("MoveNext when secondary database is already configured", !navigator.MoveNext(primaryDatabase));
				Assert(string.Format("Expected error should start with: '{0}', but was {1}", expectedErrorMessage, navigator.Message),
					navigator.Message.StartsWith(expectedErrorMessage));

				AssertEquals("Current screen was not changed", ScreenEnum.PrimaryDatabase, navigator.CurrentScreen_Exposed);

				//When secondary database is not configured
				primaryDatabase = Db.DatabaseName;

				try
				{
					dbTestHelper.CreateDatabase(primaryDatabase + "_SD999");

					Assert("Set primary database", navigator.MoveNext(primaryDatabase));
					AssertEquals("PrimaryDatabase", primaryDatabase, navigator.SetupInfo.MainDatabase.DatabaseName);
					AssertEquals("SecondaryDatabase", primaryDatabase, navigator.SetupInfo.MainDatabase.SecondaryDatabaseName);
					AssertEquals("SecondaryDatabasesExist", false, navigator.SetupInfo.SecondaryDatabasesExist());

					AssertEquals("Next step", ScreenEnum.PrimaryEDocsDatabases, navigator.CurrentScreen_Exposed);

					navigator.MoveBack();
					AssertEquals("Previous step", ScreenEnum.PrimaryDatabase, navigator.CurrentScreen_Exposed);
				}
				finally
				{
					dbTestHelper.DropDatabase(primaryDatabase + "_SD999");
				}
			}
		}

		public void TestNavigationRememberHowToGoBack()
		{
			ScreenNavigatorForTesting navigator = new ScreenNavigatorForTesting();
			navigator.CurrentScreen_Exposed = ScreenEnum.SecondaryDatabase;

			//Move next
			//SecondaryDatabase -> SourceDirectory
			Assert("Jump to SourceDirectory step", navigator.MoveNext(new object[] { SetupAction.Change, new LogShippingInfo() }));
			AssertEquals("Next step", ScreenEnum.PrimaryEDocsDatabases, navigator.CurrentScreen);
			AssertEquals("Previous step", ScreenEnum.SecondaryDatabase, navigator.PreviousScreen);

			//SourceDirectory -> LocalCopyDirectory
			navigator.CurrentScreen_Exposed = ScreenEnum.LocalCopyDirectory;
			AssertEquals("Next step", ScreenEnum.LocalCopyDirectory, navigator.CurrentScreen);
			AssertEquals("Previous step", ScreenEnum.PrimaryEDocsDatabases, navigator.PreviousScreen);

			//LocalCopyDirectory -> InitializeSecondaryDb
			using (TempDirectory tempDir = new TempDirectory())
			{
				Assert("Move to next step", navigator.MoveNext(tempDir.DirectoryName));
				AssertEquals("Next step", ScreenEnum.InitializeSecondaryDb, navigator.CurrentScreen);
				AssertEquals("Previous step", ScreenEnum.LocalCopyDirectory, navigator.PreviousScreen);
			}

			//Move Back
			//InitializeSecondaryDb -> LocalCopyDirectory
			navigator.MoveBack();
			AssertEquals("Current step", ScreenEnum.LocalCopyDirectory, navigator.CurrentScreen);
			AssertEquals("Previous step", ScreenEnum.PrimaryEDocsDatabases, navigator.PreviousScreen);

			//LocalCopyDirectory -> SourceDirectory
			navigator.MoveBack();
			AssertEquals("Current step", ScreenEnum.PrimaryEDocsDatabases, navigator.CurrentScreen);
			AssertEquals("Previous step", ScreenEnum.SecondaryDatabase, navigator.PreviousScreen);

			//SourceDirectory -> SecondaryDatabase
			navigator.MoveBack();
			AssertEquals("Current step", ScreenEnum.SecondaryDatabase, navigator.CurrentScreen);
		}

		public void TestStep1SecondaryServer()
		{
			using (DbTestHelper dbTestHelper = new DbTestHelper())
			{
				ScreenNavigatorForTesting navigator = new ScreenNavigatorForTesting();
				AssertEquals("Start Screen", ScreenEnum.SecondaryServer, navigator.CurrentScreen_Exposed);

				//Check when server instance was not entered
				SqlServerInfo secondaryServer = null;
				string expectedErrorMessage = "Error: Server instance was not entered";
				Assert(!navigator.MoveNext(secondaryServer));
				AssertEquals("Server instance was not entered", expectedErrorMessage, navigator.Message);
				AssertNull("SecondaryServer", navigator.SetupInfo.SecondaryServer);

				AssertEquals("Current screen was not changed", ScreenEnum.SecondaryServer, navigator.CurrentScreen_Exposed);

				//Check server using SqlServerInfo
				secondaryServer = new SqlServerInfoForTesting(dbTestHelper.TestConnection.ServerNameWithoutInstance, dbTestHelper.TestConnection.ServerInstanceName, dbTestHelper.TestConnection.ServerVersionNumber.ToString());

				Assert("Check server, run SQl Agent", navigator.MoveNext(secondaryServer));
				AssertNotNull("SecondaryServer", navigator.SetupInfo.SecondaryServer);
				Assert("TestSQLServerAgent stared", navigator.TestSQLServerAgentStared);

				string checkPermissionSql = "SELECT * FROM fn_my_permissions('msdb..log_shipping_secondary_databases', 'object') where permission_name = 'Select'";
				object result = Db.Connection.ExecuteScalar(checkPermissionSql);
				if (result == null)
				{
					Assert(true);//skip this bit of test because doesn't have nessessary permission;
				}
				else
				{
					int secondaryDbCount = GetCountOfLSStandByDbs();
					AssertEquals("Existing secondary databases", secondaryDbCount, navigator.ExistingLogShippingConfigurations.Length);
					if (secondaryDbCount > 0)
					{
						AssertEquals("Next step when secondary databases exist", ScreenEnum.SecondaryDatabase, navigator.CurrentScreen_Exposed);
					}
					else
					{
						AssertEquals("Next step when secondary databases don't exist", ScreenEnum.PrimaryServer, navigator.CurrentScreen_Exposed);
					}
				}

				navigator.MoveBack();
				AssertEquals("Moved back to secondary server screen", ScreenEnum.SecondaryServer, navigator.CurrentScreen_Exposed);

				//Check server using sql server string name
				navigator.SetupInfo.SecondaryServer = null;
				navigator.TestSQLServerAgentStared = false;

				Assert("Check server, run SQl Agent", navigator.MoveNext(Db.Connection.ServerNameReportedByDatabase));
				AssertNotNull("SecondaryServer", navigator.SetupInfo.SecondaryServer);
				AssertEquals("Secondary server name", Db.Connection.ServerNameReportedByDatabase, navigator.SetupInfo.SecondaryServer.FullInstanceName);
				Assert("TestSQLServerAgent stared", navigator.TestSQLServerAgentStared);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStep2SecondaryDatabase()
		{
			var navigator = new ScreenNavigatorForTesting();
			navigator.CurrentScreen_Exposed = ScreenEnum.SecondaryDatabase;

			//When SetupAction.Setup, SetupInfo should be clear except secondary server field 
			FillSetupInfoWithTestData(navigator.SetupInfo);
			var oldSetupInfo = navigator.SetupInfo;
			var arguments = new object[] { SetupAction.Setup, null }; //First argument - setup action, second - Configured LS 
			Assert("MoveNext when SetupAction.Setup and Configured LS is null", navigator.MoveNext(arguments));
			AssertNotEquals("SetupInfo should be new", oldSetupInfo, navigator.SetupInfo);
			AssertEquals("Secondary server should be same", oldSetupInfo.SecondaryServer, navigator.SetupInfo.SecondaryServer);
			AssertAllSetupInfoExceptSecondaryServerHaveDefaultValue(navigator.SetupInfo);
			AssertEquals("Should go to PrimaryServer step when SetupAction.Setup", ScreenEnum.PrimaryServer, navigator.CurrentScreen_Exposed);

			navigator.MoveBack();
			AssertEquals("Previous step", ScreenEnum.SecondaryDatabase, navigator.CurrentScreen_Exposed);

			//When SetupAction.Setup, MoveNext ignores second argument 
			FillSetupInfoWithTestData(navigator.SetupInfo);
			oldSetupInfo = navigator.SetupInfo;
			arguments = new object[] { SetupAction.Setup, oldSetupInfo };
			Assert("MoveNext when SetupAction.Setup and Configured LS is not null", navigator.MoveNext(arguments));
			AssertNotEquals("SetupInfo should be new", oldSetupInfo, navigator.SetupInfo);
			AssertEquals("Secondary server should be same", oldSetupInfo.SecondaryServer, navigator.SetupInfo.SecondaryServer);
			AssertAllSetupInfoExceptSecondaryServerHaveDefaultValue(navigator.SetupInfo);
			AssertEquals("Should go to PrimaryServer step when SetupAction.Setup", ScreenEnum.PrimaryServer, navigator.CurrentScreen_Exposed);

			navigator.MoveBack();
			AssertEquals("Previous step", ScreenEnum.SecondaryDatabase, navigator.CurrentScreen_Exposed);

			//When SetupAction.Change, SetupInfo should be same as second argument which shouldn't be null
			var testSetupInfo = new LogShippingInfo();
			FillSetupInfoWithTestData(testSetupInfo);

			arguments = new object[] { SetupAction.Change, null };
			var expectedErrorMessage = "Error: Secondary database was not selected.";
			Assert("MoveNext when SetupAction.Change and Configured LS is null", !navigator.MoveNext(arguments));
			AssertEquals("Expected error when LS secondary database was not selected", expectedErrorMessage, navigator.Message);

			arguments = new object[] { SetupAction.Change, testSetupInfo };
			Assert("MoveNext when SetupAction.Change and Configured LS is not null", navigator.MoveNext(arguments));
			AssertEquals("SetupInfo should be same as passed", testSetupInfo, navigator.SetupInfo);
			AssertTestDataNotChanged(testSetupInfo, navigator.SetupInfo);
			AssertEquals("Should go to SourceDirectory step when SetupAction.Change", ScreenEnum.PrimaryEDocsDatabases, navigator.CurrentScreen_Exposed);

			navigator.MoveBack();
			AssertEquals("Previous step", ScreenEnum.SecondaryDatabase, navigator.CurrentScreen_Exposed);

			//When SetupAction.Remove, SetupInfo should be same as second argument which shouldn't be null
			arguments = new object[] { SetupAction.Remove, null };
			Assert("MoveNext when SetupAction.Remove and Configured LS is null", !navigator.MoveNext(arguments));
			AssertEquals("Expected error when LS secondary database was not selected", expectedErrorMessage, navigator.Message);

			arguments = new object[] { SetupAction.Remove, testSetupInfo };
			Assert("MoveNext when SetupAction.Remove and Configured LS is not null", navigator.MoveNext(arguments));
			AssertEquals("SetupInfo should be same as passed", testSetupInfo, navigator.SetupInfo);
			AssertTestDataNotChanged(testSetupInfo, navigator.SetupInfo);
			AssertEquals("Should go to FinalSetupScreen step when SetupAction.Remove", ScreenEnum.FinalSetupScreen, navigator.CurrentScreen_Exposed);

			navigator.MoveBack();
			AssertEquals("Previous step", ScreenEnum.SecondaryDatabase, navigator.CurrentScreen_Exposed);
		}

		public void TestCheckDirectoryFormat()
		{
			AssertEquals(true, ScreenNavigator.CheckDirectoryFormat(@"\\sydco-ssql-1a\shared"));
			AssertEquals(true, ScreenNavigator.CheckDirectoryFormat(@"\\sydco-ssql-1a.wtg.zone\shared"));

			AssertEquals(false, ScreenNavigator.CheckDirectoryFormat(@"sydco-ssql-1a.wtg.zone\shared"));
			AssertEquals(false, ScreenNavigator.CheckDirectoryFormat(@"\sydco-ssql-1a.wtg.zone/shared"));
			AssertEquals(false, ScreenNavigator.CheckDirectoryFormat(@"\\sydco-ssql-1a.wtg.zone"));
			AssertEquals(false, ScreenNavigator.CheckDirectoryFormat(@"\\sydco-ss ql-1a.wtg.zone\shared"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStep5SourceDirectory()
		{
			using (var dbTestHelper = new DbTestHelper())
			{
				var sqlVersion = Db.Connection.ServerVersionNumber;
				var testBackupFileName = DbTestHelper.GetTestBackupFileName(sqlVersion);
				var testBackupFilePath = Path.Combine(Path.Combine(TestCase.BaseSourcePath, DbTestHelper.TestBackupFilePath), testBackupFileName);
				var bkpHeaderSql = string.Format("RESTORE HEADERONLY FROM DISK = '{0}'", testBackupFilePath);
				string backupFullServerName = null;
				string backupDbName = null;

				using (var adminConnection = Db.NewAdminConnection())
				{
					using (var reader = adminConnection.Command(bkpHeaderSql).ExecuteReader())
					{
						reader.Read();
						backupFullServerName = reader["ServerName"].ToString();
						backupDbName = reader["DatabaseName"].ToString();
					}

					var backupServerNameParts = backupFullServerName.Split('\\');
					var backupServerMachineName = backupServerNameParts[0];
					var backupServerInstanceName = (backupServerNameParts.Length > 1) ? backupServerNameParts[1] : null;

					using (var tempDir = new TempDirectory())
					{
						var navigator = new ScreenNavigatorForTesting();
						navigator.CurrentScreen_Exposed = ScreenEnum.SourceDirectory;
						navigator.SetupInfo.PrimaryServer = new SqlServerInfo(backupServerMachineName, backupServerInstanceName, Db.Connection.ServerVersionNumber.ToString());
						navigator.SetupInfo.MainDatabase = new MainDatabaseInfo(navigator.SetupInfo, Db.SqlMasterDb);
						navigator.SetupInfo.SecondaryServer = new SqlServerInfoForTesting(dbTestHelper.TestConnection.ServerNameWithoutInstance, dbTestHelper.TestConnection.ServerInstanceName, dbTestHelper.TestConnection.ServerVersionNumber.ToString());

						var expectedErrorMessage = "Error: Entered directory is not in UNC.";
						Assert(!navigator.MoveNext(""));
						AssertEquals("Directory is not in UNC.", expectedErrorMessage, navigator.Message);
						Assert(!navigator.MoveNext(tempDir.DirectoryName));
						AssertEquals("Directory is not in UNC.", expectedErrorMessage, navigator.Message);

						expectedErrorMessage = "Error: Entered directory does not exist or access is denied.";
						Assert(!navigator.MoveNext(@"\\localhost\invalid_share_location"));
						AssertEquals("Directory doesn't exist", expectedErrorMessage, navigator.Message);

						var sharedDirInUNC = ShareDirectory(tempDir.DirectoryName);
						AssertNotNull(sharedDirInUNC);

						Assert("Shared directory is in UNC but has not backups", !navigator.MoveNext(sharedDirInUNC));
						expectedErrorMessage = "There are no backup files in the entered directory.";
						Assert(string.Format("Error message should start with ['{0}'], but was ['{1}'].", expectedErrorMessage, navigator.Message),
							navigator.Message.StartsWith(expectedErrorMessage) || navigator.Message.StartsWith("Error: " + expectedErrorMessage));
						AssertNull("List of backups", navigator.ListOfBackups);

						File.Copy(testBackupFilePath, Path.Combine(sharedDirInUNC, testBackupFileName));
						AssertEquals("Shared test backup file created", true, File.Exists(Path.Combine(sharedDirInUNC, testBackupFileName)));
						CheckSharedPathHasFiles(sharedDirInUNC, adminConnection);

						Assert("Shared directory is in UNC but shlouldn't have backups for the master database", !navigator.MoveNext(sharedDirInUNC));
						Assert(string.Format("Expected No-Backup-Message should start with '{0}', but was '{1}' message", expectedErrorMessage, navigator.Message),
							navigator.Message.StartsWith(expectedErrorMessage) || navigator.Message.StartsWith("Error: " + expectedErrorMessage));
						AssertNull("List of backups", navigator.ListOfBackups);

						AssertEquals("Current screen was not changed", ScreenEnum.SourceDirectory, navigator.CurrentScreen_Exposed);

						navigator.SetupInfo.MainDatabase = new MainDatabaseInfo(navigator.SetupInfo, backupDbName);
						Assert("Shared directory is in UNC and should have backups for the test database", navigator.MoveNext(sharedDirInUNC));
						AssertEquals("BackupSourceDirectory", sharedDirInUNC, navigator.SetupInfo.BackupSourceDirectory);
						AssertEquals("ConfigurationChanged", true, navigator.SetupInfo.ConfigurationChanged);
						AssertEquals("Count of backups", 1, navigator.ListOfBackups.Count);
						AssertEquals("Backup file name", testBackupFileName, navigator.ListOfBackups[navigator.SetupInfo.MainDatabase.DatabaseName][0]);

						navigator.MoveBack();
						AssertEquals("Previous step", ScreenEnum.SourceDirectory, navigator.CurrentScreen_Exposed);

						navigator.SetupInfo.OriginalBackupLocalCopyDirectory = navigator.SetupInfo.BackupLocalCopyDirectory = BaseSourcePath;
						navigator.SetupInfo.OriginalBackupSourceDirectory = navigator.SetupInfo.BackupSourceDirectory;
						Assert("Check unchanged directory", navigator.MoveNext(navigator.SetupInfo.BackupSourceDirectory));
						AssertEquals("BackupSourceDirectory", sharedDirInUNC, navigator.SetupInfo.BackupSourceDirectory);
						AssertEquals("ConfigurationChanged", false, navigator.SetupInfo.ConfigurationChanged);

						AssertEquals("Next step", ScreenEnum.LocalCopyDirectory, navigator.CurrentScreen_Exposed);
					}
				}
			}
			DbCommitTracker.Ignore("@DirInfo");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStep6LocalCopyDirectory()
		{
			var navigator = new ScreenNavigatorForTesting();
			navigator.CurrentScreen_Exposed = ScreenEnum.LocalCopyDirectory;
			navigator.SetupInfo.MainDatabase = new MainDatabaseInfo(navigator.SetupInfo, "PrimaryDb") { ShouldInitialise = false };

			var expectedErrorMessage = "Error: Entered directory does not exist or access is denied.";
			Assert(!navigator.MoveNext(""));
			AssertEquals("Directory does not exist", expectedErrorMessage, navigator.Message);
			Assert(!navigator.MoveNext(BaseSourcePath + Guid.NewGuid()));
			AssertEquals("Directory does not exist", expectedErrorMessage, navigator.Message);
			Assert(!navigator.MoveNext(@")(@#@*%)@(>/\"));
			AssertEquals("Directory does not exist", expectedErrorMessage, navigator.Message);

			using (var tempDir = new TempDirectory())
			{
				var sharedDirInUNC = ShareDirectory(tempDir.DirectoryName);
				if (sharedDirInUNC != null)
				{
					expectedErrorMessage = "Error: Entered directory is not local.";
					Assert(!navigator.MoveNext(sharedDirInUNC));
					AssertEquals("Directory is not local", expectedErrorMessage, navigator.Message);
				}
				else
				{
					Assert("This test can only run if win security allows share directory", true);
				}

				AssertNull("Backup local copy directory", navigator.SetupInfo.BackupLocalCopyDirectory);
				AssertEquals("Current screen was not changed", ScreenEnum.LocalCopyDirectory, navigator.CurrentScreen_Exposed);

				Assert("Check local copy directory - " + tempDir.DirectoryName, navigator.MoveNext(tempDir.DirectoryName));
				AssertEquals("Backup local copy directory", tempDir.DirectoryName, navigator.SetupInfo.BackupLocalCopyDirectory);
				AssertEquals("ConfigurationChanged", true, navigator.SetupInfo.ConfigurationChanged);
				AssertEquals("Next step", ScreenEnum.InitializeSecondaryDb, navigator.CurrentScreen_Exposed);

				navigator.MoveBack();
				AssertEquals("Previous step", ScreenEnum.LocalCopyDirectory, navigator.CurrentScreen_Exposed);

				navigator.SetupInfo.OriginalBackupSourceDirectory = navigator.SetupInfo.BackupSourceDirectory = BaseSourcePath;
				navigator.SetupInfo.OriginalBackupLocalCopyDirectory = navigator.SetupInfo.BackupLocalCopyDirectory;
				Assert("Check unchanged directory", navigator.MoveNext(navigator.SetupInfo.BackupLocalCopyDirectory));
				AssertEquals("Backup local copy directory", tempDir.DirectoryName, navigator.SetupInfo.BackupLocalCopyDirectory);
				AssertEquals("ConfigurationChanged", false, navigator.SetupInfo.ConfigurationChanged);
				navigator.SetupInfo.MainDatabase.ShouldInitialise = true;
				AssertEquals("ConfigurationChanged", true, navigator.SetupInfo.ConfigurationChanged);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStep7InitialiseSecondaryDb()
		{
			var navigator = new ScreenNavigatorForTesting { TestCheckSecondaryDatabaseExists = false };
			navigator.CurrentScreen_Exposed = ScreenEnum.InitializeSecondaryDb;
			navigator.SetupInfo.BackupSourceDirectory = BaseSourcePath;
			var databaseInfo = new MainDatabaseInfoForTesting(navigator.SetupInfo, "TestDatabase");
			databaseInfo.ShouldInitialise = true;
			databaseInfo.SecondaryDatabaseExistValue = false;
			navigator.SetupInfo.MainDatabase = databaseInfo;
			navigator.SetupInfo.SecondaryServer = new SqlServerInfo(Db.Connection.ServerName, "", Db.Connection.ServerVersionNumber.ToString());

			Assert("Backup must be selected if secondary database is not initialized", !navigator.MoveNext(DatabaseInitialisationOptions.Reinitialise));
			AssertEquals("Backup was not selected", "Error: Backup was not selected.", navigator.Message);

			navigator.SetupInfo.MainDatabase.BackupFileName = DbTestHelper.GetTestBackupFileName(new SqlServerVersionNumber("13.00.0000.00"));
			Assert("Set backup file name", navigator.MoveNext(DatabaseInitialisationOptions.None));
			AssertEquals("Backup file name", Path.Combine(BaseSourcePath, DbTestHelper.GetTestBackupFileName(new SqlServerVersionNumber("13.00.0000.00"))), navigator.SetupInfo.MainDatabase.BackupFullFileName);
			AssertEquals("ShouldInitializeSecondaryDb", true, navigator.SetupInfo.ShouldInitializeSecondaryDb);
			AssertEquals("InitializeSeconaryDatabase should be called", true, navigator.IsDbInitialized);

			navigator.MoveBack();
			AssertEquals("Previous step", ScreenEnum.InitializeSecondaryDb, navigator.CurrentScreen_Exposed);

			navigator = new ScreenNavigatorForTesting { TestCheckSecondaryDatabaseExists = false };
			navigator.CurrentScreen_Exposed = ScreenEnum.InitializeSecondaryDb;
			navigator.SetupInfo.BackupSourceDirectory = BaseSourcePath;
			databaseInfo = new MainDatabaseInfoForTesting(navigator.SetupInfo, "TestDatabase");
			databaseInfo.SecondaryDatabaseExistValue = true;
			databaseInfo.ShouldInitialise = false;
			navigator.SetupInfo.MainDatabase = databaseInfo;

			Assert("Backup is not necessary if secondary database is already initialized", navigator.MoveNext(DatabaseInitialisationOptions.None));
			AssertEquals("ShouldInitializeSecondaryDb", false, navigator.SetupInfo.ShouldInitializeSecondaryDb);
			AssertEquals("InitializeSeconaryDatabase shouldn't be called", false, navigator.IsDbInitialized);
			AssertEquals(string.Empty, navigator.SetupInfo.MainDatabase.BackupFileName);

			navigator.MoveBack();
			AssertEquals("Previous step", ScreenEnum.InitializeSecondaryDb, navigator.CurrentScreen_Exposed);

			//When secondary database is initialized, it can be re-initialized with new backup
			//navigator.SetupInfo.SecondaryDatabaseExists = true;
			navigator.SetupInfo.MainDatabase.BackupFileName = DbTestHelper.GetTestBackupFileName(new SqlServerVersionNumber("13.00.0000.00"));
			Assert("Set backup file name", navigator.MoveNext(DatabaseInitialisationOptions.Reinitialise));
			AssertEquals("Backup file name", Path.Combine(BaseSourcePath, DbTestHelper.GetTestBackupFileName(new SqlServerVersionNumber("13.00.0000.00"))), navigator.SetupInfo.MainDatabase.BackupFullFileName);
			AssertEquals("ShouldInitializeSecondaryDb", true, navigator.SetupInfo.ShouldInitializeSecondaryDb);
			AssertEquals("InitializeSeconaryDatabase should be called", true, navigator.IsDbInitialized);

			AssertEquals("Next step", ScreenEnum.FinalSetupScreen, navigator.CurrentScreen_Exposed);
		}

		void CheckSharedPathHasFiles(string sharedPath, AdminConnection adminConnection)
		{
			var sqlText = string.Format(@"
				DECLARE @DirInfo TABLE (FileOrDirName varchar(max), Depth int, IsFile bit);
				INSERT @DirInfo EXEC sys.xp_dirtree '{0}', 1, 1;
				SELECT count(*) FROM @DirInfo;",
				sharedPath);

			var fileCount = (int)adminConnection.ExecuteScalar(sqlText);

			if (fileCount == 0)
			{
				sqlText = @"
					DECLARE @ServiceAccName varchar(250);
					EXEC sys.xp_instance_regread
						N'HKEY_LOCAL_MACHINE', N'SYSTEM\CurrentControlSet\Services\MSSQLSERVER',
						N'ObjectName', @ServiceAccName OUTPUT, N'no_output';
					SELECT @ServiceAccName;";
				var sqlServiceAccountName = adminConnection.ExecuteScalar(sqlText).ToString();

				Fail(string.Format(@"
					Shared files are not visible to SQL Server.
					Please ensure the SQL Server service is running under LocalSystem or a valid domain account with local PC admin rights.
					Current SQL service log on as account is [{0}]
					",
					sqlServiceAccountName));
			}
		}

		void AssertAllSetupInfoExceptSecondaryServerHaveDefaultValue(LogShippingInfo info)
		{
			AssertNull("PrimaryServer default value", info.PrimaryServer);
			AssertNull("PrimaryDatabase default value", info.MainDatabase);
			AssertNull("BackupSourceDirectory default value", info.BackupSourceDirectory);
			AssertNull("BackupLocalCopyDirectory default value", info.BackupLocalCopyDirectory);
			AssertEquals("SetupAction default value", SetupAction.Setup, info.SetupAction);
			AssertEquals("ShouldInitializeSecondaryDb default value", false, info.ShouldInitializeSecondaryDb);
		}

		void AssertTestDataNotChanged(LogShippingInfo expectedSetupInfo, LogShippingInfo info)
		{
			AssertEquals("PrimaryServer test value", expectedSetupInfo.PrimaryServer, info.PrimaryServer);
			AssertEquals("PrimaryDatabase test value", expectedSetupInfo.MainDatabase.DatabaseName, info.MainDatabase.DatabaseName);
			AssertEquals("SecondaryServer test value", expectedSetupInfo.SecondaryServer, info.SecondaryServer);
			AssertEquals("SecondaryDatabase test value", expectedSetupInfo.MainDatabase.SecondaryDatabaseName, info.MainDatabase.SecondaryDatabaseName);
			AssertEquals("BackupSourceDirectory test value", expectedSetupInfo.BackupSourceDirectory, info.BackupSourceDirectory);
			AssertEquals("BackupLocalCopyDirectory test value", expectedSetupInfo.BackupLocalCopyDirectory, info.BackupLocalCopyDirectory);
			AssertEquals("ConfigurationChanged test value", expectedSetupInfo.ConfigurationChanged, info.ConfigurationChanged);
			AssertEquals("SetupAction test value", expectedSetupInfo.SetupAction, info.SetupAction);
			AssertEquals("ShouldInitializeSecondaryDb test value", expectedSetupInfo.ShouldInitializeSecondaryDb, info.ShouldInitializeSecondaryDb);
			AssertEquals("CopyJobUid test value", expectedSetupInfo.MainDatabase.CopyJobId, info.MainDatabase.CopyJobId);
			AssertEquals("RestoreJobUid test value", expectedSetupInfo.MainDatabase.RestoreJobId, info.MainDatabase.RestoreJobId);
		}

		void FillSetupInfoWithTestData(LogShippingInfo info)
		{
			info.PrimaryServer = new SqlServerInfoForTesting("Test", string.Empty, "9.0.0.0");
			var databaseInfo = new MainDatabaseInfoForTesting(info, "TestDatabase");
			databaseInfo.SecondaryDatabaseExistValue = true;
			info.MainDatabase = databaseInfo;
			info.SecondaryServer = info.PrimaryServer;
			info.BackupSourceDirectory = BaseSourcePath;
			databaseInfo.BackupFileName = "Backup.bak";
			info.BackupLocalCopyDirectory = BaseSourcePath;

			info.SetupAction = SetupAction.Change;
			info.ShouldInitializeSecondaryDb = false;
			info.MainDatabase.CopyJobId = Guid.NewGuid();
			info.MainDatabase.RestoreJobId = Guid.NewGuid();
		}

		int GetCountOfLSStandByDbs()
		{
			return (int)Db.Connection.ExecuteScalar("SELECT Count(*) FROM msdb..log_shipping_secondary_databases");
		}

		string ShareDirectory(string directoryName)
		{
			string result = null;
			DirectoryInfo info = new DirectoryInfo(directoryName);
			DirectorySecurity security = info.GetAccessControl(AccessControlSections.Owner);
			security.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow));
			info.SetAccessControl(security);
			string shareName = @"\\localhost\" + info.FullName.Replace(":", "$");

			if (Directory.Exists(shareName))
			{
				result = shareName;
			}
			return result;
		}
	}

	sealed class ScreenNavigatorForTesting : ScreenNavigator
	{
		public ScreenNavigatorForTesting()
		{
			OnFailure += new ActionResultDelegate(ScreenNavigatorForTesting_OnFailure);
			OnSuccess += new ActionResultDelegate(ScreenNavigatorForTesting_OnSuccess);
			OnShowMessage += new NotificationDelegate(ScreenNavigatorForTesting_OnShowMessage);
			SetupInfo = new LogShippingInfo();
			TestCheckSecondaryDatabaseExists = true;
		}

		public LogShippingInfo[] ExistingSecondaryDatabases_Exposed
		{
			get { return ExistingLogShippingConfigurations; }
			set { ExistingLogShippingConfigurations = value; }
		}

		public ScreenEnum CurrentScreen_Exposed
		{
			get { return CurrentScreen; }
			set { CurrentScreen = value; }
		}

		public new bool MoveNext(object value)
		{
			base.MoveNext(value);
			return moveNextResult;
		}

		void ScreenNavigatorForTesting_OnSuccess()
		{
			moveNextResult = true;
		}

		void ScreenNavigatorForTesting_OnFailure()
		{
			moveNextResult = false;
		}

		void ScreenNavigatorForTesting_OnShowMessage(string message)
		{
			Message = message;
		}

		protected override bool StartSQLServerAgent()
		{
			return TestSQLServerAgentStared = true;
		}

		protected override bool InitializeSeconaryDatabase(bool onlyGenerateScript)
		{
			return IsDbInitialized = true;
		}

		protected override bool CheckPrimaryAndSecondaryServersDiffer(SqlServerInfo server)
		{
			return !TestPrimaryAndSecondaryServersDiffer || base.CheckPrimaryAndSecondaryServersDiffer(server);
		}

		protected override bool CheckSecondaryDatabasesExist()
		{
			return !TestCheckSecondaryDatabaseExists || base.CheckSecondaryDatabasesExist();
		}

		public bool IsDbInitialized { get; private set; }
		public bool TestSQLServerAgentStared { get; set; }
		public bool TestPrimaryAndSecondaryServersDiffer { get; set; }
		public bool TestCheckSecondaryDatabaseExists { get; set; }
		public string Message { get; set; }
		bool moveNextResult;
	}
}
