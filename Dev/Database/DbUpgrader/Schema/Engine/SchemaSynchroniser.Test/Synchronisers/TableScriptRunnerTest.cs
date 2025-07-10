using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class TableScriptRunnerTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		public void TestGetListOfTablesFromUnsupportedSchemas()
		{
			var testScriptRunner = new TableScriptRunner(TestConnection, mockMainDb, mockTemplateDb);
			var tablesFromUnsupportedSchemas = testScriptRunner.GetListOfTablesFromUnsupportedSchemas();

			AssertEquals("Number of tables in unsupported schemas", 1, tablesFromUnsupportedSchemas.Rows.Count);
			AssertEquals("Table(0) Schema", "TestSchemaOld", tablesFromUnsupportedSchemas.Rows[0]["SchemaName"].ToString());
			AssertEquals("Table(0) Name", "TestTable", tablesFromUnsupportedSchemas.Rows[0]["TableName"].ToString());
		}

		public void TestGetOldTablesDataTable()
		{
			var testScriptRunner = new TableScriptRunner(TestConnection, mockMainDb, mockTemplateDb);
			var oldTables = testScriptRunner.GetOldTablesDataTable();

			AssertEquals("Number of old tables", 2, oldTables.Rows.Count);
			AssertEquals("Table(0) Schema", "TestSchema01", oldTables.Rows[0]["SchemaName"].ToString());
			AssertEquals("Table(0) Name", "TableA", oldTables.Rows[0]["TableName"].ToString());
			AssertEquals("Table(1) Schema", "TestSchemaOld", oldTables.Rows[1]["SchemaName"].ToString());
			AssertEquals("Table(1) Name", "TestTable", oldTables.Rows[1]["TableName"].ToString());
		}

		public void TestGetNewTablesDataTable()
		{
			var testScriptRunner = new TableScriptRunner(TestConnection, mockMainDb, mockTemplateDb);
			var newTables = testScriptRunner.GetNewTablesDataTable();

			AssertEquals("Number of new tables", 1, newTables.Rows.Count);
			AssertEquals("Table(0) Schema", "TestSchema02", newTables.Rows[0]["SchemaName"].ToString());
			AssertEquals("Table(0) Name", "TableB", newTables.Rows[0]["TableName"].ToString());
		}

		public void TestGetModifiedTablesDataTable()
		{
			var testScriptRunner = new TableScriptRunner(TestConnection, mockMainDb, mockTemplateDb);
			var modifiedTables = testScriptRunner.GetModifiedTablesDataTable();

			AssertEquals("Number of modified tables", 5, modifiedTables.Rows.Count);
			AssertEquals("Table(0) Schema", "TestSchema01", modifiedTables.Rows[0]["SchemaName"].ToString());
			AssertEquals("Table(0) Name", "TABLEC", modifiedTables.Rows[0]["TableName"].ToString());
			AssertEquals("Table(1) Schema", "TestSchema01", modifiedTables.Rows[1]["SchemaName"].ToString());
			AssertEquals("Table(1) Name", "TableE", modifiedTables.Rows[1]["TableName"].ToString());
			AssertEquals("Table(2) Schema", "TestSchema01", modifiedTables.Rows[2]["SchemaName"].ToString());
			AssertEquals("Table(2) Name", "TableF", modifiedTables.Rows[2]["TableName"].ToString());
			AssertEquals("Table(3) Schema", "TestSchema02", modifiedTables.Rows[3]["SchemaName"].ToString());
			AssertEquals("Table(3) Name", "TableD", modifiedTables.Rows[3]["TableName"].ToString());
			AssertEquals("Table(4) Schema", "TestSchema02", modifiedTables.Rows[4]["SchemaName"].ToString());
			AssertEquals("Table(4) Name", "TableG", modifiedTables.Rows[4]["TableName"].ToString());
		}

		public void TestGetTablesWhichAllExistingColumnsAreBeingRemovedDataTable()
		{
			var testScriptRunner = new TableScriptRunner(TestConnection, mockMainDb, mockTemplateDb);
			var actualResultTable = testScriptRunner.GetTablesWhichAllExistingColumnsAreBeingRemovedDataTable();

			AssertEquals("Number of tables which all existing columns are being removed", 1, actualResultTable.Rows.Count);
			AssertEquals("Table(0) Schema", "TestSchema01", actualResultTable.Rows[0]["SchemaName"].ToString());
			AssertEquals("Table(0) Name", "TableF", actualResultTable.Rows[0]["TableName"].ToString());
		}

		public void TestGetTablesToFixCaseDataTable()
		{
			var testScriptRunner = new TableScriptRunner(TestConnection, mockMainDb, mockTemplateDb);
			var tablesToFixCase = testScriptRunner.GetTablesToFixCaseDataTable();

			AssertEquals("Number of tables to fix case", 1, tablesToFixCase.Rows.Count);
			AssertEquals("Table Schema", "TestSchema01", tablesToFixCase.Rows[0]["SchemaName"].ToString());
			AssertEquals("Old Table Name", "TableC", tablesToFixCase.Rows[0]["OldTableName"].ToString());
			AssertEquals("New Table Name", "TABLEC", tablesToFixCase.Rows[0]["NewTableName"].ToString());
		}

		#region Implementation

		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockMainDb, createTestMainDbObjectsScript);
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, createTestTemplateDbObjectsScript);
		}

		#region Scripts

		const string createTestMainDbObjectsScript = @"
			EXEC ('CREATE SCHEMA [TestSchema01]');
			CREATE TABLE [TestSchema01].[TableA] (Col1 INT);
			CREATE TABLE [TestSchema01].[TableB] (Col1 INT);
			CREATE TABLE [TestSchema01].[TableC] (Col1 INT);
			CREATE TABLE [TestSchema01].[TableE] (Col1 INT);
			CREATE TABLE [TestSchema01].[TableF] (Col1 INT, Col2 INT);

			EXEC ('CREATE SCHEMA [TestSchema02]');
			CREATE TABLE [TestSchema02].[TableA] (Col1 INT);
			CREATE TABLE [TestSchema02].[TableC] (Col1 INT);
			CREATE TABLE [TestSchema02].[TableD] (Col1 INT);
			CREATE TABLE [TestSchema02].[TableG] (Col1 INT, [CW!!PreserveMe] BIT);

			EXEC ('CREATE SCHEMA [TestSchemaOld]');
			CREATE TABLE [TestSchemaOld].[TestTable] (Col1 INT);
			";

		const string createTestTemplateDbObjectsScript = @"
			EXEC ('CREATE SCHEMA [TestSchema01]');
			CREATE TABLE [TestSchema01].[TableB] (Col1 INT);
			CREATE TABLE [TestSchema01].[TABLEC] (Col1 INT);
			CREATE TABLE [TestSchema01].[TableE] (Col1 BIT);
			CREATE TABLE [TestSchema01].[TableF] (Col3 INT);

			EXEC ('CREATE SCHEMA [TestSchema02]');
			CREATE TABLE [TestSchema02].[TableA] (Col1 INT);
			CREATE TABLE [TestSchema02].[TableB] (Col1 INT);
			CREATE TABLE [TestSchema02].[TableC] (Col1 INT);
			CREATE TABLE [TestSchema02].[TableD] (COL1 INT);
			CREATE TABLE [TestSchema02].[TableG] (Col2 INT);
			";

		#endregion

		#endregion
	}
}
