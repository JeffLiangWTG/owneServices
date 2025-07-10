using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.Module.Testing
{
	[TestedType(typeof(FeatureSetController))]
	public class FeatureSetControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.FeatureSet;
		}
	}
}
