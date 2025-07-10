using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(HouseBilleManifestModule))]
	sealed class HouseBilleManifestModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.CAHouseBilleManifest;
	}
}
