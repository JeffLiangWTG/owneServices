using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[HttpContextEnabledTest]
	class DefaultAspxTest : TestCaseWithFactory
	{
		public void TestPageLoad_NoPermission()
		{
			Page.Page_Load(Page, EventArgs.Empty);
			AssertContains("disabled", Page.InternalDownloadsLink.CssClass);
			AssertEquals("javascript:alert('You do not have permission to access this page.');", Page.InternalDownloadsLink.NavigateUrl);
			AssertContains("disabled", Page.InternalReportsLink.CssClass);
			AssertEquals("javascript:alert('You do not have permission to access this page.');", Page.InternalReportsLink.NavigateUrl);
			AssertContains("disabled", Page.InternalWebSecurityLink.CssClass);
			AssertEquals("javascript:alert('You do not have permission to access this page.');", Page.InternalWebSecurityLink.NavigateUrl);
			AssertContains("disabled", Page.InternalNotificationRolesLink.CssClass);
			AssertEquals("javascript:alert('You do not have permission to access this page.');", Page.InternalNotificationRolesLink.NavigateUrl);
		}

		public void TestPageLoad_AllAccessPermission()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			Factory.Save();
			Page.AppInstance.SiteUser.LoginSupportForTest(org.OH_Code);
			Page.Page_Load(Page, EventArgs.Empty);
			AssertNotContains("disabled", Page.InternalDownloadsLink.CssClass);
			AssertEquals("", Page.InternalDownloadsLink.NavigateUrl);
			AssertNotContains("disabled", Page.InternalReportsLink.CssClass);
			AssertEquals("", Page.InternalReportsLink.NavigateUrl);
			AssertNotContains("disabled", Page.InternalWebSecurityLink.CssClass);
			AssertEquals("", Page.InternalWebSecurityLink.NavigateUrl);
			AssertNotContains("disabled", Page.InternalNotificationRolesLink.CssClass);
			AssertEquals("", Page.InternalNotificationRolesLink.NavigateUrl);
		}

		public void TestPageLoad_PartialPermission()
		{
			EDIWebSecurityRightsList.RegisterThisSubTypeOverride();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			var security = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, EDIWebSecurityRightsList.Downloads.Code))[0];
			security.OX_Granted = true;
			var newContact = org.Contacts.AddNew();
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			Page.AppInstance.SiteUser.Login(org.OH_Code, "newuser@cargowise.com", "1234");
			Page.Page_Load(Page, EventArgs.Empty);
			AssertNotContains("disabled", Page.InternalDownloadsLink.CssClass);
			AssertEquals("", Page.InternalDownloadsLink.NavigateUrl);
		}

		public void TestPageLoad_InternalSetupAccreditationLink()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			Factory.Save();
			Page.AppInstance.SiteUser.LoginSupportForTest(org.OH_Code);
			Assert(string.IsNullOrEmpty(Page.InternalAccreditationLink.NavigateUrl));
		}

		protected override void SetUp()
		{
			base.SetUp();
			EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
		}

		DefaultPageForTest Page
		{
			get
			{
				if (page == null)
				{
					page = new DefaultPageForTest();
					page.InternalDownloadsLink = new HyperLink();
					page.InternalWebSecurityLink = new HyperLink();
					page.InternalNotificationRolesLink = new HyperLink();
					page.InternalReportsLink = new HyperLink();
					page.InternalAccreditationLink = new HyperLink();
					page.InternalUpdatenotesLink = new HyperLink();
					page.InternalChangepasswordLink = new HyperLink();
					page.InternaleLearningLink = new HyperLink();
					page.InternalsapphireELearningLink = new HyperLink();
					page.InternalodysseyELearningLink = new HyperLink();
				}

				return page;
			}
		}

		DefaultPageForTest page;
		class DefaultPageForTest : Default
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				GlobalForTest result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}
		}

		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
			}
		}
	}
}
