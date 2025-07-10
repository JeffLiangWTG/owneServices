namespace Enterprise.Customs.CA.Business
{
	public class InvoiceLineChargeLookups : CommonInvoiceHeaderChargeLookups
	{
		public InvoiceLineChargeLookups(InvoiceLineCharge invoiceLineCharge)
			: base(invoiceLineCharge)
		{
		}

		public new InvoiceLineCharge Parent
		{
			get { return (InvoiceLineCharge)base.Parent; }
		}
	}
}
