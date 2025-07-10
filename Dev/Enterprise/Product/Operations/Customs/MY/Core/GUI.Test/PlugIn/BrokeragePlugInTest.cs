using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.MY.GUI.Testing
{
	sealed class BrokeragePlugInTest : Customs.GUI.PlugIn.Testing.BaseBrokeragePlugInAbstractTest
	{
		public void TestMenuIsCorrectType()
		{
			using (BrokeragePlugIn plugin = new BrokeragePlugIn(Factory.New<ForwardingShipment>()))
			{
				AssertEquals(typeof(EDIMenu), plugin.TopLevelMenu.GetType());
			}
		}

		public void TestBrokerageControlIsCorrectType()
		{
			using (var plugin = new BrokeragePlugIn(Factory.New<ForwardingShipment>()))
			{
				AssertType<CustomsBrokerageUserControl>(plugin.UserControl);
			}
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new BrokeragePlugIn(Shipment);
	}
}
