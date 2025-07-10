using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class NctsContainerConfiguration : EU.NCTS.Business.NctsContainerConfiguration
{
	public INctsContainerValidationDecider GetHeaderValidationDecider(NctsHeader header) => GetHeaderValidationDeciderCore(header);

	protected override INctsDepartureHeaderContainerPhase5ValidationDecider GetNctsDepartureHeaderContainerPhase5ValidationDecider() => new NctsDepartureHeaderContainerPhase5ValidationDecider();
}
