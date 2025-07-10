using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.Module.Testing
{
	[TestedType(typeof(IntrastatReportsModule))]
	sealed class IntrastatReportsModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.IntrastatReports;
	}
}
