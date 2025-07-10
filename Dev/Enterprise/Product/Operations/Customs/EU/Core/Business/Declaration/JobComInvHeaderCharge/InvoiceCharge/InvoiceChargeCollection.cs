namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InvoiceChargeCollection<T> : Common.JobComInvChargeCollection<T> where T : InvoiceCharge
	{
		public InvoiceChargeCollection(JobComInvoiceHeader invoice)
			: base(invoice)
		{
		}
	}
}
