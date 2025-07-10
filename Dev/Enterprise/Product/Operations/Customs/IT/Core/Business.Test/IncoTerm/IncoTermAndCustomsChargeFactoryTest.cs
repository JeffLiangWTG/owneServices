using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class IncoTermAndCustomsChargeFactoryTest : UCCIncoTermAndCustomsChargeFactoryTest
{
	public void TestGetOverseasInsurance()
	{
		var itIncoTermAndChargeFactory = (IncoTermAndCustomsChargeFactory)incoTermAndChargeFactory;
		var insuranceCosts = itIncoTermAndChargeFactory.GetOverseasInsurance();
		CombineAssertions(() =>
		{
			AssertEquals(nameof(insuranceCosts.Code), UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, insuranceCosts.Code);
			AssertEquals(nameof(insuranceCosts.Description), UCCCustomsChargeTypeList.Descriptions.InsuranceCostsCharge, insuranceCosts.Description);
			AssertEquals(nameof(insuranceCosts.IsDutiable), true, insuranceCosts.IsDutiable);
			AssertEquals(nameof(insuranceCosts.IsDutiable), true, insuranceCosts.IsDutiable);
			AssertEquals(nameof(insuranceCosts.IsDutiableDeemedForThisCharge), true, insuranceCosts.IsDutiableDeemedForThisCharge);
			AssertEquals(nameof(insuranceCosts.IsVATible), true, insuranceCosts.IsVATible);
			AssertEquals(nameof(insuranceCosts.IsVATibleDeemedForThisCharge), true, insuranceCosts.IsVATibleDeemedForThisCharge);
			AssertEquals(nameof(insuranceCosts.IsStatisticalValueApplicable), true, insuranceCosts.IsStatisticalValueApplicable);
			AssertEquals(nameof(insuranceCosts.IsStatisticalValueApplicableDeemed), true, insuranceCosts.IsStatisticalValueApplicableDeemed);
			AssertEquals(nameof(insuranceCosts.IsIncoTermNeutral), true, insuranceCosts.IsIncoTermNeutral);
			AssertEquals(nameof(insuranceCosts.IsPercentageApplicable), true, insuranceCosts.IsPercentageApplicable);
			AssertEquals(nameof(insuranceCosts.ParentTypes), ChargeParentTypes.GroupInvoice, insuranceCosts.ParentTypes);
		});
	}

	public sealed override void TestFactoryType()
	{
		AssertEquals("IncoTermAndChargeFactory Type", ExpectedIncoTermAndChargeFactoryType, incoTermAndChargeFactory.GetType());
	}

	protected abstract Type ExpectedIncoTermAndChargeFactoryType { get; }
}
