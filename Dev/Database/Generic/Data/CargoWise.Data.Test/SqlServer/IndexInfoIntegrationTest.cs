using System.Linq;
using CargoWise.Common;
using CargoWise.Database.Shared;
using NUnit.Framework;

namespace CargoWise.Data.SqlServer.Testing
{
	public class IndexInfoTest : TransactionedTestCase
	{
		public void TestAll()
		{
			var schema_1 = "test_schema_1";
			var schema_2 = "test_schema_2";
			var testTable = "TestIndexInfo_4D7A415A01064EFBB46BE373758F788A";
			var testIndex = "IX_" + testTable;
			Db.Connection.ExecuteNonQuery($"CREATE SCHEMA {schema_1} CREATE TABLE [{testTable}] (Col1 int, Col2 int, Col3 char(1), Col4 datetime, Col5 tinyint);");
			Db.Connection.ExecuteNonQuery($"CREATE SCHEMA {schema_2} CREATE TABLE [{testTable}] (Col1 int, Col2 int, Col3 char(1), Col4 datetime, Col5 tinyint);");
			var timeout = Db.Connection.DefaultCommandTimeOutInSeconds;
			var disposable = MetaData.AddDelayForTests();
			try
			{
				Db.Connection.DefaultCommandTimeOutInSeconds = 1;

				var index_1 = IndexInfo.Builder.New(schema_1, testTable, testIndex)
					.Key("Col3", "Col5")
					.Key("Col1", OrderBys.DESC)
					.Include("Col4", "Col2")
					.Where("Col5 < 10")
					.GetInfo()
					.Create(Db.Connection);

				var index_2 = IndexInfo.Builder.New(schema_2, testTable, testIndex)
					.Key("Col3")
					.Key("Col5")
					.Key("Col1", OrderBys.DESC)
					.Include("Col4")
					.Include("Col2")
					.Where("((((((Col5 < 10))))))")
					.Option(IndexOptions.MAXDOP, 4)
					.Option(IndexOptions.MAXDOP, 10)
					.Option(IndexOptions.MAXDOP, 1)
					.GetInfo()
					.Create(Db.Connection);

				var expectedDefinition =
					$"NONCLUSTERED INDEX [{testIndex}] ON [{schema_2}].[{testTable}] ([Col3], [Col5], [Col1] DESC) INCLUDE ([Col2], [Col4]) WHERE (Col5 < 10) WITH (ALLOW_PAGE_LOCKS = OFF, MAXDOP = 1, ONLINE = ON)";

				AssertEquals("Definition", expectedDefinition, index_2.Definition);

				var index = IndexLoader.LoadTop1(Db.Connection, null, null, testIndex);

				expectedDefinition =
					$"NONCLUSTERED INDEX [{testIndex}] ON [{schema_1}].[{testTable}] ([Col3], [Col5], [Col1] DESC) INCLUDE ([Col2], [Col4]) WHERE ([Col5]<(10)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)";

				CombineAssertions(() =>
				{
					AssertEquals("Schema Name", schema_1, index.SchemaName);
					AssertEquals("Table Name", testTable, index.TableName);
					AssertEquals("Index Name", testIndex, index.IndexName);
					AssertEquals("IsUnique", false, index.IsUnique);
					AssertEquals("IsClustered", false, index.IsClustered);
					AssertEquals("KeyColumns.Count", 3, index.KeyColumns.Length);
					AssertEquals("HasIncludedColumns", true, index.HasIncludedColumns);
					AssertEquals("IncludedColumns.Count", 2, index.IncludedColumns.Length);
					AssertEquals("HasFilter", true, index.HasFilter);
					AssertEquals("Filter", "[Col5]<(10)", index.Filter);
					AssertEquals("HasOptions", true, index.HasOptions);
					AssertEquals("Definition", expectedDefinition, index.Definition);
					AssertEquals("ToString", expectedDefinition, index.ToString());
				});

				index = IndexLoader.LoadTop1(Db.Connection, schema_2, testTable, testIndex);

				expectedDefinition =
					$"NONCLUSTERED INDEX [{testIndex}] ON [{schema_2}].[{testTable}] ([Col3], [Col5], [Col1] DESC) INCLUDE ([Col2], [Col4]) WHERE ([Col5]<(10)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)";

				CombineAssertions(() =>
				{
					AssertEquals("Schema Name", schema_2, index.SchemaName);
					AssertEquals("Table Name", testTable, index.TableName);
					AssertEquals("Index Name", testIndex, index.IndexName);
					AssertEquals("IsUnique", false, index.IsUnique);
					AssertEquals("IsClustered", false, index.IsClustered);
					AssertEquals("KeyColumns.Count", 3, index.KeyColumns.Length);
					AssertEquals("HasIncludedColumns", true, index.HasIncludedColumns);
					AssertEquals("IncludedColumns.Count", 2, index.IncludedColumns.Length);
					AssertEquals("HasFilter", true, index.HasFilter);
					AssertEquals("Filter", "[Col5]<(10)", index.Filter);
					AssertEquals("HasOptions", true, index.HasOptions);
					AssertEquals("Definition", expectedDefinition, index.Definition);
					AssertEquals("ToString", expectedDefinition, index.ToString());
				});

				var expectedList = new string[]
				{
					$"NONCLUSTERED INDEX [{testIndex}] ON [{schema_1}].[{testTable}] ([Col3], [Col5], [Col1] DESC) INCLUDE ([Col2], [Col4]) WHERE ([Col5]<(10)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
					$"NONCLUSTERED INDEX [{testIndex}] ON [{schema_2}].[{testTable}] ([Col3], [Col5], [Col1] DESC) INCLUDE ([Col2], [Col4]) WHERE ([Col5]<(10)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
				};

				AssertContainsExactElementsInAnyOrder("All indexes", expectedList, IndexLoader.Load(Db.Connection, null, testTable, null).Select(i => i.Definition));

				index_1.Drop(Db.Connection);
				index_2.Drop(Db.Connection);

				AssertNull(IndexLoader.LoadTop1(Db.Connection, schema_1, testTable, testIndex));
				AssertNull(IndexLoader.LoadTop1(Db.Connection, schema_2, testTable, testIndex));
			}
			finally
			{
				Db.Connection.DefaultCommandTimeOutInSeconds = timeout;
				disposable.Dispose();
			}

			Db.Connection.ExecuteNonQuery($"DROP TABLE [{schema_1}].[{testTable}];");
			Db.Connection.ExecuteNonQuery($"DROP SCHEMA [{schema_1}];");
			Db.Connection.ExecuteNonQuery($"DROP TABLE [{schema_2}].[{testTable}];");
			Db.Connection.ExecuteNonQuery($"DROP SCHEMA [{schema_2}];");

			AssertNull(IndexLoader.LoadTop1(Db.Connection, schema_1, testTable, testIndex));
			AssertNull(IndexLoader.LoadTop1(Db.Connection, schema_2, testTable, testIndex));
		}

		public void TestLoadWithOptions()
		{
			// Arrange
			var schema_1 = "Test schema 1";
			var schema_2 = "Test schema 2";
			var testTable = "Test table";
			var testIndex = "IX " + testTable;
			Db.Connection.ExecuteNonQuery($"CREATE SCHEMA {schema_1.QuoteName()}");
			Db.Connection.ExecuteNonQuery($@"
CREATE TABLE {schema_1.QuoteName()}.{testTable.QuoteName()}
(
	[Col 1] int,
	[Col 2] int,
	[Col 3] char(1),
	[Col 4] datetime,
	[Col 5] tinyint,
);");
			Db.Connection.ExecuteNonQuery($"CREATE SCHEMA {schema_2.QuoteName()}");
			Db.Connection.ExecuteNonQuery($@"
CREATE TABLE {schema_2.QuoteName()}.{testTable.QuoteName()}
(
	[Col 1] int,
	[Col 2] int,
	[Col 3] char(1),
	[Col 4] datetime,
	[Col 5] tinyint,
);");

			using (Db.Connection.TemporarySetDefaultCommandTimeOut(1))
			{
				var index_1 = IndexInfo.Builder.New(schema_1, testTable, testIndex)
					.Key("Col 3", "Col 5")
					.Key("Col 1", OrderBys.DESC)
					.Include("Col 4", "Col 2")
					.Option(IndexOptions.ALLOW_ROW_LOCKS, true) // default value
					.Option(IndexOptions.ALLOW_PAGE_LOCKS, false) // not a default value
					.GetInfo()
					.Create(Db.Connection);

				var index_2 = IndexInfo.Builder.New(schema_2, testTable, testIndex)
					.Key("Col 3")
					.Key("Col 5")
					.Key("Col 1", OrderBys.DESC)
					.Include("Col 4")
					.Include("Col 2")
					.Option(IndexOptions.MAXDOP, 4) // runtime option
					.Option(IndexOptions.DATA_COMPRESSION, DataCompression.PAGE) // not a default value
					.GetInfo()
					.Create(Db.Connection);

				// Act
				var indexes = IndexLoader.Load(Db.Connection, schemaName: null, tableName: testTable, indexName: null);

				// Assert
				var expectedDefinitions = new string[]
				{
					$"NONCLUSTERED INDEX [{testIndex}] ON [{schema_1}].[{testTable}] ([Col 3], [Col 5], [Col 1] DESC) INCLUDE ([Col 2], [Col 4]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
					$"NONCLUSTERED INDEX [{testIndex}] ON [{schema_2}].[{testTable}] ([Col 3], [Col 5], [Col 1] DESC) INCLUDE ([Col 2], [Col 4]) WITH (ALLOW_PAGE_LOCKS = OFF, DATA_COMPRESSION = PAGE, ONLINE = ON)",
				};

				var expectedComparableDefinitions = new string[]
				{
					$"[{schema_1}].[{testTable}].[{testIndex}] NONCLUSTERED INDEX ([Col 3], [Col 5], [Col 1] DESC) INCLUDE ([Col 2], [Col 4]) WITH (ALLOW_PAGE_LOCKS = OFF)",
					$"[{schema_2}].[{testTable}].[{testIndex}] NONCLUSTERED INDEX ([Col 3], [Col 5], [Col 1] DESC) INCLUDE ([Col 2], [Col 4]) WITH (ALLOW_PAGE_LOCKS = OFF, DATA_COMPRESSION = PAGE)",
				};

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("Index Definitions", expectedDefinitions, indexes.Select(i => i.Definition));
					AssertContainsExactElementsInAnyOrder("Index ComparableDefinitions", expectedComparableDefinitions, indexes.Select(i => i.ComparableDefinition));
				});
			}
		}
	}
}
