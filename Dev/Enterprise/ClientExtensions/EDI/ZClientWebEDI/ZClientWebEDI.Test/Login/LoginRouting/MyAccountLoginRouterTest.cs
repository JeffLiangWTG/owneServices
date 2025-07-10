using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using CargoWise.Common;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.Login.Testing;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class MyAccountLoginRouterTest : LoginRouterTest
	{
		public void TestProperties()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var org = licence.Company.Header;
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "tester@test.org";
			var db = licence.Database;
			var userAccount = Factory.New<EdiCustomerUserAccount>();
			userAccount.EUA_LD = db.PK;
			userAccount.EUA_UserID = "TST";
			userAccount.EUA_FullName = "Test User";
			userAccount.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();
			var router1 = new MyAccountLoginRouterForFunctionTest(new Uri("https://myaccount.com/"), contact);
			var router2 = new MyAccountLoginRouterForFunctionTest(new Uri("https://myaccount.com/"), userAccount);
			AssertEquals(contact.PK, router1.ContactExposed.PK);
			AssertEquals(null, router1.UserAccountExposed);
			AssertEquals(null, router1.DatabaseExposed);
			AssertEquals(new Uri("https://myaccount.com/"), router1.OriginalUrlExposed);
			AssertEquals(contact.PK, router2.ContactExposed.PK);
			AssertEquals(userAccount.PK, router2.UserAccountExposed.PK);
			AssertEquals(db.PK, router2.DatabaseExposed.PK);
			AssertEquals(new Uri("https://myaccount.com/"), router2.OriginalUrlExposed);
		}

		public override void TestDescriptors()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var org = licence.Company.Header;
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "tester@test.org";
			var db = licence.Database;
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "TST";
			user.EUA_FullName = "Test User";
			user.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();
			var router = new MyAccountLoginRouterForDescriptorTest(new Uri("https://myaccount.com/"), user);
			var descriptorTypes = router.RoutingDescriptors_Expose.Select(x => x.GetType()).ToArray();
			var expectedTypes = new[] { typeof(SupersededLoginRoutingDescriptor), typeof(AccountReconfigurationRoutingDescriptor), typeof(UserEmailVerificationRoutingDescriptor), typeof(DistinctEmailRoutingDescriptor), typeof(LoginOptionsExemptionRoutingDescriptor), typeof(LoginOptionsRoutingDescriptor), typeof(SetPersonalEmailRoutingDescriptor), typeof(SetMasterPasswordRoutingDescriptor), typeof(PasswordRotationRoutingDescriptor), typeof(SetPasswordRoutingDescriptor), typeof(TermsAndConditionsRoutingDescriptor), typeof(AccountConfirmationRoutingDescriptor) };
			AssertArrayEqualsByElements("Descriptors should be in exact order", expectedTypes, descriptorTypes);
		}

		public override void TestGetRoutingUrlNoOriginalUrl()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "DSFNOIA";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User X";
			contact.OC_Email = "other@email.org";
			contact.OC_WebAccessEnabled = true;
			Factory.Save();
			contact.Person.PER_EmailAddress = "han@solo.com";
			var router = new MyAccountLoginRouterForFunctionTest(null, contact);
			Assert("Routing required", !router.HasAnyRoutingRequired);
			var redirectUrl = router.GetRoutingUrl();
			AssertStartsWith("Should redirect to LoginComplete", "~/Login/LoginComplete.aspx", redirectUrl.OriginalString);
			var uriDeconstructor = new UriDeconstructor(redirectUrl);
			var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
			AssertNull(secureQueryString[LoginRouter.OriginalUrlQueryStringKey]);
		}

		public override void TestConstructorShouldSetupSession()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var org = licence.Company.Header;
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "tester@test.org";
			var db = licence.Database;
			var userAccount = Factory.New<EdiCustomerUserAccount>();
			userAccount.EUA_LD = db.PK;
			userAccount.EUA_UserID = "TST";
			userAccount.EUA_FullName = "Test User";
			userAccount.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			Env.ClearUserContext();
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentUserPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentBranchPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentDepartmentPK);
			using (var global1 = new Global())
			{
				new MyAccountLoginRouterForFunctionTest(new Uri("https://myaccount.com/"), contact, global1);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
			}

			using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
			{
				Env.ClearUserContext();
			}
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentUserPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentBranchPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentDepartmentPK);
			using (var global2 = new Global())
			{
				new MyAccountLoginRouterForFunctionTest(new Uri("https://myaccount.com/"), userAccount, global2);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
			}
		}

		public static string ExtractOriginalUrl(string redirectUrl)
		{
			var redirectUri = new Uri(redirectUrl, UriKind.RelativeOrAbsolute);
			var uriDeconstructor = new UriDeconstructor(redirectUri);
			var query = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var data = query[LoginRouter.QueryStringKey];
			var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(data));
			var originalUrl = secureQueryString[LoginRouter.OriginalUrlQueryStringKey];
			return originalUrl;
		}

		public static string ExtractGlowUrl(Uri autoLoginUrl)
		{
			var query = HttpUtility.ParseQueryString(autoLoginUrl.Query);
			var data = query[SecureQueryString.QueryStringKey];
			var secureQueryString = new SecureQueryString(data);
			var glowUrl = secureQueryString["glowUrl"];
			return glowUrl;
		}

		protected override string GetErrorPage()
		{
			return new Global().ErrorPage;
		}

		protected override LoginRouter GetNewLoginRouter(Uri originalUrl, string token)
		{
			return new MyAccountLoginRouter(originalUrl, token);
		}

		class MyAccountLoginRouterForFunctionTest : MyAccountLoginRouter
		{
			public MyAccountLoginRouterForFunctionTest(Uri originalUrl, EdiCustomerUserAccount userAccount, ZGlobal appInstance = null) : base(originalUrl, userAccount, appInstance)
			{
			}

			public MyAccountLoginRouterForFunctionTest(Uri originalUrl, string token) : base(originalUrl, token)
			{
			}

			public MyAccountLoginRouterForFunctionTest(Uri originalUrl, OrgContact contact, ZGlobal appInstance = null) : base(originalUrl, contact, appInstance)
			{
			}

			protected override IEnumerable<ILoginRoutingDescriptor> RoutingDescriptors => new List<ILoginRoutingDescriptor>();
			public OrgContact ContactExposed => Contact;
			public EdiCustomerUserAccount UserAccountExposed => UserAccount;
			public LicenceDatabase DatabaseExposed => Database;
			public Uri OriginalUrlExposed => OriginalUrl;
		}

		public class MyAccountLoginRouterForTest : MyAccountLoginRouter
		{
			public MyAccountLoginRouterForTest(Uri originalUrl, OrgContact contact) : base(originalUrl, contact)
			{
			}

			public MyAccountLoginRouterForTest(Uri originalUrl, EdiCustomerUserAccount userAccount) : base(originalUrl, userAccount)
			{
			}
		}

		class MyAccountLoginRouterForDescriptorTest : MyAccountLoginRouter
		{
			public MyAccountLoginRouterForDescriptorTest(Uri originalUrl, EdiCustomerUserAccount userAccount) : base(originalUrl, userAccount)
			{
			}

			public IEnumerable<ILoginRoutingDescriptor> RoutingDescriptors_Expose => RoutingDescriptors;
		}
	}
}
