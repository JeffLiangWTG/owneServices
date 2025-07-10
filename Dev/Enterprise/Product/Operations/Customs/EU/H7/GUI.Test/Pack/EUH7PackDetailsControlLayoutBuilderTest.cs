using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7PackDetailsControlLayoutBuilder<AsycudaPack>))]
	sealed class EUH7PackDetailsControlLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<EUH7PackDetailsControlLayoutBuilder<AsycudaPack>, AsycudaPack, EUH7PackDetailsControlBag>
	{
		protected override EUH7PackDetailsControlLayoutBuilder<AsycudaPack> GetColumnLayoutBuilderForTesting() => new EUH7PackDetailsControlLayoutBuilder<AsycudaPack>();

		protected override int ExpectedMaxColumns => 1;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
