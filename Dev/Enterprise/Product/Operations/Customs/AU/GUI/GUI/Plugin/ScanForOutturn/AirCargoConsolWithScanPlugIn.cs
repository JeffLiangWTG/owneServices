using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public class AirCargoConsolWithScanPlugIn : AirCargoConsolPlugIn
	{
		public AirCargoConsolWithScanPlugIn(ForwardingConsol hostBusinessEntity)
			: base(hostBusinessEntity)
		{
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			AirScanForOutturnHost scanHost = null;

			if (this.Form != null && MasterBill != null)
			{
				scanHost = new AirScanForOutturnHost(this.Form, MasterBill);
			}

			var menu = AirCargoMasterMenuWithScan.New(Consol, Manager, scanHost);
			menu.Enabled = IsFormEditable;
			menu.ParentForm = Form;
			return menu;
		}
	}
}
