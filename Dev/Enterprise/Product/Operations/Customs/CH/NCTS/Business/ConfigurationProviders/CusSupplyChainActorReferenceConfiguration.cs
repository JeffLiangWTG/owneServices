using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class CusSupplyChainActorReferenceConfiguration : EU.NCTS.Business.CusSupplyChainActorReferenceConfiguration
{
	protected override ICusSupplyChainActorReferenceValidationDecider GetValidationDeciderCore() => new CusSupplyChainActorReferenceValidationDecider();
}
