namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class GroupInvoiceChargeValidationHelper : GroupInvoiceChargeValidation
	{
		public GroupInvoiceChargeValidationHelper(GroupInvoiceCharge invoiceLineCharge)
			: base(invoiceLineCharge)
		{
		}

		public bool IsCIFComponentUsedExposed
		{
			get { return IsCIFComponentUsed; }
		}
	}
}
