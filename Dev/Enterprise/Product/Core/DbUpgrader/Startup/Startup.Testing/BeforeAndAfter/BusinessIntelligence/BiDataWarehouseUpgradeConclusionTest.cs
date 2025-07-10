using System;
using System.IO;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.IO;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class BiDataWarehouseUpgradeConclusionTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestBackupBiDatabase()
		{
			using (var tempDirectory = new TempDirectory())
			using (var connection = Db.NewAdminConnection())
			{
				var backupFile = Path.Combine(tempDirectory, Db.EdwDatabaseName + ".bak");
				Env.Registry.BackupDirectoryPath = tempDirectory.DirectoryName;
				DataUtils.AddDbExtendedProperty(connection, "ShouldCreateBackup", "True", Db.EdwDatabaseName);

				AssertEquals("Backup file exists?", false, File.Exists(backupFile));

				var upgradeConclusion = new BiDataWarehouseUpgradeConclusionForTest(connection, connection);
				var logger = new DummyLoggerForTest();
				upgradeConclusion.BackupBiDatabase_Exposed(logger);

				AssertEquals("Backup file exists?", true, File.Exists(backupFile));
			}
		}

		public void TestArgumentNullExceptionWithNullBiConnection()
		{
			using (var testConnection = Db.NewAdminConnection(Db.DatabaseName))
			{
				AssertExceptionThrown("Expected ArgumentNullException",
					typeof(ArgumentNullException),
#if NETFRAMEWORK
				"Value cannot be null.\r\nParameter name: biConnection",
#else
				"Value cannot be null. (Parameter 'biConnection')",
#endif
				() =>
				{
					new BiDataWarehouseUpgradeConclusion(testConnection, null);
				});
			}
		}

		public void TestCleanupBiDatabasesWithNonExistentDatabase()
		{
			var logger = new DummyLoggerForTest();
			using (var testConnection = Db.NewAdminConnection(Db.DatabaseName))
			{
				var biUpgradeConclusion = new BiDataWarehouseUpgradeConclusionForTest(testConnection, testConnection);
				AssertNoExceptionThrown(() =>
				{
					biUpgradeConclusion.DropDatabaseIfExistsAndIsEmpty_Exposed(logger, testConnection, "NonExistentDbForCleanupDbTest_69E1BFFE133B4FCC93619C589B6E47A2");
				});
			}

			AssertEquals("Logger has any logs?", logger.Logs.Count, 0);
		}

		[UseSnapshotProtection]
		public void TestEnsureLoginsMappedToBiDatabaseWhenBiDatabasesAreInMainServer()
		{
			using (var mainDbConnection = Db.NewAdminConnection(Db.DatabaseName))
			using (var biConnection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var userName = "Odyssey_UnrestrictedWriterLogin";
				biConnection.ExecuteNonQuery($@"DROP USER IF EXISTS [{userName}]");
				var biUpgradeConclusion = new BiDataWarehouseUpgradeConclusionForTest(mainDbConnection, biConnection);
				biUpgradeConclusion.EnsureLoginsMappedToBiDatabases_Exposed();

				Assert("Odyssey_UnrestrictedWriterLogin should not be created",
					!Convert.ToBoolean(biConnection.ExecuteScalar($@"IF EXISTS (SELECT NULL FROM sys.database_principals WHERE name = '{userName}') SELECT 1 ELSE SELECT 0")));
			}
		}

		sealed class BiDataWarehouseUpgradeConclusionForTest : BiDataWarehouseUpgradeConclusion
		{
			public BiDataWarehouseUpgradeConclusionForTest(AdminConnection connection, AdminConnection biConnection)
				: base(connection, biConnection)
			{
			}

			public void EnsureLoginsMappedToBiDatabases_Exposed()
			{
				base.EnsureLoginsMappedToBiDatabase();
			}

			public void BackupBiDatabase_Exposed(IUpgradeTaskWorkflowLogger logger)
			{
				base.BackupBiDatabase(logger);
			}

			public void DropDatabaseIfExistsAndIsEmpty_Exposed(IUpgradeTaskWorkflowLogger logger, DbConnection connection, string dbName)
			{
				base.DropDatabaseIfExistsAndIsEmpty(logger, connection, dbName);
			}
		}
	}
}
