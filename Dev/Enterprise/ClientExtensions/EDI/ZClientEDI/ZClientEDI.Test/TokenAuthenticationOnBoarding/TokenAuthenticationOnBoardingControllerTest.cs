using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Module.Testing
{
	[TestedType(typeof(TokenAuthenticationOnBoardingController))]
	public class TokenAuthenticationOnBoardingControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.TokenAuthenticationOnBoarding;
		}
	}
}
