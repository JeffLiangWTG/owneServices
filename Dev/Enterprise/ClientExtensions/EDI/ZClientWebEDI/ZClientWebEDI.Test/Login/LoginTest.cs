using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class LoginTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPageLoad()
		{
			var page1 = GetPageForTest();
			page1.DoPageLoad();
		}

		public void TestPageLoad_MessageData()
		{
			var page = GetPageForTest();
			var secureQueryString = new SecureQueryString { { "message", "Test Message" } };
			page.Request.QueryString.Add("data", secureQueryString.ToString());
			page.DoPageLoad();
			AssertEquals("Test Message", page.MessageExposed.Text);
		}

		public void TestSigninBtn_ClickSpecialLogin()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ASAAVS";
			Factory.Save();
			WebDataRegistry.Instance.WebServiceUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "hello");
			WebDataRegistry.Instance.WebServicePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "there");
			using (var page1 = GetPageForTest())
			{
				page1.DoPageLoad();
				page1.LoginManExposed.CompanyCode = org.OH_Code;
				page1.LoginManExposed.UserName = WebDataRegistry.Instance.WebServiceUsername.Value;
				page1.LoginManExposed.Password = WebDataRegistry.Instance.WebServicePassword.Value;
				page1.SigninBtnClick();
				AssertEquals(true, page1.SiteUser.IsLoggedIn);
				AssertContains("Should be redirected to default url", "/webapp/Default.aspx", page1.Response.RedirectLocation);
			}
		}

		void SetupDomainAndOrganisationRegistry(string[] domains, Guid[] organisations)
		{
			EDIDataRegistry.Instance.RedirectedEmailDomains.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, domains);
			EDIDataRegistry.Instance.RedirectedOrganisations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, organisations);
		}

		public void TestRedirectToLoginV2Page_WhenOIDCReady()
		{
			SetupDomainAndOrganisationRegistry(Array.Empty<string>(), Array.Empty<Guid>());
			Assert("OIDC should not be ready", !OIDCLoginHelper.IsOIDCReady());

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			SetupDomainAndOrganisationRegistry(new string[] { "@test.com" }, new Guid[] { header.PK.ToGuid() });
			Assert("OIDC should be ready", OIDCLoginHelper.IsOIDCReady());
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ASAAVS";
			Factory.Save();
			WebDataRegistry.Instance.WebServiceUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "hello");
			WebDataRegistry.Instance.WebServicePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "there");
			using (var page1 = GetPageForTest())
			{
				page1.RunPage_PreInit();
				AssertContains("Should be redirected to LoginV2 url", "/webapp/Login/LoginV2.aspx", page1.Response.RedirectLocation);
			}
		}

		public void TestSigninBtn_ClickSpecialLoginShouldSetCompanyCodeForRememberMe()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ASAAVS";
			Factory.Save();
			WebDataRegistry.Instance.WebServiceUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "hello");
			WebDataRegistry.Instance.WebServicePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "there");
			using (var page1 = GetPageForTest())
			{
				page1.DoPageLoad();
				page1.LoginManExposed.CompanyCode = org.OH_Code;
				page1.LoginManExposed.UserName = WebDataRegistry.Instance.WebServiceUsername.Value;
				page1.LoginManExposed.Password = WebDataRegistry.Instance.WebServicePassword.Value;
				page1.LoginManExposed.RememberMe = true;
				page1.SigninBtnClick();
				AssertEquals(true, page1.SiteUser.IsLoggedIn);
				AssertContains("Should be redirected to default url", "/webapp/Default.aspx", page1.Response.RedirectLocation);
				var rememberMeCookie = page1.AppInstance.ApplicationCookie;
				AssertEquals("Should save company code to cookie", org.OH_Code, rememberMeCookie.GetCompanyCode());
				AssertEquals("Should save user email to cookie", WebDataRegistry.Instance.WebServiceUsername.Value, rememberMeCookie.GetUserEmail());
				AssertEquals("Should not save password to cookie", string.Empty, rememberMeCookie.GetUserPassword());
			}
		}

		public void TestSigninBtn_ClickSingleNormalLoginShouldRedirectThroughLoginRouter()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ASAAVS";
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_Email = "fly@guy.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			contact.Person.PER_EmailAddress = "white@dye.com";
			Factory.Save();
			using (var page1 = GetPageForTest())
			{
				page1.DoPageLoad();
				page1.LoginManExposed.UserName = contact.OC_Email;
				page1.LoginManExposed.Password = "1234";
				page1.SigninBtnClick();
				AssertEquals(false, page1.SiteUser.IsLoggedIn);
				AssertStartsWith("Should be redirected via LoginRouter", "/webapp/Login/LoginComplete.aspx", page1.Response.RedirectLocation);
				Assert("Should write login hash cookie", page1.Response.Cookies.Keys.OfType<string>().Any(key => key.StartsWith("DeviceCookie_")));
			}
		}

		public void TestSigninBtn_LoginSuccessWithNoKey_ShouldNotWriteLoginHashCookie()
		{
			ClearKey(WebDataRegistry.Instance.LoginFailureAttemptSecretKey.Name);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ASAAVS";
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_Email = "fly@guy.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			contact.Person.PER_EmailAddress = "white@dye.com";
			Factory.Save();
			using (var page1 = GetPageForTest())
			{
				page1.DoPageLoad();
				page1.LoginManExposed.UserName = contact.OC_Email;
				page1.LoginManExposed.Password = "1234";
				page1.SigninBtnClick();
				Assert("Should not write login hash cookie when LoginFailureAttemptSecretKey is not set", !page1.Response.Cookies.Keys.OfType<string>().Any(key => key.StartsWith("DeviceCookie_")));
			}

			ErrorReporter.Clear();
			void ClearKey(string name)
			{
				using (var command = Db.Connection.Command("UPDATE dbo.StmData SET SD_BinaryValue = NULL WHERE SD_Name = @Name"))
				{
					command.AddParameter("@Name", SqlDbType.VarChar, name);
					command.ExecuteNonQuery();
				}

				WebDataRegistry.Instance.RemoveItemFromCacheIfOlderThan(name, TimeSpan.FromSeconds(-1));
			}
		}

		public void TestSigninBtn_ClickSingleNormalLoginShouldSetCompanyCodeForRememberMe()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ASAAVS";
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_Email = "fly@guy.com";
			contact.OC_WebAccessEnabled = true;
			var password = "ChangeMe123!";
			contact.SetHashedPassword(password);
			Factory.Save();
			contact.Person.PER_EmailAddress = "white@dye.com";
			Factory.Save();
			using (var page1 = GetPageForTest())
			{
				page1.DoPageLoad();
				page1.LoginManExposed.UserName = contact.OC_Email;
				page1.LoginManExposed.Password = password;
				page1.LoginManExposed.RememberMe = true;
				page1.SigninBtnClick();
				AssertEquals(false, page1.SiteUser.IsLoggedIn);
				AssertStartsWith("Should be redirected via LoginRouter", "/webapp/Login/LoginComplete.aspx", page1.Response.RedirectLocation);
				var rememberMeCookie = page1.AppInstance.ApplicationCookie;
				AssertEquals("Should save company code to cookie", org.OH_Code, rememberMeCookie.GetCompanyCode());
				AssertEquals("Should save user email to cookie", contact.OC_Email, rememberMeCookie.GetUserEmail());
				AssertEquals("Should not save password to cookie", string.Empty, rememberMeCookie.GetUserPassword());
			}
		}

		public void TestSigninBtn_ClickMultiMatchLoginShouldRedirectToChooseCompany()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TRAINS";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "BUSES";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "CABS";
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			org4.OH_Code = "BIKES";
			const string commonEmail = "fly@guy.com";
			const string password = "ChangeMe123!";
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_OH = org1.PK;
			contact1.OC_Email = commonEmail;
			contact1.OC_WebAccessEnabled = true;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_OH = org2.PK;
			contact2.OC_Email = commonEmail;
			contact2.OC_WebAccessEnabled = true;
			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_OH = org3.PK;
			contact3.OC_Email = commonEmail;
			contact3.OC_WebAccessEnabled = true;
			contact3.SetHashedPassword(password);
			var contactWithDifferentPassword = Factory.NewWithValidTestData<OrgContact>();
			contactWithDifferentPassword.OC_OH = org4.PK;
			contactWithDifferentPassword.OC_Email = commonEmail;
			contactWithDifferentPassword.OC_WebAccessEnabled = true;
			contactWithDifferentPassword.SetHashedPassword("otherPassword");
			var contactWithDifferentEmail = Factory.NewWithValidTestData<OrgContact>();
			contactWithDifferentEmail.OC_OH = org4.PK;
			contactWithDifferentEmail.OC_Email = "different@email.com";
			contactWithDifferentEmail.OC_WebAccessEnabled = true;
			contactWithDifferentEmail.SetHashedPassword(password);
			Factory.Save();
			contact2.OC_PER = contact1.OC_PER;
			contact1.Person.PER_EmailAddress = "white@dye.com";
			contact1.Person.SetHashedPassword(password);
			Factory.Save();
			using (var page1 = GetPageForTest())
			{
				page1.DoPageLoad();
				page1.LoginManExposed.UserName = commonEmail;
				page1.LoginManExposed.Password = password;
				page1.LoginManExposed.RememberMe = true;
				page1.SigninBtnClick();
				AssertEquals(false, page1.SiteUser.IsLoggedIn);
				AssertContains("Should be redirected to Choose Company", "ChooseCompany.aspx", page1.Response.RedirectLocation);
				var query = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.LoginRouterMultiContactIdentity);
				query.AddToFilter(StmAccessTokenSchema.SAT_ParentId, new[] { contact1.PK, contact2.PK, contact3.PK });
				query.AddToFilter(StmAccessTokenSchema.SAT_ParentTableCode, OrgContactSchema.Constants.Prefix);
				query.AddToFilter(StmAccessTokenSchema.SAT_RemainingUseCount, 1);
				var routingToken = Factory.LoadTop1<StmAccessToken>(query);
				AssertNotNull(routingToken);
				AssertStartsWith("Should start with RememberMe value", FormattableString.Invariant($"{page1.LoginManExposed.RememberMe}:"), routingToken.SAT_Scope);
				AssertContains("Should contain PKs of relevant contacts", contact1.PK.ToString(), routingToken.SAT_Scope);
				AssertContains("Should contain PKs of relevant contacts", contact2.PK.ToString(), routingToken.SAT_Scope);
				AssertContains("Should contain PKs of relevant contacts", contact3.PK.ToString(), routingToken.SAT_Scope);
				AssertNotContains("Should not contain PKs of contact with different password", contactWithDifferentPassword.PK.ToString(), routingToken.SAT_Scope);
				AssertNotContains("Should not contain PKs of contact with different email", contactWithDifferentEmail.PK.ToString(), routingToken.SAT_Scope);
			}
		}

		public void TestSigninBtn_ClickTooManyContactsShouldSendAmbiguousContactsEmail()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TRAINS";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "BUSES";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "CABS";
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			org4.OH_Code = "BIKES";
			var org5 = Factory.NewWithValidTestData<OrgHeader>();
			org5.OH_Code = "BOATS";
			var org6 = Factory.NewWithValidTestData<OrgHeader>();
			org6.OH_Code = "SKATES";
			const string commonEmail = "fly@guy.com";
			const string password = "ChangeMe123!";
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_OH = org1.PK;
			contact1.OC_Email = commonEmail;
			contact1.OC_WebAccessEnabled = true;
			contact1.SetHashedPassword(password);
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_OH = org2.PK;
			contact2.OC_Email = commonEmail;
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword(password);
			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_OH = org3.PK;
			contact3.OC_Email = commonEmail;
			contact3.OC_WebAccessEnabled = true;
			contact3.SetHashedPassword(password);
			var contactWithDifferentPassword = Factory.NewWithValidTestData<OrgContact>();
			contactWithDifferentPassword.OC_OH = org4.PK;
			contactWithDifferentPassword.OC_Email = commonEmail;
			contactWithDifferentPassword.OC_WebAccessEnabled = true;
			contactWithDifferentPassword.SetHashedPassword("otherPassword");
			var contact5 = Factory.NewWithValidTestData<OrgContact>();
			contact5.OC_OH = org5.PK;
			contact5.OC_Email = commonEmail;
			contact5.OC_WebAccessEnabled = true;
			contact5.SetHashedPassword(password);
			var contact6 = Factory.NewWithValidTestData<OrgContact>();
			contact6.OC_OH = org6.PK;
			contact6.OC_Email = commonEmail;
			contact6.OC_WebAccessEnabled = true;
			contact6.SetHashedPassword(password);
			Factory.Save();
			using (var page1 = GetPageForTest())
			{
				page1.DoPageLoad();
				page1.LoginManExposed.UserName = commonEmail;
				page1.LoginManExposed.Password = password;
				page1.LoginManExposed.RememberMe = true;
				AssertEquals("Precondition", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				page1.SigninBtnClick();
				AssertEquals(false, page1.SiteUser.IsLoggedIn);
				AssertEquals("Should not redirect", null, page1.Response.RedirectLocation);
				AssertEquals("Login Failed", page1.MessageExposed.Text);
				var emails = Env.OutgoingMailManager.EmailsCreated;
				AssertEquals("Should send email notifying contact ambiguity", 1, emails.Count);
				var email = emails[0];
				Assert("Should include all relevant companies", email.Body.Contains(org1.OH_Code));
				Assert("Should include all relevant companies", email.Body.Contains(org2.OH_Code));
				Assert("Should include all relevant companies", email.Body.Contains(org3.OH_Code));
				Assert("Should include all relevant companies", email.Body.Contains(org4.OH_Code));
				Assert("Should include all relevant companies", email.Body.Contains(org5.OH_Code));
				Assert("Should include all relevant companies", email.Body.Contains(org6.OH_Code));
			}
		}

		public void TestOnInitComplete_LoggedInUser()
		{
			(TestContact.Person as EDIGlbPerson).StorePersonalEmailPromptSkip();
			TestContact.Factory.Save();
			using (var page1 = GetPageForTest())
			{
				((MyAccountLoginHelperForTest)page1.LoginHelperExposed).SetIsAuthenticated(true);
				page1.AppInstance.SiteUser.Login(TestOrg.OH_Code, TestContact.OC_Email, TestContactPassword);
				page1.OnInitCompleteExposed();
				Assert("Precondition", page1.SiteUser.IsLoggedIn);
				Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
				AssertContains("/webapp/Default.aspx", HttpContext.Current.Response.RedirectLocation);
			}
		}

		public void TestSigninBtnClick_AccountLocked()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			TestContact.OC_PER = person.PK;
			var loginFailureLog = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLog.SFL_IsLockOut = true;
			loginFailureLog.SFL_LoginName = TestContact.OC_Email + " " + TestOrg.OH_Code;
			loginFailureLog.SFL_TableCode = OrgContactSchema.Constants.Prefix;
			loginFailureLog.SFL_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			WebDataRegistry.Instance.WebLoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			using (var page1 = GetPageForTest())
			{
				page1.DoPageLoad();
				page1.LoginManExposed.CompanyCode = TestOrg.OH_Code;
				page1.LoginManExposed.UserName = TestContact.OC_Email;
				page1.LoginManExposed.Password = TestContactPassword;
				AssertNotNull("SiteUser", page1.SiteUser);
				page1.SigninBtnClick();
				AssertNotNull("SiteUser", page1.SiteUser);
				Assert(!page1.SiteUser.IsLoggedIn);
				AssertEquals("Account Locked", page1.MessageExposed.Text);
			}
		}

		public void TestLoadShouldPopulateDataSourceFieldsFromTextBox()
		{
			using (var page1 = GetPageForTest())
			{
				page1.DoPageLoad();
				page1.LoginManExposed.CompanyCode = TestOrg.OH_Code;
				page1.LoginManExposed.UserName = TestContact.OC_Email;
				page1.LoginNameTextBoxExposed.BindTo = "UserName";
				page1.LoginNameTextBoxExposed.Bind(page1.LoginManExposed);
				page1.LoginManExposed.Password = TestContactPassword;
				page1.PasswordTextBoxExposed.BindTo = "Password";
				page1.PasswordTextBoxExposed.Bind(page1.LoginManExposed);
				page1.LoginManExposed.RememberMe = true;
				page1.RememberMeCheckBoxExposed.BindTo = "RememberMe";
				page1.RememberMeCheckBoxExposed.Bind(page1.LoginManExposed);
				AssertEquals("Precondition: Company code should match assigned value", TestOrg.OH_Code, page1.LoginManExposed.CompanyCode);
				AssertEquals("Precondition: Login name should match assigned value", TestContact.OC_Email, page1.LoginManExposed.UserName);
				AssertEquals("Precondition: TextBox should match bound value", page1.LoginManExposed.UserName, page1.LoginNameTextBoxExposed.Text);
				AssertEquals("Precondition: Password should match assigned value", TestContactPassword, page1.LoginManExposed.Password);
				AssertEquals("Precondition: TextBox should match bound value", page1.LoginManExposed.Password, page1.PasswordTextBoxExposed.Text);
				AssertEquals("Precondition: Remember Me should match assigned value", true, page1.LoginManExposed.RememberMe);
				AssertEquals("Precondition: CheckBox should match bound value", page1.LoginManExposed.RememberMe, page1.RememberMeCheckBoxExposed.Checked);
				page1.ResetDataSource();
				page1.DoPageLoad();
				AssertEquals("Should populate from TextBox", TestContact.OC_Email, page1.LoginManExposed.UserName);
				AssertEquals("Should populate from TextBox", TestContactPassword, page1.LoginManExposed.Password);
				AssertEquals("Should populate from CheckBox", true, page1.LoginManExposed.RememberMe);
			}
		}

		public void TestShouldPresetUserNameAndCompanyAndHashFromSessionOnLoad()
		{
			using (var page = GetPageForTest())
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Code = "TechDeckz";
				orgHeader.OH_FullName = "Tech Decks";
				var userName = "tech@deck.com";
				page.DoPageLoad();
				page.LoginManExposed.CompanyCode = orgHeader.OH_Code;
				page.LoginManExposed.UserName = userName;
				page.SaveViewStateExposed();
				// Cookies for login hash and nonce
				WebApplicationLoginHelper.WriteLoginHashToCookie(orgHeader.OH_Code, userName);
				HttpContext.Current.Request.QueryString.Remove(MyAccountLoginHelper.RefKey);
				HttpContext.Current.Request.QueryString.Add(MyAccountLoginHelper.RefKey, page.DataSourceIndexer.ToString());
				page.LoginHelperExposed.SetParamsValueForTest("ClearSaved", "1");
				page.AppInstance.ApplicationCookie.WriteCookie("meh");
				AssertEquals("Precondition", true, page.AppInstance.ApplicationCookie.CookieExist());
				page.ResetDataSource();
				page.DoPageLoad();
				AssertEquals("Values should be populated from session", orgHeader.OH_Code, page.LoginManExposed.CompanyCode);
				AssertEquals("Values should be populated from session", userName, page.LoginManExposed.UserName);
				AssertEquals("Values should be populated from cookie", 32, page.LoginManExposed.LoginHash.Length);
				AssertEquals("Cookie should be cleared due to ClearSaved parameter", false, page.AppInstance.ApplicationCookie.CookieExist());
			}
		}

		public void TestShouldClearCookieIfPresetUserNameAndCompanyFromSessionOnLoad()
		{
			using (var page = GetPageForTest())
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Code = "TechDeckz";
				orgHeader.OH_FullName = "Tech Decks";
				var userName = "tech@deck.com";
				var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
				otherOrg.OH_Code = "Herbert";
				otherOrg.OH_FullName = "Hurt and Bert";
				var cookieContact = otherOrg.Contacts.AddNew();
				cookieContact.OC_Email = "hurt@bert.com";
				var password = "ChangeMe123!";
				cookieContact.SetHashedPassword(password);
				cookieContact.OC_WebAccessEnabled = true;
				page.DoPageLoad();
				page.LoginManExposed.CompanyCode = orgHeader.OH_Code;
				page.LoginManExposed.UserName = userName;
				page.SaveViewStateExposed();
				HttpContext.Current.Request.QueryString.Remove(MyAccountLoginHelper.RefKey);
				HttpContext.Current.Request.QueryString.Add(MyAccountLoginHelper.RefKey, page.DataSourceIndexer.ToString());
				page.LoginHelperExposed.SetParamsValueForTest("ClearSaved", "1");
				page.AppInstance.ApplicationCookie.WriteUser(otherOrg.OH_Code, cookieContact.OC_Email, password);
				AssertEquals("Precondition", true, page.AppInstance.ApplicationCookie.CookieExist());
				page.ResetDataSource();
				page.DoPageLoad();
				AssertEquals("Values should be populated from session not cookie", orgHeader.OH_Code, page.LoginManExposed.CompanyCode);
				AssertEquals("Values should be populated from session not cookie", userName, page.LoginManExposed.UserName);
				AssertEquals("Values should be populated from session not cookie", string.Empty, page.LoginManExposed.Password);
				AssertEquals("Cookie should be cleared due to ClearSaved parameter", false, page.AppInstance.ApplicationCookie.CookieExist());
			}
		}

		public void TestSigninBtnClick_IncorrectCredentials()
		{
			using (var page1 = GetPageForTest())
			{
				page1.DoPageLoad();
				page1.LoginManExposed.CompanyCode = "BADORG";
				page1.LoginManExposed.UserName = "wrong@email.com";
				page1.LoginManExposed.Password = "badPasSwoRd123!";
				AssertNotNull("SiteUser", page1.SiteUser);
				page1.SigninBtnClick();
				AssertEquals("Should show invalid login message", "Credentials do not match", page1.MessageExposed.Text);
			}
		}

		public void TestPageLoadShouldSetupOnClickJavascriptEvent()
		{
			using (var page1 = GetPageForTest())
			{
				page1.DoPageLoad();
				AssertEquals("Javascript event handler should be set up", "showCompanyCode();", page1.ShowCompanyCodeLabelDivExposed.Attributes["onClick"]);
			}
		}

		public void TestPageLoadShouldShowCompanyCodeIfParameterSet()
		{
			using (var page1 = GetPageForTest())
			{
				page1.DoPageLoad();
				page1.LoginManExposed.CompanyCode = TestOrg.OH_Code;
				page1.DoPageLoad();
				AssertEquals("Show company code text should be hidden", "none", page1.ShowCompanyCodeLabelDivExposed.Style["display"]);
				AssertEquals("Company code div should be visible", "block", page1.CompanyCodeDivExposed.Style["display"]);
			}
		}

		public void TestTabIndex()
		{
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var str = resourceRetriever.GetString(@"Login.aspx", System.Text.Encoding.UTF8);
			var lines = str.SplitByLine();
			void assertTabIndex(string id, string tabIndex)
			{
				AssertEquals($"{id}-{tabIndex}", true, lines.Any(x => x.Contains($" id=\"{id}\" ") && x.Contains($" TabIndex=\"{tabIndex}\"")));
			}

			assertTabIndex("LoginNameTextBox", "1");
			assertTabIndex("PasswordTextBox", "2");
			assertTabIndex("CompanyCodeTextBox", "3");
			assertTabIndex("SigninBtn", "4");
			assertTabIndex("RememberMeCheckBox", "5");
			AssertEquals(true, lines.Any(x => x.Contains("<a href=\"RetrieveLogin.aspx\" TabIndex=\"-1\">")));
		}

		EDIOrgHeader TestOrg;
		EDIOrgContact TestContact;
		EdiCustomerUserAccount TestUserAccount;
		const string TestContactPassword = "123";
		protected override void SetUp()
		{
			base.SetUp();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			WebDataRegistry.Instance.LoginFailureAttemptSecretKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "8F909455805C9A77DFD61579E86B54880FBBC194CE943F08B006B5A5488FBA1EF805BF4374BED6BF653FA096AF5BE06694459160390312A0C5B69AC2019E8DA5");
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

		LoginForTest GetPageForTest()
		{
			var page = new LoginForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			return page;
		}

		class LoginForTest : Login
		{
			public LoginForTest()
			{
				CompanyCodeTextBox = new ZTextBox();
				LoginNameTextBox = new ZTextBox();
				PasswordTextBox = new ZTextBox();
				RememberMeCheckBox = new ZCheckBox();
				ShowCompanyCodeLabelDiv = new HtmlGenericControl();
				CompanyCodeDiv = new HtmlGenericControl();
				SigninBtn = new Button();
				Message = new ZTextLabel();
			}

			public void RunPage_PreInit()
			{
				base.Page_PreInit(null, EventArgs.Empty);
			}

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

			class GlobalForTest : Global
			{
				public void OnCustomSessionStart()
				{
					base.OnCustomSessionStart(this, EventArgs.Empty);
				}
			}

			public Label MessageExposed => Message;
			public ZTextBox CompanyCodeTextBoxExposed => CompanyCodeTextBox;
			public ZTextBox LoginNameTextBoxExposed => LoginNameTextBox;
			public ZTextBox PasswordTextBoxExposed => PasswordTextBox;
			public ZCheckBox RememberMeCheckBoxExposed => RememberMeCheckBox;
			public Button SignInBtnExposed => SigninBtn;
			public HtmlGenericControl ShowCompanyCodeLabelDivExposed => ShowCompanyCodeLabelDiv;
			public HtmlGenericControl CompanyCodeDivExposed => CompanyCodeDiv;
			protected override MyAccountLoginHelper GetNewLoginHelper()
			{
				return new MyAccountLoginHelperForTest(this);
			}

			public MyAccountLoginHelper LoginHelperExposed => LoginHelper;
			public void SigninBtnClick() => SigninBtn_Click(null, EventArgs.Empty);
			public LoginManager LoginManExposed => LoginMan;
			public void OnInitCompleteExposed() => OnInitComplete(EventArgs.Empty);
			public void SaveViewStateExposed()
			{
				SaveViewState();
			}
		}

		class MyAccountLoginHelperForTest : MyAccountLoginHelper
		{
			public MyAccountLoginHelperForTest(BasePage page) : base(page, false)
			{
			}

			protected override bool IsAuthenticated => isAuthenticated;
			bool isAuthenticated;
			public void SetIsAuthenticated(bool shouldBeAuthenticated)
			{
				isAuthenticated = shouldBeAuthenticated;
			}
		}
	}
}
