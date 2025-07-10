using System;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.IO;
using Enterprise.DbUpgrader.Resource.Version;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class BiAuditUpgradePreparationTest : BusinessIntelligenceUpgradePreparationTest
	{
		#region TestPerformNonTransactionalDatabaseSettings

		[UseSnapshotProtection]
		public void TestPerformNonTransactionalDatabaseSettings()
		{
			const string testDbName = "TestBiDatabase";
			var testAuditDbName = testDbName + Db.AuditDatabaseSuffix;

			using (var tempoDirectory = new TempDirectory())
			using (var connection = Db.NewAdminConnection())
			{
				// Drop test DB first to ensure a clean new one is created
				AdoTestUtils.DropDbIfExists(connection, testDbName);
				AdoTestUtils.DropDbIfExists(connection, testAuditDbName);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(connection, testDbName);

					AssertDbExists(connection, testDbName, true);
					AssertDbExists(connection, testAuditDbName, false);

					using (var anotherConnection = Db.NewAdminConnection(Db.ServerName, testDbName))
					{
						var upgPreparation = new BiAuditUpgradePreparationForTest(anotherConnection);
						upgPreparation.PerformNonTransactionalSettings(new DummyLoggerForTest());

						// Assert AUDIT and EDW databases were created
						AssertDbExists(connection, testDbName, true);
						AssertDbExists(connection, testAuditDbName, true);

						// Assert properties set as required
						AssertDbProperties(connection, testAuditDbName, expectedTrustworthy: true, expectedCompatibilityLevel: connection.ServerVersionNumber.CompatibilityLevel);
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
					AdoTestUtils.DropDbIfExists(connection, testAuditDbName);
				}
			}
		}

		void AssertDbExists(DbConnection connection, string dbName, bool dbExists)
		{
			AssertEquals("database exists?", dbExists, connection.DatabaseExists(dbName));
		}

		void AssertDbProperties(DbConnection connection, string dbName, bool expectedTrustworthy, int expectedCompatibilityLevel)
		{
			AssertEquals("is_trustworthy_on", expectedTrustworthy, Convert.ToBoolean(GetDbPropertyValue(connection, "is_trustworthy_on", dbName)));
			AssertEquals("compatibility_level", expectedCompatibilityLevel, Convert.ToInt32(GetDbPropertyValue(connection, "compatibility_level", dbName)));
		}

		object GetDbPropertyValue(DbConnection connection, string ptyName, string dbName)
		{
			return connection.ExecuteScalar(string.Format("SELECT [{0}] FROM sys.databases WHERE name = '{1}'", ptyName, dbName));
		}

		#endregion

		[UseSnapshotProtection]
		public override void TestCreateDatabaseIfNotExistsMultiUser()
		{
			var testDbName = "TestDb";
			var logger = new DummyLoggerForTest();
			var versionInfo = new UpgradeVersionChangeInfoForTest(new VersionLabel(2659, 0));
			using (var adminConnection = Db.NewAdminConnection())
			{
				var upgPreparation = new BiAuditUpgradePreparationForTest(adminConnection, versionInfo);
				AdoTestUtils.CreateDbIfNotExists(adminConnection, testDbName);
				BusinessIntelligenceUpgradePreparation.SetDatabaseIntoAccessMode(testDbName, BusinessIntelligenceUpgradePreparation.SingleUser);
				upgPreparation.CreateDatabaseIfNotExists(logger, testDbName, adminConnection);
				var userMode = BusinessIntelligenceUpgradePreparation.GetDatabaseUserAccessMode(testDbName);
				AssertEquals(BusinessIntelligenceUpgradePreparation.MultiUser, userMode);
			}
		}

		public void TestMustRecreateAuditDatabases()
		{
			try
			{
				TestingState.IsRunningTests = false;
				using (var connection = Db.NewAdminConnection())
				{
					var versionInfo = new UpgradeVersionChangeInfoForTest(new VersionLabel(5829, 0));
					var upgPreparation = new BiAuditUpgradePreparationForTest(connection, versionInfo);
					if (TestingState.IsRunningOnDAT)
					{
						AssertEquals("Should recreate Audit database", true, upgPreparation.MustRecreateBiDatabase_Exposed);
					}
					else
					{
						AssertEquals("Should not recreate Audit database", false, upgPreparation.MustRecreateBiDatabase_Exposed);
					}

					versionInfo = new UpgradeVersionChangeInfoForTest(new VersionLabel(5831, 0));
					upgPreparation = new BiAuditUpgradePreparationForTest(connection, versionInfo);
					AssertEquals("Should not recreate Audit database", false, upgPreparation.MustRecreateBiDatabase_Exposed);
				}
			}
			finally
			{
				TestingState.IsRunningTests = true;
			}
		}

		[UseSnapshotProtection]
		public void TestBackUpAuditDatabase()
		{
			const string testDbName = "TestBiDatabase";
			var testAuditDbName = testDbName + Db.AuditDatabaseSuffix;

			using (var tempoDirectory = new TempDirectory())
			using (var connection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(connection, testDbName);
				AdoTestUtils.DropDbIfExists(connection, testAuditDbName);
				AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;

				try
				{
					AdoTestUtils.CreateDbIfNotExists(connection, testDbName);

					AssertDbExists(connection, testDbName, true);
					AssertDbExists(connection, testAuditDbName, false);

					using (var anotherConnection = Db.NewAdminConnection(Db.ServerName, testDbName))
					{
						var upgPreparation = new BiAuditUpgradePreparationForTest(anotherConnection);
						AssertNoExceptionThrown(() => upgPreparation.PerformNonTransactionalSettings(new DummyLoggerForTest()));

						AssertDbExists(connection, testDbName, true);
						AssertDbExists(connection, testAuditDbName, true);

						AssertDbProperties(connection, testAuditDbName, expectedTrustworthy: true, expectedCompatibilityLevel: connection.ServerVersionNumber.CompatibilityLevel);
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
					AdoTestUtils.DropDbIfExists(connection, testAuditDbName);
				}
			}
		}

		sealed class BiAuditUpgradePreparationForTest : BiAuditUpgradePreparation
		{
			public BiAuditUpgradePreparationForTest(AdminConnection connection)
				: this(connection, upgVersionInfo: null)
			{
			}

			public BiAuditUpgradePreparationForTest(AdminConnection connection, IVersionChangeInfo upgVersionInfo)
				: base(connection, connection, upgVersionInfo)
			{
			}

			public void PerformNonTransactionalDatabaseSettings_Exposed(IUpgradeTaskWorkflowLogger logger)
			{
				base.PerformNonTransactionalDatabaseSettings(logger);
			}

			public bool MustRecreateBiDatabase_Exposed
			{
				get
				{
					return MustRecreateBiDatabase;
				}
			}

			public override string AuditBackupFilePath => "BackupFilePath";
		}
	}
}
