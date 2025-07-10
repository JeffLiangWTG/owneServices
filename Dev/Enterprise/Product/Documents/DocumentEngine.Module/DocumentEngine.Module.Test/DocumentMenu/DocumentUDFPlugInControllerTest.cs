using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Module.Testing
{
	[TestedType(typeof(DocumentUDFPlugInController))]
	sealed class DocumentUDFPlugInControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DocumentUDFPlugIn;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return null;
		}
	}
}
