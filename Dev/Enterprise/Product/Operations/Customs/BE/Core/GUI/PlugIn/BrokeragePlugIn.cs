using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.BE.GUI;

public class BrokeragePlugIn : EU.GUI.BrokeragePlugIn
{
	public BrokeragePlugIn(ForwardingShipment shipment)
		: base(shipment)
	{
	}

	protected override Customs.GUI.BaseCustomsBrokerageUserControl CreateBrokerageUserControl()
	{
		return new CustomsBrokerageUserControl();
	}
}
