using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.Cognos;
using Enterprise.Client.JAS.GUI.Cognos;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Module.Testing
{
	[TestedType(typeof(CognosCsvExportController))]
	public class CognosCsvExportControllerTest : ZSingletonControllerBasherTest
	{
		public void TestGetForm()
		{
			AssertEquals(typeof(CognosCsvExportForm), Controller.ShowNewForm().GetType());
		}

		public void TestGetNewBusinessEntityInLocalFactory()
		{
			object businessEntityInLocalFactory = Controller.GetType().GetMethod("GetNewBusinessEntityInLocalFactory", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Controller, null);
			AssertEquals(typeof(CognosDataExporterBizO), businessEntityInLocalFactory.GetType());
		}

		public void TestCheckPointForView()
		{
			object securityCheckPoint = Controller.GetType().GetProperty("CheckPointForView", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Controller, null);
			AssertEquals(Env.Security.None, securityCheckPoint);
		}

		protected override ControllerID GetControllerID()
		{
			return ClientControllerRegistration.ExportCognosCsv;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new CognosDataExporterBizO();
		}
	}
}
