using CargoWise.Types;

namespace Enterprise.Customs.Common.Testing
{
	public class ForeignInlandFreightTest : CustomsChargeCodeTest
	{
		protected override string ExpectedCode => CustomsChargeTypeList.Codes.ForeignInlandFreight;

		protected override string ExpectedDescription => CustomsChargeTypeList.Descriptions.ForeignInlandFreight;

		protected override ICustomsChargeCode GetChargeCodeToTest() => new CommonIncoTermAndCustomsChargeFactory().GetCharge(CustomsChargeTypeList.Codes.ForeignInlandFreight);

		protected override bool ExpectedIsDutiable => true;

		protected override bool ExpectedIsDutiableDeemedForThisCharge => false;

		protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;

		protected override bool ExpectedIsPercentageApplicable => false;

		protected override bool ExpectedIsVATible => true;

		protected override bool ExpectedIsVATibleDeemedForThisCharge => true;

		protected override bool ExpectedIsIncoTermNeutral => false;

		protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) => incoterm != Core.Constants.IncoTerms.FreeCarrier;

		protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => incoterm != Core.Constants.IncoTerms.ExWorks && incoterm != "XXX";
	}
}
