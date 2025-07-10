using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ResourceStrings.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Module.Testing
{
	[TestedType(typeof(LocalLanguagesController))]
	public class LocalLanguageControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var language = Factory.New<RefLocalLanguage>();
			language.RA_Code = "ZX";
			language.RA_RN_NKCountryCode = "CN";
			language.RA_Description = "TestLanguage";
			Factory.Save();
			return language;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.LocalLanguages;
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new TestLocalLanguagesController();
			AssertEquals("For New", Env.Security.LocalLanguagesNew, controller.CheckPointForNew);
			AssertEquals("For View", Env.Security.LocalLanguagesView, controller.CheckPointForView);
			AssertEquals("For Edit", Env.Security.LocalLanguagesModify, controller.CheckPointForEdit);
			AssertEquals("For Delete", Env.Security.LocalLanguagesDelete, controller.CheckPointForDelete);
		}

		class TestLocalLanguagesController : LocalLanguagesController
		{
			public new SecurityCheckpoint CheckPointForNew
			{
				get { return base.CheckPointForNew; }
			}

			public new SecurityCheckpoint CheckPointForView
			{
				get { return base.CheckPointForView; }
			}

			public new SecurityCheckpoint CheckPointForEdit
			{
				get { return base.CheckPointForEdit; }
			}

			public new SecurityCheckpoint CheckPointForDelete
			{
				get { return base.CheckPointForDelete; }
			}
		}
	}
}
