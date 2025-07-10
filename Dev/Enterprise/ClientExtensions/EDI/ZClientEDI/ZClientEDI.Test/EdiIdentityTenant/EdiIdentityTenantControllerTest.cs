using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityTenant.Module.Testing
{
	[TestedType(typeof(EdiIdentityTenantController))]
	internal class EdiIdentityTenantControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.EdiIdentityTenant;
		}
	}
}
