using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(InvoiceLineDetailsLongCaptionLayoutBuilder))]
	sealed class InvoiceLineDetailsLongCaptionLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<InvoiceLineDetailsLongCaptionLayoutBuilder, JobComInvoiceLine, CommonInvoiceLineDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override bool ExpectedNarrowColumnForMediumControls => true;

		protected override InvoiceLineDetailsLongCaptionLayoutBuilder GetColumnLayoutBuilderForTesting() => new InvoiceLineDetailsLongCaptionLayoutBuilder();
	}
}
