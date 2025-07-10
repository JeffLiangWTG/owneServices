using CargoWise.Types;

namespace Enterprise.Customs.Common.Testing
{
	public class CommissionTest : CustomsChargeCodeTest
	{
		protected override bool ExpectedIsDutiable => true;

		protected override bool ExpectedIsDutiableDeemedForThisCharge => false;

		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;

		protected override bool ExpectedIsPercentageApplicable => true;

		protected override string ExpectedCode => CustomsChargeTypeList.Codes.Commission;

		protected override string ExpectedDescription => CustomsChargeTypeList.Descriptions.Commission;

		protected override bool ExpectedIsVATible => true;

		protected override bool ExpectedIsVATibleDeemedForThisCharge => false;

		protected override bool ExpectedIsIncoTermNeutral => true;

		protected override ICustomsChargeCode GetChargeCodeToTest() => new CommonIncoTermAndCustomsChargeFactory().GetCharge(CustomsChargeTypeList.Codes.Commission);

		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => false;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => !userEnteredIsDutiable;
	}
}
