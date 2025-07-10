using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Main.Startup.Tools
{
	public partial class LicenseForm : ZChildForm
	{
		public LicenseForm(string license)
		{
			InitializeComponent();

			txtLicense.Text = license;
			txtLicense.Select(0, 0);
		}

		void HasReadLicenseChanged(object sender, System.EventArgs e)
		{
			btnAccept.Enabled = chkHasReadLicense.Checked;
		}
	}
}
