using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class ChargeableTableStrategyTest : TableStrategyTest
	{
		#region Frequency

		// ChargeableTableStrategy is for Loose (LCL) so not applicable for these NonLoose(FCL) tests, hence they return empty

		protected override string TestFrequency_20GPExpectedResult => "";

		protected override string TestFrequency_20GPAnd40GPExpectedResult => "";

		protected override string TestFrequency_ContractNumber_Container_ExpectedResult => "";

		#endregion

		// Only used in 'Forwarding Landscape Pricing Page - Loose' that doesn't have container
		protected override string TestCompanyTariffAndClientRate_DestinationCharges_ExpectedResult => "";

		#region Company Tariff

		// ChargeableTableStrategy is for Loose (LCL) so not applicable for these NonLoose(FCL) tests, hence they return empty

		protected override string TestCompanyTariff_QuoteWithFrequency_ExpectedResult => "";

		protected override string TestCompanyTariff_QuoteWithBlankFrequency_ExpectedResult => "";

		protected override string TestCompanyTariff_SameContractNumberExpectedResult => "";

		protected override string TestCompanyTariff_DifferentContractNumberExpectedResult => @"";

		override protected string TestCompanyTariff_EmptyContractNumberExpectedResult => @"";

		#endregion

		protected override string ExpectedForSimilarRates =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '10.00']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '20.00']
";

		protected override string ExpectedForSimilarRates_DifferentContractNumber =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: contract1]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '10.00']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: contract2]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '20.00']
";

		protected override string ExpectedForSimilarRates_DifferentContainer =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '10.00']
  [SubRow 1, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '20.00']
";

		protected override string ExpectedForSimilarRates_DifferentContainerWithBlankContractNumber =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONT1]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '10.00']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '20.00']
  [SubRow 1, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '30.00']
";

		protected override string ExpectedForSimilarRates_DifferentContainerWithBlankContractNumber_OriginCharges => ""; // ContainerisedStrategy doesn't support Origin charges
		protected override string ExpectedForSimilarRates_DifferentContainerWithBlankContractNumber_DestinationCharges => ""; // ContainerisedStrategy doesn't support Destination charges

		public void TestDecimalComma()
		{
			var costing = Helper.NewCosting(NonLocalClient);
			var costingRateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, "SEA", "AUBNE", "NLAMS", "FRT", 10m, currency: "AUD");
			AddUnitCharge(costingRateEntry, "BAF", RatingConstants.Units.KG, "AUD", 11m);

			AssertPricingPage
			(
				Helper.ChargeCodes["FRT"],
				costingRateEntry,
				language: Core.SharedConstants.Languages.German,
				expected: @"[Row 0]
  [Entry [Empty Service Level]-GEN-Sea AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '10,00']
  [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 2]
    [Column 'Per KG' = '11,00']",
				"Should have point in its CurrentCulture for FormatNumber to parse it correclty."
			);
		}

		public void TestRounding()
		{
			var chargeCode = Helper.ChargeCodes["FRT"];
			chargeCode.AC_LocalLanguageDescription = "International Freight Local Description 地方";

			var costing = Helper.NewCosting(NonLocalClient);
			var costingRateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, "SEA", "AUBNE", "NLAMS", "FRT", 10.018m, currency: "AUD");
			AddUnitCharge(costingRateEntry, "BAF", RatingConstants.Units.KG, "AUD", 11.018m);

			AssertPricingPage
			(
				Helper.ChargeCodes["FRT"],
				costingRateEntry,
				language: Core.SharedConstants.Languages.German,
				expected: @"[Row 0]
  [Entry [Empty Service Level]-GEN-Sea AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '10,02']
  [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 2]
    [Column 'Per KG' = '11,02']",
				"GIVEN Registry EnableLocalDescription=TRUE BUT printing ClientRate on NonLocalClient in German Language THEN should print Description (Not Multilingual nor Local Description)"
			);
		}

		#region Description - Rate with Related Company Tariff

		public void TestDescription_CostingWithRelatedCompanyTariff_FlatCalculator()
		{
			var chargeCode = Helper.ChargeCodes["FRT"];
			chargeCode.AC_LocalLanguageDescription = "International Freight Local Description 地方";

			AddFlatCharge(TariffEntry, "FRT", "International Freight", "AUD", 10m);

			var costing = Helper.NewCosting(NonLocalClient);
			var costingRateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, "SEA", "AUBNE", "NLAMS", "FRT", 20m, currency: "AUD");

			AssertPricingPage
			(
				Helper.ChargeCodes["FRT"],
				costingRateEntry,
				language: Core.SharedConstants.Languages.German,
				expected: @"[Row 0]
  [Entry [Empty Service Level]-GEN-Sea AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '20,00']
[Row 1]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '10,00']",
				"GIVEN Registry EnableLocalDescription=TRUE BUT printing ClientRate on NonLocalClient in German Language THEN should print Description (Not Multilingual nor Local Description)"
			);
		}

		public void TestDescription_ClientRateWithRelatedCompanyTariff_FlatCalculator()
		{
			var chargeCode = Helper.ChargeCodes["FRT"];
			chargeCode.AC_LocalLanguageDescription = "International Freight Local Description 地方";

			AddFlatCharge(TariffEntry, "FRT", "International Freight", "AUD", 10m);

			var clientRate = Helper.NewClientRate(NonLocalClient);
			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, "SEA", "AUBNE", "NLAMS", "FRT", 20m, currency: "AUD");

			AssertPricingPage
			(
				Helper.ChargeCodes["FRT"],
				clientRateEntry,
				language: Core.SharedConstants.Languages.German,
				expected: @"[Row 0]
  [Entry [Empty Service Level]-GEN-Sea AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '20,00']
[Row 1]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '10,00']",
				"GIVEN Registry EnableLocalDescription=TRUE BUT printing ClientRate on NonLocalClient in German Language THEN should print Description (Not Multilingual nor Local Description)"
			);
		}

		public void TestDescription_ClientRateWithRelatedCompanyTariff_UnitCalculator()
		{
			var chargeCode = Helper.ChargeCodes["FRT"];
			chargeCode.AC_LocalLanguageDescription = "International Freight Local Description 地方";

			AddUnitCharge(TariffEntry, "FRT", RatingConstants.Units.KG, "AUD", 10);

			var clientRate = Helper.NewClientRate(NonLocalClient);
			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, "SEA", "AUBNE", "NLAMS", "FRT", 20m, currency: "AUD");

			AssertPricingPage
			(
				Helper.ChargeCodes["FRT"],
				clientRateEntry,
				language: Core.SharedConstants.Languages.German,
				expected: @"[Row 0]
  [Entry [Empty Service Level]-GEN-Sea AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '20,00']
[Row 1]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Per KG' = '10,00']",
				"GIVEN Registry EnableLocalDescription=TRUE BUT printing ClientRate on NonLocalClient in German Language THEN should print Description (Not Multilingual nor Local Description)"
			);
		}

		public void TestDescription_ClientRateWithRelatedCompanyTariff_FlatPlusPerUnitCalculator()
		{
			var chargeCode = Helper.ChargeCodes["FRT"];
			chargeCode.AC_LocalLanguageDescription = "International Freight Local Description 地方";

			AddFlatPlusPerUnitCharge(TariffEntry, "FRT", RatingConstants.Units.KG, "AUD", 10, 15);

			var clientRate = Helper.NewClientRate(NonLocalClient);
			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, "SEA", "AUBNE", "NLAMS", "FRT", 20m, currency: "AUD");

			AssertPricingPage
			(
				Helper.ChargeCodes["FRT"],
				clientRateEntry,
				language: Core.SharedConstants.Languages.German,
				expected: @"[Row 0]
  [Entry [Empty Service Level]-GEN-Sea AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '20,00']
[Row 1]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '10,00']
    [Column 'Per KG' = '15,00']",
				"GIVEN Registry EnableLocalDescription=TRUE BUT printing ClientRate on NonLocalClient in German Language THEN should print Description (Not Multilingual nor Local Description)"
			);
		}

		public void TestDescription_ClientRateWithRelatedCompanyTariff_FirstPlusAdditionalCalculator()
		{
			var chargeCode = Helper.ChargeCodes["FRT"];
			chargeCode.AC_LocalLanguageDescription = "International Freight Local Description 地方";

			AddFirstPlusAdditionalCharge(TariffEntry, "FRT", RatingConstants.Units.KG, "AUD", 10, 15);

			var clientRate = Helper.NewClientRate(NonLocalClient);
			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, "SEA", "AUBNE", "NLAMS", "FRT", 20m, currency: "AUD");

			AssertPricingPage
			(
				Helper.ChargeCodes["FRT"],
				clientRateEntry,
				language: Core.SharedConstants.Languages.German,
				expected: @"[Row 0]
  [Entry [Empty Service Level]-GEN-Sea AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '20,00']
[Row 1]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'First' = '10,00']
    [Column 'Additional' = '15,00']",
				"GIVEN Registry EnableLocalDescription=TRUE BUT printing ClientRate on NonLocalClient in German Language THEN should print Description (Not Multilingual nor Local Description)"
			);
		}

		public void TestDescription_ClientRateWithRelatedCompanyTariff_MinimumOrPerUnitCalculator()
		{
			var chargeCode = Helper.ChargeCodes["FRT"];
			chargeCode.AC_LocalLanguageDescription = "International Freight Local Description 地方";

			AddMinOrUnitCharge(TariffEntry, "FRT", RatingConstants.Units.KG, "AUD", 10, 15);

			var clientRate = Helper.NewClientRate(NonLocalClient);
			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, "SEA", "AUBNE", "NLAMS", "FRT", 20m, currency: "AUD");

			AssertPricingPage
			(
				Helper.ChargeCodes["FRT"],
				clientRateEntry,
				language: Core.SharedConstants.Languages.German,
				expected: @"[Row 0]
  [Entry [Empty Service Level]-GEN-Sea AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '20,00']
[Row 1]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Min' = '10,00']
    [Column 'Per KG' = '15,00']",
				"GIVEN Registry EnableLocalDescription=TRUE BUT printing ClientRate on NonLocalClient in German Language THEN should print Description (Not Multilingual nor Local Description)"
			);
		}

		public void TestDescription_ClientRateWithRelatedCompanyTariff_CombinedCalculator()
		{
			var chargeCode = Helper.ChargeCodes["FRT"];
			chargeCode.AC_LocalLanguageDescription = "International Freight Local Description 地方";

			AddCombinedCharge(TariffEntry, "FRT", RatingConstants.Units.KG, "AUD", 10);

			var clientRate = Helper.NewClientRate(NonLocalClient);
			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, "SEA", "AUBNE", "NLAMS", "FRT", 20m, currency: "AUD");

			AssertPricingPage
			(
				Helper.ChargeCodes["FRT"],
				clientRateEntry,
				language: Core.SharedConstants.Languages.German,
				expected: @"[Row 0]
  [Entry [Empty Service Level]-GEN-Sea AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '20,00']
[Row 1]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Per KG' = '10,00']",
				"GIVEN Registry EnableLocalDescription=TRUE BUT printing ClientRate on NonLocalClient in German Language THEN should print Description (Not Multilingual nor Local Description)"
			);
		}

		void AssertPricingPage(AccChargeCode chargeCode, RateEntry clientRateEntry, string language, string expected, string assertionMessage)
		{
			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Res.TemporarilySwitchLanguage(language))
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(language)))
			using (var mockRes = Res.UseMockData())
			{
				Helper.SetChargeCodeMultilingualDescription(chargeCode, mockRes, "Mein Testgebührencode");
				var page = TableStrategyTestHelper.NewPricingPage(new[] { clientRateEntry, TariffEntry });
				AssertExtractedResultsInMultilineASCII(expected, page, message: assertionMessage);
			}
		}

		OrgHeader NonLocalClient => nonLocalClient ?? (nonLocalClient = Helper.NewOrgHeader(companyTariffDefault: 1));
		OrgHeader nonLocalClient;

		#endregion

		public void TestExtract_Multilingual()
		{
			var rateLine = AddFlatCharge(TariffEntry, "FRT", "International Freight", "AUD", 100);

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				var chargeCode = rateLine.ChargeCode;
				var resKey = chargeCode.AC_DescInfo.CustomizableDataResourceStrings.GetMultilingualString(chargeCode, chargeCode.AC_Desc).ResourceKey;
				mockRes.Put(resKey, new ResourceStringData(resKey, "Mein Testgebührencode"));

				var page = TableStrategyTestHelper.NewPricingPage(TariffEntry);
				var pricingPageWrapperCollection = Strategy.Extract(page);
				AssertEquals("Multilingual description", "Mein Testgebührencode", pricingPageWrapperCollection[0].SubRows[0].Charge.Description);
			}
		}

		public void TestExtract_RateWithOverriddenDescriptionOnCharges()
		{
			AddFlatCharge(TariffEntry, "FRT", "McLaren", "AUD", 100);           // FRT - International Freight
			AddUnitCharge(TariffEntry, "BAF", RatingConstants.Units.KG, "AUD", 90);     // BAF - Bunker Adjustment Factor

			var page = TableStrategyTestHelper.NewPricingPage(TariffEntry);
			var pricingPageWrapperCollection = Strategy.Extract(page);

			AssertEquals("Rows count", 2, pricingPageWrapperCollection[0].SubRows.Count);
			AssertEquals("Overriden description", "McLaren", pricingPageWrapperCollection[0].SubRows[0].Charge.Description);
			AssertEquals("Default description", "Bunker Adjustment Factor", pricingPageWrapperCollection[0].SubRows[1].Charge.Description);
		}

		public void TestFallBack()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();

			OrgHeader client = Factory.New<OrgHeader>();
			client.OH_Code = "CLIENT";
			client.CompanyData.RateTariffLevels.SetLevel("DEF", tariff.TH_GlobalRateLevel);

			ClientRate rate = Factory.New<ClientRate>();
			rate.TH_OH = client.PK;

			Quote quote = Factory.New<Quote>();
			quote.TH_OH = client.PK;

			var entry = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUBNE", "NLAMS", "STD", "");
			entry.RateLines.RemoveAndDeleteAll();
			AddUnitCharge(entry, "FRT", QuantityUnit.KG, "AUD", 100);
			AddUnitCharge(entry, "BAF", QuantityUnit.KG, "AUD", 200);
			AddUnitCharge(entry, "CAF", QuantityUnit.KG, "AUD", 300);

			entry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUBNE", "NLAMS", "STD", "");
			entry.RateLines.RemoveAndDeleteAll();
			AddUnitCharge(entry, "BAF", QuantityUnit.KG, "AUD", 111);
			AddUnitCharge(entry, "CAF", QuantityUnit.KG, "AUD", 222);

			entry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUBNE", "NLAMS", "STD", "");
			entry.RateLines.RemoveAndDeleteAll();
			AddUnitCharge(entry, "CAF", QuantityUnit.KG, "AUD", 122);

			Factory.Save();

			const string expectedFRT = @"'FRT - International Freight']
    [Caption Group 1]
    [Column 'Per KG' = '100.00']";

			const string expectedBAF = @"'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Per KG' = '111.00']";

			const string expectedCAF = @"'CAF - Currency Adjustment Factor']
    [Caption Group 1]
    [Column 'Per KG' = '122.00']";

			var page = TableStrategyTestHelper.NewPricingPage(new RateEntry[] { entry });
			var results = TableStrategyTestHelper.Render(Strategy.Extract(page));

			AssertContains("Tariff entry is included ", expectedFRT, results);
			AssertContains("Client Rate entry is included and preferred to tariff entry", expectedBAF, results);
			AssertContains("Quote Entry is preferred to client rate and tariff entries", expectedCAF, results);
		}

		public void TestCaptionGrouping()
		{
			const string expected1 = @"
[Row 0]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'CAF - Currency Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '80.00']
    [Column 'Per KG' = '81.00']
  [SubRow 1, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '100.00']
    [Column 'Per KG' = '']
  [SubRow 2, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '']
    [Column 'Per KG' = '90.00']
  [SubRow 3, 'AUD - Australian Dollar', 'WAR - War Risk Surcharge']
    [Caption Group 2]
    [Column 'First' = '70.00']
    [Column 'Additional' = '71.00']
  [SubRow 4, 'AUD - Australian Dollar', 'PSS - Peak Season Surcharge']
    [Caption Group 3]
    [Column 'Min' = '60.00']
    [Column 'Per KG' = '61.00']
";

			const string expected2 = @"
[Row 0]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'EFAF - Emergency Fuel Adjustment Factor']
    [Caption Group 1]
    [Column 'Min' = '30.00']
    [Column '-10 per KG' = '15.00']
    [Column '+10 per KG' = '9.00']
    [Column '+20 per KG' = '8.00']
  [SubRow 1, 'AUD - Australian Dollar', 'PSS - Peak Season Surcharge']
    [Caption Group 1]
    [Column 'Min' = '60.00']
    [Column '-10 per KG' = '61.00']
    [Column '+10 per KG' = '61.00']
    [Column '+20 per KG' = '61.00']
  [SubRow 2, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Min' = '']
    [Column '-10 per KG' = '90.00']
    [Column '+10 per KG' = '90.00']
    [Column '+20 per KG' = '90.00']
  [SubRow 3, 'AUD - Australian Dollar', 'CAF - Currency Adjustment Factor']
    [Caption Group 2]
    [Column 'Flat' = '80.00']
    [Column 'Per KG' = '81.00']
  [SubRow 4, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 2]
    [Column 'Flat' = '100.00']
    [Column 'Per KG' = '']
  [SubRow 5, 'AUD - Australian Dollar', 'WAR - War Risk Surcharge']
    [Caption Group 3]
    [Column 'First' = '70.00']
    [Column 'Additional' = '71.00']
";

			AddFlatCharge(TariffEntry, "FRT", "AUD", 100);
			AddUnitCharge(TariffEntry, "BAF", RatingConstants.Units.KG, "AUD", 90);
			AddFlatPlusPerUnitCharge(TariffEntry, "CAF", RatingConstants.Units.KG, "AUD", 80, 81);
			AddFirstPlusAdditionalCharge(TariffEntry, "WAR", RatingConstants.Units.KG, "AUD", 70, 71);
			AddMinOrUnitCharge(TariffEntry, "PSS", RatingConstants.Units.KG, "AUD", 60, 61);

			PricingPage page = TableStrategyTestHelper.NewPricingPage(TariffEntry);
			AssertExtractedResults(expected1, page, message: "without combined");

			RateLine line = AddCombinedCharge(TariffEntry, "EFAF", RatingConstants.Units.KG, "AUD", 15, 10, 9, 20, 8);
			((CombinedCalculator)line.Calculator).Minimum = 30;
			AssertExtractedResults(expected2, page, message: "without combined");
		}

		public void TestCurrency()
		{
			AddFlatCharge(TariffEntry, "FRT", "JPY", 100);
			AddFlatCharge(TariffEntry, "FRT", "AUD", 100);
			AddFlatCharge(TariffEntry, "FRT", "KWD", 100);

			const string expected = @"
[Row 0]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'JPY - Japanese Yen', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '100']
  [SubRow 1, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '100.00']
  [SubRow 2, 'KWD - Kuwaiti Dinar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '100.000']
";

			PricingPage page = TableStrategyTestHelper.NewPricingPage(TariffEntry);
			AssertExtractedResults(expected, page);
		}

		public void TestCompanyTariff()
		{
			AddFlatPlusPerUnitCharge(TariffEntry, "FRT", RatingConstants.Units.KG, "AUD", 100, 10);
			AddCompanyTariffBased(RateEntry, "FRT", RatingConstants.Units.KG, "AUD", 0, 0, 10, 0);

			Factory.Save();

			const string expected = @"
[Row 0]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '110.00']
    [Column 'Per KG' = '11.00']
";

			PricingPage page = TableStrategyTestHelper.NewPricingPage(RateEntry);
			AssertExtractedResults(expected, page);
		}

		public void TestUnit()
		{
			AddUnitCharge(TariffEntry, "FRT", "KG", "AUD", 100);

			const string expected = @"
[Row 0]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Per KG' = '100.00']
";

			PricingPage page = TableStrategyTestHelper.NewPricingPage(TariffEntry);
			AssertExtractedResults(expected, page);
		}

		public void TestFlat()
		{
			AddFlatCharge(TariffEntry, "FRT", "AUD", 100);

			const string expected = @"
[Row 0]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '100.00']
";

			PricingPage page = TableStrategyTestHelper.NewPricingPage(TariffEntry);
			AssertExtractedResults(expected, page);
		}

		public void TestFlatPlusPerUnit()
		{
			AddFlatPlusPerUnitCharge(TariffEntry, "FRT", RatingConstants.Units.KG, "AUD", 100, 10);

			const string expected = @"
[Row 0]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '100.00']
    [Column 'Per KG' = '10.00']
";

			PricingPage page = TableStrategyTestHelper.NewPricingPage(TariffEntry);
			AssertExtractedResults(expected, page);
		}

		public void TestFirstPlusAdditional()
		{
			AddFirstPlusAdditionalCharge(TariffEntry, "FRT", RatingConstants.Units.KG, "AUD", 100, 10);

			const string expected = @"
[Row 0]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'First' = '100.00']
    [Column 'Additional' = '10.00']
";

			PricingPage page = TableStrategyTestHelper.NewPricingPage(TariffEntry);
			AssertExtractedResults(expected, page);
		}

		public void TestMinimumOrPerUnit()
		{
			AddMinOrUnitCharge(TariffEntry, "FRT", RatingConstants.Units.KG, "AUD", 100, 10);

			const string expected = @"
[Row 0]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Min' = '100.00']
    [Column 'Per KG' = '10.00']
";

			PricingPage page = TableStrategyTestHelper.NewPricingPage(TariffEntry);
			AssertExtractedResults(expected, page);
		}

		public void TestCombined()
		{
			AddCombinedCharge(TariffEntry, "FRT", RatingConstants.Units.KG, "AUD", 9, 10, 8, 20, 7, 30, 6);

			const string expected = @"
[Row 0]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column '-10 per KG' = '9.00']
    [Column '+10 per KG' = '8.00']
    [Column '+20 per KG' = '7.00']
    [Column '+30 per KG' = '6.00']
";

			PricingPage page = TableStrategyTestHelper.NewPricingPage(TariffEntry);
			AssertExtractedResults(expected, page);
		}

		public void TestAgent()
		{
			var line = TariffEntry.AddRateLine("FRT", CombinedCalculator.Code, "KG", "AUD");
			var minimumItem = line.RateLineItems.AddNew();
			minimumItem.TM_Type = Calculator.Items.Operator.MIN;
			minimumItem.TM_Value = 5m;
			minimumItem.TM_AgentDeclaredRate = 111.11m;
			var minusItem = line.RateLineItems.AddNew();
			minusItem.TM_Type = Calculator.Items.Operator.Minus;
			minusItem.TM_Break = 10m;
			minusItem.TM_Value = 1m;
			minusItem.TM_AgentDeclaredRate = 9.99m;
			var plus10Item = line.RateLineItems.AddNew();
			plus10Item.TM_Type = Calculator.Items.Operator.Plus;
			plus10Item.TM_Break = 10m;
			plus10Item.TM_Value = 2m;
			plus10Item.TM_AgentDeclaredRate = 8.88m;
			var plus50Item = line.RateLineItems.AddNew();
			plus50Item.TM_Type = Calculator.Items.Operator.Plus;
			plus50Item.TM_Break = 50m;
			plus50Item.TM_Value = 3m;
			plus50Item.TM_AgentDeclaredRate = 7.77m;

			AddFlatCharge(TariffEntry, "BAF", "AUD", 100);

			var expected = @"
[Row 0]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Min' = '111.11']
    [Column '-10 per KG' = '9.99']
    [Column '+10 per KG' = '8.88']
    [Column '+50 per KG' = '7.77']
";

			var page = new PricingPage(TariffEntry, Factory, PricingPageStyle.Landscape, true);
			var message = "Should display the agent rates rather than stand rates from TM_Value and not display the BAF line";

			AssertExtractedResults(expected, page, message: message);

			expected = @"
[Row 0]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Min' = '5.00']
    [Column '-10 per KG' = '1.00']
    [Column '+10 per KG' = '2.00']
    [Column '+50 per KG' = '3.00']
  [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 2]
    [Column 'Flat' = '100.00']
";

			page = new PricingPage(TariffEntry, Factory, PricingPageStyle.Landscape, false);
			message = "Should display the agent rates rather than stand rates from TM_Value. The BAF line has values so should be included";

			AssertExtractedResults(expected, page, message: message);
		}

		public void TestEntriesWithNoVisibleLinesAreNotExtracted()
		{
			var chargeCode1 = Helper.ChargeCodes["FRT"];
			chargeCode1.AC_ShowOnQuotation = true;
			chargeCode1.AC_SuppressOnQuoteIfZero = true;

			var chargeCode2 = Helper.ChargeCodes["CAF"];
			chargeCode2.AC_ShowOnQuotation = false;

			TariffEntry.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			TariffEntry.AddRateLine(chargeCode1, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 0m;
			TariffEntry.AddRateLine(chargeCode2, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 200m;

			var page = TableStrategyTestHelper.NewPricingPage(TariffEntry);
			AssertExtractedResults(string.Empty, page, Strategy, "Neither Rate Line is visible, so this rate entry should not be extrated");

			AddCombinedCharge(TariffEntry, chargeCode1.AC_Code, QuantityUnit.KG, Core.Constants.CurrencyCodes.Australia, 9, 10, 8, 50, 7);
			const string expected = @"[Row 0]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column '-10 per KG' = '9.00']
    [Column '+10 per KG' = '8.00']
    [Column '+50 per KG' = '7.00']";

			page = TableStrategyTestHelper.NewPricingPage(TariffEntry);
			AssertExtractedResults(expected, page, Strategy);
		}

		public void TestEntryWithNoVisibleLinesAreExtractedIfRelatedEntryIsFound()
		{
			var chargeCode = Helper.ChargeCodes["FRT"];
			var tariff = Factory.New<CompanyTariff>();
			var tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.LCL, "LCL", "AUMEL", "NLAMS", "STD", "");
			tariffEntry.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			tariffEntry.AddRateLine(chargeCode, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 100m;
			Factory.Save();

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, "LCL", "AUMEL", "NLAMS", "STD", "");
			rateEntry.RateLines.RemoveAndDeleteAll();
			Factory.Save();

			var page = TableStrategyTestHelper.NewPricingPage(rateEntry);
			const string expected = @"[Row 0]
  [Entry STD-GEN-LCL AUMEL => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Caption Group 1]
    [Column 'Flat' = '100.00']";

			AssertExtractedResults(expected, page, Strategy, "Client Rate has no Rate Entry, but it's linked to a Tariff that does have one.");
		}

		#region Implementation

		protected override BaseTableStrategy Strategy => strategy ?? (strategy = new ChargeableTableStrategy(Factory));
		ChargeableTableStrategy strategy;

		protected override string PageSetIndex => "ForwardingLandscapeComplexLoose";

		RateEntry TariffEntry
		{
			get
			{
				if (tariffEntry == null)
				{
					var tariff = Factory.New<CompanyTariff>();
					tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.LCL, "LCL", "AUBNE", "NLAMS", "STD", "");
					tariffEntry.RateLines.RemoveAndDeleteAll();
				}

				return tariffEntry;
			}
		}
		RateEntry tariffEntry;

		RateEntry RateEntry
		{
			get
			{
				if (rateEntry == null)
				{
					var clientRate = Factory.NewWithValidTestData<ClientRate>();
					rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, "LCL", "AUBNE", "NLAMS", "STD", "");
					rateEntry.RateLines.RemoveAndDeleteAll();
				}

				return rateEntry;
			}
		}
		RateEntry rateEntry;

		TestHelper Helper => helper ?? (helper = new TestHelper(Factory));
		TestHelper helper;

		#endregion
	}
}
