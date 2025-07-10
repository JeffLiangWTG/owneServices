using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CAAdditionChargeTest : CustomsChargeCodeTest
	{
		protected override string ExpectedCode => CAChargeTypeList.Codes.AdditionCharge;

		protected override string ExpectedDescription => CAChargeTypeList.Descriptions.AdditionCharge;

		protected override bool ExpectedIsDutiable => true;

		protected override bool ExpectedIsDutiableDeemedForThisCharge => true;

		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;

		protected override bool ExpectedIsPercentageApplicable => true;

		protected override bool ExpectedIsVATible => true;

		protected override bool ExpectedIsVATibleDeemedForThisCharge => true;

		protected override bool ExpectedIsIncoTermNeutral => true;

		protected override ICustomsChargeCode GetChargeCodeToTest() => IncoTermAndCustomsChargeFactory.AdditionCharge;

		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => false;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => false;
	}
}
