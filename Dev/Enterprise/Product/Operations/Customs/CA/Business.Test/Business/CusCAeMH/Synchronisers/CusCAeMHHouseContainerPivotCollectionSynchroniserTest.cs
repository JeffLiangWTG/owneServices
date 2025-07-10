using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHHouseContainerPivotCollectionSynchroniserTest : TestCaseWithFactory
	{
		public void TestCusCAeMHHouseContainerPivotCollectionSynchroniser()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var shipment = helper.Shipment;
			var house = helper.House;
			var container1 = helper.MasterBill.Containers.AddNew();
			container1.BQ_ContainerNumber = ConsolPluginTestHelper.Container1Num;
			var container2 = helper.MasterBill.Containers.AddNew();
			container2.BQ_ContainerNumber = ConsolPluginTestHelper.Container2Num;
			var synchroniser = new CusCAeMHHouseContainerPivotCollectionSynchroniser(shipment, house, helper.Consol);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_JC = helper.Container1.PK;
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(helper.Consol, null);
			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.SetContainer(helper.Consol, null);
			AssertEquals(2, house.Pivots.Count);
			AssertNotNull(house.Pivots.FirstOrDefault(x => x.BPA_Calc_ContainerNumber == ConsolPluginTestHelper.Container1Num));
			AssertNotNull(house.Pivots.FirstOrDefault(x => x.BPA_Calc_ContainerNumber == Core.Constants.ContainerModes.NonContainerised));
			packLine2.JL_JC = helper.Container2.PK;
			AssertEquals(3, house.Pivots.Count);
			AssertNotNull(house.Pivots.FirstOrDefault(x => x.BPA_Calc_ContainerNumber == ConsolPluginTestHelper.Container1Num));
			AssertNotNull(house.Pivots.FirstOrDefault(x => x.BPA_Calc_ContainerNumber == ConsolPluginTestHelper.Container2Num));
			AssertNotNull(house.Pivots.FirstOrDefault(x => x.BPA_Calc_ContainerNumber == Core.Constants.ContainerModes.NonContainerised));
			packLine3.JL_JC = helper.Container2.PK;
			AssertEquals(2, house.Pivots.Count);
			AssertNotNull(house.Pivots.FirstOrDefault(x => x.BPA_Calc_ContainerNumber == ConsolPluginTestHelper.Container1Num));
			AssertNotNull(house.Pivots.FirstOrDefault(x => x.BPA_Calc_ContainerNumber == ConsolPluginTestHelper.Container2Num));
			packLine1.JL_JC = helper.Container2.PK;
			AssertEquals(1, house.Pivots.Count);
			AssertNotNull(house.Pivots.FirstOrDefault(x => x.BPA_Calc_ContainerNumber == ConsolPluginTestHelper.Container2Num));
			packLine3.JL_JC = ZGuid.Empty;
			AssertEquals(2, house.Pivots.Count);
			AssertNotNull(house.Pivots.FirstOrDefault(x => x.BPA_Calc_ContainerNumber == ConsolPluginTestHelper.Container2Num));
			AssertNotNull(house.Pivots.FirstOrDefault(x => x.BPA_Calc_ContainerNumber == Core.Constants.ContainerModes.NonContainerised));
			packLine1.JL_JC = ZGuid.Empty;
			packLine2.JL_JC = ZGuid.Empty;
			AssertEquals(1, house.Pivots.Count);
			AssertNotNull(house.Pivots.FirstOrDefault(x => x.BPA_Calc_ContainerNumber == Core.Constants.ContainerModes.NonContainerised));
			shipment.OuterPackLines.RemoveAndDeleteAll();
			AssertEquals(0, house.Pivots.Count);
		}

		public void TestHasChanges()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var shipment = helper.Shipment;
			var house = helper.House;
			var container = helper.MasterBill.Containers.AddNew();
			container.BQ_ContainerNumber = ConsolPluginTestHelper.Container1Num;
			var synchroniser = new CusCAeMHHouseContainerPivotCollectionSynchroniser(shipment, house, helper.Consol);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_JC = helper.Container1.PK;
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(helper.Consol, null);
			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.SetContainer(helper.Consol, null);
			AssertEquals(2, house.Pivots.Count);
			Factory.Save();
			Assert("Precondition: HasChanges", !house.HasChanges);

			shipment.RefreshBindingIncludingChildren();
			Assert("HasChanges should NOT be set", !house.HasChanges);
		}
	}
}
