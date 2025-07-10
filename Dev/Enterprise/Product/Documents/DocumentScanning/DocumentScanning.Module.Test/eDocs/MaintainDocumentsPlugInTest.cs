using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Module.Testing
{
	[TestedType(typeof(eDocsPlugInController))]
	internal sealed class MaintainDocumentsPlugInTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return null;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.eDocsPlugIn;
		}
	}
}
