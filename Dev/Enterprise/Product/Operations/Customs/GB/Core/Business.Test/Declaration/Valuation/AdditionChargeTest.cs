using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Testing;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	internal class AdditionChargeTest : CustomsChargeCodeTest
	{
		protected override string ExpectedCode => CustomsChargeTypeList.Codes.AdditionCharge;

		protected override string ExpectedDescription => CustomsChargeTypeList.Descriptions.AdditionCharge;

		protected override bool ExpectedIsDutiable => true;

		protected override bool ExpectedIsVATible => true;

		protected override bool ExpectedIsDutiableDeemedForThisCharge => true;

		protected override bool ExpectedIsVATibleDeemedForThisCharge => true;

		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;

		protected override bool ExpectedIsPercentageApplicable => false;

		protected override bool ExpectedIsIncoTermNeutral => true;

		protected override ChargeParentTypes ExpectedChargeParentTypes => ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => false;

		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => false;

		protected override ICustomsChargeCode GetChargeCodeToTest() => new CommonIncoTermAndCustomsChargeFactory().GetCharge(CustomsChargeTypeList.Codes.AdditionCharge);
	}
}
