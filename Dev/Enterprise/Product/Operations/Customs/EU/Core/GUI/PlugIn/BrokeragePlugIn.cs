using System.Windows.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.GUI
{
	public class BrokeragePlugIn : BrokeragePlugInOneToOne
	{
		public BrokeragePlugIn(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		protected override Customs.Business.CreateDeclarationHelper GetCreateDeclarationHelperCore() => new CreateDeclarationHelper();

		protected override BaseCustomsBrokerageUserControl CreateBrokerageUserControl() => new CustomsBrokerageUserControl();

		protected override MenuItem GetNewTopLevelMenuCore() => new EDIMenu();
	}
}

