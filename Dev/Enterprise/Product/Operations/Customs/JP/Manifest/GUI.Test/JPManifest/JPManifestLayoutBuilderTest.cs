using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(JPManifestLayoutBuilder))]
	sealed class JPManifestLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<JPManifestLayoutBuilder, AsycudaManifestHeader, CommonManifestControlBag>
	{
		protected override JPManifestLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			return new JPManifestLayoutBuilder();
		}

		protected override int ExpectedMaxColumns => 3;
		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium + 30;
	}
}
