namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class InvoiceChargeValidation : Customs.Business.BaseInvoiceChargeValidation
	{
		public InvoiceChargeValidation(InvoiceCharge invoiceCharge)
			: base(invoiceCharge)
		{
		}

		public InvoiceCharge InvoiceCharge
		{
			get { return Parent; }
		}

		protected new InvoiceCharge Parent
		{
			get { return (InvoiceCharge)base.Parent; }
		}
	}
}
