using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(CusMiscRequestHeaderViewLayoutBuilder))]
	sealed class ExtendedHoursRequestViewLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<CusMiscRequestHeaderViewLayoutBuilder, CusMiscRequestHeader, CusMiscRequestHeaderViewControlBag>
	{
		protected override int ExpectedMaxColumns => 2;
		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
		protected override CusMiscRequestHeaderViewLayoutBuilder GetColumnLayoutBuilderForTesting() => new CusMiscRequestHeaderViewLayoutBuilder();
	}
}
