using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ImportInvoiceLineLayoutBuilder))]
	sealed class ImportInvoiceLineLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ImportInvoiceLineLayoutBuilder, JobComInvoiceLine, CommonInvoiceLineDetailsControlBag>
	{
		protected override ImportInvoiceLineLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			return new ImportInvoiceLineLayoutBuilder();
		}

		protected override bool ExpectedNarrowColumnForMediumControls => true;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
