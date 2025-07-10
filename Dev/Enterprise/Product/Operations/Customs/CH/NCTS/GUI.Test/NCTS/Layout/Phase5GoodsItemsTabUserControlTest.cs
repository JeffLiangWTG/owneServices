using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(Phase5GoodsItemsTabUserControl))]
sealed class Phase5GoodsItemsTabUserControlTest : TestCaseWithFactory
{
	public void TestGoodsItemSupplyChainActorsTabPageVisibility() => CombineAssertions(() =>
	{
		var nctsHeader = Factory.New<Business.NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var departureMovement = nctsHeader.MovementHeader;
		var bill = nctsHeader.Bills.AddNew();
		bill.GoodsItems.AddNew();

		using (var form = new Phase5DepartureMovementForm(nctsHeader))
		using (var control = new Phase5GoodsItemsTabUserControl())
		{
			control.SetDataBinding(nctsHeader, $"{nameof(NctsHeader.Bills)}.{nameof(NctsBill.GoodsItems)}");
			form.Controls.Add(control);
			form.Show();

			var goodsItemSupplyChainActorsTabPage = control.GoodsItemTabControl.GetTabPage("GoodsItemSupplyChainActorsTabPage");

			AssertEquals($"BM_InBondEntryType = '{departureMovement.BM_InBondEntryType}'", true, goodsItemSupplyChainActorsTabPage.TabVisible);

			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
			AssertEquals($"BM_InBondEntryType = '{departureMovement.BM_InBondEntryType}'", false, goodsItemSupplyChainActorsTabPage.TabVisible);

			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
			AssertEquals($"BM_InBondEntryType = '{departureMovement.BM_InBondEntryType}'", true, goodsItemSupplyChainActorsTabPage.TabVisible);
		}
	});

	public void TestRestrictionsTabVisibility() => CombineAssertions(() =>
	{
		var nctsHeader = Factory.New<Business.NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var departureMovement = nctsHeader.MovementHeader;
		var bill = nctsHeader.Bills.AddNew();
		bill.GoodsItems.AddNew();

		using (var form = new Phase5DepartureMovementForm(nctsHeader))
		using (var control = new Phase5GoodsItemsTabUserControl())
		{
			control.SetDataBinding(nctsHeader, $"{nameof(NctsHeader.Bills)}.{nameof(NctsBill.GoodsItems)}");
			form.Controls.Add(control);
			form.Show();

			AssertEquals($"BM_InBondEntryType='{departureMovement.BM_InBondEntryType}' (initial)", false, IsRestrictionsTabVisible());

			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
			AssertEquals($"BM_InBondEntryType='{departureMovement.BM_InBondEntryType}'", true, IsRestrictionsTabVisible());

			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
			AssertEquals($"BM_InBondEntryType='{departureMovement.BM_InBondEntryType}'", false, IsRestrictionsTabVisible());

			bool IsRestrictionsTabVisible() => control.GoodsItemTabControl.GetTabPage(nameof(RestrictionsTabPage))?.TabVisible ?? false;
		}
	});
}
