using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

public class JobDeclarationFetchStrategy : Customs.Business.FetchStrategies.BaseJobDeclarationFetchStrategy
{
	public JobDeclarationFetchStrategy(JobDeclaration jobDeclaration)
		: base(jobDeclaration)
	{
	}

	protected override void FetchForLoadCore()
	{
		base.FetchForLoadCore();
		Factory.AddFetchHint(CusEntryHeaderSchema.CH_JE, BusinessObject.PK);
	}

	protected override void AddMergeFetchHintsFor(BaseJobComInvoiceLine invoiceLine)
	{
		base.AddMergeFetchHintsFor(invoiceLine);
		Factory.AddFetchHint(CusLineTariffDetailSchema.BZ_ParentID, invoiceLine.PK);
	}
}

