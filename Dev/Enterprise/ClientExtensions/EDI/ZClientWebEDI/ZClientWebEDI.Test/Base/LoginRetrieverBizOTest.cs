using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(LoginRetrieverBizO))]
	[HttpContextEnabledTest]
	internal class LoginRetrieverBizOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEmail()
		{
			LoginRetrieverBizO bizO = new LoginRetrieverBizO(Factory);
			AssertEquals("", bizO.Email);
			bizO.Email = "MEH MEH";
			AssertEquals("MEH MEH", bizO.Email);
			AssertEquals(OrgContactSchema.OC_Email.MaxLength, bizO.EmailInfo.MaxLength);
		}

		public void TestDefaultOrgContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			CreateOrgContact(org, "Test User A", false, false);
			CreateOrgContact(org, "Test User B", false, false);
			AssertDefaultContact("Test User A");
			CreateOrgContact(org, "Test User C", false, true);
			CreateOrgContact(org, "Test User D", false, true);
			AssertDefaultContact("Test User C");
			CreateOrgContact(org, "Test User E", true, false);
			CreateOrgContact(org, "Test User F", true, false);
			AssertDefaultContact("Test User E");
			CreateOrgContact(org, "Test User G", true, true);
			AssertDefaultContact("Test User G");
		}

		public void TestDefaultContactShouldPrioritiseContactsWithPersonPassword()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_IsActive = true;
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_Email = "testuser@cargowise.com";
			contact1.OC_ContactName = "User A";
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_IsActive = true;
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_Email = "testuser@cargowise.com";
			contact2.OC_ContactName = "User B";
			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_IsActive = true;
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_Email = "testuser@cargowise.com";
			contact3.OC_ContactName = "User C";
			Factory.Save();
			contact2.Person.SetHashedPassword("password");
			Factory.Save();
			AssertDefaultContact(contact2.OC_ContactName, "contact 2's person has a password so should be prioritised.");
		}

		void AssertDefaultContact(string contactName, string message = "")
		{
			var bizO = new LoginRetrieverBizO(Factory);
			bizO.Email = "testuser@cargowise.com";
			AssertEquals(message, contactName, bizO.defaultOrgContact?.OC_ContactName);
		}

		void CreateOrgContact(OrgHeader org, string contactName, bool isActive, bool isWebAccessEnabled)
		{
			var contact = org.Contacts.AddNew();
			contact.FillWithValidTestData();
			contact.OC_IsActive = isActive;
			contact.OC_WebAccessEnabled = isWebAccessEnabled;
			contact.OC_Email = "testuser@cargowise.com";
			contact.OC_ContactName = contactName;
			Factory.Save();
		}

		public void TestRetrieve()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			OrgContact contact = org.Contacts.AddNew();
			contact.FillWithValidTestData();
			contact.OC_WebAccessEnabled = false;
			contact.OC_IsActive = false;
			contact.OC_Email = "testuser@cargowise.com";
			contact.OC_ContactName = "Test User";
			Factory.Save();
			LoginRetrieverBizO bizO = new LoginRetrieverBizO(Factory);
			bizO.Email = "MEH@meh.com";
			using (var page = TestPage)
			{
				Env.ClearUserContext();
				EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
				var message = bizO.Retrieve(page.AppInstance);
				AssertEquals("Should show generic information that doesn't allow mining of usernames", "If the email address is linked to a valid account, a reset password link has been sent.", message);
				bizO.Email = "testuser@cargowise.com";
				message = bizO.Retrieve(page.AppInstance);
				AssertEquals("Non web access account info should be emailed", 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
				var deactivatedContactEmail = EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Non web access account info should be emailed", 1, deactivatedContactEmail.Recipients.Count);
				AssertEquals("testuser@cargowise.com", deactivatedContactEmail.Recipients[0]);
				AssertContains("Password Reset Failed", deactivatedContactEmail.Subject);
				AssertContains("This is a deactivated account. Kindly raise a CR9 to request account activation.", deactivatedContactEmail.Body);
				AssertEquals("User context should not be set", Guid.Empty, Env.CurrentUserPK);
				AssertEquals("User context should not be set", Guid.Empty, Env.CurrentBranchPK);
				AssertEquals("User context should not be set", Guid.Empty, Env.CurrentDepartmentPK);
				EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals("Should show generic information that doesn't allow mining of usernames", "If the email address is linked to a valid account, a reset password link has been sent.", message);
				contact.OC_WebAccessEnabled = true;
				Factory.Save();
				AssertEquals("Precondition", 0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
				message = bizO.Retrieve(page.AppInstance);
				AssertEquals("Inactive account info should be emailed", 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
				deactivatedContactEmail = EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Inactive account info should be emailed", 1, deactivatedContactEmail.Recipients.Count);
				AssertEquals("testuser@cargowise.com", deactivatedContactEmail.Recipients[0]);
				AssertContains("Password Reset Failed", deactivatedContactEmail.Subject);
				AssertContains("This is a deactivated account. Kindly raise a CR9 to request account activation.", deactivatedContactEmail.Body);
				AssertEquals("Should show generic information that doesn't allow mining of usernames", "If the email address is linked to a valid account, a reset password link has been sent.", message);
				AssertEquals("User context should not be set", Guid.Empty, Env.CurrentUserPK);
				AssertEquals("User context should not be set", Guid.Empty, Env.CurrentBranchPK);
				AssertEquals("User context should not be set", Guid.Empty, Env.CurrentDepartmentPK);
				EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
				contact.OC_IsActive = true;
				Factory.Save();
				AssertEquals("Precondition", 0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
				message = bizO.Retrieve(page.AppInstance);
				AssertEquals("Should show generic information that doesn't allow mining of usernames", "If the email address is linked to a valid account, a reset password link has been sent.", message);
				AssertEquals("Login details should be emailed", 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("Login details should be emailed", 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
				AssertEquals("testuser@cargowise.com", EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Recipients[0]);
				AssertContains("Test User", EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Body);
				AssertContains("https://myaccount-portal.cargowise.com/myaccount/Admin/", EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Body);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
			}
		}

		public void TestRetrieve_HasPersonPassword()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_IsActive = true;
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_Email = "testuserAAA@cargowise.com";
			contact1.OC_ContactName = "User A";
			contact1.OC_PER = person1.PK;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_IsActive = true;
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_Email = "testuserBBB@cargowise.com";
			contact2.OC_ContactName = "User B";
			contact2.OC_PER = person2.PK;
			contact2.Person.SetHashedPassword("password");
			var otherContact = Factory.NewWithValidTestData<OrgContact>();
			otherContact.OC_IsActive = true;
			otherContact.OC_WebAccessEnabled = true;
			otherContact.OC_Email = "other@email.com";
			otherContact.OC_PER = person1.PK;
			Factory.Save();
			AssertEquals("Precondition", false, ((IPasswordInstructionEmailSource)contact1).ShouldSendMasterPassword);
			AssertEquals("Precondition", true, ((IPasswordInstructionEmailSource)contact2).ShouldSendMasterPassword);
			var bizO = new LoginRetrieverBizO(Factory)
			{ Email = contact1.OC_Email };
			using (var page = TestPage)
			{
				Env.ClearUserContext();
				EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
				var message = bizO.Retrieve(page.AppInstance);
				AssertEquals("Precondition: Should show generic information that doesn't allow mining of usernames", "If the email address is linked to a valid account, a reset password link has been sent.", message);
				bizO.Email = contact2.OC_Email;
				message = bizO.Retrieve(page.AppInstance);
				AssertEquals("Precondition: Should show generic information that doesn't allow mining of usernames", "If the email address is linked to a valid account, a reset password link has been sent.", message);
				AssertEquals("Precondition: Should have sent an email for each contact", 2, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
				var contact1Email = EnvProxy.Instance.OutgoingMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(contact1.OC_Email));
				var contact2Email = EnvProxy.Instance.OutgoingMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(contact2.OC_Email));
				AssertContains("Should send to normal password reset since master password has never been set", page.AppInstance.ResetPasswordPage, contact1Email.Body);
				AssertContains("Should send to normal password reset since master password has never been set", page.AppInstance.ResetPasswordKey, contact1Email.Body);
				AssertContains("Should send to master password page since it has been set before", page.AppInstance.ResetMasterPasswordPage, contact2Email.Body);
				AssertContains("Should send to master password page since it has been set before", page.AppInstance.ResetPasswordKey, contact2Email.Body);
				Assert("Should not have log since it doesn't necessarily send to a single contact", !contact1.GetLogs().HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.WebAccessPasswordEmailSentCode));
				Assert("Should not have log since it doesn't necessarily send to a single contact", !contact2.GetLogs().HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.WebAccessPasswordEmailSentCode));
				Assert("Should not have log since it doesn't necessarily send to a single contact", !contact2.Person.GetLogs().HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.WebAccessPasswordEmailSentCode));
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
			}
		}

		public void TestRetrieve_ShouldSendPersonPassword()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_IsActive = true;
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_Email = "testuserAAA@cargowise.com";
			contact1.OC_ContactName = "User A";
			Factory.Save();
			AssertEquals("Precondition", true, ((IPasswordInstructionEmailSource)contact1).ShouldSendMasterPassword);
			var bizO = new LoginRetrieverBizO(Factory)
			{ Email = contact1.OC_Email };
			using (var page = TestPage)
			{
				Env.ClearUserContext();
				EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
				AssertEquals("Precondition: Should be no emails", 0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
				var message = bizO.Retrieve(page.AppInstance);
				AssertEquals("Precondition: Should show generic information that doesn't allow mining of usernames", "If the email address is linked to a valid account, a reset password link has been sent.", message);
				AssertEquals("Precondition: Should have sent an email for the contact", 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
				var contact1Email = EnvProxy.Instance.OutgoingMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(contact1.OC_Email));
				AssertContains("Should send to master password reset as per ShouldSendMasterPassword", page.AppInstance.ResetMasterPasswordPage, contact1Email.Body);
				AssertContains("Should send to master password reset as per ShouldSendMasterPassword", page.AppInstance.ResetPasswordKey, contact1Email.Body);
				Assert("Should not have log since it doesn't necessarily send to a single contact", !contact1.GetLogs().HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.WebAccessPasswordEmailSentCode));
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
			}
		}

		public void TestRetrieve_CompanySpecificEmailTemplate()
		{
			WebDataRegistry.Instance.PasswordResetEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "nuts", EmailBody = "bolts" });
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			var contact = org.Contacts.AddNew();
			contact.FillWithValidTestData();
			contact.OC_Email = "testuser@cargowise.com";
			contact.OC_ContactName = "Test User";
			contact.OC_WebAccessEnabled = true;
			contact.OC_IsActive = true;
			Factory.Save();
			var bizO = new LoginRetrieverBizO(Factory);
			bizO.Email = "testuser@cargowise.com";
			WebDataRegistry.Instance.PasswordResetEmailTemplate.SetValue(contact.CompanyPKForLogin, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "Subject For Company", EmailBody = "Body For Company" });
			AssertEquals("Precondition", 0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			using (var page = TestPage)
			{
				AssertEquals("Should be retrieved", "If the email address is linked to a valid account, a reset password link has been sent.", bizO.Retrieve(page.AppInstance));
			}

			AssertEquals("Login details should be emailed", 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("Subject For Company", EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertContains("Body For Company", EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Body);
		}

		public void TestShouldNotThrowExceptionOnFactoryLoad()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "Mehhhh";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact 1";
			contact.OC_Email = "newuser@cargowise.com";
			contact.OC_WebAccessEnabled = true;
			Factory.Save();
			BusinessObjectFactory backgroundThreadFactory = null;
			var step = 1;
			var thread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					backgroundThreadFactory = new BusinessObjectFactory();
				}

				step = 2;
			});
			thread.Start();
			SleepUntil(ref step, 2);
			var bizO = new LoginRetrieverBizO(backgroundThreadFactory);
			bizO.Email = contact.OC_Email;
			AssertEquals("Should not report CrossThreadAccessException", null, ErrorReporter.LastExceptionReported);
			ErrorReporter.Clear();
			thread.Join();
		}

		static void SleepUntil(ref int step, int stepCondition)
		{
			for (var i = 0; ((i < 500) && step != stepCondition); ++i)
			{
				Thread.Sleep(15);
			}

			AssertEquals(stepCondition, step);
		}

		#region Implementation

		PageForTest TestPage
		{
			get
			{
				if (fTestPage == null)
				{
					fTestPage = new PageForTest();
					var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(HttpContext) }, null);
					method.Invoke(fTestPage, new object[] { HttpContext.Current });
				}

				return fTestPage;
			}
		}

		PageForTest fTestPage;
		class PageForTest : BasePage
		{
			protected override Uri RequestUrl => new Uri("http://www.test.com/MyAccount/Login/ForgotPassword.aspx");
			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			class GlobalForTest : Global
			{
				public void OnCustomSessionStart()
				{
					base.OnCustomSessionStart(this, EventArgs.Empty);
				}
			}
		}
		#endregion
	}
}
