using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobComInvoiceHeaderFetchStrategy : EU.Business.FetchStrategies.JobComInvoiceHeaderFetchStrategy
{
	public JobComInvoiceHeaderFetchStrategy(EU.Business.Declaration.JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
	{
	}

	protected override void FetchForLoadCore()
	{
		base.FetchForLoadCore();
		Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
		Factory.AddFetchHint(OrgHeaderSchema.PK, BusinessObject.JZ_OH_Supplier);
		Factory.AddFetchHint(OrgAddressSchema.PK, BusinessObject.JZ_OA_SupplierAddress);
	}
}
