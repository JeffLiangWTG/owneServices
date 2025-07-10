using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.IO;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.ChangeDataCapture.Common.Testing;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class UpgradePreparationTest : TestCase
	{
		public void TestAlterDbAuthorisationDeclinedLock()
		{
			// Arrange
			const string testDatabaseName = nameof(testDatabaseName);
			using (var blockingConnection = Db.NewAdminConnection())
			using (AdoTestUtils.CreateDbDropExistingDisposable(blockingConnection, testDatabaseName))
			{
				Assert("Precondition: Acquire Lock Initially", blockingConnection.TryGetLock("AlterDbAuthorisation_App_Lock", out var appLock, testDatabaseName));
				using (appLock)
				using (var newConnection = Db.NewAdminConnection())
				{
					// Act & Assert
					AssertExceptionThrown<UpgradeBlockedException>(() => UpgradePreparation.AlterDbAuthorisation(newConnection, testDatabaseName));
				}
			}
		}

		[UseSnapshotProtection]
		public void TestPerformNonTransactionalDatabaseSettings()
		{
			const string testDbName = "UpgradePreparationTest$Database";

			try
			{
				using (var tempoDirectory = new TempDirectory())
				using (var connection = Db.NewAdminConnection())
				{
					// Drop test DB first to ensure a clean new one is created
					AdoTestUtils.DropDbIfExists(connection, testDbName);

					AdoTestUtils.CreateDbIfNotExists(connection, testDbName);

					// Reset properties
					SetDbProperty(connection, testDbName, "TRUSTWORTHY OFF");
					SetDbProperty(connection, testDbName, "PAGE_VERIFY NONE");
					SetDbProperty(connection, testDbName, "AUTO_CLOSE ON");
					SetDbProperty(connection, testDbName, "COMPATIBILITY_LEVEL = 100");
					SetDbProperty(connection, testDbName, "RECOVERY FULL");
					SetDbProperty(connection, testDbName, "ALLOW_SNAPSHOT_ISOLATION OFF");
					SetDbProperty(connection, testDbName, "READ_COMMITTED_SNAPSHOT OFF");
					SetDbProperty(connection, testDbName, "DISABLE_BROKER");
					SetDbProperty(connection, testDbName, "ARITHABORT OFF");

					// Assert as pre-condition
					AssertDbProperties(connection, testDbName,
						expectedTrustworthy: false,
						expectedPageVerify: 0,
						expectedAutoClose: true,
						expectedCompatibilityLevel: 100,
						expectedSnapshotIsolationState: 0,
						expectedReadCommittedSnapshot: false,
						expectedBroker: false,
						expectedArithAbort: false);
					// Assert database files (pre-condition)
					AssertDataFileCount(connection, testDbName, 1);

					using (var testDbConnection = Db.NewAdminConnection(Db.ServerName, testDbName))
					{
						var upgPreparation = new UpgradePreparationForTest(testDbConnection, shouldEnableCdc_Override: true);

						var loggerMock = new Mock<IUpgradeTaskWorkflowLogger>();
						dbRecoveryModelManagerMock
							.Setup(s => s.AdjustAllDatabases(It.IsAny<AdminConnection>(), It.IsAny<string>()))
							.Returns(new[] { "message1", "message2" })
							.Verifiable();

						upgPreparation.PerformNonTransactionalDatabaseSettings_Exposed(loggerMock.Object, new string[] { testDbName });

						// Assert properties set as required
						AssertDbProperties(connection, testDbName,
							expectedTrustworthy: true,
							expectedPageVerify: 2,
							expectedAutoClose: false,
							expectedCompatibilityLevel: connection.ServerVersionNumber.CompatibilityLevel,
							expectedSnapshotIsolationState: 1,
							expectedReadCommittedSnapshot: true,
							expectedBroker: false,
							expectedArithAbort: true);

						// Assert CDC enabled
						AssertEquals("Is CDC enabled?", true, CdcDatabase.IsEnabled(connection, testDbName));

						// Assert DB properties still set as required
						AssertDbProperties(connection, testDbName,
							expectedTrustworthy: true,
							expectedPageVerify: 2,
							expectedAutoClose: false,
							expectedCompatibilityLevel: connection.ServerVersionNumber.CompatibilityLevel,
							expectedSnapshotIsolationState: 1,
							expectedReadCommittedSnapshot: true,
							expectedBroker: false,
							expectedArithAbort: true);

						// Assert database files (CDC filegroup and file created)
						AssertDataFileCount(connection, testDbName, 7);
						AssertDatabaseFileGroup(testDbConnection, testDbName, "SECONDARY", "Data02");
						AssertDatabaseFileGroup(testDbConnection, testDbName, CdcDatabase.FileGroup, "DataCdc");
						AssertDatabaseFileGroup(testDbConnection, testDbName, "REPORTGROUP", "DataReport");

						loggerMock.Verify(l => l.ShowInfoMessage("2 database(s) needed recovery model adjustment"));
						loggerMock.Verify(l => l.ShowInfoMessage("\tmessage1"));
						loggerMock.Verify(l => l.ShowInfoMessage("\tmessage2"));
						dbRecoveryModelManagerMock.Verify();
					}
				}
			}
			finally
			{
				using (var newConnection = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(newConnection, testDbName);
				}
			}
		}

		public void TestFileGroupIsCreatedForReportData()
		{
			const string testDbName = "UpgradePreparationTest$Database";

			using (var tempoDirectory = new TempDirectory())
			using (var connection = Db.NewAdminConnection())
			{
				// Drop test DB first to ensure a clean new one is created
				AdoTestUtils.DropDbIfExists(connection, testDbName);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(connection, testDbName);

					// Reset properties
					SetDbProperty(connection, testDbName, "TRUSTWORTHY OFF");
					SetDbProperty(connection, testDbName, "PAGE_VERIFY NONE");
					SetDbProperty(connection, testDbName, "AUTO_CLOSE ON");
					SetDbProperty(connection, testDbName, "COMPATIBILITY_LEVEL = 100");
					SetDbProperty(connection, testDbName, "RECOVERY SIMPLE");
					SetDbProperty(connection, testDbName, "ALLOW_SNAPSHOT_ISOLATION OFF");
					SetDbProperty(connection, testDbName, "READ_COMMITTED_SNAPSHOT OFF");
					SetDbProperty(connection, testDbName, "DISABLE_BROKER");
					SetDbProperty(connection, testDbName, "ARITHABORT OFF");

					// Assert as pre-condition
					AssertDbProperties(connection, testDbName,
						expectedTrustworthy: false,
						expectedPageVerify: 0,
						expectedAutoClose: true,
						expectedCompatibilityLevel: 100,
						expectedSnapshotIsolationState: 0,
						expectedReadCommittedSnapshot: false,
						expectedBroker: false,
						expectedArithAbort: false);
					// Assert database files (pre-condition)
					AssertDataFileCount(connection, testDbName, 1);

					using (var testDbConnection = Db.NewAdminConnection(Db.ServerName, testDbName))
					{
						var upgPreparation = new UpgradePreparationForTest(testDbConnection);
						upgPreparation.BiDisableChangeDataCapture_OverrideValueForTest = false;
						upgPreparation.PerformNonTransactionalDatabaseSettings_Exposed(new DummyLoggerForTest(), new string[] { testDbName });

						// Assert properties set as required
						AssertDbProperties(connection, testDbName,
							expectedTrustworthy: true,
							expectedPageVerify: 2,
							expectedAutoClose: false,
							expectedCompatibilityLevel: connection.ServerVersionNumber.CompatibilityLevel,
							expectedSnapshotIsolationState: 1,
							expectedReadCommittedSnapshot: true,
							expectedBroker: false,
							expectedArithAbort: true);

						// Assert database files (secondary filegroup and file created)
						AssertDataFileCount(connection, testDbName, 6);
						AssertDatabaseFileGroup(testDbConnection, testDbName, "SECONDARY", "Data02");
						AssertDatabaseFileGroup(testDbConnection, testDbName, "REPORTGROUP", "DataReport");
						var expectedFileList = new List<DatabaseFileGroupCreator.FileInfo>()
						{
							new DatabaseFileGroupCreator.FileInfo() { DBName = testDbName, FileGroupName = "REPORTGROUP", LogicalFileName = $"{testDbName}_DataReport", FileName = $"{testDbName}_DataReport", IsPercentGrowth = false, Growth = 0.0020M, GrowthUnit = "GB" },
							new DatabaseFileGroupCreator.FileInfo() { DBName = testDbName, FileGroupName = "REPORTGROUP", LogicalFileName = $"{testDbName}_DataReport1", FileName = $"{testDbName}_DataReport1", IsPercentGrowth = false, Growth = 0.0020M, GrowthUnit = "GB" },
							new DatabaseFileGroupCreator.FileInfo() { DBName = testDbName, FileGroupName = "REPORTGROUP", LogicalFileName = $"{testDbName}_DataReport2", FileName = $"{testDbName}_DataReport2", IsPercentGrowth = false, Growth = 0.0020M, GrowthUnit = "GB" },
							new DatabaseFileGroupCreator.FileInfo() { DBName = testDbName, FileGroupName = "REPORTGROUP", LogicalFileName = $"{testDbName}_DataReport3", FileName = $"{testDbName}_DataReport3", IsPercentGrowth = false, Growth = 0.0020M, GrowthUnit = "GB" }
						};
						AssertDataFileProperties(testDbConnection, testDbName, "REPORTGROUP", expectedFileList);
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}
		}

		public void TestFileGrowthIsAboveThenMinSizeForReportData()
		{
			const string testDbName = "UpgradePreparationTest$Database";

			using (var tempoDirectory = new TempDirectory())
			using (var connection = Db.NewAdminConnection())
			{
				// Drop test DB first to ensure a clean new one is created
				AdoTestUtils.DropDbIfExists(connection, testDbName);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(connection, testDbName);

					// Reset properties
					SetDbProperty(connection, testDbName, "TRUSTWORTHY OFF");
					SetDbProperty(connection, testDbName, "PAGE_VERIFY NONE");
					SetDbProperty(connection, testDbName, "AUTO_CLOSE ON");
					SetDbProperty(connection, testDbName, "COMPATIBILITY_LEVEL = 100");
					SetDbProperty(connection, testDbName, "RECOVERY SIMPLE");
					SetDbProperty(connection, testDbName, "ALLOW_SNAPSHOT_ISOLATION OFF");
					SetDbProperty(connection, testDbName, "READ_COMMITTED_SNAPSHOT OFF");
					SetDbProperty(connection, testDbName, "DISABLE_BROKER");
					SetDbProperty(connection, testDbName, "ARITHABORT OFF");

					using (var testDbConnection = Db.NewAdminConnection(Db.ServerName, testDbName))
					{
						var upgPreparation = new UpgradePreparationForTest(testDbConnection);
						upgPreparation.BiDisableChangeDataCapture_OverrideValueForTest = false;
						upgPreparation.SetFileGrowthSize = false;
						upgPreparation.PerformNonTransactionalDatabaseSettings_Exposed(new DummyLoggerForTest(), new string[] { testDbName });

						// Assert database files (secondary filegroup and file created)
						AssertDataFileCount(connection, testDbName, 6);
						AssertDatabaseFileGroup(testDbConnection, testDbName, "SECONDARY", "Data02");
						AssertDatabaseFileGroup(testDbConnection, testDbName, "REPORTGROUP", "DataReport");

						var manager = new DatabaseFileGroupCreator(testDbConnection, testDbName);
						manager.ChangeFileGrowthSize("REPORTGROUP", 1, "MB");

						var expectedFileList = new List<DatabaseFileGroupCreator.FileInfo>()
						{
							new DatabaseFileGroupCreator.FileInfo() { DBName = testDbName, FileGroupName = "REPORTGROUP", LogicalFileName = $"{testDbName}_DataReport", FileName = $"{testDbName}_DataReport", IsPercentGrowth = false, Growth = 0.0010M, GrowthUnit = "GB" },
							new DatabaseFileGroupCreator.FileInfo() { DBName = testDbName, FileGroupName = "REPORTGROUP", LogicalFileName = $"{testDbName}_DataReport1", FileName = $"{testDbName}_DataReport1", IsPercentGrowth = false, Growth = 0.0010M, GrowthUnit = "GB" },
							new DatabaseFileGroupCreator.FileInfo() { DBName = testDbName, FileGroupName = "REPORTGROUP", LogicalFileName = $"{testDbName}_DataReport2", FileName = $"{testDbName}_DataReport2", IsPercentGrowth = false, Growth = 0.0010M, GrowthUnit = "GB" },
							new DatabaseFileGroupCreator.FileInfo() { DBName = testDbName, FileGroupName = "REPORTGROUP", LogicalFileName = $"{testDbName}_DataReport3", FileName = $"{testDbName}_DataReport3", IsPercentGrowth = false, Growth = 0.0010M, GrowthUnit = "GB" }
						};
						AssertDataFileProperties(testDbConnection, testDbName, "REPORTGROUP", expectedFileList);

						//Run the Process again
						upgPreparation = new UpgradePreparationForTest(testDbConnection);
						upgPreparation.BiDisableChangeDataCapture_OverrideValueForTest = false;
						upgPreparation.PerformNonTransactionalDatabaseSettings_Exposed(new DummyLoggerForTest(), new string[] { testDbName });
						expectedFileList.ForEach(x => x.Growth = 0.002M);
						AssertDataFileProperties(testDbConnection, testDbName, "REPORTGROUP", expectedFileList);

						//Change FileGrowth size manually to 4 MB
						manager = new DatabaseFileGroupCreator(testDbConnection, testDbName);
						manager.ChangeFileGrowthSize("REPORTGROUP", 4, "MB");
						expectedFileList.ForEach(x => x.Growth = 0.0039M);
						AssertDataFileProperties(testDbConnection, testDbName, "REPORTGROUP", expectedFileList);

						//Run the Process again to see whether FileGrowth size changes.
						upgPreparation = new UpgradePreparationForTest(testDbConnection);
						upgPreparation.BiDisableChangeDataCapture_OverrideValueForTest = false;
						upgPreparation.PerformNonTransactionalDatabaseSettings_Exposed(new DummyLoggerForTest(), new string[] { testDbName });
						expectedFileList.ForEach(x => x.Growth = 0.0039M);
						AssertDataFileProperties(testDbConnection, testDbName, "REPORTGROUP", expectedFileList);
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}
		}

		public void TestFileGrowthIsAboveThenMinSizeForCdcData()
		{
			const string testDbName = "UpgradePreparationTest$Database";

			using (var tempoDirectory = new TempDirectory())
			using (var connection = Db.NewAdminConnection())
			{
				// Drop test DB first to ensure a clean new one is created
				AdoTestUtils.DropDbIfExists(connection, testDbName);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(connection, testDbName);

					using (var testDbConnection = Db.NewAdminConnection(Db.ServerName, testDbName))
					{
						var sqlCreateStmData = @"
CREATE TABLE [StmData] (
[SD_PK] UNIQUEIDENTIFIER NOT NULL,
[SD_Name] VARCHAR(300) NOT NULL DEFAULT '',
[SD_Owner] UNIQUEIDENTIFIER NULL,
[SD_DepartmentGuid] UNIQUEIDENTIFIER NULL,
[SD_Type] CHAR(3) NOT NULL DEFAULT '',
[SD_IsLogged] BIT NOT NULL DEFAULT 0,
[SD_BinaryValue] VARBINARY(MAX) NULL,
[SD_GuidValue] UNIQUEIDENTIFIER NULL,
[SD_IsCancelled] BIT NOT NULL DEFAULT 0,
[SD_PreserveTestValue] BIT NOT NULL DEFAULT 0,
[SD_SystemCreateTimeUtc] SMALLDATETIME NULL,
[SD_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
[SD_SystemLastEditTimeUtc] SMALLDATETIME NULL,
[SD_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);";
						testDbConnection.ExecuteNonQuery(sqlCreateStmData);

						var upgPreparation = new UpgradePreparationForTest(testDbConnection, shouldEnableCdc_Override: true);
						upgPreparation.SetFileGrowthSize = false;
						upgPreparation.PerformNonTransactionalDatabaseSettings_Exposed(new DummyLoggerForTest(), new string[] { testDbName });

						var cdcFileGroupName = CdcDatabase.FileGroup;

						AssertDatabaseFileGroup(testDbConnection, testDbName, cdcFileGroupName, "DataCdc");

						var manager = new DatabaseFileGroupCreator(testDbConnection, testDbName);
						manager.ChangeFileGrowthSize(cdcFileGroupName, 1, "MB");

						var expectedFileList = new List<DatabaseFileGroupCreator.FileInfo>()
						{
							new DatabaseFileGroupCreator.FileInfo() { DBName = testDbName, FileGroupName = cdcFileGroupName, LogicalFileName = $"{testDbName}_DataCdc", FileName = $"{testDbName}_DataCdc", IsPercentGrowth = false, Growth = 0.0010M, GrowthUnit = "GB" }
						};
						AssertDataFileProperties(testDbConnection, testDbName, cdcFileGroupName, expectedFileList);

						//Run the Process again
						upgPreparation = new UpgradePreparationForTest(testDbConnection, shouldEnableCdc_Override: true);
						upgPreparation.PerformNonTransactionalDatabaseSettings_Exposed(new DummyLoggerForTest(), new string[] { testDbName });
						expectedFileList.ForEach(x => x.Growth = 0.002M);
						AssertDataFileProperties(testDbConnection, testDbName, cdcFileGroupName, expectedFileList);

						//Change FileGrowth size manually to 4 MB
						manager = new DatabaseFileGroupCreator(testDbConnection, testDbName);
						manager.ChangeFileGrowthSize(cdcFileGroupName, 4, "MB");
						expectedFileList.ForEach(x => x.Growth = 0.0039M);
						AssertDataFileProperties(testDbConnection, testDbName, cdcFileGroupName, expectedFileList);

						//Run the Process again to see whether FileGrowth size changes.
						upgPreparation = new UpgradePreparationForTest(testDbConnection);
						upgPreparation.PerformNonTransactionalDatabaseSettings_Exposed(new DummyLoggerForTest(), new string[] { testDbName });
						expectedFileList.ForEach(x => x.Growth = 0.0039M);
						AssertDataFileProperties(testDbConnection, testDbName, cdcFileGroupName, expectedFileList);
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}
		}

		public void TestDBErrorIsHandledWhileCreatingFileGroup()
		{
			const string testDbName = "UpgradePreparationTest$Database";
			bool fileGroupCreated = false;

			using (new TempDirectory())
			using (var connection = Db.NewAdminConnection())
			{
				// Drop test DB first to ensure a clean new one is created
				AdoTestUtils.DropDbIfExists(connection, testDbName);

				AdoTestUtils.CreateDbIfNotExists(connection, testDbName);

				using (var testDbConnection = Db.NewAdminConnection(Db.ServerName, testDbName))
				{
					try
					{
						using (var cmd = connection.Command(string.Format(@"ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE ", testDbName)))
						{
							cmd.ExecuteNonQuery();
						}

						var fileGroupCreator = new DatabaseFileGroupCreator(testDbConnection, testDbName);
						fileGroupCreator.Create("REPORTGROUP", "DataReport", 4, 1);
						fileGroupCreated = true;
					}
					catch (Exception ex)
					{
						AssertEquals(@"Cannot open database ""UpgradePreparationTest$Database"" requested by the login. The login failed.
Login failed for user 'OdysseyAdmin'.", ex.Message);
					}
					finally
					{
						using (var cmd = connection.Command(string.Format(@"ALTER DATABASE {0} SET MULTI_USER WITH ROLLBACK IMMEDIATE ", testDbName)))
						{
							cmd.ExecuteNonQuery();
						}

						AssertEquals(false, fileGroupCreated);
						AssertDatabaseFileGroup(testDbConnection, testDbName, "REPORTGROUP", "DataReport", false);

						AdoTestUtils.DropDbIfExists(connection, testDbName);
					}
				}
			}
		}

		public void TestEnableCdc()
		{
			const string testDbName = "UpgradePreparationTest$Database";

			using (var tempoDirectory = new TempDirectory())
			using (var connection = Db.NewAdminConnection())
			{
				// Drop test DB first to ensure a clean new one is created
				AdoTestUtils.DropDbIfExists(connection, testDbName);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(connection, testDbName);

					using (var testDbConnection = Db.NewAdminConnection(Db.ServerName, testDbName))
					{
						var sqlCreateStmData = @"
CREATE TABLE [StmData] (
[SD_PK] UNIQUEIDENTIFIER NOT NULL,
[SD_Name] VARCHAR(300) NOT NULL DEFAULT '',
[SD_Owner] UNIQUEIDENTIFIER NULL,
[SD_DepartmentGuid] UNIQUEIDENTIFIER NULL,
[SD_Type] CHAR(3) NOT NULL DEFAULT '',
[SD_IsLogged] BIT NOT NULL DEFAULT 0,
[SD_BinaryValue] VARBINARY(MAX) NULL,
[SD_GuidValue] UNIQUEIDENTIFIER NULL,
[SD_IsCancelled] BIT NOT NULL DEFAULT 0,
[SD_PreserveTestValue] BIT NOT NULL DEFAULT 0,
[SD_SystemCreateTimeUtc] SMALLDATETIME NULL,
[SD_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
[SD_SystemLastEditTimeUtc] SMALLDATETIME NULL,
[SD_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);";
						testDbConnection.ExecuteNonQuery(sqlCreateStmData);

						var upgPreparation = new UpgradePreparationForTest(testDbConnection);
						upgPreparation.BiDisableChangeDataCapture_OverrideValueForTest = false;
						upgPreparation.PerformNonTransactionalDatabaseSettings_Exposed(new DummyLoggerForTest(), new string[] { testDbName });
						AssertEquals("After first upgrade", false, CdcDatabase.IsEnabled(connection, testDbName));

						upgPreparation.ShouldEnableCdc_Override = true;
						upgPreparation.PerformNonTransactionalDatabaseSettings_Exposed(new DummyLoggerForTest(), new string[] { testDbName });
						AssertEquals("After second upgrade", true, CdcDatabase.IsEnabled(connection, testDbName));
						AssertCdcJobsAreEnabled(testDbConnection, testDbName, expectedValue: false);

						testDbConnection.ExecuteNonQuery(@"CREATE TABLE dbo.TestTable (TestPk uniqueidentifier)");
						var cdcTable = new CdcTableForTesting("dbo", "TestTable");
						cdcTable.EnableCdc(testDbConnection);
						AssertCdcJobsAreEnabled(testDbConnection, testDbName, expectedValue: true);

						upgPreparation.PerformNonTransactionalDatabaseSettings_Exposed(new DummyLoggerForTest(), new string[] { testDbName });
						AssertEquals("After second upgrade", true, CdcDatabase.IsEnabled(connection, testDbName));
						AssertCdcJobsAreEnabled(testDbConnection, testDbName, expectedValue: false);
					}
				}
				finally
				{
					using (var newConnection = Db.NewAdminConnection())
					{
						AdoTestUtils.DropDbIfExists(newConnection, testDbName);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestUpgradeShouldDisableCdcIfCdcDisabledOnRegistry()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				CdcDatabase.Enable(adminConnection, Db.DatabaseName);
				var cdcTable = new CdcTableForTesting("dbo", "GlbStaff");
				if (!cdcTable.IsCdcEnabled(adminConnection))
				{
					cdcTable.EnableCdc(adminConnection);
				}
				DbRegistry.BiDisableChangeDataCapture.SaveValue(true, adminConnection);
				var logger = new Mock<IUpgradeTaskWorkflowLogger>();

				CombineAssertions("CDC should be enalbed at first.", () =>
				{
					Assert("CDC should be enabled at DB level", CdcDatabase.IsEnabled(adminConnection, Db.DatabaseName));
					Assert("CDC should be enabled on test table after disable captureInstances", cdcTable.IsCdcEnabled(adminConnection));
				});
				var upgPreparation = new UpgradePreparationForTest(adminConnection, shouldEnableCdc_Override: false);
				upgPreparation.PerformNonTransactionalDatabaseSettings_Exposed(logger.Object, new string[] { Db.DatabaseName });

				CombineAssertions(() =>
				{
					logger.Verify(l => l.ShowInfoMessage("Disabling CDC on database"));
					Assert("CDC should not be enabled at DB level", !CdcDatabase.IsEnabled(adminConnection, Db.DatabaseName));
					Assert("CDC should not be enabled on test table after disable captureInstances", !cdcTable.IsCdcEnabled(adminConnection));
					Assert("BiDisableChangeDataCapture set to false", !DbRegistry.BiDisableChangeDataCapture.LoadValue(adminConnection));
				});
			}
		}

		public void TestStorageDocsAutoStatistics()
		{
			var storageDocs = Db.Connection.GetDatabases(DatabaseType.SD);
			AssertEquals("[PRE-CONDITION] Are there any eDocs databases?", true, storageDocs.Any());

			foreach (var db in storageDocs)
			{
				TryToCreateAutoStats(db);
				CombineAssertions(() =>
				{
					AssertEquals("Auto create statistics for DB = " + db, false, GetAutoCreateStatsValue(db));
					AssertEquals("Auto created statistics count for DB = " + db, 0, GetAutoStatsCount(db));
				});
			}
		}

		public void TestConfigurationSettings()
		{
			AssertEquals("clr enabled", true, Convert.ToBoolean(Db.Connection.ExecuteScalar("select value_in_use from sys.configurations where name = 'clr enabled'")));
			AssertEquals("max text repl size", -1, Convert.ToInt32(Db.Connection.ExecuteScalar("select value_in_use from sys.configurations where name = 'max text repl size (B)'")));
			AssertEquals("default language", 0, Convert.ToInt32(Db.Connection.ExecuteScalar("select value_in_use from sys.configurations where name = 'default language'")));
			AssertEquals("remote access", true, Convert.ToBoolean(Db.Connection.ExecuteScalar("select value_in_use from sys.configurations where name = 'remote access'")));
		}

		public void TestDatabaseAutoCloseIsOff()
		{
			var allDbs = Db.Connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW);

			foreach (var db in allDbs)
			{
				AssertEquals("AUTO CLOSE setting for DB = " + db, false, (bool)GetDbPropertyValue(Db.Connection, "is_auto_close_on", db));
			}
		}

		public void TestPageVerifyOption()
		{
			AssertEquals("PAGE_VERIFY", 2, Convert.ToInt32(GetDbPropertyValue(Db.Connection, "page_verify_option", Db.DatabaseName)));
		}

		/// <summary>
		/// Using multiple database files and file groups is an efficient way to
		/// optimise performance and disaster recovery
		/// by separating data based on size, usage and relevance.
		/// In order to enforce our support to multiple files our test databases
		/// must have more than one data file.
		/// </summary>
		public void TestDatabaseHasMoreThanOneDataFileAndFileGroup()
		{
			AssertDatabaseHasMoreThanOneDataFileAndFileGroup(Db.Connection, Db.DatabaseName);
		}

		protected override void SetUp()
		{
			base.SetUp();

			dbRecoveryModelManagerMock = new DbRecoveryModelManagerMock();
			dbRecoveryModelManagerSubstitute = ObjectFactory.Substitute(dbRecoveryModelManagerMock.Object);
		}

		protected override void TearDown()
		{
			dbRecoveryModelManagerSubstitute?.Dispose();
			dbRecoveryModelManagerSubstitute = null;

			base.TearDown();
		}

		DbRecoveryModelManagerMock dbRecoveryModelManagerMock;
		IDisposable dbRecoveryModelManagerSubstitute;

		void AssertDatabaseFileGroup(DbConnection connection, string dbName, string groupName, string fileSuffix, bool expectToExist = true)
		{
			var sqlText = string.Format("SELECT data_space_id FROM [{0}].sys.filegroups WHERE name = '{1}'", dbName, groupName);
			var groupDataSpaceIdObj = connection.ExecuteScalar(sqlText);
			var groupDataSpaceId = (groupDataSpaceIdObj == null) ? 0 : Convert.ToInt32(groupDataSpaceIdObj);
			AssertEquals("Group data_space_d > 0?", expectToExist, groupDataSpaceId > 0);

			sqlText = string.Format("SELECT data_space_id FROM [{0}].sys.database_files WHERE name = '{0}_{1}'", dbName, fileSuffix);
			var fileDataSpaceIdObj = connection.ExecuteScalar(sqlText);
			var fileDataSpaceId = (fileDataSpaceIdObj == null) ? 0 : Convert.ToInt32(fileDataSpaceIdObj);
			AssertEquals("File data_space_d", expectToExist, (groupDataSpaceId > 0 && (groupDataSpaceId == fileDataSpaceId)));
		}

		void AssertDatabaseHasMoreThanOneDataFileAndFileGroup(DbConnection connection, string dbName)
		{
			var dataFiles = connection.GetDBDataFiles(dbName);
			AssertEquals("Does main DB have at least 2 DATA files?", true, dataFiles.Length >= 2);
			AssertEquals("Is there a '.mdf' DATA file?", true, dataFiles.Any(f => f.Trim().EndsWith(".mdf", StringComparison.OrdinalIgnoreCase)));
			AssertEquals("Is there a '.ndf' DATA file?", true, dataFiles.Any(f => f.Trim().EndsWith(".ndf", StringComparison.OrdinalIgnoreCase)));

			var fileGroupCount = Convert.ToInt32(connection.ExecuteScalar(string.Format("SELECT count(*) FROM [{0}].sys.filegroups", dbName)));
			AssertEquals("Is file group count >= 2?", true, fileGroupCount >= 2);
		}

		object GetDbPropertyValue(DbConnection connection, string ptyName, string dbName)
		{
			var sqlText = String.Format("SELECT [{0}] FROM sys.databases WHERE name = '{1}'", ptyName, dbName);
			return connection.ExecuteScalar(sqlText);
		}

		void AssertCdcJobsAreEnabled(DbConnection connection, string dbName, bool expectedValue)
		{
			var sqlText = String.Format(@"
DECLARE @Output int = 0;

IF EXISTS (SELECT null FROM msdb.sys.objects WHERE name = 'cdc_jobs')
BEGIN
	DECLARE @SqlText nvarchar(max) =
	N'IF EXISTS (SELECT NULL
		FROM msdb.dbo.cdc_jobs cdcjob
		INNER JOIN sys.databases db ON cdcjob.database_id = db.database_id
		INNER JOIN msdb.dbo.sysjobs sysjob ON cdcjob.job_id = sysjob.job_id
		WHERE db.name = ''{0}'' AND sysjob.enabled = 1)
	SET @Output = 1 ELSE SET @Output = 0';
	DECLARE @ParmDefinition nvarchar(max) = N'@Output int OUTPUT';

	EXEC sp_executesql @SqlText, @ParmDefinition, @Output OUTPUT
END

SELECT @Output", dbName);

			AssertEquals("CDC jobs enabled?", expectedValue, Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
		}

		void SetDbProperty(DbConnection connection, string dbName, string setCommand)
		{
			var sqlText = String.Format("ALTER DATABASE [{0}] SET {1};", dbName, setCommand);
			connection.ExecuteNonQuery(sqlText);
		}

		void AssertDbProperties(DbConnection connection, string dbName,
			bool expectedTrustworthy,
			int expectedPageVerify,
			bool expectedAutoClose,
			int expectedCompatibilityLevel,
			int expectedSnapshotIsolationState,
			bool expectedReadCommittedSnapshot,
			bool expectedBroker,
			bool expectedArithAbort)
		{
			AssertEquals("is_trustworthy_on", expectedTrustworthy, Convert.ToBoolean(GetDbPropertyValue(connection, "is_trustworthy_on", dbName)));
			AssertEquals("page_verify_option", expectedPageVerify, Convert.ToInt32(GetDbPropertyValue(connection, "page_verify_option", dbName)));
			AssertEquals("is_auto_close_on", expectedAutoClose, Convert.ToBoolean(GetDbPropertyValue(connection, "is_auto_close_on", dbName)));
			AssertEquals("compatibility_level", expectedCompatibilityLevel, Convert.ToInt32(GetDbPropertyValue(connection, "compatibility_level", dbName)));
			AssertEquals("snapshot_isolation_state", expectedSnapshotIsolationState, Convert.ToInt32(GetDbPropertyValue(connection, "snapshot_isolation_state", dbName)));
			AssertEquals("is_read_committed_snapshot_on", expectedReadCommittedSnapshot, Convert.ToBoolean(GetDbPropertyValue(connection, "is_read_committed_snapshot_on", dbName)));
			AssertEquals("is_broker_enabled", expectedBroker, Convert.ToBoolean(GetDbPropertyValue(connection, "is_broker_enabled", dbName)));
			AssertEquals("is_arithabort_on", expectedArithAbort, Convert.ToBoolean(GetDbPropertyValue(connection, "is_arithabort_on", dbName)));
		}

		void AssertDataFileCount(DbConnection connection, string dbName, int expectedDataFileCount)
		{
			AssertEquals("Data file count", expectedDataFileCount, connection.GetDBDataFiles(dbName).Length);
		}

		void AssertDataFileProperties(AdminConnection connection, string dbName, string fileGroupName, IEnumerable<DatabaseFileGroupCreator.FileInfo> expectedInfo)
		{
			var fileInfo = new DatabaseFileGroupCreator(connection, dbName).GetFileGroupInfo(fileGroupName);
			AssertContainsExactElementsInAnyOrder("FileGroup Info", expectedInfo.Select(x => x.ToString()), fileInfo.Select(x => x.ToString()));
		}

		void TryToCreateAutoStats(string dbName)
		{
			var sql = string.Format("SELECT TOP(1) * FROM [{0}].dbo.StorageDocs WHERE SC_DataType <> '';", dbName);
			Db.Connection.ExecuteNonQuery(sql);
		}

		bool GetAutoCreateStatsValue(string dbName)
		{
			var sql = string.Format("SELECT is_auto_create_stats_on FROM sys.databases WHERE name = '{0}';", dbName);
			return (bool)Db.Connection.ExecuteScalar(sql);
		}

		int GetAutoStatsCount(string dbName)
		{
			var sql = String.Format(@"
				SELECT
					COUNT(*)
				FROM
					[{0}].sys.objects    AS o
					JOIN [{0}].sys.stats AS s  ON s.object_id = o.object_id
				WHERE
					o.is_ms_shipped = 0
					AND s.auto_created = 1;",
				dbName);

			return (int)Db.Connection.ExecuteScalar(sql);
		}
	}
}
