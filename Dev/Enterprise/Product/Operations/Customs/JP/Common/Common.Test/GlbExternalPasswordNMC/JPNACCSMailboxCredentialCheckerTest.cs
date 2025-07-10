using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(JPNACCSMailboxCredentialChecker))]
sealed class JPNACCSMailboxCredentialCheckerTest : TestCase
{
	public void TestHasSetupNACCSMailbox()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
		{
			var company = GlbCompany.CurrentCompany;
			var wrapper = GlbCompanyWrapper.GetWrapper<JPGlbCompanyWrapper>(company);
			var mailboxCredential = wrapper.MailboxCredential;
			Assert(!JPNACCSMailboxCredentialChecker.HasSetupNACCSMailbox(company));

			mailboxCredential.GP_MailBoxID = "TEST";
			mailboxCredential.CurrentDecryptedPassword = "TEST";
			Assert(JPNACCSMailboxCredentialChecker.HasSetupNACCSMailbox(company));
		}
	}
}
