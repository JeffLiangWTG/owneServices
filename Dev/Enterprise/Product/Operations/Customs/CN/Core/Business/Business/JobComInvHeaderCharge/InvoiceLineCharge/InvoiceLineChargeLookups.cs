namespace Enterprise.Customs.CN.Business
{
	public class InvoiceLineChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public InvoiceLineChargeLookups(InvoiceLineCharge invoiceLineCharge)
			: base(invoiceLineCharge)
		{
		}

		public new InvoiceLineCharge Parent => (InvoiceLineCharge)base.Parent;
	}
}
