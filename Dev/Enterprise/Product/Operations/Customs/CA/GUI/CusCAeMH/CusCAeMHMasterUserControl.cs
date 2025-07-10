using System;
using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class CusCAeMHMasterUserControl : ZUserControl
	{
		public CusCAeMHMasterUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			masterBill = CurrentDataItem as CusCAeMHMaster;
			if (masterBill != null)
			{
				OverrideFreightDefaultsCheckBox.Visible = !masterBill.BP_ParentID.IsEmpty;
				masterBill.OnOverrideFreightDefaultsChanging += new System.ComponentModel.CancelEventHandler(MasterBill_OnOverrideFreightDefaultsChanging);
			}
		}

		void MasterBill_OnOverrideFreightDefaultsChanging(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var result = Globals.Message.Show(Res.GetString("87D68B97-6552-439A-B87A-D5C7D31F83ED", "Removing the override will reset your eManifest data.\r\nYou will lose changes that you have made to the eManifest data.\r\n\r\nProceed?"), Res.GetString("7B3B6C33-8A10-46D0-8162-B56F192B4DC0", "Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
			e.Cancel = result == DialogResult.No;
		}

		CusCAeMHMaster masterBill;
	}
}
