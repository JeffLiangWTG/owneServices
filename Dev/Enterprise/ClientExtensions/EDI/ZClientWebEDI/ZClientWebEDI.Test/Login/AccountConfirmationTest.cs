using System;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class AccountConfirmationTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPageLoad()
		{
			var page1 = GetPageForTest();
			page1.DoPageLoad();
		}

		public void TestInvalidQueryString()
		{
			var page1 = GetPageForTest();
			page1.DoPageLoad();
			AssertEquals("Precondition", false, page1.IdentityManager_Exposed.IsValidID());
			AssertEquals("The link which directed you to this page was invalid. Try logging in again and contact your system administrator if this issue persists.", page1.ErrorMessage_Exposed.Text);
			AssertEquals(false, page1.AccountConfirmationBox_Exposed.Visible);
		}

		public void TestOnInitComplete_UnlinkedUserAccount()
		{
			TestUserAccount.EUA_IsContactRelationshipActive = false;
			TestUserAccount.EUA_ContactRelationshipStatus = "EMC";
			Factory.Save();
			AssertEquals(TestUserAccount, TestContact.GetMostRecentUnlinkedUserAccount());
			(TestContact.Person as EDIGlbPerson).StorePersonalEmailPromptSkip();
			TestContact.Factory.Save();
			using (var page1 = GetPageForTest())
			{
				var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = LoginRouterIdentityManager.GenerateToken(TestContact), };
				page1.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
				page1.DoPageLoad();
				Assert(!HttpContext.Current.Response.IsRequestBeingRedirected);
				AssertEquals(true, page1.ActionMessage1_EMC_Exposed.Visible);
				AssertEquals(true, page1.ActionMessage2_EMC_Exposed.Visible);
				page1.ConfirmationYesClick();
				AssertEquals(true, TestUserAccount.EUA_IsContactRelationshipActive);
				AssertEquals(string.Empty, TestUserAccount.EUA_ContactRelationshipStatus);
				Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
				AssertStartsWith("Should redirect to LoginComplete", "/webapp/Login/LoginComplete.aspx", HttpContext.Current.Response.RedirectLocation);
			}
		}

		public void TestConfirmationNoClick()
		{
			TestUserAccount.EUA_IsContactRelationshipActive = false;
			TestUserAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.EmailChanged;
			Factory.Save();
			AssertEquals(TestUserAccount, TestContact.GetMostRecentUnlinkedUserAccount());
			(TestContact.Person as EDIGlbPerson).StorePersonalEmailPromptSkip();
			TestContact.Factory.Save();
			using (var page1 = GetPageForTest())
			{
				var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = LoginRouterIdentityManager.GenerateToken(TestContact), };
				page1.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
				page1.DoPageLoad();
				Assert(!HttpContext.Current.Response.IsRequestBeingRedirected);
				AssertEquals("Precondition", true, page1.ActionMessage1_EMC_Exposed.Visible);
				AssertEquals("Precondition", true, page1.ActionMessage2_EMC_Exposed.Visible);
				page1.ConfirmationNoClick();
				AssertEquals("Should remain inactive", false, TestUserAccount.EUA_IsContactRelationshipActive);
				AssertEquals("Should remain unchanged", ContactRelationshipStatusList.Codes.EmailChanged, TestUserAccount.EUA_ContactRelationshipStatus);
				Assert("Should have been disconnected", TestUserAccount.EUA_OC_WebAccessContact.IsEmpty);
				Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
				AssertStartsWith("Should redirect to LoginComplete", "/webapp/Login/LoginComplete.aspx", HttpContext.Current.Response.RedirectLocation);
			}
		}

		EDIOrgHeader TestOrg;
		EDIOrgContact TestContact;
		EdiCustomerUserAccount TestUserAccount;
		const string TestContactPassword = "123";
		protected override void SetUp()
		{
			base.SetUp();
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			TestOrg = (EDIOrgHeader)database.WebAccessOrg;
			TestContact = (EDIOrgContact)TestOrg.Contacts.AddNew();
			TestContact.OC_ContactName = "User 1";
			TestContact.OC_Email = "u1@cw1.com";
			TestContact.OC_WebAccessEnabled = true;
			TestContact.SetHashedPassword(TestContactPassword);
			TestUserAccount = Factory.New<EdiCustomerUserAccount>();
			TestUserAccount.EUA_LD = database.PK;
			TestUserAccount.EUA_UserID = "U01";
			TestUserAccount.EUA_FullName = "User 1";
			TestUserAccount.EUA_Email = "u1@cw1.com";
			TestUserAccount.EUA_OC_WebAccessContact = TestContact.PK;
			Factory.Save();
			TestContact.Person.PER_EmailAddress = TestContact.OC_Email;
			Factory.Save();
		}

		AccountConfirmationForTest GetPageForTest()
		{
			var page = new AccountConfirmationForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			page.InitialiseControls();
			return page;
		}

		class AccountConfirmationForTest : AccountConfirmation
		{
			public void DoPageLoad()
			{
				try
				{
					base.OnLoad(EventArgs.Empty);
					OnInitComplete(EventArgs.Empty);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (ex is QueryStringException)
					{
						throw;
					}
				}
			}

			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			public void InitialiseControls()
			{
				ActionMessage1_ACR = new ZTextLabel();
				ActionMessage1_EMC = new ZTextLabel();
				ActionMessage1_MUL = new ZTextLabel();
				ActionMessage2_ACR = new ZTextLabel();
				ActionMessage2_EMC = new ZTextLabel();
				ActionMessage2_MUL = new ZTextLabel();
				AccountConfirmationBox = new HtmlGenericControl();
				ErrorMessage = new ZTextLabel();
			}

			class GlobalForTest : Global
			{
				public void OnCustomSessionStart()
				{
					base.OnCustomSessionStart(this, EventArgs.Empty);
				}
			}

			public void ConfirmationYesClick() => ConfirmationYes_Click(null, null);
			public void ConfirmationNoClick() => ConfirmationNo_Click(null, null);
			public Label ActionMessage1_ACR_Exposed => ActionMessage1_ACR;
			public Label ActionMessage1_EMC_Exposed => ActionMessage1_EMC;
			public Label ActionMessage1_MUL_Exposed => ActionMessage1_MUL;
			public Label ActionMessage2_ACR_Exposed => ActionMessage2_ACR;
			public Label ActionMessage2_EMC_Exposed => ActionMessage2_EMC;
			public Label ActionMessage2_MUL_Exposed => ActionMessage2_MUL;
			public HtmlGenericControl AccountConfirmationBox_Exposed => AccountConfirmationBox;
			public Label ErrorMessage_Exposed => ErrorMessage;
			public MyAccountLoginRouterIdentityManager IdentityManager_Exposed => IdentityManager;
		}
	}
}
