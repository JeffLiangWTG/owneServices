using CargoWise.Types;

namespace Enterprise.Customs.Common.Testing
{
	public class OverseasInsuranceTest : CustomsChargeCodeTest
	{
		protected override string ExpectedCode => CustomsChargeTypeList.Codes.OverseasInsurance;

		protected override string ExpectedDescription => CustomsChargeTypeList.Descriptions.OverseasInsurance;

		protected override ICustomsChargeCode GetChargeCodeToTest() => new CommonIncoTermAndCustomsChargeFactory().GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance);

		protected override bool ExpectedIsDutiable => false;

		protected override bool ExpectedIsDutiableDeemedForThisCharge => true;

		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;

		protected override bool ExpectedIsPercentageApplicable => true;

		protected override bool ExpectedIsVATible => true;

		protected override bool ExpectedIsVATibleDeemedForThisCharge => true;

		protected override bool ExpectedIsIncoTermNeutral => false;

		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) =>
			incoterm != Core.Constants.IncoTerms.DeliveredAtPlace
			&& incoterm != Core.Constants.IncoTerms.DeliveredAtTerminal
			&& incoterm != Core.Constants.IncoTerms.DeliveredDutyPaid;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable)
		{
			return incoterm == Core.Constants.IncoTerms.CarriageAndInsurancePaidTo ||
				incoterm == Core.Constants.IncoTerms.CostAndInsurance ||
				incoterm == Core.Constants.IncoTerms.CostInsuranceAndFreight ||
				incoterm == Core.Constants.IncoTerms.DeliveredDutyPaid ||
				incoterm == Core.Constants.IncoTerms.DeliveredAtPlace ||
				incoterm == Core.Constants.IncoTerms.DeliveredAtTerminal;
		}
	}
}

