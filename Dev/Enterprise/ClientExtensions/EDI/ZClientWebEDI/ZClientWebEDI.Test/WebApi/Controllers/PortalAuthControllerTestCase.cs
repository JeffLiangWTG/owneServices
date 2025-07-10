using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Authentication.Primitives;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZClientWebCargoWiseEDI.PortalAuth;
using Newtonsoft.Json;
using static Enterprise.CustomerService.Business.StaffContactValueObjectHelper;
using static Enterprise.ZClientWebCargoWiseEDI.GlowPortalUrlHelper;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	class PortalAuthControllerTestCase : TestCaseWithFactory
	{
		#region GetAutoLoginUrl

		public void TestGetAutoLoginUrl()
		{
			CreateLicence();
			var request = CreateRequest(StaffQueryString, UserPortal.UserPortalLauncher.eRequestNewLandingPageId, "REF", "Organisation", "EA5FF6D5-F083-4C7C-85E3-46EDBBBB2015");
			string token;
			using (request)
			using (var response = Execute(request))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var autoLoginUrl = CleanResponse(response.Content.ReadAsStringAsync().Result);
				AssertContains("GlowPortalAutoLogin.aspx?qdata=", autoLoginUrl);
				var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(new Uri(autoLoginUrl));
				var resultUri = new Uri(CleanResponse(glowUrl));
				var queryParameters = UriExtensions.ParseQueryString(resultUri);
				token = queryParameters[ServerOnlyQueryStringKeys.SSOKey] ?? string.Empty;
				AssertEquals("https://unit-testing/GlowPortal/eRequestPortal?sso_otp=" + Uri.EscapeDataString(token) + "&LicenseCode=ENTCOMSRV&Module=REF&SubModule=Organisation&ReferenceId=EA5FF6D5-F083-4C7C-85E3-46EDBBBB2015#/workflow", resultUri.ToString());
			}

			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			_ = accessControl.TryConsume(token, AccessTokenTypes.LocalIdentity, out var info);
			var scope = new LocalIdentityTokenScope()
			{ BranchKey = Env.CurrentBranchPK, DepartmentKey = Env.CurrentDepartmentPK };
			var scopeObject = JsonConvert.SerializeObject(scope);
			AssertEquals(scopeObject, info.Scope);
			AssertEquals(ContactForTest.PK, info.ParentId);
			AssertEquals(OrgContactSchema.Constants.Prefix, info.ParentTableCode);
			var result = accessControl.TryConsume(token, AccessTokenTypes.LocalIdentity, out _);
			AssertEquals("Token should only be valid for one use.", false, result);
		}

		public void TestGetAutoLoginUrl_LoginOrg()
		{
			CreateLicence();
			var lic2 = BillingTestHelper.CreateAnotherLicence(LicHeaderForTest, "CO2");
			var lic3 = BillingTestHelper.CreateAnotherLicence(LicHeaderForTest, "CO3");
			var contact2 = lic2.Company.Header.Contacts.AddNew();
			contact2.OC_ContactName = "Your Name";
			contact2.OC_Email = "youremail@test.com";
			var contact3 = lic3.Company.Header.Contacts.AddNew();
			contact3.OC_ContactName = "Your Name";
			contact3.OC_Email = "youremail@test.com";
			Factory.Save();
			var queryString2 = ContactToSecuredQueryString(ContactXsd, lic2);
			var queryString3 = ContactToSecuredQueryString(ContactXsd, lic3);
			var queryString4 = ContactToSecuredQueryString(ContactXsd, lic3);
			queryString4[QueryStringKeys.LicenceCode] = "ENTYYYSRV";
			var request1 = CreateRequest(StaffQueryString, UserPortal.UserPortalLauncher.eRequestNewLandingPageId, "REF", "Organisation", "EA5FF6D5-F083-4C7C-85E3-46EDBBBB2015");
			var request2 = CreateRequest(queryString2, UserPortal.UserPortalLauncher.eRequestNewLandingPageId, "FOR", "Shipment", "621EE252-036D-4AC5-99F3-95C85BD5BE99");
			var request3 = CreateRequest(queryString3, UserPortal.UserPortalLauncher.eRequestNewLandingPageId, "ACC", "Invoices", "567EDEA5-5C49-4DAA-9392-FB01522EB02C");
			var request4 = CreateRequest(queryString4, UserPortal.UserPortalLauncher.eRequestNewLandingPageId, "ACC", "Invoices", "567EDEA5-5C49-4DAA-9392-FB01522EB02C");
			string token1;
			string token2;
			string token3;
			string token4;
			using (request1)
			using (var response = Execute(request1))
			{
				var autoLoginUrl = CleanResponse(response.Content.ReadAsStringAsync().Result);
				AssertContains("GlowPortalAutoLogin.aspx?qdata=", autoLoginUrl);
				var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(new Uri(autoLoginUrl));
				var resultUri = new Uri(CleanResponse(glowUrl));
				var queryParameters = UriExtensions.ParseQueryString(resultUri);
				token1 = queryParameters[ServerOnlyQueryStringKeys.SSOKey] ?? string.Empty;
			}

			using (request2)
			using (var response = Execute(request2))
			{
				var autoLoginUrl = CleanResponse(response.Content.ReadAsStringAsync().Result);
				AssertContains("GlowPortalAutoLogin.aspx?qdata=", autoLoginUrl);
				var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(new Uri(autoLoginUrl));
				var resultUri = new Uri(CleanResponse(glowUrl));
				var queryParameters = UriExtensions.ParseQueryString(resultUri);
				token2 = queryParameters[ServerOnlyQueryStringKeys.SSOKey] ?? string.Empty;
			}

			using (request3)
			using (var response = Execute(request3))
			{
				var autoLoginUrl = CleanResponse(response.Content.ReadAsStringAsync().Result);
				AssertContains("GlowPortalAutoLogin.aspx?qdata=", autoLoginUrl);
				var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(new Uri(autoLoginUrl));
				var resultUri = new Uri(CleanResponse(glowUrl));
				var queryParameters = UriExtensions.ParseQueryString(resultUri);
				token3 = queryParameters[ServerOnlyQueryStringKeys.SSOKey] ?? string.Empty;
			}

			using (request4)
			using (var response = Execute(request4))
			{
				var autoLoginUrl = CleanResponse(response.Content.ReadAsStringAsync().Result);
				AssertContains("GlowPortalAutoLogin.aspx?qdata=", autoLoginUrl);
				var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(new Uri(autoLoginUrl));
				var resultUri = new Uri(CleanResponse(glowUrl));
				var queryParameters = UriExtensions.ParseQueryString(resultUri);
				token4 = queryParameters[ServerOnlyQueryStringKeys.SSOKey] ?? string.Empty;
			}

			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			accessControl.TryConsume(token1, AccessTokenTypes.LocalIdentity, out var info1);
			accessControl.TryConsume(token2, AccessTokenTypes.LocalIdentity, out var info2);
			accessControl.TryConsume(token3, AccessTokenTypes.LocalIdentity, out var info3);
			accessControl.TryConsume(token4, AccessTokenTypes.LocalIdentity, out var info4);
			AssertEquals(ContactForTest.PK, info1.ParentId);
			AssertEquals(OrgContactSchema.Constants.Prefix, info1.ParentTableCode);
			AssertEquals(contact2.PK, info2.ParentId);
			AssertEquals(OrgContactSchema.Constants.Prefix, info2.ParentTableCode);
			AssertEquals(contact3.PK, info3.ParentId);
			AssertEquals(OrgContactSchema.Constants.Prefix, info3.ParentTableCode);
			AssertEquals("unknown company code gives database enterprise org", ContactForTest.PK, info4.ParentId);
		}

		public void TestGetAutoLoginUrl_NewContact()
		{
			CreateLicence();
			ContactXsd = new Xsd.OrgContact();
			ContactXsd.Name = "New Guy";
			ContactXsd.EmailAddress = "newguy@test.org";
			StaffQueryString = ContactToSecuredQueryString(ContactXsd, LicHeaderForTest);
			var request = CreateRequest(StaffQueryString, UserPortal.UserPortalLauncher.eRequestNewLandingPageId, "REF", "Organisation", "EA5FF6D5-F083-4C7C-85E3-46EDBBBB2015");
			string token;
			using (request)
			using (var response = Execute(request))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var autoLoginUrl = CleanResponse(response.Content.ReadAsStringAsync().Result);
				AssertContains("GlowPortalAutoLogin.aspx?qdata=", autoLoginUrl);
				var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(new Uri(autoLoginUrl));
				var resultUri = new Uri(CleanResponse(glowUrl));
				var queryParameters = UriExtensions.ParseQueryString(resultUri);
				token = queryParameters[ServerOnlyQueryStringKeys.SSOKey] ?? string.Empty;
				AssertEquals("https://unit-testing/GlowPortal/eRequestPortal?sso_otp=" + Uri.EscapeDataString(token) + "&LicenseCode=ENTCOMSRV&Module=REF&SubModule=Organisation&ReferenceId=EA5FF6D5-F083-4C7C-85E3-46EDBBBB2015#/workflow", resultUri.ToString());
			}

			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			var result = accessControl.TryConsume(token, AccessTokenTypes.LocalIdentity, out var info);
			var scope = new LocalIdentityTokenScope()
			{ BranchKey = Env.CurrentBranchPK, DepartmentKey = Env.CurrentDepartmentPK };
			var scopeObject = JsonConvert.SerializeObject(scope);
			AssertEquals(scopeObject, info.Scope);
			LicHeaderForTest.Company.Header.Contacts.Reload(true);
			var newContact = LicHeaderForTest.Company.Header.Contacts.Cast<OrgContact>().Single(x => x.OC_Email == "newguy@test.org");
			AssertNotEquals(ContactForTest.PK, info.ParentId);
			AssertEquals(newContact.PK, info.ParentId);
			AssertEquals(OrgContactSchema.Constants.Prefix, info.ParentTableCode);
			result = accessControl.TryConsume(token, AccessTokenTypes.LocalIdentity, out _);
			AssertEquals("Token should only be valid for one use.", false, result);
		}

		public void TestGetAutoLoginContactUrl()
		{
			CreateLicence();
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/api/PortalAuth/AutoLoginContactUrl?landingPageId=" + UserPortal.UserPortalLauncher.eRequestNewLandingPageId + "&contactPK=" + ContactForTest.PK + "&product=ZUB&module=REF&subModule=Organisation&Criticality=CR4&referenceId=EA5FF6D5-F083-4C7C-85E3-46EDBBBB2015");
			string token;
			using (request)
			using (var response = Execute(request))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var autoLoginUrl = CleanResponse(response.Content.ReadAsStringAsync().Result);
				AssertContains("GlowPortalAutoLogin.aspx?qdata=", autoLoginUrl);
				var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(new Uri(autoLoginUrl));
				var resultUri = new Uri(CleanResponse(glowUrl));
				var queryParameters = UriExtensions.ParseQueryString(resultUri);
				token = queryParameters[ServerOnlyQueryStringKeys.SSOKey] ?? string.Empty;
				AssertEquals($"https://unit-testing/GlowPortal/eRequestPortal?sso_otp={Uri.EscapeDataString(token)}&Product=ZUB&Module=REF&SubModule=Organisation&Criticality=CR4&ReferenceId=EA5FF6D5-F083-4C7C-85E3-46EDBBBB2015#/workflow", resultUri.ToString());
			}

			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			_ = accessControl.TryConsume(token, AccessTokenTypes.LocalIdentity, out var info);
			var scope = new LocalIdentityTokenScope()
			{ BranchKey = Env.CurrentBranchPK, DepartmentKey = Env.CurrentDepartmentPK };
			var scopeObject = JsonConvert.SerializeObject(scope);
			AssertEquals(scopeObject, info.Scope);
			AssertEquals(ContactForTest.PK, info.ParentId);
			AssertEquals(OrgContactSchema.Constants.Prefix, info.ParentTableCode);
			var result = accessControl.TryConsume(token, AccessTokenTypes.LocalIdentity, out _);
			AssertEquals("Token should only be valid for one use.", false, result);
		}

		public void TestGetAutoLoginUrl_PortalPageQueryString()
		{
			CreateLicence();
			EDIDataRegistry.Instance.GlowERequestPortalUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "/INC");
			var request = CreateRequest(StaffQueryString, UserPortal.UserPortalLauncher.eRequestPortalLandingPageId, "REF", "Organisation", "EA5FF6D5-F083-4C7C-85E3-46EDBBBB2015");
			string token;
			using (request)
			using (var response = Execute(request))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var autoLoginUrl = CleanResponse(response.Content.ReadAsStringAsync().Result);
				AssertContains("GlowPortalAutoLogin.aspx?qdata=", autoLoginUrl);
				var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(new Uri(autoLoginUrl));
				var resultUri = new Uri(CleanResponse(glowUrl));
				var queryParameters = UriExtensions.ParseQueryString(resultUri);
				token = queryParameters[ServerOnlyQueryStringKeys.SSOKey] ?? string.Empty;
				AssertEquals("https://unit-testing/GlowPortal/INC?sso_otp=" + Uri.EscapeDataString(token), resultUri.ToString());
			}
		}

		public void TestGetAutoLoginUrl_BadLandingPageId()
		{
			CreateLicence();
			var request = CreateRequest(StaffQueryString, "badId", "REF", "Organisation", "EA5FF6D5-F083-4C7C-85E3-46EDBBBB2015");
			using (request)
			using (var response = Execute(request))
			{
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			}
		}

		public void TestGetAutoLoginUrl_BadSecret()
		{
			CreateLicence();
			StaffQueryString[StaffContactValueObjectHelper.QueryStringKeys.Password] = "badsecret";
			var request = CreateRequest(StaffQueryString, UserPortal.UserPortalLauncher.eRequestNewLandingPageId, "REF", "Organisation", "EA5FF6D5-F083-4C7C-85E3-46EDBBBB2015");
			using (request)
			using (var response = Execute(request))
			{
				AssertEquals(HttpStatusCode.Forbidden, response.StatusCode);
			}
		}

		public void TestGetAutoLoginUrl_BadSystemId()
		{
			CreateLicence();
			StaffQueryString[StaffContactValueObjectHelper.QueryStringKeys.LicenceCode] = "badsystemId";
			StaffQueryString[StaffContactValueObjectHelper.QueryStringKeys.DatabaseNumber] = "badsystemId";
			var request = CreateRequest(StaffQueryString, UserPortal.UserPortalLauncher.eRequestNewLandingPageId, "REF", "Organisation", "EA5FF6D5-F083-4C7C-85E3-46EDBBBB2015");
			using (request)
			using (var response = Execute(request))
			{
				AssertEquals(HttpStatusCode.NotFound, response.StatusCode);
			}
		}

		public void TestGetAutoLoginUrl_NotAllowed()
		{
			CreateLicence();
			LicHeaderForTest.Database.LD_AllowAutoLogin = false;
			Factory.Save();
			ContactXsd = new Xsd.OrgContact { Name = "New Guy", EmailAddress = "newguy@test.org" };
			StaffQueryString = ContactToSecuredQueryString(ContactXsd, LicHeaderForTest);
			var request = CreateRequest(StaffQueryString, UserPortal.UserPortalLauncher.eRequestNewLandingPageId, "REF", "Organisation", "EA5FF6D5-F083-4C7C-85E3-46EDBBBB2015");
			using (request)
			using (var response = Execute(request))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var resultUri = new Uri(CleanResponse(response.Content.ReadAsStringAsync().Result));
				AssertEquals("https://unit-testing/GlowPortal/INC", resultUri.ToString());
			}
		}

		public void TestGetAutoLoginUrl_Accreditation()
		{
			CreateLicence();
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/api/PortalAuth/AutoLoginContactUrl?landingPageId=" + UserPortal.UserPortalLauncher.AccreditationAttemptPortalLadingPageId + "&contactPK=" + ContactForTest.PK);
			string token;
			using (request)
			using (var response = Execute(request))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var autoLoginUrl = CleanResponse(response.Content.ReadAsStringAsync().Result);
				AssertContains("GlowPortalAutoLogin.aspx?qdata=", autoLoginUrl);
				var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(new Uri(autoLoginUrl));
				var resultUri = new Uri(CleanResponse(glowUrl));
				var queryParameters = UriExtensions.ParseQueryString(resultUri);
				token = queryParameters[ServerOnlyQueryStringKeys.SSOKey] ?? string.Empty;
				AssertEquals("https://unit-testing/GlowPortal/AAC?sso_otp=" + Uri.EscapeDataString(token), resultUri.ToString());
			}
		}

		public void TestGetAutoLoginUrl_NonProductionSystemNewCustomerUserAccount()
		{
			Env.ClearAllEmailsCreated();
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");
			EDIDataRegistry.Instance.GlowNewERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "eRequestPortal#/workflow");
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD", true);
			var database = licence.Database;
			database.LD_DatabaseNumber = 1003;
			database.LD_LicenceType = DatabaseTypes.Codes.Training;
			var dbSecret = "123456";
			var hashInputString = string.Format(CultureInfo.InvariantCulture, "{0}{1}", database.LD_DatabaseNumber, dbSecret);
			var hashInputBytes = Encoding.UTF8.GetBytes(hashInputString);
			byte[] hashedSecretData;
			using var sha512 = SHA512.Create();
			hashedSecretData = sha512.ComputeHash(hashInputBytes);

			var expectedLDPasswordValue = BitConverter.ToString(hashedSecretData);
			database.LD_Password = expectedLDPasswordValue;
			var org = licence.Company.Header;
			org.Contacts.RemoveAndDeleteAll();
			Factory.Save();
			var contactXsd = new Xsd.OrgContact { Name = "User 1", EmailAddress = "user1@test.com" };
			var staffQueryString = new SecureQueryString { [QueryStringKeys.LicenceCode] = licence.LicenceCode, [QueryStringKeys.DatabaseNumber] = database.LD_DatabaseNumber.ToString(), [QueryStringKeys.ContactData] = ValueObjectEncoder.Serialize(contactXsd), [QueryStringKeys.StaffCode] = "US1", [QueryStringKeys.HomeBranchCode] = "SYD", [QueryStringKeys.Password] = dbSecret };
			var userAccountQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_LD, database.PK);
			userAccountQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_UserID, staffQueryString[QueryStringKeys.StaffCode]);
			AssertEquals("Precondition", 0, Factory.GetDatabaseCount(typeof(EdiCustomerUserAccount), userAccountQuery));
			AssertEquals("Precondition", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			var request = CreateRequest(staffQueryString, UserPortal.UserPortalLauncher.eRequestNewLandingPageId, "REF", "Organisation", "EA5FF6D5-F083-4C7C-85E3-46EDBBBB2015");
			using (request)
			using (var response = Execute(request))
			{
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var resultUri = new Uri(CleanResponse(response.Content.ReadAsStringAsync().Result));
				AssertStartsWith("AbsoluteUri", "https://myaccount-portal.cargowise.com/myaccount/Login/EmailSentNotification.aspx", resultUri.AbsoluteUri);
				var resultQueryString = new QueryString(resultUri.Query);
				var resultSecureQueryString = new SecureQueryString(resultQueryString["?" + SecureQueryString.QueryStringKey]);
				AssertEquals(true, new ZBool(resultSecureQueryString[UserEmailVerificationRoutingDescriptor.EmailVerificationSentUrlKey]));
				AssertEquals("user1@test.com", resultSecureQueryString[UserEmailVerificationRoutingDescriptor.EmailVerificationAddressUrlKey]);
				var userAccounts = Factory.Load<EdiCustomerUserAccount>(userAccountQuery);
				AssertEquals("Should have created the user account", 1, userAccounts.Length);
				var userAccount = userAccounts[0];
				AssertEquals("Should have been saved to the database", true, userAccount.IsInDatabase);
				AssertEquals("Email should be imported from Xsd contact", contactXsd.EmailAddress, userAccount.EUA_Email);
				AssertEquals("Email should be imported from Xsd contact", contactXsd.Name, userAccount.EUA_FullName);
				AssertEquals("Newly created user account should require email verification", true, userAccount.EUA_IsEmailVerificationRequired);
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals(userAccount.EUA_Email, Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
				var tokenQuery = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.VerifyEmailToken);
				tokenQuery.AddToFilter(StmAccessTokenSchema.SAT_ParentId, userAccount.PK);
				var accessToken = Factory.LoadTop1<StmAccessToken>(tokenQuery);
				AssertNotNull(accessToken);
				var originalRequestUrl = accessToken.SAT_Scope.SubstringSafe(accessToken.SAT_Scope.IndexOf(':') + 1);
				Assert("No original url", originalRequestUrl.IsEmpty);
			}
		}

		#endregion

		#region GetAutoLoginToken

		public void TestGetAutoLoginToken_UseInvalidData_ShouldReturnUnauthorized()
		{
			CreateLicence();
			using (Db.Connection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				using (var request = new HttpRequestMessage(HttpMethod.Post, "https://cargowise/api/PortalAuth/AutoLoginToken"))
				{
					request.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("license", "FakeLicense"), new KeyValuePair<string, string>("contact", "The Vanisher"), new KeyValuePair<string, string>("dbnumber", "-1"), new KeyValuePair<string, string>("baseUrl", "https://cargowise/eLearning.aspx"), new KeyValuePair<string, string>("password", ClientSecret) });
					using (var response = Execute(request))
					{
						var responseMessage = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
						CombineAssertions(() =>
						{
							AssertEquals("Auto login should be denied", HttpStatusCode.Unauthorized, response.StatusCode);
							AssertContains("Auto login is not allowed for this user", responseMessage);
						});
					}
				}
			}
		}

		public void TestGetAutoLoginToken_NoAutoLoginAllowed_ShouldReturnUnauthorized()
		{
			CreateLicence();
			LicHeaderForTest.Database.LD_AllowAutoLogin = false;

			using (Db.Connection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				using (var request = new HttpRequestMessage(HttpMethod.Post, "https://cargowise/api/PortalAuth/AutoLoginToken"))
				{
					request.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("license", "FakeLicense"), new KeyValuePair<string, string>("contact", "The Vanisher"), new KeyValuePair<string, string>("dbnumber", "-1"), new KeyValuePair<string, string>("baseUrl", "https://cargowise/eLearning.aspx"), new KeyValuePair<string, string>("password", ClientSecret) });
					using (var response = Execute(request))
					{
						var responseMessage = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
						CombineAssertions(() =>
						{
							AssertEquals("Auto login should be denied", HttpStatusCode.Unauthorized, response.StatusCode);
							AssertContains("Auto login is not allowed for this user", responseMessage);
						});
					}
				}
			}
		}

		public void TestGetAutoLoginToken_NullContact_ShouldReturnBadRequest()
		{
			CreateLicence();
			var licEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			licEnterprise.LE_EnterpriseCode = "_X1";
			var licCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licCompany.LC_LE = licEnterprise.PK;
			licCompany.LC_CompanyCode = "_X2";
			var licDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licDatabase.LD_LE = licEnterprise.PK;
			licDatabase.LD_ServerCode = "_X3";
			var licHeader = Factory.NewWithValidTestData<LicenceHeader>();
			licHeader.LA_LC = licCompany.PK;
			licHeader.LA_LD = licDatabase.PK;
			Factory.Save();
			var hashInputString = string.Format(CultureInfo.InvariantCulture, "{0}{1}", licHeader.Database.LD_DatabaseNumber, "myPassword");
			var hashInputBytes = Encoding.UTF8.GetBytes(hashInputString);
			byte[] hashedSecretData;
			using var sha512 = SHA512.Create();
			hashedSecretData = sha512.ComputeHash(hashInputBytes);

			var expectedLDPasswordValue = BitConverter.ToString(hashedSecretData);
			licHeader.Database.LD_Password = expectedLDPasswordValue;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			licCompany.LC_OH = org.PK;
			licEnterprise.LE_OH = org.PK;
			licHeader.Database.LD_OH_WebAccessOrg = org.PK;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Peregrin Tuk";
			Factory.Save();
			var xsdContact = new Xsd.OrgContact { Name = "Peregrin Tuk" };
			var request = new HttpRequestMessage(HttpMethod.Post, "https://cargowise/api/PortalAuth/AutoLoginToken");
			request.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("licence", "_X1_X2_X3"), new KeyValuePair<string, string>("contact", ValueObjectEncoder.Serialize(xsdContact)), new KeyValuePair<string, string>("dbnumber", licHeader.Database.LD_DatabaseNumber.ToString()), new KeyValuePair<string, string>("baseUrl", "https://cargowise/eLearning.aspx"), new KeyValuePair<string, string>("password", "myPassword") });
			request.Headers.Accept.Clear();
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			using (request)
			using (var response = Execute(request))
			{
				var responseMessage = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				CombineAssertions(() =>
				{
					AssertEquals("Token creation fails so bad request should be returned", HttpStatusCode.BadRequest, response.StatusCode);
					AssertContains(ContactImportResult.NoEmailAddressErrorMessage, responseMessage);
				});
			}
		}

		public void TestGetAutoLoginToken_UseInvalidPassword_ShouldReturnUnauthorized()
		{
			CreateLicence();
			var licEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			licEnterprise.LE_EnterpriseCode = "_X1";
			var licCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licCompany.LC_LE = licEnterprise.PK;
			licCompany.LC_CompanyCode = "_X2";
			var licDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licDatabase.LD_LE = licEnterprise.PK;
			licDatabase.LD_ServerCode = "_X3";
			var licHeader = Factory.NewWithValidTestData<LicenceHeader>();
			licHeader.LA_LC = licCompany.PK;
			licHeader.LA_LD = licDatabase.PK;
			Factory.Save();
			var hashInputString = string.Format(CultureInfo.InvariantCulture, "{0}{1}", licHeader.Database.LD_DatabaseNumber, "myPassword");
			var hashInputBytes = Encoding.UTF8.GetBytes(hashInputString);
			byte[] hashedSecretData;
			using var sha512 = SHA512.Create();
			hashedSecretData = sha512.ComputeHash(hashInputBytes);
			var expectedLDPasswordValue = BitConverter.ToString(hashedSecretData);
			licHeader.Database.LD_Password = expectedLDPasswordValue;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			licCompany.LC_OH = org.PK;
			Factory.Save();
			var xsdContact = new Xsd.OrgContact();
			xsdContact.EmailAddress = "some@mail.com";
			xsdContact.Name = "Some guy";
			var request = new HttpRequestMessage(HttpMethod.Post, "https://cargowise/api/PortalAuth/AutoLoginToken");
			var originalUrl = "https://cargowise.app/eLearning.aspx";
			request.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("license", "_X1_X2_X3"), new KeyValuePair<string, string>("contact", ValueObjectEncoder.Serialize(xsdContact)), new KeyValuePair<string, string>("dbnumber", licHeader.Database.LD_DatabaseNumber.ToString()), new KeyValuePair<string, string>("baseUrl", originalUrl), new KeyValuePair<string, string>("password", "notMyPassword") });
			request.Headers.Accept.Clear();
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			using (request)
			using (var response = Execute(request))
			{
				var responseMessage = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				CombineAssertions(() =>
				{
					AssertEquals("Auto login should be denied", HttpStatusCode.Unauthorized, response.StatusCode);
					AssertContains("Auto login is not allowed for this user", responseMessage);
				});
			}
		}

		public void TestGetAutoLoginToken_UseValidTestdata_ShouldReturnToken()
		{
			LicenceHeader licHeader;
			SHA512 sha512;
			OrgHeader org;
			string token;
			HttpRequestMessage request;
			SetupValidTestToken(out licHeader, out sha512, out org, out request);
			Uri responseUri;
			CombineAssertions(() =>
			{
				using (request)
				using (var response = Execute(request))
				{
					AssertEquals("The endpoint should have returned an ok.", HttpStatusCode.OK, response.StatusCode);
					responseUri = new Uri(CleanResponse(response.Content.ReadAsStringAsync().Result));
					var queryParameters = UriExtensions.ParseQueryString(responseUri);
					token = queryParameters["token"];

					var statement = string.Format(CultureInfo.InvariantCulture, @"
Select contact.OC_ContactName, token.SAT_ParentTableCode From 
dbo.StmAccessToken as token join dbo.OrgContact as contact on token.SAT_ParentId = contact.OC_PK
Where token.SAT_Token = '{0}'", token);
					using (var command = Db.Connection.Command(statement))
					using (var reader = command.ExecuteReader())
					{
						reader.Read();
						var result = reader.GetValue(0);
						var parentTableCode = reader.GetValue(1);
						AssertEquals("The returned url should contain the server url and the token.", CleanResponse(response.Content.ReadAsStringAsync().Result), string.Format(CultureInfo.InvariantCulture, "https://cargowise/Login/AutoLogin.aspx?token={0}", token));
						AssertEquals("The token is marked with correct parentTableCode.", "OC", parentTableCode);
						AssertEquals("The token should be linked to the user.", result, "Peregrin Tuk");
					}
				}
			});
		}

		void SetupValidTestToken(out LicenceHeader licHeader, out SHA512 sha512, out OrgHeader org, out HttpRequestMessage request)
		{
			CreateLicence();
			var licEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			licEnterprise.LE_EnterpriseCode = "_X1";
			var licCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licCompany.LC_LE = licEnterprise.PK;
			licCompany.LC_CompanyCode = "_X2";
			var licDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licDatabase.LD_LE = licEnterprise.PK;
			licDatabase.LD_ServerCode = "_X3";
			licHeader = Factory.NewWithValidTestData<LicenceHeader>();
			licHeader.LA_LC = licCompany.PK;
			licHeader.LA_LD = licDatabase.PK;
			Factory.Save();
			var hashInputString = string.Format(CultureInfo.InvariantCulture, "{0}{1}", licHeader.Database.LD_DatabaseNumber, "myPassword");
			var hashInputBytes = Encoding.UTF8.GetBytes(hashInputString);
			byte[] hashedSecretData;
			sha512 = SHA512.Create();
			hashedSecretData = sha512.ComputeHash(hashInputBytes);

			var expectedLDPasswordValue = BitConverter.ToString(hashedSecretData);
			licHeader.Database.LD_Password = expectedLDPasswordValue;
			org = Factory.NewWithValidTestData<OrgHeader>();
			licCompany.LC_OH = org.PK;
			licEnterprise.LE_OH = org.PK;
			licHeader.Database.LD_OH_WebAccessOrg = org.PK;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Peregrin Tuk";
			contact.OC_Email = "pippin@shireweb.com";
			Factory.Save();
			var xsdContact = new Xsd.OrgContact { EmailAddress = "pippin@shireweb.com", Name = "Peregrin Tuk" };
			request = new HttpRequestMessage(HttpMethod.Post, "https://cargowise/api/PortalAuth/AutoLoginToken");
			request.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("licence", "_X1_X2_X3"), new KeyValuePair<string, string>("contact", ValueObjectEncoder.Serialize(xsdContact)), new KeyValuePair<string, string>("dbnumber", licHeader.Database.LD_DatabaseNumber.ToString()), new KeyValuePair<string, string>("baseUrl", "https://cargowise/eLearning.aspx"), new KeyValuePair<string, string>("password", "myPassword") });
			request.Headers.Accept.Clear();
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public void TestGetAutoLoginToken_UseValidTestdata_ShouldReturnTokenWithDatabaseNumber()
		{
			LicenceHeader licHeader;
			SHA512 sha512;
			OrgHeader org;
			string token;
			HttpRequestMessage request;
			SetupValidTestToken(out licHeader, out sha512, out org, out request);
			Uri responseUri;
			CombineAssertions(() =>
			{
				using (request)
				using (var response = Execute(request))
				{
					AssertEquals("The endpoint should have returned an ok.", HttpStatusCode.OK, response.StatusCode);
					responseUri = new Uri(CleanResponse(response.Content.ReadAsStringAsync().Result));
					var queryParameters = UriExtensions.ParseQueryString(responseUri);
					token = queryParameters["token"];
					ITokenizedAccessControl accesscontrol = new TokenizedAccessControl();
					var consumeResult = accesscontrol.TryConsume(token, AccessTokenTypes.MyAccountAutoLogin, out AccessTokenInfo info);
					var scope = AutoLoginHelper.DeserializeFromXml(info.Scope);
					AssertEquals("The token should contain the correct LD_DatabaseNumber.", licHeader.Database.LD_DatabaseNumber.ToString(), scope.DatabaseNumber);
					AssertEquals("The token should contain the correct ReturnUrl.", "https://cargowise/eLearning.aspx", scope.ReturnUrl);
					AssertEquals("The token should contain the correct OrgCode.", org.OH_Code, scope.OrgCode);
				}
			});
		}

		public void TestGetAutoLoginToken_UseValidTestdata_WhenUrlIsMyOwnGlowInstance_ShouldReturnGlowSSOToken()
		{
			RunValidTestDataWithOwnGlowInstance("https://unit-testing/GlowPortal/TW3", "https://unit-testing/GlowPortal/TW3?sso_otp={token}");
		}

		public void TestGetAutoLoginToken_UseValidTestdata_WhenUrlIsMyOwnGlowInstance_WhenUrlAlreadyHasSsoOtp_ShouldReturnNewGlowSSOToken()
		{
			RunValidTestDataWithOwnGlowInstance("https://unit-testing/GlowPortal/TW3?sso_otp=myFakeToken", "https://unit-testing/GlowPortal/TW3?sso_otp={token}");
		}

		public void TestGetAutoLoginToken_UseValidTestdata_WhenUrlIsMyOwnGlowInstance_WhenUrlAlreadyHasQueryStringButNotSsoOtp_ShouldReturnGlowSSOToken()
		{
			RunValidTestDataWithOwnGlowInstance("https://unit-testing/GlowPortal/TW3?myExtraParam=213", "https://unit-testing/GlowPortal/TW3?myExtraParam=213&sso_otp={token}");
		}

		public void TestGetAutoLoginToken_UseValidTestdata_WhenUrlIsMyOwnGlowInstance_WhenUrlAlreadyHasUrlFragment_ShouldReturnGlowSSOToken()
		{
			RunValidTestDataWithOwnGlowInstance("https://unit-testing/GlowPortal/TW3#Desktop", "https://unit-testing/GlowPortal/TW3?sso_otp={token}#Desktop");
		}

		public void TestGetAutoLoginToken_UseValidTestdata_WhenUrlIsMyOwnGlowInstance_WhenUrlAlreadyHasQueryStringAndUrlFragmentButNoSsoOtp_ShouldReturnGlowSSOToken()
		{
			RunValidTestDataWithOwnGlowInstance("https://unit-testing/GlowPortal/TW3#Desktop?extraParam=213", "https://unit-testing/GlowPortal/TW3?sso_otp={token}#Desktop?extraParam=213");
		}

		#endregion

		#region Implementation

		LicenceHeader LicHeaderForTest;
		OrgContact ContactForTest;
		string ClientSecret;
		SecureQueryString StaffQueryString;
		Xsd.OrgContact ContactXsd;

		void RunValidTestDataWithOwnGlowInstance(string originalUrl, string expectedUrl)
		{
			CreateLicence();
			var licEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			licEnterprise.LE_EnterpriseCode = "_X1";
			var licCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licCompany.LC_LE = licEnterprise.PK;
			licCompany.LC_CompanyCode = "_X2";
			var licDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licDatabase.LD_LE = licEnterprise.PK;
			licDatabase.LD_ServerCode = "_X3";
			var licHeader = Factory.NewWithValidTestData<LicenceHeader>();
			licHeader.LA_LC = licCompany.PK;
			licHeader.LA_LD = licDatabase.PK;
			Factory.Save();
			var hashInputString = string.Format(CultureInfo.InvariantCulture, "{0}{1}", licHeader.Database.LD_DatabaseNumber, "myPassword");
			var hashInputBytes = Encoding.UTF8.GetBytes(hashInputString);
			byte[] hashedSecretData;
			using var sha512 = SHA512.Create();
			hashedSecretData = sha512.ComputeHash(hashInputBytes);

			var expectedLDPasswordValue = BitConverter.ToString(hashedSecretData);
			licHeader.Database.LD_Password = expectedLDPasswordValue;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			licCompany.LC_OH = org.PK;
			licEnterprise.LE_OH = org.PK;
			licHeader.Database.LD_OH_WebAccessOrg = org.PK;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Peregrin Tuk";
			contact.OC_Email = "pippin@shireweb.com";
			Factory.Save();
			var xsdContact = new Xsd.OrgContact { EmailAddress = "pippin@shireweb.com", Name = "Peregrin Tuk" };
			string token;
			var request = new HttpRequestMessage(HttpMethod.Post, "https://cargowise/api/PortalAuth/AutoLoginToken");
			request.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("licence", "_X1_X2_X3"), new KeyValuePair<string, string>("contact", ValueObjectEncoder.Serialize(xsdContact)), new KeyValuePair<string, string>("dbnumber", licHeader.Database.LD_DatabaseNumber.ToString()), new KeyValuePair<string, string>("baseUrl", originalUrl), new KeyValuePair<string, string>("password", "myPassword") });
			request.Headers.Accept.Clear();
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			CombineAssertions(() =>
			{
				using (request)
				using (var response = Execute(request))
				{
					AssertEquals(HttpStatusCode.OK, response.StatusCode);
					var autoLoginUrl = CleanResponse(response.Content.ReadAsStringAsync().Result);
					AssertContains("GlowPortalAutoLogin.aspx?qdata=", autoLoginUrl);
					var glowUrl = MyAccountLoginRouterTest.ExtractGlowUrl(new Uri(autoLoginUrl));
					var resultUri = new Uri(CleanResponse(glowUrl));
					var queryParameters = UriExtensions.ParseQueryString(resultUri);
					token = queryParameters[ServerOnlyQueryStringKeys.SSOKey];

					AssertNotNullOrEmpty(token);
					AssertEquals(expectedUrl.Replace("{token}", token), resultUri.ToString());
				}
			});
		}

		static string CleanResponse(string responseString) => responseString.Trim('"');

		static HttpRequestMessage CreateRequest(SecureQueryString queryString, string landingPageId, string module, string subModule, string referenceId)
		{
			queryString[QueryStringKeys.LandingPageId] = landingPageId;
			queryString[QueryStringKeys.Module] = module;
			queryString[QueryStringKeys.SubModule] = subModule;
			queryString[QueryStringKeys.ReferenceId] = referenceId;
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/api/PortalAuth/AutoLoginUrl?" + SecureQueryString.QueryStringKey + "=" + WebUtility.UrlEncode(queryString.ToString()));
			return request;
		}

		void CreateLicence()
		{
			LicHeaderForTest = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			ContactForTest = LicHeaderForTest.Company.Header.Contacts.AddNew();
			ContactForTest.OC_ContactName = "Your Name";
			ContactForTest.OC_Email = "youremail@test.com";
			Factory.Save();
			var db = LicHeaderForTest.Database;
			ClientSecret = "123456";
			var hashInputString = string.Format(CultureInfo.InvariantCulture, "{0}{1}", db.LD_DatabaseNumber, ClientSecret);
			var hashInputBytes = Encoding.UTF8.GetBytes(hashInputString);
			byte[] hashedSecretData;
			using var sha512 = SHA512.Create();
			hashedSecretData = sha512.ComputeHash(hashInputBytes);

			var expectedLDPasswordValue = BitConverter.ToString(hashedSecretData);
			LicHeaderForTest.Database.LD_Password = expectedLDPasswordValue;
			Factory.Save();
			ContactXsd = new Xsd.OrgContact();
			ContactXsd.Name = ContactForTest.OC_ContactName;
			ContactXsd.EmailAddress = ContactForTest.OC_Email;
			StaffQueryString = ContactToSecuredQueryString(ContactXsd, LicHeaderForTest);
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");
			EDIDataRegistry.Instance.GlowNewERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "eRequestPortal#/workflow");
		}

		SecureQueryString ContactToSecuredQueryString(Xsd.OrgContact contact, LicenceHeader lic)
		{
			SecureQueryString queryString = new SecureQueryString();
			queryString[QueryStringKeys.LicenceCode] = lic.LicenceCode;
			queryString[QueryStringKeys.DatabaseNumber] = lic.Database.LD_DatabaseNumber.ToString();
			queryString[QueryStringKeys.ContactData] = ValueObjectEncoder.Serialize(contact);
			queryString[QueryStringKeys.Password] = ClientSecret;
			return queryString;
		}

		HttpResponseMessage Execute(HttpRequestMessage request)
		{
			HttpResponseMessage response;
			using (var controller = new PortalAuthController(new Lazy<ITokenizedAccessControl>(() => new TokenizedAccessControl())))
			{
				response = ControllerTestHelper.Execute(controller, request);
			}

			return response;
		}
		#endregion
	}
}
