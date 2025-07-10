using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business;

public sealed class NctsPackageConfiguration : EU.NCTS.Business.NctsPackageConfiguration
{
	protected override INctsPackagePhase5ValidationDecider GetDeparturePhase5ValidationDecider() => new NctsPackageDeparturePhase5ValidationDecider();
}
