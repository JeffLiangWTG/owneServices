using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CAConstructionTest : CustomsChargeCodeTest
	{
		protected override string ExpectedCode => CAChargeTypeList.Codes.Construction;

		protected override string ExpectedDescription => CAChargeTypeList.Descriptions.Construction;

		protected override bool ExpectedIsDutiable => false;

		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;

		protected override bool ExpectedIsPercentageApplicable => false;

		protected override bool ExpectedIsVATible => false;

		protected override bool ExpectedIsDutiableDeemedForThisCharge => false;

		protected override bool ExpectedIsVATibleDeemedForThisCharge => false;

		protected override bool ExpectedIsIncoTermNeutral => true;

		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => false;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => true;

		protected override ICustomsChargeCode GetChargeCodeToTest() => IncoTermAndCustomsChargeFactory.Construction;
	}
}
