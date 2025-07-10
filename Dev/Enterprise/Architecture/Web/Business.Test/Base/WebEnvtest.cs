#if NETFRAMEWORK
using System.Web;
using Enterprise.ZArchitecture.Web.Shared;
#elif NET
using System.Globalization;
using System.Linq;
using System.Threading;

#endif
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	[HttpContextEnabledTest]
	sealed class WebEnvtest : TestCase
	{
#if NETFRAMEWORK
		public void TestAppInstance()
		{
			HttpContext.Current.ApplicationInstance = new ZEnterpriseGlobalBaseForTest();
			AssertNotNull("Should not be null if ApplicationInstance can be casted to ZEnterpriseGlobalBase", WebEnv.AppInstance);
			AssertType<ZEnterpriseGlobalBaseForTest>("Should be of concrete type of ZEnterpriseGlobalBase", WebEnv.AppInstance);
			AssertEquals("Should be from the Current HttpContext", HttpContext.Current.ApplicationInstance, WebEnv.AppInstance);
		}

		public void TestAppInstanceNonZEnterpriseGlobalBasee()
		{
			HttpContext.Current.ApplicationInstance = new EnterpriseHttpApplication();
			AssertEquals("Should be null if ApplicationInstance is not cast to ZEnterpriseGlobalBase", null, WebEnv.AppInstance);
		}

		public void TestCurrentUser()
		{
			AssertEquals("Should be from the current Application Instance's SiteUser", ((ZEnterpriseGlobalBase)HttpContext.Current.ApplicationInstance).SiteUser.LoggedInUser, WebEnv.CurrentUser);
		}

		public void TestClientCulture()
		{
			AssertEquals("en-AU", WebEnvShared.ClientCulture.ToString());
			DummyHttpApplication dummyApplication = (DummyHttpApplication)HttpContext.Current.ApplicationInstance;
			DummyWorkerRequest dummyWorkerRequest = dummyApplication.WorkerRequest;
			dummyWorkerRequest.SetUserLanguagesSeparatedByComma("ru-RU");

			AssertEquals("ru-RU", WebEnvShared.ClientCulture.ToString());
			dummyWorkerRequest.ClearUserLanguages();
		}
#elif NET
		public void TestCurrentUser()
		{
			AssertEquals("Should be from the current Application Instance's SiteUser", WebEnv.HttpContextAccessor.HttpContext.Session.GetObject<WebUser>("SiteUser").LoggedInUser.Name, WebEnv.CurrentUser.Name);
		}

		public void TestClientCulture()
		{
			AssertEquals("en-AU", ClientCulture.ToString());
			DummyWorkerRequest dummyWorkerRequest = new DummyWorkerRequest("dummyPage", "dummyQuery", new System.IO.StringWriter(), HttpContextEnabledTestAttribute.HttpContext);
			dummyWorkerRequest.SetUserLanguagesSeparatedByComma("ru-RU");

			AssertEquals("ru-RU", ClientCulture.ToString());
			dummyWorkerRequest.ClearUserLanguages();
		}

		CultureInfo ClientCulture
		{
			get
			{
				try
				{
					var context = HttpContextEnabledTestAttribute.HttpContext;
					var acceptLangHeader = context?.Request?.Headers["Accept-Language"].ToString();

					if (!string.IsNullOrWhiteSpace(acceptLangHeader))
					{
						var firstLang = acceptLangHeader.Split(',').FirstOrDefault();
						if (!string.IsNullOrWhiteSpace(firstLang))
						{
							return CultureInfo.CreateSpecificCulture(firstLang);
						}
					}

					return Thread.CurrentThread.CurrentCulture;
				}
				catch (CultureNotFoundException)
				{
					return Thread.CurrentThread.CurrentCulture;
				}
			}
		}
#endif
	}
}
