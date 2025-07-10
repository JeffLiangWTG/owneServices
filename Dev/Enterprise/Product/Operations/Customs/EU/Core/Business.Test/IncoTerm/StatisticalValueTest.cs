using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	class StatisticalValueTest : CustomsChargeCodeTest
	{
		protected override string ExpectedCode => ChargeTypeList.Codes.StatisticalValue;

		protected override string ExpectedDescription => ChargeTypeList.Descriptions.StatisticalValue;

		protected override ICustomsChargeCode GetChargeCodeToTest() => ChargeCodeProvider.StatisticalValue;

		protected override bool ExpectedIsDutiable => false;

		protected override bool ExpectedIsDutiableDeemedForThisCharge => true;

		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => true;

		protected override bool ExpectedIsPercentageApplicable => false;

		protected override bool ExpectedIsVATible => false;

		protected override bool ExpectedIsVATibleDeemedForThisCharge => true;

		protected override bool ExpectedIsIncoTermNeutral => false;

		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => false;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => false;

		protected override ChargeParentTypes ExpectedChargeParentTypes => ChargeParentTypes.InvoiceLine;
	}
}
