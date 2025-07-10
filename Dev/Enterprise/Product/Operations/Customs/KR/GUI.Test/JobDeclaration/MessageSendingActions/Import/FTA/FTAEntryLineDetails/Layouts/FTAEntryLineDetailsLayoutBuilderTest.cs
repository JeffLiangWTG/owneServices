using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(FTAEntryLineDetailsLayoutBuilder))]
	sealed class FTAEntryLineDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<FTAEntryLineDetailsLayoutBuilder, JobDeclaration, FTAEntryLineDetailsControlBag>
	{
		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int ExpectedMaxColumns => 2;

		protected override FTAEntryLineDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new FTAEntryLineDetailsLayoutBuilder();
	}
}
