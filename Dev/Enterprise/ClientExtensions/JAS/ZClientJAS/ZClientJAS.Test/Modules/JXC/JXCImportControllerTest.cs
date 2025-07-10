using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.JXC.Import;
using Enterprise.Client.JAS.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Module.Testing
{
	[TestedType(typeof(JXCImportController))]
	public class JXCImportControllerTest : ZSingletonControllerBasherTest
	{
		public void TestGetForm()
		{
			AssertEquals(typeof(JXCImporterForm), Controller.ShowNewForm().GetType());
		}

		public void TestGetNewBusinessEntityInLocalFactory()
		{
			object businessEntityInLocalFactory = Controller.GetType().GetMethod("GetNewBusinessEntityInLocalFactory", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Controller, null);
			AssertEquals(typeof(JXCDataImporterBizO), businessEntityInLocalFactory.GetType());
		}

		public void TestCheckPointForView()
		{
			object securityCheckPoint = Controller.GetType().GetProperty("CheckPointForView", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Controller, null);
			AssertEquals(Env.Security.None, securityCheckPoint);
		}

		protected override ControllerID GetControllerID()
		{
			return ClientControllerRegistration.ImportJXCFile;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new JXCDataImporterBizO();
		}
	}
}
