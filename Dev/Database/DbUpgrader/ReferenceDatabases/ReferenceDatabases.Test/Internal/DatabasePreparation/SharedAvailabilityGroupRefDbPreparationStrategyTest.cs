using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases.Testing
{
	sealed class SharedAvailabilityGroupRefDbPreparationStrategyTest : TestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new StrategyForTest(upgradeContext, TestConnection, "", RefDbTypeEnum.Enterprise, "SG", 98765));
			AssertExceptionThrown<ArgumentException>(() => new StrategyForTest(upgradeContext, TestConnection, null, RefDbTypeEnum.Enterprise, "SG", 98765));

			AssertExceptionThrown<ArgumentException>(() => new SharedAvailabilityGroupRefDbPreparationStrategy(upgradeContext, TestConnection, "blah", "", RefDbTypeEnum.Enterprise, "SG", 98765));
			AssertExceptionThrown<ArgumentException>(() => new SharedAvailabilityGroupRefDbPreparationStrategy(upgradeContext, TestConnection, "blah", null, RefDbTypeEnum.Enterprise, "SG", 98765));

			AssertExceptionThrown<NotSupportedException>("Constructor", "To use this strategy, the main database must participate in AlwaysOn Availability Group", () => new SharedAvailabilityGroupRefDbPreparationStrategy(upgradeContext, TestConnection, "blah", RefDbTypeEnum.Enterprise, "SG", 98765, string.Empty));
		}

		public void TestRefDbName()
		{
			AssertEquals("Reference Database Enterprise US",
				"CW-AG-RefDb-ORDWP4-CP1AS1-Ent-US-000023",
				new StrategyForTest(upgradeContext, TestConnection, "MainDbName", RefDbTypeEnum.Enterprise, "US", 23).RefDbName);

			AssertEquals("Reference Database Enterprise US",
				"CW-AG-RefDb-ORDWP4-CP1AS1-Ent-US-1234567",
				new StrategyForTest(upgradeContext, TestConnection, "MainDbName", RefDbTypeEnum.Enterprise, "US", 1234567).RefDbName);

			AssertEquals("Reference Database Enterprise SG",
				"CW-AG-RefDb-ORDWP4-CP1AS1-Ent-SG-098765",
				new StrategyForTest(upgradeContext, TestConnection, "MainDbName", RefDbTypeEnum.Enterprise, "SG", 98765).RefDbName);

			AssertEquals("Reference Database Tariff NZ",
				"CW-AG-RefDb-ORDWP4-CP1AS1-Trf-NZ-000000",
				new StrategyForTest(upgradeContext, TestConnection, "MainDbName", RefDbTypeEnum.Tariff, "NZ", 0).RefDbName);
		}

		public void TestGetVersionFromDatabase()
		{
			var mainDb = "SharedAGRefDb";
			var exclusiveRefDb = $"{mainDb}_RefDb_Cmr_ZZ";
			var sharedRefDb = $"CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-902";

			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, exclusiveRefDb, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, sharedRefDb, mainDb))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = new StrategyForTest(upgradeContext, upgConnection, mainDb, 902);

				// Create legacy version table
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, exclusiveRefDb, $@"
CREATE TABLE {ReferenceDbUpgrader.VersionPropertyName} (SV_Version int);
INSERT {ReferenceDbUpgrader.VersionPropertyName} VALUES (-23);

");

				// No shared db of this type with version < latest => BaseDatabaseForUpgrade == exclusive ref db
				AssertEquals("BaseDatabaseForUpgrade", exclusiveRefDb, testStrategy.BaseDatabaseForUpgrade_Exposed);

				// No synonyms pointing to version shared DB and no extended property initially => gets version from version table in the exclusive DB
				AssertEquals("Reference database version", -23, testStrategy.VersionFromDatabase);
				var extendedPtyValue = DataUtils.LoadDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, exclusiveRefDb);
				AssertEquals("RefDbVersion property", "-23", extendedPtyValue);

				// No synonyms to version shared DB, but extended property created in the exclusive DB => gets version from extended pty in the exclusive DB
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "96", exclusiveRefDb);
				AssertEquals("Reference database version", 96, testStrategy.VersionFromDatabase);

				// Create synonym on main mock database [RefDbCmrZZ_SomeTable] referencing new shared database
				upgConnection.ExecuteNonQuery($"CREATE SYNONYM [RefDbCmrZZ_SomeTable] FOR [{sharedRefDb}]..[SomeTable];");

				// Synonyms to version shared DB created, but its extended property is not set => gets version from extended pty in the exclusive DB
				AssertEquals("Reference database version", 96, testStrategy.VersionFromDatabase);

				// Synonyms to version shared DB created, and its extended property set => gets version from extended pty in the synonym base database (shared DB)
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "902", exclusiveRefDb);
				AssertEquals("Reference database version", 902, testStrategy.VersionFromDatabase);
			}
		}

		public void TestGetVersionFromDatabaseWithCloseToLatestVersionDb()
		{
			var mainDb = "SharedAGRefDb";
			var exclusiveRefDb = $"{mainDb}_RefDb_Cmr_ZZ";
			var sharedRefDb = $"CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-902";
			var closeToLatestVersionSharedDb = $"CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-900";

			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, exclusiveRefDb, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, sharedRefDb, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, closeToLatestVersionSharedDb, mainDb))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = new StrategyForTest(upgradeContext, upgConnection, mainDb, 902);

				// Ensure close-to-latest version shared ref db version matches its name.
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "900", closeToLatestVersionSharedDb);
				AssertEquals("BaseDatabaseForUpgrade", closeToLatestVersionSharedDb, testStrategy.BaseDatabaseForUpgrade_Exposed);

				// No RefDb synonyms + Exclusive db version = latest => Exclusive db version used.
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "902", exclusiveRefDb);
				testStrategy.RefreshCurrentDatabaseNameForTesting();
				AssertEquals("Reference database version", 902, testStrategy.VersionFromDatabase);

				// No RefDb synonyms, but exclusive db version != latest
				// => Use close-to-latest version shared db.
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "300", exclusiveRefDb);
				testStrategy.RefreshCurrentDatabaseNameForTesting();
				AssertEquals("Reference database version", 900, testStrategy.VersionFromDatabase);

				// Synonyms to latest-version shared DB created, but its extended property is not set
				// => gets version extended pty from close-to-latest version shared DB
				upgConnection.ExecuteNonQuery($"CREATE SYNONYM [RefDbCmrZZ_SomeTable] FOR [{sharedRefDb}]..[SomeTable];");
				testStrategy.RefreshCurrentDatabaseNameForTesting();
				AssertEquals("Reference database version", 900, testStrategy.VersionFromDatabase);

				// Synonyms point to version latest-version DB, and extended property is set
				// => gets version from extended pty in the synonym base database (latest-version shared DB)
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "902", sharedRefDb);
				testStrategy.RefreshCurrentDatabaseNameForTesting();
				AssertEquals("Reference database version", 902, testStrategy.VersionFromDatabase);
			}
		}

		public void TestGetVersionFromPrevSharedDatabaseWithCloseToLatestVersionDb()
		{
			var mainDb = "SharedAGRefDb";
			var exclusiveRefDb = $"{mainDb}_RefDb_Cmr_ZZ";
			var sharedRefDb = $"CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-902";
			var closeToLatestVersionSharedDb = $"CW-RefDb-Cmr-ZZ-900";

			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, exclusiveRefDb, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, sharedRefDb, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, closeToLatestVersionSharedDb, mainDb))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = new StrategyForTest(upgradeContext, upgConnection, mainDb, 902);

				// Ensure close-to-latest version shared ref db version matches its name.
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "900", closeToLatestVersionSharedDb);
				AssertEquals("BaseDatabaseForUpgrade", closeToLatestVersionSharedDb, testStrategy.BaseDatabaseForUpgrade_Exposed);

				// No RefDb synonyms + Exclusive db version = latest => Exclusive db version used.
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "902", exclusiveRefDb);
				testStrategy.RefreshCurrentDatabaseNameForTesting();
				AssertEquals("Reference database version", 902, testStrategy.VersionFromDatabase);

				// No RefDb synonyms, but exclusive db version != latest
				// => Use close-to-latest version shared db.
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "300", exclusiveRefDb);
				testStrategy.RefreshCurrentDatabaseNameForTesting();
				AssertEquals("Reference database version", 900, testStrategy.VersionFromDatabase);

				// Synonyms to latest-version shared DB created, but its extended property is not set
				// => gets version extended pty from close-to-latest version shared DB
				upgConnection.ExecuteNonQuery($"CREATE SYNONYM [RefDbCmrZZ_SomeTable] FOR [{sharedRefDb}]..[SomeTable];");
				testStrategy.RefreshCurrentDatabaseNameForTesting();
				AssertEquals("Reference database version", 900, testStrategy.VersionFromDatabase);

				// Synonyms point to version latest-version DB, and extended property is set
				// => gets version from extended pty in the synonym base database (latest-version shared DB)
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "902", sharedRefDb);
				testStrategy.RefreshCurrentDatabaseNameForTesting();
				AssertEquals("Reference database version", 902, testStrategy.VersionFromDatabase);
			}
		}

		public void TestPrepareAndUpgradeDatabase()
		{
			var mainDb = "SharedAGRefDb";
			var oldSharedRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000901";
			var newSharedRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000902";

			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, oldSharedRefDb, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, newSharedRefDb, mainDb))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = new StrategyForTest(upgradeContext, upgConnection, mainDb, 902);
				AssertEquals("RefDbName", newSharedRefDb, testStrategy.RefDbName);

				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, oldSharedRefDb, @"
CREATE TABLE Tab1
(
	Col11 int NOT NULL PRIMARY KEY,
	Col12 char(1),
);

INSERT Tab1 (Col11, Col12) VALUES
	(1, 'A'),
	(2, 'B')

CREATE TABLE Tab2
(
	Col21 int NOT NULL PRIMARY KEY,
	Col22 int,
);

ALTER TABLE Tab2 ADD
	CONSTRAINT FK_Tab2_Tab1 FOREIGN KEY (Col22) REFERENCES Tab1 (Col11);

INSERT Tab2 (Col21, Col22) VALUES
	(21, 1),
	(22, NULL),
	(23, 2)

ALTER TABLE Tab2 WITH CHECK ADD CONSTRAINT
	CH_Tab2_Col21 CHECK (Col21 > 0);

EXEC sys.sp_addextendedproperty @name = 'Pty01', @value = 'SourceOne';
EXEC sys.sp_addextendedproperty @name = 'Pty02', @value = 'SourceTwo';
EXEC sys.sp_addextendedproperty @name = 'Pty03', @value = 'SourceThree', @level0type = N'SCHEMA', @level0name = 'dbo';

");

				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, newSharedRefDb, @"
CREATE TABLE Tab2
(
	Col20 bit,
);

INSERT Tab2 (Col20) VALUES (1);

EXEC sys.sp_addextendedproperty @name = 'Pty02', @value = 'ExistingValue';

");

				// Create synonym on main mock database [RefDbCmrZZ_Tab1] referencing the old shared database
				upgConnection.ExecuteNonQuery($"CREATE SYNONYM [RefDbCmrZZ_Tab1] FOR [{oldSharedRefDb}]..[Tab1];");

				AssertEquals("Tab1 exists on old shared database?", true, DbObjectCreator.TableExists(upgConnection, oldSharedRefDb, "Tab1"));
				AssertEquals("Tab2 exists on old shared database?", true, DbObjectCreator.TableExists(upgConnection, oldSharedRefDb, "Tab2"));
				AssertEquals("Check constraint exists on old shared database?", true
					, upgConnection.Exists($"FROM [{oldSharedRefDb}].sys.check_constraints WHERE parent_object_id = OBJECT_ID('[{oldSharedRefDb}]..Tab2')"));
				AssertEquals("Tab1 exists on new shared database?", false, DbObjectCreator.TableExists(upgConnection, newSharedRefDb, "Tab1"));
				AssertEquals("Tab2 exists on new shared database?", true, DbObjectCreator.TableExists(upgConnection, newSharedRefDb, "Tab2"));
				AssertEquals("Check constraint exists on new shared database?", false
					, upgConnection.Exists($"FROM [{newSharedRefDb}].sys.check_constraints WHERE parent_object_id = OBJECT_ID('[{newSharedRefDb}]..Tab2')"));

				string actualUpgDb = null;
				((IRefDbPreparationStrategy)testStrategy).PrepareAndUpgradeDatabase(c => { actualUpgDb = c.CurrentDatabase; });

				// There is a shared db with version = latest-1 [CW-RefDb-Cmr-ZZ-000901] => BaseDatabaseForUpgrade == "CW-RefDb-Cmr-ZZ-000901"
				AssertEquals("BaseDatabaseForUpgrade", oldSharedRefDb, testStrategy.BaseDatabaseForUpgrade_Exposed);

				AssertEquals("Upgrade runs on new reference db", newSharedRefDb, actualUpgDb);
				AssertEquals("Does new shared database exist?", true, upgConnection.DatabaseExists(newSharedRefDb));

				using (((ICurrentDbControl)upgConnection).UseDatabase(newSharedRefDb))
				{
					AssertEquals("Tab1 created on new shared database?", true, DbObjectCreator.TableExists(upgConnection, "Tab1"));
					AssertEquals("Tab2 created on new shared database?", true, DbObjectCreator.TableExists(upgConnection, "Tab2"));
					AssertEquals("Tab1 row count", 2, upgConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM Tab1"));
					AssertEquals("Tab2 row count", 3, upgConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM Tab2"));

					AssertEquals(
						"Tab1 PK created?", true,
						upgConnection.Exists("FROM sys.key_constraints WHERE type = 'PK' AND parent_object_id = OBJECT_ID('Tab1')"));
					AssertEquals(
						"Tab2 PK created?", true,
						upgConnection.Exists("FROM sys.key_constraints WHERE type = 'PK' AND parent_object_id = object_id('Tab2')"));
					AssertEquals(
						"Tab2 FK created?", true,
						upgConnection.Exists("FROM sys.foreign_keys WHERE parent_object_id = object_id('Tab2')"));

					AssertEquals(
						"Check constraint exists on new shared database?", true,
						upgConnection.Exists($"FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID('Tab2')"));

					// Extended Properties: Pty01 created, Pty02 updated, Pty03 not copied (non DB level)
					AssertEquals("Pty01", "SourceOne", DataUtils.LoadDbExtendedProperty(upgConnection, "Pty01"));
					AssertEquals("Pty02", "SourceTwo", DataUtils.LoadDbExtendedProperty(upgConnection, "Pty02"));
					AssertNull("Pty03", DataUtils.LoadDbExtendedProperty(upgConnection, "Pty03"));
				}
			}
		}

		public void TestPrepareAndUpgradeDatabaseWithNonexistentPreviousDb()
		{
			var mainDb = "SharedAGRefDb";
			var oldExclusiveRefDb = $"{mainDb}_RefDb_Cmr_ZZ";
			var newSharedRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000001";

			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, newSharedRefDb, mainDb))
			using (AdoTestUtils.DropDbIfExistsDisposable(TestConnection, oldExclusiveRefDb))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = new StrategyForTest(upgradeContext, upgConnection, mainDb, 1);
				AssertEquals("RefDbName", newSharedRefDb, testStrategy.RefDbName);

				// Create synonym on main mock database [RefDbCmrZZ_Tab1] referencing the old ref database
				upgConnection.ExecuteNonQuery($"CREATE SYNONYM [RefDbCmrZZ_Tab1] FOR [{oldExclusiveRefDb}]..[Tab1];");

				string actualUpgDb = null;
				((IRefDbPreparationStrategy)testStrategy).PrepareAndUpgradeDatabase(c => { actualUpgDb = c.CurrentDatabase; });

				// Candidate base database (oldExclusiveRefDb) does not exist => BaseDatabaseForUpgrade == ""
				AssertEquals("BaseDatabaseForUpgrade", "", testStrategy.BaseDatabaseForUpgrade_Exposed);

				AssertEquals("Upgrade runs on new reference db", newSharedRefDb, actualUpgDb);
				AssertEquals("Does new shared database exist?", true, TestConnection.DatabaseExists(newSharedRefDb));
			}
		}

		public void TestPrepareAndUpgradeDatabaseTwoAtTheSameTime()
		{
			var mainDb = "SharedAGRefDb";
			var oldExclusiveRefDb = $"{mainDb}_RefDb_Cmr_ZZ";
			var newSharedRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000001";

			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, mainDb))
			using (AdoTestUtils.DropDbIfExistsDisposable(TestConnection, oldExclusiveRefDb, mainDb))
			using (AdoTestUtils.DropDbIfExistsDisposable(TestConnection, newSharedRefDb, mainDb))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var eventTimeout = TimeSpan.FromMinutes(1);

				// Create synonym on main mock database [RefDbCmrZZ_Tab1] referencing the old ref database
				upgConnection.ExecuteNonQuery($"CREATE SYNONYM [RefDbCmrZZ_Tab1] FOR [{oldExclusiveRefDb}]..[Tab1];");

				using (var upgrade1ReadyEvent = new AutoResetEvent(false))
				using (var upgrade2ReadyEvent = new AutoResetEvent(false))
				{
					var task1 = Task.Run(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							upgrade1ReadyEvent.Set();
							AssertEquals(true, upgrade2ReadyEvent.WaitOne(eventTimeout));
							AssertPrepareAndUpgradeDatabaseWithNonexistentPreviousDb(mainDb, newSharedRefDb);
						}
					});

					upgrade2ReadyEvent.Set();
					AssertEquals(true, upgrade1ReadyEvent.WaitOne(eventTimeout));
					AssertPrepareAndUpgradeDatabaseWithNonexistentPreviousDb(mainDb, newSharedRefDb);

					AssertEquals(true, task1.Wait(eventTimeout));
				}
			}
		}

		public void TestPrepareAndUpgradeDatabaseWithPreviousNonSharedDb()
		{
			// upgrade should use previous shared DB (assert data in the new DB matches it)
			var mainDb = "SharedAGRefDb";
			var oldExclusiveRefDb = $"{mainDb}_RefDb_Cmr_ZZ";
			var oldSharedRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000901";
			var newSharedRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000902";

			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, oldExclusiveRefDb, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, oldSharedRefDb, mainDb))
			using (AdoTestUtils.DropDbIfExistsDisposable(TestConnection, newSharedRefDb, mainDb))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = new StrategyForTest(upgradeContext, upgConnection, mainDb, 902);
				AssertEquals("RefDbName", newSharedRefDb, testStrategy.RefDbName);

				// Set version extended property on reference databases
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "900", oldExclusiveRefDb);
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "901", oldSharedRefDb);

				// Add test data to previous reference database (non-shared)
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, oldExclusiveRefDb, @"
CREATE TABLE Tab1
(
	Col11 int NOT NULL PRIMARY KEY,
	Col12 char(1),
);

INSERT Tab1 (Col11, Col12) VALUES
	(1, 'X')

CREATE TABLE TabX2
(
	Col21 int NOT NULL PRIMARY KEY,
	Col22 int,
);

INSERT TabX2 (Col21, Col22) VALUES
	(11, 1),
	(12, 2),
	(13, NULL)

");

				// Add test data to existing shared reference database(different from current non-shared database)
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, oldSharedRefDb, @"
CREATE TABLE Tab1
(
	Col11 int NOT NULL PRIMARY KEY,
	Col12 char(1),
);

INSERT Tab1 (Col11, Col12) VALUES
	(1, 'A'),
	(2, 'B')

CREATE TABLE TabS2
(
	Col21 int NOT NULL PRIMARY KEY,
	Col22 int,
);

INSERT TabS2 (Col21, Col22) VALUES
	(21, 1)

");

				// Create synonym on main mock database [RefDbCmrZZ_Tab1] referencing the current reference database
				upgConnection.ExecuteNonQuery($"CREATE SYNONYM [RefDbCmrZZ_Tab1] FOR [{oldExclusiveRefDb}]..[Tab1];");

				string actualUpgDb = null;
				((IRefDbPreparationStrategy)testStrategy).PrepareAndUpgradeDatabase(c => { actualUpgDb = c.CurrentDatabase; });

				// There is a shared db with version = latest-1 [CW-RefDb-Cmr-ZZ-000901] => BaseDatabaseForUpgrade == "CW-RefDb-Cmr-ZZ-000901"
				AssertEquals("BaseDatabaseForUpgrade", oldSharedRefDb, testStrategy.BaseDatabaseForUpgrade_Exposed);

				AssertEquals("Upgrade runs on new reference db", newSharedRefDb, actualUpgDb);
				AssertEquals("Does new shared database exist?", true, upgConnection.DatabaseExists(newSharedRefDb));

				using (((ICurrentDbControl)upgConnection).UseDatabase(newSharedRefDb))
				{
					// Assert new reference database data
					// Existing shared database should be used as the base for the upgrade
					AssertEquals("Tab1 created on new shared database?", true, DbObjectCreator.TableExists(upgConnection, "Tab1"));
					AssertEquals("TabX2 created on new shared database?", false, DbObjectCreator.TableExists(upgConnection, "TabX2"));
					AssertEquals("TabS2 created on new shared database?", true, DbObjectCreator.TableExists(upgConnection, "TabS2"));
					AssertEquals("Tab1 row count", 2, upgConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM Tab1"));
					AssertEquals("Tab2 row count", 1, upgConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM TabS2"));
					AssertEquals("Tab1 row1.col12 value", "A", upgConnection.ExecuteScalar<string>("SELECT Col12 FROM Tab1 WHERE Col11 = 1"));
					AssertEquals("Tab1 row2.col12 value", "B", upgConnection.ExecuteScalar<string>("SELECT Col12 FROM Tab1 WHERE Col11 = 2"));
					AssertEquals("TabS2 row1.col22 value", 1, upgConnection.ExecuteScalar<int>("SELECT Col22 FROM TabS2 WHERE Col21 = 21"));

					AssertEquals(
						"Tab1 PK created?", true,
						upgConnection.Exists("FROM sys.key_constraints WHERE type = 'PK' AND parent_object_id = object_id('Tab1')"));
					AssertEquals(
						"TabS2 PK created?", true,
						upgConnection.Exists("FROM sys.key_constraints WHERE type = 'PK' AND parent_object_id = object_id('TabS2')"));
				}
			}
		}

		public void TestPrepareAndUpgradeDatabaseWithLatestSharedDb()
		{
			var mainDb = "SharedAGRefDb";
			var currentRefDb = $"CW-RefDb-Cmr-ZZ-000146";
			var oldSharedAgRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000142";
			var newSharedAgRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000146";

			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, currentRefDb, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, oldSharedAgRefDb, mainDb))
			using (AdoTestUtils.DropDbIfExistsDisposable(TestConnection, newSharedAgRefDb, mainDb))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = new StrategyForTest(upgradeContext, upgConnection, mainDb, RefDbTypeEnum.Customs, "ZZ", 146);
				AssertEquals("RefDbName", newSharedAgRefDb, testStrategy.RefDbName);

				// Set version extended property on reference databases
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "146", currentRefDb);
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "142", oldSharedAgRefDb);

				// Add test data to previous reference database
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, currentRefDb, @"
CREATE TABLE Tab1
(
	Col11 int NOT NULL PRIMARY KEY,
	Col12 char(1),
);

INSERT Tab1 (Col11, Col12) VALUES
	(1, 'A'),
	(2, 'B')

CREATE TABLE TabS2
(
	Col21 int NOT NULL PRIMARY KEY,
	Col22 int,
);

INSERT TabS2 (Col21, Col22) VALUES
	(21, 1)

");

				// Add test data to existing shared reference database (different from current database)
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, oldSharedAgRefDb, @"
CREATE TABLE Tab1
(
	Col11 int NOT NULL PRIMARY KEY,
	Col12 char(1),
);

INSERT Tab1 (Col11, Col12) VALUES
	(1, 'X')

CREATE TABLE TabX2
(
	Col21 int NOT NULL PRIMARY KEY,
	Col22 int,
);

INSERT TabX2 (Col21, Col22) VALUES
	(11, 1),
	(12, 2),
	(13, NULL)

");

				// Create synonym on main mock database [RefDbCmrZZ_Tab1] referencing the current reference database
				upgConnection.ExecuteNonQuery($"CREATE SYNONYM [RefDbCmrZZ_Tab1] FOR [{currentRefDb}]..[Tab1];");

				string actualUpgDb = null;
				((IRefDbPreparationStrategy)testStrategy).PrepareAndUpgradeDatabase(c => { actualUpgDb = c.CurrentDatabase; });

				// The current database is of the latest version [CW-RefDb-Cmr-ZZ-000146] => BaseDatabaseForUpgrade == "CW-RefDb-Cmr-ZZ-000146"
				AssertEquals("BaseDatabaseForUpgrade", currentRefDb, testStrategy.BaseDatabaseForUpgrade_Exposed);

				AssertEquals("Upgrade runs on new reference db", newSharedAgRefDb, actualUpgDb);
				AssertEquals("Does new shared database exist?", true, upgConnection.DatabaseExists(newSharedAgRefDb));

				using (((ICurrentDbControl)upgConnection).UseDatabase(newSharedAgRefDb))
				{
					// Assert new reference database data
					AssertEquals("Tab1 created on new shared database?", true, DbObjectCreator.TableExists(upgConnection, "Tab1"));
					AssertEquals("TabX2 created on new shared database?", false, DbObjectCreator.TableExists(upgConnection, "TabX2"));
					AssertEquals("TabS2 created on new shared database?", true, DbObjectCreator.TableExists(upgConnection, "TabS2"));
					AssertEquals("Tab1 row count", 2, upgConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM Tab1"));
					AssertEquals("Tab2 row count", 1, upgConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM TabS2"));
					AssertEquals("Tab1 row1.col12 value", "A", upgConnection.ExecuteScalar<string>("SELECT Col12 FROM Tab1 WHERE Col11 = 1"));
					AssertEquals("Tab1 row2.col12 value", "B", upgConnection.ExecuteScalar<string>("SELECT Col12 FROM Tab1 WHERE Col11 = 2"));
					AssertEquals("TabS2 row1.col22 value", 1, upgConnection.ExecuteScalar<int>("SELECT Col22 FROM TabS2 WHERE Col21 = 21"));

					AssertEquals(
						"Tab1 PK created?", true,
						upgConnection.Exists("FROM sys.key_constraints WHERE type = 'PK' AND parent_object_id = object_id('Tab1')"));
					AssertEquals(
						"TabS2 PK created?", true,
						upgConnection.Exists("FROM sys.key_constraints WHERE type = 'PK' AND parent_object_id = object_id('TabS2')"));
				}
			}
		}

		public void TestPrepareAndUpgradeDatabase_Current()
		{
			var mainDb = "SharedAGRefDb";
			var currentRefDb = "CW-RefDb-Cmr-ZZ-000333";
			var sharedRefDb = "CW-RefDb-Cmr-ZZ-000111";
			var sharedAgRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000222";
			var newSharedAgRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000444";

			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, currentRefDb, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, sharedRefDb, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, sharedAgRefDb, mainDb))
			using (AdoTestUtils.DropDbIfExistsDisposable(TestConnection, newSharedAgRefDb, mainDb))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = new StrategyForTest(upgradeContext, upgConnection, mainDb, RefDbTypeEnum.Customs, "ZZ", 000444);
				AssertEquals("RefDbName", newSharedAgRefDb, testStrategy.RefDbName);

				// Create synonym on main mock database [RefDbCmrZZ_Tab1] referencing the current reference database
				upgConnection.ExecuteNonQuery($"CREATE SYNONYM [RefDbCmrZZ_Tab1] FOR [{currentRefDb}]..[Tab1];");

				var dbVersion111 = sharedRefDb;
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "111", dbVersion111);
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, dbVersion111, "CREATE TABLE Tab_111 (Col11 int)");

				var dbVersion222 = sharedAgRefDb;
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "222", dbVersion222);
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, dbVersion222, "CREATE TABLE Tab_222 (Col11 int)");

				var dbVersion333 = currentRefDb;
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "333", dbVersion333);
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, dbVersion333, "CREATE TABLE Tab_333 (Col11 int)");

				string actualUpgDb = null;
				((IRefDbPreparationStrategy)testStrategy).PrepareAndUpgradeDatabase(c => { actualUpgDb = c.CurrentDatabase; });

				AssertEquals("BaseDatabaseForUpgrade", currentRefDb, testStrategy.BaseDatabaseForUpgrade_Exposed);

				AssertEquals("Upgrade runs on new reference db", newSharedAgRefDb, actualUpgDb);
				AssertEquals("Does new shared database exist?", true, upgConnection.DatabaseExists(newSharedAgRefDb));

				using (((ICurrentDbControl)upgConnection).UseDatabase(newSharedAgRefDb))
				{
					// Assert new reference database data
					AssertEquals("Tab_111 created on new shared database?", false, DbObjectCreator.TableExists(upgConnection, "Tab_111"));
					AssertEquals("Tab_222 created on new shared database?", false, DbObjectCreator.TableExists(upgConnection, "Tab_222"));
					AssertEquals("Tab_333 created on new shared database?", true, DbObjectCreator.TableExists(upgConnection, "Tab_333"));
				}
			}
		}

		public void TestPrepareAndUpgradeDatabase_Shared()
		{
			var mainDb = "SharedAGRefDb";
			var currentRefDb = "CW-RefDb-Cmr-ZZ-000111";
			var sharedRefDb = "CW-RefDb-Cmr-ZZ-000333";
			var sharedAgRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000222";
			var newSharedAgRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000444";

			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, currentRefDb, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, sharedRefDb, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, sharedAgRefDb, mainDb))
			using (AdoTestUtils.DropDbIfExistsDisposable(TestConnection, newSharedAgRefDb, mainDb))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = new StrategyForTest(upgradeContext, upgConnection, mainDb, RefDbTypeEnum.Customs, "ZZ", 000444);
				AssertEquals("RefDbName", newSharedAgRefDb, testStrategy.RefDbName);

				// Create synonym on main mock database [RefDbCmrZZ_Tab1] referencing the current reference database
				upgConnection.ExecuteNonQuery($"CREATE SYNONYM [RefDbCmrZZ_Tab1] FOR [{currentRefDb}]..[Tab1];");

				var dbVersion111 = currentRefDb;
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "111", dbVersion111);
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, dbVersion111, "CREATE TABLE Tab_111 (Col11 int)");

				var dbVersion222 = sharedAgRefDb;
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "222", dbVersion222);
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, dbVersion222, "CREATE TABLE Tab_222 (Col11 int)");

				var dbVersion333 = sharedRefDb;
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "333", dbVersion333);
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, dbVersion333, "CREATE TABLE Tab_333 (Col11 int)");

				string actualUpgDb = null;
				((IRefDbPreparationStrategy)testStrategy).PrepareAndUpgradeDatabase(c => { actualUpgDb = c.CurrentDatabase; });

				AssertEquals("BaseDatabaseForUpgrade", sharedRefDb, testStrategy.BaseDatabaseForUpgrade_Exposed);

				AssertEquals("Upgrade runs on new reference db", newSharedAgRefDb, actualUpgDb);
				AssertEquals("Does new shared database exist?", true, upgConnection.DatabaseExists(newSharedAgRefDb));

				using (((ICurrentDbControl)upgConnection).UseDatabase(newSharedAgRefDb))
				{
					// Assert new reference database data
					AssertEquals("Tab_111 created on new shared database?", false, DbObjectCreator.TableExists(upgConnection, "Tab_111"));
					AssertEquals("Tab_222 created on new shared database?", false, DbObjectCreator.TableExists(upgConnection, "Tab_222"));
					AssertEquals("Tab_333 created on new shared database?", true, DbObjectCreator.TableExists(upgConnection, "Tab_333"));
				}
			}
		}

		public void TestPrepareAndUpgradeDatabase_SharedAG()
		{
			var mainDb = "SharedAGRefDb";
			var currentRefDb = "CW-RefDb-Cmr-ZZ-000111";
			var sharedRefDb = "CW-RefDb-Cmr-ZZ-000222";
			var sharedAgRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000333";
			var newSharedAgRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000444";

			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, currentRefDb, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, sharedRefDb, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, sharedAgRefDb, mainDb))
			using (AdoTestUtils.DropDbIfExistsDisposable(TestConnection, newSharedAgRefDb, mainDb))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = new StrategyForTest(upgradeContext, upgConnection, mainDb, RefDbTypeEnum.Customs, "ZZ", 000444);
				AssertEquals("RefDbName", newSharedAgRefDb, testStrategy.RefDbName);

				// Create synonym on main mock database [RefDbCmrZZ_Tab1] referencing the current reference database
				upgConnection.ExecuteNonQuery($"CREATE SYNONYM [RefDbCmrZZ_Tab1] FOR [{currentRefDb}]..[Tab1];");

				var dbVersion111 = currentRefDb;
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "111", dbVersion111);
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, dbVersion111, "CREATE TABLE Tab_111 (Col11 int)");

				var dbVersion222 = sharedRefDb;
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "222", dbVersion222);
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, dbVersion222, "CREATE TABLE Tab_222 (Col11 int)");

				var dbVersion333 = sharedAgRefDb;
				DataUtils.SaveDbExtendedProperty(upgConnection, ReferenceDbUpgrader.VersionPropertyName, "333", dbVersion333);
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, dbVersion333, "CREATE TABLE Tab_333 (Col11 int)");

				string actualUpgDb = null;
				((IRefDbPreparationStrategy)testStrategy).PrepareAndUpgradeDatabase(c => { actualUpgDb = c.CurrentDatabase; });

				AssertEquals("BaseDatabaseForUpgrade", sharedAgRefDb, testStrategy.BaseDatabaseForUpgrade_Exposed);

				AssertEquals("Upgrade runs on new reference db", newSharedAgRefDb, actualUpgDb);
				AssertEquals("Does new shared database exist?", true, upgConnection.DatabaseExists(newSharedAgRefDb));

				using (((ICurrentDbControl)upgConnection).UseDatabase(newSharedAgRefDb))
				{
					// Assert new reference database data
					AssertEquals("Tab_111 created on new shared database?", false, DbObjectCreator.TableExists(upgConnection, "Tab_111"));
					AssertEquals("Tab_222 created on new shared database?", false, DbObjectCreator.TableExists(upgConnection, "Tab_222"));
					AssertEquals("Tab_333 created on new shared database?", true, DbObjectCreator.TableExists(upgConnection, "Tab_333"));
				}
			}
		}

		public void TestCreateDatabase()
		{
			var mainDb = "SharedAGRefDb";
			var oldSharedRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000901";
			var newSharedRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000902";

			using (TemporarySetExtendedPropertiesForFilePaths())
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, oldSharedRefDb, mainDb))
			using (AdoTestUtils.DropDbIfExistsDisposable(TestConnection, newSharedRefDb, mainDb))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = (IRefDbPreparationStrategy)new StrategyForTest(upgradeContext, upgConnection, mainDb, 902);
				AssertEquals("RefDbName", newSharedRefDb, testStrategy.RefDbName);

				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, oldSharedRefDb, @"
CREATE TABLE Tab1
(
	Col1 int NOT NULL PRIMARY KEY,
	Col2 char(1),
);

INSERT Tab1 (Col1, Col2) VALUES
	(1, 'A'),
	(2, 'B')

");

				upgConnection.ExecuteNonQuery($"CREATE SYNONYM [RefDbCmrZZ_Tab1] FOR [{oldSharedRefDb}]..[Tab1];");

				AssertEquals("New shared database exists?", false, upgConnection.DatabaseExists(newSharedRefDb));

				((StrategyForTest)testStrategy).DoCreateDatabase_Exposed(upgConnection);
				AssertEquals("New shared database exists? Ensure it has been created in CreateDatabase().", true, upgConnection.DatabaseExists(newSharedRefDb));

				upgConnection.ExecuteNonQuery($@"
ALTER DATABASE [{newSharedRefDb}]
	MODIFY FILE (NAME = [{newSharedRefDb}_Data], SIZE = 112MB, FILEGROWTH = 12MB);
ALTER DATABASE [{newSharedRefDb}]
	MODIFY FILE (NAME = [{newSharedRefDb}_Log], SIZE = 111MB, FILEGROWTH = 11MB);
");

				var checkedDataFile = 0;
				var checkedLogFile = 0;
				var sql = $@"
SELECT
	size   = CEILING(size / 128.0),
	growth = CEILING(growth / 128.0),
	type   = type
FROM
	sys.master_files
WHERE
	database_id = DB_ID('{newSharedRefDb}')

";

				using (var cmd = upgConnection.Command(sql))
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
							checkedDataFile++;
						}
						else if (reader.GetByte(reader.GetOrdinal("type")) == 1)
						{
							AssertEquals("Log file should have SIZE = 111MB. Ensure it is affected by the above query, and determined correctly in CreateDatabase().",
								111, (int)reader.GetDecimal(reader.GetOrdinal("size")));
							AssertEquals("Log file should have GROWTH = 11MB. Ensure it is affected by the above query, and determined correctly in CreateDatabase().",
								11, (int)reader.GetDecimal(reader.GetOrdinal("growth")));
							checkedLogFile++;
						}
					}
				}

				AssertEquals("The data file should have been checked exactly once. Check the results of the above query in MSSMS to debug.", 1, checkedDataFile);
				AssertEquals("The log file should have been checked exactly once. Check the results of the above query in MSSMS to debug.", 1, checkedLogFile);

				string actualUpgradeDb = null;
				testStrategy.PrepareAndUpgradeDatabase(c => { actualUpgradeDb = c.CurrentDatabase; });

				AssertEquals("Upgrade should have been run on the new reference database.", newSharedRefDb, actualUpgradeDb);
				AssertEquals("Table StmExtendedProperty has the same definition as in main database", true, ExtProperty.IsTableDefinitionMatched(Db.Connection, newSharedRefDb));
			}
		}

		public void TestCreateSharedReferenceDatabaseWithDefaultLogSizeAndFileGrowth()
		{
			var mainDb = "SharedAGRefDb";
			var newSharedRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000901";

			using (TemporarySetExtendedPropertiesForFilePaths())
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, mainDb))
			using (AdoTestUtils.DropDbIfExistsDisposable(TestConnection, newSharedRefDb, mainDb))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = new StrategyForTest(upgradeContext, upgConnection, mainDb, 901);
				AssertEquals("RefDbName", newSharedRefDb, testStrategy.RefDbName);

				AssertEquals("New shared database exists?", false, upgConnection.DatabaseExists(newSharedRefDb));

				testStrategy.DoCreateDatabase_Exposed(upgConnection);

				AssertEquals("New shared database exists? Ensure it has been created in CreateDatabase().", true, upgConnection.DatabaseExists(newSharedRefDb));

				var sql = $@"
SELECT
	size   = CEILING(size / 128.0),
	growth = CEILING(growth / 128.0)
FROM
	sys.master_files
WHERE 1=1
	AND database_id = DB_ID('{newSharedRefDb}')
	AND type_desc = 'LOG'

";

				using (var command = upgConnection.Command(sql))
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

		public void TestReferenceDatabasePaths_Master()
		{
			var mainDb = "SharedAGRefDb";
			var newSharedRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000902";

			string originalReferenceDataFile = null;
			string originalReferenceLogFile = null;

			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, mainDb))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				try
				{
					upgConnection.ExecuteReader("SELECT value FROM master.sys.extended_properties WHERE name = 'ReferenceDataFile'"
						, (reader) =>
						{
							originalReferenceDataFile = (string)reader["value"];
						});

					upgConnection.ExecuteReader("SELECT value FROM master.sys.extended_properties WHERE name = 'ReferenceLogFile'"
						, (reader) =>
						{
							originalReferenceLogFile = (string)reader["value"];
						});

					if (string.IsNullOrEmpty(originalReferenceDataFile))
					{
						upgConnection.ExecuteNonQuery(@"EXEC master.sys.sp_addextendedproperty @name = N'ReferenceDataFile', @value = N'D:\SQL2016\Data\RefDB'");
					}

					if (string.IsNullOrEmpty(originalReferenceLogFile))
					{
						upgConnection.ExecuteNonQuery(@"EXEC master.sys.sp_addextendedproperty @name = N'ReferenceLogFile', @value = N'D:\SQL2016\Log\RefDB'");
					}

					var testStrategy = new StrategyForTest(upgradeContext, upgConnection, mainDb, 902);
					AssertEquals("RefDbName", newSharedRefDb, testStrategy.RefDbName);

					var mockUpgradeContext = Mock.Get(upgradeContext);
					mockUpgradeContext.Setup(x => x.IsHosted).Returns(false);
					testStrategy.GetPhysicalDatabaseSettingsFromMasterDB_Exposed(upgConnection, out var dataPath, out var logPath);
					Assert(@"Get RefDB DBPath and LogPath from Master's extended properties", string.IsNullOrEmpty(dataPath) && string.IsNullOrEmpty(logPath));

					mockUpgradeContext.Setup(x => x.IsHosted).Returns(true);
					testStrategy.GetPhysicalDatabaseSettingsFromMasterDB_Exposed(upgConnection, out dataPath, out logPath);
					Assert(@"Get RefDB DBPath and LogPath from Master's extended properties", !string.IsNullOrEmpty(dataPath) && !string.IsNullOrEmpty(logPath));
				}
				finally
				{
					if (string.IsNullOrEmpty(originalReferenceDataFile))
					{
						upgConnection.ExecuteNonQuery("EXEC master.sys.sp_dropextendedproperty @name = N'ReferenceDataFile'");
					}

					if (string.IsNullOrEmpty(originalReferenceLogFile))
					{
						upgConnection.ExecuteNonQuery("EXEC master.sys.sp_dropextendedproperty @name = N'ReferenceLogFile'");
					}
				}
			}
		}

		public void TestReferenceDatabasePaths_PreviousSharedRefDb()
		{
			var mainDb = "SharedAGRefDb";
			var oldSharedRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000901";
			var newSharedRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000902";

			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, oldSharedRefDb, mainDb))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = new StrategyForTest(upgradeContext, upgConnection, mainDb, 902);
				AssertEquals("RefDbName", newSharedRefDb, testStrategy.RefDbName);

				testStrategy.GetPhysicalDatabaseSettingsFromPreviousRefDB_Exposed(upgConnection, out var dataPath, out var logPath);
				Assert(@"Get RefDB DBPath and LogPath from Master's extended properties", !string.IsNullOrEmpty(dataPath) && !string.IsNullOrEmpty(logPath));
			}
		}

		public void TestReferenceDatabasePaths_FirstSharedRefDb()
		{
			var mainDb = "SharedAGRefDb";
			var firstSharedRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-AAA-AA-000001";
			var newSharedRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000902";

			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, mainDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, firstSharedRefDb, mainDb))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = new StrategyForTest(upgradeContext, upgConnection, mainDb, 902);
				AssertEquals("RefDbName", newSharedRefDb, testStrategy.RefDbName);

				testStrategy.GetPhysicalDatabaseSettingsFromFirstRefDB_Exposed(upgConnection, out var dataPath, out var logPath);
				Assert(@"Get RefDB DBPath and LogPath from FirstRefDB", !string.IsNullOrEmpty(dataPath) && !string.IsNullOrEmpty(logPath));
			}
		}

		public void TestReferenceDatabasePaths_MainDb()
		{
			var mainDb = "SharedAGRefDb";
			var newSharedRefDb = "CW-AG-RefDb-ORDWP4-CP1AS1-Cmr-ZZ-000902";

			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, mainDb, mainDb))
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = new StrategyForTest(upgradeContext, upgConnection, mainDb, 902);
				AssertEquals("RefDbName", newSharedRefDb, testStrategy.RefDbName);

				testStrategy.GetPhysicalDatabaseSettingsFromMainDB_Exposed(upgConnection, out var dataPath, out var logPath);
				Assert(@"Get RefDB DBPath and LogPath from MainDB", !string.IsNullOrEmpty(dataPath) && !string.IsNullOrEmpty(logPath));
			}
		}

		#region Implementation

		AdminConnection TestConnection;
		IUpgradeContext upgradeContext;

		void AssertPrepareAndUpgradeDatabaseWithNonexistentPreviousDb(string mainDb, string newSharedRefDb)
		{
			using (var upgConnection = Db.NewAdminConnection(mainDb))
			{
				var testStrategy = new StrategyForTest(upgradeContext, upgConnection, mainDb, 1);
				AssertEquals("RefDbName", newSharedRefDb, testStrategy.RefDbName);

				string actualUpgDb = null;
				((IRefDbPreparationStrategy)testStrategy).PrepareAndUpgradeDatabase(c => { actualUpgDb = c.CurrentDatabase; });

				// Candidate base database (testMockOldRefDb) does not exist => BaseDatabaseForUpgrade == ""
				AssertEquals("BaseDatabaseForUpgrade", "", testStrategy.BaseDatabaseForUpgrade_Exposed);

				AssertEquals("Upgrade runs on new reference db", newSharedRefDb, actualUpgDb);
				AssertEquals("Does new shared database exist?", true, upgConnection.DatabaseExists(newSharedRefDb));
			}
		}

		IDisposable TemporarySetExtendedPropertiesForFilePaths()
		{
			TestConnection.ExecuteNonQuery(@"
DECLARE 
	@IsValidPath     int = 1,
	@Command         nvarchar(300),
	@DataPath        nvarchar(300),
	@LogPath         nvarchar(300),
	@DefaultDataPath nvarchar(256) = CONVERT(nvarchar(256), SERVERPROPERTY('InstanceDefaultDataPath')),
	@DefaultLogPath  nvarchar(256) = CONVERT(nvarchar(256), SERVERPROPERTY('InstanceDefaultLogPath'))

if (RIGHT(@DefaultDataPath, 1) = N'\')
begin
	SET @DefaultDataPath = LEFT(@DefaultDataPath, LEN(@DefaultDataPath) - 1)
end

if (RIGHT(@DefaultLogPath, 1) = N'\')
begin
	SET @DefaultLogPath = LEFT(@DefaultLogPath, LEN(@DefaultLogPath) - 1)
end

SELECT @DataPath = CONVERT(nvarchar(max), value) FROM master.sys.extended_properties WHERE name = N'ReferenceDataFile'
SELECT @LogPath  = CONVERT(nvarchar(max), value) FROM master.sys.extended_properties WHERE name = N'ReferenceLogFile'

if (@DataPath is NOT NULL)
begin
	SET @Command = N'dir ' + @DataPath
	EXEC @IsValidPath = xp_cmdshell @Command

	if (@IsValidPath <> 0)
	begin
		if (EXISTS (SELECT NULL FROM master.sys.extended_properties WHERE name = N'ReferenceDataFile_temp'))
		begin
			EXEC master.sys.sp_updateextendedproperty @name = N'ReferenceDataFile_temp', @value = @DataPath
		end
		else
		begin
			EXEC master.sys.sp_addextendedproperty @name = N'ReferenceDataFile_temp', @value = @DataPath
		end

		EXEC master.sys.sp_updateextendedproperty @name = N'ReferenceDataFile', @value = @DefaultDataPath
	end
end
else
begin
	if (EXISTS (SELECT NULL FROM master.sys.extended_properties WHERE name = N'ReferenceDataFile_temp'))
	begin
		EXEC master.sys.sp_updateextendedproperty @name = N'ReferenceDataFile_temp', @value = NULL
	end
	else
	begin
		EXEC master.sys.sp_addextendedproperty @name = N'ReferenceDataFile_temp', @value = NULL
	end

	EXEC master.sys.sp_addextendedproperty @name = N'ReferenceDataFile', @value = @DefaultDataPath
end

if (@LogPath is NOT NULL)
begin
	SET @Command = N'dir ' + @DataPath
	EXEC @IsValidPath = xp_cmdshell @Command

	if (@IsValidPath <> 0)
	begin
		if (EXISTS (SELECT NULL FROM master.sys.extended_properties WHERE name = N'ReferenceLogFile_temp'))
		begin
			EXEC master.sys.sp_updateextendedproperty @name = N'ReferenceLogFile_temp', @value = @LogPath
		end
		else
		begin
			EXEC master.sys.sp_addextendedproperty @name = N'ReferenceLogFile_temp', @value = @LogPath
		end

		EXEC master.sys.sp_updateextendedproperty @name = N'ReferenceLogFile', @value = @DefaultLogPath
	end
end
else
begin
	if (EXISTS (SELECT NULL FROM master.sys.extended_properties WHERE name = N'ReferenceLogFile_temp'))
	begin
		EXEC master.sys.sp_updateextendedproperty @name = N'ReferenceLogFile_temp', @value = NULL
	end
	else
	begin
		EXEC master.sys.sp_addextendedproperty @name = N'ReferenceLogFile_temp', @value = NULL
	end

	EXEC master.sys.sp_addextendedproperty @name = N'ReferenceLogFile', @value = @DefaultLogPath
end

");

			return new DisposableAction(() =>
			{
				TestConnection.ExecuteNonQuery(@"
DECLARE 
	@DataPath nvarchar(300),
	@LogPath  nvarchar(300)

if (EXISTS (SELECT NULL FROM master.sys.extended_properties WHERE name = N'ReferenceDataFile_temp'))
begin
	SELECT @DataPath = CONVERT(nvarchar(max), value) FROM master.sys.extended_properties WHERE name = N'ReferenceDataFile_temp'

	EXEC master.sys.sp_dropextendedproperty @name = N'ReferenceDataFile_temp'

	if (@DataPath is NULL)
	begin
		if (EXISTS (SELECT NULL FROM master.sys.extended_properties WHERE name = N'ReferenceDataFile'))
		begin
			EXEC master.sys.sp_dropextendedproperty @name = N'ReferenceDataFile'
		end
	end
	else if (EXISTS (SELECT NULL FROM master.sys.extended_properties WHERE name = N'ReferenceDataFile'))
	begin
		EXEC master.sys.sp_updateextendedproperty @name = N'ReferenceDataFile', @value = @DataPath
	end
end

if (EXISTS (SELECT NULL FROM master.sys.extended_properties WHERE name = N'ReferenceLogFile_temp'))
begin
	SELECT @LogPath = CONVERT(nvarchar(max), value) FROM master.sys.extended_properties WHERE name = N'ReferenceLogFile_temp'

	EXEC master.sys.sp_dropextendedproperty @name = N'ReferenceLogFile_temp'

	if (@LogPath is NULL)
	begin
		if (EXISTS (SELECT NULL FROM master.sys.extended_properties WHERE name = N'ReferenceLogFile'))
		begin
			EXEC master.sys.sp_dropextendedproperty @name = N'ReferenceLogFile'
		end
	end
	else if (EXISTS (SELECT NULL FROM master.sys.extended_properties WHERE name = N'ReferenceLogFile'))
	begin
		EXEC master.sys.sp_updateextendedproperty @name = N'ReferenceLogFile', @value = @LogPath
	end
end

");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			upgradeContext = new Mock<IUpgradeContext>().Object;
			TestConnection = Db.NewAdminConnection();
		}

		protected override void TearDown()
		{
			TestConnection.Dispose();

			base.TearDown();
		}

		#endregion // Implementation
	}
}
