using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GoodsItemDifferencesDetailsLayoutBuilder<NctsArrivalCargoDesc>))]
	internal class Phase5GoodsItemDifferencesDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<Phase5GoodsItemDifferencesDetailsLayoutBuilder<NctsArrivalCargoDesc>, NctsArrivalCargoDesc, Phase5GoodsItemDifferencesDetailsControlBag>
	{
		protected override Phase5GoodsItemDifferencesDetailsLayoutBuilder<NctsArrivalCargoDesc> GetColumnLayoutBuilderForTesting() => new Phase5GoodsItemDifferencesDetailsLayoutBuilder<NctsArrivalCargoDesc>();

		protected override int ExpectedMaxColumns => 1;
	}
}
