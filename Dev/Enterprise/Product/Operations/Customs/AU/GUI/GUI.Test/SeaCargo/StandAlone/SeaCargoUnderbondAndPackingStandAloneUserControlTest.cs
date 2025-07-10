using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoUnderbondAndPackingStandAloneUserControlTest : TestCaseWithFactory
	{
		public void TestContainerDetailsReadonly()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OB001";
			var house = oceanBill.FilteredHouseBills.AddNew();
			house.CA_HouseBill = "HB1";
			var pivot = house.Pivot.AddNew();
			var container = oceanBill.Containers.AddNew();
			container.Pivots.Add(pivot);
			AssertSame("pivot.Container is container", container, pivot.Container);
			using (var testForm = new ZForm(house))
			using (var packingUserControl = new SeaCargoUnderbondAndPackingStandAloneUserControl())
			{
				testForm.Controls.Add(packingUserControl);
				packingUserControl.SetDataBinding(house, "");
				testForm.Show();
				var containersGrid = testForm.FindSingle<ZArchitecture.ZGrid>("ContainersGrid");
				AssertEquals("containersGrid.Count", 1, containersGrid.ListManager.Count);
				containersGrid.CurrentRowIndex = 1;
				CombineAssertions(() =>
				{
					AssertEquals("Associated Container is never readonly", false, containersGrid.GetColumnStyle("CV_AssociatedContainer").IsReadOnly);
					AssertEquals("CN_SealNumber IsReadOnly", true, containersGrid.GetColumnStyle("CN_SealNumber").IsReadOnly);
					AssertEquals("CN_ContainerMode IsReadOnly", true, containersGrid.GetColumnStyle("CN_ContainerMode").IsReadOnly);
					AssertEquals("CN_ContainerType IsReadOnly", true, containersGrid.GetColumnStyle("CN_ContainerType").IsReadOnly);
					AssertEquals("CN_ShipperOwnedContainer IsReadOnly", true, containersGrid.GetColumnStyle("CN_ShipperOwnedContainer").IsReadOnly);
				});
			}
		}
	}
}
