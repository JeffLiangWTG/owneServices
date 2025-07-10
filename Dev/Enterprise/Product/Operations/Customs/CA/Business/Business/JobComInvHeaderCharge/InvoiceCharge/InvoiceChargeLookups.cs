namespace Enterprise.Customs.CA.Business
{
	public class InvoiceChargeLookups : CommonInvoiceHeaderChargeLookups
	{
		public InvoiceChargeLookups(InvoiceCharge invoiceCharge)
			: base(invoiceCharge)
		{
		}

		public new InvoiceCharge Parent
		{
			get { return (InvoiceCharge)base.Parent; }
		}
	}
}
