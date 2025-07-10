using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class MinSpendSettingControl : ZUserControl
	{
		public MinSpendSettingControl()
		{
			InitializeComponent();
		}

		void InfoLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			var msg =
@"Minimum spend for the feature after discounts.
If total spend is below the minimum, the difference is added to the invoice.";
			Globals.Message.ShowInformation(msg);
		}
	}
}
