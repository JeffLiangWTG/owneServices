namespace Enterprise.Customs.IE.Business.Declaration
{
	public class InvoiceLineExportSupportingDocumentValidation : ExportSupportingDocumentValidation
	{
		public InvoiceLineExportSupportingDocumentValidation(SupportingDocument parent) : base(parent)
		{
		}

		protected override void CheckCSI_UnitOfQuantity()
		{
			InvoiceLineSupportingDocumentValidation.CheckCSI_UnitOfQuantity(Parent);
		}

		protected override void CheckCSI_RX_NKCurrency()
		{
			InvoiceLineSupportingDocumentValidation.CheckCSI_RX_NKCurrency(Parent);
		}
	}
}
