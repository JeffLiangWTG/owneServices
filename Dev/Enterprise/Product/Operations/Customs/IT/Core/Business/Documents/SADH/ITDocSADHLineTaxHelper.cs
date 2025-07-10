using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.Business;

static class ITDocSADHLineTaxHelper
{
	public static bool ExcludeDutiesInTotals(string methodOfPayment)
	{
		switch (methodOfPayment)
		{
			case DutyMethodOfPayment.GuaranteeAtTheInterventionBodyO:
			case DutyMethodOfPayment.SecurityDepositDeferredPaymentR:
			case DutyMethodOfPayment.IndividualGuaranteeS:
			case DutyMethodOfPayment.GuaranteeAccountInterestedPartyPermanentAuthorizationU:
			case DutyMethodOfPayment.GuaranteeAccountInterestedPartyIndividualAuthorizationV:
				return true;
			default:
				return false;
		}
	}
}
