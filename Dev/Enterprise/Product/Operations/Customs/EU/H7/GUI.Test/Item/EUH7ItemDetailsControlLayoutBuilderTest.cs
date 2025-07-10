using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7ItemDetailsControlLayoutBuilder<AsycudaPackedItem>))]
	sealed class EUH7ItemDetailsControlLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<EUH7ItemDetailsControlLayoutBuilder<AsycudaPackedItem>, AsycudaPackedItem, EUH7ItemDetailsCommonControlBag>
	{
		protected override EUH7ItemDetailsControlLayoutBuilder<AsycudaPackedItem> GetColumnLayoutBuilderForTesting() => new EUH7ItemDetailsControlLayoutBuilder<AsycudaPackedItem>();

		protected override int ExpectedMaxColumns => 2;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium;

		protected override bool ExpectedNarrowColumnForMediumControls => true;
	}
}
