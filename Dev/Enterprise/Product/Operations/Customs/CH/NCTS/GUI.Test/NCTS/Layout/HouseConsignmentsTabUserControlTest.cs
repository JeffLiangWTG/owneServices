using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

class HouseConsignmentsTabUserControlTest : TestCaseWithFactory
{
	public void TestTabPageVisibility() => CombineAssertions(() =>
	{
		var nctsHeader = Factory.New<Business.NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.Bills.AddNew();
		var movement = nctsHeader.MovementHeader;

		using (var form = new Phase5DepartureMovementForm(nctsHeader))
		using (var control = new HouseConsignmentsTabUserControl())
		{
			control.SetDataBinding(nctsHeader, nameof(nctsHeader.Bills));
			form.Controls.Add(control);
			form.Show();

			var tabControl = control.FindSingle<ZTabControl>("HouseConsignmentTabControl");
			var supplyChainActorsTabPage = tabControl.GetTabPage("HouseConsignmentSupplyChainActorsTabPage");

			AssertEquals($"SupplyChainActorTabPage BM_InBondEntryType='{movement.BM_InBondEntryType}'", true, supplyChainActorsTabPage.TabVisible);

			movement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
			AssertEquals($"SupplyChainActorTabPage BM_InBondEntryType='{movement.BM_InBondEntryType}'", false, supplyChainActorsTabPage.TabVisible);
		}
	});
}
