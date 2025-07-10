using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace CargoWise.Bi.ConfigLoader.Testing
{
	class EdwTableQueryTest : TestCase
	{
		public void TestEdwTableQueriesShouldNotBeNullNorEmpty()
		{
			CombineAssertions("The following queries were null or empty for the test tables.", () =>
			{
				foreach (var edwTable in ConfigData.EdwTableConfig)
				{
					Assert("[" + edwTable.Name + "] Initial Load Query", !string.IsNullOrEmpty(ConfigData.GetInitialLoadQueryForEdwTable(edwTable)));
					Assert("[" + edwTable.Name + "] Incremental Load Delete Query", !string.IsNullOrEmpty(ConfigData.GetIncrementalDeleteQueryForEdwTable(edwTable)));
					Assert("[" + edwTable.Name + "] Incremental Load Insert Query", !string.IsNullOrEmpty(ConfigData.GetIncrementalInsertQueryForEdwTable(edwTable)));
					Assert("[" + edwTable.Name + "] Custom Index Script", !string.IsNullOrEmpty(ConfigData.GetCustomIndexScriptQueryForEdwTable(edwTable)));
				}
			});
		}

		public void TestIncrementalInsertQueryForEdwTableShouldUseIndexHint()
		{
			CombineAssertions("The following queries do not use row store index for deletion.", () =>
			{
				foreach (var edwTable in ConfigData.EdwTableConfig)
				{
					var query = ConfigData.GetIncrementalDeleteQueryForEdwTable(edwTable);
					var expected = $"[mt] WITH(INDEX(IX_{edwTable.Schema}_{edwTable.Name}_{edwTable.BaseName}ID))";
					Assert($"[{edwTable.Schema}].[{edwTable.Table}]", !string.IsNullOrEmpty(query) && query.Contains(expected));
				}
			});
		}

		public void TestAggTableQueriesShouldNotBeNullNorEmpty()
		{
			CombineAssertions("The following tables have empty custom index scripts:", () =>
			{
				foreach (var aggTable in ConfigData.EdwDenormalizedTableConfig)
				{
					Assert($"[{aggTable.Schema}].[{aggTable.Name}]", !string.IsNullOrEmpty(ConfigData.GetCustomIndexScriptQueryForDenormalizedTable(aggTable)));
				}
			});
		}

		public void TestSelfReferencedTableWithCustomTableReference()
		{
			var customTable = ConfigData.EdwCustomTableConfig.AddEdwCustomTableConfigRow("Test", "CUS__UnitTestTable", -1, "", "", "", "", true, "");
			ConfigData.EdwCustomColumnConfig.AddEdwCustomColumnConfigRow(customTable, "PK", "uniqueidentifier", 0, 0, 0, "");
			ConfigData.EdwCustomColumnConfig.AddEdwCustomColumnConfigRow(customTable, "Value", "int", 0, 0, 0, "");

			var stagingTable = ConfigData.CdcTableConfig.AddCdcTableConfigRow("Test", "UnitTestTable", "Test", "", "", true, true, null, null, null, false);
			ConfigData.CdcColumnConfig.AddCdcColumnConfigRow(stagingTable, "PK", "uniqueidentifier", 0, 0, 0, false, true, "", "", true, "", "", true, true, "");
			ConfigData.CdcColumnConfig.AddCdcColumnConfigRow(stagingTable, "ParentPK", "uniqueidentifier", 0, 0, 0, false, true, "", "", true, "", "", true, true, "");

			var edwTable = ConfigData.EdwTableConfig.AddEdwTableConfigRow("Test", "BAS__UnitTestTable", "", "Test", "UnitTestTable", "", 1, false, -1, "Test");
			ConfigData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, 1, "uniqueidentifier", 16, 0, 0, "UnitTestTableID", "PK", false, false, "", "", "", "", "", "", false);
			ConfigData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, 1, "bigint", 16, 0, 0, "UnitTestTableKey", "", false, false, "", "", "", "", "", "", false);
			ConfigData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, 1, "bigint", 16, 0, 0, "SelfReferencedValue", "ParentPK", false, false, "", edwTable.Name, "UnitTestTableID", "", "", "", false);
			ConfigData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, 1, "bigint", 16, 0, 0, "SelfReferencedValue", "ParentPK", false, false, "", customTable.Name, "PK", "Value", "", "", false);

			CombineAssertions("The following queries were null or empty for the test tables.", () =>
			{
				Assert("Initial Load Query", !string.IsNullOrEmpty(ConfigData.GetInitialLoadQueryForEdwTable(edwTable)));
				Assert("Incremental Load Insert Query", !string.IsNullOrEmpty(ConfigData.GetIncrementalInsertQueryForEdwTable(edwTable)));
				Assert("Incremental Load Delete Query", !string.IsNullOrEmpty(ConfigData.GetIncrementalDeleteQueryForEdwTable(edwTable)));
				Assert("Custom Index Query", !string.IsNullOrEmpty(configData.GetEditableCustomIndexScriptQueryForEdwTable(edwTable)));
			});
		}

		public void TestRowStoreIDIndexIncludeKeyColumn()
		{
			var edwTable = ConfigData.EdwTableConfig.AddEdwTableConfigRow("Test", "BAS__UnitTestTable", "", "Test", "UnitTestTable", "", 1, false, -1, "Test");
			ConfigData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, 1, "uniqueidentifier", 16, 0, 0, "UnitTestTableID", "PK", false, false, "", "", "", "", "", "", true);
			ConfigData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, 1, "bigint", 16, 0, 0, "UnitTestTableKey", "", false, false, "", "", "", "", "", "", false);
			AssertEquals("Custom Index Query", "CREATE NONCLUSTERED INDEX [IX_Test_BAS__UnitTestTable_UnitTestTableID] ON [Test].[BAS__UnitTestTable] (UnitTestTableID, [__$transform_id]) INCLUDE(UnitTestTableKey) WITH (DATA_COMPRESSION = PAGE, DROP_EXISTING = OFF);\r\n\r\n", ConfigData.GetCustomIndexScriptQueryForEdwTable(edwTable));
		}

		public void TestConsistentIndexDefinitionInCustomTables()
		{
			CombineAssertions(() =>
			{
				CheckIndexConsistency(ConfigData.EdwCustomTableConfig.Where(r => !string.IsNullOrEmpty(r.InitialLoadQuery)).Select(r => r.InitialLoadQuery),
					"Detected indexes defined in custom tables were not properly recreated during initial load. These indexes should be dropped and recreated during load.");
				CheckIndexConsistency(ConfigData.EdwCustomTableConfig.Where(r => !string.IsNullOrEmpty(r.IncrementalLoadQuery)).Select(r => r.IncrementalLoadQuery),
					"Detected indexes defined in custom tables were not properly recreated during incremental load. These indexes should be dropped and recreated during load.");
			});
		}

		void CheckIndexConsistency(IEnumerable<string> queryCollection, string assertionMessage)
		{
			var createdIndex = new HashSet<string>();
			var droppedIndex = new HashSet<string>();

			foreach (var query in queryCollection)
			{
				var sanitizedQuery = RemoveComments(query);
				foreach (Match m in indexCreationRegex.Matches(sanitizedQuery))
				{
					createdIndex.Add(m.Groups["indexName"].Value);
				}
				foreach (Match m in indexDropRegex.Matches(sanitizedQuery))
				{
					droppedIndex.Add(m.Groups["indexName"].Value);
				}
			}

			var different = new HashSet<string>(createdIndex);
			different.SymmetricExceptWith(droppedIndex.ToArray());
			Assert(assertionMessage, different.Count == 0);
			Assert($"The following indexes were created but not dropped:\r\n{string.Join("\r\n", createdIndex.Intersect(different))}", !createdIndex.Intersect(different).Any());
			Assert($"The following indexes were dropped but not recreated:\r\n{string.Join("\r\n", droppedIndex.Intersect(different))}", !droppedIndex.Intersect(different).Any());
		}

		static readonly Regex indexCreationRegex = new Regex(@"CREATE\s+CLUSTERED\s+COLUMNSTORE\s+INDEX\s+(?<indexName>cci_\w*)\s+ON", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);
		static readonly Regex indexDropRegex = new Regex("DROP( )+INDEX( )+(?<indexName>cci_(\\w)*)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
		static readonly Regex commentRegex = new Regex(@"(--.*?$)|(/\*.*?\*/)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline);

		string RemoveComments(string query)
		{
			query = commentRegex.Replace(query, string.Empty);
			return query;
		}

		#region Implementation

		protected override void TearDown()
		{
			BiAutomationConfigLoader.Instance.ResetConfiguration();
			base.TearDown();
		}

		BiConfigurationData ConfigData
		{
			get
			{
				if (configData == null)
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
					configData = BiAutomationConfigLoader.Instance.ConfigData;
				}
				return configData;
			}
		}
		BiConfigurationData configData;

		#endregion
	}
}
