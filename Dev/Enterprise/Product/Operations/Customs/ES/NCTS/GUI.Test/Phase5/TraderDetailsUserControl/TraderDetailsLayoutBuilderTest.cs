using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Customs.ES.NCTS.GUI.TraderDetailsLayoutBuilder<Enterprise.Customs.ES.NCTS.Business.NctsHeader>;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(TraderDetailsLayoutBuilder<NctsHeader>))]
	class TraderDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TraderDetailsLayoutBuilder<NctsHeader>, NctsHeader, TraderDetailsControlBag>
	{
		protected override TraderDetailsLayoutBuilder<NctsHeader> GetColumnLayoutBuilderForTesting() => new TraderDetailsLayoutBuilder<NctsHeader>();

		protected override int ExpectedMaxColumns => 1;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => (ColumnLayoutBuilderCaptionWidthSize)CaptionWidthSize.Small;
	}
}
