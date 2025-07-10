using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARMatchingController))]
	public class ARMatchingControllerTestCase : MatchingControllerTestCase
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ZARMatching;
		}

		protected override ModuleIdentifier GetExpectedModuleID()
		{
			return ModuleIDs.ZARMatching;
		}
	}
}
