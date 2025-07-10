namespace Enterprise.Customs.IE.Business.Declaration
{
	public class InvoiceLineImportPreviousDocumentValidation : ImportPreviousDocumentValidation
	{
		public InvoiceLineImportPreviousDocumentValidation(PreviousDocument parent) : base(parent)
		{
		}

		protected override void CheckCSI_PackQty()
		{
			InvoiceLinePreviousDocumentValidation.CheckCSI_PackQty(Parent);
		}

		protected override void CheckCSI_PackType()
		{
			InvoiceLinePreviousDocumentValidation.CheckCSI_PackType(Parent);
		}

		protected override void CheckCSI_Quantity()
		{
			InvoiceLinePreviousDocumentValidation.CheckCSI_Quantity(Parent);
		}

		protected override void CheckCSI_UnitOfQuantity()
		{
			InvoiceLinePreviousDocumentValidation.CheckCSI_UnitOfQuantity(Parent);
		}

		protected override void CheckCSI_ItemNumber()
		{
			InvoiceLinePreviousDocumentValidation.CheckCSI_ItemNumber(Parent);
		}
	}
}
