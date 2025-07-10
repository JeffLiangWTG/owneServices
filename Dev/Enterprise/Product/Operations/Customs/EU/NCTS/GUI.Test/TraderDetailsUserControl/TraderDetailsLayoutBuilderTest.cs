using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(TraderDetailsLayoutBuilder<NctsHeader>))]
	class TraderDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TraderDetailsLayoutBuilder<NctsHeader>, NctsHeader, TraderDetailsControlBag>
	{
		protected override TraderDetailsLayoutBuilder<NctsHeader> GetColumnLayoutBuilderForTesting() => new TraderDetailsLayoutBuilder<NctsHeader>();

		protected override int ExpectedMaxColumns => 1;
	}
}
