using System.Windows.Forms;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public class CompanyCredentialsPlugin : CompanyCredentialsPlugIn, Integration.Customs.GB.IGBCompanyCredentialsPlugIn
	{
		public CompanyCredentialsPlugin(GlbCompany company) : base(company)
		{
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			var menu = new ZMenuItem(CredentialHelper.AuthoriseText);
			var authCredentialsMenu = new ZMenuItem(CredentialHelper.AuthoriseCredentialsText);
			menu.MenuItems.Add(authCredentialsMenu);
			authCredentialsMenu.Click += AuthoriseCredentials_Click;

			return menu;
		}

		void AuthoriseCredentials_Click(object sender, System.EventArgs e)
		{
			var companyWrapper = GetCompanyWrapper();

			if (SaveDataFirst.Confirm(companyWrapper.Company, this.Form))
			{
				CredentialHelper.AuthoriseCredentials(companyWrapper);
				CredentialHelper.SaveChangesAfterAuthorisation(companyWrapper);
			}
		}

		GBGlbCompanyWrapper GetCompanyWrapper()
		{
			return GetBusinessEntityForPlugIn() as GBGlbCompanyWrapper;
		}
	}
}
