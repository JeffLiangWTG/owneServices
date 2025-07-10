using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.IO;
using Enterprise.DataTools.DbBackupAndRestore.Business.Common;
using Enterprise.DataTools.DbBackupAndRestore.Testing.Restore;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business.Testing.Restore
{
	class DatabaseRestoreTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestCopyProductionToTest_KillOtherConnections()
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);
			const string testAuditDatabase = testDbName + "_Audit";
			const string testEdwDatabase = testDbName + "_EDW";

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testAuditDatabase);
					AdoTestUtils.DropDbIfExists(connection, testEdwDatabase);
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}))
			{
				var dbFileCollection = new DbFileInfoCollection();
				SetupTestDatabaseFile(dbFileCollection, testDbName, tempDir.DirectoryName, DbFileInfo.DbTypeMain, "");
				SetupTestDatabaseFile(dbFileCollection, testAuditDatabase, tempDir.DirectoryName, DbFileInfo.DbTypeAuditDB, "_Audit");
				SetupTestDatabaseFile(dbFileCollection, testEdwDatabase, tempDir.DirectoryName, DbFileInfo.DbTypeEdwDB, "_EDW");

				var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>());
				restoreManagerMock
					.Protected()
					.Setup(
						"DoRestore",
						ItExpr.IsAny<AdminConnection>(),
						ItExpr.IsAny<DbServerConfiguration>(),
						ItExpr.IsAny<AuditDbServerConfiguration>(),
						ItExpr.IsAny<EdwDbServerConfiguration>(),
						ItExpr.IsAny<DbRestoreSettings>()
					)
					.CallBase();
				restoreManagerMock
					.Protected()
					.Setup(
						"RestorePrimaryReplica",
						ItExpr.IsAny<List<DatabaseDetails>>(),
						ItExpr.IsAny<DbRestoreSettings>()
					)
					.CallBase();
				restoreManagerMock
					.Protected()
					.Setup(
						"ExecuteRestoreWithNoRecovery",
						ItExpr.IsAny<DatabaseDetails>(),
						ItExpr.IsAny<DbRestoreSettings>()
					)
					.CallBase();
				restoreManagerMock
					.Protected()
					.Setup(
						"ExecuteRestoreWithRecovery",
						ItExpr.IsAny<DatabaseDetails>(),
						ItExpr.IsAny<DbRestoreSettings>()
					)
					.CallBase();
				restoreManagerMock
					.Protected()
					.Setup(
						"DoRestoreWithNoRecovery",
						ItExpr.IsAny<DatabaseDetails>(),
						ItExpr.IsAny<DbRestoreSettings>(),
						ItExpr.IsAny<AdminConnection>()
					)
					.CallBase();

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = new AuditDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testAuditDatabase}.bak"));
				var edwServerConfig = new EdwDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testEdwDatabase}.bak"));
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.CopyProdToTest);

				restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

				using (var mainDbConnection = Db.NewAdminConnection(testDbName))
				using (var auditDbConnection = Db.NewAdminConnection(testAuditDatabase))
				using (var edwDbConnection = Db.NewAdminConnection(testEdwDatabase))
				{
					var mainConnectionSpid = mainDbConnection.SPID;
					var auditDbConnectionSpid = auditDbConnection.SPID;
					var edwDbConnectionSpid = edwDbConnection.SPID;

					using (DbConnection connection = Db.NewAdminConnection())
					{
						var existRegItems = TestDataHelpers.GetStmData(connection, testDbName);
						TestDataHelpers.PreconditionRegistryItems(connection, testDbName, existRegItems.Keys);
						TestDataHelpers.InsertStmData(connection, testDbName, "DATABASE_SCHEMA_VERSION", DataRegistry.MinUpgradableDbMajorSchemaVersion.ToString(), true);
						TestDataHelpers.SetDatabaseAsForTest(connection, testDbName);
						Assert($"{testDbName} must be set IsTestOrTrainingDb", Utilities.IsTestOrTrainingDb(connection, testDbName));
					}

					var cancellationTokenSource = new CancellationTokenSource();
					var connectionTask = Task.Run(() =>
					{
						while (!cancellationTokenSource.Token.IsCancellationRequested)
						{
							try
							{
								using var mainDbConnection = Db.NewAdminConnection(testDbName);
							}
							catch (SqlException)
							{
							}
							finally
							{
								Thread.Sleep(100);
							}
						}
					}, cancellationTokenSource.Token);

					// Act
					restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

					cancellationTokenSource.Cancel();
					connectionTask.Wait();

					// Assert
					CombineAssertions(() =>
					{
						AssertEquals($"{testDbName} connection should be killed", false, OtherConnectionExists(mainConnectionSpid, testDbName));
						AssertEquals($"{testAuditDatabase} connection should be killed", false, OtherConnectionExists(auditDbConnectionSpid, testAuditDatabase));
						AssertEquals($"{testEdwDatabase} connection should be killed", false, OtherConnectionExists(edwDbConnectionSpid, testEdwDatabase));
					});
				}
			}
		}

		public void TestRestoreSequence()
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>());
			restoreManagerMock
				.Protected()
				.Setup(
					"DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();
			restoreManagerMock
				.Protected()
				.Setup(
					"RestorePrimaryReplica",
					ItExpr.IsAny<List<DatabaseDetails>>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();

			string lastRestoredDbType = null;
			var counter = 0;

			restoreManagerMock
				.Protected()
				.Setup(
					"ExecuteRestoreWithNoRecovery",
					ItExpr.IsAny<DatabaseDetails>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.Callback((DatabaseDetails dbDetails, DbRestoreSettings dbRestoreSettings) =>
				{
					counter++;
					lastRestoredDbType = dbDetails.DatabaseType;
				});

			restoreManagerMock
				.Protected()
				.Setup(
					"ExecuteRestoreWithRecovery",
					ItExpr.IsAny<DatabaseDetails>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.Callback(() => AssertEquals(3, counter));

			restoreManagerMock.Setup(m => m.OverwriteDbInAvailabilityGroupPrompt()).Returns(true);
			restoreManagerMock.Setup(m => m.AuditSkipRestorePrompt()).Returns(true);

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}))
			{
				var dbFileCollection = GetSetupDbFileInfoCollection(testDbName, tempDir.DirectoryName);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = new AuditDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var edwServerConfig = new EdwDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.RestoreWithRecovery, addDbToAvailabilityGroup: false);

				//Act & Assert
				AssertNoExceptionThrown(() => restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings));
				AssertEquals(DbFileInfo.DbTypeMain, lastRestoredDbType);
			}
		}

		public void TestRestore_DbInAlwaysOnAvailabilityGroup()
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);

			var alwaysOnHelperMock = GetAlwaysOnHelperMock();

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>());
			restoreManagerMock
				.Protected()
				.Setup(
					"DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.Verifiable();

			restoreManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}))
			{
				var dbFileCollection = GetSetupDbFileInfoCollection(testDbName, tempDir.DirectoryName);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.CopyProdToTest, addDbToAvailabilityGroup: true, availabilityGroup: "testGroup");

				//Act & Assert
				AssertNoExceptionThrown(() => restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings));

				alwaysOnHelperMock.Verify(m => m.IsDbPartOfAlwaysOn(It.IsAny<DbConnection>(), It.IsAny<string>()), Times.Once);
				alwaysOnHelperMock.Verify(m => m.IsPrimaryReplicaOnAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>()), Times.Once);
				restoreManagerMock.Protected()
					.Verify("DoRestore",
						Times.Once(),
						ItExpr.IsAny<AdminConnection>(),
						ItExpr.IsAny<DbServerConfiguration>(),
						ItExpr.IsAny<AuditDbServerConfiguration>(),
						ItExpr.IsAny<EdwDbServerConfiguration>(),
						ItExpr.IsAny<DbRestoreSettings>()
						);
			}
		}

		public void TestRestore_DbInAlwaysOnAvailabilityGroup_SecondaryReplica()
		{
			TestRestore_DbInAlwaysOnAvailabilityGroup_AddToAvailabilityGroup_SecondaryReplica();
			TestRestore_DbInAlwaysOnAvailabilityGroup_RemoveFromAvailabilityGroup_SecondaryReplica();
		}

		void TestRestore_DbInAlwaysOnAvailabilityGroup_AddToAvailabilityGroup_SecondaryReplica()
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);

			var alwaysOnHelperMock = GetAlwaysOnHelperMock();
			alwaysOnHelperMock
				.Setup(a => a.IsPrimaryReplicaOnAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>()))
				.Returns(false);

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>());
			restoreManagerMock
				.Protected()
				.Setup(
					"DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.Verifiable();

			restoreManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}))
			{
				var dbFileCollection = GetSetupDbFileInfoCollection(testDbName, tempDir.DirectoryName);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.CopyProdToTest, addDbToAvailabilityGroup: true, availabilityGroup: "testGroup");

				//Act & Assert
				AssertExceptionThrown<DbBackupAndRestoreException>(
					$"The database {testDbName} is part of an Always On availability group, and the destination server {mainServerConfig.ServerName} is currently hosting a secondary replica. " +
					"Ensure that the server is hosting the primary replica, then retry the command.",
					() => restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings));

				alwaysOnHelperMock.Verify(m => m.IsDbPartOfAlwaysOn(It.IsAny<DbConnection>(), It.IsAny<string>()), Times.Once);
				alwaysOnHelperMock.Verify(m => m.IsPrimaryReplicaOnAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>()), Times.Once);
				restoreManagerMock.Protected()
					.Verify("DoRestore",
						Times.Never(),
						ItExpr.IsAny<AdminConnection>(),
						ItExpr.IsAny<DbServerConfiguration>(),
						ItExpr.IsAny<AuditDbServerConfiguration>(),
						ItExpr.IsAny<EdwDbServerConfiguration>(),
						ItExpr.IsAny<DbRestoreSettings>()
						);
			}
		}

		void TestRestore_DbInAlwaysOnAvailabilityGroup_RemoveFromAvailabilityGroup_SecondaryReplica()
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);

			var alwaysOnHelperMock = GetAlwaysOnHelperMock();
			alwaysOnHelperMock
				.Setup(a => a.IsPrimaryReplicaOnAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>()))
				.Returns(false);

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>());
			restoreManagerMock
				.Protected()
				.Setup(
					"DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.Verifiable();

			restoreManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}))
			{
				var dbFileCollection = GetSetupDbFileInfoCollection(testDbName, tempDir.DirectoryName);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.CopyProdToTest, addDbToAvailabilityGroup: false, availabilityGroup: "testGroup");

				//Act & Assert
				AssertExceptionThrown<DbBackupAndRestoreException>(
					$"The database {testDbName} is part of an Always On availability group, and the destination server {mainServerConfig.ServerName} is currently hosting a secondary replica. " +
					"Ensure that the server is hosting the primary replica, then retry the command.",
					() => restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings));

				alwaysOnHelperMock.Verify(m => m.IsDbPartOfAlwaysOn(It.IsAny<DbConnection>(), It.IsAny<string>()), Times.Once);
				alwaysOnHelperMock.Verify(m => m.IsPrimaryReplicaOnAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>()), Times.Once);
				restoreManagerMock.Protected()
					.Verify("DoRestore",
						Times.Never(),
						ItExpr.IsAny<AdminConnection>(),
						ItExpr.IsAny<DbServerConfiguration>(),
						ItExpr.IsAny<AuditDbServerConfiguration>(),
						ItExpr.IsAny<EdwDbServerConfiguration>(),
						ItExpr.IsAny<DbRestoreSettings>()
						);
			}
		}

		public void TestRestore_DbInAlwaysOnAvailabilityGroup_PrimaryReplica()
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);

			var alwaysOnHelperMock = GetAlwaysOnHelperMock();

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>(), Mock.Of<IDatabaseWaiter>());
			restoreManagerMock
				.Protected()
				.Setup(
					"RestorePrimaryReplica",
					ItExpr.IsAny<List<DatabaseDetails>>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.Verifiable();
			restoreManagerMock
				.Protected()
				.Setup(
					"DoRestoreWithNoRecovery",
					ItExpr.IsAny<DatabaseDetails>(),
					ItExpr.IsAny<DbRestoreSettings>(),
					ItExpr.IsAny<AdminConnection>()
				)
				.Verifiable();
			restoreManagerMock
				.Protected()
				.Setup("DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();

			restoreManagerMock.Setup(m => m.OverwriteDbInAvailabilityGroupPrompt()).Returns(true);
			restoreManagerMock.Setup(m => m.AuditSkipRestorePrompt()).Returns(true);

			restoreManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}))
			{
				var dbFileCollection = GetSetupDbFileInfoCollection(testDbName, tempDir.DirectoryName);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.CopyProdToTest, addDbToAvailabilityGroup: true, availabilityGroup: "testGroup");

				//Act & Assert
				AssertNoExceptionThrown(() => restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings));

				alwaysOnHelperMock.Verify(m => m.GetSecondaryReplicaNamesListForAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>()), Times.Once);
				alwaysOnHelperMock.Verify(m => m.RemoveDatabaseFromAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
				alwaysOnHelperMock.Verify(m => m.AddDatabaseToAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
				alwaysOnHelperMock.Verify(m => m.JoinSecondaryDatabaseToAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
				restoreManagerMock
					.Protected()
					.Verify(
						"RestorePrimaryReplica",
						Times.Once(),
						ItExpr.IsAny<List<DatabaseDetails>>(),
						ItExpr.IsAny<DbRestoreSettings>()
					);
				restoreManagerMock
					.Protected()
					.Verify(
						"DoRestoreWithNoRecovery",
						Times.Once(),
						ItExpr.IsAny<DatabaseDetails>(),
						ItExpr.IsAny<DbRestoreSettings>(),
						ItExpr.IsAny<AdminConnection>()
					);
			}
		}

		public void TestRestore_DbInAlwaysOnAvailabilityGroup_PrimaryReplica_WithAudit()
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);

			var alwaysOnHelperMock = GetAlwaysOnHelperMock();

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>(), Mock.Of<IDatabaseWaiter>());
			restoreManagerMock
				.Protected()
				.Setup(
					"RestorePrimaryReplica",
					ItExpr.IsAny<List<DatabaseDetails>>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.Verifiable();
			restoreManagerMock
				.Protected()
				.Setup(
					"DoRestoreWithNoRecovery",
					ItExpr.IsAny<DatabaseDetails>(),
					ItExpr.IsAny<DbRestoreSettings>(),
					ItExpr.IsAny<AdminConnection>()
				)
				.Verifiable();
			restoreManagerMock
				.Protected()
				.Setup("DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();

			restoreManagerMock.Setup(m => m.OverwriteDbInAvailabilityGroupPrompt()).Returns(true);
			restoreManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}))
			{
				var dbFileCollection = GetSetupDbFileInfoCollection(testDbName, tempDir.DirectoryName);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = new AuditDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.CopyProdToTest, addDbToAvailabilityGroup: true, availabilityGroup: "testGroup");

				//Act & Assert
				AssertNoExceptionThrown(() => restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings));

				alwaysOnHelperMock.Verify(m => m.GetSecondaryReplicaNamesListForAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>()), Times.Exactly(2));
				alwaysOnHelperMock.Verify(m => m.RemoveDatabaseFromAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
				alwaysOnHelperMock.Verify(m => m.AddDatabaseToAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
				alwaysOnHelperMock.Verify(m => m.JoinSecondaryDatabaseToAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
				restoreManagerMock
					.Protected()
					.Verify(
						"RestorePrimaryReplica",
						Times.Once(),
						ItExpr.IsAny<List<DatabaseDetails>>(),
						ItExpr.IsAny<DbRestoreSettings>()
					);
				restoreManagerMock
					.Protected()
					.Verify(
						"DoRestoreWithNoRecovery",
						Times.Exactly(2),
						ItExpr.IsAny<DatabaseDetails>(),
						ItExpr.IsAny<DbRestoreSettings>(),
						ItExpr.IsAny<AdminConnection>()
					);
			}
		}

		[UseSnapshotProtection]
		public void TestRestore_DbInAlwaysOnAvailabilityGroup_PrimaryReplica_WithAuditAndEdw()
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);

			var alwaysOnHelperMock = GetAlwaysOnHelperMock();

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>(), Mock.Of<IDatabaseWaiter>());
			restoreManagerMock
				.Protected()
				.Setup(
					"RestorePrimaryReplica",
					ItExpr.IsAny<List<DatabaseDetails>>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();
			restoreManagerMock
				.Protected()
				.Setup(
					"DoRestoreWithNoRecovery",
					ItExpr.IsAny<DatabaseDetails>(),
					ItExpr.IsAny<DbRestoreSettings>(),
					ItExpr.IsAny<AdminConnection>()
				)
				.CallBase();
			restoreManagerMock
				.Protected()
				.Setup("DoRestore",
				ItExpr.IsAny<AdminConnection>(),
				ItExpr.IsAny<DbServerConfiguration>(),
				ItExpr.IsAny<AuditDbServerConfiguration>(),
				ItExpr.IsAny<EdwDbServerConfiguration>(),
				ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();

			restoreManagerMock.Setup(m => m.OverwriteDbInAvailabilityGroupPrompt()).Returns(true);
			restoreManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;

			var messages = new List<string>();
			restoreManagerMock.Object.OnSubtaskStarted += new ProgressEvent((message, i) => messages.Add(message));

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
					AdoTestUtils.DropDbIfExists(connection, $"{testDbName}_Audit");
					AdoTestUtils.DropDbIfExists(connection, $"{testDbName}_EDW");
				}
			}))
			{
				var dbFileCollection = GetSetupDbFileInfoCollection(testDbName, tempDir.DirectoryName);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = new AuditDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var edwServerConfig = new EdwDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.CopyProdToTest, addDbToAvailabilityGroup: true, availabilityGroup: "testGroup");

				//Act & Assert
				AssertNoExceptionThrown(() => restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings));

				alwaysOnHelperMock.Verify(m => m.GetSecondaryReplicaNamesListForAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>()), Times.Exactly(3));
				alwaysOnHelperMock.Verify(m => m.RemoveDatabaseFromAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
				alwaysOnHelperMock.Verify(m => m.AddDatabaseToAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
				alwaysOnHelperMock.Verify(m => m.JoinSecondaryDatabaseToAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
				restoreManagerMock
					.Protected()
					.Verify(
						"RestorePrimaryReplica",
						Times.Exactly(1),
						ItExpr.IsAny<List<DatabaseDetails>>(),
						ItExpr.IsAny<DbRestoreSettings>()
					);
				restoreManagerMock
					.Protected()
					.Verify(
						"DoRestoreWithNoRecovery",
						Times.Exactly(3),
						ItExpr.IsAny<DatabaseDetails>(),
						ItExpr.IsAny<DbRestoreSettings>(),
						ItExpr.IsAny<AdminConnection>()
					);
			}
		}

		public void TestRestore_DbInAlwaysOnAvailabilityGroup_RestoreWithNoRecoveryNotSupportedForPrimaryReplica()
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);

			var alwaysOnHelperMock = GetAlwaysOnHelperMock();

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>());
			restoreManagerMock
				.Protected()
				.Setup(
					"DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.Verifiable();

			restoreManagerMock.Setup(m => m.OverwriteDbInAvailabilityGroupPrompt()).Returns(true);
			restoreManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}))
			{
				var dbFileCollection = GetSetupDbFileInfoCollection(testDbName, tempDir.DirectoryName);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.RestoreWithNoRecovery, addDbToAvailabilityGroup: true, availabilityGroup: "testGroup");

				//Act & Assert
				AssertExceptionThrown<DbBackupAndRestoreException>(
					$"{testDbName} is on an AlwaysOn primary availability replica. Restore WITH NORECOVERY is not supported for primary replicas.",
					() => restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings));

				alwaysOnHelperMock.Verify(m => m.IsDbPartOfAlwaysOn(It.IsAny<DbConnection>(), It.IsAny<string>()), Times.Once);
				alwaysOnHelperMock.Verify(m => m.IsPrimaryReplicaOnAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>()), Times.Once);
				restoreManagerMock.Protected()
					.Verify("DoRestore",
						Times.Never(),
						ItExpr.IsAny<AdminConnection>(),
						ItExpr.IsAny<DbServerConfiguration>(),
						ItExpr.IsAny<AuditDbServerConfiguration>(),
						ItExpr.IsAny<EdwDbServerConfiguration>(),
						ItExpr.IsAny<DbRestoreSettings>()
						);
			}
		}

		public void TestRestore_IncludeRefDbs()
		{
			TestRestore_DbInAlwaysOnAvailabilityGroup_EnsureAllDatabasesAreRemovedFromAndAddedBackToAvailabilityGroup(true);
		}

		public void TestRestore_DoNotIncludeRefDbs()
		{
			TestRestore_DbInAlwaysOnAvailabilityGroup_EnsureAllDatabasesAreRemovedFromAndAddedBackToAvailabilityGroup(false);
		}

		void TestRestore_DbInAlwaysOnAvailabilityGroup_EnsureAllDatabasesAreRemovedFromAndAddedBackToAvailabilityGroup(bool includeRefDbs)
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);

			var alwaysOnHelperMock = GetAlwaysOnHelperMock();

			// specifically set "testDbName_NotInAG" to be missing from AlwaysOn to ensure it is not removed from AG
			alwaysOnHelperMock
				.Setup(a => a.IsDbPartOfAlwaysOn(It.IsAny<AdminConnection>(), testDbName + "_NotInAG"))
				.Returns(false);

			var shouldThrowException = false;

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>(), Mock.Of<IDatabaseWaiter>());
			restoreManagerMock
				.Protected()
				.Setup(
					"ExecuteRestoreWithNoRecovery",
					ItExpr.IsAny<DatabaseDetails>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.Callback((
					DatabaseDetails dbDetails,
					DbRestoreSettings dbRestoreSettings
				) =>
				{
					if (dbDetails.DbFiles.Count > 0 && dbDetails.DbFiles[0].DbType == DbFileInfo.DbTypeEDocs)
					{
						var actualDbName = restoreManagerMock.Object.GetActualDatabaseName(dbDetails.DatabaseName, dbDetails.DbFiles[0]);
						if (dbDetails.DbFiles.Cast<DbFileInfo>().Any(dbFile => restoreManagerMock.Object.GetActualDatabaseName(dbDetails.DatabaseName, dbFile) != actualDbName))
						{
							shouldThrowException = true;
						}
					}
				});
			restoreManagerMock
				.Protected()
				.Setup("DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();
			restoreManagerMock.Setup(m => m.OverwriteDbInAvailabilityGroupPrompt()).Returns(true);
			restoreManagerMock.Setup(m => m.AuditSkipRestorePrompt()).Returns(true);

			restoreManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;

			var infoMessages = new List<string>();
			restoreManagerMock.Object.OnShowInfoMessage += message => infoMessages.Add(message);

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}))
			{
				var dbFileCollection = new DbFileInfoCollection();
				SetupTestDatabaseFile(dbFileCollection, testDbName, tempDir.DirectoryName, DbFileInfo.DbTypeMain, "");
				SetupTestDatabaseFile(dbFileCollection, testDbName + "_Audit", tempDir.DirectoryName, DbFileInfo.DbTypeUserRepository, "_UserRepository");
				SetupTestDatabaseFile(dbFileCollection, testDbName + "_EDW", tempDir.DirectoryName, DbFileInfo.DbTypeEdwDB, "_EDW");
				SetupTestDatabaseFile(dbFileCollection, testDbName + "_SD001", tempDir.DirectoryName, DbFileInfo.DbTypeEDocs, "_SD001");
				SetupTestDatabaseFile(dbFileCollection, testDbName + "_SD002", tempDir.DirectoryName, DbFileInfo.DbTypeEDocs, "_SD002");
				SetupTestDatabaseFile(dbFileCollection, testDbName + "_NotInAG", tempDir.DirectoryName, DbFileInfo.DbTypeEDocs, "_NotInAG");
				SetupTestDatabaseFile(dbFileCollection, "CW1Bkp_T-20151023-111725_S-SYDCO-WJBL-1_D-OdysseyWithDescription_RefDb_Trf_CA", tempDir.DirectoryName, DbFileInfo.DbTypeRefDB, "_RefDb_Trf_CA");
				SetupTestDatabaseFile(dbFileCollection, "CW1Bkp_T-20230630-114910_S-SYDCO-LMZT-1_D-CW-RefDatabase", tempDir.DirectoryName, DbFileInfo.DbTypeSingleSharedRefDB, "CW-RefDatabase");

				dbFileCollection.SetDbFileTypeVisible(DbFileInfo.DbTypeAuditDB, false);
				dbFileCollection.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, includeRefDbs);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = new AuditDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var edwServerConfig = new EdwDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.CopyProdToTest, addDbToAvailabilityGroup: true, availabilityGroup: "testGroup");

				//Act
				restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

				//Assert
				AssertEquals("Error in restoring secondary replicas: dbFiles contains duplicate files.", false, shouldThrowException);

				Assert("Main Database Removed", infoMessages.Contains($"Removing {testDbName} database from availability group"));
				Assert("EDW Database Removed", infoMessages.Contains($"Removing {testDbName}_EDW database from availability group"));
				Assert("User Repository Database Removed", infoMessages.Contains($"Removing {testDbName}_UserRepository database from availability group"));
				Assert("eDocs Database 1 Removed", infoMessages.Contains($"Removing {testDbName}_SD001 database from availability group"));
				Assert("eDocs Database 2 Removed", infoMessages.Contains($"Removing {testDbName}_SD002 database from availability group"));
				Assert("eDocs Database 3 not Removed", !infoMessages.Contains($"Removing {testDbName}_NotInAG database from availability group"));
				Assert("Reference Database Removed", infoMessages.Contains($"Removing {testDbName}_RefDb_Trf_CA database from availability group") == includeRefDbs);
				Assert("Shared Reference Database not Removed", !infoMessages.Contains($"Removing {testDbName}CW-RefDatabase database from availability group"));

				Assert("Main Database Rejoined", infoMessages.Contains($"Joining database {testDbName} on primary replica {Db.ServerName} to availability group"));
				Assert("EDW Database Rejoined", infoMessages.Contains($"Joining database {testDbName}_EDW on primary replica {Db.ServerName} to availability group"));
				Assert("User Repository Database Rejoined", infoMessages.Contains($"Joining database {testDbName}_UserRepository on primary replica {Db.ServerName} to availability group"));
				Assert("eDocs Database 1 Rejoined", infoMessages.Contains($"Joining database {testDbName}_SD001 on primary replica {Db.ServerName} to availability group"));
				Assert("eDocs Database 2 Rejoined", infoMessages.Contains($"Joining database {testDbName}_SD002 on primary replica {Db.ServerName} to availability group"));
				Assert("eDocs Database 3 Rejoined", infoMessages.Contains($"Joining database {testDbName}_NotInAG on primary replica {Db.ServerName} to availability group"));
				Assert("Reference Database Rejoined", infoMessages.Contains($"Joining database {testDbName}_RefDb_Trf_CA on primary replica {Db.ServerName} to availability group") == includeRefDbs);
				Assert("Shared Reference Database not Rejoined", !infoMessages.Contains($"Joining database {testDbName}CW-RefDatabase on primary replica {Db.ServerName} to availability group"));
			}
		}

		public void TestRestore_DbInAlwaysOnAvailabilityGroup_EnsureAllDatabasesAreRemovedFromGroup()
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);

			var alwaysOnHelperMock = GetAlwaysOnHelperMock();

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>(), Mock.Of<IDatabaseWaiter>());
			restoreManagerMock
				.Protected()
				.Setup("DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();

			restoreManagerMock.Setup(m => m.AuditSkipRestorePrompt()).Returns(true);
			restoreManagerMock.Setup(m => m.OverwriteDbInAvailabilityGroupPrompt()).Returns(true);
			restoreManagerMock.Setup(m => m.RemoveDbFromAvailabilityGroupPrompt()).CallBase();

			var calledConfirmRemoveDbFromAvailabilityGroup = false;
			restoreManagerMock.Object.OnConfirmationPrompt = (args) =>
			{
				var expectedMessage = "";
				if (args.PromptType == PromptType.ConfirmRemoveDbFromAvailabilityGroup)
				{
					expectedMessage = "Database will be removed from all nodes in the availability group. Are you sure?";
					calledConfirmRemoveDbFromAvailabilityGroup = true;
				}

				AssertEquals(expectedMessage, args.PromptMessage);
				args.Result = ConfirmationPromptResult.Yes;
			};

			restoreManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;

			var infoMessages = new List<string>();
			restoreManagerMock.Object.OnShowInfoMessage += message => infoMessages.Add(message);

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}))
			{
				var dbFileCollection = new DbFileInfoCollection();
				SetupTestDatabaseFile(dbFileCollection, testDbName, tempDir.DirectoryName, DbFileInfo.DbTypeMain, "");
				SetupTestDatabaseFile(dbFileCollection, testDbName + "_Audit", tempDir.DirectoryName, DbFileInfo.DbTypeUserRepository, "_UserRepository");
				SetupTestDatabaseFile(dbFileCollection, testDbName + "_EDW", tempDir.DirectoryName, DbFileInfo.DbTypeEdwDB, "_EDW");
				SetupTestDatabaseFile(dbFileCollection, testDbName + "_SD001", tempDir.DirectoryName, DbFileInfo.DbTypeEDocs, "_SD001");
				SetupTestDatabaseFile(dbFileCollection, testDbName + "_SD002", tempDir.DirectoryName, DbFileInfo.DbTypeEDocs, "_SD002");
				SetupTestDatabaseFile(dbFileCollection, "CW1Bkp_T-20230630-114910_S-SYDCO-LMZT-1_D-CW-RefDatabase", tempDir.DirectoryName, DbFileInfo.DbTypeSingleSharedRefDB, "CW-RefDatabase");

				dbFileCollection.SetDbFileTypeVisible(DbFileInfo.DbTypeAuditDB, false);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = new AuditDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var edwServerConfig = new EdwDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.CopyProdToTest, addDbToAvailabilityGroup: false, availabilityGroup: "testgroup");

				//Act
				restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

				//Assert
				restoreManagerMock.Verify(m => m.RemoveDbFromAvailabilityGroupPrompt(), Times.Once());
				Assert("Confirmation prompt 'RemoveDbFromAvailabilityGroup' was not called.", calledConfirmRemoveDbFromAvailabilityGroup);
				restoreManagerMock.Verify(m => m.DropDbFromOtherReplicasInAvailabilityGroup(It.IsAny<List<DatabaseDetails>>(), It.IsAny<List<string>>()), Times.Once());

				Assert("Main Database Removed", infoMessages.Contains($"Removing {testDbName} database from availability group"));
				Assert("EDW Database Removed", infoMessages.Contains($"Removing {testDbName}_EDW database from availability group"));
				Assert("User Repository Database Removed", infoMessages.Contains($"Removing {testDbName}_UserRepository database from availability group"));
				Assert("eDocs Database 1 Removed", infoMessages.Contains($"Removing {testDbName}_SD001 database from availability group"));
				Assert("eDocs Database 2 Removed", infoMessages.Contains($"Removing {testDbName}_SD002 database from availability group"));
				Assert("Shared Reference Database not Removed", !infoMessages.Contains($"Removing {testDbName}CW-RefDatabase database from availability group"));

				Assert("Main Database not Rejoined", !infoMessages.Contains($"Joining database {testDbName} on primary replica {Db.ServerName} to availability group"));
				Assert("EDW Database not Rejoined", !infoMessages.Contains($"Joining database {testDbName}_EDW on primary replica {Db.ServerName} to availability group"));
				Assert("User Repository Database not Rejoined", !infoMessages.Contains($"Joining database {testDbName}_UserRepository on primary replica {Db.ServerName} to availability group"));
				Assert("eDocs Database 1 not Rejoined", !infoMessages.Contains($"Joining database {testDbName}_SD001 on primary replica {Db.ServerName} to availability group"));
				Assert("eDocs Database 2 not Rejoined", !infoMessages.Contains($"Joining database {testDbName}_SD002 on primary replica {Db.ServerName} to availability group"));
				Assert("Shared Reference Database not Rejoined", !infoMessages.Contains($"Joining database {testDbName}CW-RefDatabase on primary replica {Db.ServerName} to availability group"));
			}
		}

		public void TestRestore_DbNotInAlwaysOnAvailabilityGroupButExistsOnOtherReplicas_EnsureAllDatabasesAreRemovedFromGroup()
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);

			var alwaysOnHelperMock = GetAlwaysOnHelperMock();
			alwaysOnHelperMock.Setup(a => a.IsDbPartOfAlwaysOn(It.IsAny<AdminConnection>(), It.IsAny<string>())).Returns(false);
			alwaysOnHelperMock.Setup(m => m.GetReplicaServersWithDatabase(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new List<string> { "replica1", "replica2" });

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>(), Mock.Of<IDatabaseWaiter>());
			restoreManagerMock
				.Protected()
				.Setup("DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();

			restoreManagerMock.Setup(m => m.AuditSkipRestorePrompt()).Returns(true);
			restoreManagerMock.Setup(m => m.OverwriteDbInAvailabilityGroupPrompt()).Returns(true);
			restoreManagerMock.Setup(m => m.RemoveDbFromAvailabilityGroupPrompt()).CallBase();

			var calledConfirmRemoveDbFromAvailabilityGroup = false;
			restoreManagerMock.Object.OnConfirmationPrompt = (args) =>
			{
				var expectedMessage = "";
				if (args.PromptType == PromptType.ConfirmRemoveDbFromAvailabilityGroup)
				{
					expectedMessage = "Database will be removed from all nodes in the availability group. Are you sure?";
					calledConfirmRemoveDbFromAvailabilityGroup = true;
				}

				AssertEquals(expectedMessage, args.PromptMessage);
				args.Result = ConfirmationPromptResult.Yes;
			};

			restoreManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;

			var infoMessages = new List<string>();
			restoreManagerMock.Object.OnShowInfoMessage += message => infoMessages.Add(message);

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}))
			{
				var dbFileCollection = new DbFileInfoCollection();
				SetupTestDatabaseFile(dbFileCollection, testDbName, tempDir.DirectoryName, DbFileInfo.DbTypeMain, "");
				SetupTestDatabaseFile(dbFileCollection, testDbName + "_Audit", tempDir.DirectoryName, DbFileInfo.DbTypeUserRepository, "_UserRepository");
				SetupTestDatabaseFile(dbFileCollection, testDbName + "_EDW", tempDir.DirectoryName, DbFileInfo.DbTypeEdwDB, "_EDW");
				SetupTestDatabaseFile(dbFileCollection, testDbName + "_SD001", tempDir.DirectoryName, DbFileInfo.DbTypeEDocs, "_SD001");
				SetupTestDatabaseFile(dbFileCollection, testDbName + "_SD002", tempDir.DirectoryName, DbFileInfo.DbTypeEDocs, "_SD002");
				SetupTestDatabaseFile(dbFileCollection, "CW1Bkp_T-20230630-114910_S-SYDCO-LMZT-1_D-CW-RefDatabase", tempDir.DirectoryName, DbFileInfo.DbTypeSingleSharedRefDB, "CW-RefDatabase");

				dbFileCollection.SetDbFileTypeVisible(DbFileInfo.DbTypeAuditDB, false);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = new AuditDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var edwServerConfig = new EdwDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.CopyProdToTest, addDbToAvailabilityGroup: false, availabilityGroup: "testgroup");

				//Act
				restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

				//Assert
				restoreManagerMock.Verify(m => m.RemoveDbFromAvailabilityGroupPrompt(), Times.Once());
				Assert("Confirmation prompt 'RemoveDbFromAvailabilityGroup' was not called.", calledConfirmRemoveDbFromAvailabilityGroup);
				restoreManagerMock.Verify(m => m.DropDbFromOtherReplicasInAvailabilityGroup(It.IsAny<List<DatabaseDetails>>(), It.IsAny<List<string>>()), Times.Once());

				Assert("Main Database not Rejoined", !infoMessages.Contains($"Joining database {testDbName} on primary replica {Db.ServerName} to availability group"));
				Assert("EDW Database not Rejoined", !infoMessages.Contains($"Joining database {testDbName}_EDW on primary replica {Db.ServerName} to availability group"));
				Assert("User Repository Database not Rejoined", !infoMessages.Contains($"Joining database {testDbName}_UserRepository on primary replica {Db.ServerName} to availability group"));
				Assert("eDocs Database 1 not Rejoined", !infoMessages.Contains($"Joining database {testDbName}_SD001 on primary replica {Db.ServerName} to availability group"));
				Assert("eDocs Database 2 not Rejoined", !infoMessages.Contains($"Joining database {testDbName}_SD002 on primary replica {Db.ServerName} to availability group"));
				Assert("Shared Reference Database not Rejoined", !infoMessages.Contains($"Joining database {testDbName}CW-RefDatabase on primary replica {Db.ServerName} to availability group"));
			}
		}

		public void TestRestore_DbNotInAlwaysOnAvailabilityGroup_EnsureAllDatabasesAreAddedToAvailabilityGroup()
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);
			var alwaysOnHelperMock = GetAlwaysOnHelperMock();
			alwaysOnHelperMock.Setup(a => a.IsDbPartOfAlwaysOn(It.IsAny<AdminConnection>(), It.IsAny<string>())).Returns(false);

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>(), Mock.Of<IDatabaseWaiter>());
			restoreManagerMock
				.Protected()
				.Setup(
					"ExecuteRestoreWithNoRecovery",
					ItExpr.IsAny<DatabaseDetails>(),
					ItExpr.IsAny<DbRestoreSettings>()
				);
			restoreManagerMock
				.Protected()
				.Setup("DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();
			restoreManagerMock.Setup(m => m.AuditSkipRestorePrompt()).Returns(true);
			restoreManagerMock.Setup(m => m.OverwriteDbInAvailabilityGroupPrompt()).Returns(true);

			restoreManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;

			var infoMessages = new List<string>();
			restoreManagerMock.Object.OnShowInfoMessage += message => infoMessages.Add(message);

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}))
			{
				var dbFileCollection = new DbFileInfoCollection();
				SetupTestDatabaseFile(dbFileCollection, testDbName, tempDir.DirectoryName, DbFileInfo.DbTypeMain, "");
				SetupTestDatabaseFile(dbFileCollection, testDbName + "_Audit", tempDir.DirectoryName, DbFileInfo.DbTypeUserRepository, "_UserRepository");
				SetupTestDatabaseFile(dbFileCollection, testDbName + "_EDW", tempDir.DirectoryName, DbFileInfo.DbTypeEdwDB, "_EDW");
				SetupTestDatabaseFile(dbFileCollection, testDbName + "_SD001", tempDir.DirectoryName, DbFileInfo.DbTypeEDocs, "_SD001");
				SetupTestDatabaseFile(dbFileCollection, testDbName + "_SD002", tempDir.DirectoryName, DbFileInfo.DbTypeEDocs, "_SD002");
				SetupTestDatabaseFile(dbFileCollection, "CW1Bkp_T-20230630-114910_S-SYDCO-LMZT-1_D-CW-RefDatabase", tempDir.DirectoryName, DbFileInfo.DbTypeSingleSharedRefDB, "CW-RefDatabase");

				dbFileCollection.SetDbFileTypeVisible(DbFileInfo.DbTypeAuditDB, false);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = new AuditDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var edwServerConfig = new EdwDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.CopyProdToTest, addDbToAvailabilityGroup: true, availabilityGroup: "testgroup");

				//Act
				restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

				//Assert
				Assert("Main Database Joined", infoMessages.Contains($"Joining database {testDbName} on primary replica {Db.ServerName} to availability group"));
				Assert("EDW Database Joined", infoMessages.Contains($"Joining database {testDbName}_EDW on primary replica {Db.ServerName} to availability group"));
				Assert("User Repository Database Joined", infoMessages.Contains($"Joining database {testDbName}_UserRepository on primary replica {Db.ServerName} to availability group"));
				Assert("eDocs Database 1 Joined", infoMessages.Contains($"Joining database {testDbName}_SD001 on primary replica {Db.ServerName} to availability group"));
				Assert("eDocs Database 2 Joined", infoMessages.Contains($"Joining database {testDbName}_SD002 on primary replica {Db.ServerName} to availability group"));
				Assert("Shared Reference Database not Joined", !infoMessages.Contains($"Joining database {testDbName}CW-RefDatabase on primary replica {Db.ServerName} to availability group"));
			}
		}

		public void TestRestore_DbInAlwaysOnAvailabilityGroup_RestoreFailedOnPrimary()
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);

			var alwaysOnHelperMock = GetAlwaysOnHelperMock();

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>(), Mock.Of<IDatabaseWaiter>());
			restoreManagerMock
				.Protected()
				.Setup(
					"RestorePrimaryReplica",
					ItExpr.IsAny<List<DatabaseDetails>>(),
					ItExpr.IsAny<DbRestoreSettings>()
				).Throws(new DbBackupAndRestoreException("Primary Restore Failed"));
			restoreManagerMock
				.Protected()
				.Setup(
					"DoRestoreWithNoRecovery",
					ItExpr.IsAny<DatabaseDetails>(),
					ItExpr.IsAny<DbRestoreSettings>(),
					ItExpr.IsAny<AdminConnection>()
				)
				.Verifiable();
			restoreManagerMock
				.Protected()
				.Setup(
					"DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();
			restoreManagerMock.Setup(m => m.AuditSkipRestorePrompt()).Returns(true);
			restoreManagerMock.Setup(m => m.OverwriteDbInAvailabilityGroupPrompt()).Returns(true);

			restoreManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}))
			{
				var dbFileCollection = GetSetupDbFileInfoCollection(testDbName, tempDir.DirectoryName);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.CopyProdToTest, addDbToAvailabilityGroup: true, availabilityGroup: "testGroup");

				//Act & Assert
				AssertExceptionThrown<DbBackupAndRestoreException>("Primary Restore Failed",
					() => restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings)
				);

				alwaysOnHelperMock.Verify(m => m.AddDatabaseToAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
				restoreManagerMock
					.Protected()
					.Verify(
						"RestorePrimaryReplica",
						Times.Once(),
						ItExpr.IsAny<List<DatabaseDetails>>(),
						ItExpr.IsAny<DbRestoreSettings>()
					);
			}
		}

		public void TestRestore_DbInAlwaysOnAvailabilityGroup_RestoreFailedOnSecondary()
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);

			var alwaysOnHelperMock = GetAlwaysOnHelperMock();

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>(), Mock.Of<IDatabaseWaiter>());
			restoreManagerMock
				.Protected()
				.Setup(
					"RestorePrimaryReplica",
					ItExpr.IsAny<List<DatabaseDetails>>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.Verifiable();
			restoreManagerMock
				.Protected()
				.Setup(
					"DoRestoreWithNoRecovery",
					ItExpr.IsAny<DatabaseDetails>(),
					ItExpr.IsAny<DbRestoreSettings>(),
					ItExpr.IsAny<AdminConnection>()
				)
				.Throws(new DbBackupAndRestoreException("Secondary Restore Failed"));
			restoreManagerMock
				.Protected()
				.Setup(
					"DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();

			restoreManagerMock.Setup(m => m.AuditSkipRestorePrompt()).Returns(true);
			restoreManagerMock.Setup(m => m.OverwriteDbInAvailabilityGroupPrompt()).Returns(true);

			restoreManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;

			var messages = new List<string>();

			restoreManagerMock.Object.OnShowInfoMessage += new InformationEvent((message) => messages.Add(message));

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}))
			{
				var dbFileCollection = GetSetupDbFileInfoCollection(testDbName, tempDir.DirectoryName);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.CopyProdToTest, addDbToAvailabilityGroup: true, availabilityGroup: "testGroup");

				//Act & Assert
				AssertNoExceptionThrown(
					() => restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings)
				);

				alwaysOnHelperMock.Verify(m => m.AddDatabaseToAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
				alwaysOnHelperMock.Verify(m => m.JoinSecondaryDatabaseToAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
				restoreManagerMock
					.Protected()
					.Verify(
						"RestorePrimaryReplica",
						Times.Once(),
						ItExpr.IsAny<List<DatabaseDetails>>(),
						ItExpr.IsAny<DbRestoreSettings>()
				);
				restoreManagerMock
					.Protected()
					.Verify(
						"DoRestoreWithNoRecovery",
						Times.Once(),
						ItExpr.IsAny<DatabaseDetails>(),
						ItExpr.IsAny<DbRestoreSettings>(),
						ItExpr.IsAny<AdminConnection>()
					);
			}

			AssertCollectionContains(messages, message => message.StartsWith($"Failed to restore database {testDbName} secondary replica:", StringComparison.OrdinalIgnoreCase));
		}

		public void TestRestoreWithoutOperationalDatabasesConfirmationPromptIsCalled()
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);

			var alwaysOnHelperMock = GetAlwaysOnHelperMock();

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>(), Mock.Of<IDatabaseWaiter>());
			restoreManagerMock
				.Protected()
				.Setup(
					"RestorePrimaryReplica",
					ItExpr.IsAny<List<DatabaseDetails>>(),
					ItExpr.IsAny<DbRestoreSettings>()
				);
			restoreManagerMock
				.Protected()
				.Setup(
					"DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();
			restoreManagerMock.Setup(m => m.AuditSkipRestorePrompt()).Returns(true);
			restoreManagerMock.Setup(m => m.OverwriteDbInAvailabilityGroupPrompt()).Returns(true);
			restoreManagerMock.Setup(m => m.RestoreOperationalDatabasesPrompt()).CallBase();

			restoreManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;

			var calledConfirmRestoreWithoutOperationalDatabases = false;
			restoreManagerMock.Object.OnConfirmationPrompt = (args) =>
			{
				string expectedMessage = "";
				if (args.PromptType == PromptType.ConfirmRestoreWithoutOperationalDatabases)
				{
					expectedMessage = "Operational Databases have been found. Restoring without them may cause errors. Dropping or updating the databases first is recommended. Are you sure you want to continue without dropping or updating the databases?";
					calledConfirmRestoreWithoutOperationalDatabases = true;
				}

				AssertEquals(expectedMessage, args.PromptMessage);
				args.Result = ConfirmationPromptResult.Yes;
			};

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}))
			{
				var dbFileCollection = GetSetupDbFileInfoCollection(testDbName, tempDir.DirectoryName);

				SetupTestDatabaseFile(dbFileCollection, testDbName + "_SD001", tempDir.DirectoryName, DbFileInfo.DbTypeEDocs, "_SD001");
				dbFileCollection.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, true);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.CopyProdToTest, false, addDbToAvailabilityGroup: true, availabilityGroup: "testgroup");

				//Act & Assert
				AssertNoExceptionThrown(() => restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings));

				restoreManagerMock.Verify(m => m.RestoreOperationalDatabasesPrompt(), Times.Once());
				Assert("Confirmation prompt 'RestoreOperationalDatabases' was not called.", calledConfirmRestoreWithoutOperationalDatabases);
			}
		}

		public void TestRestoreWithoutOperationalDatabasesConfirmationPromptIsCalled_DoNotContinue()
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);

			var alwaysOnHelperMock = GetAlwaysOnHelperMock();

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>(), Mock.Of<IDatabaseWaiter>());
			restoreManagerMock
				.Protected()
				.Setup(
					"RestorePrimaryReplica",
					ItExpr.IsAny<List<DatabaseDetails>>(),
					ItExpr.IsAny<DbRestoreSettings>()
				);
			restoreManagerMock
				.Protected()
				.Setup(
					"DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();

			restoreManagerMock.Setup(m => m.OverwriteDbInAvailabilityGroupPrompt()).Returns(true);
			restoreManagerMock.Setup(m => m.RestoreOperationalDatabasesPrompt()).CallBase();

			restoreManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;

			bool calledConfirmRestoreWithoutOperationalDatabases = false;
			restoreManagerMock.Object.OnConfirmationPrompt = (args) =>
			{
				string expectedMessage = "";
				if (args.PromptType == PromptType.ConfirmRestoreWithoutOperationalDatabases)
				{
					expectedMessage = "Operational Databases have been found. Restoring without them may cause errors. Dropping or updating the databases first is recommended. Are you sure you want to continue without dropping or updating the databases?";
					calledConfirmRestoreWithoutOperationalDatabases = true;
				}
				var promptMessage = args.PromptMessage;

				AssertEquals(expectedMessage, args.PromptMessage);
				args.Result = ConfirmationPromptResult.No;
			};

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}))
			{
				var dbFileCollection = GetSetupDbFileInfoCollection(testDbName, tempDir.DirectoryName);

				SetupTestDatabaseFile(dbFileCollection, testDbName + "_SD001", tempDir.DirectoryName, DbFileInfo.DbTypeEDocs, "_SD001");
				dbFileCollection.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, true);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.CopyProdToTest, false, addDbToAvailabilityGroup: true, availabilityGroup: "testgroup");

				//Act & Assert
				AssertExceptionThrown<DbBackupAndRestoreException>("Restore Without Operational Databases Cancelled",
				() => restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings));

				restoreManagerMock.Verify(m => m.RestoreOperationalDatabasesPrompt(), Times.Once());
				Assert("Confirmation prompt 'RestoreOperationalDatabases' was not called.", calledConfirmRestoreWithoutOperationalDatabases);
			}
		}

		public void TestOverwriteDbOnAvailabilityGroupNodesConfirmationPrompt_Accept()
		{
			// Arrange
			var testDbName = nameof(DatabaseRestoreTest);

			var alwaysOnHelperMock = GetAlwaysOnHelperMock();
			alwaysOnHelperMock.Setup(a => a.IsDbPartOfAlwaysOn(It.IsAny<AdminConnection>(), It.IsAny<string>())).Returns(false);
			alwaysOnHelperMock.Setup(m => m.GetReplicaServersWithDatabase(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new List<string> { "replica1", "replica2" });

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>(), Mock.Of<IDatabaseWaiter>());
			restoreManagerMock
				.Protected()
				.Setup(
					"DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();

			restoreManagerMock.Setup(m => m.AuditSkipRestorePrompt()).Returns(true);
			restoreManagerMock.Setup(m => m.RestoreOperationalDatabasesPrompt()).Returns(true);
			restoreManagerMock.Setup(m => m.OverwriteDbInAvailabilityGroupPrompt()).CallBase();

			restoreManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;

			var calledConfirmOverwriteDbInAvailabilityGroup = false;
			restoreManagerMock.Object.OnConfirmationPrompt = (args) =>
			{
				var expectedMessage = "";
				if (args.PromptType == PromptType.ConfirmOverwriteDbInAvailabilityGroup)
				{
					expectedMessage = "Database will be overwritten on all nodes in the availability group. Are you sure?";
					calledConfirmOverwriteDbInAvailabilityGroup = true;
				}

				AssertEquals(expectedMessage, args.PromptMessage);
				args.Result = ConfirmationPromptResult.Yes;
			};

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}))
			{
				var dbFileCollection = GetSetupDbFileInfoCollection(testDbName, tempDir.DirectoryName);

				SetupTestDatabaseFile(dbFileCollection, testDbName + "_SD001", tempDir.DirectoryName, DbFileInfo.DbTypeEDocs, "_SD001");
				dbFileCollection.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, true);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.CopyProdToTest, addDbToAvailabilityGroup: true, availabilityGroup: "testgroup");

				//Act & Assert
				AssertNoExceptionThrown(() => restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings));

				restoreManagerMock.Verify(m => m.OverwriteDbInAvailabilityGroupPrompt(), Times.Once());

				Assert("Confirmation prompt 'OverwriteDbInAvailabilityGroup' was not called.", calledConfirmOverwriteDbInAvailabilityGroup);
			}
		}

		public void TestOverwriteDbOnAvailabilityGroupNodesConfirmationPrompt_Reject()
		{
			// Arrange
			var testDbName = nameof(DatabaseRestoreTest);

			var alwaysOnHelperMock = GetAlwaysOnHelperMock();
			alwaysOnHelperMock.Setup(a => a.IsDbPartOfAlwaysOn(It.IsAny<AdminConnection>(), It.IsAny<string>())).Returns(false);
			alwaysOnHelperMock.Setup(m => m.GetReplicaServersWithDatabase(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new List<string> { "replica1", "replica2" });

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>(), Mock.Of<IDatabaseWaiter>());
			restoreManagerMock
				.Protected()
				.Setup(
					"DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();

			restoreManagerMock.Setup(m => m.AuditSkipRestorePrompt()).Returns(true);
			restoreManagerMock.Setup(m => m.RestoreOperationalDatabasesPrompt()).Returns(true);
			restoreManagerMock.Setup(m => m.OverwriteDbInAvailabilityGroupPrompt()).Returns(false);

			var messages = new List<string>();
			restoreManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;
			restoreManagerMock.Object.OnShowInfoMessage += new InformationEvent((message) => messages.Add(message));

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}))
			{
				var dbFileCollection = GetSetupDbFileInfoCollection(testDbName, tempDir.DirectoryName);

				SetupTestDatabaseFile(dbFileCollection, testDbName + "_SD001", tempDir.DirectoryName, DbFileInfo.DbTypeEDocs, "_SD001");
				dbFileCollection.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, true);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.CopyProdToTest, addDbToAvailabilityGroup: true, availabilityGroup: "testgroup");

				//Act & Assert
				AssertExceptionThrown<DbBackupAndRestoreException>("Restore to availability group - process cancelled.",
					() => restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings));

				restoreManagerMock.Verify(m => m.OverwriteDbInAvailabilityGroupPrompt(), Times.Once());
				AssertCollectionNotContains(messages, message => message.StartsWith($"Removing {testDbName} database from availability group", StringComparison.OrdinalIgnoreCase));
			}
		}

		public void TestRestore_CreatesLoopbackLinkedServerOnAllReplicas()
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);
			const string secondarySQLServerName = "secondaryServer";
			var callOrder = new List<string>();

			var alwaysOnHelperMock = GetAlwaysOnHelperMock();
			alwaysOnHelperMock
				.Setup(a => a.GetSecondaryReplicaNamesListForAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>()))
				.Returns(() => new List<string> { secondarySQLServerName });
			alwaysOnHelperMock
				.Setup(a => a.JoinSecondaryDatabaseToAvailabilityGroup(It.IsAny<AdminConnection>(), It.Is(testDbName, StringComparer.OrdinalIgnoreCase), It.IsAny<string>()))
				.Callback(() => callOrder.Add("JoinSecondaryDatabaseToAvailabilityGroup"))
				.Returns(true);

			var llsuMock = new Mock<ServerConfigurationUtils>();
			llsuMock
				.Setup(x => x.ConfigureServerFromDatabase(It.IsAny<AdminConnection>(), It.Is(testDbName, StringComparer.OrdinalIgnoreCase)))
				.Callback<AdminConnection, string>((connection, targetDatabaseName) =>
				{
					if (connection.ServerName == secondarySQLServerName)
					{
						callOrder.Add("ConfigureServerFromDatabaseOnSecondaryReplica");
					}
				});

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>(), Mock.Of<IDatabaseWaiter>());
			restoreManagerMock.Object.serverConfigurationUtils = llsuMock.Object;

			restoreManagerMock
				.Protected()
				.Setup(
					"ExecuteRestoreWithNoRecovery",
					ItExpr.IsAny<DatabaseDetails>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();

			restoreManagerMock
				.Protected()
				.Setup(
					"RestorePrimaryReplica",
					ItExpr.IsAny<List<DatabaseDetails>>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();

			restoreManagerMock
				.Protected()
				.Setup(
					"DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();

			restoreManagerMock
				.Protected()
				.Setup(
					"IsTargetDatabaseMainDb",
					ItExpr.IsAny<string>()
				)
				.CallBase();

			restoreManagerMock.Object.AlwaysOnHelper = alwaysOnHelperMock.Object;
			restoreManagerMock.Setup(m => m.OverwriteDbInAvailabilityGroupPrompt()).Returns(true);

			var messages = new List<string>();
			restoreManagerMock.Object.OnShowInfoMessage += new InformationEvent((message) => messages.Add(message));

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
					AdoTestUtils.DropDbIfExists(connection, $"{testDbName}_Audit");
					AdoTestUtils.DropDbIfExists(connection, $"{testDbName}_EDW");
				}
			}))
			{
				var dbFileCollection = GetSetupDbFileInfoCollection(testDbName, tempDir.DirectoryName);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = new AuditDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var edwServerConfig = new EdwDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.CopyProdToTest, addDbToAvailabilityGroup: true, availabilityGroup: "testGroup");

				//Act 
				restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

				//Assert
				llsuMock.Verify(x => x.ConfigureServerFromDatabase(It.Is<AdminConnection>(connection => connection.ServerName == Db.ServerName), It.Is(testDbName, StringComparer.OrdinalIgnoreCase)), Times.Once);
				llsuMock.Verify(x => x.ConfigureServerFromDatabase(It.Is<AdminConnection>(connection => connection.ServerName == secondarySQLServerName), It.Is(testDbName, StringComparer.OrdinalIgnoreCase)), Times.Once);
				llsuMock.Verify(x => x.ConfigureServerFromDatabase(It.IsAny<AdminConnection>(), It.Is($"{testDbName}_Audit", StringComparer.OrdinalIgnoreCase)), Times.Never);
				llsuMock.Verify(x => x.ConfigureServerFromDatabase(It.IsAny<AdminConnection>(), It.Is($"{testDbName}_EDW", StringComparer.OrdinalIgnoreCase)), Times.Never);

				AssertGreaterThan(callOrder.IndexOf("ConfigureServerFromDatabaseOnSecondaryReplica"), callOrder.IndexOf("JoinSecondaryDatabaseToAvailabilityGroup"));
			}
		}

		public void TestRestoreWithNoRecovery_DoNotCreateLoopbackLinkedServer()
		{
			// Arrange
			const string testDbName = nameof(DatabaseRestoreTest);

			var llsuMock = new Mock<ServerConfigurationUtils>();
			llsuMock.Setup(x => x.ConfigureServerFromDatabase(It.IsAny<AdminConnection>(), It.Is(testDbName, StringComparer.OrdinalIgnoreCase)));

			var restoreManagerMock = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>(), Mock.Of<IDatabaseWaiter>());
			restoreManagerMock.Object.serverConfigurationUtils = llsuMock.Object;

			restoreManagerMock
				.Protected()
				.Setup(
					"ExecuteRestoreWithNoRecovery",
					ItExpr.IsAny<DatabaseDetails>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();

			restoreManagerMock
				.Protected()
				.Setup(
					"DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.CallBase();

			restoreManagerMock
				.Protected()
				.Setup(
					"IsTargetDatabaseMainDb",
					ItExpr.IsAny<string>()
				)
				.CallBase();

			restoreManagerMock.Setup(m => m.OverwriteDbInAvailabilityGroupPrompt()).Returns(true);

			var messages = new List<string>();
			restoreManagerMock.Object.OnShowInfoMessage += new InformationEvent((message) => messages.Add(message));

			using (var tempDir = new TempDirectory())
			using (new DisposableAction(() =>
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
					AdoTestUtils.DropDbIfExists(connection, $"{testDbName}_Audit");
					AdoTestUtils.DropDbIfExists(connection, $"{testDbName}_EDW");
				}
			}))
			{
				var dbFileCollection = GetSetupDbFileInfoCollection(testDbName, tempDir.DirectoryName);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var auditServerConfig = new AuditDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var edwServerConfig = new EdwDbServerConfiguration(Db.ServerName, Path.Combine(tempDir.DirectoryName, $"{testDbName}.bak"));
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFileCollection, DbRestoreOption.RestoreWithNoRecovery, addDbToAvailabilityGroup: false, availabilityGroup: string.Empty);

				//Act 
				restoreManagerMock.Object.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

				//Assert
				llsuMock.Verify(x => x.ConfigureServerFromDatabase(It.IsAny<AdminConnection>(), It.IsAny<string>()), Times.Never);
			}
		}

		#region TestHelpers

		Mock<IAlwaysOnHelper> GetAlwaysOnHelperMock()
		{
			var alwaysOnHelperMock = new Mock<IAlwaysOnHelper>();
			alwaysOnHelperMock
				.Setup(a => a.IsDbPartOfAlwaysOn(It.IsAny<AdminConnection>(), It.IsAny<string>()))
				.Returns(true);
			alwaysOnHelperMock
				.Setup(a => a.IsPrimaryReplicaOnAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>()))
				.Returns(true);
			alwaysOnHelperMock
				.Setup(a => a.GetSecondaryReplicaNamesListForAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>()))
				.Returns(() => new List<string> { Db.ServerName });
			alwaysOnHelperMock
				.Setup(a => a.RemoveDatabaseFromAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(true);
			alwaysOnHelperMock
				.Setup(a => a.AddDatabaseToAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(true);
			alwaysOnHelperMock
				.Setup(a => a.GetAvailabilityGroupName(It.IsAny<AdminConnection>(), It.IsAny<string>()))
				.Returns("testGroup");
			alwaysOnHelperMock
				.Setup(a => a.JoinSecondaryDatabaseToAvailabilityGroup(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(true);
			alwaysOnHelperMock
				.Setup(a => a.GetReplicaServersWithDatabase(It.IsAny<AdminConnection>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(() => new List<string> { });

			return alwaysOnHelperMock;
		}

		DbFileInfoCollection GetSetupDbFileInfoCollection(string databaseName, string tmpDir)
		{
			var dbFileCollection = new DbFileInfoCollection();
			SetupTestDatabaseFile(dbFileCollection, databaseName, tmpDir, DbFileInfo.DbTypeMain, "");
			SetupTestDatabaseFile(dbFileCollection, databaseName + "_Audit", tmpDir, DbFileInfo.DbTypeAuditDB, "_Audit");
			SetupTestDatabaseFile(dbFileCollection, databaseName + "_EDW", tmpDir, DbFileInfo.DbTypeEdwDB, "_EDW");

			dbFileCollection.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
			dbFileCollection.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
			dbFileCollection.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

			return dbFileCollection;
		}

		void SetupTestDatabaseFile(DbFileInfoCollection collection, string databaseBackupName, string tmpDir, string dbType, string restoreDbName)
		{
			var dbFile = databaseBackupName + ".bak";
			collection.Add(new DbFileInfo($"TestProdDbCopy{restoreDbName}", tmpDir, DbFileInfo.FileTypeData, Path.Combine(tmpDir, dbFile), dbType, restoreDbName));
			collection.Add(new DbFileInfo($"TestProdDbCopy{restoreDbName}_log", tmpDir, DbFileInfo.FileTypeLog, Path.Combine(tmpDir, dbFile), dbType, restoreDbName));
			TestDataHelpers.CopyTestFileResource(tmpDir, dbFile);
		}

		bool OtherConnectionExists(int spid, string dbName)
		{
			var sql = $@"
FROM
	sys.dm_exec_sessions AS s
WHERE 1=1
	AND s.is_user_process = 1
	AND s.session_id = @spid
	AND s.database_id = DB_ID(N'{dbName}')
";
			using (DbConnection adminConnection = Db.NewAdminConnection())
			{
				return adminConnection.Exists(sql, cmd =>
				{
					cmd.AddParameter("@spid", SqlDbType.Int, spid);
				});
			}
		}

		#endregion
	}
}
