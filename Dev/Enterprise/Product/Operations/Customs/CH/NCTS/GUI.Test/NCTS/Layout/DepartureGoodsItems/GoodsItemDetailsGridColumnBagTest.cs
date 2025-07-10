using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

class GoodsItemDetailsGridColumnBagTest : TestCase
{
	public void TestFormattedHarmonisedTariffColumn()
	{
		AssertNotNull(ColumnsBag.FormattedHarmonisedTariffColumn);
		var columnInfo = ColumnsBag.FormattedHarmonisedTariffColumn.CreateGridColumnInfo() as NctsTariffColumnStyleInfo;
		AssertNotNull(columnInfo);
		CombineAssertions(() =>
		{
			AssertNull("CountryCode", columnInfo.GetCountryCode?.Invoke());
			AssertNull("SelectNomenclatureModes", columnInfo.SelectNomenclatureModes);
			AssertEquals("ShowDescriptionFilterOnNonNomenclatureTariffModule", false, columnInfo.ShowDescriptionFilterOnNonNomenclatureTariffModule);
			AssertEquals("ColumnName", "BY_FormattedHarmonisedTariff", columnInfo.ColumnName);
		});
	}

	GoodsItemDetailsGridColumnBag ColumnsBag => GoodsItemDetailsGridColumnBag.Instance;
}
