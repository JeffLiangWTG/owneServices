using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business;

public sealed class NctsContainerConfiguration : EU.NCTS.Business.NctsContainerConfiguration
{
	protected override INctsDepartureHeaderContainerPhase5ValidationDecider GetNctsDepartureHeaderContainerPhase5ValidationDecider() => new NctsDepartureHeaderContainerPhase5ValidationDecider();
}
