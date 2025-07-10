using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1178:Do Not Invoke Old Res.GetString Methods", Justification = "There are no ResString class method in Web.Business project, so suppress here")]
	public class WebUserAdminManagerTest : TestCaseWithFactory
	{
		public void TestChangePassword_IgnoreOverride()
		{
			const string emailSubject = "KRAV";
			const string emailBody = "BLAH";
			WebDataRegistry.Instance.PasswordResetSuccessfullyEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = emailSubject, EmailBody = emailBody });
			WebDataRegistry.Instance.PasswordResetEmailFooter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ResString.GetMultilingualString("E36C91AF-2FC4-4D91-A392-2F42C79739FF", "Test Footer"));

			try
			{
				DataRegistry.Instance.EmailDestinationOverride = "override@email.com";
				GlbStaff.CurrentUser.GS_EmailAddress = "example@cargowise.com";

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "WiseTech Global";
				org.OH_Code = "WiseTech";
				var newContact = org.Contacts.AddNew();
				newContact.OC_ContactName = "Glen Maclarty";
				newContact.OC_Email = "glen.maclarty@cargowise.com";
				newContact.SetHashedPassword("Th1s1sMyPassw0rd");

				Factory.Save();

				AddDummyContact(newContact);

				var manager = GetNewWebUserAdminManagerForTest(newContact);
				var result = manager.ChangePassword("ThisIsNotMyPassword", "This1sMyN3wPassw0rd!", "This1sMyN3wPassw0rd!");
				var freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);

				AssertEquals("PreCondition: Email count should be 0", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				result = manager.ChangePassword("Th1s1sMyPassw0rd", "This1sMyN3wPassw0rd!", "This1sMyN3wPassw0rd!", true);
				freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);

				AssertEquals("Message should indicate success", "Your password has been changed.", result);
				AssertNotNull(freshContact.OC_PasswordSalt);
				AssertNotNull(freshContact.OC_PasswordHash);
				AssertEquals(DataRegistry.Instance.PasswordHashingIterationsCount, freshContact.OC_PasswordHashIterations);
				Assert("Password should be updated", freshContact.VerifyPassword("This1sMyN3wPassw0rd!"));

				AssertEquals("should send email to contact to notify password has been reset", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals(true, freshContact.GetLogs().DatabaseHasLogs(new ZQuery(StmALogSchema.SL_SE_NKEvent, new[] { AutoEvents.WebAccessPasswordChangedCode })));

				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(emailSubject, email.Subject);

				CombineAssertions(() =>
				{
					Assert(email.Body.Contains(emailBody));
					Assert(!email.Body.Contains(emailSubject));
					AssertEquals(1, email.Recipients.Count);
					Assert(email.Recipients[0].IsForSystemCommunication);
				});

				using (Env.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				{
					Env.OutgoingMailManager.EmailsCreated.Clear();
					AssertEquals(true, Env.CurrentUser.IsWebUser);
					manager = GetNewWebUserAdminManagerForTest(newContact);
					manager.ChangePassword("This1sMyN3wPassw0rd!", "This1sMyN4wPassw0rd!", "This1sMyN4wPassw0rd!", true);
					email = Env.OutgoingMailManager.EmailsCreated[0];
					Assert(email.Recipients[0].IsForSystemCommunication);
				}
			}
			finally
			{
				DataRegistry.Instance.EmailDestinationOverride = "";
			}
		}

		void AddDummyContact(OrgContact contact)
		{
			// we can reset contact password only when if it is not the only contact in the person relationship
			contact.Person.ContactCollection.Add(Factory.NewWithValidTestData<OrgContact>());
		}

		public void TestPasswordChange()
		{
			const string emailSubject = "KRAV";
			const string emailBody = "BLAH";
			WebDataRegistry.Instance.PasswordResetSuccessfullyEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = emailSubject, EmailBody = emailBody });
			WebDataRegistry.Instance.PasswordResetEmailFooter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ResString.GetMultilingualString("E36C91AF-2FC4-4D91-A392-2F42C79739FF", "Test Footer"));

			GlbStaff.CurrentUser.GS_EmailAddress = "example@cargowise.com";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "WiseTech Global";
			org.OH_Code = "WiseTech";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "Glen Maclarty";
			newContact.OC_Email = "glen.maclarty@cargowise.com";
			newContact.SetHashedPassword("Th1s1sMyPassw0rd");

			Factory.Save();

			AddDummyContact(newContact);

			var manager = GetNewWebUserAdminManagerForTest(newContact);
			var result = manager.ChangePassword("ThisIsNotMyPassword", "This1sMyN3wPassw0rd!", "This1sMyN3wPassw0rd!");
			var freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			Assert("Password should not have changed", freshContact.VerifyPassword("Th1s1sMyPassw0rd"));
			AssertEquals("Message should indicate mismatch", "The password supplied does not match your current password.", result);

			result = manager.ChangePassword("Th1s1sMyPassw0rd", "", "");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			Assert("Password should not have changed", freshContact.VerifyPassword("Th1s1sMyPassw0rd"));
			AssertEquals("Message should indicate mismatch", "The new password cannot be empty.", result);

			result = manager.ChangePassword("Th1s1sMyPassw0rd", "This1sMyN3wPassw0rd!", "");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			Assert("Password should not have changed", freshContact.VerifyPassword("Th1s1sMyPassw0rd"));
			AssertEquals("Message should indicate mismatch", "The password and confirmation do not match.", result);

			AssertEquals("PreCondition: Email count should be 0", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			result = manager.ChangePassword("Th1s1sMyPassw0rd", "This1sMyN3wPassw0rd!", "This1sMyN3wPassw0rd!");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);

			AssertNotNull(freshContact.OC_PasswordSalt);
			AssertNotNull(freshContact.OC_PasswordHash);
			AssertEquals(DataRegistry.Instance.PasswordHashingIterationsCount, freshContact.OC_PasswordHashIterations);
			Assert("Password should be updated", freshContact.VerifyPassword("This1sMyN3wPassw0rd!"));

			AssertEquals("Message should indicate success", "Your password has been changed.", result);
			AssertEquals("should send email to contact to notify password has been reset", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(true, freshContact.GetLogs().DatabaseHasLogs(new ZQuery(StmALogSchema.SL_SE_NKEvent, new[] { AutoEvents.WebAccessPasswordChangedCode })));

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(emailSubject, email.Subject);

			CombineAssertions(() =>
			{
				Assert(email.Body.Contains(emailBody));
				Assert(!email.Body.Contains(emailSubject));
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("glen.maclarty@cargowise.com", email.Recipients[0].Email);
				AssertEquals("Should be from the contact's email address", ExpectedFromEmailAddress, email.FromAddress);
				AssertEquals("Should be from the contact", ExpectedFromContactName, email.FromDisplayName);
				AssertEquals(false, email.Recipients[0].IsForSystemCommunication);
			});

			using (Env.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals(true, Env.CurrentUser.IsWebUser);
				manager = GetNewWebUserAdminManagerForTest(newContact);
				manager.ChangePassword("This1sMyN3wPassw0rd!", "This1sMyN4wPassw0rd!", "This1sMyN4wPassw0rd!");
				email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(ExpectedFromEmailAddress, email.FromAddress);
				AssertEquals(ExpectedFromContactName, email.FromDisplayName);
				AssertEquals(ExpectedReplyTo, email.ReplyTo);
				AssertEquals(false, email.Recipients[0].IsForSystemCommunication);
			}
		}

		public void TestPasswordChange_Set()
		{
			const string emailSubject = "KRAV";
			const string emailBody = "BLAH";
			WebDataRegistry.Instance.PasswordSetSuccessfullyEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = emailSubject, EmailBody = emailBody });
			WebDataRegistry.Instance.PasswordSetEmailFooter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ResString.GetMultilingualString("E36C91AF-2FC4-4D91-A392-2F42C79739FF", "Test Footer"));

			GlbStaff.CurrentUser.GS_EmailAddress = "example@cargowise.com";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "WiseTech Global";
			org.OH_Code = "WiseTech";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "Glen Maclarty";
			newContact.OC_Email = "glen.maclarty@cargowise.com";
			newContact.SetHashedPassword("Th1s1sMyPassw0rd");

			Factory.Save();

			AddDummyContact(newContact);

			var manager = GetNewWebUserAdminManagerForTest(newContact);
			var result = manager.ChangePassword("This1sMyN3wPassw0rd!", "This1sMyN3wPassw0rd!", PasswordInstructionType.Set);
			var freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			Assert("Password should be updated", freshContact.VerifyPassword("This1sMyN3wPassw0rd!"));
			AssertEquals("Message should indicate success", "Your password has been changed.", result.Message);
			AssertEquals("should send email to contact to notify password has been set", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(true, freshContact.GetLogs().DatabaseHasLogs(new ZQuery(StmALogSchema.SL_SE_NKEvent, new[] { AutoEvents.WebAccessPasswordChangedCode })));

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(emailSubject, email.Subject);

			CombineAssertions(() =>
			{
				Assert(email.Body.Contains(emailBody));
				Assert(!email.Body.Contains(emailSubject));
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("glen.maclarty@cargowise.com", email.Recipients[0].Email);
				AssertEquals("Should be from the contact's email address", ExpectedFromEmailAddress, email.FromAddress);
				AssertEquals("Should be from the contact", ExpectedFromContactName, email.FromDisplayName);
			});

			using (Env.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals(true, Env.CurrentUser.IsWebUser);
				manager = GetNewWebUserAdminManagerForTest(newContact);
				result = manager.ChangePassword("This1sMyN4wPassw0rd!", "This1sMyN4wPassw0rd!", PasswordInstructionType.Set);
				AssertEquals("Precondition: Message should indicate success", "Your password has been changed.", result.Message);
				email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(ExpectedFromEmailAddress, email.FromAddress);
				AssertEquals(ExpectedFromContactName, email.FromDisplayName);
				AssertEquals(ExpectedReplyTo, email.ReplyTo);
			}
		}

		public void TestPasswordChangeEmailSendFail()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "";
			Env.Registry.EnterpriseMailboxEmailAddress = "";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "WiseTech Global";
			org.OH_Code = "WiseTech";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "Glen Maclarty";
			newContact.OC_Email = "";
			newContact.SetHashedPassword("Th1s1sMyPassw0rd");

			Factory.Save();

			AddDummyContact(newContact);
			var manager = GetWebUserAdminManagerForTest(newContact);
			var result = manager.ChangePassword("This1sMyN3wPassw0rd!", "This1sMyN3wPassw0rd!", PasswordInstructionType.Set);
			AssertEquals("Your password has been updated but we encountered a problem sending the confirmation email. Please contact your system administrator", result.Message);
		}

		public void TestPasswordSetEmailSendFail()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "";
			Env.Registry.EnterpriseMailboxEmailAddress = "";
			Env.Registry.SMTPServer = "";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "WiseTech Global";
			org.OH_Code = "WiseTech";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "Glen Maclarty";
			newContact.OC_Email = "";
			newContact.OC_WebAccessEnabled = true;
			newContact.OC_IsActive = true;
			newContact.SetHashedPassword("Th1s1sMyPassw0rd");

			Factory.Save();

			AddDummyContact(newContact);
			var manager = GetWebUserAdminManagerForTest(newContact);
			var result = manager.SetMasterPassword("This1sMyN3wPassw0rd!", "This1sMyN3wPassw0rd!", PasswordInstructionType.Set);
			AssertEquals("Your password has been updated but we encountered a problem sending the confirmation email. Please contact your system administrator", result);
		}

		public void TestPasswordChange_SetCompanySpecific()
		{
			WebDataRegistry.Instance.PasswordSetSuccessfullyEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "MLAH", EmailBody = "WAH" });

			GlbStaff.CurrentUser.GS_EmailAddress = "example@cargowise.com";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "WiseTech Global";
			org.OH_Code = "WiseTech";
			org.CompanyData.OB_GB_ControllingBranch = branch.PK;
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "Glen Maclarty";
			newContact.OC_Email = "glen.maclarty@cargowise.com";
			newContact.SetHashedPassword("Th1s1sMyPassw0rd");

			Factory.Save();

			AddDummyContact(newContact);

			const string companySpecificSubject = "KRAV";
			const string companySpecificBody = "BLAH";
			WebDataRegistry.Instance.PasswordSetSuccessfullyEmailTemplate.SetValue(newContact.ParentOrg.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = companySpecificSubject, EmailBody = companySpecificBody });

			var manager = GetNewWebUserAdminManagerForTest(newContact);
			var result = manager.ChangePassword("This1sMyN3wPassw0rd!", "This1sMyN3wPassw0rd!", PasswordInstructionType.Set);
			var freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			AssertEquals("Precondition: Change should be successful", "Your password has been changed.", result.Message);
			Assert("Precondition: Password should be updated", freshContact.VerifyPassword("This1sMyN3wPassw0rd!"));
			AssertEquals("Precondition: Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email subject should match the company specific registry", companySpecificSubject, email.Subject);
			AssertContains("Email body should match the company specific registry", companySpecificBody, email.Body);
			AssertNotContains("Email body should not have a subject copy in it", companySpecificSubject, email.Body);
		}

		public void TestPasswordChange_ResetCompanySpecific()
		{
			WebDataRegistry.Instance.PasswordResetSuccessfullyEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "MLAH", EmailBody = "WAH" });

			GlbStaff.CurrentUser.GS_EmailAddress = "example@cargowise.com";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "WiseTech Global";
			org.OH_Code = "WiseTech";
			org.CompanyData.OB_GB_ControllingBranch = branch.PK;
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "Glen Maclarty";
			newContact.OC_Email = "glen.maclarty@cargowise.com";
			newContact.SetHashedPassword("Th1s1sMyPassw0rd");

			Factory.Save();

			AddDummyContact(newContact);

			const string companySpecificSubject = "KRAV";
			const string companySpecificBody = "BLAH";
			WebDataRegistry.Instance.PasswordResetSuccessfullyEmailTemplate.SetValue(newContact.ParentOrg.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = companySpecificSubject, EmailBody = companySpecificBody });

			var manager = GetNewWebUserAdminManagerForTest(newContact);
			var result = manager.ChangePassword("This1sMyN3wPassw0rd!", "This1sMyN3wPassw0rd!");
			var freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			AssertEquals("Precondition: Change should be successful", "Your password has been changed.", result.Message);
			Assert("Precondition: Password should be updated", freshContact.VerifyPassword("This1sMyN3wPassw0rd!"));
			AssertEquals("Precondition: Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email subject should match the company specific registry", companySpecificSubject, email.Subject);
			AssertContains("Email body should match the company specific registry", companySpecificBody, email.Body);
			AssertNotContains("Email body should not have a subject copy in it", companySpecificSubject, email.Body);
		}

		public void TestPasswordChange_ResolveSubjectMacro()
		{
			var emailTemplate = new NotificationEmailTemplate { EmailSubject = "(*MailboxDisplayName*) Subject", EmailBody = "(*MailboxDisplayName*) body" };
			var emailTemplateWithCompany = new NotificationEmailTemplate { EmailSubject = "(*MailboxDisplayName*) company subject", EmailBody = "(*MailboxDisplayName*) company body" };

			WebDataRegistry.Instance.PasswordResetSuccessfullyEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailTemplate);

			GlbStaff.CurrentUser.GS_EmailAddress = "example@cargowise.com";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "WiseTech Global";
			org.OH_Code = "WiseTech";
			org.CompanyData.OB_GB_ControllingBranch = branch.PK;
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "Glen Maclarty";
			newContact.OC_Email = "glen.maclarty@cargowise.com";
			newContact.SetHashedPassword("Th1s1sMyPassw0rd");

			Factory.Save();

			AddDummyContact(newContact);

			WebDataRegistry.Instance.PasswordResetSuccessfullyEmailTemplate.SetValue(newContact.ParentOrg.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, emailTemplateWithCompany);

			var manager = GetNewWebUserAdminManagerForTest(newContact);
			var result = manager.ChangePassword("This1sMyN3wPassw0rd!", "This1sMyN3wPassw0rd!");
			var freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			AssertEquals("Precondition: Change should be successful", "Your password has been changed.", result.Message);
			Assert("Precondition: Password should be updated", freshContact.VerifyPassword("This1sMyN3wPassw0rd!"));
			AssertEquals("Precondition: Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotContains("Email subject should resolve the macro", "MailboxDisplayName", email.Subject);
			AssertNotContains("Email body should resolve the macro", "MailboxDisplayName", email.Body);
		}

		public void TestCompanyPKForEmailTemplate()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "WiseTech Global";
			org.OH_Code = "WiseTech";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "Glen Maclarty";
			newContact.OC_Email = "glen.maclarty@cargowise.com";
			newContact.SetHashedPassword("Th1s1sMyPassw0rd");

			var templateGuid = Guid.NewGuid();

			var manager1 = new WebUserAdminManagerForTest(newContact);
			AssertNotEquals(templateGuid, manager1.MailSender_Exposed.CompanyPk_Exposed);

			newContact.CompanyPKForEmailTemplate = templateGuid;
			var manager2 = new WebUserAdminManagerForTest(newContact);
			AssertEquals(templateGuid, manager2.MailSender_Exposed.CompanyPk_Exposed);
		}

		public void TestPasswordChangeShouldEnforcePasswordPolicy()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "WiseTech Global";
			org.OH_Code = "WiseTech";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "Glen Maclarty";
			newContact.OC_Email = "glen.maclarty@cargowise.com";
			newContact.SetHashedPassword("Th1s1sMyPassw0rd");

			Factory.Save();

			var manager = GetNewWebUserAdminManagerForTest(newContact);
			var result = manager.ChangePassword("Th1s1sMyPassw0rd", "1aA!", "1aA!");
			var freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			Assert("Password should not have changed", freshContact.VerifyPassword("Th1s1sMyPassw0rd"));
			AssertEquals("Password must be at least 12 characters long.", result);

			result = manager.ChangePassword("Th1s1sMyPassw0rd", "1a!abacusabacus", "1a!abacusabacus");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			Assert("Password should not have changed", freshContact.VerifyPassword("Th1s1sMyPassw0rd"));
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", result);

			result = manager.ChangePassword("Th1s1sMyPassw0rd", "1A!ABACUSABACUS", "1A!ABACUSABACUS");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			Assert("Password should not have changed", freshContact.VerifyPassword("Th1s1sMyPassw0rd"));
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", result);

			result = manager.ChangePassword("Th1s1sMyPassw0rd", "Aa!abacusabacus", "Aa!abacusabacus");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			Assert("Password should not have changed", freshContact.VerifyPassword("Th1s1sMyPassw0rd"));
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", result);

			result = manager.ChangePassword("Th1s1sMyPassw0rd", "Aa1abacusabacus", "Aa1abacusabacus");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			Assert("Password should not have changed", freshContact.VerifyPassword("Th1s1sMyPassw0rd"));
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", result);

			result = manager.ChangePassword("Th1s1sMyPassw0rd", "Aa1!abacusabacu", "Aa1!abacusabacu");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			AssertEquals("Your password has been changed.", result);
			AssertNotNull(freshContact.OC_PasswordSalt);
			AssertNotNull(freshContact.OC_PasswordHash);
			AssertEquals(DataRegistry.Instance.PasswordHashingIterationsCount, freshContact.OC_PasswordHashIterations);
			Assert("Password should be updated", freshContact.VerifyPassword("Aa1!abacusabacu"));
		}

		public void TestPasswordChangeShouldNotAllowPersonName()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "WiseTech Global";
			org.OH_Code = "WiseTech";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "Glen A Maclarty (1)";
			newContact.OC_Email = "glen.maclarty@cargowise.com";
			newContact.SetHashedPassword("Th1s1sMyPassw0rd");

			Factory.Save();

			var manager = GetNewWebUserAdminManagerForTest(newContact);
			var result = manager.ChangePassword("Th1s1sMyPassw0rd", "1aA!Iamglenglenasaurus", "1aA!Iamglenglenasaurus");
			var freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			Assert("Password should not have changed", freshContact.VerifyPassword("Th1s1sMyPassw0rd"));
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, result);

			result = manager.ChangePassword("Th1s1sMyPassw0rd", "1a!cAllMemAcLarty", "1a!cAllMemAcLarty");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			Assert("Password should not have changed", freshContact.VerifyPassword("Th1s1sMyPassw0rd"));
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, result);

			result = manager.ChangePassword("Th1s1sMyPassw0rd", "1a!cAllMemAcvarty(1)", "1a!cAllMemAcvarty(1)");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			Assert("Password should have changed - initial A and (1) shouldn't be blacklisted", freshContact.VerifyPassword("1a!cAllMemAcvarty(1)"));
			AssertEquals("Your password has been changed.", result);
		}

		public void TestChangePasswordShouldShowPasswordRequirementsMessageBeforeMismatch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "WiseTech Global";
			org.OH_Code = "WiseTech";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "Glen Maclarty";
			newContact.OC_Email = "glen.maclarty@cargowise.com";
			newContact.SetHashedPassword("Th1s1sMyPassw0rd");

			Factory.Save();

			var manager = GetNewWebUserAdminManagerForTest(newContact);
			var result = manager.ChangePassword("Th1s1sMyPassw0rd", "1aA!", "iaohsfiobn@sA1sfo");
			var freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			Assert("Password should not have changed", freshContact.VerifyPassword("Th1s1sMyPassw0rd"));
			AssertEquals("Should show password requirements message before mismatch since password will need to be changed regardless", "Password must be at least 12 characters long.", result);
		}

		public void TestPasswordChange_ValidateNameAndEmail()
		{
			const string emailSubject = "KRAV";
			const string emailBody = "BLAH";
			WebDataRegistry.Instance.PasswordResetSuccessfullyEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = emailSubject, EmailBody = emailBody });
			WebDataRegistry.Instance.PasswordResetEmailFooter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ResString.GetMultilingualString("E36C91AF-2FC4-4D91-A392-2F42C79739FF", "Test Footer"));

			GlbStaff.CurrentUser.GS_EmailAddress = "example@cargowise.com";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "WiseTech Global";
			org.OH_Code = "WiseTech";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "Glen Maclarty";
			newContact.OC_Email = "wise.tech@cargowise.com";
			newContact.SetHashedPassword("Th1s1sMyPassw0rd");

			Factory.Save();

			var manager = GetNewWebUserAdminManagerForTest(newContact);
			var result = manager.ChangePassword("Th1s1sMyPassw0rd", "This1sMaclartyPassw0rd!", "This1sMaclartyPassw0rd!");
			var freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			Assert("Password should not have changed", freshContact.VerifyPassword("Th1s1sMyPassw0rd"));
			AssertEquals("Name should be validated when changing password", ContactPasswordValidator.ExcludedStringErrorMessage, result);

			result = manager.ChangePassword("Th1s1sMyPassw0rd", "This1sWisePassw0rd!", "This1sWisePassw0rd!");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(newContact.PK);
			Assert("Password should not have changed", freshContact.VerifyPassword("Th1s1sMyPassw0rd"));
			AssertEquals("Email should be validated when changing password", ContactPasswordValidator.ExcludedStringErrorMessage, result);
		}

		#region Master Password

		public void TestSetMasterPassword()
		{
			var webEnabledContact = Factory.NewWithValidTestData<OrgContact>();
			webEnabledContact.OC_Email = "camo@mile.com";
			webEnabledContact.OC_WebAccessEnabled = true;
			webEnabledContact.SetHashedPassword("1234");
			Factory.Save();
			var person = webEnabledContact.Person;
			person.PER_EmailAddress = "earl@grey.com";
			var inactiveContact = Factory.NewWithValidTestData<OrgContact>();
			inactiveContact.OC_Email = "mile@mile.com";
			inactiveContact.OC_WebAccessEnabled = true;
			inactiveContact.OC_IsActive = false;
			inactiveContact.OC_PER = person.PK;
			inactiveContact.SetHashedPassword("1234");
			var nonWebContact = Factory.NewWithValidTestData<OrgContact>();
			nonWebContact.OC_Email = "bleach@mile.com";
			nonWebContact.OC_WebAccessEnabled = false;
			nonWebContact.OC_PER = person.PK;
			nonWebContact.SetHashedPassword("1234");
			Factory.Save();
			Assert("Precondition", webEnabledContact.OC_IsActive);
			Assert("Precondition", webEnabledContact.OC_WebAccessEnabled);
			Assert("Precondition", !inactiveContact.OC_IsActive);
			Assert("Precondition", inactiveContact.OC_WebAccessEnabled);
			Assert("Precondition", nonWebContact.OC_IsActive);
			Assert("Precondition", !nonWebContact.OC_WebAccessEnabled);

			var masterPasswordHelper = new WebUserAdminManager(webEnabledContact);

			AssertEquals("The password and confirmation do not match.", masterPasswordHelper.SetMasterPassword("password!P4sword", "different", PasswordInstructionType.Set));
			webEnabledContact.Factory.Save();
			AssertEquals(false, webEnabledContact.Person.VerifyPassword("password!P4sword"));

			AssertEquals(masterPasswordHelper.PasswordChangeSuccess, masterPasswordHelper.SetMasterPassword("password!P4sword", "password!P4sword", PasswordInstructionType.Set));
			webEnabledContact.Factory.Save();
			AssertEquals(true, webEnabledContact.Person.VerifyPassword("password!P4sword"));
			Assert("Contact password should be removed", webEnabledContact.OC_PasswordHash.IsEmpty);
			Assert("Contact password should be removed", webEnabledContact.OC_PasswordSalt.IsEmpty);
			AssertEquals("Contact password should be removed", 0, webEnabledContact.OC_PasswordHashIterations);
			Assert("Contact password should be removed", inactiveContact.OC_PasswordHash.IsEmpty);
			Assert("Contact password should be removed", inactiveContact.OC_PasswordSalt.IsEmpty);
			AssertEquals("Contact password should be removed", 0, inactiveContact.OC_PasswordHashIterations);
			Assert("Contact password should be removed", nonWebContact.OC_PasswordHash.IsEmpty);
			Assert("Contact password should be removed", nonWebContact.OC_PasswordSalt.IsEmpty);
			AssertEquals("Contact password should be removed", 0, nonWebContact.OC_PasswordHashIterations);

			var createdEmails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("A password set confirmation should have been sent to the active web access contact", 1, createdEmails.Count);
			AssertEquals(1, createdEmails.Count(x => x.Recipients.Contains(webEnabledContact.OC_Email)));
		}

		public void TestSetMasterPasswordShouldEnforcePasswordPolicy()
		{
			var webEnabledContact = Factory.NewWithValidTestData<OrgContact>();
			webEnabledContact.OC_Email = "camo@mile.com";
			webEnabledContact.OC_WebAccessEnabled = true;
			webEnabledContact.SetHashedPassword("1234");
			Factory.Save();
			var person = webEnabledContact.Person;
			person.PER_EmailAddress = "earl@grey.com";
			Factory.Save();
			Assert("Precondition", webEnabledContact.OC_IsActive);
			Assert("Precondition", webEnabledContact.OC_WebAccessEnabled);

			var masterPasswordHelper = new WebUserAdminManager(webEnabledContact);

			var result = masterPasswordHelper.SetMasterPassword("1aA!", "1aA!");
			var freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			AssertEquals("Password should not have changed", true, freshContact.VerifyPassword("1234"));
			AssertEquals("Password must be at least 12 characters long.", result);

			result = masterPasswordHelper.SetMasterPassword("1a!abacusabacus", "1a!abacusabacus");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			AssertEquals("Password should not have changed", true, freshContact.VerifyPassword("1234"));
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", result);

			result = masterPasswordHelper.SetMasterPassword("1A!ABACUSABACUS", "1A!ABACUSABACUS");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			AssertEquals("Password should not have changed", true, freshContact.VerifyPassword("1234"));
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", result);

			result = masterPasswordHelper.SetMasterPassword("Aa!abacusabacus", "Aa!abacusabacus");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			AssertEquals("Password should not have changed", true, freshContact.VerifyPassword("1234"));
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", result);

			result = masterPasswordHelper.SetMasterPassword("Aa1abacusabacus", "Aa1abacusabacus");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			AssertEquals("Password should not have changed", true, freshContact.VerifyPassword("1234"));
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", result);

			result = masterPasswordHelper.SetMasterPassword("Aa1!abacusabacu", "Aa1!abacusabacu");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			AssertEquals("Password should have changed", false, freshContact.VerifyPassword("1234"));
			AssertEquals(masterPasswordHelper.PasswordChangeSuccess, result);
			AssertNotNull(freshContact.Person.PER_PasswordSalt);
			AssertNotNull(freshContact.Person.PER_PasswordHash);
			AssertEquals(DataRegistry.Instance.PasswordHashingIterationsCount, freshContact.Person.PER_PasswordHashIterations);
			Assert("Person Password should be updated", freshContact.VerifyPassword("Aa1!abacusabacu"));
			Assert("Person Password should be updated", freshContact.Person.VerifyPassword("Aa1!abacusabacu"));
		}

		public void TestSetMasterPasswordShouldNotAllowPersonName()
		{
			var webEnabledContact = Factory.NewWithValidTestData<OrgContact>();
			webEnabledContact.OC_Email = "camo@mile.com";
			webEnabledContact.OC_ContactName = "Glen A Maclarty (1)";
			webEnabledContact.OC_WebAccessEnabled = true;
			webEnabledContact.SetHashedPassword("1234");
			Factory.Save();
			var person = webEnabledContact.Person;
			person.PER_EmailAddress = "earl@grey.com";
			Factory.Save();
			Assert("Precondition", webEnabledContact.OC_IsActive);
			Assert("Precondition", webEnabledContact.OC_WebAccessEnabled);

			var masterPasswordHelper = new WebUserAdminManager(webEnabledContact);

			var result = masterPasswordHelper.SetMasterPassword("1aA!Iamglenglenasaurus", "1aA!Iamglenglenasaurus");
			var freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			AssertEquals("Password should not have changed", true, freshContact.VerifyPassword("1234"));
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, result);

			result = masterPasswordHelper.SetMasterPassword("1a!cAllMemAcLarty", "1a!cAllMemAcLarty");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			AssertEquals("Password should not have changed", true, freshContact.VerifyPassword("1234"));
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, result);

			result = masterPasswordHelper.SetMasterPassword("1a!cAllMemAcvarty(1)", "1a!cAllMemAcvarty(1)");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			Assert("Password should have changed - initial A and number (1) shouldn't be blacklisted", freshContact.VerifyPassword("1a!cAllMemAcvarty(1)"));
			AssertEquals("Your password has been changed.", result);
		}

		public void TestChangeMasterPassword()
		{
			var webEnabledContact = Factory.NewWithValidTestData<OrgContact>();
			webEnabledContact.OC_Email = "camo@mile.com";
			webEnabledContact.OC_WebAccessEnabled = true;
			webEnabledContact.SetHashedPassword("1234");
			Factory.Save();
			var person = webEnabledContact.Person;
			person.PER_EmailAddress = "earl@grey.com";
			person.SetHashedPassword("green");
			var inactiveContact = Factory.NewWithValidTestData<OrgContact>();
			inactiveContact.OC_Email = "mile@mile.com";
			inactiveContact.OC_WebAccessEnabled = true;
			inactiveContact.OC_IsActive = false;
			inactiveContact.OC_PER = person.PK;
			inactiveContact.SetHashedPassword("1234");
			var nonWebContact = Factory.NewWithValidTestData<OrgContact>();
			nonWebContact.OC_Email = "bleach@mile.com";
			nonWebContact.OC_WebAccessEnabled = false;
			nonWebContact.OC_PER = person.PK;
			nonWebContact.SetHashedPassword("1234");
			Factory.Save();
			Assert("Precondition", webEnabledContact.OC_IsActive);
			Assert("Precondition", webEnabledContact.OC_WebAccessEnabled);
			Assert("Precondition", !inactiveContact.OC_IsActive);
			Assert("Precondition", inactiveContact.OC_WebAccessEnabled);
			Assert("Precondition", nonWebContact.OC_IsActive);
			Assert("Precondition", !nonWebContact.OC_WebAccessEnabled);

			var masterPasswordHelper = new WebUserAdminManager(webEnabledContact);

			AssertEquals("The password supplied does not match your current password.", masterPasswordHelper.ChangeMasterPassword("different", "This1sMyN3wPassw0rd!", "This1sMyN3wPassw0rd!"));
			webEnabledContact.Factory.Save();
			AssertEquals(false, webEnabledContact.Person.VerifyPassword("This1sMyN3wPassw0rd!"));

			AssertEquals("The password and confirmation do not match.", masterPasswordHelper.ChangeMasterPassword("green", "This1sMyN3wPassw0rd!", "password"));
			webEnabledContact.Factory.Save();
			AssertEquals(false, webEnabledContact.Person.VerifyPassword("This1sMyN3wPassw0rd!"));

			AssertEquals(masterPasswordHelper.PasswordChangeSuccess, masterPasswordHelper.ChangeMasterPassword("green", "This1sMyN3wPassw0rd!", "This1sMyN3wPassw0rd!"));
			webEnabledContact.Factory.Save();
			AssertEquals(true, webEnabledContact.Person.VerifyPassword("This1sMyN3wPassw0rd!"));
			Assert("Contact password should be removed", webEnabledContact.OC_PasswordHash.IsEmpty);
			Assert("Contact password should be removed", webEnabledContact.OC_PasswordSalt.IsEmpty);
			AssertEquals("Contact password should be removed", 0, webEnabledContact.OC_PasswordHashIterations);
			Assert("Contact password should be removed", inactiveContact.OC_PasswordHash.IsEmpty);
			Assert("Contact password should be removed", inactiveContact.OC_PasswordSalt.IsEmpty);
			AssertEquals("Contact password should be removed", 0, inactiveContact.OC_PasswordHashIterations);
			Assert("Contact password should be removed", nonWebContact.OC_PasswordHash.IsEmpty);
			Assert("Contact password should be removed", nonWebContact.OC_PasswordSalt.IsEmpty);
			AssertEquals("Contact password should be removed", 0, nonWebContact.OC_PasswordHashIterations);

			var createdEmails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("A password set confirmation should have been sent to the active web access contact", 1, createdEmails.Count);
			AssertEquals(1, createdEmails.Count(x => x.Recipients.Contains(webEnabledContact.OC_Email)));
		}

		public void TestChangeMasterPasswordShouldEnforcePasswordPolicy()
		{
			var webEnabledContact = Factory.NewWithValidTestData<OrgContact>();
			webEnabledContact.OC_Email = "camo@mile.com";
			webEnabledContact.OC_WebAccessEnabled = true;
			Factory.Save();
			var person = webEnabledContact.Person;
			person.SetHashedPassword("1234");
			person.PER_EmailAddress = "earl@grey.com";
			Factory.Save();
			Assert("Precondition", webEnabledContact.OC_IsActive);
			Assert("Precondition", webEnabledContact.OC_WebAccessEnabled);

			var masterPasswordHelper = new WebUserAdminManager(webEnabledContact);

			var result = masterPasswordHelper.ChangeMasterPassword("1234", "1aA!", "1aA!");
			var freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			AssertEquals("Password should not have changed", true, freshContact.VerifyPassword("1234"));
			AssertEquals("Password must be at least 12 characters long.", result);

			result = masterPasswordHelper.ChangeMasterPassword("1234", "1a!abacusabacus", "1a!abacusabacus");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			AssertEquals("Password should not have changed", true, freshContact.VerifyPassword("1234"));
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", result);

			result = masterPasswordHelper.ChangeMasterPassword("1234", "1A!ABACUSABACUS", "1A!ABACUSABACUS");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			AssertEquals("Password should not have changed", true, freshContact.VerifyPassword("1234"));
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", result);

			result = masterPasswordHelper.ChangeMasterPassword("1234", "Aa!abacusabacus", "Aa!abacusabacus");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			AssertEquals("Password should not have changed", true, freshContact.VerifyPassword("1234"));
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", result);

			result = masterPasswordHelper.ChangeMasterPassword("1234", "Aa1abacusabacus", "Aa1abacusabacus");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			AssertEquals("Password should not have changed", true, freshContact.VerifyPassword("1234"));
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", result);

			result = masterPasswordHelper.ChangeMasterPassword("1234", "Aa1!abacusabacu", "Aa1!abacusabacu");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			AssertEquals("Password should have changed", false, freshContact.VerifyPassword("1234"));
			AssertEquals(masterPasswordHelper.PasswordChangeSuccess, result);
			AssertNotNull(freshContact.Person.PER_PasswordSalt);
			AssertNotNull(freshContact.Person.PER_PasswordHash);
			AssertEquals(DataRegistry.Instance.PasswordHashingIterationsCount, freshContact.Person.PER_PasswordHashIterations);
			Assert("Person Password should be updated", freshContact.VerifyPassword("Aa1!abacusabacu"));
			Assert("Person Password should be updated", freshContact.Person.VerifyPassword("Aa1!abacusabacu"));
		}

		public void TestChangeMasterPasswordShouldNotAllowPersonName()
		{
			var webEnabledContact = Factory.NewWithValidTestData<OrgContact>();
			webEnabledContact.OC_Email = "camo@mile.com";
			webEnabledContact.OC_ContactName = "Glen A Maclarty (1)";
			webEnabledContact.OC_WebAccessEnabled = true;
			webEnabledContact.Person.SetHashedPassword("1234");
			Factory.Save();
			var person = webEnabledContact.Person;
			person.PER_EmailAddress = "earl@grey.com";
			Factory.Save();
			Assert("Precondition", webEnabledContact.OC_IsActive);
			Assert("Precondition", webEnabledContact.OC_WebAccessEnabled);

			var masterPasswordHelper = new WebUserAdminManager(webEnabledContact);

			var result = masterPasswordHelper.ChangeMasterPassword("1234", "1aA!Iamglenglenasaurus", "1aA!Iamglenglenasaurus");
			var freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			AssertEquals("Password should not have changed", true, freshContact.VerifyPassword("1234"));
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, result);

			result = masterPasswordHelper.ChangeMasterPassword("1234", "1a!cAllMemAcLarty", "1a!cAllMemAcLarty");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			AssertEquals("Password should not have changed", true, freshContact.VerifyPassword("1234"));
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, result);

			result = masterPasswordHelper.ChangeMasterPassword("1234", "1a!cAllMemAcvarty(1)", "1a!cAllMemAcvarty(1)");
			freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			Assert("Password should have changed - initial A and number (1) shouldn't be blacklisted", freshContact.VerifyPassword("1a!cAllMemAcvarty(1)"));
			AssertEquals("Your password has been changed.", result);
		}

		public void TestChangeMasterPasswordShouldShowPasswordRequirementsMessageBeforeMismatch()
		{
			var webEnabledContact = Factory.NewWithValidTestData<OrgContact>();
			webEnabledContact.OC_Email = "camo@mile.com";
			webEnabledContact.OC_WebAccessEnabled = true;
			Factory.Save();
			var person = webEnabledContact.Person;
			person.SetHashedPassword("1234");
			person.PER_EmailAddress = "earl@grey.com";
			Factory.Save();
			Assert("Precondition", webEnabledContact.OC_IsActive);
			Assert("Precondition", webEnabledContact.OC_WebAccessEnabled);

			var masterPasswordHelper = new WebUserAdminManager(webEnabledContact);

			var result = masterPasswordHelper.ChangeMasterPassword("1234", "1aA!", "iaohsfiobn@sA1sfo");
			var freshContact = new BusinessObjectFactory().Load<OrgContact>(webEnabledContact.PK);
			AssertEquals("Password should not have changed", true, freshContact.VerifyPassword("1234"));
			AssertEquals("Should show password requirements message before mismatch since password will need to be changed regardless", "Password must be at least 12 characters long.", result);
		}

		public void TestSetMasterPassword_CompanySpecific_Contact()
		{
			var webEnabledContact = Factory.NewWithValidTestData<OrgContact>();
			webEnabledContact.OC_Email = "camo@mile.com";
			webEnabledContact.OC_WebAccessEnabled = true;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			webEnabledContact.ParentOrg.CompanyData.OB_GB_ControllingBranch = branch.PK;
			Factory.Save();
			var person = webEnabledContact.Person;
			person.PER_EmailAddress = "earl@grey.com";
			var otherContact = Factory.NewWithValidTestData<OrgContact>();
			otherContact.OC_Email = "alo@mile.com";
			otherContact.OC_WebAccessEnabled = true;
			otherContact.OC_PER = person.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			otherContact.ParentOrg.CompanyData.OB_GB_ControllingBranch = branch2.PK;
			Factory.Save();

			AssertNotEquals("Precondition", Guid.Empty, webEnabledContact.ParentOrg.Branch.Company.PK);
			webEnabledContact.CompanyPKForEmailTemplate = webEnabledContact.ParentOrg.Branch.Company.PK.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, otherContact.ParentOrg.Branch.Company.PK);

			const string emailSubject = "KRAV";
			const string emailBody = "BLAH";
			var expectedFooter = ResString.GetMultilingualString("E36C91AF-2FC4-4D91-A392-2F42C79739FF", "Test Footer");
			WebDataRegistry.Instance.MasterPasswordSetSuccessfullyEmailTemplate.SetValue(webEnabledContact.CompanyPKForEmailTemplate, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = emailSubject, EmailBody = emailBody });
			WebDataRegistry.Instance.MasterPasswordSetSuccessfullyEmailTemplate.SetValue(otherContact.ParentOrg.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "faf", EmailBody = "double" });
			WebDataRegistry.Instance.PasswordSetEmailFooter.SetValue(webEnabledContact.CompanyPKForEmailTemplate, Guid.Empty, Guid.Empty, expectedFooter);
			WebDataRegistry.Instance.PasswordSetEmailFooter.SetValue(otherContact.ParentOrg.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, ResString.GetMultilingualString("18396612-b171-47d5-99c0-22d818bac5ab", "Other Footer"));
			Assert("Precondition", webEnabledContact.OC_IsActive);
			Assert("Precondition", webEnabledContact.OC_WebAccessEnabled);

			var masterPasswordHelper = new WebUserAdminManager(webEnabledContact);

			AssertEquals(masterPasswordHelper.PasswordChangeSuccess, masterPasswordHelper.SetMasterPassword("This1sMyN3wPassw0rd!", "This1sMyN3wPassw0rd!", PasswordInstructionType.Set));
			webEnabledContact.Factory.Save();
			AssertEquals(true, webEnabledContact.Person.VerifyPassword("This1sMyN3wPassw0rd!"));

			var createdEmails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("A password set confirmation should have been sent to the active web access contact", 2, createdEmails.Count);
			AssertEquals(1, createdEmails.Count(x => x.Recipients.Contains(webEnabledContact.OC_Email)));
			AssertEquals("All emails should use template from the web enabled contact's CompanyPKForEmailTemplate", 2, createdEmails.Count(x => x.Subject.Contains(emailSubject)));
			AssertEquals("All emails should use template from the web enabled contact's CompanyPKForEmailTemplate", 2, createdEmails.Count(x => x.Body.Contains(emailBody)));
			AssertEquals("All emails should use template from the web enabled contact's CompanyPKForEmailTemplate", 2, createdEmails.Count(x => x.Body.Contains(expectedFooter)));
		}

		public void TestSetMasterPassword_TemplateFallback_Contact()
		{
			var webEnabledContact = Factory.NewWithValidTestData<OrgContact>();
			webEnabledContact.OC_Email = "camo@mile.com";
			webEnabledContact.OC_WebAccessEnabled = true;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			webEnabledContact.ParentOrg.CompanyData.OB_GB_ControllingBranch = branch.PK;
			Factory.Save();
			var person = webEnabledContact.Person;
			person.PER_EmailAddress = "earl@grey.com";
			var otherContact = Factory.NewWithValidTestData<OrgContact>();
			otherContact.OC_Email = "alo@mile.com";
			otherContact.OC_WebAccessEnabled = true;
			otherContact.OC_PER = person.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			otherContact.ParentOrg.CompanyData.OB_GB_ControllingBranch = branch2.PK;
			var duplicateEmailContact = Factory.NewWithValidTestData<OrgContact>();
			duplicateEmailContact.OC_Email = "alo@mile.com";
			duplicateEmailContact.OC_WebAccessEnabled = true;
			duplicateEmailContact.OC_PER = person.PK;
			Factory.Save();
			AssertNotEquals("Precondition", Guid.Empty, webEnabledContact.ParentOrg.Branch.Company.PK);

			AssertNotEquals("Precondition", Guid.Empty, otherContact.ParentOrg.Branch.Company.PK);

			const string fallbackSubject = "KRAV";
			const string otherSubject = "GAGA";
			const string fallbackBody = "BLAH";
			const string otherBody = "HAHA";
			var fallbackFooter = ResString.GetMultilingualString("E36C91AF-2FC4-4D91-A392-2F42C79739FF", "Test Footer");
			var otherFooter = ResString.GetMultilingualString("7caf13c9-6851-49fa-973f-9b51927b61cc", "Other Footer");

			WebDataRegistry.Instance.MasterPasswordSetSuccessfullyEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = fallbackSubject, EmailBody = fallbackBody });
			WebDataRegistry.Instance.MasterPasswordSetSuccessfullyEmailTemplate.SetValue(otherContact.ParentOrg.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = otherSubject, EmailBody = otherBody });
			WebDataRegistry.Instance.PasswordSetEmailFooter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackFooter);
			WebDataRegistry.Instance.PasswordSetEmailFooter.SetValue(otherContact.ParentOrg.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, otherFooter);
			Assert("Precondition", webEnabledContact.OC_IsActive);
			Assert("Precondition", webEnabledContact.OC_WebAccessEnabled);

			var masterPasswordHelper = new WebUserAdminManager(webEnabledContact);

			AssertEquals(masterPasswordHelper.PasswordChangeSuccess, masterPasswordHelper.SetMasterPassword("This1sMyN3wPassw0rd!", "This1sMyN3wPassw0rd!", PasswordInstructionType.Set));
			webEnabledContact.Factory.Save();
			AssertEquals(true, webEnabledContact.Person.VerifyPassword("This1sMyN3wPassw0rd!"));

			var createdEmails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("A password set confirmation should have been sent to the active web access contacts", 2, createdEmails.Count);
			var firstContactEmail = createdEmails.FirstOrDefault(x => x.Recipients.Contains(webEnabledContact.OC_Email));
			var otherContactEmail = createdEmails.FirstOrDefault(x => x.Recipients.Contains(otherContact.OC_Email));
			AssertEquals(1, createdEmails.Count(x => x.Recipients.Contains(webEnabledContact.OC_Email)));
			AssertEquals(1, createdEmails.Count(x => x.Recipients.Contains(otherContact.OC_Email)));
			AssertContains(fallbackSubject, firstContactEmail.Subject);
			AssertContains(fallbackBody, firstContactEmail.Body);
			AssertContains(fallbackFooter, firstContactEmail.Body);
			AssertContains(otherSubject, otherContactEmail.Subject);
			AssertContains(otherBody, otherContactEmail.Body);
			AssertContains(otherFooter, otherContactEmail.Body);
		}

		public void TestChangeMasterPassword_CompanySpecific_Contact()
		{
			var webEnabledContact = Factory.NewWithValidTestData<OrgContact>();
			webEnabledContact.OC_Email = "camo@mile.com";
			webEnabledContact.OC_WebAccessEnabled = true;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			webEnabledContact.ParentOrg.CompanyData.OB_GB_ControllingBranch = branch.PK;
			Factory.Save();
			var person = webEnabledContact.Person;
			person.PER_EmailAddress = "earl@grey.com";
			person.SetHashedPassword("green");
			var otherContact = Factory.NewWithValidTestData<OrgContact>();
			otherContact.OC_Email = "alo@mile.com";
			otherContact.OC_WebAccessEnabled = true;
			otherContact.OC_PER = person.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			otherContact.ParentOrg.CompanyData.OB_GB_ControllingBranch = branch2.PK;
			Factory.Save();
			AssertNotEquals("Precondition", Guid.Empty, webEnabledContact.ParentOrg.Branch.Company.PK);
			webEnabledContact.CompanyPKForEmailTemplate = webEnabledContact.ParentOrg.Branch.Company.PK.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, otherContact.ParentOrg.Branch.Company.PK);

			const string emailSubject = "KRAV";
			const string emailBody = "BLAH";
			var expectedFooter = ResString.GetMultilingualString("E36C91AF-2FC4-4D91-A392-2F42C79739FF", "Test Footer");
			WebDataRegistry.Instance.MasterPasswordResetSuccessfullyEmailTemplate.SetValue(webEnabledContact.CompanyPKForEmailTemplate, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = emailSubject, EmailBody = emailBody });
			WebDataRegistry.Instance.MasterPasswordResetSuccessfullyEmailTemplate.SetValue(otherContact.ParentOrg.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "faf", EmailBody = "double" });
			WebDataRegistry.Instance.PasswordResetEmailFooter.SetValue(webEnabledContact.CompanyPKForEmailTemplate, Guid.Empty, Guid.Empty, expectedFooter);
			WebDataRegistry.Instance.PasswordResetEmailFooter.SetValue(otherContact.ParentOrg.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, ResString.GetMultilingualString("18396612-b171-47d5-99c0-22d818bac5ab", "Other Footer"));
			Assert("Precondition", webEnabledContact.OC_IsActive);
			Assert("Precondition", webEnabledContact.OC_WebAccessEnabled);

			var masterPasswordHelper = new WebUserAdminManager(webEnabledContact);

			AssertEquals(masterPasswordHelper.PasswordChangeSuccess, masterPasswordHelper.ChangeMasterPassword("green", "This1sMyN3wPassw0rd!", "This1sMyN3wPassw0rd!"));
			webEnabledContact.Factory.Save();
			AssertEquals(true, webEnabledContact.Person.VerifyPassword("This1sMyN3wPassw0rd!"));

			var createdEmails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("A password set confirmation should have been sent to the active web access contact", 2, createdEmails.Count);
			AssertEquals(1, createdEmails.Count(x => x.Recipients.Contains(webEnabledContact.OC_Email)));
			AssertEquals("All emails should use template from the web enabled contact's CompanyPKForEmailTemplate", 2, createdEmails.Count(x => x.Subject.Contains(emailSubject)));
			AssertEquals("All emails should use template from the web enabled contact's CompanyPKForEmailTemplate", 2, createdEmails.Count(x => x.Body.Contains(emailBody)));
			AssertEquals("All emails should use template from the web enabled contact's CompanyPKForEmailTemplate", 2, createdEmails.Count(x => x.Body.Contains(expectedFooter)));
		}

		public void TestChangeMasterPassword_TemplateFallback_Contact()
		{
			var webEnabledContact = Factory.NewWithValidTestData<OrgContact>();
			webEnabledContact.OC_Email = "camo@mile.com";
			webEnabledContact.OC_WebAccessEnabled = true;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			webEnabledContact.ParentOrg.CompanyData.OB_GB_ControllingBranch = branch.PK;
			Factory.Save();
			var person = webEnabledContact.Person;
			person.PER_EmailAddress = "earl@grey.com";
			person.SetHashedPassword("green");
			var otherContact = Factory.NewWithValidTestData<OrgContact>();
			otherContact.OC_Email = "alo@mile.com";
			otherContact.OC_WebAccessEnabled = true;
			otherContact.OC_PER = person.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			otherContact.ParentOrg.CompanyData.OB_GB_ControllingBranch = branch2.PK;
			var duplicateEmailContact = Factory.NewWithValidTestData<OrgContact>();
			duplicateEmailContact.OC_Email = "alo@mile.com";
			duplicateEmailContact.OC_WebAccessEnabled = true;
			duplicateEmailContact.OC_PER = person.PK;
			Factory.Save();
			AssertNotEquals("Precondition", Guid.Empty, webEnabledContact.ParentOrg.Branch.Company.PK);

			AssertNotEquals("Precondition", Guid.Empty, otherContact.ParentOrg.Branch.Company.PK);

			const string fallbackSubject = "KRAV";
			const string otherSubject = "GAGA";
			const string fallbackBody = "BLAH";
			const string otherBody = "HAHA";
			var fallbackFooter = ResString.GetMultilingualString("E36C91AF-2FC4-4D91-A392-2F42C79739FF", "Test Footer");
			var otherFooter = ResString.GetMultilingualString("7caf13c9-6851-49fa-973f-9b51927b61cc", "Other Footer");

			WebDataRegistry.Instance.MasterPasswordResetSuccessfullyEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = fallbackSubject, EmailBody = fallbackBody });
			WebDataRegistry.Instance.MasterPasswordResetSuccessfullyEmailTemplate.SetValue(otherContact.ParentOrg.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = otherSubject, EmailBody = otherBody });
			WebDataRegistry.Instance.PasswordResetEmailFooter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackFooter);
			WebDataRegistry.Instance.PasswordResetEmailFooter.SetValue(otherContact.ParentOrg.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, otherFooter);
			Assert("Precondition", webEnabledContact.OC_IsActive);
			Assert("Precondition", webEnabledContact.OC_WebAccessEnabled);

			var masterPasswordHelper = new WebUserAdminManager(webEnabledContact);

			AssertEquals(masterPasswordHelper.PasswordChangeSuccess, masterPasswordHelper.ChangeMasterPassword("green", "This1sMyN3wPassw0rd!", "This1sMyN3wPassw0rd!"));
			webEnabledContact.Factory.Save();
			AssertEquals(true, webEnabledContact.Person.VerifyPassword("This1sMyN3wPassw0rd!"));

			var createdEmails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("A password set confirmation should have been sent to the active web access contacts", 2, createdEmails.Count);
			var firstContactEmail = createdEmails.FirstOrDefault(x => x.Recipients.Contains(webEnabledContact.OC_Email));
			var otherContactEmail = createdEmails.FirstOrDefault(x => x.Recipients.Contains(otherContact.OC_Email));
			AssertEquals(1, createdEmails.Count(x => x.Recipients.Contains(webEnabledContact.OC_Email)));
			AssertEquals(1, createdEmails.Count(x => x.Recipients.Contains(otherContact.OC_Email)));
			AssertContains(fallbackSubject, firstContactEmail.Subject);
			AssertContains(fallbackBody, firstContactEmail.Body);
			AssertContains(fallbackFooter, firstContactEmail.Body);
			AssertContains(otherSubject, otherContactEmail.Subject);
			AssertContains(otherBody, otherContactEmail.Body);
			AssertContains(otherFooter, otherContactEmail.Body);
		}

		public void TestSetMasterPasswordShouldAddLog()
		{
			var webEnabledContact = Factory.NewWithValidTestData<OrgContact>();
			webEnabledContact.OC_Email = "camo@mile.com";
			webEnabledContact.OC_WebAccessEnabled = true;
			Factory.Save();
			var person = webEnabledContact.Person;
			person.PER_EmailAddress = "earl@grey.com";
			Factory.Save();
			Assert("Precondition", webEnabledContact.OC_IsActive);
			Assert("Precondition", webEnabledContact.OC_WebAccessEnabled);

			var masterPasswordHelper = new WebUserAdminManager(webEnabledContact);
			AssertEquals("Precondition: Should not have log", false, webEnabledContact.Person.GetLogs().DatabaseHasLogs(new ZQuery(StmALogSchema.SL_SE_NKEvent, new[] { AutoEvents.WebAccessPasswordChangedCode })));

			AssertEquals(masterPasswordHelper.PasswordChangeSuccess, masterPasswordHelper.SetMasterPassword("This1sMyN3wPassw0rd!", "This1sMyN3wPassw0rd!"));
			webEnabledContact.Factory.Save();
			AssertEquals("Precondition", true, webEnabledContact.Person.VerifyPassword("This1sMyN3wPassw0rd!"));
			AssertEquals("Should add password changed log to the person", true, webEnabledContact.Person.GetLogs().DatabaseHasLogs(new ZQuery(StmALogSchema.SL_SE_NKEvent, new[] { AutoEvents.WebAccessPasswordChangedCode })));
		}

		public void TestChangeMasterPasswordShouldAddLog()
		{
			var webEnabledContact = Factory.NewWithValidTestData<OrgContact>();
			webEnabledContact.OC_Email = "camo@mile.com";
			webEnabledContact.OC_WebAccessEnabled = true;
			Factory.Save();
			var person = webEnabledContact.Person;
			person.PER_EmailAddress = "earl@grey.com";
			person.SetHashedPassword("green");
			Factory.Save();
			Assert("Precondition", webEnabledContact.OC_IsActive);
			Assert("Precondition", webEnabledContact.OC_WebAccessEnabled);

			var masterPasswordHelper = new WebUserAdminManager(webEnabledContact);

			AssertEquals("Precondition", "The password and confirmation do not match.", masterPasswordHelper.ChangeMasterPassword("green", "This1sMyN3wPassw0rd!", "password"));
			webEnabledContact.Factory.Save();
			AssertEquals("Precondition", false, webEnabledContact.Person.VerifyPassword("This1sMyN3wPassw0rd!"));
			AssertEquals("Should not add log", false, webEnabledContact.Person.GetLogs().DatabaseHasLogs(new ZQuery(StmALogSchema.SL_SE_NKEvent, new[] { AutoEvents.WebAccessPasswordChangedCode })));

			AssertEquals(masterPasswordHelper.PasswordChangeSuccess, masterPasswordHelper.ChangeMasterPassword("green", "This1sMyN3wPassw0rd!", "This1sMyN3wPassw0rd!"));
			webEnabledContact.Factory.Save();
			AssertEquals("Precondition", true, webEnabledContact.Person.VerifyPassword("This1sMyN3wPassw0rd!"));
			AssertEquals("Should add password changed log to the person", true, webEnabledContact.Person.GetLogs().DatabaseHasLogs(new ZQuery(StmALogSchema.SL_SE_NKEvent, new[] { AutoEvents.WebAccessPasswordChangedCode })));
		}

		public void TestCopyMasterPasswordFromContact()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			contact.OC_Email = "abba@gmail.com";
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword("5678");
			contact2.OC_Email = "zoro@gmail.com";
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			contact2.ParentOrg.CompanyData.OB_GB_ControllingBranch = branch2.PK;
			Factory.Save();
			Assert("Precondition", !contact.Person.HasPassword);
			AssertNotEquals("Precondition", Guid.Empty, contact2.ParentOrg.Branch.Company.PK);
			AssertEquals("Precondition: Should not have log", false, contact.Person.GetLogs().DatabaseHasLogs(new ZQuery(StmALogSchema.SL_SE_NKEvent, new[] { AutoEvents.WebAccessPasswordChangedCode })));

			const string fallbackSubject = "KRAV";
			const string fallbackBody = "BLAH";
			var fallbackFooter = ResString.GetMultilingualString("E36C91AF-2FC4-4D91-A392-2F42C79739FF", "Test Footer");
			const string otherSubject = "GAGA";
			const string otherBody = "HAHA";
			var otherFooter = ResString.GetMultilingualString("7caf13c9-6851-49fa-973f-9b51927b61cc", "Other Footer");

			WebDataRegistry.Instance.MasterPasswordSetSuccessfullyEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = fallbackSubject, EmailBody = fallbackBody });
			WebDataRegistry.Instance.MasterPasswordSetSuccessfullyEmailTemplate.SetValue(contact2.ParentOrg.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = otherSubject, EmailBody = otherBody });
			WebDataRegistry.Instance.PasswordSetEmailFooter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackFooter);
			WebDataRegistry.Instance.PasswordSetEmailFooter.SetValue(contact2.ParentOrg.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, otherFooter);

			var masterPasswordHelper = new WebUserAdminManager(contact);
			AssertEquals(masterPasswordHelper.PasswordChangeSuccess, masterPasswordHelper.CopyMasterPasswordFromContact(contact));

			var newFactory = new BusinessObjectFactory();
			var reloadedPerson = newFactory.Load<GlbPerson>(contact.OC_PER);
			AssertEquals("Password should be updated and saved", true, reloadedPerson.VerifyPassword("1234"));
			AssertEquals(true, contact.OC_PasswordHash.IsEmpty);
			AssertEquals(0, contact.OC_PasswordHashIterations);
			AssertEquals(true, contact.OC_PasswordSalt.IsEmpty);
			AssertEquals(true, contact2.OC_PasswordHash.IsEmpty);
			AssertEquals(0, contact2.OC_PasswordHashIterations);
			AssertEquals(true, contact2.OC_PasswordSalt.IsEmpty);
			AssertEquals("Should add password changed log to the person", true, contact.Person.GetLogs().DatabaseHasLogs(new ZQuery(StmALogSchema.SL_SE_NKEvent, new[] { AutoEvents.WebAccessPasswordChangedCode })));

			var createdEmails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("A password set confirmation should have been sent to the active web access contacts", 2, createdEmails.Count);
			var contactEmail = createdEmails.FirstOrDefault(x => x.Recipients.Contains(contact.OC_Email));
			var contact2Email = createdEmails.FirstOrDefault(x => x.Recipients.Contains(contact2.OC_Email));
			AssertEquals(1, createdEmails.Count(x => x.Recipients.Contains(contact.OC_Email)));
			AssertEquals(1, createdEmails.Count(x => x.Recipients.Contains(contact2.OC_Email)));
			AssertContains(fallbackSubject, contactEmail.Subject);
			AssertContains(fallbackBody, contactEmail.Body);
			AssertContains(fallbackFooter, contactEmail.Body);
			AssertContains(otherSubject, contact2Email.Subject);
			AssertContains(otherBody, contact2Email.Body);
			AssertContains(otherFooter, contact2Email.Body);
		}

		public void TestLogPasswordChangeFailure()
		{
			WebDataRegistry.Instance.PasswordSetSuccessfullyEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "MLAH", EmailBody = "WAH" });
			GlbStaff.CurrentUser.GS_EmailAddress = "example@cargowise.com";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "WiseTech Global";
			org.OH_Code = "WiseTech";
			org.CompanyData.OB_GB_ControllingBranch = branch.PK;
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Contact1 CW1";
			contact1.OC_Email = "contact1.cw1@cargowise.com";
			contact1.SetHashedPassword("Th1s1sMyPassw0rd");

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Contact2 CW1";
			contact2.OC_Email = "contact2.cw1@cargowise.com";
			contact2.SetHashedPassword("Th1s1sMyPassw0rd");

			Factory.Save();
			void FactorySavingWithException(BusinessObjectFactory factory) => throw new NotImplementedException();

			var manager = new WebUserAdminManager(contact1);
			contact1.OC_NotifyMode = "@@@";
			AssertHasErrors(contact1.OC_NotifyModeInfo);
			var result = manager.ChangePassword("@This1sMyN3wPassw0rd", "@This1sMyN3wPassw0rd");
			AssertEquals(false, result.IsSuccess);
			AssertEquals("Unable to update your password. Please contact your Customer Service Representative.", result.Message);
			AssertMostRecentLog(contact1, "ERR", "PasswordChangeFailure\r\nOrgContact\r\nError - OC_NotifyMode: Enter a valid Notify Mode.\r\n");

			contact1.OC_NotifyMode = "EML";
			contact1.Factory.Saving += FactorySavingWithException;
			manager.ChangePassword("@This1sMyN3wPassw0rd", "@This1sMyN3wPassw0rd");
			AssertEquals(false, result.IsSuccess);
			AssertEquals("Unable to update your password. Please contact your Customer Service Representative.", result.Message);
			AssertMostRecentLog(contact1, "ERR", "PasswordChangeFailure\r\nThe method or operation is not implemented.");

			var person = contact2.Person;
			contact1.Factory.Saving -= FactorySavingWithException;
			manager = new WebUserAdminManager(contact2);
			person.PER_Gender = "@";
			AssertHasErrors(person.PER_GenderInfo);
			var resultAsString = manager.SetMasterPassword("@This1sMyN3wPassw0rd", "@This1sMyN3wPassw0rd");
			AssertEquals("Unable to update your password. Please contact your Customer Service Representative.", resultAsString);
			AssertMostRecentLog(person, "ERR", "PasswordChangeFailure\r\nGlbPerson\r\nError - PER_Gender: Enter a valid Gender.");

			person.PER_Gender = "M";
			person.Factory.Saving += FactorySavingWithException;
			resultAsString = manager.SetMasterPassword("@This1sMyN3wPassw0rd", "@This1sMyN3wPassw0rd");
			AssertEquals("Unable to update your password. Please contact your Customer Service Representative.", resultAsString);
			AssertMostRecentLog(person, "ERR", "PasswordChangeFailure\r\nThe method or operation is not implemented.");

			using (SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertMostRecentLog(contact2, "ERR", "PasswordChangeFailure\r\nThe method or operation is not implemented.");
			}
		}

		public void TestContactPasswordWithUsedPasswords()
		{
			WebDataRegistry.Instance.WebPasswordHistoryCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			WebDataRegistry.Instance.PasswordSetSuccessfullyEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "MLAH", EmailBody = "WAH" });
			GlbStaff.CurrentUser.GS_EmailAddress = "example@cargowise.com";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "WiseTech Global";
			org.OH_Code = "WiseTech";
			org.CompanyData.OB_GB_ControllingBranch = branch.PK;
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Contact1 CW1";
			contact1.OC_Email = "contact1.cw1@cargowise.com";
			Factory.Save();

			var manager = new WebUserAdminManager(contact1);
			var resultAsString = manager.ChangePassword("@This1sMyN3wPassw0rd", "@This1sMyN3wPassw0rd").Message;
			AssertEquals("Your password has been changed.", resultAsString);
			resultAsString = manager.ChangePassword("@This1sMyN3wPassw0rd+", "@This1sMyN3wPassw0rd+").Message;
			AssertEquals("Your password has been changed.", resultAsString);
			resultAsString = manager.ChangePassword("@This1sMyN3wPassw0rd+", "@This1sMyN3wPassw0rd++", "@This1sMyN3wPassw0rd++");
			AssertEquals("Your password has been changed.", resultAsString);
			Factory.Save();

			resultAsString = manager.ChangePassword("@This1sMyN3wPassw0rd", "@This1sMyN3wPassw0rd").Message;
			AssertEquals("The password entered has been used previously. Please enter a new password.", resultAsString);
			resultAsString = manager.ChangePassword("@This1sMyN3wPassw0rd+", "@This1sMyN3wPassw0rd+").Message;
			AssertEquals("The password entered has been used previously. Please enter a new password.", resultAsString);
			resultAsString = manager.ChangePassword("@This1sMyN3wPassw0rd++", "@This1sMyN3wPassw0rd++").Message;
			AssertEquals("The password entered has been used previously. Please enter a new password.", resultAsString);
			resultAsString = manager.ChangePassword("@This1sMyN3wPassw0rd+++", "@This1sMyN3wPassw0rd+++").Message;
			AssertEquals("Your password has been changed.", resultAsString);
		}

		public void TestMasterPasswordWithUsedPasswords()
		{
			WebDataRegistry.Instance.WebPasswordHistoryCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			WebDataRegistry.Instance.PasswordSetSuccessfullyEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "MLAH", EmailBody = "WAH" });
			GlbStaff.CurrentUser.GS_EmailAddress = "example@cargowise.com";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "WiseTech Global";
			org.OH_Code = "WiseTech";
			org.CompanyData.OB_GB_ControllingBranch = branch.PK;
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Contact1 CW1";
			contact1.OC_Email = "contact1.cw1@cargowise.com";
			Factory.Save();

			var manager = new WebUserAdminManager(contact1);
			var resultAsString = manager.SetMasterPassword("@This1sMyN3wPassw0rd", "@This1sMyN3wPassw0rd");
			AssertEquals("Your password has been changed.", resultAsString);
			resultAsString = manager.SetMasterPassword("@This1sMyN3wPassw0rd+", "@This1sMyN3wPassw0rd+");
			AssertEquals("Your password has been changed.", resultAsString);
			resultAsString = manager.ChangeMasterPassword("@This1sMyN3wPassw0rd+", "@This1sMyN3wPassw0rd++", "@This1sMyN3wPassw0rd++");
			AssertEquals("Your password has been changed.", resultAsString);

			resultAsString = manager.SetMasterPassword("@This1sMyN3wPassw0rd", "@This1sMyN3wPassw0rd");
			AssertEquals("The password entered has been used previously. Please enter a new password.", resultAsString);
			resultAsString = manager.SetMasterPassword("@This1sMyN3wPassw0rd+", "@This1sMyN3wPassw0rd+");
			AssertEquals("The password entered has been used previously. Please enter a new password.", resultAsString);
			resultAsString = manager.SetMasterPassword("@This1sMyN3wPassw0rd++", "@This1sMyN3wPassw0rd++");
			AssertEquals("The password entered has been used previously. Please enter a new password.", resultAsString);
			resultAsString = manager.SetMasterPassword("@This1sMyN3wPassw0rd+++", "@This1sMyN3wPassw0rd+++");
			AssertEquals("Your password has been changed.", resultAsString);
		}

		public void TestSetMasterPassword_ValidateNameAndEmail()
		{
			var webEnabledContact = Factory.NewWithValidTestData<OrgContact>();
			webEnabledContact.OC_Email = "camo@mile.com";
			webEnabledContact.OC_WebAccessEnabled = true;
			webEnabledContact.SetHashedPassword("1234");
			Factory.Save();
			var person = webEnabledContact.Person;
			person.PER_FullName = "Camo Mile";
			person.PER_EmailAddress = "earl@grey.com";
			Factory.Save();

			var masterPasswordHelper = new WebUserAdminManager(webEnabledContact);

			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, masterPasswordHelper.SetMasterPassword("camo!P4sword", "different", PasswordInstructionType.Set));
			webEnabledContact.Factory.Save();
			AssertEquals(false, webEnabledContact.Person.VerifyPassword("password!P4sword"));

			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, masterPasswordHelper.SetMasterPassword("earl!P4sword", "different", PasswordInstructionType.Set));
			webEnabledContact.Factory.Save();
			AssertEquals(false, webEnabledContact.Person.VerifyPassword("password!P4sword"));
		}

		void AssertMostRecentLog(BusinessObject bizObj, string eventCode, string expectedReferenceTextStart)
		{
			var log = new BusinessObjectFactory().Load(bizObj.GetType(), bizObj.PK).GetLogs().MostRecentLog;
			CombineAssertions(() =>
			{
				AssertEquals(eventCode, log.SL_SE_NKEvent);
				AssertStartsWith("", expectedReferenceTextStart, log.SL_Reference);
			});
		}

		#endregion

		protected virtual WebUserAdminManager GetNewWebUserAdminManagerForTest(OrgContact newContact)
		{
			return new WebUserAdminManager(newContact);
		}

		protected WebUserAdminManager GetWebUserAdminManagerForTest(OrgContact newContact)
		{
			return new WebUserAdminManager(newContact);
		}

		protected virtual string ExpectedFromContactName => Env.CurrentUser.IsWebUser ? Env.Registry.MailboxDisplayName : GlbStaff.CurrentUser.GS_FullName.ToString();

		protected virtual string ExpectedFromEmailAddress => Env.CurrentUser.IsWebUser ? Env.Registry.EnterpriseMailboxEmailAddress : GlbStaff.CurrentUser.GS_EmailAddress.ToString();

		protected virtual string ExpectedReplyTo => Env.Registry.SMTPDefaultReturnEmailAddress;

		protected virtual string AccountInfo => "WiseTech Global / WiseTech account";

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.MailboxDisplayName = "Company Name Co.";
			Env.Registry.EnterpriseMailboxEmailAddress = "company@company.com";
			Env.Registry.SMTPDefaultReturnEmailAddress = "reply@company.com";
		}

		class WebUserAdminManagerForTest : WebUserAdminManager
		{
			public WebUserAdminManagerForTest(OrgContact contact)
				: base(contact)
			{
				MailSender_Exposed = new WebUserConfirmationEmailSenderForTest(contact);
			}

			protected override WebUserConfirmationEmailSender CreateMailSender() => MailSender_Exposed;

			public WebUserConfirmationEmailSenderForTest MailSender_Exposed { get; }
		}

		class WebUserConfirmationEmailSenderForTest : WebUserConfirmationEmailSender
		{
			public WebUserConfirmationEmailSenderForTest(OrgContact contact)
				: base(contact)
			{
			}

			public Guid CompanyPk_Exposed => CompanyPK;
		}
	}
}
