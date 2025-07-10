using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.BR.GUI.Testing
{
	sealed class BrokeragePlugInTest : Customs.GUI.PlugIn.Testing.BaseBrokeragePlugInAbstractTest
	{
		public void TestMenuIsCorrectType()
		{
			using (var plugin = new BrokeragePlugIn(Shipment))
			{
				AssertType<EDIMenu>(plugin.TopLevelMenu);
			}
		}

		public void TestBrokerageControlIsCorrectType()
		{
			using (var plugin = new BrokeragePlugInForTesting(Shipment))
			{
				using (var control = plugin.CreateBrokerageUserControl())
				{
					AssertType<CustomsBrokerageUserControl>(control);
				}
			}
		}

		public void TestGetNewMessagingActionsController()
		{
			using (var plugin = new BrokeragePlugInForTesting(Shipment))
			{
				var control = plugin.GetNewMessagingActionsController();
				AssertType<SendsMessagesToCustomsGUI>(control);
			}
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new BrokeragePlugIn(Shipment);
	}

	class BrokeragePlugInForTesting : BrokeragePlugIn
	{
		public BrokeragePlugInForTesting(ForwardingShipment shipment) : base(shipment)
		{
		}

		public new BaseCustomsBrokerageUserControl CreateBrokerageUserControl() => base.CreateBrokerageUserControl();

		public new Customs.GUI.SendsMessagesToCustomsGUI GetNewMessagingActionsController() => base.GetNewMessagingActionsController();
	}
}
