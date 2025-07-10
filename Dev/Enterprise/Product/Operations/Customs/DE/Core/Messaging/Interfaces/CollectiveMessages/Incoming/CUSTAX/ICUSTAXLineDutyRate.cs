using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSTAXLineDutyRate : IInboundProvider
	{
		string CriteriaType { get; }

		string AssessmentScale { get; }

		decimal Rate { get; }
	}
}
