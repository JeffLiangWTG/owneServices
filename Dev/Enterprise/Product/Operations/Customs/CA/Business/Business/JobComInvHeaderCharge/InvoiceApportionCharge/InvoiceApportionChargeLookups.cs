namespace Enterprise.Customs.CA.Business
{
	public class InvoiceApportionChargeLookups : CommonInvoiceHeaderChargeLookups
	{
		public InvoiceApportionChargeLookups(InvoiceApportionCharge invoiceApportionCharge)
			: base(invoiceApportionCharge)
		{
		}

		public new InvoiceApportionCharge Parent
		{
			get { return (InvoiceApportionCharge)base.Parent; }
		}
	}
}
