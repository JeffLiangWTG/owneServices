using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Module.DocumentMenu.DocDataPlugIn.Testing
{
	[TestedType(typeof(DocDataPlugInController))]
	sealed class DocDataPlugInControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DocDataPlugIn;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return null;
		}
	}
}
