using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(ManifestForwardModule))]
	sealed class ManifestForwardModuleTest : ZModuleBasherTest
	{
		public void TestSecurityCheckPoint()
		{
			using (var module = new ManifestForwardModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.CAManifestForward, module.SecurityCheckpoint);
			}
		}

		public void TestFilterControl()
		{
			using (var module = new ManifestForwardModule())
			{
				AssertType<ManifestForwardFilterControl>(module.EmbeddedControl);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.CAManifestForward;

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;
	}
}
