using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHHouseContainerPivotSynchroniserTest : TestCaseWithFactory
	{
		public void TestThrowExceptionWhenDestinationIsDeleted()
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

			house.Pivots.DeleteAll();
			AssertNoExceptionThrown(() => synchroniser.Synchronise(true));
		}

		public void TestCusCAeMHHouseContainerPivotSynchroniser()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var packLine = helper.Shipment.OuterPackLines.AddNew();
			packLine.JL_JC = helper.Container1.PK;
			var pivot = helper.House.Pivots.AddNew();
			var container = helper.MasterBill.Containers.AddNew();
			container.BQ_ContainerNumber = ConsolPluginTestHelper.Container1Num;

			var synchroniser = new CusCAeMHHouseContainerPivotSynchroniser(pivot, packLine);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			AssertEquals(container.PK, pivot.BPA_BQ_Container);
		}
	}
}
