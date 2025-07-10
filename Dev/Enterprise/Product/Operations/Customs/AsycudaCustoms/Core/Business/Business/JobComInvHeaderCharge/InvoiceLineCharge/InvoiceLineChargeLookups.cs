namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class InvoiceLineChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
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
