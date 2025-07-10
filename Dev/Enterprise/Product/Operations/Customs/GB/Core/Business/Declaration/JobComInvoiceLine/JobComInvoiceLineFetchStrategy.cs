using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class JobComInvoiceLineFetchStrategy : EU.Business.FetchStrategies.JobComInvoiceLineFetchStrategy
	{
		public JobComInvoiceLineFetchStrategy(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(CusReferenceSchema.CFR_ParentID, BusinessObject.PK);
		}
	}
}
