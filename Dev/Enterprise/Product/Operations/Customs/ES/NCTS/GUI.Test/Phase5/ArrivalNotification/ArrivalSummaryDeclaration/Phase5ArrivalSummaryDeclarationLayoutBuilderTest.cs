using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5ArrivalSummaryDeclarationLayoutBuilder<NctsHeader>))]
	class Phase5ArrivalSummaryDeclarationLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<Phase5ArrivalSummaryDeclarationLayoutBuilder<NctsHeader>, NctsHeader, Phase5ArrivalSummaryDeclarationControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override Phase5ArrivalSummaryDeclarationLayoutBuilder<NctsHeader> GetColumnLayoutBuilderForTesting()
			=> new Phase5ArrivalSummaryDeclarationLayoutBuilder<NctsHeader>();
	}
}
