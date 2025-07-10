using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	sealed class JobComInvoiceLineFetchStrategy : Customs.Business.FetchStrategies.JobComInvoiceLineFetchStrategy
	{
		public JobComInvoiceLineFetchStrategy(JobComInvoiceLine invoiceLine)
				: base(invoiceLine)
		{
		}

		new JobComInvoiceLine BusinessObject => (JobComInvoiceLine)base.BusinessObject;

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(JobComInvoiceLineTaxSchema.JLT_JI, BusinessObject.PK);
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(JobComInvLineRefsSchema.JG_JI, BusinessObject.PK);
			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(CusLineTariffDetailSchema.BZ_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(JobComInvoiceLineTaxSchema.JLT_JI, BusinessObject.PK);
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
		}
	}
}
