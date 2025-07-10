using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(ExportInvoiceLineDetailsLayoutBuilder))]
	sealed class ExportInvoiceLineDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ExportInvoiceLineDetailsLayoutBuilder, JobComInvoiceLine, CommonInvoiceLineDetailsControlBag>
	{
		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int ExpectedMaxColumns => 3;

		protected override bool ExpectedNarrowColumnForMediumControls => true;

		protected override ExportInvoiceLineDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new ExportInvoiceLineDetailsLayoutBuilder();

		protected override void SetUp()
		{
			base.SetUp();
			Factory.New<JobComInvoiceLine>();
		}
	}
}
