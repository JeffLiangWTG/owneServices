using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class GoodsItemsGridUserControlTest : TestCaseWithFactory
{
	public void TestHarmonisedTariffColumnStyle()
	{
		using (var control = new GoodsItemsGridUserControl())
		{
			var harmonisedTariffColumnStyle = control.GoodsItemsGrid.GetColumnStyle(NctsDepartureCargoDesc.Schema.BY_HarmonisedTariff);
			var formattedHarmonisedTariffColumnStyle = control.GoodsItemsGrid.GetColumnStyle(NctsDepartureCargoDesc.Schema.BY_FormattedHarmonisedTariff);
			CombineAssertions(() =>
			{
				AssertNull(nameof(harmonisedTariffColumnStyle), harmonisedTariffColumnStyle);
				AssertNotNull(nameof(formattedHarmonisedTariffColumnStyle), formattedHarmonisedTariffColumnStyle);
			});
		}
	}
}
