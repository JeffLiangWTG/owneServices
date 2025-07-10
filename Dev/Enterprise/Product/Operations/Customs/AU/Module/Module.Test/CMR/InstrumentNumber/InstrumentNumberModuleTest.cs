using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(InstrumentNumberModule))]
	sealed class InstrumentNumberModuleTest : CMRSearchOnlyModuleTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.InstrumentNumber;
	}
}
