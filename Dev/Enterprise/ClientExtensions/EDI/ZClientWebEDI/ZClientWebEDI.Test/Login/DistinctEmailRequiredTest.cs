using System;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class DistinctEmailRequiredTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPageLoad_InvalidRequest()
		{
			var page1 = GetPageForTest();
			page1.DoPageLoad();
			AssertEquals("The link which directed you to this page was invalid. Try logging in again and contact your system administrator if this issue persists.", page1.InstructionLabelExposed.Text);
			AssertEquals(false, page1.EmailTextboxExposed.Visible);
		}

		public void TestPageFooter()
		{
			var footer = @"<b>IMPORTANT:</b> Please read this before taking the exam.
*Do not press the back button while taking exams.*";
			EDIDataRegistry.Instance.DistinctEmailRequiredPageFooterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, footer);
			var page = GetPageForTest();
			page.DoPageLoad();
			AssertEquals(@"<b>IMPORTANT:</b> Please read this before taking the exam.<br>*Do not press the back button while taking exams.*", page.DistinctEmailRequiredMessage_Expose.InnerHtml);
		}

		public void TestChangeEmail_DuplicatesShouldShowAccountVerification()
		{
			var page = SetupForAccountVerificationTest();
			AssertEquals("Precondition", false, page.LoginOptionsDivExposed.Visible);
			AssertEquals("Should not automatically redirect immediately after verification", false, page.AccountVerificationExposed.ShouldRedirectAfterVerification);
			page.EmailTextboxExposed.Text = "nonunique@email.address";
			page.RegisterEmail_Exposed();
			AssertEquals("Account verification should be shown for duplicate email", true, page.LoginOptionsDivExposed.Visible);
			AssertEquals("Text should explain that duplicate email is not unique.", "Work Email is not unique, please enter another email.", page.MessageLabelExposed.Text);
		}

		public void TestChangeEmail_DuplicatesWithExistingUserAccountInSameDatabaseShouldNotShowAccountVerification()
		{
			var page = SetupForAccountVerificationTest();
			var contact2UserAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			contact2UserAccount.EUA_OC_WebAccessContact = Contact2.PK;
			contact2UserAccount.EUA_LD = UserAccount.EUA_LD;
			contact2UserAccount.EUA_UserID = "US2";
			contact2UserAccount.EUA_FullName = "name";
			contact2UserAccount.EUA_Email = "nonunique@email.address";
			Factory.Save();
			AssertEquals("Precondition", false, page.LoginOptionsDivExposed.Visible);
			AssertEquals("Should not automatically redirect immediately after verification", false, page.AccountVerificationExposed.ShouldRedirectAfterVerification);
			page.EmailTextboxExposed.Text = "nonunique@email.address";
			page.RegisterEmail_Exposed();
			AssertEquals("Account verification should not be shown since the duplicate contact already has a user account in the same db", false, page.LoginOptionsDivExposed.Visible);
			AssertEquals("Text should explain that duplicate email is not unique.", "Work Email is not unique, please enter another email.", page.MessageLabelExposed.Text);
		}

		public void TestChangeEmail_DuplicatesWithExistingUserAccountInDifferentDatabaseShouldNotShowAccountVerification()
		{
			var page = SetupForAccountVerificationTest();
			var contact2UserAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			contact2UserAccount.EUA_OC_WebAccessContact = Contact2.PK;
			contact2UserAccount.EUA_LD = Factory.NewWithValidTestData<LicenceDatabase>().PK;
			contact2UserAccount.EUA_UserID = "US2";
			contact2UserAccount.EUA_FullName = "name";
			contact2UserAccount.EUA_Email = "nonunique@email.address";
			var unrelatedUserAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			unrelatedUserAccount.EUA_LD = UserAccount.EUA_LD;
			unrelatedUserAccount.EUA_UserID = "US2";
			unrelatedUserAccount.EUA_FullName = "name";
			unrelatedUserAccount.EUA_Email = "nonunique@email.address";
			Factory.Save();
			AssertEquals("Precondition", false, page.LoginOptionsDivExposed.Visible);
			AssertEquals("Should not automatically redirect immediately after verification", false, page.AccountVerificationExposed.ShouldRedirectAfterVerification);
			page.EmailTextboxExposed.Text = "nonunique@email.address";
			page.RegisterEmail_Exposed();
			AssertEquals("Account verification should be shown for duplicate email", true, page.LoginOptionsDivExposed.Visible);
			AssertEquals("Text should explain that duplicate email is not unique.", "Work Email is not unique, please enter another email.", page.MessageLabelExposed.Text);
		}

		public void TestShouldMergeContactsOnAccountVerification()
		{
			var page = SetupForAccountVerificationTest();
			AssertEquals("Precondition", false, page.LoginOptionsDivExposed.Visible);
			page.EmailTextboxExposed.Text = "nonunique@email.address";
			page.RegisterEmail_Exposed();
			AssertEquals("Account verification should be shown for duplicate email", true, page.LoginOptionsDivExposed.Visible);
			page.AccountVerification_AccountVerificationCompletedExposed();
			Contact1.Reload();
			UserAccount.Reload();
			Assert("Should deactivate dud contact", !Contact1.OC_IsActive);
			Assert("Should remove web access from dud contact", !Contact1.OC_WebAccessEnabled);
			Assert("Should remove email from dud contact", Contact1.OC_Email.IsEmpty);
			AssertEquals("Should be moved to the contact that already had the correct email", Contact2.PK, UserAccount.EUA_OC_WebAccessContact);
			AssertEquals("Persons should be merged", Contact2.OC_PER, Contact1.OC_PER);
			AssertEquals("Your web access has been extended to this account.", page.MessageLabelExposed.Text);
			AssertContains("UpdateContactInformation.aspx", page.Response.RedirectLocation);
		}

		public void TestChangeEmail_NoUserAccountFound()
		{
			var page = (DistinctEmailRequiredForNRETest)SetupForAccountVerificationTest(isNRETest: true);

			page.EmailTextboxExposed.Text = "nonunique@email.address";
			page.isUserAccountNull = true;
			AssertNoExceptionThrown("Should not throw NRE", () => page.RegisterEmail_Exposed());
			AssertEquals("Text should explain that the page has expired and the user needs to login again.", "The page has expired. Please login again.", page.MessageLabelExposed.Text);
		}

		OrgContact Contact1;
		OrgContact Contact2;
		EdiCustomerUserAccount UserAccount;
		DistinctEmailRequiredForTest SetupForAccountVerificationTest(bool isNRETest = false)
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "~f~";
			var oldContact = org.Contacts.AddNew();
			oldContact.OC_ContactName = "name 1";
			oldContact.OC_Email = "some@email.address";
			oldContact.OC_WebAccessEnabled = true;
			Contact1 = org.Contacts.AddNew();
			Contact1.OC_ContactName = "name 1 (1)";
			Contact1.OC_Email = "some@email.address";
			Contact1.OC_WebAccessEnabled = false;
			Contact2 = org.Contacts.AddNew();
			Contact2.OC_ContactName = "name 2";
			Contact2.OC_Email = "nonunique@email.address";
			Contact2.OC_WebAccessEnabled = true;
			Contact2.SetHashedPassword("1234");
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			database.LD_LicenceType = DatabaseTypes.Codes.Test;
			database.LD_OH_WebAccessOrg = org.PK;
			UserAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			UserAccount.EUA_OC_WebAccessContact = Contact1.PK;
			UserAccount.EUA_IsContactRelationshipActive = false;
			UserAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.DistinctEmailRequired;
			UserAccount.EUA_LD = database.PK;
			UserAccount.EUA_UserID = "US1";
			UserAccount.EUA_FullName = "name";
			UserAccount.EUA_Email = "some@email.address";
			Factory.Save();
			var token = MyAccountLoginRouterIdentityManager.GenerateToken(UserAccount);
			var secureQueryString = new SecureQueryString { { LoginRouter.OriginalUrlQueryStringKey, "http://wwww.cw1.com/autologin.aspx?u=1&pwd=2" }, { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			var page = isNRETest ? GetPageForNRETest() : GetPageForTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			page.DoPageLoad();
			return page;
		}

		public void TestChangeEmail_NoDuplicates()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "~f~";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "name 1";
			contact1.OC_Email = "some@email.address";
			contact1.OC_WebAccessEnabled = true;
			contact1.SetHashedPassword("1234");
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "name 2";
			contact2.OC_Email = "nonunique@email.address";
			contact2.OC_WebAccessEnabled = true;
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			database.LD_LicenceType = DatabaseTypes.Codes.Test;
			database.LD_OH_WebAccessOrg = Guid.Empty;
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount.EUA_OC_WebAccessContact = contact1.PK;
			userAccount.EUA_IsContactRelationshipActive = false;
			userAccount.EUA_LD = database.PK;
			userAccount.EUA_UserID = "US1";
			userAccount.EUA_FullName = "name";
			userAccount.EUA_Email = "some@email.address";
			Factory.Save();
			var token = MyAccountLoginRouterIdentityManager.GenerateToken(userAccount);
			var secureQueryString = new SecureQueryString { { LoginRouter.OriginalUrlQueryStringKey, "http://wwww.cw1.com/autologin.aspx?u=1&pwd=2" }, { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			var page = GetPageForTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			page.SiteUser.LoginForTest(contact1.OrgCode, contact1.Email, "1234");
			var myaccountSiteUser = page.SiteUser as MyAccountWebUser;
			myaccountSiteUser.LoggedInUserAccountPK = userAccount.PK;
			page.DoPageLoad();
			page.EmailTextboxExposed.Text = "unique@email.address";
			page.RegisterEmail_Exposed();
			AssertEquals("Text should reflect email being sent", "An email was sent to unique@email.address to verify your work email address. Please verify the email to complete the update.", page.MessageLabelExposed.Text);
		}

		DistinctEmailRequiredForTest GetPageForTest()
		{
			var page = new DistinctEmailRequiredForTest();
			MethodInfo method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			page.AppInstance.SiteUser.Login("MEHMEH", "newuser@cargowise.com", CWSupportLoginToken.TokenForTest);
			page.InitialiseControls();
			return page;
		}

		class DistinctEmailRequiredForTest : DistinctEmailRequired
		{
			public void DoPageLoad()
			{
				try
				{
					base.OnLoad(EventArgs.Empty);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (ex is QueryStringException)
					{
						throw;
					}
				}
			}

			public void RegisterEmail_Exposed()
			{
				base.RegisterEmail();
			}

			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			public void InitialiseControls()
			{
				MessageLabel = new ZTextLabel();
				InstructionLabel = new ZTextLabel();
				EmailTextbox = new ZTextBox();
				DistinctEmailRequiredMessage = new HtmlGenericControl();
				AccountVerification = new AccountVerificationControlForTest();
				AccountVerification.Page = this;
				LoginOptionsDiv = new HtmlGenericControl();
				LoginOptionsDiv.Visible = false;
			}

			public Label InstructionLabelExposed => InstructionLabel;
			public Label MessageLabelExposed => MessageLabel;
			public ZTextBox EmailTextboxExposed => EmailTextbox;
			public HtmlGenericControl LoginOptionsDivExposed => LoginOptionsDiv;
			public AccountVerificationControl AccountVerificationExposed => AccountVerification;
			public void AccountVerification_AccountVerificationCompletedExposed() => AccountVerification_AccountVerificationCompleted(null, EventArgs.Empty);
			public HtmlGenericControl DistinctEmailRequiredMessage_Expose => DistinctEmailRequiredMessage;
			class GlobalForTest : Global
			{
				public void OnCustomSessionStart()
				{
					base.OnCustomSessionStart(this, EventArgs.Empty);
				}
			}
		}

		DistinctEmailRequiredForNRETest GetPageForNRETest()
		{
			var page = new DistinctEmailRequiredForNRETest();
			MethodInfo method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			page.AppInstance.SiteUser.Login("MEHMEH", "newuser@cargowise.com", CWSupportLoginToken.TokenForTest);
			page.InitialiseControls();
			return page;
		}

		class DistinctEmailRequiredForNRETest : DistinctEmailRequiredForTest
		{
			protected override MyAccountLoginRouterIdentityManager IdentityManager => isUserAccountNull ? new MyAccountLoginRouterIdentityManager() : base.IdentityManager;

			public bool isUserAccountNull { get; set; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
		}
	}
}
