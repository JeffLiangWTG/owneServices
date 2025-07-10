using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	sealed class DbSchemaChangeTest : TransactionedTestCase
	{
		public void TestGetDropStoredProcedureIfExistsScript()
		{
			string tableName = "TestTable";
			string columnName = "TestColumn";
			TestConnection.ExecuteNonQuery(string.Format("CREATE TABLE {0} ({1} VARCHAR(5))", tableName, columnName));
			TestConnection.ExecuteNonQuery(string.Format("ALTER TABLE {0} ADD  DEFAULT ('') FOR {1}", tableName, columnName));
			AssertEquals("Constraint should Exists", true, DefaultConstraintExistsFromColumnName(TestConnection, tableName, columnName));
			TestConnection.ExecuteNonQuery(DbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(tableName, columnName));
			AssertEquals("Constraint should NOT Exists", false, DefaultConstraintExistsFromColumnName(TestConnection, tableName, columnName));

			string spName = "sp_TEST_ParameterOfStoredPrecedureExists";
			TestConnection.ExecuteNonQuery(string.Format(@"CREATE PROCEDURE {0}
										@Para CHAR 
									AS
									SELECT top 1 * FROM sys.types
									", spName));
			AssertEquals("PROCEDURE exist", true, DbObjectCreator.ObjectExists(TestConnection, spName));

			TestConnection.ExecuteNonQuery(DbSchemaChange.GetDropStoredProcedureIfExistsScript(spName));
			AssertEquals("PROCEDURE should not exist", false, DbObjectCreator.ObjectExists(TestConnection, spName));
		}

		public void TestGetCreateIndexIfNotExistsAndDrop()
		{
			string indexName = "TestGetCreateIndexIfNotExists_Index";
			string tableName = "TestGetCreateIndexIfNotExists_Table";
			string columnName = "TestGetCreateIndexIfNotExists_Column";
			string indexCreateScript = string.Format("create index {0} on {1} ({2})", indexName, tableName, columnName);

			TestConnection.ExecuteNonQuery(string.Format("CREATE TABLE {0} (col1 int)", tableName));
			TestConnection.ExecuteNonQuery(string.Format("ALTER TABLE {0} ADD {1} int", tableName, columnName));

			AssertEquals("index exists", false, DbObjectCreator.IndexExists(TestConnection, tableName, indexName));

			TestConnection.ExecuteNonQuery(DbSchemaChange.GetCreateIndexIfNotExistsScript(tableName, indexName, indexCreateScript));
			AssertEquals("index exists", true, DbObjectCreator.IndexExists(TestConnection, tableName, indexName));

			TestConnection.ExecuteNonQuery(DbSchemaChange.GetCreateIndexIfNotExistsScript(tableName, indexName, indexCreateScript));
			AssertEquals("no duplicate index created and index still exists", true, DbObjectCreator.IndexExists(TestConnection, tableName, indexName));

			TestConnection.ExecuteNonQuery(DbSchemaChange.GetDropIndexIfExistsScript(tableName, indexName));
			AssertEquals("index should be dropped", false, DbObjectCreator.IndexExists(TestConnection, tableName, indexName));
		}

		public void TestGetAddCheckConstranintIfNotExistsScript()
		{
			string tableName = "TestGetAddCheckConstranintIfNotExistsScript_Table";
			string constraintName = "CK_col1";
			TestConnection.ExecuteNonQuery(string.Format("CREATE TABLE {0} (col1 int)", tableName));
			AssertEquals("Constraint should NOT Exists?", false, CheckConstraintExists(TestConnection, constraintName));
			TestConnection.ExecuteNonQuery(DbSchemaChange.GetAddCheckConstranintIfNotExistsScript(tableName, constraintName, "col1 <> 100"));
			AssertEquals("Constraint should Exists?", true, CheckConstraintExists(TestConnection, constraintName));
		}

		bool CheckConstraintExists(DbConnection connection, string constraintName)
		{
			string sqlText = string.Format("SELECT COUNT(*) FROM sys.check_constraints where name = '{0}'", constraintName);
			bool result = (Convert.ToInt32(connection.ExecuteScalar(sqlText)) > 0);
			return result;
		}

		public void TestGetCreateColumnIfNotExists()
		{
			string tableName = "TestGetCreateColumnIfNotExists_Table";
			string columnName = "TestGetCreateColumnIfNotExists_NewColumn";
			TestConnection.ExecuteNonQuery(string.Format("CREATE TABLE {0} (col1 int)", tableName));
			AssertEquals("Column Exists?", false, DbObjectCreator.ColumnExists(TestConnection, tableName, columnName));
			TestConnection.ExecuteNonQuery(DbSchemaChange.GetAddColumnIfNotExistsScript(tableName, columnName, "int null"));
			AssertEquals("Column Exists?", true, DbObjectCreator.ColumnExists(TestConnection, tableName, columnName));
		}

		public void TestEnsureCorrectColumnCollation()
		{
			// Create table with different collation columns
			string createTableSql = @"
				CREATE TABLE [TestEnsureCorrectColumnCollation!Table!]
				(
					Col1 varchar(10) COLLATE French_CI_AS NULL,
					Col2 char(2) COLLATE Latin1_General_BIN NOT NULL
				)";
			TestConnection.ExecuteNonQuery(createTableSql);

			// Assert collation before
			AssertColumnCollation(TestConnection, "TestEnsureCorrectColumnCollation!Table!", "Col1", "French_CI_AS");
			AssertColumnCollation(TestConnection, "TestEnsureCorrectColumnCollation!Table!", "Col2", "Latin1_General_BIN");

			// Ensure column collation is correct
			DbSchemaChange.EnsureCorrectColumnCollation(TestConnection);

			// Assert collation afterwards
			AssertColumnCollation(TestConnection, "TestEnsureCorrectColumnCollation!Table!", "Col1", Db.DatabaseCollation);
			AssertColumnCollation(TestConnection, "TestEnsureCorrectColumnCollation!Table!", "Col2", Db.DatabaseCollation);
		}

		public static void AssertColumnCollation(DbConnection connection, string tableName, string columnName, string expectedCollation)
		{
			string sqlText = string.Format(@"
				SELECT
					c.collation_name
				FROM
					sys.columns c
					INNER JOIN sys.tables t ON t.object_id = c.object_id
				WHERE
					t.name = '{0}'
					AND c.name = '{1}'",
				tableName,
				columnName);

			object actualCollationObj = connection.ExecuteScalar(sqlText);
			string actualCollation = (actualCollationObj == null || actualCollationObj == DBNull.Value) ? null : actualCollationObj.ToString();
			AssertEquals(string.Format("[{0}].[{1}] collation", tableName, columnName), expectedCollation, actualCollation);
		}

		public void TestGetAddForeignKeyIfNotExistsScript()
		{
			string tableName1 = "TestGetAddForeignKeyIfNotExistsScript_Table1";
			string tableName2 = "TestGetAddForeignKeyIfNotExistsScript_Table2";
			string foreignKeyName = "FK_COL1";
			TestConnection.ExecuteNonQuery(string.Format("CREATE TABLE {0} (T1_COL1 VARCHAR(10))", tableName1));
			TestConnection.ExecuteNonQuery(string.Format("CREATE TABLE {0} (T2_COL1 VARCHAR(10) PRIMARY KEY)", tableName2));
			AssertEquals("Foreign Key should NOT Exists", false, CheckForeignKeyExists(TestConnection, foreignKeyName));
			TestConnection.ExecuteNonQuery(DbSchemaChange.GetAddForeignKeyIfNotExistsScript(tableName1, foreignKeyName, "T1_COL1", "TestGetAddForeignKeyIfNotExistsScript_Table2 (T2_COL1)"));
			AssertEquals("Foreign Key should Exists", true, CheckForeignKeyExists(TestConnection, foreignKeyName));
		}

		bool CheckForeignKeyExists(DbConnection connection, string foreignKeyName)
		{
			string sqlText = string.Format("SELECT COUNT(*) FROM sys.foreign_keys where name = '{0}'", foreignKeyName);
			bool result = (Convert.ToInt32(connection.ExecuteScalar(sqlText)) > 0);
			return result;
		}

		public void TestGetAddDefaultConstranintIfNotExistsScript()
		{
			string tableName = "TestGetAddDefaultConstranintIfNotExistsScript_Table1";
			string constraintName = "CK_col1";
			TestConnection.ExecuteNonQuery(string.Format("CREATE TABLE {0} (T1_COL1 VARCHAR(5))", tableName));
			AssertEquals("Constraint should NOT Exists?", false, DefaultConstraintExists(TestConnection, constraintName));
			TestConnection.ExecuteNonQuery(DbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(tableName, constraintName, "''", "T1_COL1"));
			AssertEquals("Constraint should NOT Exists", true, DefaultConstraintExists(TestConnection, constraintName));
		}

		public void TestGetDropConstraintIfExistsScript()
		{
			string tableName = "TestGetAddDefaultConstranintIfNotExistsScript_Table1";
			string constraintName = "CK_col1";
			TestConnection.ExecuteNonQuery(string.Format("CREATE TABLE {0} (T1_COL1 VARCHAR(5))", tableName));
			TestConnection.ExecuteNonQuery(DbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(tableName, constraintName, "''", "T1_COL1"));
			AssertEquals("Constraint should NOT Exists", true, DefaultConstraintExists(TestConnection, constraintName));
			TestConnection.ExecuteNonQuery(DbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", tableName, constraintName));
			AssertEquals("Constraint should NOT Exists?", false, DefaultConstraintExists(TestConnection, constraintName));
		}

		bool DefaultConstraintExists(DbConnection connection, string constraintName)
		{
			string sqlText = string.Format("SELECT COUNT(*) FROM sys.default_constraints where name = '{0}'", constraintName);
			bool result = (Convert.ToInt32(connection.ExecuteScalar(sqlText)) > 0);
			return result;
		}

		public void TestGetDropColumnIfExistsScript()
		{
			string tableName = "TestGetDropColumnIfExistsScript_Table";
			string columnName = "TestGetDropColumnIfExistsScript_Column";
			TestConnection.ExecuteNonQuery(string.Format("CREATE TABLE {0} (COL1 VARCHAR(5), {1} VARCHAR(5))", tableName, columnName));
			Assert("Column Exists?", DbObjectCreator.ColumnExists(TestConnection, tableName, columnName));
			TestConnection.ExecuteNonQuery(DbSchemaChange.GetDropColumnIfExistsScript(tableName, columnName));
			Assert("Column Exists?", !DbObjectCreator.ColumnExists(TestConnection, tableName, columnName));
		}

		public void TestGetDropConstraintIfExistsFromColumnNameScript()
		{
			string tableName = "TestTable";
			string columnName = "TestColumn";
			TestConnection.ExecuteNonQuery(string.Format("CREATE TABLE {0} ({1} VARCHAR(5))", tableName, columnName));
			TestConnection.ExecuteNonQuery(string.Format("ALTER TABLE {0} ADD  DEFAULT ('') FOR {1}", tableName, columnName));
			AssertEquals("Constraint should Exists", true, DefaultConstraintExistsFromColumnName(TestConnection, tableName, columnName));
			TestConnection.ExecuteNonQuery(DbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(tableName, columnName));
			AssertEquals("Constraint should NOT Exists", false, DefaultConstraintExistsFromColumnName(TestConnection, tableName, columnName));
		}

		bool DefaultConstraintExistsFromColumnName(DbConnection connection, string tableName, string columnName)
		{
			string script = string.Format(@"
SELECT count(*) from sys.objects WHERE object_id = (SELECT sys.columns.default_object_id FROM sys.objects INNER JOIN sys.columns ON sys.objects.object_id = sys.columns.object_id 
    WHERE upper(sys.columns.name) = '{1}' AND upper(sys.objects.name) = '{0}')", tableName, columnName);
			bool result = Convert.ToInt32(connection.ExecuteScalar(script)) > 0;
			return result;
		}

		public void TestGetRenameColumnIfExistsScript()
		{
			string tableName = "TestGetRenameColumnIfExistsScript_Table";
			string columnName = "TestGetRenameColumnIfExistsScript_Column_Old";
			string columnNewName = "TestGetRenameColumnIfExistsScript_Column_New";
			TestConnection.ExecuteNonQuery(string.Format("CREATE TABLE {0} ({1} VARCHAR(5))", tableName, columnName));
			Assert("Column Exists?", DbObjectCreator.ColumnExists(TestConnection, tableName, columnName));
			TestConnection.ExecuteNonQuery(DbSchemaChange.GetRenameColumnIfExistsScript(tableName, columnName, columnNewName));
			Assert("Column Exists?", !DbObjectCreator.ColumnExists(TestConnection, tableName, columnName));
			Assert("Column Exists?", DbObjectCreator.ColumnExists(TestConnection, tableName, columnNewName));
		}

		public void TestGetDropForeignKeyFromIndexNameScript()
		{
			var tableName1 = "Table1";
			var tableName2 = "Table2";
			TestConnection.ExecuteNonQuery(string.Format("CREATE TABLE {0} (T1_COL1 VARCHAR(10))", tableName1));
			TestConnection.ExecuteNonQuery(string.Format("CREATE TABLE {0} (T2_COL1 VARCHAR(10))", tableName2));

			var indexName = "IX_Table2_T2_COL1";
			var createIndexScript = "CREATE UNIQUE NONCLUSTERED INDEX IX_Table2_T2_COL1 ON Table2 (T2_COL1 ASC)";
			TestConnection.ExecuteNonQuery(DbSchemaChange.GetCreateIndexIfNotExistsScript(tableName2, indexName, createIndexScript));

			var foreignKeyName = "FK_Table2_T2_COL1";
			TestConnection.ExecuteNonQuery(DbSchemaChange.GetAddForeignKeyIfNotExistsScript(tableName1, foreignKeyName, "T1_COL1", "Table2 (T2_COL1)"));
			AssertEquals("Foreign Key should Exists", true, CheckForeignKeyExists(TestConnection, foreignKeyName));

			TestConnection.ExecuteNonQuery(DbSchemaChange.GetDropForeignKeyFromIndexNameScript(TestConnection, "IX_Table2_T2_COL1"));
			AssertEquals("Foreign Key should not Exists", false, CheckForeignKeyExists(TestConnection, foreignKeyName));
		}

		public void TestGetAddPrimaryKeyIfNotExistsScript()
		{
			var tableName1 = "Table1";
			TestConnection.ExecuteNonQuery(string.Format("CREATE TABLE {0} (T1_COL1 VARCHAR(10) NOT NULL)", tableName1));
			AssertEquals("Primary Key should not exist", false, CheckPrimaryKeyExists(TestConnection, tableName1));

			TestConnection.ExecuteNonQuery(DbSchemaChange.GetAddPrimaryKeyIfNotExistsScript(tableName1, "T1_COL1_PK", "T1_COL1"));
			AssertEquals("Primary Key should exist", true, CheckPrimaryKeyExists(TestConnection, tableName1));
		}

		bool CheckPrimaryKeyExists(DbConnection connection, string tableName)
		{
			string sqlText = string.Format(@"
SELECT COUNT(*) FROM information_schema.table_constraints con WHERE con.constraint_Type = 'PRIMARY KEY'
					AND con.table_name = '{0}'", tableName);

			bool result = (Convert.ToInt32(connection.ExecuteScalar(sqlText)) > 0);
			return result;
		}
	}
}
