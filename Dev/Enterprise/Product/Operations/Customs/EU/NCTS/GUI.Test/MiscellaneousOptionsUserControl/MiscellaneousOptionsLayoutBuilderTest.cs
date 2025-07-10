using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(MiscellaneousOptionsLayoutBuilder<NctsHeader>))]
	sealed class MiscellaneousOptionsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<MiscellaneousOptionsLayoutBuilder<NctsHeader>, NctsHeader, MiscellaneousOptionsControlBag>
	{
		protected override MiscellaneousOptionsLayoutBuilder<NctsHeader> GetColumnLayoutBuilderForTesting() => new MiscellaneousOptionsLayoutBuilder<NctsHeader>();

		protected override int ExpectedMaxColumns => 1;
	}
}
