using System.Windows.Forms;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public class BrokeragePlugIn : BrokeragePlugInOneToOne
	{
		public BrokeragePlugIn(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		protected override BaseCustomsBrokerageUserControl CreateBrokerageUserControl() => new CustomsBrokerageUserControl();

		MenuItem fTopLevelMenu;
		protected override MenuItem GetNewTopLevelMenuCore() => fTopLevelMenu ?? (fTopLevelMenu = new EDIMenu());
	}
}
