using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(PremisesModule))]
	sealed class PremisesModuleTest : CMRSearchOnlyModuleTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Premises;
	}
}
