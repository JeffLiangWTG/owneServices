using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class JobComInvoiceLineFetchStrategy : EU.Business.FetchStrategies.JobComInvoiceLineFetchStrategy
	{
		public JobComInvoiceLineFetchStrategy(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(CusHouseContPackInvoiceLinePivotSchema.CHC_JI, BusinessObject.PK);
		}
	}
}
