using System;
using System.Web;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class SingleMyAccountWebUserTest : TestCaseWithFactory, IHttpContextEnabledTestWithAppInstance
	{
		public ZEnterpriseGlobalBase AppInstance => globalForTest;

		readonly GlobalForTest globalForTest = new GlobalForTest();

		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
				EnableSingleActiveUserSession = true;
			}

			public void SetSingleSessionFlag(bool value) => EnableSingleActiveUserSession = value;

			public void Session_EndExposed(object sender, EventArgs e) => base.Session_End(sender, e);
		}

		public void TestFeatureFlag()
		{
			globalForTest.SetSingleSessionFlag(false);
			Assert(!(HttpContext.Current.ApplicationInstance as Global).EnableSingleActiveUserSession);

			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "DDDSCWSYD";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("12340");
			Factory.Save();

			var webUser = new MyAccountWebUser();
			webUser.Login(contact.OC_Email, "12340");
			Assert(webUser.IsLoggedIn);
			AssertNull(HttpContext.Current.Session["heartbeatUniqueId"]);
			webUser.Logout();

			Assert(!webUser.IsLoggedIn);
			globalForTest.SetSingleSessionFlag(true);
			Assert((HttpContext.Current.ApplicationInstance as Global).EnableSingleActiveUserSession);
			webUser.Login(contact.OC_Email, "12340");

			var heartbeatID = (Guid)HttpContext.Current.Session["heartbeatUniqueId"];
			var dbManager = new SemaphoreDbManager();
			AssertNotEquals(Guid.Empty, heartbeatID);

			var handles = dbManager.GetActiveSemaphoreHandles(heartbeatID);
			AssertEquals(1, handles.Length);
			AssertEquals(contact.PK.ToGuid(), handles[0].OwnerSession.UserPk);
		}

		public void TestLogin_UpdateContext()
		{
			Assert((HttpContext.Current.ApplicationInstance as Global).EnableSingleActiveUserSession);
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "DDDSCWSYD";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("12340");

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "ntz";
			contact2.OC_Email = "ntz@test.com";
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword("12340");

			Factory.Save();

			var webUser = new MyAccountWebUser();
			webUser.Login(contact.OC_Email, "12340");
			Assert(webUser.IsLoggedIn);

			var heartbeatID = (Guid)HttpContext.Current.Session["heartbeatUniqueId"];
			var dbManager = new SemaphoreDbManager();
			AssertNotEquals(Guid.Empty, heartbeatID);
			var handles = dbManager.GetActiveSemaphoreHandles(heartbeatID);
			AssertEquals(1, handles.Length);
			AssertEquals(contact.PK.ToGuid(), handles[0].OwnerSession.UserPk);

			webUser.Login(contact2.OC_Email, "12340");
			Assert(webUser.IsLoggedIn);
			AssertEquals(contact2.PK, webUser.LoggedInUserPK);
			AssertEquals("ID should not be updated", heartbeatID, (Guid)HttpContext.Current.Session["heartbeatUniqueId"]);
			var handles2 = dbManager.GetActiveSemaphoreHandles(heartbeatID);
			AssertEquals(1, handles2.Length);
			AssertEquals(contact2.PK.ToGuid(), handles2[0].OwnerSession.UserPk);
		}

		public void TestLogout()
		{
			var application = HttpContext.Current.ApplicationInstance as GlobalForTest;
			AssertEquals(HttpContext.Current.Session, application.Session);
			Assert(application.EnableSingleActiveUserSession);

			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "DDDSCWSYD";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("12340");
			Factory.Save();

			var webUser = new MyAccountWebUser();
			HttpContext.Current.Session["SiteUser"] = webUser;

			webUser.Login(contact.OC_Email, "12340");
			Assert(webUser.IsLoggedIn);

			var heartbeatID = (Guid)HttpContext.Current.Session["heartbeatUniqueId"];
			var dbManager = new SemaphoreDbManager();
			AssertNotEquals(Guid.Empty, heartbeatID);
			var handles = dbManager.GetActiveSemaphoreHandles(heartbeatID);
			AssertEquals(1, handles.Length);
			AssertEquals(contact.PK.ToGuid(), handles[0].OwnerSession.UserPk);

			application.SignOut(false);
			application.Session_EndExposed(this, EventArgs.Empty);

			Assert(!webUser.IsLoggedIn);
			AssertEquals(Guid.Empty, HttpContext.Current.Session["heartbeatUniqueId"]);
			AssertEquals("Handles and Heartbeat should be deleted", 0, dbManager.GetActiveSemaphoreHandles(heartbeatID).Length);
		}
	}
}
