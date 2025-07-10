using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using JobComInvoiceLine = Enterprise.Customs.IE.Business.Declaration.JobComInvoiceLine;

namespace Enterprise.Customs.IE.GUI.Testing
{
	public class ExportComInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestRemovedColumns()
		{
			using (var form = new ZForm())
			using (var control = new ExportComInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.InitializeGridLayout();

				var entryInstructionInfo = control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI);
				AssertNull("JobComInvoiceLine.Schema.JI_CEI", entryInstructionInfo);
				var entryInstructionDescriptionInfo = control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.EntryInstructionDescription);
				AssertNull("JobComInvoiceLine.Schema.EntryInstructionDescription", entryInstructionDescriptionInfo);
				var entryInstructionReferenceNumberInfo = control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.EntryReferenceNumber);
				AssertNull("JobComInvoiceLine.Schema.EntryReferenceNumber", entryInstructionReferenceNumberInfo);
				var mergedLineNumberInfo = control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.MergedLineNumber);
				AssertNull("JobComInvoiceLine.Schema.MergedLineNumber", mergedLineNumberInfo);
			}
		}
	}
}
