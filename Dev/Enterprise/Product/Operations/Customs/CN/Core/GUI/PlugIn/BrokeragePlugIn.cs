using System.Windows.Forms;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CN.GUI
{
	public class BrokeragePlugIn : BrokeragePlugInOneToOne
	{
		public BrokeragePlugIn(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		protected override Customs.Business.CreateDeclarationHelper GetCreateDeclarationHelperCore()
		{
			return new CreateDeclarationHelper();
		}

		protected override BaseCustomsBrokerageUserControl CreateBrokerageUserControl()
		{
			return new CustomsBrokerageUserControl();
		}

		protected MenuItem fTopLevelMenu;
		protected override MenuItem GetNewTopLevelMenuCore()
		{
			if (fTopLevelMenu == null)
			{
				fTopLevelMenu = new EDIMenu();
			}
			return fTopLevelMenu;
		}
	}
}
