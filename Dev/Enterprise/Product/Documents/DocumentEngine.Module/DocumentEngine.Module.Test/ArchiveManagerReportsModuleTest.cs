using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ArchiveReports))]
	sealed class ArchiveManagerReportsModuleTest : ZEmbeddedModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ArchiveReports;
		}
	}
}

