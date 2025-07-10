using CargoWise.Application;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting.Testing;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Accounting.GUI.Testing.JobInvoicing.Apportionment
{
	internal class ApportionmentForTransitTransportationUnitPluginTest : ApportionmentPlugInBaseTest
	{
		public override void TestMenuName()
		{
			var receiveTransportationUnit = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsItemReceiveTransportationUnit>());
			using (var plugIn = new ApportionmentForTransitTransportationUnitPlugin(receiveTransportationUnit))
			{
				AssertEquals("&Costing", plugIn.MenuName);
			}

			var dispatchTransportationUnit = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsItemDispatchTransportationUnit>());
			using (var plugIn = new ApportionmentForTransitTransportationUnitPlugin(dispatchTransportationUnit))
			{
				AssertEquals("&Costing", plugIn.MenuName);
			}
		}
	}
}
