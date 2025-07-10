using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.EndpointManagement.Module.Testing
{
	[TestedType(typeof(EdiTrustedSystemController))]
	public class EdiTrustedSystemControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.EdiTrustedSystem;
		}
	}
}
