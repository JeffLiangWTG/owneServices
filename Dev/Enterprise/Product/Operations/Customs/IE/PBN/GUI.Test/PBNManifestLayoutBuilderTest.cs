using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.IE.PBN.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.GUI.Testing
{
	[TestedType(typeof(PBNManifestLayoutBuilder<AsycudaManifestHeader>))]
	sealed class PBNManifestLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<PBNManifestLayoutBuilder<AsycudaManifestHeader>, AsycudaManifestHeader, CommonManifestControlBag>
	{
		protected override PBNManifestLayoutBuilder<AsycudaManifestHeader> GetColumnLayoutBuilderForTesting() => new PBNManifestLayoutBuilder<AsycudaManifestHeader>();
		protected override int ExpectedMaxColumns => 3;
	}
}
