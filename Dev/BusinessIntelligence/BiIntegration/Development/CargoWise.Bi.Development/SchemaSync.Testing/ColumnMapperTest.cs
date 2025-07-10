using System.Linq;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Bi.Developement.SchemaSync;
using CargoWise.Bi.Development.Common;
using NUnit.Framework;

sealed class ColumnMapperTest : TestCase
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	[SnailTest]
	public void TestSourceToModelColumnMapper()
	{
		var mapper = new ColumnMapper();

		var mappings = mapper.GetViewMappingFromStringOfColumns("TST_SourceColumn");
		AssertEquals("Number of mappings found", 1, mappings.Count());
		CombineAssertions(() =>
		{
			var mapping = mappings.First();
			AssertEquals("SourceSchema", null, mapping.SourceSchema);
			AssertEquals("SourceTable", null, mapping.SourceTable);
			AssertEquals("SourceColumn", "TST_SourceColumn", mapping.SourceColumn);
			AssertEquals("ModelSchema", null, mapping.ModelSchema);
			AssertEquals("ModelView", null, mapping.ModelView);
			AssertEquals("ModelColumn", null, mapping.ModelColumn);
		});

		CreateSourceTable();

		mappings = mapper.GetViewMappingFromStringOfColumns("TST_SourceColumn");
		AssertEquals("Number of mappings found", 1, mappings.Count());
		CombineAssertions(() =>
		{
			var mapping = mappings.First();
			AssertEquals("SourceSchema", "TestSourceSchema", mapping.SourceSchema);
			AssertEquals("SourceTable", "TestSourceTable", mapping.SourceTable);
			AssertEquals("SourceColumn", "TST_SourceColumn", mapping.SourceColumn);
			AssertEquals("ModelSchema", null, mapping.ModelSchema);
			AssertEquals("ModelView", null, mapping.ModelView);
			AssertEquals("ModelColumn", null, mapping.ModelColumn);
		});

		CreateEdwTables();
		mapper.MapCdcColumnsToModelView();

		mappings = mapper.GetViewMappingFromStringOfColumns("TST_SourceColumn");
		AssertEquals("Number of mappings found", 1, mappings.Count());
		CombineAssertions(() =>
		{
			var mapping = mappings.First();
			AssertEquals("SourceSchema", "TestSourceSchema", mapping.SourceSchema);
			AssertEquals("SourceTable", "TestSourceTable", mapping.SourceTable);
			AssertEquals("SourceColumn", "TST_SourceColumn", mapping.SourceColumn);
			AssertEquals("ModelSchema", "TestModelSchema", mapping.ModelSchema);
			AssertEquals("ModelView", "TestModelView", mapping.ModelView);
			AssertEquals("ModelColumn", "TestModelColumn", mapping.ModelColumn);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		BiAutomationConfigLoaderForDevelopment.Instance.ResetConfiguration();
	}

	protected override void TearDown()
	{
		BiAutomationConfigLoaderForDevelopment.Instance.ResetConfiguration();
		base.TearDown();
	}

	void CreateSourceTable()
	{
		var cdcTable = ConfigData.CdcTableConfig.AddCdcTableConfigRow("TestSourceSchema", "TestSourceTable", "TestModelSchema", "", "", true, true, null, null, null, false);
		ConfigData.CdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "TST_SourceColumn", "int", 4, 0, 0, true, false, "", "", true, "", "", true, true, "");
	}

	void CreateEdwTables()
	{
		var edwTable = ConfigData.EdwTableConfig.AddEdwTableConfigRow("TestModelSchema", "BAS__TestBaseTable", "", "TestSourceSchema", "TestSourceTable", "", 1, false, 1, "");
		ConfigData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "int", 4, 0, 0, "TestColumn", "TST_SourceColumn", false, false, "", "", "", "", "", "", false);

		var denormTable = ConfigData.EdwDenormalizedTableConfig.AddEdwDenormalizedTableConfigRow("TestModelSchema", "AGG__TestAggregateTable", "[TestModelSchema].[BAS__TestBaseTable] as base", -1, false, "", "");
		ConfigData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(denormTable, "TestColumn", "int", 4, 0, 0, "base.[TestColumn]", false, "", false);

		var modelView = ConfigData.EdwModelViewTableConfig.AddEdwModelViewTableConfigRow("TestModelSchema", "TestModelView", "[TestModelSchema].[AGG__TestAggregateTable] as agg", "", "");
		ConfigData.EdwModelViewColumnConfig.AddEdwModelViewColumnConfigRow(modelView, "TestModelColumn", "[TestColumn]", "", "");
	}

	BiAutomationConfigDataSet configData;
	BiAutomationConfigDataSet ConfigData => configData ?? (configData = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData);
}
