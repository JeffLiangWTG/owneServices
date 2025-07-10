using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.AFR.GUI
{
	public partial class JPAFRConsolManifestUserControl : ZUserControl
	{
		public JPAFRConsolManifestUserControl()
		{
			InitializeComponent();
		}

		public JPAFRConsolManifestUserControl(JPAFRHeader header)
			: this()
		{
			Argument.NotNull(header, "header");
			this.header = header;
			header.OnOverrideFreightDefaultsChanging += new System.ComponentModel.CancelEventHandler(header_OnOverrideFreightDefaultsChanging);
		}

		public void SelectAndShowBill(ZGuid bilPK)
		{
			if (header != null && BillsTabPage.TabVisible)
			{
				var bill = (JPAFRBills)header.Bills.FindByPK(bilPK);
				if (bill != null)
				{
					MainTabControl.SelectedTab = BillsTabPage;
					var manager = (CurrencyManager)BillsDetailsUserControl.BindingContext[header, "Bills"];
					var index = manager.List.IndexOf(bill);
					if (index >= 0)
					{
						manager.Position = index;
					}
				}
			}
		}

		void header_OnOverrideFreightDefaultsChanging(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var result = Globals.Message.Show(Res.GetString("JPAFRConsolManifestUserControl|8C6EC7F4-ABCE-4A78-919E-AEF05DC737F7", "Removing the override will reset your AFR data.\r\nYou will lose changes that you have made to the AFR data.\r\n\r\nProceed?"), Res.GetString("JPAFRConsolManifestUserControl|045EC8D8-E8ED-493B-B7AC-98A6323E3F45", "Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
			e.Cancel = result == DialogResult.No;
		}

		readonly JPAFRHeader header;
	}
}
