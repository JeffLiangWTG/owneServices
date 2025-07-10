using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5UASessionDetails
	{
		ZString PenaltyExemptionIndicator { get; }
		ZString PenaltyExemptionReasonCode { get; }
		ZString PenaltyExemptionReason { get; }
		ZInt DutyPenaltyExemption5UASequenceNumber { get; }
		ZDecimal PenaltyExemptionAmount { get; }
	}
}
