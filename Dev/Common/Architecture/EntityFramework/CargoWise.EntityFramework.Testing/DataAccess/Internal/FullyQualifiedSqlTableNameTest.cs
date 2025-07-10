using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class FullyQualifiedSqlTableNameTest : TestCase
	{
		public void TestFullyQualifiedNameWithDatabaseNameSchemaNameAndTableName()
		{
			FullyQualifiedSqlTableName sql = new FullyQualifiedSqlTableName() { FullyQualifiedName = "db1.schema1.table1" };

			AssertEquals("db1", sql.Database);
			AssertEquals("schema1", sql.Owner);
			AssertEquals("table1", sql.Table);
			AssertEquals("db1.schema1.table1", sql.FullyQualifiedName);
		}

		public void TestFullyQualifiedNameWithDboSchemaAndTableName()
		{
			FullyQualifiedSqlTableName sql = new FullyQualifiedSqlTableName() { FullyQualifiedName = "dbo.table1" };

			AssertEquals("", sql.Database);
			AssertEquals("dbo", sql.Owner);
			AssertEquals("table1", sql.Table);
			AssertEquals("table1", sql.FullyQualifiedName);
		}

		public void TestFullyQualifiedNameWithSchemaNameAndTableName()
		{
			FullyQualifiedSqlTableName sql = new FullyQualifiedSqlTableName() { FullyQualifiedName = "schema1.table1" };

			AssertEquals("", sql.Database);
			AssertEquals("schema1", sql.Owner);
			AssertEquals("table1", sql.Table);
			AssertEquals("schema1.table1", sql.FullyQualifiedName);
		}

		public void TestFullyQualifiedNameWithTableName()
		{
			FullyQualifiedSqlTableName sql = new FullyQualifiedSqlTableName() { FullyQualifiedName = "table1" };

			AssertEquals("", sql.Database);
			AssertEquals("dbo", sql.Owner);
			AssertEquals("table1", sql.Table);
			AssertEquals("table1", sql.FullyQualifiedName);
		}

		public void TestConstructor()
		{
			FullyQualifiedSqlTableName sql = new FullyQualifiedSqlTableName("table1");

			AssertEquals("", sql.Database);
			AssertEquals("dbo", sql.Owner);
			AssertEquals("table1", sql.Table);
			AssertEquals("table1", sql.FullyQualifiedName);
		}
	}
}
