namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class InvoiceLineApportionChargeValidationHelper : InvoiceLineApportionChargeValidation
	{
		public InvoiceLineApportionChargeValidationHelper(InvoiceLineApportionCharge invoiceLineCharge)
			: base(invoiceLineCharge)
		{
		}

		public bool IsCIFComponentUsedExposed
		{
			get { return IsCIFComponentUsed; }
		}
	}
}
