using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NL.Business.Declaration;

public class JobDeclarationFetchStrategy : EU.Business.FetchStrategies.JobDeclarationFetchStrategy
{
	public JobDeclarationFetchStrategy(JobDeclaration declaration) : base(declaration)
	{
	}

	protected override void FetchForLoadCore()
	{
		base.FetchForLoadCore();
		Factory.AddFetchHint(CusEntryHeaderSchema.CH_JE, BusinessObject.PK);
		Factory.AddFetchHint(CusExitHeaderSchema.CXH_ParentID, BusinessObject.PK);
	}
}
