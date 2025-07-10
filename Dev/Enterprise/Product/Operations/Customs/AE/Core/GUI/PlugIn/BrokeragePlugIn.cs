using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AE.GUI;

public class BrokeragePlugIn : BrokeragePlugInOneToOne
{
	public BrokeragePlugIn(ForwardingShipment shipment) : base(shipment)
	{
	}

	protected override BaseCustomsBrokerageUserControl CreateBrokerageUserControl() => new CustomsBrokerageUserControl();
}
