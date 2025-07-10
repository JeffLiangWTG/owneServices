using Enterprise.Customs.Common;

namespace Enterprise.Customs.CN.Business.Testing
{
	class IncoTermAndCustomsChargeFactoryTest : Common.Testing.IncoTermAndCustomsChargeFactoryTest
	{
		public override void TestGetAllIncoTerms()
		{
			var allIncoTerms = incoTermAndChargeFactory.GetAllIncoTerms();
			CombineAssertions(() =>
			{
				AssertEquals("Length", 6, allIncoTerms.Length);
				AssertCollectionContains("CostAndInsurance", Core.Constants.IncoTerms.CostAndInsurance, allIncoTerms);
				AssertCollectionContains("CostInsuranceAndFreight", Core.Constants.IncoTerms.CostInsuranceAndFreight, allIncoTerms);
				AssertCollectionContains("CostAndFreight", Core.Constants.IncoTerms.CostAndFreight, allIncoTerms);
				AssertCollectionContains("FreeOnBoard", Core.Constants.IncoTerms.FreeOnBoard, allIncoTerms);
				AssertCollectionContains("ExWorks", Core.Constants.IncoTerms.ExWorks, allIncoTerms);
				AssertCollectionContains("ErrorIncoTermCode", IncoTermAndCustomsChargeFactory.ErrorIncoTermCode, allIncoTerms);
			});
		}

		public override void TestGetAllCharges()
		{
			var allCharges = incoTermAndChargeFactory.GetAllCharges();
			AssertEquals(11, allCharges.Length);
		}

		public override void TestGetCharge()
		{
			AssertGetCharge(CustomsChargeTypeList.Codes.OverseasFreight, IncoTermAndCustomsChargeFactory.OverseasFreight);
			AssertGetCharge(CustomsChargeTypeList.Codes.OverseasInsurance, IncoTermAndCustomsChargeFactory.OverseasInsurance);
			AssertGetCharge(CustomsChargeTypeList.Codes.Commission, CustomsChargeCodeProvider.Commission);
			AssertGetCharge(CustomsChargeTypeList.Codes.DeductionCharge, CustomsChargeCodeProvider.DeductionCharge);
			AssertGetCharge(CustomsChargeTypeList.Codes.ExWorks, CustomsChargeCodeProvider.ExWorks);
			AssertGetCharge(CustomsChargeTypeList.Codes.ForeignInlandFreight, CustomsChargeCodeProvider.ForeignInlandFreight);
			AssertGetCharge(CustomsChargeTypeList.Codes.LandingCharges, CustomsChargeCodeProvider.LandingCharges);
			AssertGetCharge(CustomsChargeTypeList.Codes.OtherCharges, CustomsChargeCodeProvider.OtherCharges);
			AssertGetCharge(CustomsChargeTypeList.Codes.PackingCost, CustomsChargeCodeProvider.PackingCost);
			AssertGetCharge(CustomsChargeTypeList.Codes.AdditionCharge, CustomsChargeCodeProvider.AdditionCharge);
			AssertGetCharge(CustomsChargeTypeList.Codes.Discount, CustomsChargeCodeProvider.Discount);
		}

		public virtual void TestIsIncludedInITOTIfDeemed()
		{
			CombineAssertions(() =>
			{
				AssertEquals("OverseasFreight IsIncludedInITOTIfDeemed", true, IncoTermAndCustomsChargeFactory.OverseasFreight.IsIncludedInITOTIfDeemed);
				AssertEquals("OverseasInsurance IsIncludedInITOTIfDeemed", true, IncoTermAndCustomsChargeFactory.OverseasInsurance.IsIncludedInITOTIfDeemed);
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\CN\Core\Business.Test\InvoiceCharges\TestFile\IncoTermAndCustomsChargeConfiguration.csv";

		protected override string GetCountryContext() => Core.Constants.CountryCodes.China;
	}
}
