using Enterprise.Customs.AU.Module.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Testing
{
	[TestedType(typeof(UPEExportCustomsManifestModule))]
	sealed class UPEExportCustomsManifestModuleOverrideTest : ExportCustomsManifestTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.AU.ExportCustomsManifest;
		}
	}
}
