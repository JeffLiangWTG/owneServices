using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.CA.GUI.PlugIns.Testing
{
	sealed class RNSPlugInTest : ZPlugInGenericTest
	{
		public void TestOverrides()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var plugin = new RNSPlugIn(new RNSPlugInSupportShipmentWrapper(shipment)))
			{
				AssertEquals("Name", "RNS", plugin.Name);
				AssertEquals("TopLevelMenu", typeof(RNSMenu), plugin.TopLevelMenu.GetType());
				AssertEquals("UserControl", typeof(RNSUserControl), plugin.UserControl.GetType());

				AssertEquals("Should be disabled for export", false, plugin.Enabled);
				shipment.JS_RL_NKOrigin = "USAAA";
				shipment.JS_RL_NKDestination = "CABBB";
				AssertEquals("Should be enabled for import", true, plugin.Enabled);
			}
		}

		public void TestGatePassShipment()
		{
			var shipment = Factory.New<GatePassShipment>();
			using (var plugin = new RNSPlugIn(new RNSPlugInSupportShipmentWrapper(shipment)))
			{
				AssertEquals("Name", "RNS", plugin.Name);
				AssertNull("TopLevelMenu", plugin.TopLevelMenu);
			}
		}

		protected override ZPlugIn GetPlugInToTest() => new RNSPlugIn(new RNSPlugInSupportShipmentWrapper(Factory.New<ForwardingShipment>()));
	}
}
