using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(UnloadingDetailsLayoutBuilder<NctsArrivalMovementHeader>))]
	class UnloadingDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<UnloadingDetailsLayoutBuilder<NctsArrivalMovementHeader>, NctsArrivalMovementHeader, UnloadingDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override UnloadingDetailsLayoutBuilder<NctsArrivalMovementHeader> GetColumnLayoutBuilderForTesting() => new UnloadingDetailsLayoutBuilder<NctsArrivalMovementHeader>();
	}
}
