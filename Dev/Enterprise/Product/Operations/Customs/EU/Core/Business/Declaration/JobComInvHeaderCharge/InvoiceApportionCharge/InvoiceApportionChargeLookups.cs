namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InvoiceApportionChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
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
