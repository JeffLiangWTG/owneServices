using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI.Gateway.Test
{
	[HttpContextEnabledTest]
	class MyAccountGatewayLoginControllerTestCase : TestCaseWithFactory
	{
		#region /gateway/authorize_via_login_router
		public void TestAuthorize_InvalidIP()
		{
			var supersededContact = Factory.NewWithValidTestData<OrgContact>();
			supersededContact.OC_WebAccessEnabled = true;
			supersededContact.OC_Email = "howdy@partner.com";
			supersededContact.SupersedeWebAccess();
			supersededContact.SetHashedPassword("1234");
			Factory.Save();
			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_WebAccessEnabled = true;
			activeContact.OC_Email = "howdy@partner.com";
			activeContact.OC_PER = supersededContact.OC_PER;
			activeContact.SetHashedPassword("1234");
			Factory.Save();
			const string nonce = "r3nid9s";
			string hashString;
			using (HMACSHA256 hmac = new HMACSHA256(EDIDataRegistry.Instance.MyAccountGatewaySecret.DataType.Serialise(EDIDataRegistry.Instance.MyAccountGatewaySecret.Value)))
			{
				var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(supersededContact.PK + nonce));
				hashString = Convert.ToBase64String(hash);
			}

			var callback = "ab://callback/here";
			var state = "thisisthestate";
			var controller = new MyAccountGatewayLoginControllerForTest(false);
			using (var request = new HttpRequestMessage(HttpMethod.Get, FormattableString.Invariant($"http://unit-testing/gateway/authorize_via_login_router?contact_pk={supersededContact.PK}&nonce={nonce}&hash={WebUtility.UrlEncode(hashString)}&redirect_uri={Uri.EscapeDataString(callback)}&state={state}")))
			{
				SetupControllerContext(controller, request);
				var response = controller.Authorize(supersededContact.PK.ToString(), nonce, hashString, callback, state).ExecuteAsync(CancellationToken.None).Result;
				AssertEquals("Invalid IP", HttpStatusCode.BadRequest, response.StatusCode);
			}
		}

		public void TestAuthorize_InvalidContact()
		{
			var fakeContactPK = new Guid();
			const string nonce = "r3nid9s";
			string hashString;
			using (HMACSHA256 hmac = new HMACSHA256(EDIDataRegistry.Instance.MyAccountGatewaySecret.DataType.Serialise(EDIDataRegistry.Instance.MyAccountGatewaySecret.Value)))
			{
				var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(fakeContactPK + nonce));
				hashString = Convert.ToBase64String(hash);
			}

			var callback = "ab://callback/here";
			var state = "thisisthestate";
			var controller = new MyAccountGatewayLoginControllerForTest();
			using (var request = new HttpRequestMessage(HttpMethod.Get, FormattableString.Invariant($"http://unit-testing/gateway/authorize_via_login_router?contact_pk={fakeContactPK}&nonce={nonce}&hash={WebUtility.UrlEncode(hashString)}&redirect_uri={Uri.EscapeDataString(callback)}&state={state}")))
			{
				SetupControllerContext(controller, request);
				var response = controller.Authorize(fakeContactPK.ToString(), nonce, hashString, callback, state).ExecuteAsync(CancellationToken.None).Result;
				AssertEquals("Contact does not exist", HttpStatusCode.BadRequest, response.StatusCode);
			}
		}

		public void TestAuthorize_InvalidState()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "howdy@partner.com";
			contact.SupersedeWebAccess();
			contact.SetHashedPassword("1234");
			Factory.Save();
			const string nonce = "r3nid9s";
			string hashString;
			using (HMACSHA256 hmac = new HMACSHA256(EDIDataRegistry.Instance.MyAccountGatewaySecret.DataType.Serialise(EDIDataRegistry.Instance.MyAccountGatewaySecret.Value)))
			{
				var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(contact.PK + nonce));
				hashString = Convert.ToBase64String(hash);
			}

			var callback = "ab://callback/here";
			var controller = new MyAccountGatewayLoginControllerForTest();
			using (var request = new HttpRequestMessage(HttpMethod.Get, FormattableString.Invariant($"http://unit-testing/gateway/authorize_via_login_router?contact_pk={contact.PK}&nonce={nonce}&hash={WebUtility.UrlEncode(hashString)}&redirect_uri={Uri.EscapeDataString(callback)}")))
			{
				SetupControllerContext(controller, request);
				var response = controller.Authorize(contact.PK.ToString(), nonce, hashString, callback, string.Empty).ExecuteAsync(CancellationToken.None).Result;
				AssertEquals("Empty state should return bad request", HttpStatusCode.BadRequest, response.StatusCode);
			}
		}

		public void TestAuthorize_InvalidContactHash()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_Email = "howdy@partner.com";
			contact1.SupersedeWebAccess();
			contact1.SetHashedPassword("1234");
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_Email = "not@thesame.com";
			contact2.SupersedeWebAccess();
			contact2.SetHashedPassword("1234");
			Factory.Save();
			const string nonce = "r3nid9s";
			string hashString;
			using (HMACSHA256 hmac = new HMACSHA256(EDIDataRegistry.Instance.MyAccountGatewaySecret.DataType.Serialise(EDIDataRegistry.Instance.MyAccountGatewaySecret.Value)))
			{
				var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(contact1.PK + nonce));
				hashString = Convert.ToBase64String(hash);
			}

			var callback = "ab://callback/here";
			var state = "thisisthestate";
			var controller = new MyAccountGatewayLoginControllerForTest();
			using (var request = new HttpRequestMessage(HttpMethod.Get, FormattableString.Invariant($"http://unit-testing/gateway/authorize_via_login_router?contact_pk={contact2.PK}&nonce={nonce}&hash={WebUtility.UrlEncode(hashString)}&redirect_uri={Uri.EscapeDataString(callback)}&state={state}")))
			{
				SetupControllerContext(controller, request);
				var response = controller.Authorize(contact2.PK.ToString(), nonce, hashString, callback, state).ExecuteAsync(CancellationToken.None).Result;
				AssertEquals("Contact PK does not match the one which was hashed", HttpStatusCode.BadRequest, response.StatusCode);
			}
		}

		public void TestAuthorize_InvalidNonceHash()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_Email = "howdy@partner.com";
			contact1.SupersedeWebAccess();
			contact1.SetHashedPassword("1234");
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_Email = "not@thesame.com";
			contact2.SupersedeWebAccess();
			contact2.SetHashedPassword("1234");
			Factory.Save();
			const string nonce = "r3nid9s";
			string hashString;
			using (HMACSHA256 hmac = new HMACSHA256(EDIDataRegistry.Instance.MyAccountGatewaySecret.DataType.Serialise(EDIDataRegistry.Instance.MyAccountGatewaySecret.Value)))
			{
				var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(contact1.PK + "12345"));
				hashString = Convert.ToBase64String(hash);
			}

			var callback = "ab://callback/here";
			var state = "thisisthestate";
			var controller = new MyAccountGatewayLoginControllerForTest();
			using (var request = new HttpRequestMessage(HttpMethod.Get, FormattableString.Invariant($"http://unit-testing/gateway/authorize_via_login_router?contact_pk={contact1.PK}&nonce={nonce}&hash={WebUtility.UrlEncode(hashString)}&redirect_uri={Uri.EscapeDataString(callback)}&state={state}")))
			{
				SetupControllerContext(controller, request);
				var response = controller.Authorize(contact1.PK.ToString(), nonce, hashString, callback, state).ExecuteAsync(CancellationToken.None).Result;
				AssertEquals("nonce does not match the one which was hashed", HttpStatusCode.BadRequest, response.StatusCode);
			}
		}

		public void TestAuthorize_BadRedirectUri_ReturnsBadRequest()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "howdy@partner.com";
			contact.SupersedeWebAccess();
			contact.SetHashedPassword("1234");
			Factory.Save();
			const string nonce = "r3nid9s";
			string hashString;
			using (HMACSHA256 hmac = new HMACSHA256(EDIDataRegistry.Instance.MyAccountGatewaySecret.DataType.Serialise(EDIDataRegistry.Instance.MyAccountGatewaySecret.Value)))
			{
				var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(contact.PK + nonce));
				hashString = Convert.ToBase64String(hash);
			}

			var callback = "Now Where Did I Put That Absolute Uri?";
			var state = "thisisthestate";
			var controller = new MyAccountGatewayLoginControllerForTest();
			using (var request = new HttpRequestMessage(HttpMethod.Get, FormattableString.Invariant($"http://unit-testing/gateway/authorize_via_login_router?contact_pk={contact.PK}&nonce={nonce}&hash={WebUtility.UrlEncode(hashString)}&redirect_uri={Uri.EscapeDataString(callback)}&state={state}")))
			{
				SetupControllerContext(controller, request);
				var response = controller.Authorize(contact.PK.ToString(), nonce, hashString, callback, state).ExecuteAsync(CancellationToken.None).Result;
				AssertEquals("redirect_uri is invalid", HttpStatusCode.BadRequest, response.StatusCode);
			}
		}

		public void TestAuthorize_ShouldRedirectToLoginRouter()
		{
			var supersededContact = Factory.NewWithValidTestData<OrgContact>();
			supersededContact.OC_WebAccessEnabled = true;
			supersededContact.OC_Email = "howdy@partner.com";
			supersededContact.SupersedeWebAccess();
			supersededContact.SetHashedPassword("1234");
			Factory.Save();
			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_WebAccessEnabled = true;
			activeContact.OC_Email = "howdy@partner.com";
			activeContact.OC_PER = supersededContact.OC_PER;
			activeContact.SetHashedPassword("1234");
			Factory.Save();
			const string nonce = "r3nid9s";
			string hashString;
			using (HMACSHA256 hmac = new HMACSHA256(EDIDataRegistry.Instance.MyAccountGatewaySecret.DataType.Serialise(EDIDataRegistry.Instance.MyAccountGatewaySecret.Value)))
			{
				hashString = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(supersededContact.PK + nonce)));
			}

			var callback = "ab://callback/here";
			var state = "thisisthestate";
			var controller = new MyAccountGatewayLoginControllerForTest();
			using (var request = new HttpRequestMessage(HttpMethod.Get, FormattableString.Invariant($"http://unit-testing/gateway/authorize_via_login_router?contact_pk={supersededContact.PK}&nonce={nonce}&hash={WebUtility.UrlEncode(hashString)}&redirect_uri={Uri.EscapeDataString(callback)}&state={state}")))
			{
				SetupControllerContext(controller, request);
				var response = controller.Authorize(supersededContact.PK.ToString(), nonce, hashString, callback, state).ExecuteAsync(CancellationToken.None).Result;
				AssertEquals(HttpStatusCode.Found, response.StatusCode);
				var redirectUri = response.Headers.Location;
				AssertStartsWith("Should redirect to login superseded page", "https://unit-testing/webapp/Login/LoginSuperseded.aspx?udata", redirectUri.OriginalString);
				var uriDeconstructor = new UriDeconstructor(redirectUri);
				var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
				var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
				var identityToken = secureQueryString[LoginRouter.IdentityTokenQueryStringKey];
				var originalRequestUrl = secureQueryString[LoginRouter.OriginalUrlQueryStringKey];
				var tokenAccessControl = (ITokenizedAccessControl)new TokenizedAccessControl();
				AssertEquals("Access token should be valid", true, tokenAccessControl.TryPeek(identityToken, AccessTokenTypes.LoginRouterIdentity, out var info));
				AssertEquals("Token parent should be the contact", supersededContact.PK, info.ParentId);
				AssertEquals("Token should be for OrgContact", OrgContactSchema.Constants.Prefix, info.ParentTableCode);
				AssertEquals("Should contain original redirect uri", FormattableString.Invariant($"https://myaccount-portal.cargowise.com/myaccount/Login/GatewayLogin.aspx?redirect_uri={Uri.EscapeDataString(callback)}&state={state}"), originalRequestUrl);
			}
		}

		#endregion
		#region Implementation
		void SetupControllerContext(MyAccountGatewayLoginControllerForTest controller, HttpRequestMessage request)
		{
			using (var configuration = new HttpConfiguration())
			{
				configuration.MapHttpAttributeRoutes();
				configuration.EnsureInitialized();
				var requestContext = new HttpRequestContext { Configuration = configuration, Url = new UrlHelper(request) };
				request.Properties[HttpPropertyKeys.RequestContextKey] = requestContext;
				var controllerType = controller.GetType();
				var controllerDescriptor = new HttpControllerDescriptor(configuration, controllerType.Name, controllerType);
				var context = new HttpControllerContext(requestContext, request, controllerDescriptor, controller)
				{ RouteData = configuration.Routes.GetRouteData(request) };
				controller.ControllerContext = context;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var salt = new byte[] { 0x58, 0xCA, 0x63, 0x36, 0x21, 0x77, 0x4B, 0xD5, 0x9C, 0xB6, 0xCC, 0x92, 0x0A, 0x22, 0x8F, 0x2A };
			using (var derive = new Rfc2898DeriveBytes("ABC123", salt, 1000, HashAlgorithmName.SHA1))
			{
				var passPhrase = derive.GetBytes(64);
				EDIDataRegistry.Instance.MyAccountGatewaySecret.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BitConverter.ToString(passPhrase).Replace("-", string.Empty));
			}
		}

		class MyAccountGatewayLoginControllerForTest : MyAccountGatewayLoginController
		{
			public MyAccountGatewayLoginControllerForTest(bool isValidClientIp = true) : base()
			{
				IsValidClientIpOverride = isValidClientIp;
			}

			protected override bool IsValidClientIp()
			{
				return IsValidClientIpOverride;
			}

			bool IsValidClientIpOverride { get; }
		}
		#endregion
	}
}
