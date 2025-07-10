using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public partial class CredentialsDetailsUserControl : ZUserControl
	{
		readonly MenuItem authoriseMenuItem;

		public CredentialsDetailsUserControl()
		{
			InitializeComponent();

			authoriseMenuItem = new ZMenuItem(CredentialHelper.AuthoriseText);
			authoriseMenuItem.Click += AuthoriseMenu_Click;
			AccUserGrid.ContextMenu.MenuItems.Add(authoriseMenuItem);
			AccUserGrid.ContextMenu.Popup += ContextMenu_Popup;
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			authoriseMenuItem.Enabled = AccUserGrid.SelectedElements.Any(x => (x as GlbExternalPassword_GB).Status == PasswordStatusList.Codes.Invalid);
		}

		void AuthoriseMenu_Click(object sender, EventArgs e)
		{
			var company = AccUserGrid.SelectedElements.Select(x => (x as GlbExternalPassword_GB).Company).FirstOrDefault();
			var form = this.FindForm() as ZForm;

			if (company != null && form != null)
			{
				if (SaveDataFirst.Confirm(company, form))
				{
					var passwordSelected = AccUserGrid.GetCurrent() as GlbExternalPassword_GB;
					if (passwordSelected != null)
					{
						CredentialHelper.AuthoriseCredential(passwordSelected);
						CredentialHelper.SaveChangesAfterAuthorisation(GBGlbCompanyWrapper.GetWrapper<GBGlbCompanyWrapper>(company));
					}
				}
			}
		}
	}
}
