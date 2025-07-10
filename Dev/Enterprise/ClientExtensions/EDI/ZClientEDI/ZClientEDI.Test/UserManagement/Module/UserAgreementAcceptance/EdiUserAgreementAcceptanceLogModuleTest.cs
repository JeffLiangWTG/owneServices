using Enterprise.Client.EDI.UserManagement.Module;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Test
{
	[TestedType(typeof(EdiUserAgreementAcceptanceLogModule))]
	public class EdiUserAgreementAcceptanceLogModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => Modules.ClientModuleRegistration.UserAgreementAcceptances;
	}
}
