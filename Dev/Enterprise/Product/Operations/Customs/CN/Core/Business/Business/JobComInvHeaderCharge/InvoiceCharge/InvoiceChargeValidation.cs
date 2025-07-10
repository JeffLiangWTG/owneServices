namespace Enterprise.Customs.CN.Business
{
	public class InvoiceChargeValidation : Customs.Business.BaseInvoiceChargeValidation
	{
		public InvoiceChargeValidation(InvoiceCharge invoiceCharge)
			: base(invoiceCharge)
		{
		}

		public InvoiceCharge InvoiceCharge => Parent;

		protected new InvoiceCharge Parent => (InvoiceCharge)base.Parent;
	}
}
