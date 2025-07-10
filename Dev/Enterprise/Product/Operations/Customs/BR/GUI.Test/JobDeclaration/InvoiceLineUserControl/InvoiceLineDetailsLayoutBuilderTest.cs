using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(InvoiceLineDetailsLayoutBuilder))]
	sealed class InvoiceLineDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<InvoiceLineDetailsLayoutBuilder, JobComInvoiceLine, CommonInvoiceLineDetailsControlBag>
	{
		public void TestEntryInstructionVisibility_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var layout = ((IPanelLayoutProvider)new ExportInvoiceLineDetailsLayout()).Layout;
			AssertEquals("EntryInstructionGuidDropEdit Should be visible", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, invoiceLine));

			declaration.MakeNonPersistent();
			AssertEquals("EntryInstructionGuidDropEdit Should be visible", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, invoiceLine));
		}

		public void TestEntryInstructionVisibility_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var layout = ((IPanelLayoutProvider)new ImportInvoiceLineDetailsLayout()).Layout;
			AssertEquals("EntryInstructionGuidDropEdit Should be visible", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, invoiceLine));

			declaration.MakeNonPersistent();
			AssertEquals("EntryInstructionGuidDropEdit Should be visible", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, invoiceLine));
		}

		protected override int ExpectedMaxColumns => 3;

		protected override bool ExpectedNarrowColumnForMediumControls => true;

		protected override InvoiceLineDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new InvoiceLineDetailsLayoutBuilder();
	}
}
