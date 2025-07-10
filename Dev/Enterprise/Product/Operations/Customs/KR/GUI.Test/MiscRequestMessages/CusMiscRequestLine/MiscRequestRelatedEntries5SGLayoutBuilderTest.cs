using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MiscRequestRelatedEntries5SGLayoutBuilder))]
	sealed class MiscRequestRelatedEntries5SGLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<MiscRequestRelatedEntries5SGLayoutBuilder, CusMiscRequestLine, MiscRequestRelatedEntriesControlBag>
	{
		protected override int ExpectedMaxColumns => 1;
		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
		protected override MiscRequestRelatedEntries5SGLayoutBuilder GetColumnLayoutBuilderForTesting() => new ();
	}
}
