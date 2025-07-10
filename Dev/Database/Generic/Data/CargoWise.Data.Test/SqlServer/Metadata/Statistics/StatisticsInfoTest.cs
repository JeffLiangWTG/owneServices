using System.Data;
using System.Linq;
using CargoWise.Database.Shared;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class StatisticsInfoTest : TransactionedTestCase
	{
		public void TestAll()
		{
			var schema_1 = "test_schema_1";
			var schema_2 = "test_schema_2";
			var testTable = "TestStatisticsInfo_8E90533521DB469193058CA7E7E48B05";
			var testIndex = "IX_" + testTable;
			var testStats = "Stats_" + testTable;

			Db.Connection.ExecuteNonQuery($"CREATE SCHEMA {schema_1} CREATE TABLE [{testTable}] (Col1 int, Col2 int, Col3 char(1), Col4 datetime, Col5 tinyint);");
			Db.Connection.ExecuteNonQuery($"CREATE SCHEMA {schema_2} CREATE TABLE [{testTable}] (Col1 int, Col2 int, Col3 char(1), Col4 datetime, Col5 tinyint);");

			var index_1 = IndexInfo.Builder.New(schema_1, testTable, testIndex)
				.Key("Col3", "Col5")
				.Key("Col1", OrderBys.DESC)
				.Include("Col4", "Col2")
				.Where("Col5 < 10")
				.GetInfo()
				.Create(Db.Connection);

			var index_2 = IndexInfo.Builder.New(schema_2, testTable, testIndex)
				.Key("Col2", "Col1")
				.Include("Col5")
				.Where("Col5 > 2")
				.GetInfo()
				.Create(Db.Connection);

			var stats_1 = MetaData.StatisticsInfo.Builder.New(schema_1, testTable, testStats)
				.Key("Col3", "Col2")
				.Where("((((Col1 = 20))))")
				.Info
				.Create(Db.Connection);

			var stats = MetaData.StatisticsInfo.LoadTop1(Db.Connection, null, null, testIndex);

			var expectedDefinition =
				$"STATISTICS [{testIndex}] ON [{schema_1}].[{testTable}] ([Col3], [Col5], [Col1]) WHERE ([Col5]<(10))";

			CombineAssertions(() =>
			{
				AssertEquals("Schema Name", schema_1, stats.SchemaName);
				AssertEquals("Table Name", testTable, stats.TableName);
				AssertEquals("Statistics Name", testIndex, stats.StatisticsName);
				AssertEquals("KeyColumns.Count", 3, stats.KeyColumns.Count);
				AssertEquals("HasFilter", true, stats.HasFilter);
				AssertEquals("Filter", "[Col5]<(10)", stats.Filter);
				AssertEquals("HasOptions", false, stats.HasOptions);
				AssertEquals("Definition", expectedDefinition, stats.Definition);
				AssertEquals("ToString", expectedDefinition, stats.ToString());
				AssertEquals("IsAutoCreated", false, stats.IsAutoCreated);
				AssertEquals("IsUserCreated", false, stats.IsUserCreated);
				AssertEquals("IsTemporary", false, stats.IsTemporary);
				AssertEquals("IsIncremental", false, stats.IsIncremental);
			});

			stats = MetaData.StatisticsInfo.LoadTop1(Db.Connection, schema_2, testTable, testIndex);

			expectedDefinition =
				$"STATISTICS [{testIndex}] ON [{schema_2}].[{testTable}] ([Col2], [Col1]) WHERE ([Col5]>(2))";

			CombineAssertions(() =>
			{
				AssertEquals("Schema Name", schema_2, stats.SchemaName);
				AssertEquals("Table Name", testTable, stats.TableName);
				AssertEquals("Statistics Name", testIndex, stats.StatisticsName);
				AssertEquals("KeyColumns.Count", 2, stats.KeyColumns.Count);
				AssertEquals("HasFilter", true, stats.HasFilter);
				AssertEquals("Filter", "[Col5]>(2)", stats.Filter);
				AssertEquals("HasOptions", false, stats.HasOptions);
				AssertEquals("Definition", expectedDefinition, stats.Definition);
				AssertEquals("ToString", expectedDefinition, stats.ToString());
				AssertEquals("IsAutoCreated", false, stats.IsAutoCreated);
				AssertEquals("IsUserCreated", false, stats.IsUserCreated);
				AssertEquals("IsTemporary", false, stats.IsTemporary);
				AssertEquals("IsIncremental", false, stats.IsIncremental);
			});

			stats = MetaData.StatisticsInfo.LoadTop1(Db.Connection, schema_1, testTable, testStats);

			expectedDefinition =
				$"STATISTICS [{testStats}] ON [{schema_1}].[{testTable}] ([Col3], [Col2]) WHERE ([Col1]=(20))";

			CombineAssertions(() =>
			{
				AssertEquals("Schema Name", schema_1, stats.SchemaName);
				AssertEquals("Table Name", testTable, stats.TableName);
				AssertEquals("Statistics Name", testStats, stats.StatisticsName);
				AssertEquals("KeyColumns.Count", 2, stats.KeyColumns.Count);
				AssertEquals("HasFilter", true, stats.HasFilter);
				AssertEquals("Filter", "[Col1]=(20)", stats.Filter);
				AssertEquals("HasOptions", false, stats.HasOptions);
				AssertEquals("Definition", expectedDefinition, stats.Definition);
				AssertEquals("ToString", expectedDefinition, stats.ToString());
				AssertEquals("IsAutoCreated", false, stats.IsAutoCreated);
				AssertEquals("IsUserCreated", true, stats.IsUserCreated);
				AssertEquals("IsTemporary", false, stats.IsTemporary);
				AssertEquals("IsIncremental", false, stats.IsIncremental);
			});

			var expectedList = new string[]
			{
				$"STATISTICS [{testIndex}] ON [{schema_1}].[{testTable}] ([Col3], [Col5], [Col1]) WHERE ([Col5]<(10))",
				$"STATISTICS [{testIndex}] ON [{schema_2}].[{testTable}] ([Col2], [Col1]) WHERE ([Col5]>(2))",
				$"STATISTICS [{testStats}] ON [{schema_1}].[{testTable}] ([Col3], [Col2]) WHERE ([Col1]=(20))",
			};

			AssertContainsExactElementsInAnyOrder("All statistics", expectedList, MetaData.StatisticsInfo.Load(Db.Connection, null, testTable, null).Select(s => s.Definition));

			index_1.Drop(Db.Connection);
			index_2.Drop(Db.Connection);
			stats_1.Drop(Db.Connection);
			stats_1.Drop(Db.Connection);

			AssertNull(MetaData.StatisticsInfo.LoadTop1(Db.Connection, schema_1, testTable, testIndex));
			AssertNull(MetaData.StatisticsInfo.LoadTop1(Db.Connection, schema_2, testTable, testIndex));
			AssertNull(MetaData.StatisticsInfo.LoadTop1(Db.Connection, schema_1, testTable, testStats));

			Db.Connection.ExecuteNonQuery($"DROP TABLE [{schema_1}].[{testTable}];");
			Db.Connection.ExecuteNonQuery($"DROP SCHEMA [{schema_1}];");
			Db.Connection.ExecuteNonQuery($"DROP TABLE [{schema_2}].[{testTable}];");
			Db.Connection.ExecuteNonQuery($"DROP SCHEMA [{schema_2}];");

			AssertNull(MetaData.StatisticsInfo.LoadTop1(Db.Connection, schema_1, testTable, testIndex));
			AssertNull(MetaData.StatisticsInfo.LoadTop1(Db.Connection, schema_2, testTable, testIndex));
			AssertNull(MetaData.StatisticsInfo.LoadTop1(Db.Connection, schema_1, testTable, testStats));
		}
	}
}
