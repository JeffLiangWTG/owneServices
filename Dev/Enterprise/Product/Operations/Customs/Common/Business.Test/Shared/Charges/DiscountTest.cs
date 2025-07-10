using CargoWise.Types;

namespace Enterprise.Customs.Common.Testing
{
	public class DiscountTest : CustomsChargeCodeTest
	{
		protected override bool ExpectedIsDutiable => false;

		protected override bool ExpectedIsDutiableDeemedForThisCharge => false;

		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;

		protected override bool ExpectedIsPercentageApplicable => true;

		protected override string ExpectedCode => CustomsChargeTypeList.Codes.Discount;

		protected override string ExpectedDescription => CustomsChargeTypeList.Descriptions.Discount;

		protected override bool ExpectedIsVATible => false;

		protected override bool ExpectedIsVATibleDeemedForThisCharge => false;

		protected override bool ExpectedIsIncoTermNeutral => true;

		protected override ICustomsChargeCode GetChargeCodeToTest() => new CommonIncoTermAndCustomsChargeFactory().GetCharge(CustomsChargeTypeList.Codes.Discount);

		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => false;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => false;
	}
}
