namespace Enterprise.Customs.EU.NCTS.Business;

public class CusAuthorizationUsageConfiguration
{
	public ICusAuthorizationUsagePhase5ValidationDecider GetValidationDecider(NctsHeader header) =>  header switch
	{
		{ IsPhase5: true } => GetPhase5ValidationDeciderCore(header),
		_ => null,
	};

	protected virtual ICusAuthorizationUsagePhase5ValidationDecider GetPhase5ValidationDeciderCore(NctsHeader header) => new CusAuthorizationUsagePhase5ValidationDecider();
}
