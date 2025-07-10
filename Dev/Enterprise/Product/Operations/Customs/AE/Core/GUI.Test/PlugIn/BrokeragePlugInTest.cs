using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AE.GUI.Testing;

class BrokeragePlugInTest : TestCaseWithFactory
{
	public void TestBrokerageControlIsCorrectType()
	{
		using (var plugin = new BrokeragePlugInForTest(Factory.New<ForwardingShipment>()))
		{
			using (var control = plugin.CreateBrokerageUserControl())
			{
				AssertType<CustomsBrokerageUserControl>("CreateBrokerageUserControl should return an object with correct type.", control);
			}
		}
	}
}

class BrokeragePlugInForTest : BrokeragePlugIn
{
	public BrokeragePlugInForTest(ForwardingShipment shipment) : base(shipment)
	{
	}

	public new BaseCustomsBrokerageUserControl CreateBrokerageUserControl() => base.CreateBrokerageUserControl();
}
