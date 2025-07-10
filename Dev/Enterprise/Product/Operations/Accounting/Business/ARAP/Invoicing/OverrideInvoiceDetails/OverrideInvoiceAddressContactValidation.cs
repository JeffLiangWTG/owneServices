namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class OverrideInvoiceAddressContactValidation : OverrideInvoiceDetailValidation
	{
		public OverrideInvoiceAddressContactValidation(InvoicingBase parent)
			: base(parent)
		{
		}

		public override void ValidateDisplayInvoiceAddressOverride()
		{
			InvoiceBaseValidation.ValidateDisplayInvoiceAddressOverride();
		}

		public override void ValidateDisplayInvoiceContactOverride()
		{
			InvoiceBaseValidation.ValidateDisplayInvoiceContactOverride();
		}

		InvoiceBaseValidation InvoiceBaseValidation
		{
			get
			{
				return invoiceBaseValidation ?? (invoiceBaseValidation = new InvoiceBaseValidation((InvoicingBase)Parent));
			}
		}

		InvoiceBaseValidation invoiceBaseValidation;
	}
}
