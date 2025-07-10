namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InvoiceChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
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
