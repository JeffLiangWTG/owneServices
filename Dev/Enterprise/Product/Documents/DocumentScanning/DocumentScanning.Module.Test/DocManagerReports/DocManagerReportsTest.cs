using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Module.Testing
{
	[TestedType(typeof(DocManagerReports))]
	internal sealed class DocManagerReportsTest : ZEmbeddedModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DocManagerReports;
		}
	}
}
