using Enterprise.Customs.AE.Manifest.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.GUI.Testing;

[TestedType(typeof(ManifestLayoutBuilder))]
sealed class ManifestLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ManifestLayoutBuilder, AsycudaManifestHeader, Customs.ASYCUDA.GUI.CommonManifestControlBag>
{
	protected override ManifestLayoutBuilder GetColumnLayoutBuilderForTesting() => new ManifestLayoutBuilder();

	protected override int ExpectedMaxColumns => 3;
}
