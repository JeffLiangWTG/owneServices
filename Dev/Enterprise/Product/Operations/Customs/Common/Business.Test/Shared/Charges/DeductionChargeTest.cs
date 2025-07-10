using CargoWise.Types;

namespace Enterprise.Customs.Common.Testing
{
	public class DeductionChargeTest : CustomsChargeCodeTest
	{
		protected override bool ExpectedIsDutiable => false;

		protected override bool ExpectedIsDutiableDeemedForThisCharge => true;

		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;

		protected override bool ExpectedIsPercentageApplicable => false;

		protected override string ExpectedCode => CustomsChargeTypeList.Codes.DeductionCharge;

		protected override string ExpectedDescription => CustomsChargeTypeList.Descriptions.DeductionCharge;

		protected override bool ExpectedIsVATible => false;

		protected override bool ExpectedIsVATibleDeemedForThisCharge => true;

		protected override bool ExpectedIsIncoTermNeutral => true;

		protected override ICustomsChargeCode GetChargeCodeToTest() => new CommonIncoTermAndCustomsChargeFactory().GetCharge(CustomsChargeTypeList.Codes.DeductionCharge);

		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => false;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => true;
	}
}
