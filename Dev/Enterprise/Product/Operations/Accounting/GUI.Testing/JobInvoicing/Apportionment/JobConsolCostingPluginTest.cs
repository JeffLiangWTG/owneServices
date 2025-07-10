using CargoWise.Application;
using Enterprise.Accounting.GUI.Testing.JobInvoicing.Apportionment;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting.Testing
{
	public class JobConsolCostingPluginTest : ApportionmentPluginTest
	{
		public void TestMenuName()
		{
			var receiveTransportationUnit = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsItemReceiveTransportationUnit>());
			using (var plugIn = new JobConsolCostingPlugin(receiveTransportationUnit))
			{
				AssertEquals("&Costing", plugIn.MenuName);
			}

			var dispatchTransportationUnit = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsItemDispatchTransportationUnit>());
			using (var plugIn = new JobConsolCostingPlugin(dispatchTransportationUnit))
			{
				AssertEquals("&Costing", plugIn.MenuName);
			}
		}

		public void TestIsSpecifiedNotToAddMenuItem()
		{
			var receiveTransportationUnit = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsItemReceiveTransportationUnit>());
			using (var plugIn = new JobConsolCostingPlugin(receiveTransportationUnit))
			{
				AssertEquals(true, plugIn.IsSpecifiedNotToAddMenuItem);
			}
		}
	}
}
