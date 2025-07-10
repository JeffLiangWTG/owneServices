using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public class NctsPackageConfiguration : EU.NCTS.Business.NctsPackageConfiguration
{
	protected override INctsPackagePhase5ValidationDecider GetDeparturePhase5ValidationDecider()
		=> new NctsPackageDeparturePhase5ValidationDecider();
}
