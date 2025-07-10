using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5UASessionDetails : IImport5UASessionDetails
	{
		public ZString PenaltyExemptionReasonCode { get; set; }

		public ZString PenaltyExemptionReason { get; set; }

		public ZInt DutyPenaltyExemption5UASequenceNumber { get; set; }

		public ZDecimal PenaltyExemptionAmount { get; set; }

		ZString IImport5UASessionDetails.PenaltyExemptionIndicator => throw new System.NotImplementedException();

		ZString IImport5UASessionDetails.PenaltyExemptionReasonCode => PenaltyExemptionReasonCode;

		ZString IImport5UASessionDetails.PenaltyExemptionReason => PenaltyExemptionReason;

		ZInt IImport5UASessionDetails.DutyPenaltyExemption5UASequenceNumber => DutyPenaltyExemption5UASequenceNumber;

		ZDecimal IImport5UASessionDetails.PenaltyExemptionAmount => PenaltyExemptionAmount;
	}
}
