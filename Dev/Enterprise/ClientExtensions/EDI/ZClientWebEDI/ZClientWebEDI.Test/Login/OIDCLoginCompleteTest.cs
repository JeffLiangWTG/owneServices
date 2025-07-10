using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Web;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZClientWebCargoWiseEDI.OIDC;
using NUnit.Framework;
using WTG.OAuth2.Token.TestFramework;
using WTG.OpenIDConnect.Token;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(OIDCLoginComplete))]
	[HttpContextEnabledTest]
	[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
	public class OIDCLoginCompleteTest : TestCaseWithFactory
	{
		void SetupCookie(string email, string organisationCode, string returlUrl, string nonce = "nonce", string verifier = "verifier")
		{
			var userData = new UserCredentialData(email, organisationCode, returlUrl);
			var cookie = new HttpCookie(OIDCLoginHelper.Constants.OIDCCookieName, new OIDCAuthUserCookieData(verifier, nonce, userData).ToString());
			Page.Request.Cookies.Add(cookie);
		}

		public void TestFindUserAndRedirect()
		{
			Page.Request.QueryString.Add(OIDCLoginHelper.Constants.CodeKey, "123456789");
			Page.Request.QueryString.Add(OIDCLoginHelper.Constants.StateKey, "/myaccount/default.aspx");

			var (staff, contact) = CreateIDPIncludedUser();
			SetupCookie(staff.GS_EmailAddress, "", "/myaccount/default.aspx");
			IDPServer.Claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, "nonce"));

			contact.OC_OH = OIDCOrg.PK;
			Factory.Save();
			Page.DoPageLoad();
			AssertNotContains("User should be not redirected to OIDCLogin", "/Login/LoginLite.aspx", Page.Response.RedirectLocation);
		}

		public void TestFindUserAndRedirect_LiteViewModeUrl()
		{
			var returnUrl = "/myaccount/admin/switchcompany.aspx";
			EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://dummy.local/");
			Page.Request.QueryString.Add(OIDCLoginHelper.Constants.CodeKey, "123456789");
			Page.Request.QueryString.Add(OIDCLoginHelper.Constants.StateKey, returnUrl);

			var (staff, contact) = CreateIDPIncludedUser();
			staff.Person.PER_EmailAddress = staff.GS_EmailAddress;
			contact.SetHashedPassword("test");
			SetupCookie(staff.GS_EmailAddress, "", returnUrl);
			IDPServer.Claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, "nonce"));

			contact.OC_OH = OIDCOrg.PK;
			Factory.Save();
			Page.DoPageLoad();

			var url = new UriDeconstructor(new Uri(Page.Response.RedirectLocation, UriKind.Relative));
			AssertEndsWith("The redirect url should be end with LoginComplete", "/LoginComplete.aspx", url.BaseUrl);

			var completeQuery = new QueryString(url.Query);
			var decodeQueryString = new SecureQueryString(WebUtility.UrlDecode(completeQuery["udata"]));
			var originalUrl = decodeQueryString["OriginalUrl"];

			var expectedUrl = EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.Value.Trim('/') + returnUrl;
			AssertEquals("User should be not redirected to lite view site", expectedUrl, originalUrl);
		}

		public void TestFindUserAndRedirect_NotFoundOIDCContact()
		{
			Page.Request.QueryString.Add(OIDCLoginHelper.Constants.CodeKey, "123456789");
			Page.Request.QueryString.Add(OIDCLoginHelper.Constants.StateKey, "/myaccount/default.aspx");

			var (staff, contact) = CreateIDPIncludedUser();
			SetupCookie(staff.GS_EmailAddress, "", "/myaccount/default.aspx");
			IDPServer.Claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, "nonce"));
			var tokenString = IDPServer.GenerateIdentityToken(ZDateTime.UtcNow.AddDays(1).ToDateTime());
			var token = TokenValidator.ReadJwtToken(tokenString, new TokenValidationLogger(new LoggerForTest()));
			Assert(token.Claims.Any(x => x.Type == JwtRegisteredClaimNames.Nonce));

			var nonOIDCOrg = Factory.NewWithValidTestData<OrgHeader>();
			nonOIDCOrg.OH_Code = "nonorg";
			contact.OC_OH = nonOIDCOrg.PK;
			Factory.Save();

			Page.DoPageLoad();
			AssertNotEquals(OIDCOrg.PK, contact.OC_OH);
			AssertContains("User should be redirected to OIDCLogin because of belonging non-oidc org", "/Login/LoginV2.aspx", Page.Response.RedirectLocation);
		}

		public void TestFindUserAndRedirect_RedirectToChooseCompany()
		{
			Page.Request.QueryString.Add(OIDCLoginHelper.Constants.CodeKey, "123456789");

			var (staff, contact) = CreateIDPIncludedUser();
			SetupCookie(staff.GS_EmailAddress, "", "/myaccount/default.aspx");
			IDPServer.Claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, "nonce"));

			contact.OC_OH = OIDCOrg.PK;
			var contact2 = Factory.NewWithValidTestData<EDIOrgContact>();
			contact2.OC_Email = staff.GS_EmailAddress;
			contact2.OC_PER = staff.GS_PER;
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_IsActive = true;
			contact2.OC_OH = OIDCOrg2.PK;
			Factory.Save();

			Page.DoPageLoad();
			var query = new ZQuery(OrgContactSchema.OC_PER, staff.GS_PER)
					.AddToFilter(OrgContactSchema.OC_IsActive, true)
					.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, true);
			AssertEquals(2, Factory.Load<OrgContact>(query).Length);
			AssertContains("User should be redirected to ChooseCompany page", "Login/ChooseCompany.aspx", Page.Response.RedirectLocation);
			//var query = WebUtility.UrlDecode(Page.Response.RedirectLocation.Split('?')[1]);
		}

		public void TestCheckUserAndIDPData()
		{
			var state = "/myaccount/abc.aspx";
			var nonce = "nonceValue";
			var (verifier, challenge) = OIDCLoginHelper.GenerateOIDCProofKeyPair();
			var (staff, contact) = CreateIDPIncludedUser();

			IDPServer.Claims.Add(new Claim(OIDCLoginHelper.Constants.NonceKey, nonce));
			SetupCookie(staff.GS_EmailAddress, string.Empty, state, nonce, verifier);

			Page.Request.QueryString.Add(OIDCLoginHelper.Constants.CodeKey, "123456789");

			Page.DoPageLoad();

			AssertNotContains("/Login/LoginV2.aspx", Page.Response.RedirectLocation);
			AssertNotContains("/Login/OIDCLite.aspx", Page.Response.RedirectLocation);
			AssertNotContains("PasswordForLogin.aspx", Page.Response.RedirectLocation);
		}

		public void TestCheckUserAndIDPData_IDPReturnsError()
		{
			Page.Request.QueryString.Add("error", "error");
			Page.Request.QueryString.Add("error_description", "description");

			Page.DoPageLoad();

			var uri = Page.Response.RedirectLocation;
			AssertContains("If IDP returned an error, myAccount should redirect to OIDCLogin page", "/Login/LoginV2.aspx", uri);

			AssertContains("If IDP returned an error, page should show prompt message", "Unexpected response", new SecureQueryString(WebUtility.UrlDecode(uri.Split(new string[] { "data=" }, StringSplitOptions.None)[1]))["message"]);
			Assert("If IDP returned an error, a log should be added", Logger.LogEntries.Any(x => x.Contains("MyAccount OIDC | Received a error form | error | description")));
		}

		public void TestCheckUserAndIDPData_IDPMissedCode()
		{
			var state = "/myaccount/abc.aspx";
			var nonce = "nonceValue";
			var (verifier, challenge) = OIDCLoginHelper.GenerateOIDCProofKeyPair();
			var cookie = new HttpCookie(OIDCLoginHelper.Constants.OIDCCookieName, new OIDCAuthUserCookieData(state, verifier, nonce, "123@123.com").ToString());

			Page.Request.Cookies.Add(cookie);
			Page.Request.QueryString.Add("state", state);
			Page.DoPageLoad();

			AssertContains("/Login/LoginV2.aspx", Page.Response.RedirectLocation);
		}

		public void TestCheckUserCookie_MissingVerifier()
		{
			var state = "/myaccount/abc.aspx";
			var nonce = "nonceValue";
			SetupCookie("123@123.com", OIDCOrg.OH_Code, state, nonce, string.Empty);
			Page.Request.QueryString.Add(OIDCLoginHelper.Constants.CodeKey, "123456789");
			Page.Request.QueryString.Add(OIDCLoginHelper.Constants.StateKey, state);

			Page.DoPageLoad();

			AssertContains("/Login/LoginV2.aspx", Page.Response.RedirectLocation);
			AssertContains("The certificate has expired", new SecureQueryString(WebUtility.UrlDecode(Page.Response.RedirectLocation.Split(new string[] { "data=" }, StringSplitOptions.None)[1]))["message"]);
		}

		public void TestCheckUserCookie_MissingNonce()
		{
			var state = "/myaccount/abc.aspx";
			var (verifier, challenge) = OIDCLoginHelper.GenerateOIDCProofKeyPair();
			SetupCookie("123@123.com", OIDCOrg.OH_Code, state, string.Empty, verifier);
			Page.Request.QueryString.Add(OIDCLoginHelper.Constants.CodeKey, "123456789");
			Page.Request.QueryString.Add(OIDCLoginHelper.Constants.StateKey, state);

			Page.DoPageLoad();

			AssertContains("/Login/LoginV2.aspx", Page.Response.RedirectLocation);
			AssertContains("The certificate has expired", new SecureQueryString(WebUtility.UrlDecode(Page.Response.RedirectLocation.Split(new string[] { "data=" }, StringSplitOptions.None)[1]))["message"]);
		}

		public void TestCheckUserCookie_InvalidCookie()
		{
			var state = "/myaccount/abc.aspx";
			var nonce = "nonceValue";
			var (verifier, challenge) = OIDCLoginHelper.GenerateOIDCProofKeyPair();
			var userData = new UserCredentialData("123@123.com", OIDCOrg.OH_Code, state);
			var cookie = new HttpCookie(OIDCLoginHelper.Constants.OIDCCookieName, new OIDCAuthUserCookieData(verifier, nonce, userData).ToString() + "!@#@$#$%$^%$&");

			Page.Request.Cookies.Add(cookie);
			Page.Request.QueryString.Add(OIDCLoginHelper.Constants.CodeKey, "123456789");
			Page.Request.QueryString.Add(OIDCLoginHelper.Constants.StateKey, state);

			Page.DoPageLoad();

			AssertContains("/Login/LoginV2.aspx", Page.Response.RedirectLocation);
			AssertContains("Invalid session. Please try again.", new SecureQueryString(WebUtility.UrlDecode(Page.Response.RedirectLocation.Split(new string[] { "data=" }, StringSplitOptions.None)[1]))["message"]);
			Assert("There should be a log of invalid cookies", Logger.LogEntries.Any(x => x.Contains("MyAccount OIDC | The cookie is invalid")));
		}

		public void TestCheckUserCookie_WhenNoOIDCCookieName()
		{
			var state = "/myaccount/abc.aspx";
			var nonce = "nonceValue";
			var userData = new UserCredentialData("123@123.com", OIDCOrg.OH_Code, state);
			var cookie = new HttpCookie(OIDCLoginHelper.Constants.ErrorKey, new OIDCAuthUserCookieData(string.Empty, nonce, userData).ToString());

			Page.Request.Cookies.Add(cookie);
			Page.Request.QueryString.Add(OIDCLoginHelper.Constants.CodeKey, "123456789");
			Page.Request.QueryString.Add(OIDCLoginHelper.Constants.StateKey, state);

			Page.DoPageLoad();

			AssertContains("/Login/LoginV2.aspx", Page.Response.RedirectLocation);
			AssertContains("Invalid session. Please try again.", new SecureQueryString(WebUtility.UrlDecode(Page.Response.RedirectLocation.Split(new string[] { "data=" }, StringSplitOptions.None)[1]))["message"]);
			Assert("There should be a log of invalid cookies", Logger.LogEntries.Any(x => x.Contains("MyAccount OIDC | The cookie is invalid")));
		}

		EDIOrgHeader OIDCOrg;
		EDIOrgHeader OIDCOrg2;

		readonly NLogWrapperForTest Logger = new NLogWrapperForTest(typeof(OIDCLoginCompleteTest));

		(EDIGlbStaff, EDIOrgContact) CreateIDPIncludedUser()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_EmailAddress = "123@oidc.com";
			staff.GS_LoginName = "oidcStaff";
			Factory.Save();

			var person = staff.Person;
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			contact.OC_Email = staff.GS_EmailAddress;
			contact.OC_PER = person.PK;
			contact.OC_WebAccessEnabled = true;
			contact.OC_IsActive = true;
			contact.OC_OH = OIDCOrg.PK;
			Factory.Save();

			return (staff, contact);
		}

		MockOpenIDIdentityServer IDPServer
		{
			get
			{
				if (idpServer == null)
				{
					idpServer = new MockOpenIDIdentityServer
					{
						ClientIdentifier = "MyAccount",
						Claims = new List<Claim>
				{
					new Claim("company_code", "WTG"),
					new Claim("user_name", "oidcStaff"),
				},
					};
					idpServer.ClientIdentifier = ClientID;
				}

				return idpServer;
			}
		}

		MockOpenIDIdentityServer idpServer;
		const string ClientID = "0ce6653e-3deb-457d-aaf1-77e05e7bab52";

		protected override void SetUp()
		{
			TransactionedTestCase.RunClientDbCreateScripts();
			OIDCOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
			OIDCOrg2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			OIDCOrg2.OH_Code = "org2";
			Factory.Save();

			EDIDataRegistry.Instance.RedirectedEmailDomains.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "@oidc.com" });
			EDIDataRegistry.Instance.RedirectedOrganisations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { OIDCOrg.PK.ToGuid(), OIDCOrg2.PK.ToGuid() });
			EDIDataRegistry.Instance.MyAccountQueryOIDCClientID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ClientID);
			EDIDataRegistry.Instance.MyAccountEndpointBaseUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://dummyy-account.local");
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = false,
				OIDCServerType = OIDCServerTypes.Azure,
				AuthorityURL = $"https://{MockIdentityServerBase.HostName}:{IDPServer.Port}",
				ClientIdentifier = "CW1DesktopID"
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping() { ClaimName = "user_name", Identifier = "GlbStaff.GS_LoginName" });
			oidcConfig.Scopes.Add(new OIDCScope() { ScopeName = "testid" });
			oidcConfig.IsVerified = true;
			SystemDataRegistry.Instance.OIDCConfig.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig);
			Assert(OIDCLoginHelper.IsOIDCReady());
			base.SetUp();
		}

		protected override void TearDown()
		{
			IDPServer.Dispose();
			base.TearDown();
		}

		OIDCLoginCompleteForTest Page
		{
			get
			{
				if (page == null)
				{
					page = new OIDCLoginCompleteForTest();
					page.SetLogger(Logger);
					var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
					method.Invoke(page, new object[] { HttpContext.Current });
				}

				return page;
			}
		}
		OIDCLoginCompleteForTest page;
	}

	class OIDCLoginCompleteForTest : OIDCLoginComplete
	{
		public void DoPageLoad()
		{
			try
			{
				base.OnLoad(EventArgs.Empty);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (ex is QueryStringException || ex is NullReferenceException)
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

		public void SetLogger(NLogWrapperForTest logger)
		{
			base.Logger = logger;
		}

		public HttpRequest RequestForTest => base.Request;
	}
}
