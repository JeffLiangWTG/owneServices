using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.DbUpgrader.Shared.DbObjectCreator;

namespace Enterprise.DbUpgrader.Shared
{
	public sealed class DbObjectCreatorTest : TransactionedTestCase
	{
		public void TestGetParameterCountOfStoredPrecedure()
		{
			Db.Connection.ExecuteNonQuery(@"CREATE PROCEDURE sp_TEST_GetParameterCountOfStoredPrecedure
										@Para1 CHAR(5), 
										@Para2 CHAR(5) 
									AS
									SELECT top 1 * FROM sys.types
									");
			AssertEquals("Count", 2, DbObjectCreator.GetParameterCountOfStoredPrecedure(TestConnection, "sp_TEST_GetParameterCountOfStoredPrecedure"));
		}

		public void TestParameterOfStoredPrecedureExists()
		{
			Db.Connection.ExecuteNonQuery(@"CREATE PROCEDURE sp_TEST_ParameterOfStoredPrecedureExists
										@Para CHAR(5) 
									AS
									SELECT top 1 * FROM sys.types
									");
			AssertEquals("@Para exist", true, DbObjectCreator.ParameterOfStoredPrecedureExists(TestConnection, "sp_TEST_ParameterOfStoredPrecedureExists", "@Para"));
			AssertEquals("@Para1 NOt exist", false, DbObjectCreator.ParameterOfStoredPrecedureExists(TestConnection, "sp_TEST_ParameterOfStoredPrecedureExists", "@Para1"));
		}

		public void TestParameterOfStoredPrecedureExistsWithCorrectTypeAndMaxLengh()
		{
			Db.Connection.ExecuteNonQuery(@"CREATE PROCEDURE sp_TEST_ParameterOfStoredPrecedureExistsWithCorrectTypeAndMaxLengh
										@Para CHAR(5) 
									AS
									SELECT top 1 * FROM sys.types
									");
			AssertEquals("@Para exist", true, DbObjectCreator.ParameterOfStoredPrecedureExistsWithCorrectTypeAndMaxLengh(TestConnection, "sp_TEST_ParameterOfStoredPrecedureExistsWithCorrectTypeAndMaxLengh", "@Para", "CHAR", 5));
			AssertEquals("@Para NOt exist with unmatch type", false, DbObjectCreator.ParameterOfStoredPrecedureExistsWithCorrectTypeAndMaxLengh(TestConnection, "sp_TEST_ParameterOfStoredPrecedureExistsWithCorrectTypeAndMaxLengh", "@Para", "VARCHAR", 5));
			AssertEquals("@Para NOt exist with unmatch max length", false, DbObjectCreator.ParameterOfStoredPrecedureExistsWithCorrectTypeAndMaxLengh(TestConnection, "sp_TEST_ParameterOfStoredPrecedureExistsWithCorrectTypeAndMaxLengh", "@Para", "CHAR", 3));
			AssertEquals("@Para1 NOt exist", false, DbObjectCreator.ParameterOfStoredPrecedureExistsWithCorrectTypeAndMaxLengh(TestConnection, "sp_TEST_ParameterOfStoredPrecedureExistsWithCorrectTypeAndMaxLengh", "@Para1", "CHAR", 5));
		}

		public void TestDatabaseExists()
		{
			AssertEquals(string.Format("DB [{0}] should exist", Db.DatabaseName), true, DbObjectCreator.DatabaseExists(TestConnection, Db.DatabaseName));
			AssertEquals("DB [DbObjectsCreatorTest_NonexistingDb] should NOT exist", false, DbObjectCreator.DatabaseExists(TestConnection, "DbObjectsCreatorTest_NonexistingDb"));
		}

		public void TestTableExists()
		{
			AssertEquals("<main_db>.GlbStaff exists?", true, DbObjectCreator.TableExists(TestConnection, Db.DatabaseName, GlbStaffSchema.Constants.TableName, Db.SqlDbOwnerSchema));
			AssertEquals("<main_db>.NonExistingTable exists?", false, DbObjectCreator.TableExists(TestConnection, Db.DatabaseName, "NonExistingTable"));
			AssertEquals("<current_db>.GlbCompany exists?", true, DbObjectCreator.TableExists(TestConnection, GlbCompanySchema.Constants.TableName));
			AssertEquals("<current_db>.AnotherIvalidTable exists?", false, DbObjectCreator.TableExists(TestConnection, "AnotherIvalidTable"));
			AssertEquals("NonExistingDb.AnyTable exists?", false, DbObjectCreator.TableExists(TestConnection, "NonExistingDb", "AnyTable"));

			var edocsDbList = TestConnection.GetDatabases(DatabaseType.SD);

			if (edocsDbList.Any())
			{
				AssertEquals("<edocs_db>.StorageDocs exists?", true, DbObjectCreator.TableExists(TestConnection, edocsDbList.First(), StorageDocsSchema.Constants.TableName, null));
			}
		}

		public void TestCreateTableIfNotExists()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.SqlMasterDb))
			{
				string nonexistingTable = "DbObjectsCreatorTest_NonexistingTable";

				AssertEquals(string.Format("Table should NOT exist on {0}", Db.DatabaseName), false, DbObjectCreator.TableExists(TestConnection, Db.DatabaseName, nonexistingTable));
				AssertEquals(string.Format("Table should NOT exist on {0}", Db.SqlMasterDb), false, DbObjectCreator.TableExists(TestConnection, Db.SqlMasterDb, nonexistingTable));

				DbObjectCreator.CreateTableIfNotExists(TestConnection, Db.DatabaseName, nonexistingTable, String.Format("CREATE TABLE {0} (col1 int)", nonexistingTable));

				AssertEquals(string.Format("Table should exist on {0}", Db.DatabaseName), true, DbObjectCreator.TableExists(TestConnection, Db.DatabaseName, nonexistingTable));
				AssertEquals(string.Format("Table should NOT exist on {0}", Db.SqlMasterDb), false, DbObjectCreator.TableExists(TestConnection, Db.SqlMasterDb, nonexistingTable));

				var tableSchemaName = "_TEST_";
				using (((ICurrentDbControl)TestConnection).UseDatabase(Db.DatabaseName))
				{
					TestConnection.ExecuteNonQuery($@"CREATE SCHEMA [{tableSchemaName}];");
				}
				DbObjectCreator.CreateTableIfNotExists(TestConnection, Db.DatabaseName, tableSchemaName, nonexistingTable, $"CREATE TABLE [{tableSchemaName}].[{nonexistingTable}] (col1 int)");
				AssertEquals(string.Format("Table should exist on {0}", Db.DatabaseName), true, DbObjectCreator.TableExists(TestConnection, Db.DatabaseName, nonexistingTable, tableSchema: tableSchemaName));
			}
		}

		public void TestDropTableIfExists()
		{
			string tableToTest = "DbObjectsCreatorTestDropTableIfExists";
			AssertEquals("Table should NOT exist before create", false, DbObjectCreator.TableExists(TestConnection, Db.DatabaseName, tableToTest));

			DbObjectCreator.CreateTableIfNotExists(TestConnection, Db.DatabaseName, tableToTest, String.Format("CREATE TABLE {0} (col1 int)", tableToTest));
			AssertEquals("Table should exist after create", true, DbObjectCreator.TableExists(TestConnection, Db.DatabaseName, tableToTest));

			DbObjectCreator.DropTableIfExists(TestConnection, Db.DatabaseName, tableToTest);
			AssertEquals("Table should NOT exist after drop", false, DbObjectCreator.TableExists(TestConnection, Db.DatabaseName, tableToTest));
		}

		public void TestDeleteAllRecordsIfTableExists()
		{
			string tableToTest = "DbObjectsCreatorTestDeleteAllRecordsIfTableExists";

			DbObjectCreator.CreateTableIfNotExists(TestConnection, Db.DatabaseName, tableToTest, string.Format("CREATE TABLE {0} (col1 int)", tableToTest));
			Db.Connection.ExecuteNonQuery($"INSERT INTO {tableToTest} (col1) VALUES (5), (6)");
			AssertEquals("Table should exist after create", true, DbObjectCreator.TableExists(TestConnection, Db.DatabaseName, tableToTest));
			AssertEquals("Table should has 2 records", 2, Db.Connection.ExecuteScalar($"select count(*) from {tableToTest}"));

			DbObjectCreator.DeleteAllRecordsIfTableExists(TestConnection, tableToTest);
			AssertEquals("Table should has  records", 0, Db.Connection.ExecuteScalar($"select count(*) from {tableToTest}"));
		}

		public void TestRenameTable()
		{
			const string oldTable = "TestRenameTable_Table";
			const string newTable = "TestRenameTable_TABLE";

			AssertEquals("[PRE-CONDITION]" + oldTable + " exists?", false, DbObjectCreator.TableExists(TestConnection, Db.DatabaseName, oldTable, Db.SqlDbOwnerSchema));
			AssertEquals("[PRE-CONDITION]" + newTable + " exists?", false, DbObjectCreator.TableExists(TestConnection, Db.DatabaseName, newTable, Db.SqlDbOwnerSchema));

			TestConnection.ExecuteNonQuery(String.Format("CREATE TABLE {0} (col1 int)", oldTable));

			AssertEquals(oldTable + " exists?", true, DbObjectCreator.TableExists(TestConnection, Db.DatabaseName, oldTable, Db.SqlDbOwnerSchema));
			AssertTableExistCaseSensitive(TestConnection, Db.DatabaseName, Db.SqlDbOwnerSchema, oldTable, expected: true);
			AssertTableExistCaseSensitive(TestConnection, Db.DatabaseName, Db.SqlDbOwnerSchema, newTable, expected: false);

			DbObjectCreator.RenameTable(TestConnection, Db.DatabaseName, Db.SqlDbOwnerSchema, oldTable, newTable);

			AssertEquals(newTable + " exists?", true, DbObjectCreator.TableExists(TestConnection, Db.DatabaseName, newTable, Db.SqlDbOwnerSchema));
			AssertTableExistCaseSensitive(TestConnection, Db.DatabaseName, Db.SqlDbOwnerSchema, oldTable, expected: false);
			AssertTableExistCaseSensitive(TestConnection, Db.DatabaseName, Db.SqlDbOwnerSchema, newTable, expected: true);
		}

		public void TestGetColumnType()
		{
			string nonExistingTable = "This_Got_To_Be_A_Non_Existing_Table_5B031E89611748C285794278AE95F45E";
			string nonExistingColumn = "This_Got_To_Be_A_Non_Existing_Column_5B031E89611748C285794278AE95F45E";
			AssertGetColumnType(nonExistingTable, nonExistingColumn, null);
			AssertGetColumnType(GlbBranchSchema.Constants.TableName, nonExistingColumn, null);
			AssertGetColumnType(nonExistingTable, GlbBranchSchema.Constants.PK, null);
			AssertGetColumnType(GlbBranchSchema.Constants.TableName, GlbBranchSchema.Constants.PK, "uniqueidentifier");
			AssertGetColumnType(GlbBranchSchema.Constants.TableName, GlbBranchSchema.Constants.GB_Code, "char");
		}

		public void TestGetColumnTypeAndMaxLength()
		{
			string nonExistingTable = "This_Got_To_Be_A_Non_Existing_Table_5B031E89611748C285794278AE95F45E";
			string nonExistingColumn = "This_Got_To_Be_A_Non_Existing_Column_5B031E89611748C285794278AE95F45E";
			AssertGetColumnTypeAndMaxLength(nonExistingTable, nonExistingColumn, (null, 0));
			AssertGetColumnTypeAndMaxLength(GlbBranchSchema.Constants.TableName, nonExistingColumn, (null, 0));
			AssertGetColumnTypeAndMaxLength(nonExistingTable, GlbBranchSchema.Constants.PK, (null, 0));
			AssertGetColumnTypeAndMaxLength(GlbBranchSchema.Constants.TableName, GlbBranchSchema.Constants.PK, ("uniqueidentifier", 16));
			AssertGetColumnTypeAndMaxLength(GlbBranchSchema.Constants.TableName, GlbBranchSchema.Constants.GB_Code, ("char", 3));
		}

		public void TestGetTableColumns()
		{
			Db.Connection.ExecuteNonQuery(@"CREATE TABLE TestTable_GetTableColumns (col1 int, col2 int)");
			var columns = DbObjectCreator.GetTableColumns(Db.Connection, "TestTable_GetTableColumns");
			AssertContainsExactElementsInAnyOrder(new[] { "col1", "col2" }, columns);
		}

		public void TestGetExtendedTableColumns()
		{
			Db.Connection.ExecuteNonQuery(@"CREATE TABLE TestTable_GetExtendedTableColumns (pk UNIQUEIDENTIFIER, name VARCHAR(35))");
			var columns = DbObjectCreator.GetExtendedTableColumns(Db.Connection, "TestTable_GetExtendedTableColumns");
			AssertContainsExactElementsInAnyOrder(new[]
			{
				new DbColumn("pk", "uniqueidentifier", -1),
				new DbColumn("name", "varchar", 35)
			}, columns);
		}

		public void TestGetViewColumns()
		{
			Db.Connection.ExecuteNonQuery(@"CREATE TABLE TestTable_GetViewColumns (col1 int, col2 int)");
			Db.Connection.ExecuteNonQuery(@"CREATE VIEW TestView_GetViewColumns AS SELECT col1, col2 FROM TestTable_GetViewColumns");
			var columns = DbObjectCreator.GetViewColumns(Db.Connection, "TestView_GetViewColumns");
			AssertContainsExactElementsInAnyOrder(new[] { "col1", "col2" }, columns);
		}

		public void TestGetExtendedViewColumns()
		{
			Db.Connection.ExecuteNonQuery(@"CREATE TABLE TestTable_GetExtendedViewColumns (pk UNIQUEIDENTIFIER, name VARCHAR(35))");
			Db.Connection.Command(@"CREATE VIEW TestView_TgetExtendedViewColumns AS SELECT pk, name FROM TestTable_GetExtendedViewColumns");
			var columns = DbObjectCreator.GetExtendedViewColumns(Db.Connection, "TestTable_GetExtendedViewColumns");
			AssertContainsExactElementsInAnyOrder(new[]
			{
				new DbColumn("pk", "uniqueidentifier", -1),
				new DbColumn("name", "varchar", 35)
			}, columns);
		}

		public void TestColumnExists_InOtherSchema()
		{
			const string tablename = "test";
			const string columnName = "col1";

			TestConnection.ExecuteNonQuery("CREATE SCHEMA sc1");
			TestConnection.ExecuteNonQuery("CREATE SCHEMA sc2");

			var tableInSchema1 = new TableDescriptor("sc1", tablename);
			var tableInSchema2 = new TableDescriptor("sc2", tablename);

			CreateTable(TestConnection, "sc1", tablename, columnName);
			CombineAssertions("When Column & Table exists in schema #1, but not in schema #2", () =>
			{
				Assert("ColumnExists should return true for schema #1", DbObjectCreator.ColumnExists(TestConnection, tableInSchema1, columnName));
				Assert("ColumnExists should return false for schema #2", !DbObjectCreator.ColumnExists(TestConnection, tableInSchema2, columnName));
			});

			CreateTable(TestConnection, "sc2", tablename, columnName);
			CombineAssertions("When identical column & Table exists in both schema #1 and schema #2", () =>
			{
				Assert("ColumnExists should return true for schema #1", DbObjectCreator.ColumnExists(TestConnection, tableInSchema1, columnName));
				Assert("ColumnExists should return true for schema #2", DbObjectCreator.ColumnExists(TestConnection, tableInSchema2, columnName));
			});
		}

		static void CreateTable(DbConnection connection, string schema, string table, string column)
			=> connection.ExecuteNonQuery(FormattableString.Invariant($"CREATE TABLE {schema}.{table} ({column} int)"));

		public void TestColumnExists()
		{
			AssertColumnExists(GlbBranchSchema.Constants.TableName, GlbBranchSchema.Constants.PK, AssertColumnExist, AssertColumnExist);
		}

		public void TestViewColumnExists()
		{
			AssertColumnExists(ZZRefCarrierCombinedSchema.Constants.TableName, ZZRefCarrierCombinedSchema.Constants.PK, AssertViewColumnExist, AssertViewColumnExist);
		}

		void AssertColumnExists(string objectName, string columnName, Action<string, string, bool> assertWithoutDb, Action<DbConnection, bool, string, string, string, string> assertWithDb)
		{
			string nonExistingObject = "This_Got_To_Be_A_Non_Existing_Object_5B031E89611748C285794278AE95F45E";
			string nonExistingColumn = "This_Got_To_Be_A_Non_Existing_Column_5B031E89611748C285794278AE95F45E";
			assertWithoutDb(nonExistingObject, nonExistingColumn, false);
			assertWithoutDb(objectName, nonExistingColumn, false);
			assertWithoutDb(nonExistingObject, columnName, false);
			assertWithoutDb(objectName, columnName, true);

			var nonExistingDatabase = "This_Got_To_Be_A_Non_Existing_Database_5B031E89611748C285794278AE95F45E";
			var nonExistingSchema = "This_Got_To_Be_A_Non_Existing_Schema_5B031E89611748C285794278AE95F45E";
			var dbName = Db.DatabaseName;
			var schemaName = Db.SqlDbOwnerSchema;

			using (var conn = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				assertWithDb(conn, false, nonExistingDatabase, nonExistingSchema, nonExistingObject, nonExistingColumn);
				assertWithDb(conn, false, nonExistingDatabase, nonExistingSchema, nonExistingObject, columnName);
				assertWithDb(conn, false, nonExistingDatabase, nonExistingSchema, objectName, columnName);
				assertWithDb(conn, false, nonExistingDatabase, schemaName, objectName, columnName);
				assertWithDb(conn, true, dbName, schemaName, objectName, columnName);
			}
		}

		public void TestColumnsExist()
		{
			Db.Connection.ExecuteNonQuery(@"CREATE TABLE dbo.TestTable_ColumnsExist (pk UNIQUEIDENTIFIER, ColumnName1 VARCHAR(35), ColumnName2 VARCHAR(35), ColumnName3 DATETIME)");

			Assert(ColumnsExist(Db.Connection, Db.Connection.CurrentDatabase, "dbo", "TestTable_ColumnsExist", new[] {
				"ColumnName1", "ColumnName2", "ColumnName3"
			}));
			Assert(!ColumnsExist(Db.Connection, Db.Connection.CurrentDatabase, "dbo", "TestTable_ColumnsExist", new[] {
				"ColumnName1", "ColumnName2", "ColumnName4"
			}));
		}

		public void TestIndexColumnExists()
		{
			Db.Connection.ExecuteNonQuery(@"CREATE TABLE TestTable_GetIndexIncludeColumns (pk UNIQUEIDENTIFIER, ColumnName1 VARCHAR(35), ColumnName2 VARCHAR(35), ColumnName3 DATETIME)");
			Db.Connection.ExecuteNonQuery(@"CREATE NONCLUSTERED INDEX TestIndex1 ON TestTable_GetIndexIncludeColumns(ColumnName1, ColumnName2)");
			Db.Connection.ExecuteNonQuery(@"CREATE NONCLUSTERED INDEX TestIndex2 ON TestTable_GetIndexIncludeColumns(ColumnName1) INCLUDE (ColumnName2, ColumnName3)");

			Assert(DbObjectCreator.IndexColumnExists(Db.Connection, "TestTable_GetIndexIncludeColumns", "TestIndex1", 0, "ColumnName1"));
			Assert(DbObjectCreator.IndexColumnExists(Db.Connection, "TestTable_GetIndexIncludeColumns", "TestIndex1", 0, "ColumnName2"));
			Assert(!DbObjectCreator.IndexColumnExists(Db.Connection, "TestTable_GetIndexIncludeColumns", "TestIndex1", 1, "ColumnName3"));

			Assert(!DbObjectCreator.IndexColumnExists(Db.Connection, "TestTable_GetIndexIncludeColumns", "TestIndex2", 0, "ColumnName2"));
			Assert(DbObjectCreator.IndexColumnExists(Db.Connection, "TestTable_GetIndexIncludeColumns", "TestIndex2", 1, "ColumnName2"));
			Assert(DbObjectCreator.IndexColumnExists(Db.Connection, "TestTable_GetIndexIncludeColumns", "TestIndex2", 1, "ColumnName3"));
		}

		public void TestComputedColumnExists()
		{
			Db.Connection.ExecuteNonQuery(@"CREATE TABLE dbo.TestTable_ComputedColumnExists (pk UNIQUEIDENTIFIER, ColumnName1 VARCHAR(35), ColumnName2 VARCHAR(35), ColumnName3 AS CONCAT(ColumnName1, ColumnName2))");

			Assert(ComputedColumnExists(Db.Connection, "dbo", "TestTable_ComputedColumnExists", "ColumnName3"));
			Assert(!ComputedColumnExists(Db.Connection, "dbo", "TestTable_ComputedColumnExists", "ColumnName2"));
			Assert(!ComputedColumnExists(Db.Connection, "dbo", "TestTable_ComputedColumnExists", "ColumnName1"));
			Assert(!ComputedColumnExists(Db.Connection, "dbo", "TestTable_ComputedColumnExists", "ColumnName0"));
		}

		[UseSnapshotProtection]
		public void TestCreateColumn()
		{
			string nonExistingTable = "TestTableExists_NonExistingTable";
			string nonExistingColumn = "TestTableExists_NonExistingColumn";

			Db.Connection.ExecuteNonQuery(String.Format("CREATE TABLE {0} (col1 int)", nonExistingTable));

			AssertEquals("Column should NOT exist", false, DbObjectCreator.ColumnExists(TestConnection, nonExistingTable, nonExistingColumn));
			DbObjectCreator.CreateColumn(TestConnection, nonExistingTable, nonExistingColumn, "int");
			AssertEquals("Column should exist", true, DbObjectCreator.ColumnExists(TestConnection, nonExistingTable, nonExistingColumn));
		}

		[UseSnapshotProtection]
		public void TestCreateColumnWithSchema()
		{
			string schema = "test";
			string nonExistingTable = "TestTableExists_NonExistingTable";
			string nonExistingColumn = "TestTableExists_NonExistingColumn";

			using (var admin = Db.NewAdminConnection())
			{
				admin.ExecuteNonQuery($"CREATE SCHEMA {schema}");
				admin.ExecuteNonQuery($"CREATE TABLE {schema}.{nonExistingTable} (col1 int);");

				var descriptor = new TableDescriptor(schema, nonExistingTable);
				AssertEquals("Column should NOT exist", false, DbObjectCreator.ColumnExists(admin, descriptor, nonExistingColumn));
				DbObjectCreator.CreateColumn(admin, descriptor, nonExistingColumn, "int");
				AssertEquals("Column should exist", true, DbObjectCreator.ColumnExists(admin, descriptor, nonExistingColumn));
			}
		}

		[UseSnapshotProtection]
		public void TestCreateColumnIfNotExists()
		{
			string nonExistingTable = "TestTableExists_NonExistingTable";
			string nonExistingColumn = "TestTableExists_NonExistingColumn";

			AssertEquals("Should NOT be able to create column as the table doesn't exist", false, DbObjectCreator.CreateColumnIfNotExists(TestConnection, nonExistingTable, nonExistingColumn, "int"));
			AssertEquals("Column should NOT exist as table doesn't exist", false, DbObjectCreator.ColumnExists(TestConnection, nonExistingTable, nonExistingColumn));

			Db.Connection.ExecuteNonQuery(String.Format("CREATE TABLE {0} (col1 int)", nonExistingTable));
			AssertEquals("Column should still NOT exist", false, DbObjectCreator.ColumnExists(TestConnection, nonExistingTable, nonExistingColumn));

			AssertEquals("Should be able to create column", true, DbObjectCreator.CreateColumnIfNotExists(TestConnection, nonExistingTable, nonExistingColumn, "int"));
			AssertEquals("Column should exist", true, DbObjectCreator.ColumnExists(TestConnection, nonExistingTable, nonExistingColumn));
		}

		[UseSnapshotProtection]
		public void TestCreateColumnIfNotExistsWithSchema()
		{
			string schema = "test";
			string nonExistingTable = "TestTableExists_NonExistingTable";
			string nonExistingColumn = "TestTableExists_NonExistingColumn";

			using (var admin = Db.NewAdminConnection())
			{
				AssertEquals("Should NOT be able to create column as the table doesn't exist", false, DbObjectCreator.CreateColumnIfNotExists(admin, nonExistingTable, nonExistingColumn, "int"));
				AssertEquals("Column should NOT exist as table doesn't exist", false, DbObjectCreator.ColumnExists(admin, nonExistingTable, nonExistingColumn));

				admin.ExecuteNonQuery($"CREATE SCHEMA {schema}");
				admin.ExecuteNonQuery($"CREATE TABLE {schema}.{nonExistingTable} (col1 int);");

				var descriptor = new TableDescriptor(schema, nonExistingTable);
				AssertEquals("Column should still NOT exist", false, DbObjectCreator.ColumnExists(admin, descriptor, nonExistingColumn));
				AssertEquals("Should be able to create column", true, DbObjectCreator.CreateColumnIfNotExists(admin, descriptor, nonExistingColumn, "int"));
				AssertEquals("Column should exist", true, DbObjectCreator.ColumnExists(admin, descriptor, nonExistingColumn));
			}
		}

		public void TestCreateForeignKeyIfNotExists()
		{
			var nonexistingTable = "DbObjectsCreatorTest_NonexistingTable";
			TestConnection.ExecuteNonQuery($"CREATE TABLE {nonexistingTable} (col1 varchar(2), col2 varchar(2))");
			var nonexistingTable2 = "DbObjectsCreatorTest_NonexistingTable2";
			TestConnection.ExecuteNonQuery($"CREATE TABLE {nonexistingTable2} (col2 varchar(2) primary key)");

			var nonexistingForeignKey = "DbObjectsCreatorTest_NonexistingForeignKey";
			AssertEquals("Foreign Key should NOT exist on DbObjectsCreatorTest_NonexistingTable", false, DbObjectCreator.ForeignKeyExists(TestConnection, nonexistingTable, nonexistingForeignKey));

			DbObjectCreator.CreateForeignKeyIfNotExists(TestConnection, nonexistingTable, nonexistingForeignKey, $"ALTER TABLE {nonexistingTable} ADD CONSTRAINT {nonexistingForeignKey} FOREIGN KEY (col2) REFERENCES {nonexistingTable2} (col2);");
			AssertEquals("Foreign Key should exist on DbObjectsCreatorTest_NonexistingTable", true, DbObjectCreator.ForeignKeyExists(TestConnection, nonexistingTable, nonexistingForeignKey));

			AssertNoExceptionThrown(() => DbObjectCreator.CreateForeignKeyIfNotExists(TestConnection, nonexistingTable, nonexistingForeignKey, $"ALTER TABLE {nonexistingTable} ADD CONSTRAINT {nonexistingForeignKey} FOREIGN KEY (col2) REFERENCES {nonexistingTable2} (ZZZ!!!!);"));
			AssertEquals("Foreign Key should exist on DbObjectsCreatorTest_NonexistingTable", true, DbObjectCreator.ForeignKeyExists(TestConnection, nonexistingTable, nonexistingForeignKey));
		}

		public void TestCreateIndexIfNotExists()
		{
			string nonexistingTable = "DbObjectsCreatorTest_NonexistingTable";
			DbObjectCreator.CreateTableIfNotExists(TestConnection, Db.DatabaseName, nonexistingTable, String.Format("CREATE TABLE {0} (col1 varchar(2))", nonexistingTable));

			string nonexistingIndex = "DbObjectsCreatorTest_NonexistingIndex";

			AssertEquals(string.Format("Index should NOT exist on {0}", Db.DatabaseName), false, DbObjectCreator.IndexExists(TestConnection, nonexistingTable, nonexistingIndex));

			DbObjectCreator.CreateIndexIfNotExists(TestConnection, Db.DatabaseName, nonexistingIndex, String.Format("CREATE INDEX {0} ON {1}(col1)", nonexistingIndex, nonexistingTable));

			AssertEquals(string.Format("Index should NOT exist on {0}", Db.DatabaseName), true, DbObjectCreator.IndexExists(TestConnection, nonexistingTable, nonexistingIndex));
		}

		public void TestTriggerExists()
		{
			string tableName = "TestTriggerExists_Table";
			DbObjectCreator.CreateTableIfNotExists(TestConnection, Db.DatabaseName, tableName, String.Format("CREATE TABLE {0} (col1 varchar(2))", tableName));

			string triggerName = "TestTriggerExists_Trigger";
			AssertEquals("Trigger should NOT exist", false, DbObjectCreator.TriggerExists(TestConnection, tableName, triggerName));

			string triggerCreateScript = $@"
CREATE TRIGGER {triggerName} ON {tableName}
FOR INSERT, UPDATE
AS
BEGIN
	print 'AAA'
END";
			using (var command = Db.Connection.Command(triggerCreateScript))
			{
				command.ExecuteNonQuery();
			}

			AssertEquals("Trigger should exist", true, DbObjectCreator.TriggerExists(TestConnection, tableName, triggerName));
		}

		public void TestGetInsteadOfDeleteTriggerName()
		{
			var tableName = "TestTriggerExists_Table";
			var tableSchemaName = "_TEST_";
			var triggerName = "TestTriggerExists_Trigger";
			AssertNull("No Instead OF DELETE Trigger", DbObjectCreator.GetInsteadOfDeleteTriggerName(TestConnection, tableSchemaName, tableName));
			TestConnection.ExecuteNonQuery($"CREATE SCHEMA [{tableSchemaName}];");
			TestConnection.ExecuteNonQuery($"CREATE TABLE [{tableSchemaName}].[{tableName}] (col1 varchar(2));");

			AssertNull("Still No Instead OF DELETE Trigger", DbObjectCreator.GetInsteadOfDeleteTriggerName(TestConnection, tableSchemaName, tableName));

			TestConnection.ExecuteNonQuery($@"
CREATE TRIGGER [{triggerName}] ON [{tableSchemaName}].[{tableName}]
INSTEAD OF DELETE
AS
BEGIN
	print 'AAA'
END");
			AssertEquals("Instead Of DELETE Trigger should exist", triggerName, DbObjectCreator.GetInsteadOfDeleteTriggerName(TestConnection, tableSchemaName, tableName));

			TestConnection.ExecuteNonQuery($"DROP TRIGGER [{tableSchemaName}].[{triggerName}];");
			TestConnection.ExecuteNonQuery($@"
CREATE TRIGGER [{triggerName}] ON [{tableSchemaName}].[{tableName}]
INSTEAD OF UPDATE, INSERT
AS
BEGIN
	print 'AAA'
END;");
			AssertNull("No Instead OF DELETE Trigger", DbObjectCreator.GetInsteadOfDeleteTriggerName(TestConnection, tableSchemaName, tableName));

			TestConnection.ExecuteNonQuery($@"
CREATE TRIGGER [{triggerName}1] ON [{tableSchemaName}].[{tableName}]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	print 'AAA'
END;");
			AssertNull("No Instead OF DELETE Trigger", DbObjectCreator.GetInsteadOfDeleteTriggerName(TestConnection, tableSchemaName, tableName));
		}

		public void TestDropTriggerIfExists()
		{
			var tableName = "TestTrigger_Table";
			CreateTableIfNotExists(TestConnection, Db.DatabaseName, tableName, String.Format("CREATE TABLE {0} (col1 varchar(2))", tableName));

			var triggerName = "TestTrigger_Trigger";
			AssertEquals("Trigger should NOT exist", false, TriggerExists(TestConnection, tableName, triggerName));

			var triggerCreateScript = $@"
CREATE TRIGGER {triggerName} ON {tableName}
FOR INSERT, UPDATE
AS
BEGIN
	print 'AAA'
END";

			using (var command = Db.Connection.Command(triggerCreateScript))
			{
				command.ExecuteNonQuery();
			}

			AssertEquals("Trigger should exist", true, TriggerExists(TestConnection, tableName, triggerName));

			DropTriggerIfExists(TestConnection, triggerName);

			AssertEquals("Trigger should exist", false, TriggerExists(TestConnection, tableName, triggerName));
		}

		public void TestViewExists()
		{
			string inexistingView = "TestViewExists_InexistingView";

			AssertEquals("View should NOT exist", false, DbObjectCreator.ViewExists(TestConnection, inexistingView));

			using (var command = Db.Connection.Command(string.Format("CREATE VIEW {0} AS (SELECT 0 AS Col1);", inexistingView)))
			{
				command.ExecuteNonQuery();
			}

			AssertEquals("View should exist", true, DbObjectCreator.ViewExists(TestConnection, inexistingView));
		}

		public void TestRenameColumn()
		{
			const string testTable = "TestRenameColumn_Table";
			const string oldColumn = "TestRenameColumn_OldColumn";
			const string newColumn = "TestRenameColumn_NewColumn";

			TestConnection.ExecuteNonQuery(String.Format("CREATE TABLE {0} (col1 int)", testTable));
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, testTable, oldColumn, "char(2)", "'BC'");

			AssertEquals("Column should be renamed", true, DbObjectCreator.RenameColumn(TestConnection, Db.SqlDbOwnerSchema, testTable, oldColumn, newColumn));
			AssertEquals("Old Column should NOT exist", false, DbObjectCreator.ColumnExists(TestConnection, testTable, oldColumn));
			AssertEquals("New Column should exist", true, DbObjectCreator.ColumnExists(TestConnection, testTable, newColumn));
		}

		public void TestRenameColumn_CaseDifferenceOnly()
		{
			const string testTable = "TestRenameColumn_CaseDifferenceOnly_Table";
			const string oldColumn = "TestRenameColumn_CaseDifferenceOnly_CoLuMn";
			const string newColumn = "TestRenameColumn_CaseDifferenceOnly_Column";

			TestConnection.ExecuteNonQuery(String.Format("CREATE TABLE {0} (col1 int)", testTable));
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, testTable, oldColumn, "char(2)", "'CS'");

			bool columnRenamingSuccessful = DbObjectCreator.RenameColumn(TestConnection, Db.SqlDbOwnerSchema, testTable, oldColumn, newColumn);
			AssertEquals("Column with case difference renamed:", true, columnRenamingSuccessful);

			// Case Insensitive check
			AssertColumnExist(testTable, oldColumn, expected: true);
			AssertColumnExist(testTable, newColumn, expected: true);

			// Case Sensitive check
			AssertColumnExistCaseSensitive(TestConnection, Db.DatabaseName, Db.SqlDbOwnerSchema, testTable, oldColumn, expected: false);
			AssertColumnExistCaseSensitive(TestConnection, Db.DatabaseName, Db.SqlDbOwnerSchema, testTable, newColumn, expected: true);
		}

		public void TestRenameDefaultColumnConstraintIfExists()
		{
			const string testTable = "TestRenameDefault";
			const string columnName = "col1";
			TestConnection.ExecuteNonQuery("CREATE SCHEMA sc1");
			TestConnection.ExecuteNonQuery($@"
CREATE TABLE sc1.{testTable} ({columnName} int CONSTRAINT DF_XXX DEFAULT 1);
CREATE TABLE dbo.{testTable} ({columnName} int CONSTRAINT DF_XXX DEFAULT 1);
");
			var newName = DbObjectCreator.GenerateDefaultColumnConstraintName(testTable, columnName);
			var renamingSuccessful = DbObjectCreator.RenameDefaultColumnConstraintIfExists(TestConnection, "sc1", testTable, columnName, newName);
			AssertEquals("ConstraintRenamingSuccessful", true, renamingSuccessful);
			AssertEquals("Default name", newName, GetDefaultColumnConstraintName("sc1", testTable, columnName));
			AssertEquals("Other schema is unchanged", "DF_XXX", GetDefaultColumnConstraintName("dbo", testTable, columnName));
		}

		public void TestRenameDefaultColumnConstraintIfExists_WhenProposedNameExists()
		{
			const string testTable = "TestRenameDefault";
			const string columnName = "col1";
			TestConnection.ExecuteNonQuery(
$@"CREATE TABLE dbo.{testTable} (
	{columnName} int CONSTRAINT DF_XXX DEFAULT 1,
	col2 int CONSTRAINT DF_{testTable}_{columnName} DEFAULT 2
)");
			var newName = DbObjectCreator.GenerateDefaultColumnConstraintName(testTable, columnName);
			var renamingSuccessful = DbObjectCreator.RenameDefaultColumnConstraintIfExists(TestConnection, Db.SqlDbOwnerSchema, testTable, columnName, newName);
			AssertEquals("columnRenamingSuccessful", false, renamingSuccessful);
			AssertEquals("Default name not changed", $"DF_XXX", GetDefaultColumnConstraintName("dbo", testTable, columnName));
		}

		public void TestRenameDefaultColumnConstraintIfExists_WhenNotExists()
		{
			const string testTable = "TestRenameDefault";
			const string columnName = "col1";
			TestConnection.ExecuteNonQuery(
$@"CREATE TABLE dbo.{testTable} (
	{columnName} int,
)");
			var newName = DbObjectCreator.GenerateDefaultColumnConstraintName(testTable, columnName);
			var renamingSuccessful = DbObjectCreator.RenameDefaultColumnConstraintIfExists(TestConnection, Db.SqlDbOwnerSchema, testTable, columnName, newName);
			AssertEquals("columnRenamingSuccessful", false, renamingSuccessful);
		}

		public void TestGenerateDefaultColumnConstraintName()
		{
			var testTableName = "TestRenameDefault";
			var longColumnName = "MyModestColumnHasAFairlyLongName";

			var expected = "DF_" + testTableName + "_" + longColumnName;
			AssertEquals("Default name", expected, GenerateDefaultColumnConstraintName(testTableName, longColumnName));
		}

		public void TestGenerateDefaultColumnConstraintName_IsTruncatedToSysnameLength()
		{
			var longTestTableName = "TestRenameDefault";
			longTestTableName += new String('X', 120 - longTestTableName.Length);
			var longColumnName = "MyModestColumnHasAFairlyLongName";

			var expected = "DF_" + longTestTableName + "_" + longColumnName;
			expected = expected.Substring(0, 128);
			AssertEquals("Default name", expected, GenerateDefaultColumnConstraintName(longTestTableName, longColumnName));
		}

		string GetDefaultColumnConstraintName(string schemaName, string tableName, string columnName)
			=> GetDefaultColumnConstraintName(TestConnection, schemaName, tableName, columnName);

		public static string GetDefaultColumnConstraintName(DbConnection connection, string schemaName, string tableName, string columnName)
		{
			var sql = FormattableString.Invariant($@"
SELECT sys.default_constraints.name 
FROM sys.default_constraints
JOIN sys.columns ON columns.column_id = default_constraints.parent_column_id AND columns.object_id = default_constraints.parent_object_id
JOIN sys.tables ON tables.object_id = columns.object_id
JOIN sys.schemas ON schemas.schema_id = tables.schema_id
WHERE schemas.name = @schemaName AND tables.name = @tableName AND columns.name = @columnName");

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@schemaName", SqlDbType.NVarChar, 128, schemaName);
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, tableName);
				cmd.AddParameter("@columnName", SqlDbType.NVarChar, 128, columnName);
				return cmd.ExecuteScalar() as string;
			}
		}

		void AssertColumnExist(string tableName, string columnName, bool expected)
		{
			string message = String.Format("Column exists [{0}.{1}]?", tableName, columnName);
			AssertEquals(message, expected, DbObjectCreator.ColumnExists(TestConnection, tableName, columnName));
		}

		void AssertColumnExist(DbConnection conn, bool expected, string dbName, string schemaName, string tableName, string columnName)
		{
			string message = String.Format("Column exists [{0}].[{1}].[{2}].[{3}]?", dbName, schemaName, tableName, columnName);
			AssertEquals(message, expected, DbObjectCreator.ColumnExists(conn, dbName, schemaName, tableName, columnName));
		}

		void AssertViewColumnExist(string viewName, string columnName, bool expected)
		{
			string message = String.Format("View Column exists [{0}.{1}]?", viewName, columnName);
			AssertEquals(message, expected, DbObjectCreator.ViewColumnExists(TestConnection, viewName, columnName));
		}

		void AssertViewColumnExist(DbConnection conn, bool expected, string dbName, string schemaName, string viewName, string columnName)
		{
			string message = String.Format("Column exists [{0}].[{1}].[{2}].[{3}]?", dbName, schemaName, viewName, columnName);
			AssertEquals(message, expected, DbObjectCreator.ViewColumnExists(conn, dbName, schemaName, viewName, columnName));
		}

		void AssertGetColumnType(string tableName, string columnName, string expected)
		{
			string message = String.Format("Column exists [{0}.{1}]?", tableName, columnName);
			AssertEquals(message, expected, DbObjectCreator.GetColumnType(TestConnection, tableName, columnName));
		}

		void AssertGetColumnTypeAndMaxLength(string tableName, string columnName, (string, short) expected)
		{
			string message = String.Format("Column exists [{0}.{1}]?", tableName, columnName);
			AssertEquals(message, expected, DbObjectCreator.GetColumnTypeAndMaxLength(TestConnection, tableName, columnName));
		}

		public static void AssertTableExistCaseSensitive(DbConnection connection, string dbName, string schemaName, string tableName, bool expected)
		{
			string message = String.Format("Table exists (case sensitive) [{0}.{1}.{2}]?", dbName, schemaName, tableName);

			string sqlText = String.Format(@"
				IF EXISTS(
					SELECT null
					FROM
						[{0}].sys.schemas sch 
						INNER JOIN [{0}].sys.tables tab ON sch.schema_id = tab.schema_id
					WHERE
						sch.name = '{1}'
						AND tab.name = '{2}' COLLATE SQL_Latin1_General_CP1_CS_AS
				) SELECT 1;
				ELSE SELECT 0;",
				dbName, schemaName, tableName);

			AssertEquals(message, expected, Convert.ToBoolean(connection.ExecuteScalar(sqlText)));
		}

		public static void AssertColumnExistCaseSensitive(DbConnection connection, string dbName, string schemaName, string tableName, string columnName, bool expected)
		{
			string message = String.Format("Column exists (case sensitive) [{0}.{1}.{2}.{3}]?", dbName, schemaName, tableName, columnName);

			AssertEquals(message, expected, TestDbObjectHelper.IsColumnExistCaseSensitive(connection, dbName, schemaName, tableName, columnName));
		}

		public static void AssertForeignKeyExists(DbConnection connection, string foreignKeyName,
			string expectedSourceTable, string expectedSourceColumn, string expectedReferenceTable,
			string expectedReferenceColumn, bool useCombineAssertions = true)
		{
			var sqlText = $@"
SELECT OBJECT_NAME(f.[parent_object_id]) AS SourceTable,
	COL_NAME(fc.[parent_object_id], fc.[parent_column_id]) AS SourceColumn,
	OBJECT_NAME (f.[referenced_object_id]) AS ReferenceTable,
	COL_NAME(fc.[referenced_object_id], fc.[referenced_column_id]) AS ReferenceColumn
FROM sys.foreign_keys AS f
INNER JOIN sys.foreign_key_columns AS fc ON f.object_id = fc.constraint_object_id
INNER JOIN sys.tables AS p ON p.object_id = fc.referenced_object_id
WHERE f.name = '{foreignKeyName}';";
			using (var reader = connection.Command(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					var sourceTable = reader["SourceTable"];
					var sourceColumn = reader["SourceColumn"];
					var referenceTable = reader["ReferenceTable"];
					var referenceColumn = reader["ReferenceColumn"];
					if (sourceTable != DBNull.Value && sourceColumn != DBNull.Value
						&& referenceTable != DBNull.Value && referenceColumn != DBNull.Value)
					{
						VoidParameterlessDelegate assertData = () =>
						{
							AssertEquals($"{foreignKeyName}.SourceTable", expectedSourceTable, sourceTable);
							AssertEquals($"{foreignKeyName}.SourceColumn", expectedSourceColumn, sourceColumn);
							AssertEquals($"{foreignKeyName}.ReferenceTable", expectedReferenceTable, referenceTable);
							AssertEquals($"{foreignKeyName}.ReferenceColumn", expectedReferenceColumn, referenceColumn);
						};
						if (useCombineAssertions)
						{
							CombineAssertions(assertData);
						}
						else
						{
							assertData();
						}
					}
					else
					{
						Fail($"Foreign Key [{foreignKeyName}] does not exist");
					}
				}
			}
		}

		public static void AssertIndexExists(DbConnection connection, string indexName, bool expectedIsUnique, string expectedClusterType, string expectedTableName, string expectedIndexColumns, bool useCombineAssertions = true)
		{
			var sqlText = $@"
SELECT
	ind.is_unique AS IsUnique,
	ind.type_desc AS ClusterType,
	tab.name AS TableName,
	[{Db.Connection.CurrentDatabase}].dbo.CLRConcatenateAgg(col.name, ', ', 1) AS IndexColumns
FROM
	sys.tables tab
	INNER JOIN sys.indexes ind
		ON tab.object_id = ind.object_id
	INNER JOIN sys.index_columns ikey
		ON ikey.object_id = ind.object_id
		AND ikey.index_id = ind.index_id
	INNER JOIN sys.columns col
		ON col.object_id = ikey.object_id
		AND col.column_id = ikey.column_id
WHERE
	ind.name = '{indexName}'
GROUP BY ind.is_unique,
	ind.type_desc,
	tab.name;";
			using (var reader = connection.Command(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					var isUnique = reader["IsUnique"];
					var clusterType = reader["ClusterType"];
					var tableName = reader["TableName"];
					var indexColumns = reader["IndexColumns"];
					if (isUnique != DBNull.Value && clusterType != DBNull.Value
						&& tableName != DBNull.Value && indexColumns != DBNull.Value)
					{
						VoidParameterlessDelegate assertData = () =>
						{
							AssertEquals($"{indexName}.isUnique", expectedIsUnique, isUnique);
							AssertEquals($"{indexName}.clusterType", expectedClusterType, clusterType);
							AssertEquals($"{indexName}.tableName", expectedTableName, tableName);
							AssertEquals($"{indexName}.indexColumns", expectedIndexColumns, indexColumns);
						};
						if (useCombineAssertions)
						{
							CombineAssertions(assertData);
						}
						else
						{
							assertData();
						}
					}
					else
					{
						Fail($"Index [{indexName}] does not exist");
					}
				}
			}
		}
	}
}
