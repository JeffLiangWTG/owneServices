namespace Enterprise.Customs.GB.Business.Declaration
{
	public class InvoiceChargeValidation
		: EU.Business.Declaration.InvoiceChargeValidation
	{
		public InvoiceChargeValidation(InvoiceCharge invoiceCharge)
			: base(invoiceCharge)
		{
		}

		public new InvoiceCharge Parent => (InvoiceCharge)base.Parent;
	}
}
