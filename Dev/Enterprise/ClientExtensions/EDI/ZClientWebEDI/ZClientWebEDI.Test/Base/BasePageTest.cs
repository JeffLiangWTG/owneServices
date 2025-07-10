using System;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class BasePageTest : TestCaseWithFactory
	{
		public void TestOnLoadShouldSetupSession()
		{
			Env.ClearUserContext();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			AssertEquals("Precondition", true, Page.ShouldSetupSessionOnLoadExposed);
			AssertEquals("Precondition", false, Page.Request.IsAuthenticated);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentUserPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentBranchPK);
			AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentDepartmentPK);
			Page.OnLoadExposed();
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
		}

		public void TestInitializeCulture()
		{
			ObjectFactory.Get<IResourceStrings>().CurrentLanguage = "ZH-CN";
			HttpContext.Current.Request.Cookies.Add(new HttpCookie("language", "AZ-AZ"));

			var method = typeof(Page).GetMethod("InitializeCulture", BindingFlags.NonPublic | BindingFlags.Instance);
			method.Invoke(Page, Array.Empty<object>());

			AssertEquals(Res.DefaultLanguage, ObjectFactory.Get<IResourceStrings>().CurrentLanguage);
		}

		#region Implementation
		BasePageForTest Page
		{
			get
			{
				if (fPage == null)
				{
					fPage = new BasePageForTest();
					var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
					method.Invoke(fPage, new object[] { HttpContext.Current });
				}

				return fPage;
			}
		}

		BasePageForTest fPage;
		#endregion
	}

	class BasePageForTest : BasePage
	{
		protected override bool ShouldSetupSessionOnLoad => true;
		public bool ShouldSetupSessionOnLoadExposed => ShouldSetupSessionOnLoad;
		public void OnLoadExposed() => OnLoad(EventArgs.Empty);
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
	}
}
