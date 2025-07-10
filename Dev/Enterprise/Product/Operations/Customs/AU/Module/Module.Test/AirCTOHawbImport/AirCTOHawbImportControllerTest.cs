using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AirCTOHawbImportController))]
	sealed class AirCTOHawbImportControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.AirCTOHawbImport;
	}
}
