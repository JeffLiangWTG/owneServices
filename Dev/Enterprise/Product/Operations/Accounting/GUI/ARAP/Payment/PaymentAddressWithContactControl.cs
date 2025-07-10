using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.GUI.Organisation.UserControls.Address;

namespace Enterprise.Accounting.GUI.ARAP
{
	public class PaymentAddressWithContactControl : ZAddressWithContactControl
	{
		public PaymentAddressWithContactControl()
		{
			OrganisationFindBox.Visible = false;
			AddressesLink.Visible = false;
			ContactsLink.Visible = false;
			//DetailsTabControl.Top = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(0);
			GroupBox.Dock = DockStyle.Fill;
			DetailsTabControl.Dock = DockStyle.Fill;
			InvoiceContactTabPage.CaptionResourceString = Res.GetData("ZAddressWithContactControl|1ef7010f-583b-43f3-9c68-c27893e282de", "Payment Contact");
			ContactInfoTabVisible = false;
			LabelCaptionRenderProvider.SetLabelCaptionVisible(this, false);
			LabelCaptionRenderProvider.SetLabelCaptionVisible(GroupBox, false);
			DetailsTabControl.TabStop = true;
			DetailsTabControl.TabIndex = 0;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ContactInfoTabVisible = false;
		}
	}
}
