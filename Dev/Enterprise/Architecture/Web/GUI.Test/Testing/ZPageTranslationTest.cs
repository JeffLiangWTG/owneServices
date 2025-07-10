using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZPageTranslationTest : ZPageLifeCycleTest
	{
		protected override bool NeedRenderControl
		{
			get
			{
				return false;
			}
		}

		protected override ZPage GetNewPage()
		{
			var fPage = new DummyPage();
			MethodInfo method = typeof(Page).GetMethod("SetIntrinsics",
				BindingFlags.NonPublic | BindingFlags.Instance,
				null,
				new Type[] { typeof(HttpContext) },
				null);
			method.Invoke(fPage, new object[] { HttpContext.Current });
			return fPage;
		}

		protected new DummyPage TestPage
		{
			get { return base.TestPage as DummyPage; }
		}

		public void TestTranslationFeedbackManagerWhenPageIsPostBack()
		{
			TestTranslationFeedbackManagerWhenPageIsPostBack(SharedConstants.Languages.French);
			TestTranslationFeedbackManagerWhenPageIsPostBack(SharedConstants.Languages.English);
		}

		void TestTranslationFeedbackManagerWhenPageIsPostBack(string language)
		{
			try
			{
				using (Res.TemporarilySwitchLanguage(language))
				{
					TranslationFeedbackManager.CleanupResourceStringUsageFiles(TestPage.Session.SessionID);
					var siteUser = new OrgContactWebUser();
					siteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

					this.AssertUnload += ZPageLifeCycleTest_AssertUnload;
					PageLoginWithUser(siteUser);
					RunPageLifeCycle();

					PageLoginWithUser(siteUser);
					IsPostBack = true;
					TestPage.IsPostBack = true;
					RunPageLifeCycle();
				}
			}
			finally
			{
				var resourceStringUsageDataBasePath = CommonProgramData.GetCargoWiseDirectory("ResourceStringUsageData", Db.ServerName, Db.DatabaseName);
				if (Directory.Exists(resourceStringUsageDataBasePath))
				{
					Directory.Delete(resourceStringUsageDataBasePath, true);
				}
			}
		}

		public void TestTranslationFeedbackManagerAddHostToTrustedList()
		{
			var originValue = Env.Registry.ResourceStringUsageTrustedDomains;
			AssertEquals("Pre condition: Resource String Usage Trusted Domains List should be empty.", 0, originValue.Length);

			var hosts = new string[] { TestPage.Request.Url.GetLeftPart(UriPartial.Authority) };

			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.ChineseSimplified))
			{
				using (Env.Registry.RawRegistry.ResourceStringUsageTrustedDomains.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, originValue))
				{
					var siteUser = new OrgContactWebUser();
					siteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
					PageLoginWithUser(siteUser);
					RunPageLifeCycle();

					AssertArrayEqualsByElements("Host of ZPage should be added to trusted list.", hosts, Env.Registry.ResourceStringUsageTrustedDomains);
				}

				using (Env.Registry.RawRegistry.ResourceStringUsageTrustedDomains.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, hosts))
				{
					var siteUser = new OrgContactWebUser();
					siteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
					PageLoginWithUser(siteUser);
					RunPageLifeCycle();

					AssertArrayEqualsByElements("Host of ZPage should be added to trusted list.", hosts, Env.Registry.ResourceStringUsageTrustedDomains);
				}
			}
		}

		void PageLoginWithUser(WebUser user)
		{
			TestPage.ShowLoginStatusOverride = false;
			var dummyTestGlobal = (ZDummyTestGlobal)TestPage.AppInstance;
			dummyTestGlobal.SiteUserOverride = user;
		}

		void ZPageLifeCycleTest_AssertUnload(object sender, EventArgs e)
		{
			var resourceStringUsageDataBasePath = CommonProgramData.GetCargoWiseDirectory("ResourceStringUsageData", Db.ServerName, Db.DatabaseName);
			var resourceStringUsageFileDirectory = Path.Combine(resourceStringUsageDataBasePath, TestPage.Session.SessionID);
			if (Res.CurrentLanguage == SharedConstants.Languages.English)
			{
				AssertEquals("There should be no page session specific directory created", false, Directory.Exists(resourceStringUsageFileDirectory));
			}
			else
			{
				var filesCount = Directory.GetFiles(resourceStringUsageFileDirectory).Length;
				AssertEquals("There should be only 1 ResourceStringUsageData file throughout life cycle of Test Page.", 1, filesCount);
			}
		}
	}
}
