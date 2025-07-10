using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Environment;
using Enterprise.ErrorReporting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class RegisterMasterPasswordPageTest : TestCaseWithFactory
	{
		public void TestValidContactsAndPassword()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword("5678");
			Factory.Save();
			Assert("Precondition", !contact.Person.HasPassword);
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);
			TestPage.OnLoadForTest();
			Assert("Login Instructions should be visible", TestPage.ContactsBoxForTest.Visible);
			Assert("Error message should not be visible", !TestPage.ErrorMessageForTest.Visible);
		}

		public void TestInvalidQueryString()
		{
			TestPage.OnLoadForTest();
			AssertEquals("Precondition", null, TestPage.RequestQueryString_Exposed[LoginRouter.QueryStringKey]);
			Assert("Login Instructions should not be visible", !TestPage.ContactsBoxForTest.Visible);
			Assert("Error message should be visible", TestPage.ErrorMessageForTest.Visible);
			AssertEquals("Error message should be set", "You have been redirected here incorrectly. Please attempt login again and if this issue is recurring, contact your system administrator.", TestPage.ErrorMessageForTest.Text);
		}

		public void TestContactInTokenHasPersonPassword()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword("5678");
			contact.Person.SetHashedPassword("9101");
			Factory.Save();
			Assert("Precondition", contact.Person.HasPassword);
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);
			TestPage.OnLoadForTest();
			Assert("Login Instructions should not be visible", !TestPage.ContactsBoxForTest.Visible);
			Assert("Error message should be visible", TestPage.ErrorMessageForTest.Visible);
			AssertEquals("Error message should be set", "You have been redirected here incorrectly. Please attempt login again and if this issue is recurring, contact your system administrator.", TestPage.ErrorMessageForTest.Text);
		}

		public void TestUseExistingButtonClickWithNoSelectedContact_ShouldShowError()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			contact.Header.OH_Code = "ARMOUR";
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword("5678");
			contact2.Header.OH_Code = "PARMA";
			Factory.Save();
			Assert("Precondition", !contact.Person.HasPassword);
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);
			var dataSourceForBinding = new MasterPasswordManager(contact.Person).PasswordHoldingContacts;
			TestPage.OnLoadForTest();
			TestPage.BindRepeater(dataSourceForBinding.Cast<OrgContact>().ToList());
			var repeater = TestPage.PasswordSourceContactRepeaterExposed;
			AssertEquals("Precondition: Should be an item for each contact on the person", 2, repeater.Items.Count);
			var contact1CheckBox = (ZRadioButton)repeater.Items[0].FindControl("Checked");
			contact1CheckBox.Checked = false;
			var contact2CheckBox = (ZRadioButton)repeater.Items[1].FindControl("Checked");
			contact2CheckBox.Checked = false;
			TestPage.UseExistingButton_ClickExposed();
			Assert("Contact passwords should be unchanged", contact.VerifyPassword("1234"));
			Assert("Contact passwords should be unchanged", contact2.VerifyPassword("5678"));
			Assert("Person password should be empty", !contact.Person.HasPassword);
			Assert("Login Instructions should be visible", TestPage.ContactsBoxForTest.Visible);
			Assert("Error message should be visible", TestPage.ErrorMessageForTest.Visible);
			AssertEquals("Error message should be set", "Please select a contact", TestPage.ErrorMessageForTest.Text);
		}

		public void TestUseExistingButtonClickWithBrokenCompanyCode()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			contact.Header.OH_Code = "ARMOUR";
			contact.OC_Email = "a@a.com";
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword("5678");
			contact2.Header.OH_Code = "PARMA";
			contact2.OC_Email = "b@b.com";
			Factory.Save();
			Assert("Precondition", !contact.Person.HasPassword);
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);
			var dataSourceForBinding = new MasterPasswordManager(contact.Person).PasswordHoldingContacts;
			TestPage.OnLoadForTest();
			TestPage.BindRepeater(dataSourceForBinding.Cast<OrgContact>().ToList());
			var repeater = TestPage.PasswordSourceContactRepeaterExposed;
			AssertEquals("Precondition: Should be an item for each contact on the person", 2, repeater.Items.Count);
			var contact1CheckBox = (ZRadioButton)repeater.Items[0].FindControl("Checked");
			contact1CheckBox.Checked = true;
			var contact1CompanyCode = (ZTextLabel)repeater.Items[0].FindControl("CompanyCode");
			contact1CompanyCode.Text = string.Empty;
			var contact2CheckBox = (ZRadioButton)repeater.Items[1].FindControl("Checked");
			contact2CheckBox.Checked = false;
			ErrorReporter.Instance.Clear();
			TestPage.UseExistingButton_ClickExposed();
			Assert("Contact passwords should be unchanged", contact.VerifyPassword("1234"));
			Assert("Contact passwords should be unchanged", contact2.VerifyPassword("5678"));
			Assert("Person password should be empty", !contact.Person.HasPassword);
			Assert("Login Instructions should be visible", TestPage.ContactsBoxForTest.Visible);
			Assert("Error message should be visible", TestPage.ErrorMessageForTest.Visible);
			AssertEquals("Error message should be set", "There was an error while loading this contact. Please refresh the page. If the issue persists, please raise an incident.", TestPage.ErrorMessageForTest.Text);
			AssertEquals("Should report error when contact cannot be found", "Organization code or email mismatch", ErrorReporter.LastKeyReported);
			var passwordHoldingContacts = dataSourceForBinding.Cast<OrgContact>().ToArray();
			AssertEquals("Should report error when contact cannot be found", FormattableString.Invariant($"Mismatched Organization Code: , Mismatched Email: {dataSourceForBinding[0].OC_Email}, Login Contact Organization Codes: {string.Join(",", passwordHoldingContacts.Select(x => x.OrganisationCode))}, Login Contact Emails: {string.Join(",", passwordHoldingContacts.Select(x => x.OC_Email))}"), ErrorReporter.LastMessageReported);
			ErrorReporter.Instance.Clear();
		}

		public void TestUseExistingButtonClick_ShouldMoveSelectedContactPasswordToPerson()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			contact.Header.OH_Code = "ARMOUR";
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword("5678");
			contact2.Header.OH_Code = "PARMA";
			Factory.Save();
			Assert("Precondition", !contact.Person.HasPassword);
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);
			var dataSourceForBinding = new MasterPasswordManager(contact.Person).PasswordHoldingContacts;
			TestPage.OnLoadForTest();
			TestPage.BindRepeater(dataSourceForBinding.Cast<OrgContact>().ToList());
			var repeater = TestPage.PasswordSourceContactRepeaterExposed;
			AssertEquals("Precondition: Should be an item for each contact on the person", 2, repeater.Items.Count);
			var companyCode = ((ZTextLabel)repeater.Items[1].FindControl("CompanyCode")).Text;
			ZRadioButton contact1CheckBox;
			ZRadioButton contact2CheckBox;
			if (contact2.OrgCode.EqualsIgnoringCase(companyCode))
			{
				contact1CheckBox = (ZRadioButton)repeater.Items[0].FindControl("Checked");
				contact2CheckBox = (ZRadioButton)repeater.Items[1].FindControl("Checked");
			}
			else
			{
				contact1CheckBox = (ZRadioButton)repeater.Items[1].FindControl("Checked");
				contact2CheckBox = (ZRadioButton)repeater.Items[0].FindControl("Checked");
			}

			contact1CheckBox.Checked = false;
			contact2CheckBox.Checked = true;
			TestPage.UseExistingButton_ClickExposed();
			contact.Reload();
			contact2.Reload();
			contact.Person.Reload();
			CombineAssertions("Contact passwords should be cleared", () =>
			{
				Assert(contact.OC_PasswordHash.IsEmpty);
				Assert(contact.OC_PasswordSalt.IsEmpty);
				AssertEquals(0, contact.OC_PasswordHashIterations);
				Assert(contact2.OC_PasswordHash.IsEmpty);
				Assert(contact2.OC_PasswordSalt.IsEmpty);
				AssertEquals(0, contact2.OC_PasswordHashIterations);
			});
			Assert("Person password should be updated from contact2", contact.Person.VerifyPassword("5678"));
			AssertContains("Should be redirected to the next login router page", "/Login/UpdateContactInformation.aspx", TestPage.Response.RedirectLocation);
		}

		public void TestUseExistingButtonClick_ShouldNotThrowNREIfEnvironmentVariablesAreNull()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			contact.Header.OH_Code = "ARMOUR";
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword("5678");
			contact2.Header.OH_Code = "PARMA";
			Factory.Save();
			Assert("Precondition", !contact.Person.HasPassword);
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);
			var dataSourceForBinding = new MasterPasswordManager(contact.Person).PasswordHoldingContacts;
			TestPage.OnLoadForTest();
			TestPage.BindRepeater(dataSourceForBinding.Cast<OrgContact>().ToList());
			var repeater = TestPage.PasswordSourceContactRepeaterExposed;
			AssertEquals("Precondition: Should be an item for each contact on the person", 2, repeater.Items.Count);
			var companyCode = ((ZTextLabel)repeater.Items[1].FindControl("CompanyCode")).Text;
			ZRadioButton contact1CheckBox;
			ZRadioButton contact2CheckBox;
			if (contact2.OrgCode.EqualsIgnoringCase(companyCode))
			{
				contact1CheckBox = (ZRadioButton)repeater.Items[0].FindControl("Checked");
				contact2CheckBox = (ZRadioButton)repeater.Items[1].FindControl("Checked");
			}
			else
			{
				contact1CheckBox = (ZRadioButton)repeater.Items[1].FindControl("Checked");
				contact2CheckBox = (ZRadioButton)repeater.Items[0].FindControl("Checked");
			}

			contact1CheckBox.Checked = false;
			contact2CheckBox.Checked = true;
			Env.ClearUserContext();
			AssertNoExceptionThrown("Should setup session if it disappears", () => TestPage.UseExistingButton_ClickExposed());
		}

		public void TestUseExistingButtonClick_ShouldNotCreateNewToken()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			contact.Header.OH_Code = "ARMOUR";
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword("5678");
			contact2.Header.OH_Code = "PARMA";
			Factory.Save();
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);
			var dataSourceForBinding = new MasterPasswordManager(contact.Person).LoginContacts;
			TestPage.OnLoadForTest();
			TestPage.BindRepeater(dataSourceForBinding.Cast<OrgContact>().ToList());
			TestPage.UseExistingButton_ClickExposed();
			var existingTokenQuery = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.LoginRouterIdentity);
			existingTokenQuery.AddToFilter(StmAccessTokenSchema.SAT_ParentId, contact.PK);
			var existingToken = Factory.LoadTop1<StmAccessToken>(existingTokenQuery);
			AssertEquals(token, existingToken.SAT_Token);
		}

		public void TestSetNewPasswordButtonClick_ShouldSendUserToSetMasterPasswordPage()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword("5678");
			Factory.Save();
			Assert("Precondition", !contact.Person.HasPassword);
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			const string originalUrl = "https://google.com.au/";
			var secureQueryString = new SecureQueryString { { LoginRouter.OriginalUrlQueryStringKey, originalUrl }, { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);
			TestPage.OnLoadForTest();
			TestPage.SetNewPasswordButton_ClickExposed();
			Assert("Contact passwords should be unchanged", contact.VerifyPassword("1234"));
			Assert("Contact passwords should be unchanged", contact2.VerifyPassword("5678"));
			Assert("Person password should be empty", !contact.Person.HasPassword);
			var query = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.SetMasterPassword);
			query.AddToFilter(StmAccessTokenSchema.SAT_ParentId, contact.PK);
			query.AddToFilter(StmAccessTokenSchema.SAT_ParentTableCode, OrgContactSchema.Constants.Prefix);
			query.AddToFilter(StmAccessTokenSchema.SAT_RemainingUseCount, 1);
			var masterPasswordToken = Factory.LoadTop1<StmAccessToken>(query);
			AssertNotNull(masterPasswordToken);
			AssertEquals("Scope should match original url", originalUrl, masterPasswordToken.SAT_Scope);
			AssertContains("Token should be included in query string", FormattableString.Invariant($"SetMasterPassword.aspx?"), TestPage.Response.RedirectLocation);
		}

		public void TestSetNewPasswordButtonClick_NoOriginalUrl()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword("5678");
			Factory.Save();
			Assert("Precondition", !contact.Person.HasPassword);
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);
			TestPage.OnLoadForTest();
			TestPage.SetNewPasswordButton_ClickExposed();
			var query = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.SetMasterPassword);
			query.AddToFilter(StmAccessTokenSchema.SAT_ParentId, contact.PK);
			query.AddToFilter(StmAccessTokenSchema.SAT_ParentTableCode, OrgContactSchema.Constants.Prefix);
			query.AddToFilter(StmAccessTokenSchema.SAT_RemainingUseCount, 1);
			var masterPasswordToken = Factory.LoadTop1<StmAccessToken>(query);
			AssertNotNull(masterPasswordToken);
			AssertEquals("Scope should match my account index page", EDIDataRegistry.Instance.MyAccountIndexPage.Value, masterPasswordToken.SAT_Scope);
		}

		public void TestSetNewPasswordButtonClick_EnsureContactIsNotNull()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword("5678");
			Factory.Save();
			Assert("Precondition", !contact.Person.HasPassword);
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			const string originalUrl = "https://google.com.au/";
			var secureQueryString = new SecureQueryString { { LoginRouter.OriginalUrlQueryStringKey, originalUrl }, { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);
			TestPage.OnLoadForTest();
			TestPage.ClearIdentityManagerContactValue();
			TestPage.SetNewPasswordButton_ClickExposed();
			var query = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.SetMasterPassword);
			query.AddToFilter(StmAccessTokenSchema.SAT_ParentId, contact.PK);
			query.AddToFilter(StmAccessTokenSchema.SAT_ParentTableCode, OrgContactSchema.Constants.Prefix);
			query.AddToFilter(StmAccessTokenSchema.SAT_RemainingUseCount, 1);
			var masterPasswordToken = Factory.LoadTop1<StmAccessToken>(query);
			AssertNotNull(masterPasswordToken);
		}

		public void TestSetNewPasswordButtonClick_ShouldShowErrorIfContactIsNull()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword("5678");
			Factory.Save();
			Assert("Precondition", !contact.Person.HasPassword);
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			const string originalUrl = "https://google.com.au/";
			var secureQueryString = new SecureQueryString { { LoginRouter.OriginalUrlQueryStringKey, originalUrl }, { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);
			var idManager = new LoginRouterIdentityManager();
			idManager.PopulatePropertiesFromToken(token);
			idManager.ConsumeToken();
			TestPage.OnLoadForTest();
			TestPage.ClearIdentityManagerContactValue();
			TestPage.SetNewPasswordButton_ClickExposed();
			var query = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.SetMasterPassword);
			query.AddToFilter(StmAccessTokenSchema.SAT_ParentId, contact.PK);
			query.AddToFilter(StmAccessTokenSchema.SAT_ParentTableCode, OrgContactSchema.Constants.Prefix);
			query.AddToFilter(StmAccessTokenSchema.SAT_RemainingUseCount, 1);
			var masterPasswordToken = Factory.LoadTop1<StmAccessToken>(query);
			AssertNull(masterPasswordToken);
			Assert("Login Instructions should not be visible", !TestPage.ContactsBoxForTest.Visible);
			Assert("Error message should be visible", TestPage.ErrorMessageForTest.Visible);
			AssertEquals("Error message should be set", "There was a problem loading your account. Please try to login again.", TestPage.ErrorMessageForTest.Text);
			AssertEquals(1, TestPage.ErrorReportedCount);
		}

		public void TestSetNewPasswordButtonClick_ShouldNotReportErrorIfTokenExpired()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword("5678");
			Factory.Save();
			Assert("Precondition", !contact.Person.HasPassword);
			var expiredToken = LoginRouterIdentityManager.GenerateToken(contact);
			var tokenBizO = Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Token, expiredToken));
			tokenBizO.SAT_ExpiresAt = ZDateTime.UtcNow - TimeSpan.FromMinutes(15);
			Factory.Save();
			const string originalUrl = "https://google.com.au/";
			var secureQueryString = new SecureQueryString { { LoginRouter.OriginalUrlQueryStringKey, originalUrl }, { LoginRouter.IdentityTokenQueryStringKey, expiredToken } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);
			var errorReportQuery = new ZQuery(StmErrorReportSchema.QER_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcToday);
			var initialErrorReportCount = Factory.GetDatabaseCount(typeof(StmErrorReport), errorReportQuery);
			TestPage.OnLoadForTest();
			TestPage.ClearIdentityManagerContactValue();
			TestPage.SetNewPasswordButton_ClickExposed();
			AssertEquals("Should not have any new error reports", initialErrorReportCount, Factory.GetDatabaseCount(typeof(StmErrorReport), errorReportQuery));
		}

		public void TestSetNewPasswordButtonClick_ShouldNotReportErrorIfQueryStringExpired()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword("5678");
			Factory.Save();
			Assert("Precondition", !contact.Person.HasPassword);
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			const string originalUrl = "https://google.com.au/";
			var secureQueryString = new SecureQueryString { { LoginRouter.OriginalUrlQueryStringKey, originalUrl }, { LoginRouter.IdentityTokenQueryStringKey, token } };
			secureQueryString.ExpireTime = TimeSpan.FromMinutes(-5);
			var idManager = new LoginRouterIdentityManager();
			idManager.PopulatePropertiesFromToken(token);
			idManager.ConsumeToken();
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);
			var errorReportQuery = new ZQuery(StmErrorReportSchema.QER_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcToday);
			var initialErrorReportCount = Factory.GetDatabaseCount(typeof(StmErrorReport), errorReportQuery);
			TestDateAttribute.AddMinutes(40);
			TestPage.OnLoadForTest();
			TestPage.ClearIdentityManagerContactValue();
			TestPage.SetNewPasswordButton_ClickExposed();
			AssertEquals("Should not have any new error reports", initialErrorReportCount, Factory.GetDatabaseCount(typeof(StmErrorReport), errorReportQuery));
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
		}

		RegisterMasterPasswordForTest TestPage
		{
			get
			{
				if (testPage == null)
				{
					testPage = GetNewControl();
				}

				return testPage;
			}
		}

		RegisterMasterPasswordForTest testPage;
		RegisterMasterPasswordForTest GetNewControl()
		{
			var testPage = new RegisterMasterPasswordForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(testPage, new object[] { HttpContext.Current });
			return testPage;
		}

		#region Class for Test
		public class RegisterMasterPasswordForTest : RegisterMasterPassword
		{
			public RegisterMasterPasswordForTest()
			{
				ContactsBox = new HtmlGenericControl();
				PasswordSourceContactRepeater = new ZRepeater();
				Controls.Add(PasswordSourceContactRepeater);
				ErrorMessage = new ZTextLabel();
			}

			public void OnLoadForTest()
			{
				OnLoad(EventArgs.Empty);
			}

			public NameValueCollection RequestQueryString_Exposed => RequestQueryString;
			public MasterPasswordManager ManagerForTest => Manager;
			public HtmlGenericControl ContactsBoxForTest => ContactsBox;
			public ZRepeater PasswordSourceContactRepeaterExposed => PasswordSourceContactRepeater;
			public ZTextLabel ErrorMessageForTest => ErrorMessage;
			public void UseExistingButton_ClickExposed()
			{
				UseExistingButton_Click(null, EventArgs.Empty);
			}

			public void SetNewPasswordButton_ClickExposed()
			{
				SetNewPasswordButton_Click(null, EventArgs.Empty);
			}

			public void ClearIdentityManagerContactValue()
			{
				var property = typeof(MyAccountLoginRouterIdentityManager).GetProperty("Contact");
				property.SetValue(IdentityManager, null);
			}

			public void BindRepeater(List<OrgContact> passwordHoldingContacts)
			{
				PasswordSourceContactRepeater.Bind(passwordHoldingContacts);
				var iterator = 0;
				foreach (var item in PasswordSourceContactRepeater.Items.Cast<RepeaterItem>())
				{
					var checkBox = new ZRadioButton { ID = "Checked", Checked = iterator == 0 };
					var companyCodeLabel = new ZTextLabel(passwordHoldingContacts[iterator].OrganisationCode)
					{ ID = "CompanyCode" };
					var companyNameLabel = new ZTextLabel(passwordHoldingContacts[iterator].WorkingAddressCompanyName)
					{ ID = "CompanyName" };
					var emailLabel = new ZTextLabel(passwordHoldingContacts[iterator].OC_Email)
					{ ID = "Email" };
					var primaryWorkplaceLabel = new ZTextLabel(passwordHoldingContacts[iterator].OC_IsPrimaryContact.ToString())
					{ ID = "PrimaryWorkplace" };
					item.Controls.Add(checkBox);
					item.Controls.Add(companyCodeLabel);
					item.Controls.Add(companyNameLabel);
					item.Controls.Add(emailLabel);
					item.Controls.Add(primaryWorkplaceLabel);
					iterator++;
				}
			}

			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			public bool LastPeekSuccessful;
			public AccessTokenInfo LastAccessTokenInfo;
			public int ErrorReportedCount;
			protected override void ReportError(bool peekSuccessful, AccessTokenInfo accessToken)
			{
				ErrorReportedCount++;
				LastPeekSuccessful = peekSuccessful;
				LastAccessTokenInfo = accessToken;
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
		#endregion
	}
}
