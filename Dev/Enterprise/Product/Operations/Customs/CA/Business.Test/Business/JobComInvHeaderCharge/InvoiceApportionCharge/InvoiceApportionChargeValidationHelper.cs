namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class InvoiceApportionChargeValidationHelper : InvoiceApportionChargeValidation
	{
		public InvoiceApportionChargeValidationHelper(InvoiceApportionCharge invoiceLineCharge)
			: base(invoiceLineCharge)
		{
		}

		public bool IsCIFComponentUsedExposed
		{
			get { return IsCIFComponentUsed; }
		}
	}
}
