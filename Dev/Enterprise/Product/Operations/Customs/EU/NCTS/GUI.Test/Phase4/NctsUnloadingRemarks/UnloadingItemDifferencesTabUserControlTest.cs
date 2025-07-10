using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class UnloadingItemDifferencesTabUserControlTest : TestCaseWithFactory
	{
		public void TestTariffColumnStyle()
		{
			using (var control = new UnloadingItemDifferencesTabUserControl())
			{
				AssertType<ZArchitecture.ZTextBoxColumnStyleInfo>(control.GoodsItemsGrid.GetColumnStyle(nameof(NctsCommonCargoDesc.BY_FormattedHarmonisedTariff)));
			}
		}
	}
}
