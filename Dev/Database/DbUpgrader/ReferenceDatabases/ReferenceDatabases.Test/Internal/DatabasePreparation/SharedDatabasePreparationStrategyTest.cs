using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	sealed class SharedDatabasePreparationStrategyTest : TestCase
	{
		public void TestRefDbName()
		{
			AssertEquals("Reference Database Enterprise US",
				"CW-RefDb-Ent-US-000023",
				new SharedDatabasePreparationStrategy(upgradeContext, "WhateverMainDbName", RefDbTypeEnum.Enterprise, "US", TestConnection, 23).RefDbName);
			AssertEquals("Reference Database Enterprise US",
				"CW-RefDb-Ent-US-1234567",
				new SharedDatabasePreparationStrategy(upgradeContext, "NoMatterWhatMainDbName", RefDbTypeEnum.Enterprise, "US", TestConnection, 1234567).RefDbName);
			AssertEquals("Reference Database Enterprise SG",
				"CW-RefDb-Ent-SG-098765",
				new SharedDatabasePreparationStrategy(upgradeContext, "", RefDbTypeEnum.Enterprise, "SG", TestConnection, 98765).RefDbName);
			AssertEquals("Reference Database Tariff NZ",
				"CW-RefDb-Trf-NZ-000000",
				new SharedDatabasePreparationStrategy(upgradeContext, null, RefDbTypeEnum.Tariff, "NZ", TestConnection, 0).RefDbName);
		}

		[UseSnapshotProtection]
		public void TestGetVersionFromDatabase()
		{
			var testStrategy = new SharedDatabasePreparationStrategyWithMockMainDbForTesting(upgradeContext, TestConnection, 902);

			try
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testStrategy.TestMockExclusiveRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testStrategy.RefDbName, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, testStrategy.TestMockExclusiveRefDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, testStrategy.RefDbName, Db.DatabaseName);
				}

				// Create legacy version table
				string sqlText = String.Format(@"
					CREATE TABLE {0} (SV_Version int);
					INSERT {0} VALUES (-23);",
					ReferenceDbUpgrader.VersionPropertyName);
				UpgCommandRunner.RunCommandOnGivenDb(TestConnection, testStrategy.TestMockExclusiveRefDb, sqlText);

				// No shared db of this type with version < latest => BaseDatabaseForUpgrade == exclusive ref db
				AssertEquals("BaseDatabaseForUpgrade", testStrategy.TestMockExclusiveRefDb, testStrategy.BaseDatabaseForUpgrade_Exposed);

				// No synonyms pointing to version shared DB and no extended property initially => gets version from version table in the exclusive DB
				int version = ((IRefDbPreparationStrategy)testStrategy).GetVersionFromDatabase();
				AssertEquals("Reference database version", -23, version);
				string extendedPtyValue = DataUtils.LoadDbExtendedProperty(TestConnection, ReferenceDbUpgrader.VersionPropertyName, testStrategy.TestMockExclusiveRefDb);
				AssertEquals("RefDbVersion property", "-23", extendedPtyValue);

				// No synonyms to version shared DB, but extended property created in the exclusive DB => gets version from extended pty in the exclusive DB
				DataUtils.SaveDbExtendedProperty(TestConnection, ReferenceDbUpgrader.VersionPropertyName, "96", testStrategy.TestMockExclusiveRefDb);
				version = ((IRefDbPreparationStrategy)testStrategy).GetVersionFromDatabase();
				AssertEquals("Reference database version", 96, version);

				// Create synonym on main mock database [RefDbCmrZZ_SomeTable] referencing new shared database
				sqlText = String.Format("CREATE SYNONYM [RefDbCmrZZ_SomeTable] FOR [{0}]..[SomeTable];", testStrategy.RefDbName);
				UpgCommandRunner.RunCommandOnGivenDb(TestConnection, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, sqlText);

				// Synonyms to version shared DB created, but its extended property is not set => gets version from extended pty in the exclusive DB
				version = ((IRefDbPreparationStrategy)testStrategy).GetVersionFromDatabase();
				AssertEquals("Reference database version", 96, version);

				// Synonyms to version shared DB created, and its extended property set => gets version from extended pty in the synonym base database (shared DB)
				DataUtils.SaveDbExtendedProperty(TestConnection, ReferenceDbUpgrader.VersionPropertyName, "902", testStrategy.TestMockExclusiveRefDb);
				version = ((IRefDbPreparationStrategy)testStrategy).GetVersionFromDatabase();
				AssertEquals("Reference database version", 902, version);
			}
			finally
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testStrategy.TestMockExclusiveRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testStrategy.RefDbName, Db.DatabaseName);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestGetVersionFromDatabaseWithCloseToLatestVersionDb()
		{
			var testStrategy = new SharedDatabasePreparationStrategyWithMockMainDbForTesting(upgradeContext, TestConnection, 902);
			string closeToLatestVersionSharedDb = testStrategy.VersionSharedRefDbPrefix_Exposed + "900";

			try
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testStrategy.TestMockExclusiveRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testStrategy.RefDbName, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, closeToLatestVersionSharedDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, testStrategy.TestMockExclusiveRefDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, closeToLatestVersionSharedDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, testStrategy.RefDbName, Db.DatabaseName);
				}

				using (((ICurrentDbControl)TestConnection).UseDatabase(SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb))
				{
					// Ensure close-to-latest version shared ref db version matches its name.
					DataUtils.SaveDbExtendedProperty(TestConnection, ReferenceDbUpgrader.VersionPropertyName, "900", closeToLatestVersionSharedDb);
					AssertEquals("BaseDatabaseForUpgrade", closeToLatestVersionSharedDb, testStrategy.BaseDatabaseForUpgrade_Exposed);

					// No RefDb synonyms + Exclusive db version = latest => Exclusive db version used.
					DataUtils.SaveDbExtendedProperty(TestConnection, ReferenceDbUpgrader.VersionPropertyName, "902", testStrategy.TestMockExclusiveRefDb);
					testStrategy.RefreshCurrentDatabaseNameForTesting();
					int version = ((IRefDbPreparationStrategy)testStrategy).GetVersionFromDatabase();
					AssertEquals("Reference database version", 902, version);

					// No RefDb synonyms, but exclusive db version != latest
					// => Use close-to-latest version shared db.
					DataUtils.SaveDbExtendedProperty(TestConnection, ReferenceDbUpgrader.VersionPropertyName, "300", testStrategy.TestMockExclusiveRefDb);
					testStrategy.RefreshCurrentDatabaseNameForTesting();
					version = ((IRefDbPreparationStrategy)testStrategy).GetVersionFromDatabase();
					AssertEquals("Reference database version", 900, version);

					// Synonyms to latest-version shared DB created, but its extended property is not set
					// => gets version extended pty from close-to-latest version shared DB
					string sqlText = String.Format("CREATE SYNONYM [RefDbCmrZZ_SomeTable] FOR [{0}]..[SomeTable];", testStrategy.RefDbName);
					UpgCommandRunner.RunCommandOnGivenDb(TestConnection, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, sqlText);
					testStrategy.RefreshCurrentDatabaseNameForTesting();
					version = ((IRefDbPreparationStrategy)testStrategy).GetVersionFromDatabase();
					AssertEquals("Reference database version", 900, version);

					// Synonyms point to version latest-version DB, and extended property is set
					// => gets version from extended pty in the synonym base database (latest-version shared DB)
					DataUtils.SaveDbExtendedProperty(TestConnection, ReferenceDbUpgrader.VersionPropertyName, "902", testStrategy.RefDbName);
					testStrategy.RefreshCurrentDatabaseNameForTesting();
					version = ((IRefDbPreparationStrategy)testStrategy).GetVersionFromDatabase();
					AssertEquals("Reference database version", 902, version);
				}
			}
			finally
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testStrategy.TestMockExclusiveRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testStrategy.RefDbName, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, closeToLatestVersionSharedDb, Db.DatabaseName);
				}
			}
		}

		public void TestPrepareAndUpgradeDatabase()
		{
			const string testMockOldSharedRefDb = "CW-RefDb-Cmr-ZZ-000901";
			const string testMockNewSharedRefDb = "CW-RefDb-Cmr-ZZ-000902";

			try
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockOldSharedRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockNewSharedRefDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, testMockOldSharedRefDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, testMockNewSharedRefDb, Db.DatabaseName);
				}

				using (var upgConnection = Db.NewAdminConnection(SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb))
				{
					var testStrategy = new SharedDatabasePreparationStrategyWithMockMainDbForTesting(upgradeContext, upgConnection, 902);
					testStrategy.OnSnapshotCreated += c =>
					{
						UpgCommandRunner.RunCommandOnGivenDb(c, testStrategy.BaseDatabaseForUpgrade_Exposed, @"INSERT Tab2 VALUES (24, null);");
					};
					AssertEquals("RefDbName", testMockNewSharedRefDb, testStrategy.RefDbName);

					string sqlText = @"
						CREATE TABLE Tab1 (Col11 int not null, Col12 char(1), PRIMARY KEY (Col11));
						INSERT Tab1 VALUES (1, 'A'), (2, 'B');
						CREATE TABLE Tab2 (Col21 int not null, Col22 int, PRIMARY KEY (Col21));
						ALTER TABLE Tab2 ADD CONSTRAINT FK_Tab2_Tab1 FOREIGN KEY (Col22) REFERENCES Tab1 (Col11);
						INSERT Tab2 VALUES (21, 1), (22, null), (23, 2);
						ALTER TABLE Tab2 WITH CHECK ADD CONSTRAINT CH_Tab2_Col21 CHECK (Col21 > 0);
						EXEC sys.sp_addextendedproperty @name = 'Pty01', @value = 'SourceOne';
						EXEC sys.sp_addextendedproperty @name = 'Pty02', @value = 'SourceTwo';
						EXEC sys.sp_addextendedproperty @name = 'Pty03', @value = 'SourceThree', @level0type = N'SCHEMA', @level0name = 'dbo';";
					UpgCommandRunner.RunCommandOnGivenDb(upgConnection, testMockOldSharedRefDb, sqlText);

					sqlText = @"
						CREATE TABLE Tab2 (Col20 bit);
						INSERT Tab2 VALUES (1);
						EXEC sys.sp_addextendedproperty @name = 'Pty02', @value = 'ExistingValue';";
					UpgCommandRunner.RunCommandOnGivenDb(upgConnection, testStrategy.RefDbName, sqlText);

					// Create synonym on main mock database [RefDbCmrZZ_Tab1] referencing the old shared database
					sqlText = String.Format("CREATE SYNONYM [RefDbCmrZZ_Tab1] FOR [{0}]..[Tab1];", testMockOldSharedRefDb);
					upgConnection.ExecuteNonQuery(sqlText);

					AssertEquals("Tab1 exists on old shared database?", true, DbObjectCreator.TableExists(upgConnection, testMockOldSharedRefDb, "Tab1"));
					AssertEquals("Tab2 exists on old shared database?", true, DbObjectCreator.TableExists(upgConnection, testMockOldSharedRefDb, "Tab2"));
					AssertEquals("Check constraint exists on old shared database?", true
						, upgConnection.Exists($"FROM [{testMockOldSharedRefDb}].sys.check_constraints WHERE parent_object_id = OBJECT_ID('[{testMockOldSharedRefDb}]..Tab2')"));
					AssertEquals("Tab1 exists on new shared database?", false, DbObjectCreator.TableExists(upgConnection, testStrategy.RefDbName, "Tab1"));
					AssertEquals("Tab2 exists on new shared database?", true, DbObjectCreator.TableExists(upgConnection, testStrategy.RefDbName, "Tab2"));
					AssertEquals("Check constraint exists on new shared database?", false
						, upgConnection.Exists($"FROM [{testStrategy.RefDbName}].sys.check_constraints WHERE parent_object_id = OBJECT_ID('[{testStrategy.RefDbName}]..Tab2')"));

					string actualUpgDb = null;
					((IRefDbPreparationStrategy)testStrategy).PrepareAndUpgradeDatabase(c => { actualUpgDb = c.CurrentDatabase; });

					// There is a shared db with version = latest-1 [CW-RefDb-Cmr-ZZ-000901] => BaseDatabaseForUpgrade == "CW-RefDb-Cmr-ZZ-000901"
					AssertEquals("BaseDatabaseForUpgrade", testMockOldSharedRefDb, testStrategy.BaseDatabaseForUpgrade_Exposed);

					AssertEquals("Upgrade runs on new reference db", testStrategy.RefDbName, actualUpgDb);
					// * Main app login mapping to old DB removed
					AssertEquals("Does new shared database exist?", true, upgConnection.DatabaseExists(testStrategy.RefDbName));

					using (((ICurrentDbControl)upgConnection).UseDatabase(testStrategy.RefDbName))
					{
						AssertEquals("Tab1 created on new shared database?", true, DbObjectCreator.TableExists(upgConnection, "Tab1"));
						AssertEquals("Tab2 created on new shared database?", true, DbObjectCreator.TableExists(upgConnection, "Tab2"));
						AssertEquals("Tab1 row count", 2, (int)upgConnection.ExecuteScalar("SELECT count(*) FROM Tab1"));
						AssertEquals("Tab2 row count", 3, (int)upgConnection.ExecuteScalar("SELECT count(*) FROM Tab2"));
						AssertEquals("Tab2 row 24 exists?", 0, upgConnection.ExecuteScalar<int>("SELECT count(*) FROM Tab2 WHERE Col21 = 24"));

						AssertEquals(
							"Tab1 PK created?", true,
							(int)upgConnection.ExecuteScalar("SELECT count(*) FROM sys.key_constraints WHERE type = 'PK' AND parent_object_id = object_id('Tab1')") == 1);
						AssertEquals(
							"Tab2 PK created?", true,
							(int)upgConnection.ExecuteScalar("SELECT count(*) FROM sys.key_constraints WHERE type = 'PK' AND parent_object_id = object_id('Tab2')") == 1);
						AssertEquals(
							"Tab2 FK created?", true,
							(int)upgConnection.ExecuteScalar("SELECT count(*) FROM sys.foreign_keys WHERE parent_object_id = object_id('Tab2')") == 1);

						AssertEquals("Check constraint exists on new shared database?", true
							, upgConnection.Exists($"FROM [{testStrategy.RefDbName}].sys.check_constraints WHERE parent_object_id = OBJECT_ID('[{testStrategy.RefDbName}]..Tab2')"));

						// Extended Properties: Pty01 created, Pty02 updated, Pty03 not copied (non DB level)
						AssertEquals("Pty01", "SourceOne", DataUtils.LoadDbExtendedProperty(upgConnection, "Pty01"));
						AssertEquals("Pty02", "SourceTwo", DataUtils.LoadDbExtendedProperty(upgConnection, "Pty02"));
						AssertNull("Pty03", DataUtils.LoadDbExtendedProperty(upgConnection, "Pty03"));
					}
				}
			}
			finally
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockOldSharedRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockNewSharedRefDb, Db.DatabaseName);
				}

				DbCommitTracker.Ignore(SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb);
				DbCommitTracker.Ignore(testMockOldSharedRefDb);
				DbCommitTracker.Ignore(testMockNewSharedRefDb);
			}
		}

		public void TestPrepareAndUpgradeDatabaseWithNonexistentPreviousDb()
		{
			const string testMockOldRefDb = SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb + "_RefDb_Cmr_ZZ";
			const string testMockNewSharedRefDb = "CW-RefDb-Cmr-ZZ-000001";

			try
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockOldRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockNewSharedRefDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, testMockNewSharedRefDb, Db.DatabaseName);
				}

				using (var upgConnection = Db.NewAdminConnection(SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb))
				{
					var testStrategy = new SharedDatabasePreparationStrategyWithMockMainDbForTesting(upgradeContext, upgConnection, 1);

					// Create synonym on main mock database [RefDbCmrZZ_Tab1] referencing the old ref database
					string sqlText = String.Format("CREATE SYNONYM [RefDbCmrZZ_Tab1] FOR [{0}]..[Tab1];", testMockOldRefDb);
					upgConnection.ExecuteNonQuery(sqlText);

					string actualUpgDb = null;
					((IRefDbPreparationStrategy)testStrategy).PrepareAndUpgradeDatabase(c => { actualUpgDb = c.CurrentDatabase; });

					// Candidate base database (testMockOldRefDb) does not exist => BaseDatabaseForUpgrade == ""
					AssertEquals("BaseDatabaseForUpgrade", "", testStrategy.BaseDatabaseForUpgrade_Exposed);

					AssertEquals("Upgrade runs on new reference db", testStrategy.RefDbName, actualUpgDb);
					AssertEquals("Does new shared database exist?", true, TestConnection.DatabaseExists(testStrategy.RefDbName));
				}
			}
			finally
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockNewSharedRefDb, Db.DatabaseName);
				}

				DbCommitTracker.Ignore(SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb);
				DbCommitTracker.Ignore(testMockNewSharedRefDb);
			}
		}

		public void TestPrepareAndUpgradeDatabaseTwoAtTheSameTime()
		{
			const string testMockOldRefDb = SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb + "_RefDb_Cmr_ZZ";
			const string testMockNewSharedRefDb = "CW-RefDb-Cmr-ZZ-000001";
			TimeSpan eventTimeout = TimeSpan.FromMinutes(1);

			try
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockOldRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockNewSharedRefDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
				}

				using (var upgConnection = Db.NewAdminConnection(SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb))
				{
					// Create synonym on main mock database [RefDbCmrZZ_Tab1] referencing the old ref database
					string sqlText = String.Format("CREATE SYNONYM [RefDbCmrZZ_Tab1] FOR [{0}]..[Tab1];", testMockOldRefDb);
					upgConnection.ExecuteNonQuery(sqlText);
				}

				using (var upgrade1ReadyEvent = new AutoResetEvent(false))
				using (var upgrade2ReadyEvent = new AutoResetEvent(false))
				{
					var task1 = Task.Run(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							upgrade1ReadyEvent.Set();
							Assert(upgrade2ReadyEvent.WaitOne(eventTimeout));
							AssertPrepareAndUpgradeDatabaseWithNonexistentPreviousDb();
						}
					});

					upgrade2ReadyEvent.Set();
					Assert(upgrade1ReadyEvent.WaitOne(eventTimeout));
					AssertPrepareAndUpgradeDatabaseWithNonexistentPreviousDb();

					Assert(task1.Wait(eventTimeout));
				}
			}
			finally
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockNewSharedRefDb, Db.DatabaseName);
				}

				DbCommitTracker.Ignore(SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb);
				DbCommitTracker.Ignore(testMockNewSharedRefDb);
			}
		}

		void AssertPrepareAndUpgradeDatabaseWithNonexistentPreviousDb()
		{
			using (var upgConnection = Db.NewAdminConnection(SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb))
			{
				var testStrategy = new SharedDatabasePreparationStrategyWithMockMainDbForTesting(upgradeContext, upgConnection, 1);

				string actualUpgDb = null;
				((IRefDbPreparationStrategy)testStrategy).PrepareAndUpgradeDatabase(c => { actualUpgDb = c.CurrentDatabase; });

				// Candidate base database (testMockOldRefDb) does not exist => BaseDatabaseForUpgrade == ""
				AssertEquals("BaseDatabaseForUpgrade", "", testStrategy.BaseDatabaseForUpgrade_Exposed);

				AssertEquals("Upgrade runs on new reference db", testStrategy.RefDbName, actualUpgDb);
				AssertEquals("Does new shared database exist?", true, TestConnection.DatabaseExists(testStrategy.RefDbName));
			}
		}

		public void TestPrepareAndUpgradeDatabaseWithPreviousNonSharedDb()
		{
			// upgrade should use previous shared DB (assert data in the new DB matches it)
			const string testMockPreviousRefDb = SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb + "_RefDb_Cmr_ZZ";
			const string testMockExistingSharedRefDb = "CW-RefDb-Cmr-ZZ-000901";
			const string testMockNewSharedRefDb = "CW-RefDb-Cmr-ZZ-000902";

			try
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockPreviousRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockExistingSharedRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockNewSharedRefDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, testMockPreviousRefDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, testMockExistingSharedRefDb, Db.DatabaseName);
				}

				using (var upgConnection = Db.NewAdminConnection(SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb))
				{
					var testStrategy = new SharedDatabasePreparationStrategyWithMockMainDbForTesting(upgradeContext, upgConnection, 902);
					AssertEquals("RefDbName", testMockNewSharedRefDb, testStrategy.RefDbName);

					// Set version extended property on reference databases
					DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "900", testMockPreviousRefDb);
					DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "901", testMockExistingSharedRefDb);

					// Add test data to previous reference database (non-shared)
					string sqlText = @"
						CREATE TABLE Tab1 (Col11 int not null, Col12 char(1), PRIMARY KEY (Col11));
						INSERT Tab1 VALUES (1, 'X');
						CREATE TABLE TabX2 (Col21 int not null, Col22 int, PRIMARY KEY (Col21));
						INSERT TabX2 VALUES (11, 1), (12, 2), (13, null);";
					UpgCommandRunner.RunCommandOnGivenDb(upgConnection, testMockPreviousRefDb, sqlText);

					// Add test data to existing shared reference database(different from current non-shared database)
					sqlText = @"
						CREATE TABLE Tab1 (Col11 int not null, Col12 char(1), PRIMARY KEY (Col11));
						INSERT Tab1 VALUES (1, 'A'), (2, 'B');
						CREATE TABLE TabS2 (Col21 int not null, Col22 int, PRIMARY KEY (Col21));
						INSERT TabS2 VALUES (21, 1);";
					UpgCommandRunner.RunCommandOnGivenDb(upgConnection, testMockExistingSharedRefDb, sqlText);

					// Create synonym on main mock database [RefDbCmrZZ_Tab1] referencing the current reference database
					sqlText = String.Format("CREATE SYNONYM [RefDbCmrZZ_Tab1] FOR [{0}]..[Tab1];", testMockPreviousRefDb);
					upgConnection.ExecuteNonQuery(sqlText);

					string actualUpgDb = null;
					((IRefDbPreparationStrategy)testStrategy).PrepareAndUpgradeDatabase(c => { actualUpgDb = c.CurrentDatabase; });

					// There is a shared db with version = latest-1 [CW-RefDb-Cmr-ZZ-000901] => BaseDatabaseForUpgrade == "CW-RefDb-Cmr-ZZ-000901"
					AssertEquals("BaseDatabaseForUpgrade", testMockExistingSharedRefDb, testStrategy.BaseDatabaseForUpgrade_Exposed);

					AssertEquals("Upgrade runs on new reference db", testStrategy.RefDbName, actualUpgDb);
					AssertEquals("Does new shared database exist?", true, upgConnection.DatabaseExists(testStrategy.RefDbName));

					using (((ICurrentDbControl)upgConnection).UseDatabase(testStrategy.RefDbName))
					{
						// Assert new reference database data
						// Existing shared database should be used as the base for the upgrade
						AssertEquals("Tab1 created on new shared database?", true, DbObjectCreator.TableExists(upgConnection, "Tab1"));
						AssertEquals("TabX2 created on new shared database?", false, DbObjectCreator.TableExists(upgConnection, "TabX2"));
						AssertEquals("TabS2 created on new shared database?", true, DbObjectCreator.TableExists(upgConnection, "TabS2"));
						AssertEquals("Tab1 row count", 2, (int)upgConnection.ExecuteScalar("SELECT count(*) FROM Tab1"));
						AssertEquals("Tab2 row count", 1, (int)upgConnection.ExecuteScalar("SELECT count(*) FROM TabS2"));
						AssertEquals("Tab1 row1.col12 value", "A", upgConnection.ExecuteScalar("SELECT Col12 FROM Tab1 WHERE Col11 = 1").ToString());
						AssertEquals("Tab1 row2.col12 value", "B", upgConnection.ExecuteScalar("SELECT Col12 FROM Tab1 WHERE Col11 = 2").ToString());
						AssertEquals("TabS2 row1.col22 value", 1, (int)upgConnection.ExecuteScalar("SELECT Col22 FROM TabS2 WHERE Col21 = 21"));

						AssertEquals(
							"Tab1 PK created?", true,
							(int)upgConnection.ExecuteScalar("SELECT count(*) FROM sys.key_constraints WHERE type = 'PK' AND parent_object_id = object_id('Tab1')") == 1);
						AssertEquals(
							"TabS2 PK created?", true,
							(int)upgConnection.ExecuteScalar("SELECT count(*) FROM sys.key_constraints WHERE type = 'PK' AND parent_object_id = object_id('TabS2')") == 1);
					}
				}
			}
			finally
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockPreviousRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockExistingSharedRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockNewSharedRefDb, Db.DatabaseName);
				}

				DbCommitTracker.Ignore(SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb);
				DbCommitTracker.Ignore(testMockPreviousRefDb);
				DbCommitTracker.Ignore(testMockExistingSharedRefDb);
				DbCommitTracker.Ignore(testMockNewSharedRefDb);
			}
		}

		public void TestPrepareAndUpgradeDatabase_Current_Latest()
		{
			var mainDb = "SharedRefDb";
			var currentRefDb = $"{mainDb}_RefDb_Cmr_ZZ";
			var sharedRefDb = "CW-RefDb-Cmr-ZZ-000111";
			var newSharedRefDb = "CW-RefDb-Cmr-ZZ-000333";

			using (AdoTestUtils.CreateDbDropExistingDisposable(mainDb, Db.DatabaseName))
			using (AdoTestUtils.CreateDbDropExistingDisposable(currentRefDb, Db.DatabaseName))
			using (AdoTestUtils.CreateDbDropExistingDisposable(sharedRefDb, Db.DatabaseName))
			using (AdoTestUtils.DropDbIfExistsDisposable(newSharedRefDb, Db.DatabaseName))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = new SharedDatabasePreparationStrategyWithMockMainDbForTesting(upgradeContext, upgConnection, mainDb, 000333);
				AssertEquals("RefDbName", newSharedRefDb, testStrategy.RefDbName);

				// Create synonym on main mock database [RefDbCmrZZ_Tab1] referencing the current reference database
				upgConnection.ExecuteNonQuery($"CREATE SYNONYM [RefDbCmrZZ_Tab1] FOR [{currentRefDb}]..[Tab1];");

				var dbVersion111 = sharedRefDb;
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "111", dbVersion111);
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, dbVersion111, "CREATE TABLE Tab_111 (Col11 int)");

				var dbVersion222 = currentRefDb;
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "222", dbVersion222);
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, dbVersion222, "CREATE TABLE Tab_222 (Col11 int)");

				string actualUpgDb = null;
				((IRefDbPreparationStrategy)testStrategy).PrepareAndUpgradeDatabase(c => { actualUpgDb = c.CurrentDatabase; });

				AssertEquals("BaseDatabaseForUpgrade", currentRefDb, testStrategy.BaseDatabaseForUpgrade_Exposed);

				AssertEquals("Upgrade runs on new reference db", newSharedRefDb, actualUpgDb);
				AssertEquals("Does new shared database exist?", true, upgConnection.DatabaseExists(newSharedRefDb));

				using (((ICurrentDbControl)upgConnection).UseDatabase(newSharedRefDb))
				{
					// Assert new reference database data
					AssertEquals("Tab_111 created on new shared database?", false, DbObjectCreator.TableExists(upgConnection, "Tab_111"));
					AssertEquals("Tab_222 created on new shared database?", true, DbObjectCreator.TableExists(upgConnection, "Tab_222"));
				}
			}
		}

		public void TestPrepareAndUpgradeDatabase_Current()
		{
			var mainDb = "SharedRefDb";
			var currentRefDb = "CW-RefDb-Cmr-ZZ-000222";
			var sharedRefDb = "CW-RefDb-Cmr-ZZ-000111";
			var newSharedRefDb = "CW-RefDb-Cmr-ZZ-000444";

			using (AdoTestUtils.CreateDbDropExistingDisposable(mainDb, Db.DatabaseName))
			using (AdoTestUtils.CreateDbDropExistingDisposable(currentRefDb, Db.DatabaseName))
			using (AdoTestUtils.CreateDbDropExistingDisposable(sharedRefDb, Db.DatabaseName))
			using (AdoTestUtils.DropDbIfExistsDisposable(newSharedRefDb, Db.DatabaseName))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = new SharedDatabasePreparationStrategyWithMockMainDbForTesting(upgradeContext, upgConnection, mainDb, 000444);
				AssertEquals("RefDbName", newSharedRefDb, testStrategy.RefDbName);

				// Create synonym on main mock database [RefDbCmrZZ_Tab1] referencing the current reference database
				upgConnection.ExecuteNonQuery($"CREATE SYNONYM [RefDbCmrZZ_Tab1] FOR [{currentRefDb}]..[Tab1];");

				var dbVersion111 = sharedRefDb;
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "111", dbVersion111);
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, dbVersion111, @"CREATE TABLE Tab_111 (Col11 int)");

				var dbVersion222 = currentRefDb;
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "222", dbVersion222);
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, dbVersion222, @"CREATE TABLE Tab_222 (Col11 int)");

				string actualUpgDb = null;
				((IRefDbPreparationStrategy)testStrategy).PrepareAndUpgradeDatabase(c => { actualUpgDb = c.CurrentDatabase; });

				AssertEquals("BaseDatabaseForUpgrade", currentRefDb, testStrategy.BaseDatabaseForUpgrade_Exposed);

				AssertEquals("Upgrade runs on new reference db", newSharedRefDb, actualUpgDb);
				AssertEquals("Does new shared database exist?", true, upgConnection.DatabaseExists(newSharedRefDb));

				using (((ICurrentDbControl)upgConnection).UseDatabase(newSharedRefDb))
				{
					// Assert new reference database data
					AssertEquals("Tab_111 created on new shared database?", false, DbObjectCreator.TableExists(upgConnection, "Tab_111"));
					AssertEquals("Tab_222 created on new shared database?", true, DbObjectCreator.TableExists(upgConnection, "Tab_222"));
				}
			}
		}

		public void TestPrepareAndUpgradeDatabase_Shared()
		{
			var mainDb = "SharedRefDb";
			var currentRefDb = "CW-RefDb-Cmr-ZZ-000111";
			var sharedRefDb = "CW-RefDb-Cmr-ZZ-000222";
			var newSharedRefDb = "CW-RefDb-Cmr-ZZ-000444";

			using (AdoTestUtils.CreateDbDropExistingDisposable(mainDb, Db.DatabaseName))
			using (AdoTestUtils.CreateDbDropExistingDisposable(currentRefDb, Db.DatabaseName))
			using (AdoTestUtils.CreateDbDropExistingDisposable(sharedRefDb, Db.DatabaseName))
			using (AdoTestUtils.DropDbIfExistsDisposable(newSharedRefDb, Db.DatabaseName))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = new SharedDatabasePreparationStrategyWithMockMainDbForTesting(upgradeContext, upgConnection, mainDb, 000444);
				AssertEquals("RefDbName", newSharedRefDb, testStrategy.RefDbName);

				// Create synonym on main mock database [RefDbCmrZZ_Tab1] referencing the current reference database
				upgConnection.ExecuteNonQuery($"CREATE SYNONYM [RefDbCmrZZ_Tab1] FOR [{currentRefDb}]..[Tab1];");

				var dbVersion111 = currentRefDb;
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "111", dbVersion111);
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, dbVersion111, @"CREATE TABLE Tab_111 (Col11 int)");

				var dbVersion222 = sharedRefDb;
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "222", dbVersion222);
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, dbVersion222, @"CREATE TABLE Tab_222 (Col11 int)");

				string actualUpgDb = null;
				((IRefDbPreparationStrategy)testStrategy).PrepareAndUpgradeDatabase(c => { actualUpgDb = c.CurrentDatabase; });

				AssertEquals("BaseDatabaseForUpgrade", sharedRefDb, testStrategy.BaseDatabaseForUpgrade_Exposed);

				AssertEquals("Upgrade runs on new reference db", newSharedRefDb, actualUpgDb);
				AssertEquals("Does new shared database exist?", true, upgConnection.DatabaseExists(newSharedRefDb));

				using (((ICurrentDbControl)upgConnection).UseDatabase(newSharedRefDb))
				{
					// Assert new reference database data
					AssertEquals("Tab_111 created on new shared database?", false, DbObjectCreator.TableExists(upgConnection, "Tab_111"));
					AssertEquals("Tab_222 created on new shared database?", true, DbObjectCreator.TableExists(upgConnection, "Tab_222"));
				}
			}
		}

		[UseSnapshotProtection]
		public void TestPrepareAndUpgradeDatabase_LegacyStmExtendedProperty()
		{
			const string testMockNewSharedRefDb = "CW-RefDb-Cmr-ZZ-000902";

			try
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					PrepareExtendedPropertiesBeforeGetPhysicalPath(adminConnection);

					AdoTestUtils.DropDbIfExists(adminConnection, testMockNewSharedRefDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, testMockNewSharedRefDb, Db.DatabaseName);

					IRefDbPreparationStrategy testStrategy = new SharedDatabasePreparationStrategyWithMockMainDbForTesting(upgradeContext, adminConnection, Db.DatabaseName, 902);
					AssertEquals("RefDbName", testMockNewSharedRefDb, testStrategy.RefDbName);

					var sql = @"
						CREATE TABLE [STMEXTENDEDPROPERTY] (
 [SEP_CLASS] VARCHAR(60) NOT NULL DEFAULT '',
 [SEP_DATABASENAMESUFFIX] VARCHAR(128) NOT NULL DEFAULT '',
 [SEP_SCHEMANAME] NVARCHAR(128) NOT NULL DEFAULT '',
 [SEP_MAJOROBJECTNAME] NVARCHAR(128) NOT NULL DEFAULT '',
 [SEP_MINOROBJECTNAME] NVARCHAR(128) NOT NULL DEFAULT '',
 [SEP_NAME] VARCHAR(200) NOT NULL DEFAULT '',
 [SEP_VALUE] VARCHAR(200) NOT NULL DEFAULT '',
);
CREATE UNIQUE CLUSTERED INDEX [NR_UC__SEP_CLASS_SEP_DATABASENAMESUFFIX_SEP_SCHEMANAME_SEP_MAJOROBJECTNAME_SEP_MINOROBJECTNAME_SEP_NAME] ON [STMEXTENDEDPROPERTY] ([SEP_CLASS] ASC,[SEP_DATABASENAMESUFFIX] ASC,[SEP_SCHEMANAME] ASC,[SEP_MAJOROBJECTNAME] ASC,[SEP_MINOROBJECTNAME] ASC,[SEP_NAME] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

ALTER TABLE [STMEXTENDEDPROPERTY]
 SET (LOCK_ESCALATION = DISABLE);";
					UpgCommandRunner.RunCommandOnGivenDb(adminConnection, testMockNewSharedRefDb, sql);

					string actualUpgradeDb = null;
					testStrategy.PrepareAndUpgradeDatabase(c => { actualUpgradeDb = c.CurrentDatabase; });
					AssertEquals("Upgrade should have been run on the new reference database.", testStrategy.RefDbName, actualUpgradeDb);
					AssertEquals("Table StmExtendedProperty should be upgraded the same definition as in main database", true, ExtProperty.IsTableDefinitionMatched(adminConnection, testStrategy.RefDbName));
				}
			}
			finally
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, testMockNewSharedRefDb, Db.DatabaseName);

					RestoreExtendedPropertiesAfterGetPhysicalPath(adminCnx);
				}

				DbCommitTracker.Ignore(testMockNewSharedRefDb);
			}
		}

		[UseSnapshotProtection]
		public void TestCreateDatabase()
		{
			const string testMockOldSharedRefDb = "CW-RefDb-Cmr-ZZ-000901";
			const string testMockNewSharedRefDb = "CW-RefDb-Cmr-ZZ-000902";

			try
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					PrepareExtendedPropertiesBeforeGetPhysicalPath(adminConnection);

					AdoTestUtils.DropDbIfExists(adminConnection, testMockOldSharedRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminConnection, testMockNewSharedRefDb, Db.DatabaseName);

					AdoTestUtils.CreateDbIfNotExists(adminConnection, testMockOldSharedRefDb, Db.DatabaseName);

					IRefDbPreparationStrategy testStrategy = new SharedDatabasePreparationStrategyWithMockMainDbForTesting(upgradeContext, adminConnection, Db.DatabaseName, 902);
					AssertEquals("RefDbName", testMockNewSharedRefDb, testStrategy.RefDbName);

					var sql = @"
						CREATE TABLE Tab1 (Col1 int not null, Col2 char(1), PRIMARY KEY (Col1));
						INSERT Tab1 VALUES (1, 'A'), (2, 'B');";
					UpgCommandRunner.RunCommandOnGivenDb(adminConnection, testMockOldSharedRefDb, sql);

					sql = String.Format("CREATE SYNONYM [RefDbCmrZZ_Tab1] FOR [{0}]..[Tab1];", testMockOldSharedRefDb);
					adminConnection.ExecuteNonQuery(sql);

					Assert("New shared database should not yet exist.", !adminConnection.DatabaseExists(testStrategy.RefDbName));
					((SharedDatabasePreparationStrategyWithMockMainDbForTesting)testStrategy).DoCreateDatabase_Exposed(adminConnection);
					Assert("New shared database should now exist. Ensure it has been created in CreateDatabase().", adminConnection.DatabaseExists(testStrategy.RefDbName));

					sql = String.Format(@"
						ALTER DATABASE [{0}]
						MODIFY FILE (NAME = [{0}_Data], SIZE = 112MB, FILEGROWTH = 12MB);
						ALTER DATABASE [{0}]
						MODIFY FILE (NAME = [{0}_Log], SIZE = 111MB, FILEGROWTH = 11MB);",
						testStrategy.RefDbName);
					adminConnection.ExecuteNonQuery(sql);

					int checkedDataFile, checkedLogFile;
					checkedDataFile = checkedLogFile = 0;
					sql = String.Format(@"
						SELECT
							CEILING(size/128.0) AS size,
							CEILING(growth/128.0) AS growth,
							type AS type
						FROM sys.master_files AS mf
						WHERE DB_NAME(mf.database_id) = '{0}';",
						testStrategy.RefDbName);

					using (var cmd = adminConnection.Command(sql))
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							if (reader.GetByte(reader.GetOrdinal("type")) == 0)
							{
								AssertEquals("Data file should have SIZE = 112MB. Ensure it is affected by the above query, and determined correctly in CreateDatabase().",
									112, (int)reader.GetDecimal(reader.GetOrdinal("size")));
								AssertEquals("Data file should have GROWTH = 12MB. Ensure it is affected by the above query, and determined correctly in CreateDatabase().",
									12, (int)reader.GetDecimal(reader.GetOrdinal("growth")));
								++checkedDataFile;
							}
							else if (reader.GetByte(reader.GetOrdinal("type")) == 1)
							{
								AssertEquals("Log file should have SIZE = 111MB. Ensure it is affected by the above query, and determined correctly in CreateDatabase().",
									111, (int)reader.GetDecimal(reader.GetOrdinal("size")));
								AssertEquals("Log file should have GROWTH = 11MB. Ensure it is affected by the above query, and determined correctly in CreateDatabase().",
									11, (int)reader.GetDecimal(reader.GetOrdinal("growth")));
								++checkedLogFile;
							}
						}
					}

					AssertEquals("The data file should have been checked exactly once. Check the results of the above query in MSSMS to debug.", 1, checkedDataFile);
					AssertEquals("The log file should have been checked exactly once. Check the results of the above query in MSSMS to debug.", 1, checkedLogFile);

					string actualUpgradeDb = null;
					testStrategy.PrepareAndUpgradeDatabase(c => { actualUpgradeDb = c.CurrentDatabase; });
					AssertEquals("Upgrade should have been run on the new reference database.", testStrategy.RefDbName, actualUpgradeDb);
					AssertEquals("Table StmExtendedProperty has the same definition as in main database", true, ExtProperty.IsTableDefinitionMatched(adminConnection, testStrategy.RefDbName));
				}
			}
			finally
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, testMockOldSharedRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockNewSharedRefDb, Db.DatabaseName);

					RestoreExtendedPropertiesAfterGetPhysicalPath(adminCnx);
				}

				DbCommitTracker.Ignore(testMockOldSharedRefDb);
				DbCommitTracker.Ignore(testMockNewSharedRefDb);
			}
		}

		[UseSnapshotProtection]
		public void TestCreateSharedReferenceDatabaseWithDefaultLogSizeAndFileGrowth()
		{
			const string testRefDbName = "CW-RefDb-Cmr-ZZ-000901";

			try
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					PrepareExtendedPropertiesBeforeGetPhysicalPath(adminConnection);

					AdoTestUtils.DropDbIfExists(adminConnection, testRefDbName, Db.DatabaseName);
					IRefDbPreparationStrategy testStrategy = new SharedDatabasePreparationStrategyWithMockMainDbForTesting(upgradeContext, adminConnection, Db.DatabaseName, 901);
					AssertEquals("RefDbName", testRefDbName, testStrategy.RefDbName);

					Assert("New shared database should not yet exist.", !adminConnection.DatabaseExists(testStrategy.RefDbName));
					((SharedDatabasePreparationStrategyWithMockMainDbForTesting)testStrategy).DoCreateDatabase_Exposed(adminConnection);
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
			}
			finally
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, testRefDbName, Db.DatabaseName);

					RestoreExtendedPropertiesAfterGetPhysicalPath(adminCnx);
				}

				DbCommitTracker.Ignore(testRefDbName);
			}
		}

		public void TestReferenceDatabasePaths()
		{
			const string testMockFirstSharedRefDb = "CW-RefDb-AAA-AA-000001";
			const string testMockOldSharedRefDb = "CW-RefDb-Cmr-ZZ-000901";
			const string testMockNewSharedRefDb = "CW-RefDb-Cmr-ZZ-000902";
			string originalReferenceDataFile = null;
			string originalReferenceLogFile = null;

			try
			{
				//Test Master extended properites
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);

					string sqlText = @"SELECT value FROM [master].SYS.EXTENDED_PROPERTIES where [name] = 'ReferenceDataFile'";
					using (var command = adminCnx.Command(sqlText))
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							originalReferenceDataFile = (string)reader["value"];
						}
					}

					sqlText = @"SELECT value FROM [master].SYS.EXTENDED_PROPERTIES where [name] = 'ReferenceLogFile'";
					using (var command = adminCnx.Command(sqlText))
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							originalReferenceLogFile = (string)reader["value"];
						}
					}
					if (String.IsNullOrEmpty(originalReferenceDataFile))
					{
						sqlText = @"EXEC [master].sys.sp_addextendedproperty @name = N'ReferenceDataFile', @value = N'D:\SQL2016\Data\RefDB'";
						using (var command = adminCnx.Command(sqlText))
						{
							command.ExecuteNonQuery();
						}
					}
					if (String.IsNullOrEmpty(originalReferenceLogFile))
					{
						sqlText = @"EXEC [master].sys.sp_addextendedproperty @name = N'ReferenceLogFile', @value = N'D:\SQL2016\Log\RefDB'";
						using (var command = adminCnx.Command(sqlText))
						{
							command.ExecuteNonQuery();
						}
					}
				}

				using (var upgConnection = Db.NewAdminConnection(SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb))
				{
					var testStrategy = new SharedDatabasePreparationStrategyWithMockMainDbForTesting(upgradeContext, upgConnection, 902);
					AssertEquals("RefDbName", testMockNewSharedRefDb, testStrategy.RefDbName);

					var mockUpgradeContext = Mock.Get(upgradeContext);
					mockUpgradeContext.Setup(x => x.IsHosted).Returns(false);
					testStrategy.GetPhysicalDatabaseSettingsFromMasterDB_Exposed(out var dataPath, out var logPath, upgConnection);
					Assert(@"Get RefDB DBPath and LogPath from Master's extended properties", string.IsNullOrEmpty(dataPath) && string.IsNullOrEmpty(logPath));

					mockUpgradeContext.Setup(x => x.IsHosted).Returns(true);
					testStrategy.GetPhysicalDatabaseSettingsFromMasterDB_Exposed(out dataPath, out logPath, upgConnection);
					Assert(@"Get RefDB DBPath and LogPath from Master's extended properties", !string.IsNullOrEmpty(dataPath) && !string.IsNullOrEmpty(logPath));
				}

				using (var adminCnx = Db.NewAdminConnection())
				{
					if (String.IsNullOrEmpty(originalReferenceDataFile))
					{
						string sqlText = @"EXEC [master].sys.sp_dropextendedproperty @name=N'ReferenceDataFile'";
						using (var command = adminCnx.Command(sqlText))
						{
							command.ExecuteNonQuery();
						}
					}
					if (String.IsNullOrEmpty(originalReferenceLogFile))
					{
						string sqlText = @"EXEC [master].sys.sp_dropextendedproperty @name=N'ReferenceLogFile'";
						using (var command = adminCnx.Command(sqlText))
						{
							command.ExecuteNonQuery();
						}
					}
				}

				//Test Previous shared ref db	
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, testMockOldSharedRefDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, testMockOldSharedRefDb, Db.DatabaseName);
				}

				using (var upgConnection = Db.NewAdminConnection(SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb))
				{
					var testStrategy = new SharedDatabasePreparationStrategyWithMockMainDbForTesting(upgradeContext, upgConnection, 902);
					AssertEquals("RefDbName", testMockNewSharedRefDb, testStrategy.RefDbName);

					testStrategy.GetPhysicalDatabaseSettingsFromPreviousRefDB_Exposed(out var dataPath, out var logPath, upgConnection);
					Assert(@"Get RefDB DBPath and LogPath from Master's extended properties", !string.IsNullOrEmpty(dataPath) && !string.IsNullOrEmpty(logPath));
				}

				//Test First shared ref db	
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, testMockOldSharedRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockNewSharedRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockFirstSharedRefDb, Db.DatabaseName);
					AdoTestUtils.CreateDbIfNotExists(adminCnx, testMockFirstSharedRefDb, Db.DatabaseName);
				}

				using (var upgConnection = Db.NewAdminConnection(SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb))
				{
					var testStrategy = new SharedDatabasePreparationStrategyWithMockMainDbForTesting(upgradeContext, upgConnection, 902);

					testStrategy.GetPhysicalDatabaseSettingsFromFirstRefDB_Exposed(out var dataPath, out var logPath, upgConnection);
					Assert(@"Get RefDB DBPath and LogPath from FirstRefDB", !string.IsNullOrEmpty(dataPath) && !string.IsNullOrEmpty(logPath));
				}

				//Test main db	
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, testMockOldSharedRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockNewSharedRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockFirstSharedRefDb, Db.DatabaseName);
				}

				using (var upgConnection = Db.NewAdminConnection(SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb))
				{
					var testStrategy = new SharedDatabasePreparationStrategyWithMockMainDbForTesting(upgradeContext, upgConnection, 902);
					AssertEquals("RefDbName", testMockNewSharedRefDb, testStrategy.RefDbName);

					testStrategy.GetPhysicalDatabaseSettingsFromMainDB_Exposed(out var dataPath, out var logPath, upgConnection);
					Assert(@"Get RefDB DBPath and LogPath from MainDB", !string.IsNullOrEmpty(dataPath) && !string.IsNullOrEmpty(logPath));
				}
			}
			finally
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockOldSharedRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockNewSharedRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockFirstSharedRefDb, Db.DatabaseName);
				}
				DbCommitTracker.Ignore(SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb);
				DbCommitTracker.Ignore(testMockOldSharedRefDb);
				DbCommitTracker.Ignore(testMockNewSharedRefDb);
				DbCommitTracker.Ignore(testMockFirstSharedRefDb);
			}
		}

		void PrepareExtendedPropertiesBeforeGetPhysicalPath(AdminConnection adminConnection)
		{
			string sqlText =
			#region sqlText
@"
DECLARE 
	@IsValidPath int = 1,
	@Command nvarchar(300),
	@DataPath nvarchar(300) = NULL, 
	@LogPath nvarchar(300) = NULL,
	@DefaultDataPath nvarchar(256) = CONVERT(nvarchar(256), SERVERPROPERTY('InstanceDefaultDataPath')),
	@DefaultLogPath nvarchar(256) = CONVERT(nvarchar(256), SERVERPROPERTY('InstanceDefaultLogPath'))

IF (RIGHT(@DefaultDataPath, 1) = N'\')
	SET @DefaultDataPath = LEFT(@DefaultDataPath, LEN(@DefaultDataPath) - 1)
IF (RIGHT(@DefaultLogPath, 1) = N'\')
	SET @DefaultLogPath = LEFT(@DefaultLogPath, LEN(@DefaultLogPath) - 1)

SELECT @DataPath = CAST(value AS nvarchar(max)) FROM master.sys.extended_properties WHERE name = N'ReferenceDataFile'
SELECT @LogPath = CAST(value AS nvarchar(max)) FROM master.sys.extended_properties WHERE name = N'ReferenceLogFile'

IF (@DataPath IS NOT NULL) BEGIN
	SET @Command = N'dir ' + @DataPath
	EXEC @IsValidPath = xp_cmdshell @Command

	IF (@IsValidPath <> 0) BEGIN
		IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'ReferenceDataFile_temp')
			EXEC master.sys.sp_updateextendedproperty @name = N'ReferenceDataFile_temp', @value = @DataPath
		ELSE
			EXEC master.sys.sp_addextendedproperty @name = N'ReferenceDataFile_temp', @value = @DataPath

		EXEC master.sys.sp_updateextendedproperty @name = N'ReferenceDataFile', @value = @DefaultDataPath
	END
END
ELSE BEGIN
	IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'ReferenceDataFile_temp')
		EXEC master.sys.sp_updateextendedproperty @name = N'ReferenceDataFile_temp', @value = NULL
	ELSE
		EXEC master.sys.sp_addextendedproperty @name = N'ReferenceDataFile_temp', @value = NULL

	EXEC master.sys.sp_addextendedproperty @name = N'ReferenceDataFile', @value = @DefaultDataPath
END

IF (@LogPath IS NOT NULL) BEGIN
	SET @Command = N'dir ' + @DataPath
	EXEC @IsValidPath = xp_cmdshell @Command

	IF (@IsValidPath <> 0) BEGIN
		IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'ReferenceLogFile_temp')
			EXEC master.sys.sp_updateextendedproperty @name = N'ReferenceLogFile_temp', @value = @LogPath
		ELSE
			EXEC master.sys.sp_addextendedproperty @name = N'ReferenceLogFile_temp', @value = @LogPath

		EXEC master.sys.sp_updateextendedproperty @name = N'ReferenceLogFile', @value = @DefaultLogPath
	END
END
ELSE BEGIN
	IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'ReferenceLogFile_temp')
		EXEC master.sys.sp_updateextendedproperty @name = N'ReferenceLogFile_temp', @value = NULL
	ELSE
		EXEC master.sys.sp_addextendedproperty @name = N'ReferenceLogFile_temp', @value = NULL

	EXEC master.sys.sp_addextendedproperty @name = N'ReferenceLogFile', @value = @DefaultLogPath
END
";
			#endregion //sqlText

			adminConnection.ExecuteNonQuery(sqlText);
		}

		void RestoreExtendedPropertiesAfterGetPhysicalPath(AdminConnection adminConnection)
		{
			string sqlText =
			#region sqlText
@"
DECLARE 
	@DataPath nvarchar(300) = NULL, 
	@LogPath nvarchar(300) = NULL

IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'ReferenceDataFile_temp') BEGIN
	SELECT @DataPath = CAST(value AS nvarchar(max)) FROM master.sys.extended_properties WHERE name = N'ReferenceDataFile_temp'
	exec master.sys.sp_dropextendedproperty @name = N'ReferenceDataFile_temp'

	IF (@DataPath IS NULL) BEGIN
		IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'ReferenceDataFile')
			exec master.sys.sp_dropextendedproperty @name = N'ReferenceDataFile'
	END
	ELSE IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'ReferenceDataFile')
		exec master.sys.sp_updateextendedproperty @name = N'ReferenceDataFile', @value = @DataPath
END

IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'ReferenceLogFile_temp') BEGIN
	SELECT @LogPath = CAST(value AS nvarchar(max)) FROM master.sys.extended_properties WHERE name = N'ReferenceLogFile_temp'
	exec master.sys.sp_dropextendedproperty @name = N'ReferenceLogFile_temp'

	IF (@LogPath IS NULL) BEGIN
		IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'ReferenceLogFile')
			exec master.sys.sp_dropextendedproperty @name = N'ReferenceLogFile'
	END
	ELSE IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'ReferenceLogFile')
		exec master.sys.sp_updateextendedproperty @name = N'ReferenceLogFile', @value = @LogPath
END
";
			#endregion //sqlText

			adminConnection.ExecuteNonQuery(sqlText);
		}

		protected override void SetUp()
		{
			upgradeContext = new Mock<IUpgradeContext>().Object;
			base.SetUp();
		}

		DbConnection TestConnection => Db.Connection;
		IUpgradeContext upgradeContext;

		class SharedDatabasePreparationStrategyWithMockMainDbForTesting : SharedDatabasePreparationStrategy
		{
			public SharedDatabasePreparationStrategyWithMockMainDbForTesting(IUpgradeContext context, DbConnection upgradeConnection, int versionToUpgradTo)
				: base(context, TestMockDb, RefDbTypeEnum.Customs, "ZZ", upgradeConnection, versionToUpgradTo)
			{
			}

			public SharedDatabasePreparationStrategyWithMockMainDbForTesting(IUpgradeContext context, DbConnection upgradeConnection, string mainDbName, int versionToUpgradTo)
				: base(context, mainDbName, RefDbTypeEnum.Customs, "ZZ", upgradeConnection, versionToUpgradTo)
			{
			}

			public const string TestMockDb = "MockDbSharedDatabasePreparationStrategyTest";

			public string TestMockExclusiveRefDb
			{
				get { return CalculatedPrivateRefDbName; }
			}

			public void DoCreateDatabase_Exposed(AdminConnection upgConnection)
			{
				DoCreateDatabase(upgConnection);
			}

			public string VersionSharedRefDbPrefix_Exposed
			{
				get { return VersionSharedRefDbPrefix; }
			}

			public string BaseDatabaseForUpgrade_Exposed
			{
				get { return BaseDatabaseForUpgrade; }
			}

			public void GetPhysicalDatabaseSettingsFromMasterDB_Exposed(out string dataPath, out string logPath, AdminConnection adminConnection)
			{
				GetPhysicalDatabaseSettingsFromMasterDB(out dataPath, out logPath, adminConnection);
			}

			public void GetPhysicalDatabaseSettingsFromPreviousRefDB_Exposed(out string dataPath, out string logPath, AdminConnection adminConnection)
			{
				GetPhysicalDatabaseSettingsFromPreviousRefDB(out dataPath, out logPath, adminConnection);
			}

			public void GetPhysicalDatabaseSettingsFromFirstRefDB_Exposed(out string dataPath, out string logPath, AdminConnection adminConnection)
			{
				GetPhysicalDatabaseSettingsFromFirstRefDB(out dataPath, out logPath, adminConnection);
			}

			public void GetPhysicalDatabaseSettingsFromMainDB_Exposed(out string dataPath, out string logPath, AdminConnection adminConnection)
			{
				GetPhysicalDatabaseSettingsFromMainDB(out dataPath, out logPath, adminConnection);
			}
		}
	}
}
