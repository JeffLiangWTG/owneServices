namespace Enterprise.Customs.CN.Business.Testing
{
	sealed class ImportIncoTermAndCustomsChargeFactoryTest : IncoTermAndCustomsChargeFactoryTest
	{
		public override void TestIsIncludedInITOTIfDeemed()
		{
			var importFactory = (ImportIncoTermAndCustomsChargeFactory)Common.IncoTermAndCustomsChargeFactory.GetByCountryCode("CNIMP");
			CombineAssertions(() =>
			{
				AssertEquals("OverseasFreight IsIncludedInITOTIfDeemed", false, importFactory.GetOverseasFreight().IsIncludedInITOTIfDeemed);
				AssertEquals("OverseasInsurance IsIncludedInITOTIfDeemed", false, importFactory.GetOverseasFreight().IsIncludedInITOTIfDeemed);
			});
		}

		public override void TestGetAllIncoTerms()
		{
			var allIncoTerms = incoTermAndChargeFactory.GetAllIncoTerms();
			AssertEquals(6, allIncoTerms.Length);
			AssertCollectionContains(Core.Constants.IncoTerms.CostAndInsurance, allIncoTerms);
			AssertCollectionContains(Core.Constants.IncoTerms.CostInsuranceAndFreight, allIncoTerms);
			AssertCollectionContains(Core.Constants.IncoTerms.CostAndFreight, allIncoTerms);
			AssertCollectionContains(Core.Constants.IncoTerms.FreeOnBoard, allIncoTerms);
			AssertCollectionContains(Core.Constants.IncoTerms.ExWorks, allIncoTerms);
			AssertCollectionContains(IncoTermAndCustomsChargeFactory.ErrorIncoTermCode, allIncoTerms);
		}

		public override void TestGetAllCharges()
		{
			var allCharges = incoTermAndChargeFactory.GetAllCharges();
			AssertEquals("There should be 12 charges", 12, allCharges.Length);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\CN\Core\Business.Test\InvoiceCharges\TestFile\ImportIncoTermAndCustomsChargeConfiguration.csv";

		protected override string GetCountryContext() => Core.Constants.CountryCodes.China + Common.Shared.SharedJobMessageTypeList.Codes.Import;
	}
}
