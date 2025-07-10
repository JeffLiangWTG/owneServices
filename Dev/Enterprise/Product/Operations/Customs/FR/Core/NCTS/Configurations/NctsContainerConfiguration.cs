using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Configurations
{
	public sealed class NctsContainerConfiguration : EU.NCTS.Business.NctsContainerConfiguration
	{
		protected override EU.NCTS.Business.INctsArrivalHeaderContainerPhase5ValidationDecider GetNctsArrivalHeaderContainerPhase5ValidationDecider() => new NctsArrivalHeaderContainerPhase5ValidationDecider();
	}
}
