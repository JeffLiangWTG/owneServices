using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Business
{
	sealed class JobComInvoiceHeaderFetchStrategy : Customs.Business.FetchStrategies.BaseJobComInvoiceHeaderFetchStrategy
	{
		public JobComInvoiceHeaderFetchStrategy(BaseJobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
		{
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(CusReferenceSchema.CFR_ParentID, BusinessObject.PK);
		}
	}
}
