using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	sealed class StandardRefDbPreparationStrategyTest : TransactionedTestCase
	{
		public void TestRefDbName()
		{
			AssertEquals("Reference Database Enterprise US",
				Db.DatabaseName + "_" + RefDbTableNameResolver.RefDbAffix + "_Ent_US",
				new StandardRefDbPreparationStrategy(Db.DatabaseName, RefDbTypeEnum.Enterprise, "US", TestConnection).RefDbName);
			AssertEquals("Reference Database Enterprise SG",
				"SomeMainDb_" + RefDbTableNameResolver.RefDbAffix + "_Ent_SG",
				new StandardRefDbPreparationStrategy("SomeMainDb", RefDbTypeEnum.Enterprise, "SG", TestConnection).RefDbName);
			AssertEquals("Reference Database Tariff NZ",
				"AnotherMainDb_" + RefDbTableNameResolver.RefDbAffix + "_Trf_NZ",
				new StandardRefDbPreparationStrategy("AnotherMainDb", RefDbTypeEnum.Tariff, "NZ", TestConnection).RefDbName);
		}

		public void TestGetVersionFromDatabase()
		{
			IRefDbPreparationStrategy testStrategy = new StandardRefDbPreparationStrategyOnMainDbForTesting(TestConnection);

			string sqlText = String.Format(@"
				CREATE TABLE {0} (SV_Version int);
				INSERT {0} VALUES (9876);",
				ReferenceDbUpgrader.VersionPropertyName);
			TestConnection.ExecuteNonQuery(sqlText);

			string extendedPtyValue = DataUtils.LoadDbExtendedProperty(TestConnection, ReferenceDbUpgrader.VersionPropertyName, testStrategy.RefDbName);
			AssertNull("RefDbVersion property", extendedPtyValue);

			int version = testStrategy.GetVersionFromDatabase();
			AssertEquals("Reference database version", 9876, version);

			extendedPtyValue = DataUtils.LoadDbExtendedProperty(TestConnection, ReferenceDbUpgrader.VersionPropertyName, testStrategy.RefDbName);
			AssertEquals("RefDbVersion property", "9876", extendedPtyValue);

			DataUtils.SaveDbExtendedProperty(TestConnection, ReferenceDbUpgrader.VersionPropertyName, "345", testStrategy.RefDbName);
			version = testStrategy.GetVersionFromDatabase();
			AssertEquals("Reference database version", 345, version);
		}

		public void TestPrepareAndUpgradeDatabase()
		{
			IRefDbPreparationStrategy testStrategy = new StandardRefDbPreparationStrategyOnMainDbForTesting(TestConnection);

			string sqlText = String.Format("CREATE TABLE {0} (SV_Version int)", ReferenceDbUpgrader.VersionPropertyName);
			TestConnection.ExecuteNonQuery(sqlText);

			AssertEquals("Does legacy version table exist?", true, DbObjectCreator.TableExists(TestConnection, ReferenceDbUpgrader.VersionPropertyName));

			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.SqlMasterDb))
			{
				string actualUpgDb = null;
				testStrategy.PrepareAndUpgradeDatabase(c => { actualUpgDb = c.CurrentDatabase; });

				AssertEquals("TestConnection.CurrentDatabase", Db.SqlMasterDb, TestConnection.CurrentDatabase);
				AssertEquals("CurrentDatabase changed to reference db during upgrade", Db.DatabaseName, actualUpgDb);
			}

			AssertEquals("Does legacy version table exist?", false, DbObjectCreator.TableExists(TestConnection, ReferenceDbUpgrader.VersionPropertyName));
		}

		public void TestCreateReferenceDatabaseWithDefaultLogSizeAndFileGrowth()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				IRefDbPreparationStrategy testStrategy = new RefDbPreparationStrategyForTesting(Db.DatabaseName, RefDbTypeEnum.Enterprise, "ZZ", adminConnection);

				try
				{
					AdoTestUtils.DropDbIfExists(adminConnection, testStrategy.RefDbName);

					Assert("New shared database should not yet exist.", !adminConnection.DatabaseExists(testStrategy.RefDbName));
					((RefDbPreparationStrategyForTesting)testStrategy).DoCreateDatabase_Exposed(adminConnection);
					Assert("New shared database should now exist. Ensure it has been created in CreateDatabase().", adminConnection.DatabaseExists(testStrategy.RefDbName));

					string sqlText = String.Format(@"
						SELECT
							size = CEILING(size / 128.0),
							growth = CEILING(growth / 128.0)
						FROM sys.master_files
						WHERE
								 database_id = DB_ID('{0}')
								 AND type_desc = 'LOG';",
						testStrategy.RefDbName);

					using (var command = adminConnection.Command(sqlText))
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							AssertEquals("Log file should have SIZE = 100MB. Ensure it is affected by the above query, and determined correctly in CreateDatabase().",
								100, Convert.ToInt32(reader["size"]));
							AssertEquals("Log file should have GROWTH = 100MB. Ensure it is affected by the above query, and determined correctly in CreateDatabase().",
								100, Convert.ToInt32(reader["growth"]));
						}
						else
						{
							Fail("Log file details are unavailable. Check if the database name is correct");
						}
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(adminConnection, testStrategy.RefDbName);
					DbCommitTracker.Ignore(testStrategy.RefDbName);
				}
			}
		}

		class StandardRefDbPreparationStrategyOnMainDbForTesting : StandardRefDbPreparationStrategy
		{
			public StandardRefDbPreparationStrategyOnMainDbForTesting(DbConnection upgradeConnection)
				: base(Db.DatabaseName, RefDbTypeEnum.Enterprise, null, upgradeConnection)
			{
			}

			protected override string GetLatestVersionDatabaseName()
			{
				return Db.DatabaseName;
			}
		}
	}
}
