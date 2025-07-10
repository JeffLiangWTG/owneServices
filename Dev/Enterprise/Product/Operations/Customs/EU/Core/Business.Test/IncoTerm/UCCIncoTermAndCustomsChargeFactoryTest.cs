using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class UCCIncoTermAndCustomsChargeFactoryTest : EUIncoTermAndCustomsChargeFactoryTest
	{
		public override void TestFactoryType()
		{
			AssertEquals(typeof(UCCIncoTermAndCustomsChargeFactory), incoTermAndChargeFactory.GetType());
		}

		public override void TestGetAllIncoTerms()
		{
			AssertEquals("Count", 16, incoTermAndChargeFactory.GetAllIncoTerms().Length);
		}

		public override void TestGetAllCharges()
		{
			AssertEquals(25, incoTermAndChargeFactory.GetAllCharges().Length);
		}

		public override void TestGetCharge()
		{
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, UCCChargesProvider.CommissionAndBrokerage);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge, UCCChargesProvider.ContainersAndPacking);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, UCCChargesProvider.MaterialsComponentsParts);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, UCCChargesProvider.ToolsMiesMoulds);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, UCCChargesProvider.MaterialsConsumed);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge, UCCChargesProvider.EngineeringDevelopmentArtwork);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, UCCChargesProvider.RoyaltiesLicenseFee);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge, UCCChargesProvider.ProceedsOfAnySubsequentResale);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge, UCCChargesProvider.IndirectAndOtherPayments);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, UCCChargesProvider.InsuranceCosts);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.Additions71Charge, UCCChargesProvider.Additions71);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, UCCChargesProvider.TransportCosts);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge, UCCChargesProvider.OtherNotElsewhereDeclared);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.AdjustmentCharge, UCCChargesProvider.Adjustment);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, UCCChargesProvider.AirTransportCosts);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, UCCChargesProvider.ConstructionErectionAssembly);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, UCCChargesProvider.ImportDutiesOrOther);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.InterestCharge, UCCChargesProvider.Interest);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.RightToReproduceCharge, UCCChargesProvider.RightToReproduce);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge, UCCChargesProvider.BuyingCommissions);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.Deductions71Charge, UCCChargesProvider.Deductions71);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.DiscountNotElsewhereDeclaredCharge, UCCChargesProvider.DiscountNotElsewhereDeclared);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.DeductionsNotElsewhereDeclaredCharge, UCCChargesProvider.DeductionsNotElsewhereDeclared);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge, UCCChargesProvider.CommissionExceptBuyingCommissions);
			AssertGetCharge(ChargeTypeList.Codes.StatisticalValue, ChargeCodeProvider.StatisticalValue);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\EU\Core\Business.Test\IncoTerm\TestFiles\UCCIncoTermAndCustomsChargeConfiguration.csv";
		protected override string FreightToEUBorderCode => UCCCustomsChargeTypeList.Codes.TransportCostsCharge;
		protected override string FreightAfterEUBorderCode => UCCCustomsChargeTypeList.Codes.TransportCostsCharge;

		protected override string GetCountryContext() => "EUUCC";
	}

	sealed class UCCIncoTermAndCustomsChargeFactoryBaseOnlyTest : TestCaseWithFactory
	{
		public void TestGetOverseasInsurance()
		{
			var uccIncoTermAndCustomsChargeFactory = new UCCIncoTermAndCustomsChargeFactory();
			var insuranceCosts = uccIncoTermAndCustomsChargeFactory.GetOverseasInsurance();
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
				AssertEquals(nameof(insuranceCosts.IsPercentageApplicable), false, insuranceCosts.IsPercentageApplicable);
				AssertEquals(nameof(insuranceCosts.ParentTypes), ChargeParentTypes.GroupInvoice, insuranceCosts.ParentTypes);
			});
		}
	}
}
