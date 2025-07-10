using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public static class CredentialHelper
	{
		public static void AuthoriseCredentials(GBGlbCompanyWrapper companyWrapper)
		{
			foreach (var password in companyWrapper.GetCredentialsToAuthorise())
			{
				AuthoriseCredential(password);
			}
		}

		public static void AuthoriseCredential(GlbExternalPassword_GB password)
		{
			if (password != null && password.IsInvalid)
			{
				var authorisationManager = new GlbExternalPasswordAuthorisationManager();
				ZFormModaliser.ShowDialogAndDispose(new CredentialAuthorisationForm(password, authorisationManager));

				if (authorisationManager.IsAuthorised)
				{
					password.RefreshBinding();
				}
			}
		}

		public static void SaveChangesAfterAuthorisation(GBGlbCompanyWrapper companyWrapper)
		{
			if (companyWrapper != null)
			{
				companyWrapper.Factory.Save();
			}
		}

		public static readonly MultilingualString AuthoriseText = Customs.GB.GUI.ResString.GetMultilingualString("2EE685A2-BEC0-4EF8-BEAC-FF3E8A8B9234", "Authorize");
		public static readonly MultilingualString AuthoriseCredentialsText = Customs.GB.GUI.ResString.GetMultilingualString("2EE685A2-BEC0-4EF8-BEAC-FF3E8A8B9198", "Authorize Credentials");
	}
}
