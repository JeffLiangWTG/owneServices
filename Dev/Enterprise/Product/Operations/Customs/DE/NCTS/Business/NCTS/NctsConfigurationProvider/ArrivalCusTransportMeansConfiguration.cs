using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class ArrivalCusTransportMeansConfiguration : EU.NCTS.Business.ArrivalCusTransportMeansConfiguration
	{
		protected override IArrivalCusTransportMeansPhase5ValidationDecider GetArrivalPhase5ValidationDecider() => new ArrivalCusTransportMeansValidationDecider();
	}
}
