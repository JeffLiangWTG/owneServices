using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class JobComInvoiceLineFetchStrategy : Customs.Business.FetchStrategies.JobComInvoiceLineFetchStrategy
	{
		public JobComInvoiceLineFetchStrategy(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		protected new JobComInvoiceLine BusinessObject => (JobComInvoiceLine)base.BusinessObject;

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(CusVehicleSchema.CVH_ParentID, BusinessObject.PK);
		}
	}
}
