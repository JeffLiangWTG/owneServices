using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.IO;
using Enterprise.DbUpgrader.Resource.Version;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class BiDataWarehouseUpgradePreparationTest : BusinessIntelligenceUpgradePreparationTest
	{
		#region TestPerformNonTransactionalDatabaseSettings

		[UseSnapshotProtection]
		public void TestPerformNonTransactionalDatabaseSettings()
		{
			const string testDbName = "TestBiDatabase";
			var testEdwDbName = testDbName + Db.EdwDatabaseSuffix;

			using (var tempoDirectory = new TempDirectory())
			using (var connection = Db.NewAdminConnection())
			{
				// Drop test DB first to ensure a clean new one is created
				AdoTestUtils.DropDbIfExists(connection, testDbName);
				AdoTestUtils.DropDbIfExists(connection, testEdwDbName);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(connection, testDbName);

					AssertDbExists(connection, testDbName, true);
					AssertDbExists(connection, testEdwDbName, false);

					using (var anotherConnection = Db.NewAdminConnection(Db.ServerName, testDbName))
					{
						var upgPreparation = new BiDataWarehouseUpgradePreparationForTest(anotherConnection);
						upgPreparation.PerformNonTransactionalSettings(new DummyLoggerForTest());

						// Assert AUDIT and EDW databases were created
						AssertDbExists(connection, testDbName, true);
						AssertDbExists(connection, testEdwDbName, true);

						// Assert properties set as required
						AssertDbProperties(connection, testEdwDbName, expectedTrustworthy: true, expectedCompatibilityLevel: connection.ServerVersionNumber.CompatibilityLevel);
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
					AdoTestUtils.DropDbIfExists(connection, testEdwDbName);
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

		public void TestMustRecreateEdwDatabases()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var versionInfo = new UpgradeVersionChangeInfoForTest(new VersionLabel(2659, 0));
				var upgPreparation = new BiDataWarehouseUpgradePreparationForTest(connection, versionInfo);
				AssertEquals("Should recreate EDW database", true, upgPreparation.MustRecreateBiDatabase_Exposed);

				versionInfo = new UpgradeVersionChangeInfoForTest(new VersionLabel(3010, 0));
				upgPreparation = new BiDataWarehouseUpgradePreparationForTest(connection, versionInfo);
				AssertEquals("Should recreate EDW database", true, upgPreparation.MustRecreateBiDatabase_Exposed);

				versionInfo = new UpgradeVersionChangeInfoForTest(new VersionLabel(7340, 0));
				upgPreparation = new BiDataWarehouseUpgradePreparationForTest(connection, versionInfo);
				AssertEquals("Should recreate EDW database", true, upgPreparation.MustRecreateBiDatabase_Exposed);

				versionInfo = new UpgradeVersionChangeInfoForTest(new VersionLabel(7348, 0));
				upgPreparation = new BiDataWarehouseUpgradePreparationForTest(connection, versionInfo);
				AssertEquals("Should not recreate EDW database", false, upgPreparation.MustRecreateBiDatabase_Exposed);
			}
		}

		[UseSnapshotProtection]
		public override void TestCreateDatabaseIfNotExistsMultiUser()
		{
			var testDbName = "TestDb";
			var logger = new DummyLoggerForTest();
			var versionInfo = new UpgradeVersionChangeInfoForTest(new VersionLabel(2659, 0));
			using (var adminConnection = Db.NewAdminConnection())
			{
				var upgPreparation = new BiDataWarehouseUpgradePreparationForTest(adminConnection, versionInfo);
				AdoTestUtils.CreateDbIfNotExists(adminConnection, testDbName);
				BusinessIntelligenceUpgradePreparation.SetDatabaseIntoAccessMode(testDbName, BusinessIntelligenceUpgradePreparation.SingleUser);
				upgPreparation.CreateDatabaseIfNotExists(logger, testDbName, adminConnection);
				var userMode = BusinessIntelligenceUpgradePreparation.GetDatabaseUserAccessMode(testDbName);
				AssertEquals(BusinessIntelligenceUpgradePreparation.MultiUser, userMode);
			}
		}

		sealed class BiDataWarehouseUpgradePreparationForTest : BiDataWarehouseUpgradePreparation
		{
			public BiDataWarehouseUpgradePreparationForTest(AdminConnection connection)
				: this(connection, upgVersionInfo: null)
			{
			}

			public BiDataWarehouseUpgradePreparationForTest(AdminConnection connection, IVersionChangeInfo upgVersionInfo)
				: base(connection, connection, upgVersionInfo)
			{
				RunEdwEtl = true;
			}

			public void PerformNonTransactionalDatabaseSettings_Exposed(IUpgradeTaskWorkflowLogger logger)
			{
				base.PerformNonTransactionalDatabaseSettings(logger);
			}

			public bool RunEdwEtl { get; set; }

			public bool MustRecreateBiDatabase_Exposed => MustRecreateBiDatabase;
		}
	}
}
