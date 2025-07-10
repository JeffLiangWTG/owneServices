using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.AUS.Modules.Testing
{
	[TestedType(typeof(OriginPreferenceMappingController))]
	public class OriginPreferenceMappingControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ClientControllerRegistration.OriginPreferenceMapping;
		}
	}
}
