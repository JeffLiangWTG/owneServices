using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.Matching;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Module.Testing
{
	[TestedType(typeof(PreMatchingExportController))]
	public class PreMatchingExportControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ClientControllerRegistration.ExportAccountingDataForPreMatching;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new PreMatchedDataExporter();
		}
	}
}
