using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public partial class ExportComInvoiceLineUserControl : ExportInvoiceLineUserControl
	{
		public ExportComInvoiceLineUserControl()
		{
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();

			var entryInstructionInfo = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(entryInstructionInfo);
			var entryInstructionDescriptionInfo = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.EntryInstructionDescription);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(entryInstructionDescriptionInfo);
			var entryInstructionReferenceNumberInfo = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.EntryReferenceNumber);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(entryInstructionReferenceNumberInfo);
			var mergedLineNumberInfo = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.MergedLineNumber);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(mergedLineNumberInfo);
		}
	}
}
