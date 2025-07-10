
namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface IArrivalCusTransportMeansValidationDecider
	{
	}

	public interface IArrivalCusTransportMeansPhase5ValidationDecider : IArrivalCusTransportMeansValidationDecider
	{
		bool IsRuleNR0081Active { get; }

		bool IsRuleNR0082Active { get; }
	}
}
