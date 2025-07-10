using System;
using System.Linq;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.Testing
{
	sealed class PasswordResetEmailSenderTest : TestCaseWithFactory
	{
		public void TestSendPasswordResetEmail()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(contact.CompanyPKForLogin, Guid.Empty, Guid.Empty, "http://localhost/webtracker");
			Env.Registry.MailboxDisplayName = "Test Dummy Company";

			Env.OutgoingMailManager.EmailsCreated.Clear();

			PasswordInstructionEmailSender.SendPasswordInstructionEmail(contact, PasswordInstructionType.Reset);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var query = new ZQuery(StmAccessTokenSchema.SAT_Scope, contact.Email);
			query.OrderBy = StmAccessTokenSchema.Constants.SAT_SystemCreateTimeUtc + " DESC";
			var stmToken = Factory.LoadTop1<StmAccessToken>(query);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(Env.CurrentCompany.Name + " Password Reset", sentEmail.Subject);
			AssertContains($"http://localhost/webtracker/Admin/ResetMasterPassword.aspx?ResetKey={stmToken.SAT_Token}", sentEmail.Body);
		}

		public void TestSendPasswordResetEmail_CompanySpecific()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			WebDataRegistry.Instance.PasswordResetEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "MLAH", EmailBody = "WAH" });
			WebDataRegistry.Instance.PasswordResetEmailTemplate.SetValue(contact.CompanyPKForLogin, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "Subject Company", EmailBody = "Test Email Body Template Company" });

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var contactWithoutCompanyInfo = new ContactWithoutCompanyInfo(contact.ContactNameWithoutNumberSuffix, contact.Email,
									"http://localhost/myaccount", contact.Salutation, contact.ExtraInstruction, contact.OrgCode, contact.EmailOrgCodes, contact, false);

			contactWithoutCompanyInfo.CompanyPKForEmailTemplate = contact.CompanyPKForLogin;

			PasswordInstructionEmailSender.SendPasswordResetEmail(contactWithoutCompanyInfo);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Subject Company", sentEmail.Subject);
			AssertContains("Test Email Body Template Company", sentEmail.Body);
		}

		public void TestSendPasswordInstructionEmail()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(contact.CompanyPKForLogin, Guid.Empty, Guid.Empty, "http://localhost/webtracker");
			Env.Registry.MailboxDisplayName = "Test Dummy Company";

			Env.OutgoingMailManager.EmailsCreated.Clear();

			PasswordInstructionEmailSender.SendPasswordInstructionEmail(contact, PasswordInstructionType.Set);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var query = new ZQuery(StmAccessTokenSchema.SAT_ParentId, contact.PK);
			query.OrderBy = StmAccessTokenSchema.Constants.SAT_SystemCreateTimeUtc + " DESC";
			var stmToken = Factory.LoadTop1<StmAccessToken>(query);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(Env.CurrentCompany.Name + " Password Set", sentEmail.Subject);
			AssertContains($"http://localhost/webtracker/Admin/SetMasterPassword.aspx?SetKey={stmToken.SAT_Token}", sentEmail.Body);
		}

		public void TestSendPasswordInstructionEmail_UsingDoNotReply()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(contact.CompanyPKForLogin, Guid.Empty, Guid.Empty, "http://localhost/webtracker");
			Env.Registry.MailboxDisplayName = "Test Dummy Company";

			Env.OutgoingMailManager.EmailsCreated.Clear();

			PasswordInstructionEmailSender.SendPasswordInstructionEmail(contact, PasswordInstructionType.Set, useCurrentUserInfo: false);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var query = new ZQuery(StmAccessTokenSchema.SAT_ParentId, contact.PK);
			query.OrderBy = StmAccessTokenSchema.Constants.SAT_SystemCreateTimeUtc + " DESC";
			var stmToken = Factory.LoadTop1<StmAccessToken>(query);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("PleaseDoNotReply@wisetechglobal.com", sentEmail.FromAddress);
			AssertEquals(Env.CurrentCompany.Name + " Password Set", sentEmail.Subject);
			AssertContains($"http://localhost/webtracker/Admin/SetMasterPassword.aspx?SetKey={stmToken.SAT_Token}", sentEmail.Body);
		}

		public void TestSendPasswordInstructionEmail_UsingCurrentUser()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "test@example.com";
			Factory.Save();

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(contact.CompanyPKForLogin, Guid.Empty, Guid.Empty, "http://localhost/webtracker");
			Env.Registry.MailboxDisplayName = "Test Dummy Company";

			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				PasswordInstructionEmailSender.SendPasswordInstructionEmail(contact, PasswordInstructionType.Set,
					useCurrentUserInfo: true);
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

				var query = new ZQuery(StmAccessTokenSchema.SAT_ParentId, contact.PK);
				query.OrderBy = StmAccessTokenSchema.Constants.SAT_SystemCreateTimeUtc + " DESC";
				var stmToken = Factory.LoadTop1<StmAccessToken>(query);

				var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(Env.CurrentUser.EmailAddress, sentEmail.FromAddress);
				AssertEquals(Env.CurrentCompany.Name + " Password Set", sentEmail.Subject);
				AssertContains($"http://localhost/webtracker/Admin/SetMasterPassword.aspx?SetKey={stmToken.SAT_Token}",
					sentEmail.Body);
			}
		}

		public void TestSendPasswordResetEmail_Token()
		{
			var relatedContact1 = Factory.NewWithValidTestData<OrgContact>();
			var relatedContact2 = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			var companyPk = GlbCompany.CurrentCompany.PK.ToGuid();

			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_Email = "testEmail@wisetech.com";
			contact1.SetHashedPassword("1234");
			contact1.OC_PER = relatedContact1.OC_PER;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_Email = "testEmail2@wisetech.com";
			contact2.OC_PER = relatedContact2.OC_PER;
			Factory.Save();

			contact2.Person.SetHashedPassword("1234");
			Factory.Save();

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(contact1.CompanyPKForLogin, Guid.Empty, Guid.Empty, "http://localhost/webtracker");

			Env.OutgoingMailManager.EmailsCreated.Clear();

			contact1.CompanyPKForEmailTemplate = companyPk;
			contact2.CompanyPKForEmailTemplate = companyPk;
			PasswordInstructionEmailSender.SendPasswordResetEmail(contact1);
			PasswordInstructionEmailSender.SendPasswordResetEmail(contact2);

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
			var query1 = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.ResetPassword)
			{
				OrderBy = StmAccessTokenSchema.Constants.SAT_SystemCreateTimeUtc + " DESC"
			};
			var contact1Token = Factory.LoadTop1<StmAccessToken>(query1);
			var query2 = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.ResetMasterPassword)
			{
				OrderBy = StmAccessTokenSchema.Constants.SAT_SystemCreateTimeUtc + " DESC"
			};
			var contact2Token = Factory.LoadTop1<StmAccessToken>(query2);
			var contact1Email = Env.OutgoingMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(contact1.OC_Email));
			var contact2Email = Env.OutgoingMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(contact2.OC_Email));

			AssertNotEquals("Should have generated a password token", null, contact1Token);
			AssertNotEquals("Should have generated a master password token", null, contact2Token);
			AssertContains($"http://localhost/webtracker/Admin/ResetPassword.aspx?ResetKey={contact1Token.SAT_Token}", contact1Email.Body);
			AssertContains($"http://localhost/webtracker/Admin/ResetMasterPassword.aspx?ResetKey={contact2Token.SAT_Token}", contact2Email.Body);
		}

		public void TestSendPasswordInstructionEmail_PasswordResetInfo()
		{
			var companyPk = GlbCompany.CurrentCompany.PK.ToGuid();
			WebDataRegistry.Instance.PasswordResetEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "MLAH", EmailBody = "WAH" });
			WebDataRegistry.Instance.PasswordResetEmailTemplate.SetValue(companyPk, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "Subject Company", EmailBody = "Test Email Body Template Company" });

			var passwordResetInfo = new PasswordResetInfo()
			{
				EmailTemplateCompanyPk = companyPk.ToString()
			};

			var contact = Factory.NewWithValidTestData<OrgContact>();

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(contact.CompanyPKForLogin, Guid.Empty, Guid.Empty, "http://localhost/webtracker");

			Env.OutgoingMailManager.EmailsCreated.Clear();

			PasswordInstructionEmailSender.SendPasswordInstructionEmail(contact, PasswordInstructionType.Reset, passwordResetInfo);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Subject Company", sentEmail.Subject);
			AssertContains("Test Email Body Template Company", sentEmail.Body);
		}

		public void TestSendPasswordResetEmail_CreatePasswordResetInfo()
		{
			var companyPk = GlbCompany.CurrentCompany.PK.ToGuid();
			WebDataRegistry.Instance.PasswordResetEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "MLAH", EmailBody = "WAH" });
			WebDataRegistry.Instance.PasswordResetEmailTemplate.SetValue(companyPk, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "Subject Company", EmailBody = "Test Email Body Template Company" });

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "testEmail@wisetech.com";

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(contact.CompanyPKForLogin, Guid.Empty, Guid.Empty, "http://localhost/webtracker");

			Env.OutgoingMailManager.EmailsCreated.Clear();

			contact.CompanyPKForEmailTemplate = companyPk;
			PasswordInstructionEmailSender.SendPasswordResetEmail(contact);

			var passwordResetInfo = PasswordInstructionEmailSenderForTest.GetPasswordResetInfoForSendPasswordResetEmail_Exposed(contact);
			AssertEquals(contact.Email, passwordResetInfo.ContactEmail);
			AssertEquals(companyPk.ToString(), passwordResetInfo.EmailTemplateCompanyPk);
			AssertEquals("Should be empty when reseting from the web, to alow the user to select a company in the dropdown", null, passwordResetInfo.OrgCode);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Subject Company", sentEmail.Subject);
			AssertContains("Test Email Body Template Company", sentEmail.Body);
		}

		public void TestSendPasswordInstructionEmail_GlowStrategy()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glow.com/Portals");
			Env.Registry.MailboxDisplayName = "Test Dummy Company";

			Env.OutgoingMailManager.EmailsCreated.Clear();

			PasswordInstructionEmailSender.SendPasswordInstructionEmail(contact, PasswordInstructionType.Set, null, PasswordInstructionUrlType.Glow);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var query = new ZQuery(StmAccessTokenSchema.SAT_ParentId, contact.PK);
			query.OrderBy = StmAccessTokenSchema.Constants.SAT_SystemCreateTimeUtc + " DESC";
			var stmToken = Factory.LoadTop1<StmAccessToken>(query);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(Env.CurrentCompany.Name + " Password Set", sentEmail.Subject);
			AssertContains($"https://glow.com/Portals/GHC?token={stmToken.SAT_Token}#/resetPassword", sentEmail.Body);
		}

		public void TestSendPasswordInstructionEmail_GlowStrategy_WithNavigateUrl()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glow.com/Portals");
			Env.Registry.MailboxDisplayName = "Test Dummy Company";

			Env.OutgoingMailManager.EmailsCreated.Clear();

			PasswordInstructionEmailSender.SendPasswordInstructionEmail(contact, PasswordInstructionType.Set, new PasswordResetInfo { NavigateUrl = new Uri("https://frontend.site/TST?token=AAAAA#resetPassword") }, PasswordInstructionUrlType.Glow);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var query = new ZQuery(StmAccessTokenSchema.SAT_ParentId, contact.PK);
			query.OrderBy = StmAccessTokenSchema.Constants.SAT_SystemCreateTimeUtc + " DESC";
			var stmToken = Factory.LoadTop1<StmAccessToken>(query);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(Env.CurrentCompany.Name + " Password Set", sentEmail.Subject);
			AssertContains($"https://frontend.site/TST?token=AAAAA#resetPassword", sentEmail.Body);
			AssertNotContains($"https://glow.com/", sentEmail.Body);
		}

		class PasswordInstructionEmailSenderForTest
		{
			public static PasswordResetInfo GetPasswordResetInfoForSendPasswordResetEmail_Exposed(IPasswordInstructionEmailSource emailSource)
			{
				return PasswordInstructionEmailSender.GetPasswordResetInfoForSendPasswordResetEmail(emailSource);
			}
		}
	}
}
