using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.Login.Testing
{
	[HttpContextEnabledTest]
	public class LoginRouterTest : TestCaseWithFactory
	{
		public void TestGetRoutingUrl()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Test User";
			contact.OC_Email = "tester@test.org";

			Factory.Save();

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var router = new LoginRouterForTest(new Uri("https://myaccount.com/"), token);

			Assert("Routing required", router.HasAnyRoutingRequired);
			var redirectUrl = router.GetRoutingUrl();
			AssertStartsWith("ChangeContactNameRoutingDescriptor is triggered", "https://myaccount.com/changename.aspx?token=12345&udata=", redirectUrl.AbsoluteUri);

			var queryDictionary = HttpUtility.ParseQueryString(redirectUrl.Query);
			var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			var originalUrl = secureQueryString[LoginRouter.OriginalUrlQueryStringKey];
			var tokenFromQueryString = secureQueryString[LoginRouter.IdentityTokenQueryStringKey];
			AssertEquals("https://myaccount.com/", originalUrl);
			AssertEquals(token, tokenFromQueryString);

			Assert("Routing required", router.HasAnyRoutingRequired);
			redirectUrl = router.GetRoutingUrl();
			AssertStartsWith("ChangeContactNameRoutingDescriptor is triggered again", "https://myaccount.com/changename.aspx?token=12345&udata=", redirectUrl.AbsoluteUri);

			contact.OC_ContactName = "Developer";
			Factory.Save();
			Assert("Routing required", router.HasAnyRoutingRequired);
			redirectUrl = router.GetRoutingUrl();
			AssertStartsWith("CheckContactEmailRoutingDescriptor is triggered", "https://myaccount.com/changeemail.aspx?udata=", redirectUrl.AbsoluteUri);
			AssertEquals("Routing action is performed, contact email is updated", "tester@test.org.au", contact.OC_Email);

			contact.OC_Email = "user@dev.org";
			Factory.Save();
			Assert("No more routing required", !router.HasAnyRoutingRequired);
			redirectUrl = router.GetRoutingUrl();
			AssertContains("Should redirect to LoginComplete.aspx", "/Login/LoginComplete.aspx", redirectUrl.OriginalString);
			var uriDeconstructor = new UriDeconstructor(redirectUrl);
			queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			originalUrl = secureQueryString[LoginRouter.OriginalUrlQueryStringKey];
			tokenFromQueryString = secureQueryString[LoginRouter.IdentityTokenQueryStringKey];
			AssertEquals("https://myaccount.com/", originalUrl);
			AssertEquals(token, tokenFromQueryString);
			var timeStamp = secureQueryString[SecureQueryString.TimeStampKey];
			AssertNotEquals(new DateTime(2079, 06, 06).ToString(CultureInfo.InvariantCulture.DateTimeFormat.UniversalSortableDateTimePattern, CultureInfo.InvariantCulture.DateTimeFormat), timeStamp);

			router.AddUserDefinedQueryParameter("k1", "v1");
			redirectUrl = router.GetRoutingUrl();
			uriDeconstructor = new UriDeconstructor(redirectUrl);
			queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			originalUrl = secureQueryString[LoginRouter.OriginalUrlQueryStringKey];
			tokenFromQueryString = secureQueryString[LoginRouter.IdentityTokenQueryStringKey];
			var udfValue1 = secureQueryString["UDF_k1"];
			var udfValue2 = secureQueryString["UDF_k2"];
			AssertEquals("https://myaccount.com/", originalUrl);
			AssertEquals(token, tokenFromQueryString);
			AssertEquals("v1", udfValue1);
			AssertEquals(null, udfValue2);
		}

		public virtual void TestConstructorShouldSetupSession()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Test User";
			contact.OC_Email = "tester@test.org";
			Factory.Save();

			Env.ClearUserContext();
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentUserPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentBranchPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentDepartmentPK);

			using (var global1 = new ZGlobalForTest())
			{
				new LoginRouterForTest(new Uri("https://myaccount.com/"), contact, global1);

				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
			}
		}

		class ZGlobalForTest : ZGlobal
		{
			public override string DefaultPage { get; }
		}

		public void TestGetRoutingUrl_InvalidIdentityShouldRedirectToErrorPage()
		{
			var router = GetNewLoginRouter(new Uri("https://myaccount.com/"), "Invalid Token");

			Assert("Routing required", !router.HasAnyRoutingRequired);
			var redirectUrl = router.GetRoutingUrl();
			var queryString = new SecureQueryString
			{
				["title"] = "Login Session Expired",
				["message"] = "Your login session was invalid or has expired. Please attempt to login again."
			};
			queryString.ExpireTime = TimeSpan.FromMinutes(10);

			var errorPage = GetErrorPage();
			AssertStartsWith("Should redirect to error page since token is invalid", FormattableString.Invariant($"{errorPage}?data={WebUtility.UrlEncode(queryString.ToString())}"), redirectUrl.OriginalString);
		}

		protected virtual string GetErrorPage()
		{
			return new ZGlobalForTesting().ErrorPage;
		}

		public void TestConstructors()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Test User";
			contact.OC_Email = "tester@test.org";

			Factory.Save();

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var originalUrl = "https://myaccount.com/";
			var router1 = new LoginRouterForTest(new Uri(originalUrl), token);
			var router2 = new LoginRouterForTest(new Uri(originalUrl), contact);

			AssertEquals(originalUrl, router1.OriginalUrlExposed.AbsoluteUri);
			AssertEquals(originalUrl, router2.OriginalUrlExposed.AbsoluteUri);
			AssertEquals(contact.PK, router1.ContactExposed.PK);
			AssertEquals(contact.PK, router2.ContactExposed.PK);
		}

		public void TestGetOriginalUrlFromRequest()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");

			Factory.Save();

			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();

			var originalUrl = "https://google.com.au/";
			var secureQueryString = new SecureQueryString
			{
				[LoginRouter.OriginalUrlQueryStringKey] = originalUrl
			};

			var page = GetPageForTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));

			AssertEquals(originalUrl, LoginRouter.GetOriginalUrlFromRequest(page.Request).AbsoluteUri);
		}

		public void TestUserDefinedQueryParameterValue()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Test User";
			contact.OC_Email = "tester@test.org";
			Factory.Save();

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var router = new LoginRouterForTest(new Uri("https://myaccount.com/"), token);
			router.AddUserDefinedQueryParameter("k1", "v1");
			router.AddUserDefinedQueryParameter("k2", "v2");
			var queryString = HttpUtility.ParseQueryString(router.GetRoutingUrl().Query);
			AssertEquals("v1", LoginRouter.GetUserDefinedQueryParameterValue(queryString, "k1"));
			AssertEquals("v2", LoginRouter.GetUserDefinedQueryParameterValue(queryString, "k2"));
			AssertEquals(null, LoginRouter.GetUserDefinedQueryParameterValue(queryString, "k3"));
		}

		public void TestGetIdentityTokenFromRequest()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");

			Factory.Save();

			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString
			{
				[LoginRouter.IdentityTokenQueryStringKey] = token
			};

			var page = GetPageForTest();
			page.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));

			AssertEquals(token, LoginRouter.GetIdentityTokenFromRequest(page.Request));
		}

		public void TestGetIdentityTokenFromSupersededRequest()
		{
			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_WebAccessEnabled = true;
			Factory.Save();

			Assert("Precondition", activeContact.OC_IsActive);
			Assert("Precondition", activeContact.OC_WebAccessEnabled);

			var supersededContact = Factory.NewWithValidTestData<OrgContact>();
			supersededContact.OC_Email = "trench@coat.com";
			supersededContact.OC_WebAccessEnabled = true;
			supersededContact.OC_PER = activeContact.OC_PER;
			supersededContact.SupersedeWebAccess();
			supersededContact.SetHashedPassword("1234");
			Assert("Precondition", supersededContact.OC_WebAccessEnabled);

			activeContact.OC_Email = "camo@mile.com";
			Factory.Save();
			Assert("Precondition", supersededContact.WebAccessSuperseded);

			var contactLoginHelper = new OrgContactSupersededHelper(supersededContact);
			var passwordBasePageUrl = new Uri("https://google.com");
			var originalUrl1 = new Uri("https://yahoo.com");
			var token = LoginRouterIdentityManager.GenerateToken(supersededContact);
			var url = contactLoginHelper.GenerateSetMasterPasswordUrl(passwordBasePageUrl, originalUrl1, token);

			var page = GetPageForTest();
			page.Request.QueryString.Add(HttpUtility.ParseQueryString(url.Query));

			AssertEquals(token, LoginRouter.GetIdentityTokenFromRequest(page.Request));
		}

		public virtual void TestDescriptors()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "tester@test.org";

			Factory.Save();

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var router = new LoginRouterForDescriptorTest(new Uri("https://myaccount.com/"), token);
			var descriptorTypes = router.RoutingDescriptors_Expose.Select(x => x.GetType()).ToArray();
			var expectedTypes = new[]
			{
				typeof(SupersededLoginRoutingDescriptor),
				typeof(PasswordRotationRoutingDescriptor),
			};

			AssertArrayEqualsByElements("Descriptors should be in exact order", expectedTypes, descriptorTypes);
		}

		public virtual void TestGetRoutingUrlNoOriginalUrl()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User X";
			contact.OC_Email = "other@email.org";
			contact.OC_WebAccessEnabled = true;
			Factory.Save();

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var router = new LoginRouterForTest(null, token);
			Assert("Routing required", !router.HasAnyRoutingRequired);
			var redirectUrl = router.GetRoutingUrl();
			AssertContains("Should redirect to LoginComplete.aspx", "/Login/LoginComplete.aspx", redirectUrl.OriginalString);
			var uriDeconstructor = new UriDeconstructor(redirectUrl);
			var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			var tokenFromQueryString = secureQueryString[LoginRouter.IdentityTokenQueryStringKey];
			AssertEquals(token, tokenFromQueryString);
			AssertNull(secureQueryString[LoginRouter.OriginalUrlQueryStringKey]);
		}

		public void TestHasAnyRoutingRequired()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Test User";
			contact.OC_Email = "tester@test.org";

			Factory.Save();

			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var router1 = new LoginRouterForTest(new Uri("https://myaccount.com/"), token);
			var router2 = new LoginRouterForTest(new Uri("https://myaccount.com/"), "blah");

			Assert("Routing required", router1.HasAnyRoutingRequired);
			Assert("No routing required since token is invalid", !router2.HasAnyRoutingRequired);
			contact.OC_ContactName = "Developer";
			contact.OC_Email = "user@dev.org";
			Factory.Save();
			Assert("Routing required", !router1.HasAnyRoutingRequired);
		}

		#region Implementation

		protected virtual LoginRouter GetNewLoginRouter(Uri originalUrl, string token)
		{
			return new LoginRouterForTest(originalUrl, token);
		}

		class LoginRouterForTest : LoginRouter
		{
			public LoginRouterForTest(Uri originalUrl, string token)
				: base(originalUrl, token)
			{
			}

			public LoginRouterForTest(Uri originalUrl, OrgContact contact, ZGlobal appInstance = null)
				: base(originalUrl, contact, appInstance)
			{
			}

			protected override Uri DefaultUrl => new Uri("https://yahoo.com.au");

			protected override IEnumerable<ILoginRoutingDescriptor> RoutingDescriptors
			{
				get
				{
					var descriptors = new List<ILoginRoutingDescriptor>(base.RoutingDescriptors)
					{
						new ChangeContactNameRoutingDescriptor(Contact),
						new CheckContactEmailRoutingDescriptor(Contact)
					};

					return descriptors;
				}
			}

			protected override ZGlobal GetNewGlobal()
			{
				return new ZGlobalForTesting();
			}

			public Uri OriginalUrlExposed => OriginalUrl;
			public OrgContact ContactExposed => Contact;
		}

		class ChangeContactNameRoutingDescriptor : ILoginRoutingDescriptor
		{
			public ChangeContactNameRoutingDescriptor(OrgContact contact)
			{
				this.contact = contact;
			}
			readonly OrgContact contact;

			public Uri RoutingUrl => new Uri("https://myaccount.com/changename.aspx?token=12345");

			public bool IsRoutingRequired => contact != null && contact.OC_ContactName == "Test User";

			public void RoutingAction()
			{
			}
		}

		class CheckContactEmailRoutingDescriptor : ILoginRoutingDescriptor
		{
			public CheckContactEmailRoutingDescriptor(OrgContact contact)
			{
				this.contact = contact;
			}
			readonly OrgContact contact;

			public Uri RoutingUrl => new Uri("https://myaccount.com/changeemail.aspx");

			public bool IsRoutingRequired => contact != null && contact.Email.Contains("@test.org");

			public void RoutingAction()
			{
				contact.OC_Email += ".au";
				contact.Factory.Save();
			}
		}

		class LoginRouterForDescriptorTest : LoginRouter
		{
			public LoginRouterForDescriptorTest(Uri originalUrl, string token)
				: base(originalUrl, token)
			{
			}

			public IEnumerable<ILoginRoutingDescriptor> RoutingDescriptors_Expose => RoutingDescriptors;

			protected override Uri DefaultUrl { get; }

			protected override ZGlobal GetNewGlobal()
			{
				return new ZGlobalForTesting();
			}
		}

		PageForTest GetPageForTest()
		{
			var page = new PageForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics",
				BindingFlags.NonPublic | BindingFlags.Instance,
				null,
				new Type[] { typeof(HttpContext) },
				null);
			method.Invoke(page, new object[] { HttpContext.Current });

			return page;
		}

		class PageForTest : ZPage
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			class GlobalForTest : ZGlobal
			{
				public override string DefaultPage => string.Empty;

				public void OnCustomSessionStart()
				{
					base.OnCustomSessionStart(this, EventArgs.Empty);
				}
			}
		}

		#endregion
	}
}
