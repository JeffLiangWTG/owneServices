using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class CusTransportMeansConfiguration : EU.NCTS.Business.CusTransportMeansConfiguration
{
	protected override IDepartureCusTransportMeansPhase5ValidationDecider GetDepartureCusTransportMeansPhase5ValidationDecider() => new DepartureCusTransportMeansPhase5ValidationDecider();
}
