using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.FI.GUI
{
	public class BrokeragePlugIn : EU.GUI.BrokeragePlugIn
	{
		public BrokeragePlugIn(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		protected override BaseCustomsBrokerageUserControl CreateBrokerageUserControl() => new CustomsBrokerageUserControl();
	}
}
