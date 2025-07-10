using CargoWise.Types;

namespace Enterprise.Customs.Common.Testing
{
	public class LandingChargesTest : CustomsChargeCodeTest
	{
		protected override string ExpectedCode => CustomsChargeTypeList.Codes.LandingCharges;

		protected override string ExpectedDescription => CustomsChargeTypeList.Descriptions.LandingCharges;

		protected override ICustomsChargeCode GetChargeCodeToTest() => new CommonIncoTermAndCustomsChargeFactory().GetCharge(CustomsChargeTypeList.Codes.LandingCharges);

		protected override bool ExpectedIsDutiable => false;

		protected override bool ExpectedIsDutiableDeemedForThisCharge => true;

		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;

		protected override bool ExpectedIsPercentageApplicable => false;

		protected override bool ExpectedIsVATible => false;

		protected override bool ExpectedIsVATibleDeemedForThisCharge => true;

		protected override bool ExpectedIsIncoTermNeutral => false;

		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => incoterm != Core.Constants.IncoTerms.DeliveredAtTerminal && incoterm != Core.Constants.IncoTerms.DeliveredAtPlace;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable)
		{
			return incoterm == Core.Constants.IncoTerms.DeliveredDutyPaid ||
				incoterm == Core.Constants.IncoTerms.DeliveredAtPlace ||
				incoterm == Core.Constants.IncoTerms.DeliveredAtTerminal;
		}
	}
}
