using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IE.GUI
{
	public class BrokeragePlugIn : EU.GUI.BrokeragePlugIn
	{
		public BrokeragePlugIn(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		protected override Customs.GUI.BaseCustomsBrokerageUserControl CreateBrokerageUserControl() => new CustomsBrokerageUserControl();

		protected override MenuItem GetNewTopLevelMenuCore() => new EDIMenu();
	}
}
