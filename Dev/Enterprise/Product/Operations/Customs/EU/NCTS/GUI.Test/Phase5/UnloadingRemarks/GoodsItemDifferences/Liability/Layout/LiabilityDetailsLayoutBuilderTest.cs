using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(LiabilityDetailsLayoutBuilder<NctsArrivalCargoDesc>))]
	sealed class LiabilityDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<LiabilityDetailsLayoutBuilder<NctsArrivalCargoDesc>, NctsArrivalCargoDesc, LiabilityDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override LiabilityDetailsLayoutBuilder<NctsArrivalCargoDesc> GetColumnLayoutBuilderForTesting() => new LiabilityDetailsLayoutBuilder<NctsArrivalCargoDesc>();
	}
}
