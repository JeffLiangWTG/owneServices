using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class Phase5GoodsItemDifferencesDetailsGridColumnsBagTest : TestCase
	{
		public void TestLineNoTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.LineNoTextBoxColumn);
			var columnInfo = ColumnsBag.LineNoTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_LineNo", columnInfo.ColumnName);
		}

		public void TestDeclarationGoodsItemNumberTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.DeclarationGoodsItemNumberTextBoxColumn);
			var columnInfo = ColumnsBag.DeclarationGoodsItemNumberTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_DeclarationGoodsItemNumber", columnInfo.ColumnName);
		}

		public void TestUnloadedState()
		{
			AssertNotNull(ColumnsBag.UnloadedStateDropEditColumn);
			var columnInfo = ColumnsBag.UnloadedStateDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_UnloadedState", columnInfo.ColumnName);
		}

		public void TestDescriptionTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.DescriptionTextBoxColumn);
			var columnInfo = ColumnsBag.DescriptionTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_Description", columnInfo.ColumnName);
		}

		public void TestGrossWeightCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.GrossWeightCalcEditColumn);
			var columnInfo = ColumnsBag.GrossWeightCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertNull("BindToDecimalPlaces", columnInfo.BindToDecimalPlaces);
				AssertEquals("ColumnName", "BY_GrossWeight", columnInfo.ColumnName);
			});
		}

		public void TestGrossWeightUnitDropEditColumn()
		{
			AssertNotNull(ColumnsBag.GrossWeightUnitDropEditColumn);
			var columnInfo = ColumnsBag.GrossWeightUnitDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_GrossWeightUnit", columnInfo.ColumnName);
		}

		public void TestNetWeightCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.NetWeightCalcEditColumn);
			var columnInfo = ColumnsBag.NetWeightCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertNull("BindToDecimalPlaces", columnInfo.BindToDecimalPlaces);
				AssertEquals("ColumnName", "BY_NetWeight", columnInfo.ColumnName);
			});
		}

		public void TestNetWeightUnitDropEditColumn()
		{
			AssertNotNull(ColumnsBag.NetWeightUnitDropEditColumn);
			var columnInfo = ColumnsBag.NetWeightUnitDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_NetWeightUnit", columnInfo.ColumnName);
		}

		public void TestFormattedHarmonisedTariffColumn()
		{
			AssertNotNull(ColumnsBag.FormattedHarmonisedTariffColumn);
			var columnInfo = ColumnsBag.FormattedHarmonisedTariffColumn.CreateGridColumnInfo() as TariffColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertNull("CountryCode", columnInfo.GetCountryCode?.Invoke());
				AssertNull("SelectNomenclatureModes", columnInfo.SelectNomenclatureModes);
				AssertEquals("ShowDescriptionFilterOnNonNomenclatureTariffModule", false, columnInfo.ShowDescriptionFilterOnNonNomenclatureTariffModule);
				AssertEquals("ColumnName", "BY_FormattedHarmonisedTariff", columnInfo.ColumnName);
				AssertNull("TariffType", columnInfo.TariffType);
			});
		}

		public void TestCusC4NumberCodeFindBoxColumn()
		{
			AssertNotNull(ColumnsBag.CusC4NumberCodeFindBoxColumn);
			var columnInfo = ColumnsBag.CusC4NumberCodeFindBoxColumn.CreateGridColumnInfo() as ZCodeFindBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_CusC4Number", columnInfo.ColumnName);
		}

		Phase5GoodsItemDifferencesDetailsGridColumnsBag ColumnsBag => Phase5GoodsItemDifferencesDetailsGridColumnsBag.Instance;
	}
}
