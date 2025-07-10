using System.Windows.Forms;
using CargoWise.BrandManager;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.Core.Forms
{
#if DEBUG
	[SuppressFormsLocalizedTest]
#endif
	public partial class LicenceErrorForm : ZChildForm
	{
		public LicenceErrorForm(string moduleName, string licenceErrorText)
		{
			InitializeComponent();

			EDISupportLabel.Text = Res.GetString("LicenceErrorForm.ContactInformation", "{0} Contact Information:", Constants.ProductSupportName);

			pictureBox1.Image = BrandingFactory.Instance.ProductLogo;
			LicenceErrorRichBox.BackColor = this.BackColor;

			ModuleNameLabel.Text = moduleName;
			LicenceErrorRichBox.Text = licenceErrorText;
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Events

		internal void OKButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		void EmailLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WebUrlLauncher.Launch("mailto:" + EmailLinkLabel.Text);
		}

		void WebsiteLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WebUrlLauncher.Launch(WebsiteLinkLabel.Text);
		}

		#endregion
	}
}
