namespace Enterprise.Customs.EU.NCTS.Business;

public class CusSupplyChainActorReferenceConfiguration
{
	public ICusSupplyChainActorReferenceValidationDecider GetValidationDecider() => GetValidationDeciderCore();

	protected virtual ICusSupplyChainActorReferenceValidationDecider GetValidationDeciderCore() => new CusSupplyChainActorReferenceValidationDecider();
}
