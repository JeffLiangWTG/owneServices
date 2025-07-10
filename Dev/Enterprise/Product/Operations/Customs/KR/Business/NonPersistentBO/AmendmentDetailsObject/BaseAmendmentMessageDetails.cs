using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public abstract class BaseAmendmentMessageDetails : IAmendmentDetails
	{
		protected BaseAmendmentMessageDetails()
		{ }

		public abstract ZInt AmendmentVersionNo { get; }
		public abstract ZString AmendmentType { get; }
		public abstract ZString AmendReasonDescription { get; }
		public abstract ZString ReasonCode { get; }
		public abstract ZString FaultParty { get; }
		public abstract ZString FaultPartyOtherDescription { get; }
		public abstract ZString PenaltyPaymentReasonCode { get; }
		public abstract ZDate DateOfFinalPrice { get; }

		ZInt IAmendmentDetails.AmendmentVersionNo => AmendmentVersionNo;
		ZString IAmendmentDetails.AmendmentType => AmendmentType;
		ZString IAmendmentDetails.AmendReasonDescription => AmendReasonDescription;
		ZString IAmendmentDetails.ReasonCode => ReasonCode;
		ZString IAmendmentDetails.FaultParty => FaultParty;
		ZString IAmendmentDetails.FaultPartyOtherDescription => FaultPartyOtherDescription;
		ZString IAmendmentDetails.PenaltyPaymentReasonCode => PenaltyPaymentReasonCode;
		ZDate IAmendmentDetails.DateOfFinalPrice => DateOfFinalPrice;
	}
}
