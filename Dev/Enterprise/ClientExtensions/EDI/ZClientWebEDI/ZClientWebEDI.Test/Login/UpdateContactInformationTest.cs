using System;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Security.AntiXss;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.ErrorReporting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	class UpdateContactInformationTest : ZPageLifeCycleTest
	{
		protected override ZPage GetNewPage()
		{
			return UpdateContactInformationPage;
		}

		public void TestPageLoad_PopulatePersonalInfo()
		{
			EDIDataRegistry.Instance.RegisterPersonalEmailPageFooterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "blah \r\nblah");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "<svg/onload=alert(document.domain)>";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();

			var personAsEDI = newContact.Person as EDIGlbPerson;
			var token = LoginRouterIdentityManager.GenerateToken(newContact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());

			UpdateContactInformationPage.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			UpdateContactInformationPage.DoPageLoad();

			AssertNotEquals("Your personal recovery email has already been setup. To change it, go to your MyAccount Profile Menu => Contact Information.", UpdateContactInformationPage.ErrorMessage_Expose.Text);
			AssertEquals(string.Empty, UpdateContactInformationPage.PersonalEmailTextBox_Expose.Text);
			AssertEquals(EDIDataRegistry.Instance.RegisterPersonalEmailPageFooterText.Value.Replace("\r\n", "<br>"), UpdateContactInformationPage.RegisterPersonalEmailMessage_Expose.InnerHtml);
			AssertEquals("Precondition", false, personAsEDI.ShouldSkipPersonalEmailPrompt());
			AssertEquals(false, UpdateContactInformationPage.DoNotAskMeAgainCheckBox_Expose.Checked);
			AssertEquals("display:none", UpdateContactInformationPage.ContinueButton_Expose.Attributes["style"]);
			AssertEquals(FormattableString.Invariant($"Hi {AntiXssEncoder.HtmlEncode(newContact.OC_ContactName, false)},"), UpdateContactInformationPage.GreetingLabel_Expose.Text);

			newContact.Person.PER_EmailAddress = "newuser@gmail.com";
			Factory.Save();
			newContact.Person.Reload();
			UpdateContactInformationPage.Request.QueryString.Remove(LoginRouter.QueryStringKey);

			ResetLabelsBoxesAndButtonsToInline(UpdateContactInformationPage);
			UpdateContactInformationPage.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			UpdateContactInformationPage.DoPageLoad();

			AssertEquals(AntiXssEncoder.HtmlEncode("Your personal recovery email has already been setup. To change it, go to your MyAccount Profile Menu => Contact Information.",false), UpdateContactInformationPage.ErrorMessage_Expose.Text);
			AssertEquals(false, UpdateContactInformationPage.ContactInformationBox_Expose.Visible);
			AssertEquals("display:inline", UpdateContactInformationPage.ContinueButton_Expose.Attributes["style"]);
		}

		public void TestSavePersonalEmail()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			var token = LoginRouterIdentityManager.GenerateToken(newContact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			UpdateContactInformationPage.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			UpdateContactInformationPage.DoPageLoad();
			AssertEquals("Precondition", "display:none", UpdateContactInformationPage.ContinueButton_Expose.Attributes["style"]);
			AssertEquals("display:inline", UpdateContactInformationPage.SaveEmailButton_Expose.Attributes["style"]);
			AssertNotEquals("Your personal account has an error. Please contact your system administrator if this persists.", UpdateContactInformationPage.UpdateContactInformationInstructionsLabel_Expose.Text);
			AssertEquals("", UpdateContactInformationPage.MessageLabel_Expose.Text);
			UpdateContactInformationPage.SaveEmailButton_OnClick();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("display:none", UpdateContactInformationPage.ContinueButton_Expose.Attributes["style"]);
			AssertEquals("display:inline", UpdateContactInformationPage.SaveEmailButton_Expose.Attributes["style"]);
			AssertNotEquals("Your personal account has an error. Please contact your system administrator if this persists.", UpdateContactInformationPage.UpdateContactInformationInstructionsLabel_Expose.Text);
			AssertEquals("The personal email address cannot be empty.", UpdateContactInformationPage.MessageLabel_Expose.Text);
			UpdateContactInformationPage.PersonalEmailTextBox_Expose.Text = "newuser";
			UpdateContactInformationPage.SaveEmailButton_OnClick();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("display:none", UpdateContactInformationPage.ContinueButton_Expose.Attributes["style"]);
			AssertEquals("display:inline", UpdateContactInformationPage.SaveEmailButton_Expose.Attributes["style"]);
			AssertNotEquals("Your personal account has an error. Please contact your system administrator if this persists.", UpdateContactInformationPage.UpdateContactInformationInstructionsLabel_Expose.Text);
			AssertEquals("The personal email address is invalid.", UpdateContactInformationPage.MessageLabel_Expose.Text);
			UpdateContactInformationPage.PersonalEmailTextBox_Expose.Text = "newuser@gmail.com";
			UpdateContactInformationPage.SaveEmailButton_OnClick();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("/Admin/RegisterPersonalEmail.aspx?RegisterKey=", email.Body);
			AssertEquals("Personal Email Confirmation", email.Subject);
			AssertEquals("newuser@gmail.com", email.Recipients[0].Email);
			AssertEquals("display:inline", UpdateContactInformationPage.ContinueButton_Expose.Attributes["style"]);
			AssertEquals(false, UpdateContactInformationPage.SaveEmailButton_Expose.Visible);
			AssertEquals("Account Verification", UpdateContactInformationPage.UpdateContactInformationHeadingLabel_Expose.Text);
			AssertEquals(AntiXssEncoder.HtmlEncode("An email was sent to 'newuser@gmail.com' to verify your personal recovery email. Please verify the email to complete the update.", false), UpdateContactInformationPage.UpdateContactInformationInstructionsLabel_Expose.Text);
			AssertEquals("", UpdateContactInformationPage.MessageLabel_Expose.Text);
		}

		public void TestSavePersonalEmailRedirect()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			const string originalUrl = "https://google.com.au/";
			var token = LoginRouterIdentityManager.GenerateToken(newContact);
			var secureQueryString = new SecureQueryString { { LoginRouter.OriginalUrlQueryStringKey, originalUrl }, { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			UpdateContactInformationPage.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			UpdateContactInformationPage.DoPageLoad();
			AssertEquals("Precondition", "display:none", UpdateContactInformationPage.ContinueButton_Expose.Attributes["style"]);
			UpdateContactInformationPage.PersonalEmailTextBox_Expose.Text = "newuser@gmail.com";
			UpdateContactInformationPage.SaveEmailButton_OnClick();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("/Admin/RegisterPersonalEmail.aspx?RegisterKey=", email.Body);
			AssertEquals("Personal Email Confirmation", email.Subject);
			AssertEquals("newuser@gmail.com", email.Recipients[0].Email);
			AssertEquals("display:inline", UpdateContactInformationPage.ContinueButton_Expose.Attributes["style"]);
			UpdateContactInformationPage.ContinueButton_OnClick();
			var redirectUrl = new Uri(UpdateContactInformationPage.Response.RedirectLocation, UriKind.Relative);
			var uriDeconstructor = new UriDeconstructor(redirectUrl);
			AssertStartsWith("Should redirect via Login Router", "/webapp/Login/LoginComplete.aspx", UpdateContactInformationPage.Response.RedirectLocation);
			var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var secureQueryStringDecoded = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			var originalUrlFromQuery = secureQueryStringDecoded[LoginRouter.OriginalUrlQueryStringKey];
			AssertEquals(originalUrl, originalUrlFromQuery);
		}

		[TestDate(2020, 01, 01)]
		public void TestDoNotAskAboutPersonalEmail()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			var personAsEDI = newContact.Person as EDIGlbPerson;
			var token = LoginRouterIdentityManager.GenerateToken(newContact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			UpdateContactInformationPage.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			UpdateContactInformationPage.DoPageLoad();
			UpdateContactInformationPage.DoNotAskMeAgainCheckBox_Expose.Checked = true;
			UpdateContactInformationPage.SkipButton_OnClick();
			AssertEquals(true, personAsEDI.ShouldSkipPersonalEmailPrompt());
			TestDateAttribute.AddDays(7);
			AssertEquals(true, personAsEDI.ShouldSkipPersonalEmailPrompt());
			TestDateAttribute.AddDays(1);
			AssertEquals(true, personAsEDI.ShouldSkipPersonalEmailPrompt());
		}

		[TestDate(2020, 01, 01)]
		public void TestSkipRedirect()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			var personAsEDI = newContact.Person as EDIGlbPerson;
			const string originalUrl = "https://google.com.au/";
			var token = LoginRouterIdentityManager.GenerateToken(newContact);
			var secureQueryString = new SecureQueryString { { LoginRouter.OriginalUrlQueryStringKey, originalUrl }, { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			UpdateContactInformationPage.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			UpdateContactInformationPage.DoPageLoad();
			AssertEquals("Precondition", false, UpdateContactInformationPage.DoNotAskMeAgainCheckBox_Expose.Checked);
			UpdateContactInformationPage.SkipButton_OnClick();
			var redirectUrl = new Uri(UpdateContactInformationPage.Response.RedirectLocation, UriKind.Relative);
			var uriDeconstructor = new UriDeconstructor(redirectUrl);
			AssertStartsWith("Should redirect via Login Router", "/webapp/Login/LoginComplete.aspx", UpdateContactInformationPage.Response.RedirectLocation);
			var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var secureQueryStringDecoded = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			var originalUrlFromQuery = secureQueryStringDecoded[LoginRouter.OriginalUrlQueryStringKey];
			AssertEquals(originalUrl, originalUrlFromQuery);
			AssertEquals(true, personAsEDI.ShouldSkipPersonalEmailPrompt());
			TestDateAttribute.AddDays(7);
			AssertEquals(true, personAsEDI.ShouldSkipPersonalEmailPrompt());
			TestDateAttribute.AddDays(1);
			AssertEquals(false, personAsEDI.ShouldSkipPersonalEmailPrompt());
		}

		[TestDate(2020, 01, 01)]
		public void TestSkip()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			var personAsEDI = newContact.Person as EDIGlbPerson;
			var token = LoginRouterIdentityManager.GenerateToken(newContact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			UpdateContactInformationPage.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			UpdateContactInformationPage.DoPageLoad();
			AssertEquals("Precondition", false, UpdateContactInformationPage.DoNotAskMeAgainCheckBox_Expose.Checked);
			UpdateContactInformationPage.SkipButton_OnClick();
			AssertEquals(true, personAsEDI.ShouldSkipPersonalEmailPrompt());
			TestDateAttribute.AddDays(7);
			AssertEquals(true, personAsEDI.ShouldSkipPersonalEmailPrompt());
			TestDateAttribute.AddDays(1);
			AssertEquals(false, personAsEDI.ShouldSkipPersonalEmailPrompt());
		}

		public void TestSkip_PageExpired()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			var token = LoginRouterIdentityManager.GenerateToken(newContact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			var page = GetPageForExpiryTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			page.DoPageLoad();
			page.IdentityManagerForExpiryTest_Expose.ResetContact();
			page.SkipButton_OnClick();
			AssertEquals("Should hide info box", false, page.ContactInformationBox_Expose.Visible);
			AssertEquals(BasePage.PageExpiredMessage, page.ErrorMessage_Expose.Text);
		}

		public void TestInvalidQueryString()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, "dud" } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			UpdateContactInformationPage.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			UpdateContactInformationPage.DoPageLoad();
			AssertEquals("Precondition", false, UpdateContactInformationPage.IdentityManager_Expose.IsValidID());
			AssertEquals("The link which directed you to this page was invalid. Try logging in again and contact your system administrator if this issue persists.", UpdateContactInformationPage.ErrorMessage_Expose.Text);
			AssertEquals(false, UpdateContactInformationPage.ContactInformationBox_Expose.Visible);
			AssertEquals("display:inline", UpdateContactInformationPage.ContinueButton_Expose.Attributes["style"]);
		}

		public void TestContinue()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			newContact.Person.PER_EmailAddress = "slex@blah.com";
			Factory.Save();
			var token = LoginRouterIdentityManager.GenerateToken(newContact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			UpdateContactInformationPage.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			UpdateContactInformationPage.DoPageLoad();
			UpdateContactInformationPage.ContinueButton_OnClick();
			var redirectUrl = new Uri(UpdateContactInformationPage.Response.RedirectLocation, UriKind.Relative);
			var uriDeconstructor = new UriDeconstructor(redirectUrl);
			AssertStartsWith("Should redirect via Login Router", "/webapp/Login/LoginComplete.aspx", UpdateContactInformationPage.Response.RedirectLocation);
			var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var secureQueryStringDecoded = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			var originalUrlFromQuery = secureQueryStringDecoded[LoginRouter.OriginalUrlQueryStringKey];
			AssertEquals(null, originalUrlFromQuery);
		}

		public void TestContinueRedirect()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			newContact.Person.PER_EmailAddress = "slex@blah.com";
			Factory.Save();
			const string originalUrl = "https://google.com.au/";
			var token = LoginRouterIdentityManager.GenerateToken(newContact);
			var secureQueryString = new SecureQueryString { { LoginRouter.OriginalUrlQueryStringKey, originalUrl }, { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			UpdateContactInformationPage.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			UpdateContactInformationPage.DoPageLoad();
			UpdateContactInformationPage.ContinueButton_OnClick();
			var redirectUrl = new Uri(UpdateContactInformationPage.Response.RedirectLocation, UriKind.Relative);
			var uriDeconstructor = new UriDeconstructor(redirectUrl);
			AssertStartsWith("Should redirect via Login Router", "/webapp/Login/LoginComplete.aspx", UpdateContactInformationPage.Response.RedirectLocation);
			var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var secureQueryStringDecoded = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			var originalUrlFromQuery = secureQueryStringDecoded[LoginRouter.OriginalUrlQueryStringKey];
			AssertEquals(originalUrl, originalUrlFromQuery);
		}

		public void TestSaveEmailShouldLoadContactFromNewFactoryIfNotFound()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			newContact.Person.PER_EmailAddress = "slex@blah.com";
			Factory.Save();
			const string originalUrl = "https://google.com.au/";
			var token = LoginRouterIdentityManager.GenerateToken(newContact);
			var secureQueryString = new SecureQueryString { { LoginRouter.OriginalUrlQueryStringKey, originalUrl }, { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			var updateContactInformationPage = GetPageForExceptionTest();
			updateContactInformationPage.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			updateContactInformationPage.SetIdentityManagerContactToNull();
			updateContactInformationPage.PersonalEmailTextBox_Expose.Text = "newuser@gmail.com";
			updateContactInformationPage.SaveEmailButton_OnClick();
			AssertEquals("Should reload contact from new factory and successfully send email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("Precondition", "/Admin/RegisterPersonalEmail.aspx?RegisterKey=", email.Body);
		}

		public void TestSaveEmailShouldReportErrorIfContactNotFound()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			newContact.Person.PER_EmailAddress = "slex@blah.com";
			Factory.Save();
			const string originalUrl = "https://google.com.au/";
			var token = LoginRouterIdentityManager.GenerateToken(newContact);
			var secureQueryString = new SecureQueryString { { LoginRouter.OriginalUrlQueryStringKey, originalUrl }, { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			var updateContactInformationPage = GetPageForExceptionTest(true);
			updateContactInformationPage.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			updateContactInformationPage.PersonalEmailTextBox_Expose.Text = "newuser@gmail.com";
			updateContactInformationPage.SaveEmailButton_OnClick();
			AssertEquals("Should fail to reload contact from new factory", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Error message should be set", "There was a problem loading your account. Please try to login again.", updateContactInformationPage.ErrorMessage_Expose.Text);
			AssertEquals(1, updateContactInformationPage.ErrorReportedCount);
		}

		public void TestSaveEmailShouldNotReportErrorIfTokenExpired()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			newContact.Person.PER_EmailAddress = "slex@blah.com";
			Factory.Save();
			var expiredToken = LoginRouterIdentityManager.GenerateToken(newContact);
			var tokenBizO = Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Token, expiredToken));
			tokenBizO.SAT_ExpiresAt = ZDateTime.UtcNow - TimeSpan.FromMinutes(15);
			Factory.Save();
			const string originalUrl = "https://google.com.au/";
			var secureQueryString = new SecureQueryString { { LoginRouter.OriginalUrlQueryStringKey, originalUrl }, { LoginRouter.IdentityTokenQueryStringKey, expiredToken } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			var errorReportQuery = new ZQuery(StmErrorReportSchema.QER_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcToday);
			var initialErrorReportCount = Factory.GetDatabaseCount(typeof(StmErrorReport), errorReportQuery);
			UpdateContactInformationPage.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			UpdateContactInformationPage.PersonalEmailTextBox_Expose.Text = "newuser@gmail.com";
			UpdateContactInformationPage.SaveEmailButton_OnClick();
			AssertEquals("Should fail to reload contact from new factory", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Session expired message should be set", BasePage.SessionExpiredMessage, UpdateContactInformationPage.ErrorMessage_Expose.Text);
			AssertEquals("Should not have any new error reports", initialErrorReportCount, Factory.GetDatabaseCount(typeof(StmErrorReport), errorReportQuery));
		}

		public void TestSaveEmailShouldReportErrorIfTokenDeleted()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			newContact.Person.PER_EmailAddress = "slex@blah.com";
			Factory.Save();
			var token = LoginRouterIdentityManager.GenerateToken(newContact);
			const string originalUrl = "https://google.com.au/";
			var secureQueryString = new SecureQueryString { { LoginRouter.OriginalUrlQueryStringKey, originalUrl }, { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			var errorReportQuery = new ZQuery(StmErrorReportSchema.QER_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcToday);
			var initialErrorReportCount = Factory.GetDatabaseCount(typeof(StmErrorReport), errorReportQuery);
			var updateContactInformationPage = GetPageForExceptionTest();
			updateContactInformationPage.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			var manager = updateContactInformationPage.IdentityManager_Expose;
			updateContactInformationPage.DoPageLoad();
			var tokenBizO = Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Token, token));
			var creationTime = tokenBizO.SAT_SystemCreateTimeUtc;
			var expiry = tokenBizO.SAT_ExpiresAt;
			tokenBizO.Delete();
			Factory.Save();
			updateContactInformationPage.SetIdentityManagerContactToNull();
			updateContactInformationPage.PersonalEmailTextBox_Expose.Text = "newuser@gmail.com";
			updateContactInformationPage.SaveEmailButton_OnClick();
			Assert("Message contains 'IdentityManager.Token value: '", updateContactInformationPage.LastReportedMessage.Contains("IdentityManager.Token value: "));
			AssertEquals($"IdentityManager.Token value: {manager.Token}, Token Parent ID: 00000000-0000-0000-0000-000000000000, Token Parent Code: , Created at: {creationTime.ToBestReadableDateTimeString()}, Expires at: {expiry.ToBestReadableDateTimeString()}", updateContactInformationPage.LastReportedMessage);
		}

		public void TestSaveEmailShouldNotReportErrorIfTokenDeletedAndSessionEnded()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			newContact.Person.PER_EmailAddress = "slex@blah.com";
			Factory.Save();
			var token = LoginRouterIdentityManager.GenerateToken(newContact);
			const string originalUrl = "https://google.com.au/";
			var secureQueryString = new SecureQueryString { { LoginRouter.OriginalUrlQueryStringKey, originalUrl }, { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			var errorReportQuery = new ZQuery(StmErrorReportSchema.QER_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcToday);
			var initialErrorReportCount = Factory.GetDatabaseCount(typeof(StmErrorReport), errorReportQuery);
			var updateContactInformationPage = GetPageForExceptionTest();
			updateContactInformationPage.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			var manager = updateContactInformationPage.IdentityManager_Expose;
			updateContactInformationPage.DoPageLoad();
			var tokenBizO = Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Token, token));
			var creationTime = tokenBizO.SAT_SystemCreateTimeUtc;
			var expiry = tokenBizO.SAT_ExpiresAt;
			tokenBizO.Delete();
			Factory.Save();
			updateContactInformationPage.SetIdentityManagerContactToNull();
			updateContactInformationPage.PersonalEmailTextBox_Expose.Text = "newuser@gmail.com";
			updateContactInformationPage.Session["TokenCreationTime"] = null;
			updateContactInformationPage.Session["TokenExpiryTime"] = null;
			updateContactInformationPage.SaveEmailButton_OnClick();
			AssertNull("Should not report error since token times are not stored, so session must have expired", updateContactInformationPage.LastReportedMessage);
		}

		public void TestSaveEmailShouldNotReportErrorIfQueryStringExpired()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			newContact.Person.PER_EmailAddress = "slex@blah.com";
			Factory.Save();
			const string originalUrl = "https://google.com.au/";
			var expiredToken = LoginRouterIdentityManager.GenerateToken(newContact);
			var tokenBizO = Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Token, expiredToken));
			tokenBizO.Delete();
			Factory.Save();
			var secureQueryString = new SecureQueryString { { LoginRouter.OriginalUrlQueryStringKey, originalUrl }, { LoginRouter.IdentityTokenQueryStringKey, expiredToken } };
			secureQueryString.ExpireTime = TimeSpan.FromMinutes(-5);
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			var errorReportQuery = new ZQuery(StmErrorReportSchema.QER_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcToday);
			var initialErrorReportCount = Factory.GetDatabaseCount(typeof(StmErrorReport), errorReportQuery);
			TestDateAttribute.AddMinutes(40);
			UpdateContactInformationPage.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			UpdateContactInformationPage.PersonalEmailTextBox_Expose.Text = "newuser@gmail.com";
			UpdateContactInformationPage.SaveEmailButton_OnClick();
			AssertEquals("Should fail to reload contact from new factory", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Session expired message should be set", BasePage.SessionExpiredMessage, UpdateContactInformationPage.ErrorMessage_Expose.Text);
			AssertEquals("Should not have any new error reports", initialErrorReportCount, Factory.GetDatabaseCount(typeof(StmErrorReport), errorReportQuery));
		}

		static UpdateContactInformationForTest GetPageForTest()
		{
			var page = new UpdateContactInformationForTest();
			page.GreetingLabel_Expose = new ZTextLabel();
			page.UpdateContactInformationHeadingLabel_Expose = new ZTextLabel();
			page.UpdateContactInformationInstructionsLabel_Expose = new ZTextLabel();
			page.PersonalEmailTextBox_Expose = new ZTextBox();
			page.RegisterPersonalEmailMessage_Expose = new HtmlGenericControl();
			page.SaveEmailButton_Expose = new ZButton();
			page.ContinueButton_Expose = new ZButton();
			page.SkipButton_Expose = new ZLinkButton();
			page.DoNotAskMeAgainCheckBox_Expose = new ZCheckBox();
			page.MessageLabel_Expose = new ZTextLabel();
			page.ContactInformationBox_Expose = new HtmlGenericControl();
			page.ErrorMessage_Expose = new ZTextLabel();
			ResetLabelsBoxesAndButtonsToInline(page);
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			return page;
		}

		static UpdateContactInformationForExceptionTest GetPageForExceptionTest(bool isDoubleExceptionTest = false)
		{
			var page = isDoubleExceptionTest ? new UpdateContactInformationForDoubleExceptionTest() : new UpdateContactInformationForExceptionTest();
			page.GreetingLabel_Expose = new ZTextLabel();
			page.UpdateContactInformationHeadingLabel_Expose = new ZTextLabel();
			page.UpdateContactInformationInstructionsLabel_Expose = new ZTextLabel();
			page.PersonalEmailTextBox_Expose = new ZTextBox();
			page.RegisterPersonalEmailMessage_Expose = new HtmlGenericControl();
			page.SaveEmailButton_Expose = new ZButton();
			page.ContinueButton_Expose = new ZButton();
			page.SkipButton_Expose = new ZLinkButton();
			page.DoNotAskMeAgainCheckBox_Expose = new ZCheckBox();
			page.MessageLabel_Expose = new ZTextLabel();
			page.ContactInformationBox_Expose = new HtmlGenericControl();
			page.ErrorMessage_Expose = new ZTextLabel();
			ResetLabelsBoxesAndButtonsToInline(page);
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			return page;
		}

		UpdateContactInformationForTest UpdateContactInformationPage => page ?? (page = GetPageForTest());
		UpdateContactInformationForTest page;
		static void ResetLabelsBoxesAndButtonsToInline(UpdateContactInformationForTest page)
		{
			page.UpdateContactInformationInstructionsLabel_Expose.Attributes["style"] = "display:inline";
			page.PersonalEmailTextBox_Expose.Attributes["style"] = "display:inline";
			page.RegisterPersonalEmailMessage_Expose.Attributes["style"] = "display:inline";
			page.SaveEmailButton_Expose.Attributes["style"] = "display:inline";
			page.ContinueButton_Expose.Attributes["style"] = "display:inline";
			page.SkipButton_Expose.Attributes["style"] = "display:inline";
			page.DoNotAskMeAgainCheckBox_Expose.Attributes["style"] = "display:inline";
		}

		class UpdateContactInformationForTest : UpdateContactInformation
		{
			public void DoPageLoad()
			{
				base.OnLoad(EventArgs.Empty);
			}

			public void SaveEmailButton_OnClick()
			{
				base.SaveEmailButton_OnClick(this, EventArgs.Empty);
			}

			public void ContinueButton_OnClick()
			{
				base.ContinueButton_OnClick(this, EventArgs.Empty);
			}

			public void SkipButton_OnClick()
			{
				base.SkipButton_OnClick(this, EventArgs.Empty);
			}

			public MyAccountLoginRouterIdentityManager IdentityManager_Expose => base.IdentityManager;
			public ZTextLabel UpdateContactInformationHeadingLabel_Expose { get => base.UpdateContactInformationHeadingLabel; set => base.UpdateContactInformationHeadingLabel = value; }

			public ZTextLabel GreetingLabel_Expose { get => base.GreetingLabel; set => base.GreetingLabel = value; }

			public ZTextLabel UpdateContactInformationInstructionsLabel_Expose { get => base.UpdateContactInformationInstructionsLabel; set => base.UpdateContactInformationInstructionsLabel = value; }

			public ZTextBox PersonalEmailTextBox_Expose { get => base.PersonalEmailTextBox; set => base.PersonalEmailTextBox = value; }

			public HtmlGenericControl RegisterPersonalEmailMessage_Expose { get => base.RegisterPersonalEmailMessage; set => base.RegisterPersonalEmailMessage = value; }

			public ZLinkButton SkipButton_Expose { get => base.SkipButton; set => base.SkipButton = value; }

			public Button SaveEmailButton_Expose { get => base.SaveEmailButton; set => base.SaveEmailButton = value; }

			public ZButton ContinueButton_Expose { get => base.ContinueButton; set => base.ContinueButton = value; }

			public ZCheckBox DoNotAskMeAgainCheckBox_Expose { get => base.DoNotAskMeAgainCheckBox; set => base.DoNotAskMeAgainCheckBox = value; }

			public ZTextLabel MessageLabel_Expose { get => base.MessageLabel; set => base.MessageLabel = value; }

			public ZTextLabel ErrorMessage_Expose { get => base.ErrorMessage; set => base.ErrorMessage = value; }

			public HtmlGenericControl ContactInformationBox_Expose { get => base.ContactInformationBox; set => base.ContactInformationBox = value; }

			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			public int ErrorReportedCount;
			public string LastReportedMessage;
			protected override void ReportErrorCore(string message)
			{
				ErrorReportedCount++;
				LastReportedMessage = message;
			}
		}

		class UpdateContactInformationForExceptionTest : UpdateContactInformationForTest
		{
			protected override MyAccountLoginRouterIdentityManager GetNewIdentityManager(BusinessObjectFactory factory)
			{
				return new MyAccountLoginRouterIdentityManagerForTest(factory);
			}

			public void SetIdentityManagerContactToNull()
			{
				((MyAccountLoginRouterIdentityManagerForTest)IdentityManager).SetContactToNull();
			}

			class MyAccountLoginRouterIdentityManagerForTest : MyAccountLoginRouterIdentityManager
			{
				public MyAccountLoginRouterIdentityManagerForTest(BusinessObjectFactory factory) : base(factory)
				{
				}

				public void SetContactToNull()
				{
					Contact = null;
				}
			}
		}

		class UpdateContactInformationForDoubleExceptionTest : UpdateContactInformationForExceptionTest
		{
			protected override MyAccountLoginRouterIdentityManager GetNewIdentityManager(BusinessObjectFactory factory)
			{
				return new MyAccountLoginRouterIdentityManagerForDoubleExceptionTest(factory);
			}

			class MyAccountLoginRouterIdentityManagerForDoubleExceptionTest : MyAccountLoginRouterIdentityManagerForTest
			{
				public MyAccountLoginRouterIdentityManagerForDoubleExceptionTest(BusinessObjectFactory factory) : base(factory)
				{
				}

				protected override void PopulatePropertiesFromTokenCore(AccessTokenInfo token)
				{
					Contact = null;
				}
			}
		}

		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
			}
		}

		static UpdateContactInformationForExpiryTest GetPageForExpiryTest()
		{
			var page = new UpdateContactInformationForExpiryTest();
			page.GreetingLabel_Expose = new ZTextLabel();
			page.UpdateContactInformationHeadingLabel_Expose = new ZTextLabel();
			page.UpdateContactInformationInstructionsLabel_Expose = new ZTextLabel();
			page.PersonalEmailTextBox_Expose = new ZTextBox();
			page.RegisterPersonalEmailMessage_Expose = new HtmlGenericControl();
			page.SaveEmailButton_Expose = new ZButton();
			page.ContinueButton_Expose = new ZButton();
			page.SkipButton_Expose = new ZLinkButton();
			page.DoNotAskMeAgainCheckBox_Expose = new ZCheckBox();
			page.MessageLabel_Expose = new ZTextLabel();
			page.ContactInformationBox_Expose = new HtmlGenericControl();
			page.ErrorMessage_Expose = new ZTextLabel();
			ResetLabelsBoxesAndButtonsToInline(page);
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			return page;
		}

		class UpdateContactInformationForExpiryTest : UpdateContactInformationForTest
		{
			protected override MyAccountLoginRouterIdentityManager IdentityManager
			{
				get
				{
					if (identityManager == null)
					{
						identityManager = new MyAccountLoginRouterIdentityManagerForTest(new BusinessObjectFactory());
						identityManager.PopulatePropertiesFromToken(Token);
					}

					return identityManager;
				}
			}

			MyAccountLoginRouterIdentityManagerForTest identityManager;
			public MyAccountLoginRouterIdentityManagerForTest IdentityManagerForExpiryTest_Expose => (MyAccountLoginRouterIdentityManagerForTest)IdentityManager;
		}

		class MyAccountLoginRouterIdentityManagerForTest : MyAccountLoginRouterIdentityManager
		{
			public MyAccountLoginRouterIdentityManagerForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public void ResetContact()
			{
				Contact = null;
			}
		}
	}
}
