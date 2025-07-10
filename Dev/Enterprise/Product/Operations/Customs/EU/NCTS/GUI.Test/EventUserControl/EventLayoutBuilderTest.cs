using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(EventLayoutBuilder<NctsHeader>))]
	class EventLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<EventLayoutBuilder<NctsHeader>, NctsHeader, EventControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override EventLayoutBuilder<NctsHeader> GetColumnLayoutBuilderForTesting() => new EventLayoutBuilder<NctsHeader>();
	}
}
