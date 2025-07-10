using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class CarrierProvider : PartyProvider
	{
		public CarrierProvider(JobDocAddress jobDocAddress, bool isInPhase5TransitionPeriod) : base(jobDocAddress, isInPhase5TransitionPeriod)
		{
		}
	}
}
