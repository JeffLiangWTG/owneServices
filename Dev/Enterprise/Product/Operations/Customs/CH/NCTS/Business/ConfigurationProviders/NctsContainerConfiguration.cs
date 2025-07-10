using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NctsContainerConfiguration : EU.NCTS.Business.NctsContainerConfiguration
{
	protected override INctsArrivalHeaderContainerPhase5ValidationDecider GetNctsArrivalHeaderContainerPhase5ValidationDecider() => new NctsArrivalHeaderContainerPhase5ValidationDecider();

	protected override INctsDepartureHeaderContainerPhase5ValidationDecider GetNctsDepartureHeaderContainerPhase5ValidationDecider() => new NctsDepartureHeaderContainerPhase5ValidationDecider();
}
