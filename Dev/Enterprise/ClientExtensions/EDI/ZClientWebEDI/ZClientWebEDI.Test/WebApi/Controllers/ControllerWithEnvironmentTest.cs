using System;
using System.Web;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class ControllerWithEnvironmentTest : TestCaseWithFactory
	{
		[HttpContextEnabledTest]
		public void TestCurrentLanguage_Default()
		{
			var controller = new ControllerWithEnvironmentForTest();
			AssertEquals("EN", Res.CurrentLanguage);
		}

		[HttpContextEnabledTest]
		public void TestCurrentLanguage_UserLanguage()
		{
			AssertLanguage("zh-CN,EN", "ZH-CN");
		}

		[HttpContextEnabledTest]
		public void TestCurrentLanguage_InvalidUserLanguage()
		{
			AssertLanguage("##@@,@@##", "EN-GB");
		}

		class ControllerWithEnvironmentForTest : ControllerWithEnvironment
		{
			public ControllerWithEnvironmentForTest() : base()
			{
			}
		}

		void AssertLanguage(string userLanguages, string expectedLanguage)
		{
			var type = ObjectFactory.GetType<IUserContext>();
			var userContext = Activator.CreateInstance(type, Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK, null, true, new BusinessObjectFactory()) as IUserContext;
			type.GetProperty("Company").SetValue(userContext, null, null);
			using (Env.SetTemporaryUserContext(userContext))
			{
				AssertEquals(null, Env.CurrentCompany);
				var dummyApplication = (DummyHttpApplication)HttpContext.Current.ApplicationInstance;
				var dummyWorkerRequest = dummyApplication.WorkerRequest;
				dummyWorkerRequest.SetUserLanguagesSeparatedByComma(userLanguages);
				var controller = new ControllerWithEnvironmentForTest();
				AssertEquals(expectedLanguage, Res.CurrentLanguage);
				dummyWorkerRequest.ClearUserLanguages();
			}
		}
	}
}