using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.FetchStrategies
{
	public class JobComInvoiceLineFetchStrategy : Customs.Business.FetchStrategies.JobComInvoiceLineFetchStrategy
	{
		public JobComInvoiceLineFetchStrategy(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			var euDeclaration = (JobDeclaration)BusinessObject.Declaration;

			base.FetchForLoadChildEditableObjectsCore();
			if (BusinessObject.Declaration?.IsImport ?? false)
			{
				Factory.AddFetchHint(CusLineTariffDetailSchema.BZ_ParentID, BusinessObject.PK);
			}

			var fetchOnlyFromLocalCache = !BusinessObject.IsInDatabase;
			Factory.AddFetchHintWithClusterKey(BusinessObject.JI_ClusterKey, BusinessObject.PK, typeof(CusAuthorizationUsage), CusAuthorizationUsageSchema.AGC_ClusterKey, CusAuthorizationUsageSchema.AGC_ParentID, fetchOnlyFromLocalCache);

			Factory.AddFetchHint(CusReferenceSchema.CFR_ParentID, BusinessObject.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(CusReferenceSchema.CFR_ParentID, BusinessObject.PK);
		}
	}
}
