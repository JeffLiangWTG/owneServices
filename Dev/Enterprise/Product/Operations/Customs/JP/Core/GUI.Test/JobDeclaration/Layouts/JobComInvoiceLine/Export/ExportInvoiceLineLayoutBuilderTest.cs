using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ExportInvoiceLineLayoutBuilder))]
	sealed class ExportInvoiceLineLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ExportInvoiceLineLayoutBuilder, JobComInvoiceLine, CommonInvoiceLineDetailsControlBag>
	{
		protected override ExportInvoiceLineLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			return new ExportInvoiceLineLayoutBuilder();
		}

		protected override bool ExpectedNarrowColumnForMediumControls => true;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
