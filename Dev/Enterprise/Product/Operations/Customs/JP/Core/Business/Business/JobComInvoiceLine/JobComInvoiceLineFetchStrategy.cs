using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Business
{
	public class JobComInvoiceLineFetchStrategy : Customs.Business.FetchStrategies.JobComInvoiceLineFetchStrategy
	{
		public JobComInvoiceLineFetchStrategy(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(CusLineTariffDetailSchema.BZ_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(CusReferenceSchema.CFR_ParentID, BusinessObject.PK);
		}
	}
}
