using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IAmendmentDetails
	{
		ZInt AmendmentVersionNo { get; }
		ZString AmendmentType { get; }
		ZString AmendReasonDescription { get; }
		ZString ReasonCode { get; }
		ZString FaultParty { get; }
		ZString FaultPartyOtherDescription { get; }
		ZString PenaltyPaymentReasonCode { get; }
		ZDate DateOfFinalPrice { get; }
	}
}
