using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Common;

public static class JPNACCSMailboxCredentialChecker
{
	public static bool HasSetupNACCSMailbox(GlbCompany company)
	{
		if (company != null)
		{
			var wrapper = GlbCompanyWrapper.GetWrapper<JPGlbCompanyWrapper>(company);
			var credential = wrapper?.MailboxCredential;
			if (credential != null)
			{
				return !credential.GP_MailBoxID.IsEmpty && !credential.GP_CurrentPassword.IsEmpty;
			}
		}
		return false;
	}
}
