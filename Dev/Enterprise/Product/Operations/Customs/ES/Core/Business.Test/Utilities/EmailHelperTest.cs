using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class EmailHelperTest : TestCaseWithFactory
	{
		public void TestGetCurrentUserMainEmail()
		{
			var staffCurrentUser = Factory.New<GlbStaff>();
			staffCurrentUser.GS_Code = "XAX";
			staffCurrentUser.GS_LoginName = "Current User";

			using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Test no Email when current user has no email", ZString.Empty, EmailHelper.GetCurrentUserMainEmail());

					staffCurrentUser.GS_EmailAddress = ZString.Empty;
					AssertEquals("Test no Email when email of current user is empty", ZString.Empty, EmailHelper.GetCurrentUserMainEmail());

					staffCurrentUser.GS_EmailAddress = "main@test.com";
					var emailAddress1 = staffCurrentUser.EmailAddresses.FindByEmailAddressType(Core.Constants.EmailFromAddressTypes.Codes.Main);
					emailAddress1.GSE_Type = "AAA";
					AssertEquals("Test no Email when email of current user is not main email", ZString.Empty, EmailHelper.GetCurrentUserMainEmail());

					emailAddress1.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Main;
					AssertEquals("Test Email when is main email", staffCurrentUser.GS_EmailAddress, EmailHelper.GetCurrentUserMainEmail());
				});
			}
		}

		public void TestGetDeclEmailAddrFromRegistry()
		{
			const string clearanceEmail = "mail1.mail@mail.com";
			const string mailboxEmail = "mail2.mail@mail.com";

			CombineAssertions(() =>
			{
				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(clearanceEmail))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
				{
					AssertEquals("Expected filled DeclarationEmail with clearance email recipient when filled", clearanceEmail, EmailHelper.GetDeclEmailAddrFromRegistry());
				}

				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
				{
					AssertEquals("Expected filled DeclarationEmail with mailbox email address when filled and clearance email recipient is empty", mailboxEmail, EmailHelper.GetDeclEmailAddrFromRegistry());
				}

				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
				{
					AssertEquals("Expected empty DeclarationEmail when clearance email recipient and mailbox email address are empty", ZString.Empty, EmailHelper.GetDeclEmailAddrFromRegistry());
				}
			});
		}
	}
}
