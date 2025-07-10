using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class ChooseCompanyTest : ZPageLifeCycleTest
	{
		public void TestPageLoad()
		{
			using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
			{
				AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, "invalidToken");
				AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, "");
				AssertPageLoad(AssertOnLoadComplete_WithValidTokenRememberMeOn, TestTokenRememberMeOn);
				AssertPageLoad(AssertOnLoadComplete_WithValidTokenRememberMeOff, TestTokenRememberMeOff);
				AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, TestToken_Expired);
				AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, TestToken_InvalidType);
			}
		}

		void AssertPageLoad(EventHandler onLoadCompleteHandler, string token)
		{
			var passwordSecureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			HttpContext.Current.Request.QueryString.Remove(SecureQueryString.QueryStringKey);
			HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, WebUtility.UrlEncode(passwordSecureQueryString.ToString()));
			Env.ClearUserContext();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			AssertOnLoadComplete += onLoadCompleteHandler;
			try
			{
				RunPageLifeCycle();
			}
			finally
			{
				AssertOnLoadComplete -= onLoadCompleteHandler;
			}
		}

		void AssertOnLoadComplete_WithInvalidToken(object sender, EventArgs e)
		{
			var testPage = (ChooseCompanyForTest)TestPage;
			AssertEquals("Should be hidden on page load if token invalid", false, testPage.LoginContactsDivExposed.Visible);
			AssertEquals("Should be hidden on page load if token invalid", false, testPage.SignInButtonExposed.Visible);
			AssertEquals("Error message should be shown", true, testPage.MessageExposed.Visible);
			AssertEquals("The link is invalid. Please attempt login again and if this issue is recurring, raise an incident.", testPage.MessageExposed.Text);
			AssertEquals("User context should be unset", Guid.Empty, Env.CurrentUserPK);
			AssertEquals("User context should be unset", Guid.Empty, Env.CurrentBranchPK);
			AssertEquals("User context should be unset", Guid.Empty, Env.CurrentDepartmentPK);
		}

		void AssertOnLoadComplete_WithValidTokenRememberMeOn(object sender, EventArgs e)
		{
			var testPage = (ChooseCompanyForTest)TestPage;
			AssertEquals("Should be shown on page load if token is valid", true, testPage.LoginContactsDivExposed.Visible);
			AssertEquals("Should be shown on page load if token is valid", true, testPage.SignInButtonExposed.Visible);
			AssertEquals("Should not show any error message", false, testPage.MessageExposed.Visible);
			AssertNotEquals("Site user should not be null", null, testPage.SiteUser);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
			AssertEquals("Should be checked (loaded from token)", true, testPage.RememberMeExposed);
			AssertEquals("Should have 3 contacts loaded from token", 3, testPage.ChooseCompanyManagerExposed.LoginContacts.Count);
		}

		void AssertOnLoadComplete_WithValidTokenRememberMeOff(object sender, EventArgs e)
		{
			var testPage = (ChooseCompanyForTest)TestPage;
			AssertEquals("Should be shown on page load if token is valid", true, testPage.LoginContactsDivExposed.Visible);
			AssertEquals("Should be shown on page load if token is valid", true, testPage.SignInButtonExposed.Visible);
			AssertEquals("Should not show any error message", false, testPage.MessageExposed.Visible);
			AssertNotEquals("Site user should not be null", null, testPage.SiteUser);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
			AssertEquals("Should not be checked (loaded from token)", false, testPage.RememberMeExposed);
			AssertEquals("Should have 3 contacts loaded from token", 3, testPage.ChooseCompanyManagerExposed.LoginContacts.Count);
		}

		public void TestSigninBtn_ClickShouldRedirectThroughLoginRouter()
		{
			using (var page1 = GetPageForTest())
			{
				var passwordSecureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, TestTokenRememberMeOn } };
				HttpContext.Current.Request.QueryString.Remove(SecureQueryString.QueryStringKey);
				HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, WebUtility.UrlEncode(passwordSecureQueryString.ToString()));
				page1.DoPageLoad();
				AssertEquals("Precondition: Contacts repeater div should be visible", true, page1.LoginContactsDivExposed.Visible);
				var dataSourceForBinding = page1.ChooseCompanyManagerExposed.LoginContacts;
				page1.BindRepeater(dataSourceForBinding.Cast<OrgContact>().ToList());
				var repeater = page1.LoginContactRepeaterExposed;
				AssertEquals("Precondition: Should be an item for each contact in the scope", 3, repeater.Items.Count);
				var companyCode1 = ((ZTextLabel)repeater.Items[0].FindControl("CompanyCode")).Attributes["value"];
				var companyCode2 = ((ZTextLabel)repeater.Items[1].FindControl("CompanyCode")).Attributes["value"];
				ZRadioButton contact2CheckBox;
				ZRadioButton otherCheckBox1;
				ZRadioButton otherCheckBox2;
				if (TestOrg2.OH_Code.EqualsIgnoringCase(companyCode1))
				{
					contact2CheckBox = (ZRadioButton)repeater.Items[0].FindControl("Checked");
					otherCheckBox1 = (ZRadioButton)repeater.Items[1].FindControl("Checked");
					otherCheckBox2 = (ZRadioButton)repeater.Items[2].FindControl("Checked");
				}
				else if (TestOrg2.OH_Code.EqualsIgnoringCase(companyCode2))
				{
					contact2CheckBox = (ZRadioButton)repeater.Items[1].FindControl("Checked");
					otherCheckBox1 = (ZRadioButton)repeater.Items[0].FindControl("Checked");
					otherCheckBox2 = (ZRadioButton)repeater.Items[2].FindControl("Checked");
				}
				else
				{
					contact2CheckBox = (ZRadioButton)repeater.Items[2].FindControl("Checked");
					otherCheckBox1 = (ZRadioButton)repeater.Items[0].FindControl("Checked");
					otherCheckBox2 = (ZRadioButton)repeater.Items[1].FindControl("Checked");
				}

				contact2CheckBox.Checked = true;
				otherCheckBox1.Checked = false;
				otherCheckBox2.Checked = false;
				page1.SigninBtnClick();
				AssertEquals(false, page1.SiteUser.IsLoggedIn);
				AssertStartsWith("Should be redirected via LoginRouter", "/webapp/Login/LoginComplete.aspx", page1.Response.RedirectLocation);
				var query = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.LoginRouterIdentity);
				query.AddToFilter(StmAccessTokenSchema.SAT_ParentId, TestContact2.PK);
				query.AddToFilter(StmAccessTokenSchema.SAT_ParentTableCode, OrgContactSchema.Constants.Prefix);
				query.AddToFilter(StmAccessTokenSchema.SAT_RemainingUseCount, 1);
				var routingToken = Factory.LoadTop1<StmAccessToken>(query);
				AssertNotNull(routingToken);
			}
		}

		public void TestSigninSelectedContactBtn_ClickForMultiMatchedContactsShouldSetCompanyCodeForRememberMe()
		{
			AssertSigninSelectedContactBtn_ClickForMultiMatchedContactsShouldSetCompanyCodeForRememberMe(true);
		}

		public void TestSigninSelectedContactBtn_ClickForMultiMatchedContactsShouldNotSetCompanyCodeForRememberMe()
		{
			AssertSigninSelectedContactBtn_ClickForMultiMatchedContactsShouldSetCompanyCodeForRememberMe(false);
		}

		void AssertSigninSelectedContactBtn_ClickForMultiMatchedContactsShouldSetCompanyCodeForRememberMe(bool rememberMe)
		{
			using (var page1 = GetPageForTest())
			{
				var passwordSecureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, rememberMe ? TestTokenRememberMeOn : TestTokenRememberMeOff } };
				HttpContext.Current.Request.QueryString.Remove(SecureQueryString.QueryStringKey);
				HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, WebUtility.UrlEncode(passwordSecureQueryString.ToString()));
				if (rememberMe)
				{
					page1.AppInstance.ApplicationCookie.WriteUser(string.Empty, TestContact.OC_Email, TestContactPassword);
				}

				page1.DoPageLoad();
				AssertEquals("Precondition: Contacts repeater div should be visible", true, page1.LoginContactsDivExposed.Visible);
				var dataSourceForBinding = page1.ChooseCompanyManagerExposed.LoginContacts;
				page1.BindRepeater(dataSourceForBinding.Cast<OrgContact>().ToList());
				var repeater = page1.LoginContactRepeaterExposed;
				AssertEquals("Precondition: Should be an item for each contact in the scope", 3, repeater.Items.Count);
				var companyCode1 = ((ZTextLabel)repeater.Items[0].FindControl("CompanyCode")).Text;
				var companyCode2 = ((ZTextLabel)repeater.Items[1].FindControl("CompanyCode")).Text;
				ZRadioButton contact1CheckBox;
				ZRadioButton otherCheckBox1;
				ZRadioButton otherCheckBox2;
				var testOrgCode = TestOrg.OH_Code;
				if (companyCode1.StartsWith(testOrgCode))
				{
					contact1CheckBox = (ZRadioButton)repeater.Items[0].FindControl("Checked");
					otherCheckBox1 = (ZRadioButton)repeater.Items[1].FindControl("Checked");
					otherCheckBox2 = (ZRadioButton)repeater.Items[2].FindControl("Checked");
				}
				else if (companyCode2.StartsWith(testOrgCode))
				{
					contact1CheckBox = (ZRadioButton)repeater.Items[1].FindControl("Checked");
					otherCheckBox1 = (ZRadioButton)repeater.Items[0].FindControl("Checked");
					otherCheckBox2 = (ZRadioButton)repeater.Items[2].FindControl("Checked");
				}
				else
				{
					contact1CheckBox = (ZRadioButton)repeater.Items[2].FindControl("Checked");
					otherCheckBox1 = (ZRadioButton)repeater.Items[0].FindControl("Checked");
					otherCheckBox2 = (ZRadioButton)repeater.Items[1].FindControl("Checked");
				}

				contact1CheckBox.Checked = true;
				otherCheckBox1.Checked = false;
				otherCheckBox2.Checked = false;
				var initialCookieCount = page1.Response.Cookies.Count;
				page1.SigninBtnClick();
				AssertEquals(false, page1.SiteUser.IsLoggedIn);
				AssertStartsWith("Precondition: Should be redirected via LoginRouter", "/webapp/Login/LoginComplete.aspx", page1.Response.RedirectLocation);
				if (rememberMe)
				{
					var encoder = new TwoWayEncoder(new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC"));
					var responseCookie = page1.Response.Cookies.Get(initialCookieCount + 1);
					AssertEquals("Should be set since manager.RememberMe is true", TestOrg.OH_Code, encoder.Decrypt(responseCookie.Values["0"]));
					AssertEquals("Should be set since manager.RememberMe is true", TestContact.OC_Email, encoder.Decrypt(responseCookie.Values["1"]));
					AssertEquals("Should be set since manager.RememberMe is true", TestContactPassword, encoder.Decrypt(responseCookie.Values["2"]));
				}
				else
				{
					AssertEquals(initialCookieCount, page1.Response.Cookies.Count);
				}
			}
		}

		public void TestReportErrorIfNoMatchingContacts()
		{
			using (var page1 = GetPageForTest())
			{
				var passwordSecureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, TestTokenRememberMeOff } };
				HttpContext.Current.Request.QueryString.Remove(SecureQueryString.QueryStringKey);
				HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, WebUtility.UrlEncode(passwordSecureQueryString.ToString()));
				page1.DoPageLoad();
				AssertEquals("Precondition: Contacts repeater div should be visible", true, page1.LoginContactsDivExposed.Visible);
				var dataSourceForBinding = page1.ChooseCompanyManagerExposed.LoginContacts;
				page1.BindRepeater(dataSourceForBinding.Cast<OrgContact>().ToList());
				var repeater = page1.LoginContactRepeaterExposed;
				AssertEquals("Precondition: Should be an item for each contact in the scope", 3, repeater.Items.Count);
				var contact1CheckBox = (ZRadioButton)repeater.Items[0].FindControl("Checked");
				var otherCheckBox1 = (ZRadioButton)repeater.Items[1].FindControl("Checked");
				var otherCheckBox2 = (ZRadioButton)repeater.Items[2].FindControl("Checked");
				var label = ((ZTextLabel)repeater.Items[0].FindControl("CompanyCode"));

				label.Text = string.Empty;
				label.Attributes.Remove("value");

				contact1CheckBox.Checked = true;
				otherCheckBox1.Checked = false;
				otherCheckBox2.Checked = false;
				ErrorReporter.Instance.Clear();
				page1.SigninBtnClick();
				AssertEquals(false, page1.SiteUser.IsLoggedIn);
				AssertEquals("Precondition: Should not be redirected", null, page1.Response.RedirectLocation);
				AssertEquals("Should report error when contact cannot be found", "ChooseCompany.aspx contact not found", ErrorReporter.LastKeyReported);
				var passwordHoldingContacts = dataSourceForBinding.Cast<OrgContact>().ToArray();
				AssertEquals("Should report error when contact cannot be found", FormattableString.Invariant($"Mismatched Organization Code: , Login Contact Organization Codes: {string.Join(",", passwordHoldingContacts.Select(x => x.OrganisationCode))}"), ErrorReporter.LastMessageReported);
				ErrorReporter.Instance.Clear();
			}
		}

		#region Implementation
		EDIOrgHeader TestOrg;
		OrgHeader TestOrg2;
		EDIOrgContact TestContact;
		OrgContact TestContact2;
		EdiCustomerUserAccount TestUserAccount;
		const string TestContactPassword = "Changeme123!";
		string TestTokenRememberMeOn;
		string TestTokenRememberMeOff;
		string TestToken_Expired;
		string TestToken_InvalidType;
		protected override void SetUp()
		{
			base.SetUp();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			TestOrg = (EDIOrgHeader)database.WebAccessOrg;
			TestContact = (EDIOrgContact)TestOrg.Contacts.AddNew();
			TestContact.OC_ContactName = "User 1";
			TestContact.OC_Email = "u1@cw1.com";
			TestContact.OC_WebAccessEnabled = true;
			TestUserAccount = Factory.New<EdiCustomerUserAccount>();
			TestUserAccount.EUA_LD = database.PK;
			TestUserAccount.EUA_UserID = "U01";
			TestUserAccount.EUA_FullName = "User 1";
			TestUserAccount.EUA_Email = "u1@cw1.com";
			TestUserAccount.EUA_OC_WebAccessContact = TestContact.PK;
			TestOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			TestContact2 = TestOrg2.Contacts.AddNew();
			TestContact2.OC_ContactName = "User 1";
			TestContact2.OC_Email = "u1@cw1.com";
			TestContact2.OC_WebAccessEnabled = true;
			TestContact2.SetHashedPassword(TestContactPassword);
			Factory.Save();
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_ContactName = "User 1";
			contact3.OC_Email = "u1@cw1.com";
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_PER = TestContact.OC_PER;
			TestContact.Person.PER_EmailAddress = "other@home.com";
			TestContact2.Person.PER_EmailAddress = "new@home.com";
			TestContact.Person.SetHashedPassword(TestContactPassword);
			Factory.Save();
			TestTokenRememberMeOn = "testToken";
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			TestTokenRememberMeOn = accessControl.CreateLimitedToken(AccessTokenTypes.LoginRouterMultiContactIdentity, new AccessTokenInfo(FormattableString.Invariant($"{true}:{TestContact.PK},{TestContact2.PK},{contact3.PK}"), TestContact.PK.ToGuid(), "OC"), maxUses: 1);
			TestTokenRememberMeOff = accessControl.CreateLimitedToken(AccessTokenTypes.LoginRouterMultiContactIdentity, new AccessTokenInfo(FormattableString.Invariant($"{false}:{TestContact.PK},{TestContact2.PK},{contact3.PK}"), TestContact.PK.ToGuid(), "OC"), maxUses: 1);
			TestToken_Expired = accessControl.CreateLimitedToken(AccessTokenTypes.LoginRouterMultiContactIdentity, new AccessTokenInfo(FormattableString.Invariant($"{TestContact.PK},{TestContact2.PK},{contact3.PK}"), TestContact.PK.ToGuid(), "OC"), TimeSpan.FromHours(-1), 1);
			TestToken_InvalidType = accessControl.CreateLimitedToken("INV", new AccessTokenInfo("", Guid.Empty, "INV"), maxUses: 1);
		}

		ChooseCompanyForTest GetPageForTest()
		{
			var page = new ChooseCompanyForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			return page;
		}

		class ChooseCompanyForTest : ChooseCompany
		{
			public ChooseCompanyForTest()
			{
				SignInButton = new Button();
				SignInButton.Visible = true;
				Message = new ZTextLabel();
				Message.Visible = false;
				LoginContactsDiv = new HtmlGenericControl();
				LoginContactsDiv.Visible = true;
				CopyrightYear = new ZTextLabel();
				LoginContactRepeater = new ZRepeater();
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

			public void BindRepeater(List<OrgContact> loginContacts)
			{
				LoginContactRepeater.Bind(loginContacts);
				var iterator = 0;
				foreach (var item in LoginContactRepeater.Items.Cast<RepeaterItem>())
				{
					var radioButton = new ZRadioButton { ID = "Checked", Checked = iterator == 0 };
					var contact = loginContacts[iterator];
					var companyCodeLabel = new ZTextLabel($"{contact.OrganisationCode} - {contact.WorkingAddressCompanyName}");
					companyCodeLabel.ID = "CompanyCode";
					companyCodeLabel.Attributes.Add("value", contact.OrganisationCode);

					item.Controls.Add(radioButton);
					item.Controls.Add(companyCodeLabel);
					iterator++;
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
			public Button SignInButtonExposed => SignInButton;
			public HtmlGenericControl LoginContactsDivExposed => LoginContactsDiv;
			public ZRepeater LoginContactRepeaterExposed => LoginContactRepeater;
			public ChooseCompanyManager ChooseCompanyManagerExposed => (ChooseCompanyManager)DataSource;
			public bool RememberMeExposed => RememberMe;
			public void SigninBtnClick() => SignInButton_Click(null, EventArgs.Empty);
		}

		protected override ZPage GetNewPage()
		{
			return GetPageForTest();
		}
		#endregion
	}
}
