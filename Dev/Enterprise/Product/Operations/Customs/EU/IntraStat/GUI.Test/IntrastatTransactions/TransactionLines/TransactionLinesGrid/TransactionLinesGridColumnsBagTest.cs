using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing
{
	sealed class TransactionLinesGridColumnsBagTest : TestCase
	{
		public void TestDescriptionTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.DescriptionTextBoxColumn);
			var columnInfo = ColumnsBag.DescriptionTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CIL_DescriptionOfGoods", columnInfo.ColumnName);
		}

		public void TestTariffColumn()
		{
			AssertNotNull(ColumnsBag.TariffColumn);
			var columnInfo = ColumnsBag.TariffColumn.CreateGridColumnInfo() as TariffColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CIL_FormattedTariff", columnInfo.ColumnName);
			AssertNull("SelectNomenclatureModes", columnInfo.SelectNomenclatureModes);
			AssertEquals("ShowDescriptionFilterOnNonNomenclatureTariffModule", false, columnInfo.ShowDescriptionFilterOnNonNomenclatureTariffModule);
			AssertEquals("TariffType", "EXP", columnInfo.TariffType);
		}

		public void TestInvoiceValueCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.InvoiceValueCalcEditColumn);
			var columnInfo = ColumnsBag.InvoiceValueCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CIL_InvoiceValue", columnInfo.ColumnName);
			AssertEquals("BindToDecimalPlaces", null, columnInfo.BindToDecimalPlaces);
		}

		public void TestStatisticalValueCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.StatisticalValueCalcEditColumn);
			var columnInfo = ColumnsBag.StatisticalValueCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CIL_StatisticalValue", columnInfo.ColumnName);
			AssertEquals("BindToDecimalPlaces", null, columnInfo.BindToDecimalPlaces);
		}

		public void TestMassInKilogramsCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.MassInKilogramsCalcEditColumn);
			var columnInfo = ColumnsBag.MassInKilogramsCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CIL_MassInKilograms", columnInfo.ColumnName);
			AssertEquals("BindToDecimalPlaces", null, columnInfo.BindToDecimalPlaces);
		}

		public void TestSupplementaryQuantityCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.SupplementaryQuantityCalcEditColumn);
			var columnInfo = ColumnsBag.SupplementaryQuantityCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CIL_SupplementaryQuantity", columnInfo.ColumnName);
			AssertEquals("BindToDecimalPlaces", null, columnInfo.BindToDecimalPlaces);
		}

		public void TestCurrencyDropEditColumn()
		{
			AssertNotNull(ColumnsBag.CurrencyDropEditColumn);
			var columnInfo = ColumnsBag.CurrencyDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CIL_RX_NKCurrency", columnInfo.ColumnName);
		}

		public void TestMassInKilogramsUnitDropEditColumn()
		{
			AssertNotNull(ColumnsBag.MassInKilogramsUnitDropEditColumn);
			var columnInfo = ColumnsBag.MassInKilogramsUnitDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CIL_MassInKilogramsUnit", columnInfo.ColumnName);
		}

		public void TestSupplementaryQuantityUnit()
		{
			AssertNotNull(ColumnsBag.SupplementaryQuantityUnit);
			var columnInfo = ColumnsBag.SupplementaryQuantityUnit.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CIL_SupplementaryQuantityUnit", columnInfo.ColumnName);
		}

		public void TestCountryOfOriginDropEditColumn()
		{
			AssertNotNull(ColumnsBag.CountryOfOriginDropEditColumn);
			var columnInfo = ColumnsBag.CountryOfOriginDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CIL_RN_NKCountryOfOrigin", columnInfo.ColumnName);
		}

		public void TestRegionDropEditColumn()
		{
			AssertNotNull(ColumnsBag.RegionDropEditColumn);
			var columnInfo = ColumnsBag.RegionDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CIL_Region", columnInfo.ColumnName);
		}

		TransactionLinesGridColumnsBag ColumnsBag => TransactionLinesGridColumnsBag.Instance;
	}
}
