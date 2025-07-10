using Enterprise.Customs.IL.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	[TestedType(typeof(AsycudaTransportDocumentDetailsBuilder))]
	public sealed class AsycudaTransportDocumentDetailsBuilderTest : ColumnLayoutBuilderAbstractTest<AsycudaTransportDocumentDetailsBuilder, AsycudaAdditionalInfo, AsycudaTransportDocumentDetailsControlBag>
	{
		protected override AsycudaTransportDocumentDetailsBuilder GetColumnLayoutBuilderForTesting() => new AsycudaTransportDocumentDetailsBuilder();

		protected override int ExpectedMaxColumns => 1;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
