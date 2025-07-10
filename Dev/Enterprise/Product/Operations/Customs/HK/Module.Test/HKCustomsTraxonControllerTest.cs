using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.HK.Module.Testing
{
	[TestedType(typeof(HKCustomsTraxonController))]
	class HKCustomsTraxonControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.HK.Traxon;
	}
}
