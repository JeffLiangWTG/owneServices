using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobComInvoiceHeaderFetchStrategy : Customs.Business.FetchStrategies.BaseJobComInvoiceHeaderFetchStrategy
	{
		public JobComInvoiceHeaderFetchStrategy(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected new JobComInvoiceHeader BusinessObject
		{
			get { return (JobComInvoiceHeader)base.BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, BusinessObject.PK);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(CusStorageDocPivotSchema.CSD_ParentID, BusinessObject.PK);
		}
	}
}
