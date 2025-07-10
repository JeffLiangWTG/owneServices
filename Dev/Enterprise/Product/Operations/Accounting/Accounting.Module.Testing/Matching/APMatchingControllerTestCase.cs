using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APMatchingController))]
	public class APMatchingControllerTestCase : MatchingControllerTestCase
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ZAPMatching;
		}

		protected override ModuleIdentifier GetExpectedModuleID()
		{
			return ModuleIDs.ZAPMatching;
		}
	}
}
