using CargoWise.Types;

namespace Enterprise.Customs.Common.Testing
{
	public class OtherChargesTest : CustomsChargeCodeTest
	{
		protected override string ExpectedCode => CustomsChargeTypeList.Codes.OtherCharges;

		protected override string ExpectedDescription => CustomsChargeTypeList.Descriptions.OtherCharges;

		protected override ICustomsChargeCode GetChargeCodeToTest() => new CommonIncoTermAndCustomsChargeFactory().GetCharge(CustomsChargeTypeList.Codes.OtherCharges);

		protected override bool ExpectedIsDutiable => true;

		protected override bool ExpectedIsDutiableDeemedForThisCharge => false;

		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;

		protected override bool ExpectedIsPercentageApplicable => false;

		protected override bool ExpectedIsVATible => true;

		protected override bool ExpectedIsVATibleDeemedForThisCharge => false;

		protected override bool ExpectedIsIncoTermNeutral => true;

		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => false;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => !userEnteredIsDutiable;
	}
}
