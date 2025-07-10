using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class VoyageManifestForm : CMRMessagingForm
	{
		public VoyageManifestForm()
		{
			InitializeComponent();
		}

		public VoyageManifestForm(CusSeaManTranHead tranHead)
			: base(tranHead)
		{
			InitializeComponent();
			SetupImportMenu(tranHead);
			tranHead.OceanBillsView.DeletingOceanBillWhenDisallowed += new EventHandler(OnDeletingHouseBillWhenDisallowed);
		}

		void OnDeletingHouseBillWhenDisallowed(object sender, EventArgs e)
		{
			CusSeaManOBLHeader oceanBill = sender as CusSeaManOBLHeader;
			if (oceanBill != null)
			{
				Globals.Message.ShowError("You should withdraw the OceanBill prior to delete.");
			}
		}

		public override string FormCaption
		{
			get { return "Customs Import Manifest"; }
		}

		protected override Customs.Business.MultiMessageManager GetManager()
		{
			return new CusSeaManTranHeadMessageManager(TranHead);
		}

		protected override MenuItem GetMessagingMenu()
		{
			return new VoyageManifestMessagingMenu(Manager);
		}

		void SetupImportMenu(CusSeaManTranHead tranHead)
		{
			VoyageManifestImportMenu menu = VoyageManifestImportMenu.New();
			menu.TranHead = tranHead;
			MainMenu.MenuItems.Add(MainMenu.MenuItems.Count - 1, menu);
		}

		CusSeaManTranHead TranHead
		{
			get
			{
				return (CusSeaManTranHead)BusinessEntity;
			}
		}

		public bool HasStartedDisposing { get; private set; }
	}
}
