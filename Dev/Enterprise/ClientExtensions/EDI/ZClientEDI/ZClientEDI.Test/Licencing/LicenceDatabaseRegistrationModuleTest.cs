using Enterprise.Client.EDI.LicenceKeyBuilder.Module;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Module.Testing
{
	[TestedType(typeof(LicenceDatabaseRegistrationModule))]
	public class LicenceDatabaseRegistrationModuleTest : ZPopupModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => Modules.ClientModuleRegistration.LicenceDatabaseRegistration;
	}
}
