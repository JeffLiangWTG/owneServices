using CargoWise.Types;

namespace Enterprise.Customs.Common.Testing
{
	public class OverseasFreightTest : CustomsChargeCodeTest
	{
		protected override string ExpectedCode => CustomsChargeTypeList.Codes.OverseasFreight;

		protected override string ExpectedDescription => CustomsChargeTypeList.Descriptions.OverseasFreight;

		protected override ICustomsChargeCode GetChargeCodeToTest() => new CommonIncoTermAndCustomsChargeFactory().GetCharge(CustomsChargeTypeList.Codes.OverseasFreight);

		protected override bool ExpectedIsDutiable => false;

		protected override bool ExpectedIsDutiableDeemedForThisCharge => true;

		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;

		protected override bool ExpectedIsPercentageApplicable => false;

		protected override bool ExpectedIsVATible => true;

		protected override bool ExpectedIsVATibleDeemedForThisCharge => true;

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
}
