using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public sealed class GuaranteeConfiguration : EU.NCTS.Business.GuaranteeConfiguration
{
	protected override INctsGuaranteePhase5ValidationDecider GetGuaranteeeDeparturePhase5ValidationDecider() => new NctsGuaranteeDeparturePhase5ValidationDecider();
}
