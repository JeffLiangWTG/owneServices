using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CN.GUI.Testing
{
	sealed class BrokeragePlugIn01Test : Customs.GUI.PlugIn.Testing.BaseBrokeragePlugInAbstractTest
	{
		public void TestMenuIsCorrectType()
		{
			using (var plugin = new BrokeragePlugIn(Factory.New<ForwardingShipment>()))
			{
				AssertEquals(typeof(EDIMenu), plugin.TopLevelMenu.GetType());
			}
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new BrokeragePlugIn(Shipment);
	}
}
