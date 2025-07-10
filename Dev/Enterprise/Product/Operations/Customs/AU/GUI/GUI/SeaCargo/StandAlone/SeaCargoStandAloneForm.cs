using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoStandAloneForm : Declaration.GUI.CMRMessagingForm
	{
		protected internal SeaCargoStandAloneMenu seaCargoMenu;
		protected internal CMRSeaCargoUserControl OceanBillControl;

		public SeaCargoStandAloneForm()
		{
		}

		public SeaCargoStandAloneForm(CusSCAOceanBill oceanBill)
			: base(oceanBill)
		{
			OceanBillControl = new CMRSeaCargoUserControl(isStandalone: true);
			OceanBillControl.Dock = DockStyle.Fill;
			MainTabPage.Controls.Add(OceanBillControl);
			_ = oceanBill.AllUnderbonds;

			workflowTabPage.Initialize(oceanBill);
		}

		internal CusSCAOceanBill OceanBill => (CusSCAOceanBill)BusinessEntity;

		protected override Business.MultiMessageManager GetManager()
		{
			return new CusSCAOceanBillMessageManager(OceanBill);
		}

		protected override MenuItem GetMessagingMenu()
		{
			SeaScanForOutturnHost scanHost = null;
			if (OceanBill is CusSCAOceanBill)
			{
				scanHost = new SeaScanForOutturnHost(this, new ScanCusSCAOceanBill(OceanBill));
			}
			seaCargoMenu = new SeaCargoStandAloneMenuWithScan((CusSCAOceanBillMessageManager)Manager, scanHost);

			this.seaCargoMenu.Text = "Sea Cargo Messaging";
			return seaCargoMenu;
		}

		public override string FormCaption
		{
			get { return "Sea Cargo"; }
		}

		protected virtual void ChangeVisibility(bool isOceanBillUnpack)
		{
			OceanBillControl.ChangeVisibility(isOceanBillUnpack);
		}
	}
}
