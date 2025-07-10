using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Moq;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	class SessionFixationIdManagerTest : TestCaseWithFactory
	{
		public void TestSessionIdIsNullWhenUserNotAuthenticated()
		{
			var sessionId = fixationIdManager.GetSessionID(HttpContext.Current);
			AssertNull(sessionId);
		}

		public void TestSessionIdGeneratedWhenUserAuthenticated()
		{
			var identity = new FormsIdentity(new FormsAuthenticationTicket("testUser", false, 10));
			GenericPrincipal principal = new GenericPrincipal(identity, System.Array.Empty<string>());
			HttpContext.Current.User = principal;
			var sessionId = fixationIdManager.GetSessionID(HttpContext.Current);
			AssertEquals("new id", sessionId);
		}

		public void TestSessionIdGeneratedWhenRoutingTokenExists()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "EDISAS";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();
			var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = LoginRouterIdentityManager.GenerateToken(contact), };
			HttpContext.Current.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
			var sessionId = fixationIdManager.GetSessionID(HttpContext.Current);
			AssertEquals("new id", sessionId);
		}

		public void TestSessionIdGeneratedWhenUserAccountRoutingTokenExists()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "EDISAS";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();
			contact.Person.PER_EmailAddress = contact.OC_Email;
			Factory.Save();
			var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = MyAccountLoginRouterIdentityManager.GenerateToken(userAccount), };
			HttpContext.Current.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
			var sessionId = fixationIdManager.GetSessionID(HttpContext.Current);
			AssertEquals("new id", sessionId);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var sessionIDManagerMock = new Mock<ISessionIDManager>();
			sessionIDManagerMock.Setup(x => x.GetSessionID(It.IsAny<HttpContext>())).Returns("new id");
			fixationIdManager = new SessionFixationIdManager(sessionIDManagerMock.Object);
		}

		SessionFixationIdManager fixationIdManager;
	}
}
