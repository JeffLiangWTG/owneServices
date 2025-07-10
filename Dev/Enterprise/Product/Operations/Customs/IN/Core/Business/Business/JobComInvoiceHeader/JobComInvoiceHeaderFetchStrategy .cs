using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IN.Business;

public class JobComInvoiceHeaderFetchStrategy : Customs.Business.FetchStrategies.BaseJobComInvoiceHeaderFetchStrategy
{
	public JobComInvoiceHeaderFetchStrategy(JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
	{
	}

	protected override void FetchForValidateCore()
	{
		base.FetchForValidateCore();
		Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
	}
}
