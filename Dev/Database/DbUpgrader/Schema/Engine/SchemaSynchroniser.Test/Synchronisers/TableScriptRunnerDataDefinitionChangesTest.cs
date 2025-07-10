using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class TableScriptRunnerDataDefinitionChangesTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		public void TestCreateNewTable()
		{
			AssertEquals("[PRE-CONDITION] IdentityColumnTable exists?", false, DbObjectCreator.TableExists(TestConnection, mockMainDb, "IdentityColumnTable"));
			new TableScriptRunner(TestConnection, mockMainDb, mockTemplateDb).CreateNewTable(Db.SqlDbOwnerSchema, "IdentityColumnTable");
			AssertTableCreated();
		}

		void AssertTableCreated()
		{
			AssertEquals("IdentityColumnTable exists?", true, DbObjectCreator.TableExists(TestConnection, mockMainDb, "IdentityColumnTable", Db.SqlDbOwnerSchema));
			AssertColumnExistInMockMainDb("IdentityColumnTable", "Col_Varchar", "VARCHAR", "YES", "5");
			AssertColumnExistInMockMainDb("IdentityColumnTable", "Col_Identity", "INT", "NO", null);
			AssertColumnExistInMockMainDb("IdentityColumnTable", "Col_DateTimeOffset", "DateTimeOffset", "No", "4");
			AssertIdentityMockMainDbIdentityColumnSeedAndIncrement("IdentityColumnTable", "Col_Identity", 2, 32);
		}

		public void TestRenameTable()
		{
			DbObjectCreatorTest.AssertTableExistCaseSensitive(TestConnection, mockMainDb, Db.SqlDbOwnerSchema, "MatcH_TableNameCaseDiff", expected: true);
			DbObjectCreatorTest.AssertTableExistCaseSensitive(TestConnection, mockMainDb, Db.SqlDbOwnerSchema, "Match_TableNameCaseDiff", expected: false);
			new TableScriptRunner(TestConnection, mockMainDb, mockTemplateDb).RenameTable(Db.SqlDbOwnerSchema, "MatcH_TableNameCaseDiff", "Match_TableNameCaseDiff");
			DbObjectCreatorTest.AssertTableExistCaseSensitive(TestConnection, mockMainDb, Db.SqlDbOwnerSchema, "MatcH_TableNameCaseDiff", expected: false);
			DbObjectCreatorTest.AssertTableExistCaseSensitive(TestConnection, mockMainDb, Db.SqlDbOwnerSchema, "Match_TableNameCaseDiff", expected: true);
		}

		public void TestDropReferencingFKs()
		{
			AssertFkExistsInTestMainDb("FK_XmlTypeWithPkChange_TO_XmlTypeIssues_01", expected: true);
			new TableScriptRunner(TestConnection, mockMainDb, mockTemplateDb).DropReferencingFKs(Db.SqlDbOwnerSchema, "XmlTypeIssues");
			AssertFkExistsInTestMainDb("FK_XmlTypeWithPkChange_TO_XmlTypeIssues_01", expected: false);
		}

		void AssertFkExistsInTestMainDb(string fkName, bool expected)
		{
			string sqlText = String.Format("IF EXISTS (SELECT null FROM [{0}].sys.foreign_keys WHERE name = '{1}') SELECT 1 ELSE SELECT 0", mockMainDb, fkName);
			AssertEquals(String.Format("FK {0} exists?", fkName), expected, Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText)));
		}

		public void TestDropPrimaryXmlIndexes()
		{
			AssertXmlIndexExistsInTestMainDb("IX_XmlTypeWithPkChange_01", expected: true);
			new TableScriptRunner(TestConnection, mockMainDb, mockTemplateDb).DropPrimaryXmlIndexes(Db.SqlDbOwnerSchema, "XmlTypeWithPkChange");
			AssertXmlIndexExistsInTestMainDb("IX_XmlTypeWithPkChange_01", expected: false);
		}

		void AssertXmlIndexExistsInTestMainDb(string indName, bool expected)
		{
			string sqlText = String.Format("IF EXISTS (SELECT null FROM [{0}].sys.indexes WHERE type = 3 AND name = '{1}') SELECT 1 ELSE SELECT 0", mockMainDb, indName);
			AssertEquals(String.Format("XML Index {0} exists?", indName), expected, Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText)));
		}

		public void TestDropSpatialIndexes()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(mockMainDb))
			{
				AssertEquals(true, SpatialIndexLoader.Load(TestConnection, null, "Spatial_AlterPK", null).Count > 0);
			}

			new TableScriptRunner(TestConnection, mockMainDb, mockTemplateDb).DropSpatialIndexes(Db.SqlDbOwnerSchema, "Spatial_AlterPK");

			using (((ICurrentDbControl)TestConnection).UseDatabase(mockMainDb))
			{
				AssertEquals(false, SpatialIndexLoader.Load(TestConnection, null, "Spatial_AlterPK", null).Count > 0);
			}
		}

		public void TestNewComputedColumns()
		{
			var tableName = "TableNewComputedColumns";

			using (((ICurrentDbControl)TestConnection).UseDatabase(mockTemplateDb))
			{
				TestConnection.ExecuteNonQuery($@"
CREATE TABLE dbo.{tableName}
(
	Col1     int NOT NULL,
	CompCol1 AS ISNULL(Col1 % 2, 0),
	CompCol2 AS ISNULL(Col1 % 2, 0) PERSISTED NOT NULL,
);

");
			}

			AssertEquals($"[PRE-CONDITION] {tableName} exists?", false, DbObjectCreator.TableExists(TestConnection, mockMainDb, tableName));

			new TableScriptRunner(TestConnection, mockMainDb, mockTemplateDb).CreateNewTable(Db.SqlDbOwnerSchema, tableName);

			AssertEquals($"{tableName} exists?", true, DbObjectCreator.TableExists(TestConnection, mockMainDb, tableName, Db.SqlDbOwnerSchema));
			AssertComputedColumnDefinition(mockMainDb, Db.SqlDbOwnerSchema, tableName, "CompCol1", "CompCol1 int NOT NULL NOT PERSISTED AS (isnull([Col1]%(2),(0)))");
			AssertComputedColumnDefinition(mockMainDb, Db.SqlDbOwnerSchema, tableName, "CompCol2", "CompCol2 int NOT NULL PERSISTED AS (isnull([Col1]%(2),(0)))");
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, createTestTemplateDbObjectsScript);
		}

		const string createTestTemplateDbObjectsScript = @"
			CREATE TABLE dbo.IdentityColumnTable
			( 
				Col_Identity INT IDENTITY(2, 32) NOT NULL,
				Col_Varchar VARCHAR(5) NULL,
				Col_DateTimeOffset datetimeoffset(4) Not NULL,
			);
			";
	}
}
