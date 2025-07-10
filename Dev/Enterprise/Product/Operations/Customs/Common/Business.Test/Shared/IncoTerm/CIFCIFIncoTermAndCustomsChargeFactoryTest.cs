namespace Enterprise.Customs.Common.Testing
{
	using CargoWise.Types;
	using Enterprise.Customs.Common;

	class CIFCIFOverseasFreightTest : CustomsChargeCodeTest
	{
		protected override string ExpectedCode => CustomsChargeTypeList.Codes.OverseasFreight;

		protected override string ExpectedDescription => CustomsChargeTypeList.Descriptions.OverseasFreight;

		protected override ICustomsChargeCode GetChargeCodeToTest() => new CIFCIFIncoTermAndCustomsChargeFactory().GetCharge(CustomsChargeTypeList.Codes.OverseasFreight);

		protected override bool ExpectedIsDutiable => true;

		protected override bool ExpectedIsDutiableDeemedForThisCharge => true;

		protected override bool ExpectedIsVATible => true;

		protected override bool ExpectedIsVATibleDeemedForThisCharge => true;

		protected override bool ExpectedIsPercentageApplicable => false;

		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;

		protected override bool ExpectedIsIncoTermNeutral => false;

		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => incoterm != Core.Constants.IncoTerms.DeliveredAtTerminal;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable)
		{
			return incoterm != Core.Constants.IncoTerms.ExWorks &&
				incoterm != Core.Constants.IncoTerms.FreeCarrier &&
				incoterm != Core.Constants.IncoTerms.FreeAlongsideShip &&
				incoterm != Core.Constants.IncoTerms.FreeOnBoard &&
				incoterm != "XXX";
		}
	}

	class CIFCIFOverseasInsuranceTest : CustomsChargeCodeTest
	{
		protected override string ExpectedCode => CustomsChargeTypeList.Codes.OverseasInsurance;

		protected override string ExpectedDescription => CustomsChargeTypeList.Descriptions.OverseasInsurance;

		protected override ICustomsChargeCode GetChargeCodeToTest() => new CIFCIFIncoTermAndCustomsChargeFactory().GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance);

		protected override bool ExpectedIsDutiable => true;

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

	class CIFCIFIncoTermAndCustomsChargeFactoryTest : IncoTermAndCustomsChargeFactoryTest
	{
	}
}
