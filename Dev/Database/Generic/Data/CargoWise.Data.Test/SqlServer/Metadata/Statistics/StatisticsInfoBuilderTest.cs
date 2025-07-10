using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class StatisticsInfoBuilderTest : TestCase
	{
		public void TestNew()
		{
			var schema = "test_schema_1";
			var testTable = "TestStatisticsInfoBuilder";
			var testStats = "Stats_" + testTable;

			var stats = MetaData.StatisticsInfo.Builder.New(schema, testTable, testStats)
				.Key("Col3", "Col2")
				.Where("((((Col1 = 20))))")
				.Info
				;

			var expectedDefinition =
				$"STATISTICS [{testStats}] ON [{schema}].[{testTable}] ([Col3], [Col2]) WHERE (Col1 = 20)";

			CombineAssertions(() =>
			{
				AssertEquals("Schema Name", schema, stats.SchemaName);
				AssertEquals("Table Name", testTable, stats.TableName);
				AssertEquals("Statistics Name", testStats, stats.StatisticsName);
				AssertEquals("KeyColumns.Count", 2, stats.KeyColumns.Count);
				AssertEquals("HasFilter", true, stats.HasFilter);
				AssertEquals("Filter", "Col1 = 20", stats.Filter);
				AssertEquals("HasOptions", false, stats.HasOptions);
				AssertEquals("Definition", expectedDefinition, stats.Definition);
				AssertEquals("ToString", expectedDefinition, stats.ToString());
				AssertEquals("IsAutoCreated", false, stats.IsAutoCreated);
				AssertEquals("IsUserCreated", true, stats.IsUserCreated);
				AssertEquals("IsTemporary", false, stats.IsTemporary);
				AssertEquals("IsIncremental", false, stats.IsIncremental);
			});
		}
	}
}
