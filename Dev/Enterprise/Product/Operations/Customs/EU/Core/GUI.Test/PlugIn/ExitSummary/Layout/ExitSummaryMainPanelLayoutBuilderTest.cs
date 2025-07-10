using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	[TestedType(typeof(ExitSummaryMainPanelLayoutBuilder<CusExitControlHeader>))]
	class ExitSummaryMainPanelLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ExitSummaryMainPanelLayoutBuilder<CusExitControlHeader>, CusExitControlHeader, ExitSummaryMainPanelControlBag>
	{
		protected override ExitSummaryMainPanelLayoutBuilder<CusExitControlHeader> GetColumnLayoutBuilderForTesting() => new ExitSummaryMainPanelLayoutBuilder<CusExitControlHeader>();

		protected override int ExpectedMaxColumns => 4;

		protected override bool ExpectedNarrowColumnForMediumControls => true;
	}
}
