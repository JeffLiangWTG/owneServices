using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class InsuranceCostsChargeTest : CustomsChargeCodeTest
{
	public void TestIsStatisticalValueApplicable()
	{
		CombineAssertions(() =>
		{
			var insuranceCosts = GetChargeCodeToTest();
			AssertEquals("IsStatisticalValueApplicable", true, insuranceCosts.IsStatisticalValueApplicable);
			AssertEquals("IsStatisticalValueApplicableDeemed", true, insuranceCosts.IsStatisticalValueApplicableDeemed);
		});
	}

	protected override string ExpectedCode => UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge;

	protected override string ExpectedDescription => UCCCustomsChargeTypeList.Descriptions.InsuranceCostsCharge;

	protected override bool ExpectedIsDutiable => true;

	protected override bool ExpectedIsVATible => true;

	protected override bool ExpectedIsDutiableDeemedForThisCharge => true;

	protected override bool ExpectedIsVATibleDeemedForThisCharge => true;

	protected override bool ExpectedIsIncludedInITOTDeemedForThisCharge => false;

	protected override bool ExpectedIsPercentageApplicable => true;

	protected override bool ExpectedIsIncoTermNeutral => true;

	protected override bool ExpectedGetCalculatedIncludedInITOT(ZString incoterm, bool userEnteredIsDutiable) => false;

	protected override bool ExpectedIsIncludedInInvoiceDeemedForThisCharge(ZString incoterm) =>
		incoterm != Core.Constants.IncoTerms.DeliveredAtFrontier &&
		incoterm != Core.Constants.IncoTerms.DeliveredAtPlace &&
		incoterm != Core.Constants.IncoTerms.DeliveredAtTerminal &&
		incoterm != Core.Constants.IncoTerms.CostAndFreight;

	protected override ChargeParentTypes ExpectedChargeParentTypes => ChargeParentTypes.GroupInvoice;

	protected override ICustomsChargeCode GetChargeCodeToTest() => CustomsChargeCodeProvider.GetNewInsuranceCostsCharge();
}
