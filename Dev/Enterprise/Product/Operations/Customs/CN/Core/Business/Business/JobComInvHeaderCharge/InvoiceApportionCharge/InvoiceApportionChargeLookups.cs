namespace Enterprise.Customs.CN.Business
{
	public class InvoiceApportionChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public InvoiceApportionChargeLookups(InvoiceApportionCharge invoiceApportionCharge)
			: base(invoiceApportionCharge)
		{
		}

		public new InvoiceApportionCharge Parent => (InvoiceApportionCharge)base.Parent;
	}
}
