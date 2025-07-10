namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class InvoiceChargeValidationHelper : InvoiceChargeValidation
	{
		public InvoiceChargeValidationHelper(InvoiceCharge invoiceLineCharge)
			: base(invoiceLineCharge)
		{
		}

		public bool IsCIFComponentUsedExposed
		{
			get { return IsCIFComponentUsed; }
		}
	}
}
