using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(UnloadingDifferencesDetailsLayoutBuilder<NctsArrivalMovementHeader>))]
	class UnloadingDifferencesDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<UnloadingDifferencesDetailsLayoutBuilder<NctsArrivalMovementHeader>, NctsArrivalMovementHeader, UnloadingDifferencesDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override UnloadingDifferencesDetailsLayoutBuilder<NctsArrivalMovementHeader> GetColumnLayoutBuilderForTesting() => new UnloadingDifferencesDetailsLayoutBuilder<NctsArrivalMovementHeader>();
	}
}
