using Enterprise.Customs.IL.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	[TestedType(typeof(AsycudaPackedItemDetailsBuilder))]
	public class AsycudaPackedItemDetailsBuilderTest : ColumnLayoutBuilderAbstractTest<AsycudaPackedItemDetailsBuilder, AsycudaPackedItem, AsycudaPackedItemDetailsControlBag>
	{
		protected override AsycudaPackedItemDetailsBuilder GetColumnLayoutBuilderForTesting() => new AsycudaPackedItemDetailsBuilder();

		protected override int ExpectedMaxColumns => 1;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
