using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CN.GUI.Testing
{
	class BrokeragePlugInTest : TestCaseWithFactory
	{
		public void TestBrokerageControlIsCorrectType()
		{
			using (var plugin = new BrokeragePlugInForTest(Factory.New<ForwardingShipment>()))
			{
				using (var control = plugin.CreateBrokerageUserControl())
				{
					AssertEquals(typeof(CustomsBrokerageUserControl), control.GetType());
				}
			}
		}

		public void TestCreateDeclarationHelperIsCorrectType()
		{
			using (var plugin = new BrokeragePlugInForTest(Factory.New<ForwardingShipment>()))
			{
				AssertType<CreateDeclarationHelper>(plugin.CreateDeclarationHelper);
			}
		}
	}

	class BrokeragePlugInForTest : BrokeragePlugIn
	{
		public BrokeragePlugInForTest(ForwardingShipment shipment) : base(shipment)
		{
		}

		public new BaseCustomsBrokerageUserControl CreateBrokerageUserControl() => base.CreateBrokerageUserControl();

		public new CreateDeclarationHelper CreateDeclarationHelper => (CreateDeclarationHelper)base.CreateDeclarationHelper;
	}
}
