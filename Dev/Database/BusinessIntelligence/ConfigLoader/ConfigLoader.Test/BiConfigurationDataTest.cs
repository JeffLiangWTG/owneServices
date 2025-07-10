using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Resource.Shared;
using NUnit.Framework;

namespace CargoWise.Bi.ConfigLoader
{
	public class BiConfigurationDataTest : TestCase
	{
		protected override void TearDown()
		{
			BiAutomationConfigLoader.Instance.ResetConfiguration();
			base.TearDown();
		}

		public void TestLoadConfiguration()
		{
			var configData = new BiConfigurationData();
			var cdcTableConfiguration = configData.CdcTableConfig;
			AssertNotEquals("CdcTableConfig", cdcTableConfiguration.Count, 0);

			var edwTableConfiguration = configData.EdwTableConfig;
			AssertNotEquals("EdwTableConfig", edwTableConfiguration.Count, 0);

			var edwDenormalizedTableConfiguration = configData.EdwDenormalizedTableConfig;
			AssertNotEquals("EdwDenormalizedTableConfig", edwDenormalizedTableConfiguration.Count, 0);

			var edwCustomTableConfiguration = configData.EdwCustomTableConfig;
			AssertNotEquals("EdwCustomTableConfig", edwCustomTableConfiguration.Count, 0);

			var edwModelViewTableConfiguration = configData.EdwModelViewTableConfig;
			AssertNotEquals("EdwModelViewTableConfig", edwModelViewTableConfiguration.Count, 0);

			var xmlDataSources = configData.XmlDataSources;
			AssertNotEquals("XmlDataSources", xmlDataSources.Count, 0);
		}

		class BiConfigurationDataForTest : BiConfigurationData
		{
			public BiConfigurationDataForTest()
			{
			}

			public BiConfigurationDataForTest(string schema, string table, params string[] columns)
			{
				this.schema = schema;
				this.table = table;
				this.columns = columns;
			}

			protected override IEnumerable<CdcRequiredTable> LoadCdcConfigurationFromShared()
			{
				InitCdcTableConfigWithTestData();
				return string.IsNullOrEmpty(schema) ?
					Enumerable.Empty<CdcRequiredTable>() :
					new[] { new CdcRequiredTable(schema, table, columns) };
			}

			void InitCdcTableConfigWithTestData()
			{
				var prop = typeof(BiConfigurationData).GetProperty("ConfigurationDataSet", BindingFlags.NonPublic | BindingFlags.Instance);
				var getter = prop.GetGetMethod(nonPublic: true);
				var configurationDataSet = (BiAutomationConfigDataSet)getter.Invoke(this, null);

				var cdcTableConfig = configurationDataSet.CdcTableConfig.AddCdcTableConfigRow("TestSchema", "TestTable", "", "", "", false, false, "", "", "", false);
				configurationDataSet.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TestColumn", "int", 9, 8, 7, true, true, "", "", false, "", "", false, false, "");
			}

			readonly string schema;
			readonly string table;
			readonly string[] columns;
		}

		public void TestLoadCdcConfiguration()
		{
			string schema = "TestSchema";
			string table = "TestTable";
			string column = "TestColumn";
			var configData = new BiConfigurationDataForTest();
			AssertEquals(configData.CdcTableConfig.Single(x => x.SourceSchema == schema && x.SourceTable == table).TableInAudit, false);

			var configDataWithCdcConfigurationFromShared = new BiConfigurationDataForTest(schema, table, column);
			AssertEquals(configDataWithCdcConfigurationFromShared.CdcTableConfig.Single(x => x.SourceSchema == schema && x.SourceTable == table).TableInAudit, true);
			var cdcColumn = configDataWithCdcConfigurationFromShared.CdcColumnConfig.Single(x => x.SourceTable == table && x.SourceColumn == column);
			AssertEquals(cdcColumn.CdcEnabled, true);
			AssertEquals(cdcColumn.ColumnInAudit, true);
		}

		[ExpectNoExceptions]
		public void TestValidate()
		{
			var configData = new BiConfigurationData();
			configData.Validate();
		}

		[ExpectNoExceptions]
		public void TestConfigurationDataIsThreadSafe()
		{
			BiAutomationConfigLoader.Instance.ResetConfiguration();

			var threads = new List<Thread>();
			for (int i = 0; i < 10; i++)
			{
				var thread = new Thread(new ThreadStart(() =>
				{
					var edwConfig = BiAutomationConfigLoader.Instance.ConfigData.EdwTableConfig;
				}));
				threads.Add(thread);
			}

			foreach (var thread in threads)
			{
				thread.Start();
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}
		}
	}
}
