using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Module.Testing
{
	[TestedType(typeof(NctsReportsModule))]
	class NctsReportsModuleBasherTest : ZEmbeddedModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.NctsReportsModule;
	}
}
