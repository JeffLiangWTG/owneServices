using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class CusSupplyChainActorReferenceValidationDecider : ICusSupplyChainActorReferenceValidationDecider
{
	public bool IsRuleR0840Active => false;
}
