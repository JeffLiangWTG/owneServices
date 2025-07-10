using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module.Testing
{
	[TestedType(typeof(LicenseKeyModule))]
	public class LicenseKeyModuleTest : ZModuleBasherTest
	{
		public void TestModuleIDAndSupportWorkflow()
		{
			using (var module = new LicenseKeyModule())
			{
				AssertEquals(Modules.ClientModuleRegistration.LicenseKey, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		protected override ModuleIdentifier GetModuleID() => Modules.ClientModuleRegistration.LicenseKey;
	}
}
