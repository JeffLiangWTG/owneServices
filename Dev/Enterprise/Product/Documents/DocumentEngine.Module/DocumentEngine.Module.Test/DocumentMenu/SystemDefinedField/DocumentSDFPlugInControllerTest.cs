using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Module.SDF.Testing
{
	[TestedType(typeof(DocumentSDFPlugInController))]
	sealed class DocumentSDFPlugInControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DocumentSDFPlugIn;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return null;
		}
	}
}
