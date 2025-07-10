using System.Windows.Forms;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public partial class CredentialAuthorisationForm : ZChildForm
	{
		readonly IGlbExternalPasswordAuthorisationManager authorisationManager;
		readonly GlbExternalPassword_GB password;

		public CredentialAuthorisationForm(GlbExternalPassword_GB password, IGlbExternalPasswordAuthorisationManager authorisationManager)
		{
			InitializeComponent();
			this.password = password;
			this.authorisationManager = authorisationManager;
			label1.Text = Res.GetString("EE469611-02D2-49AA-B3A0-31C5EB292264", "You are about to undertake the process to grant {0} permission to act on behalf of your company\r\nfor the purpose of making customs declarations and others kinds of declaration.  This is the new style of security implemented by HMRC.\r\nYou're about to be taken, using your default web browser, to the www.gov.uk web page, where you will be asked \r\nto sign in using your Government Gateway credentials.  \r\nOnce you have signed in, you will be asked to click a green button to grant authority to {0}. \r\nFrom there, some invisible processing will occur and shortly the www.gov.uk website will redirect you to the WTG website, \r\nwhich will tell you that the process has completed.\r\nAfter that, you will be told you may close your web browser.  Note that you must sign in to www.gov.uk using\r\nGovernment Gateway credentials that are tied to your company.  WTG will never see these credentials.\r\nYou must have registered your Government Gateway account to access the necessary services.\r\nAfter you have completed the process, WTG and HMRC will communicate further, without your involvement, to finalize the process. \r\nIf everything is successful, the record you're currently editing will be updated to reflect success.\r\nDon't forget that the authority you grant only lasts for a limited time, usually 18 months, after which you must repeat this process.  \r\nIf authority lapses, {0} will not be allowed to upload declarations for your company. It is your responsibility to repeat\r\nthis process (although of course {0} will remind you as the expiration looms).\r\nPlease check all items below before Authorizing.", Core.Constants.ProductName);
			label2.Text = Res.GetString("CE19F3B0-0116-47B2-9FAF-E1FA2CAA6C13", "Requesting authorization for Badge: {0} - EORI: {1}", password.Badge, password.EORI);
			ButtonAuthorise.Text = CredentialHelper.AuthoriseText;
		}

		void ButtonAuthorise_Click(object sender, System.EventArgs e)
		{
			var url = password.GetUrl();
			if (!string.IsNullOrEmpty(url))
			{
				WebUrlLauncher.Launch(url);
				password.StatusMessage = PendingAuthorisation;
				authorisationManager.IsAuthorised = true;
				Close();
			}
			else if (Globals.Message.Show(Res.GetString("59FA05DE-8F17-4685-979C-32D96F0FA4C0", "Cannot authorize due to missing data!"),
				Res.GetString("555D60CB-06FA-426F-AB8D-9846F5057885", "Authorization"), MessageBoxButtons.OK, DialogResult.OK) == DialogResult.OK)
			{
				Close();
			}
		}

		const string PendingAuthorisation = "Pending Authorisation";

		void CheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			ButtonAuthorise.Enabled = (checkBox1.Checked && checkBox2.Checked && checkBox3.Checked && checkBox4.Checked);
		}
	}
}
