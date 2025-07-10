using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public class SeaCargoConsolWithScanPlugin : SeaCargoConsolPlugIn
	{
		public SeaCargoConsolWithScanPlugin(ForwardingConsol hostBusinessEntity)
			: base(hostBusinessEntity)
		{ }

		protected override System.Windows.Forms.MenuItem GetNewTopLevelMenu()
		{
			SetExistingOceanBill();
			SeaScanForOutturnHost scanHost = null;

			if (this.Form != null && OceanBill is CusSCAOceanBill)
			{
				scanHost = new SeaScanForOutturnHost(this.Form, new ScanCusSCAOceanBill(OceanBill));
			}

			var menu = new SeaCargoConsolMenuWithScan(this, Manager, scanHost);
			menu.Enabled = IsFormEditable;
			menu.ParentForm = Form;
			return menu;
		}

		protected override void OnIsFormEditableChanged()
		{
			base.OnIsFormEditableChanged();
			if (TopLevelMenu != null)
			{
				TopLevelMenu.Enabled = IsFormEditable;
			}
		}
	}
}
