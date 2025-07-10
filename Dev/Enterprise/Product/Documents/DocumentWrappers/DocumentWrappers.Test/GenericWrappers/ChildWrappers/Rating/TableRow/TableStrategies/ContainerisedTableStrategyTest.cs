using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;
using Mode = Enterprise.Core.Constants.RateMode;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class ContainerisedTableStrategyTest : TableStrategyTest
	{
		#region Container

		public void TestExtract_GivenFCLRateWithBlankAndNonBlankContainers()
		{
			var quotation = Factory.NewWithValidTestData<Quote>();

			var rateEntry1 = quotation.AddRateEntry(Category.FCL, Mode.SEA, "AUSYD", "USLAX", container: "", removeLines: true);
			rateEntry1.AddFlatRateLine("FRT", 100);
			rateEntry1.AddFlatRateLine("BAF", 101);

			var rateEntry2 = quotation.AddRateEntry(Category.FCL, Mode.SEA, "AUSYD", "USLAX", container: "20GP", removeLines: true);
			rateEntry2.AddFlatRateLine("FRT", 120);
			rateEntry2.AddFlatRateLine("CAF", 122);

			Factory.Save();

			AssertExtractedResultsInString
			(
				@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'USD - United States Dollar', 'FRT - International Freight']
    [Column '' = '100.00 Flat']
    [Column '20GP' = '120.00 Flat']
  [Line 1,1, 'Bunker Adjustment Factor', 'USD', '101.00', '']
  [Line 2,1, 'Currency Adjustment Factor', '', '', '']
  [Line 2,2, '20GP', 'USD', '122.00', '']
",
				TableStrategyTestHelper.GetPricingPages(Factory, quotation, PageSetIndex),
				message: "GIVEN FCL rate with blank and non blank containers THEN should be printed"
			);
		}

		public void TestExtract_GivenFCLRateWithBlankContainerAndRelatedClientRate()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = client.PK;
			var rateEntry11 = clientRate.AddRateEntry(Category.FCL, Mode.SEA, "AUSYD", "USLAX", container: "", removeLines: true);
			rateEntry11.AddFlatRateLine("FRT", 100);
			rateEntry11.AddFlatRateLine("BAF", 101);
			rateEntry11.AddFlatRateLine("CAF", 102);
			var rateEntry12 = clientRate.AddRateEntry(Category.FCL, Mode.SEA, "AUSYD", "USLAX", container: "40GP", removeLines: true);
			rateEntry12.AddFlatRateLine("FRT", 140);
			rateEntry12.AddFlatRateLine("BAF", 141);
			rateEntry12.AddFlatRateLine("CAF", 142);

			var quotation = Factory.NewWithValidTestData<Quote>();
			quotation.TH_OH = client.PK;
			var rateEntry21 = quotation.AddRateEntry(Category.FCL, Mode.SEA, "AUSYD", "USLAX", container: "", removeLines: true);
			rateEntry21.AddFlatRateLine("FRT", 200);
			rateEntry21.AddFlatRateLine("BAF", 201);

			Factory.Save();

			AssertExtractedResultsInString
			(
				@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'USD - United States Dollar', 'FRT - International Freight']
    [Column '' = '200.00 Flat']
  [Line 1,1, 'Bunker Adjustment Factor', 'USD', '201.00', '']
  [Line 2,1, 'Currency Adjustment Factor', 'USD', '102.00', '']
",
				TableStrategyTestHelper.GetPricingPages(Factory, quotation, PageSetIndex),
				message: "GIVEN FCL rate with blank container and related client rate THEN should be printed"
			);
		}

		#endregion

		#region Frequency

		protected override string TestFrequency_20GPExpectedResult => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '200.00 Flat']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every Day]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '201.00 Flat']

[Row 2]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every 2 Days]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '202.00 Flat']
";

		protected override string TestFrequency_20GPAnd40GPExpectedResult => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '200.00 Flat']
    [Column '40GP' = '400.00 Flat']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every Day]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '201.00 Flat']
    [Column '40GP' = '401.00 Flat']

[Row 2]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every 2 Days]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '202.00 Flat']
    [Column '40GP' = '']

[Row 3]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every 3 Days]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '']
    [Column '40GP' = '403.00 Flat']
";

		protected override string TestFrequency_ContractNumber_Container_ExpectedResult => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT1]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '2,001.00 Flat']
    [Column '40GP' = '4,001.00 Flat']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every Day, Contract Number: CONTRACT1]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '2,011.00 Flat']
    [Column '40GP' = '4,011.00 Flat']

[Row 2]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every 2 Days, Contract Number: CONTRACT1]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '2,021.00 Flat']
    [Column '40GP' = '4,021.00 Flat']

[Row 3]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT2]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '2,002.00 Flat']
    [Column '40GP' = '4,002.00 Flat']

[Row 4]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every Day, Contract Number: CONTRACT2]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '2,012.00 Flat']
    [Column '40GP' = '4,012.00 Flat']

[Row 5]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every 3 Days, Contract Number: CONTRACT2]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '2,032.00 Flat']
    [Column '40GP' = '4,032.00 Flat']
";

		#endregion

		// Only for Freight charges, not Origin and Destination
		protected override string TestCompanyTariffAndClientRate_DestinationCharges_ExpectedResult => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
  [Line 1,1, 'War Risk Surcharge', '', '', '']
  [Line 1,2, '20GP', 'AUD', '1000.00', '']
";

		#region Company Tariff

		protected override string TestCompanyTariff_QuoteWithFrequency_ExpectedResult => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '10.00 Flat']
  [Line 1,1, 'War Risk Surcharge', '', '', '']
  [Line 1,2, '20GP', 'AUD', '20.00', '']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every Day]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP', 'AUD', '11.00', '']
  [Line 2,1, 'War Risk Surcharge', '', '', '']
  [Line 2,2, '20GP', 'AUD', '21.00', '']

[Row 2]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every 3 Days]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
  [Line 1,1, 'War Risk Surcharge', '', '', '']
  [Line 1,2, '20GP', 'AUD', '23.00', '']
";

		protected override string TestCompanyTariff_QuoteWithBlankFrequency_ExpectedResult => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '10.00 Flat']
  [Line 1,1, 'War Risk Surcharge', '', '', '']
  [Line 1,2, '20GP', 'AUD', '20.00', '']
";

		protected override string TestCompanyTariff_SameContractNumberExpectedResult => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT1]
  [SubRow 0, 'USD - United States Dollar', 'FRT - International Freight']
    [Column '20GP' = '21.00 Flat']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP', 'USD', '12.00', '']
  [Line 2,1, 'Currency Adjustment Factor', '', '', '']
  [Line 2,2, '20GP', 'USD', '22.00', '']
";

		protected override string TestCompanyTariff_DifferentContractNumberExpectedResult => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT-QUOTATION]
  [SubRow 0, 'USD - United States Dollar', 'FRT - International Freight']
    [Column '20GP' = '21.00 Flat']
  [Line 1,1, 'Currency Adjustment Factor', '', '', '']
  [Line 1,2, '20GP', 'USD', '22.00', '']
";

		override protected string TestCompanyTariff_EmptyContractNumberExpectedResult => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT-QUOTATION]
  [SubRow 0, 'USD - United States Dollar', 'FRT - International Freight']
    [Column '20GP' = '21.00 Flat']
  [Line 1,1, 'Currency Adjustment Factor', '', '', '']
  [Line 1,2, '20GP', 'USD', '22.00', '']
";

		#endregion

		public void TestDontBlowUpIfNoFreightChargeCodeIsSpecifiedInTheRegistry()
		{
			Env.Registry.FreightChargeCode = Guid.Empty;

			CompanyTariff tariff = Factory.New<CompanyTariff>();

			RateEntry entry = tariff.AddRateEntry(Category.FCL, Mode.SEA, "AU", "NL", "", "20GP");
			entry.TI_RH_NKCommodityCode = ZString.Empty;
			AddUnitCharge(entry, "FRT", RatingConstants.Units.CN, "AUD", 50);

			Factory.Save();

			PricingPage page = TableStrategyTestHelper.NewPricingPage(entry);

			const string expected = @"
[Row 0]
  [Entry [Empty Service Level]-[Empty Commodity]-FCL AU => NL]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
  [Line 1,1, 'International Freight', 'AUD', '50.00', 'per 20GP Container']
";

			AssertExtractedResults(expected, page);
		}

		public void TestExtract_FreightAndNonFreight()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var rateEntry = clientRate.AddRateEntry(Category.LCL, Mode.LCL, "AUSYD", "USLAX", "STD"); // LRA - Rail Freight(LWL)
			rateEntry.RateLines.RemoveAndDeleteAll();
			AddFlatCharge(rateEntry, "FRT", "AUD", 10);
			AddFlatCharge(rateEntry, "BAF", "AUD", 20);

			Factory.Save();

			AssertExtract_MultipleRateEntries
			(
				rateEntries: new List<RateEntry>() { rateEntry },
				expected: @"
[Row 0]
  [Entry STD-GEN-LCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column 'LCL Min.' = '']
    [Column 'LCL per M3' = '10.00 Flat']
  [Line 1,1, 'Bunker Adjustment Factor', 'AUD', '20.00', '']"
			);
		}

		#region Same Rate Entries with same/different Modes of Rail

		public void TestExtract_MultipleRateEntriesWithDifferentRailMode()
		{
			var quote = CreateQuote();

			var rateEntry1 = quote.AddRateEntry(Category.LCL, Mode.LRA, "AUSYD", "USLAX", "STD"); // LRA - Rail Freight(LWL)
			rateEntry1.RateLines.RemoveAndDeleteAll();
			AddFlatCharge(rateEntry1, "FRT", "AUD", 10);

			var rateEntry2 = quote.AddRateEntry(Category.LCL, Mode.FWL, "AUSYD", "USLAX", "STD"); // FWL - Rail Freight(FWL)
			rateEntry2.RateLines.RemoveAndDeleteAll();
			AddFlatCharge(rateEntry2, "FRT", "AUD", 20);

			Factory.Save();

			AssertExtract_MultipleRateEntries
			(
				rateEntries: new List<RateEntry>() { rateEntry1, rateEntry2 },
				expected: @"
[Row 0]
  [Entry STD-GEN-LTA Rail AUSYD => USLAX]
  [Entry STD-GEN-Full Wagon Load AUSYD => USLAX]
  [Line 1,1, 'International Freight', 'AUD', '10.00', '']
  [Line 2,1, 'International Freight', 'AUD', '20.00', '']"
			);
		}

		public void TestExtract_MultipleRateEntriesWithDifferentRailMode_DifferentContractNumber_DifferentRateDesc()
		{
			var quote = CreateQuote();

			var rateEntry1 = quote.AddRateEntry(Category.LCL, Mode.LRA, "AUSYD", "USLAX", "STD"); // LRA - Rail Freight(LWL)
			rateEntry1.TI_ContractNumber = "CON10";
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = AddFlatCharge(rateEntry1, "FRT", "AUD", 10);
			rateLine1.OverrideChargeDescription = true;
			rateLine1.TL_RateDesc = "Rate Line 10";

			var rateEntry2 = quote.AddRateEntry(Category.LCL, Mode.FWL, "AUSYD", "USLAX", "STD"); // FWL - Rail Freight(FWL)
			rateEntry2.TI_ContractNumber = "CON20";
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = AddFlatCharge(rateEntry2, "FRT", "AUD", 20);
			rateLine2.OverrideChargeDescription = true;
			rateLine2.TL_RateDesc = "Rate Line 20";

			Factory.Save();

			AssertExtract_MultipleRateEntries
			(
				rateEntries: new List<RateEntry>() { rateEntry1, rateEntry2 },
				expected: @"
[Row 0]
  [Entry STD-GEN-LTA Rail AUSYD => USLAX, Contract Number: CON10]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column 'LCL Min.' = '']
    [Column 'LCL per M3' = '10.00 Flat']
[Row 1]
  [Entry STD-GEN-Full Wagon Load AUSYD => USLAX, Contract Number: CON20]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column 'LCL Min.' = '']
    [Column 'LCL per M3' = '20.00 Flat']"
			);
		}

		public void TestExtract_MultipleRateEntriesWithDifferentRailMode_DifferentContractNumber_SameRateDesc()
		{
			var quote = CreateQuote();

			var rateEntry1 = quote.AddRateEntry(Category.LCL, Mode.LRA, "AUSYD", "USLAX", "STD"); // LRA - Rail Freight(LWL)
			rateEntry1.TI_ContractNumber = "CON10";
			rateEntry1.RateLines.RemoveAndDeleteAll();
			AddFlatCharge(rateEntry1, "FRT", "AUD", 10);

			var rateEntry2 = quote.AddRateEntry(Category.LCL, Mode.FWL, "AUSYD", "USLAX", "STD"); // FWL - Rail Freight(FWL)
			rateEntry2.TI_ContractNumber = "CON20";
			rateEntry2.RateLines.RemoveAndDeleteAll();
			AddFlatCharge(rateEntry2, "FRT", "AUD", 20);

			Factory.Save();

			AssertExtract_MultipleRateEntries
			(
				rateEntries: new List<RateEntry>() { rateEntry1, rateEntry2 },
				expected: @"
[Row 0]
  [Entry STD-GEN-LTA Rail AUSYD => USLAX, Contract Number: CON10]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column 'LCL Min.' = '']
    [Column 'LCL per M3' = '10.00 Flat']
[Row 1]
  [Entry STD-GEN-Full Wagon Load AUSYD => USLAX, Contract Number: CON20]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column 'LCL Min.' = '']
    [Column 'LCL per M3' = '20.00 Flat']"
			);
		}

		public void TestExtract_MultipleRateEntriesWithDifferentRailMode_DifferentContractNumber_DifferentRateDesc_MINCalculator()
		{
			var quote = CreateQuote();

			var rateEntry1 = quote.AddRateEntry(Category.LCL, Mode.LRA, "AUSYD", "USLAX", "STD"); // LRA - Rail Freight(LWL)
			rateEntry1.TI_ContractNumber = "CON10";
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = AddMinCharge(rateEntry1, "FRT", 10);
			rateLine1.OverrideChargeDescription = true;
			rateLine1.TL_RateDesc = "Rate Line 10";

			var rateEntry2 = quote.AddRateEntry(Category.LCL, Mode.FWL, "AUSYD", "USLAX", "STD"); // FWL - Rail Freight(FWL)
			rateEntry2.TI_ContractNumber = "CON20";
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = AddMinCharge(rateEntry2, "FRT", 20);
			rateLine2.OverrideChargeDescription = true;
			rateLine2.TL_RateDesc = "Rate Line 20";

			Factory.Save();

			AssertExtract_MultipleRateEntries
			(
				rateEntries: new List<RateEntry>() { rateEntry1, rateEntry2 },
				expected: @"
[Row 0]
  [Entry STD-GEN-LTA Rail AUSYD => USLAX, Contract Number: CON10]
  [SubRow 0, 'USD - United States Dollar', 'FRT - International Freight']
    [Column 'LCL Min.' = '']
    [Column 'LCL per M3' = 'See Below']
  [Line 1,1, 'Rate Line 10 - Job Minimum', 'USD', '10.00', '']
[Row 1]
  [Entry STD-GEN-Full Wagon Load AUSYD => USLAX, Contract Number: CON20]
  [SubRow 0, 'USD - United States Dollar', 'FRT - International Freight']
    [Column 'LCL Min.' = '']
    [Column 'LCL per M3' = 'See Below']
  [Line 1,1, 'Rate Line 20 - Job Minimum', 'USD', '20.00', '']"
			);
		}

		static RateLine AddMinCharge(RateEntry entry, ZString chargecode, ZDecimal minimumValue)
		{
			var line = entry.AddRateLine(chargecode, MinimumCalculator.Code);
			line.GetCalculator<MinimumCalculator>().MinimumValue = minimumValue;

			return line;
		}

		public void TestExtract_MultipleRateEntriesWithSameRailMode_DifferentContractNumber_DifferentRateDesc()
		{
			var quote = CreateQuote();

			var rateEntry1 = quote.AddRateEntry(Category.LCL, Mode.LRA, "AUSYD", "USLAX", "STD"); // LRA - Rail Freight(LWL)
			rateEntry1.TI_ContractNumber = "CON10";
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = AddFlatCharge(rateEntry1, "FRT", "AUD", 10);
			rateLine1.OverrideChargeDescription = true;
			rateLine1.TL_RateDesc = "Rate Line 10";

			var rateEntry2 = quote.AddRateEntry(Category.LCL, Mode.LRA, "AUSYD", "USLAX", "STD"); // LRA - Rail Freight(LWL)
			rateEntry2.TI_ContractNumber = "CON20";
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = AddFlatCharge(rateEntry2, "FRT", "AUD", 20);
			rateLine2.OverrideChargeDescription = true;
			rateLine2.TL_RateDesc = "Rate Line 20";

			Factory.Save();

			AssertExtract_MultipleRateEntries
			(
				rateEntries: new List<RateEntry>() { rateEntry1, rateEntry2 },
				expected: @"
[Row 0]
  [Entry STD-GEN-LTA Rail AUSYD => USLAX, Contract Number: CON10]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column 'LCL Min.' = '']
    [Column 'LCL per M3' = '10.00 Flat']
[Row 1]
  [Entry STD-GEN-LTA Rail AUSYD => USLAX, Contract Number: CON20]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column 'LCL Min.' = '']
    [Column 'LCL per M3' = '20.00 Flat']"
			);
		}

		public void TestExtract_MultipleRateEntriesWithSameRailMode_DifferentContractNumber_SameRateDesc()
		{
			var quote = CreateQuote();

			var rateEntry1 = quote.AddRateEntry(Category.LCL, Mode.LRA, "AUSYD", "USLAX", "STD"); // LRA - Rail Freight(LWL)
			rateEntry1.TI_ContractNumber = "CON10";
			rateEntry1.RateLines.RemoveAndDeleteAll();
			AddFlatCharge(rateEntry1, "FRT", "AUD", 10);

			var rateEntry2 = quote.AddRateEntry(Category.LCL, Mode.LRA, "AUSYD", "USLAX", "STD"); // LRA - Rail Freight(LWL)
			rateEntry2.TI_ContractNumber = "CON20";
			rateEntry2.RateLines.RemoveAndDeleteAll();
			AddFlatCharge(rateEntry2, "FRT", "AUD", 20);

			Factory.Save();

			AssertExtract_MultipleRateEntries
			(
				rateEntries: new List<RateEntry>() { rateEntry1, rateEntry2 },
				expected: @"
[Row 0]
  [Entry STD-GEN-LTA Rail AUSYD => USLAX, Contract Number: CON10]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column 'LCL Min.' = '']
    [Column 'LCL per M3' = '10.00 Flat']
[Row 1]
  [Entry STD-GEN-LTA Rail AUSYD => USLAX, Contract Number: CON20]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column 'LCL Min.' = '']
    [Column 'LCL per M3' = '20.00 Flat']"
			);
		}

		void AssertExtract_MultipleRateEntries(List<RateEntry> rateEntries, string expected)
		{
			PricingPage page = null;

			using (var enumerator = rateEntries.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					page = new PricingPage(enumerator.Current, Factory, PricingPageStyle.Landscape);
					while (enumerator.MoveNext())
					{
						page.AddRateEntry(enumerator.Current);
					}
				}
			}

			AssertExtractedResults(expected, page);
		}

		Quote CreateQuote()
		{
			var client = Factory.New<OrgHeader>();
			client.OH_Code = "CLIENT";

			var quote = Factory.New<Quote>();
			quote.TH_OH = client.PK;

			return quote;
		}

		#endregion

		protected override string ExpectedForSimilarRates => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '10.00 Flat']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '20.00 Flat']
";

		protected override string ExpectedForSimilarRates_DifferentContractNumber => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: contract1]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '10.00 Flat']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: contract2]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '20.00 Flat']
";

		protected override string ExpectedForSimilarRates_DifferentContainer => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '10.00 Flat']
    [Column '40GP' = '20.00 Flat']
";

		protected override string ExpectedForSimilarRates_DifferentContainerWithBlankContractNumber => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONT1]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '10.00 Flat']
    [Column '40GP' = '']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '20.00 Flat']
    [Column '40GP' = '30.00 Flat']
";

		protected override string ExpectedForSimilarRates_DifferentContainerWithBlankContractNumber_OriginCharges => ""; // ContainerisedStrategy doesn't support Origin charges
		protected override string ExpectedForSimilarRates_DifferentContainerWithBlankContractNumber_DestinationCharges => ""; // ContainerisedStrategy doesn't support Destination charges

		public void TestExtract()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();

			OrgHeader client = Factory.New<OrgHeader>();
			client.OH_Code = "CLIENT";
			client.CompanyData.RateTariffLevels.SetLevel("DEF", tariff.TH_GlobalRateLevel);

			ClientRate rate = Factory.New<ClientRate>();
			rate.TH_OH = client.PK;

			Quote quote = Factory.New<Quote>();
			quote.TH_OH = client.PK;

			RatingHeader[] headers = { tariff, rate, quote };
			ZString[] containers = { "20GP", "20RE", "20OT", "40GP" };
			List<RateEntry> entries = new List<RateEntry>();

			for (int i = 0; i < headers.Length; i++)
			{
				for (int j = 0; j < containers.Length; j++)
				{
					RateEntry fclentry1 = headers[i].AddRateEntry(Category.FCL, "SEA", "AUBNE", "NLAMS", "STD", containers[j]);
					fclentry1.RateLines.RemoveAndDeleteAll();

					RateEntry fclentry2 = headers[i].AddRateEntry(Category.FCL, "SEA", "NLAMS", "AUBNE", "STD", containers[j]);
					fclentry2.RateLines.RemoveAndDeleteAll();

					if (j >= i)
					{
						AddFlatCharge(fclentry1, "FRT", "AUD", 100 + i * 10 + j);
						AddUnitCharge(fclentry2, "FRT", RatingConstants.Units.CN, "AUD", 100 + i * 10 + j);

						AddFlatCharge(fclentry2, "BAF", "AUD", 200 + i * 10 + j);
					}

					if (i == headers.Length - 1)
					{
						entries.Add(fclentry1);
						entries.Add(fclentry2);
					}
				}

				RateEntry lclentry1 = headers[i].AddRateEntry(Category.LCL, "LCL", "AUBNE", "NLAMS", "STD", "");
				lclentry1.RateLines.RemoveAndDeleteAll();
				AddMinOrUnitCharge(lclentry1, "FRT", RatingConstants.Units.M3, "USD", 100 + i * 10, 50 + i);

				RateEntry lclentry2 = headers[i].AddRateEntry(Category.LCL, "LCL", "NLAMS", "AUBNE", "STD", "");
				lclentry2.RateLines.RemoveAndDeleteAll();
				AddMinOrUnitCharge(lclentry2, "FRT", RatingConstants.Units.M3, "USD", 100 + i * 10, 50 + i);

				if (i == headers.Length - 1)
				{
					entries.Add(lclentry1);
					entries.Add(lclentry2);
				}
			}

			Factory.Save();

			PricingPage page = null;

			using (IEnumerator<RateEntry> enumerator = entries.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					page = new PricingPage(enumerator.Current, Factory, PricingPageStyle.Landscape);

					while (enumerator.MoveNext())
					{
						page.AddRateEntry(enumerator.Current);
					}
				}
			}

			const string expected = @"
[Row 0]
  [Entry STD-GEN-FCL AUBNE => NLAMS]
  [Entry STD-GEN-LCL AUBNE => NLAMS]
  [SubRow 0, 'USD - United States Dollar', 'FRT - International Freight']
    [Column '20GP' = '']
    [Column '20RE' = '']
    [Column '20OT' = '']
    [Column '40GP' = '']
    [Column 'LCL Min.' = '120.00']
    [Column 'LCL per M3' = '52.00']
  [SubRow 1, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '100.00 Flat']
    [Column '20RE' = '111.00 Flat']
    [Column '20OT' = '122.00 Flat']
    [Column '40GP' = '123.00 Flat']
    [Column 'LCL Min.' = '']
    [Column 'LCL per M3' = '']

[Row 1]
  [Entry STD-GEN-FCL NLAMS => AUBNE]
  [Entry STD-GEN-LCL NLAMS => AUBNE]
  [SubRow 0, 'USD - United States Dollar', 'FRT - International Freight']
    [Column '20GP' = '']
    [Column '20RE' = '']
    [Column '20OT' = '']
    [Column '40GP' = '']
    [Column 'LCL Min.' = '120.00']
    [Column 'LCL per M3' = '52.00']
  [SubRow 1, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '100.00']
    [Column '20RE' = '111.00']
    [Column '20OT' = '122.00']
    [Column '40GP' = '123.00']
    [Column 'LCL Min.' = '']
    [Column 'LCL per M3' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP', 'AUD', '200.00', '']
  [Line 1,3, '20OT', 'AUD', '222.00', '']
  [Line 1,4, '20RE', 'AUD', '211.00', '']
  [Line 1,5, '40GP', 'AUD', '223.00', '']
";

			AssertExtractedResults(expected, page);
		}

		public void TestExtractConversionFactor_FCL()
		{
			var client = Factory.New<OrgHeader>();
			client.OH_Code = "CLIENT";
			var quote = Factory.New<Quote>();
			quote.TH_OH = client.PK;

			var entry = quote.AddRateEntry(Category.FCL, Mode.SEA, "AU", "NLAMS", "STD", "20GP");
			entry.RateLines.RemoveAndDeleteAll();
			AddUnitCharge(entry, "FRT", QuantityUnit.CN, Core.Constants.CurrencyCodes.Australia, 220);
			AddFlatCharge(entry, "WAR", QuantityUnit.CN, Core.Constants.CurrencyCodes.Australia, 200);
			Factory.Save();

			AssertEquals("Pre-condition", ConversionFactor.Empty, entry.RateLines[0].ConversionFactor);

			var page = new PricingPage(entry, Factory, PricingPageStyle.Landscape);

			bool hasFoundRateLine = false;
			var wrappers = Strategy.Extract(page);
			foreach (PricingPageTableRowWrapper row in wrappers)
			{
				foreach (PricingPageTableSubRowWrapper subRow in row.SubRows)
				{
					if (subRow.Currency != null && subRow.Currency.ToString().Contains(Core.Constants.CurrencyCodes.Australia))
					{
						hasFoundRateLine = true;

						var message = "Using currency to check that the row is actually matched to the original rate entry";
						AssertEquals(message, ConversionFactor.Standard.Metric.Sea.ToString(), subRow.ConversionFactor.ToString());
					}
				}
			}

			Assert(hasFoundRateLine);
		}

		public void TestExtractConversionFactor_LCL()
		{
			var client = Factory.New<OrgHeader>();
			client.OH_Code = "CLIENT";
			var quote = Factory.New<Quote>();
			quote.TH_OH = client.PK;

			var entry = quote.AddRateEntry(Category.LCL, Mode.LCL, "AU", "");
			entry.RateLines.RemoveAndDeleteAll();
			AddUnitCharge(entry, "FRT", QuantityUnit.CN, Core.Constants.CurrencyCodes.Australia, 220);
			AddFlatCharge(entry, "WAR", QuantityUnit.CN, Core.Constants.CurrencyCodes.Australia, 200);
			Factory.Save();

			AssertEquals("Pre-condition", ConversionFactor.Empty, entry.RateLines[0].ConversionFactor);

			var page = new PricingPage(entry, Factory, PricingPageStyle.Landscape);

			bool hasFoundRateLine = false;
			var wrappers = Strategy.Extract(page);
			foreach (PricingPageTableRowWrapper row in wrappers)
			{
				foreach (PricingPageTableSubRowWrapper subRow in row.SubRows)
				{
					if (subRow.Currency != null && subRow.Currency.ToString().Contains(Core.Constants.CurrencyCodes.Australia))
					{
						hasFoundRateLine = true;

						var message = "Using currency to check that the row is actually matched to the original rate entry";
						AssertEquals(message, ConversionFactor.Standard.Metric.Sea.ToString(), subRow.ConversionFactor.ToString());
					}
				}
			}

			Assert(hasFoundRateLine);
		}

		public void TestHiddenRateLines()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();

			AccChargeCode frt = GetChargeCode("FRT");
			frt.AC_ShowOnQuotation = true;
			frt.AC_SuppressOnQuoteIfZero = false;

			AccChargeCode baf = GetChargeCode("BAF");
			baf.AC_ShowOnQuotation = true;
			baf.AC_SuppressOnQuoteIfZero = false;

			RateEntry entry1 = tariff.AddRateEntry(Category.FCL, "SEA", "AUBNE", "NLAMS", "STD", "20GP");
			entry1.RateLines.RemoveAndDeleteAll();
			AddFlatCharge(entry1, "FRT", "AUD", 10);
			AddFlatCharge(entry1, "BAF", "AUD", 10);

			RateEntry entry2 = tariff.AddRateEntry(Category.FCL, "SEA", "AUBNE", "NLAMS", "STD", "20RE");
			entry2.RateLines.RemoveAndDeleteAll();
			AddUnitCharge(entry2, "FRT", RatingConstants.Units.CN, "AUD", 10);
			AddUnitCharge(entry2, "BAF", RatingConstants.Units.CN, "AUD", 10);

			RateEntry entry3 = tariff.AddRateEntry(Category.FCL, "SEA", "AUBNE", "NLAMS", "STD", "40GP");
			entry3.RateLines.RemoveAndDeleteAll();
			AddMinOrUnitCharge(entry3, "FRT", RatingConstants.Units.CN, "AUD", 10, 10);
			AddMinOrUnitCharge(entry3, "BAF", RatingConstants.Units.CN, "AUD", 10, 10);

			RateEntry entry4 = tariff.AddRateEntry(Category.FCL, "SEA", "AUBNE", "NLAMS", "STD", "40RE");
			entry4.RateLines.RemoveAndDeleteAll();
			AddUnitCharge(entry4, "FRT", RatingConstants.Units.CN, "AUD", 0);
			AddUnitCharge(entry4, "BAF", RatingConstants.Units.CN, "AUD", 0);

			Factory.Save();

			PricingPage page = new PricingPage(entry1, Factory, PricingPageStyle.Landscape);
			page.AddRateEntry(entry2);
			page.AddRateEntry(entry3);
			page.AddRateEntry(entry4);

			const string expectedVisible = @"
[Row 0]
  [Entry STD-GEN-FCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '10.00 Flat']
    [Column '20RE' = '10.00']
    [Column '40GP' = 'See Below']
    [Column '40RE' = '0.00']
  [Line 1,1, 'International Freight', '', '', '']
  [Line 1,2, 'Minimum', 'AUD', '10.00', '']
  [Line 1,3, 'Per Unit', 'AUD', '10.00', 'per 40GP Container']
  [Line 2,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 2,2, '20GP', 'AUD', '10.00', '']
  [Line 2,3, '20RE', 'AUD', '10.00', 'per 20RE Container']
  [Line 2,4, '40GP', '', '', '']
  [Line 2,5, 'Minimum', 'AUD', '10.00', '']
  [Line 2,6, 'Per Unit', 'AUD', '10.00', 'per 40GP Container']
  [Line 2,7, '40RE', '', 'Not Charged', '']
";

			const string expectedConditional = @"
[Row 0]
  [Entry STD-GEN-FCL AUBNE => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '10.00 Flat']
    [Column '20RE' = '10.00']
    [Column '40GP' = 'See Below']
    [Column '40RE' = '']
  [Line 1,1, 'International Freight', '', '', '']
  [Line 1,2, 'Minimum', 'AUD', '10.00', '']
  [Line 1,3, 'Per Unit', 'AUD', '10.00', 'per 40GP Container']
  [Line 2,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 2,2, '20GP', 'AUD', '10.00', '']
  [Line 2,3, '20RE', 'AUD', '10.00', 'per 20RE Container']
  [Line 2,4, '40GP', '', '', '']
  [Line 2,5, 'Minimum', 'AUD', '10.00', '']
  [Line 2,6, 'Per Unit', 'AUD', '10.00', 'per 40GP Container']
";

			AssertExtractedResults(expectedVisible, page, message: "Always Visible");

			frt.AC_SuppressOnQuoteIfZero = true;
			baf.AC_SuppressOnQuoteIfZero = true;
			Factory.Save();

			AssertExtractedResults(expectedConditional, page, message: "Visible If Not Zero");

			frt.AC_ShowOnQuotation = false;
			baf.AC_ShowOnQuotation = false;
			Factory.Save();

			AssertExtractedResults(string.Empty, page, message: "Always Hidden");
		}

		public void TestShowOriginAndDestinationWithoutFreightCharge()
		{
			ClientRate rate = Factory.NewWithValidTestData<ClientRate>();

			RateEntry entry1 = rate.AddRateEntry(Category.FCL, "SEA", "AUMEL", "NZAKL", "STD", "20GP");
			entry1.RateLines.RemoveAndDeleteAll();
			AddUnitCharge(entry1, "BAF", RatingConstants.Units.CN, "AUD", 10);

			RateEntry entry2 = rate.AddRateEntry(Category.FCL, "SEA", "AUMEL", "NZAKL", "STD", "20RE");
			entry2.RateLines.RemoveAndDeleteAll();
			AddUnitCharge(entry2, "BAF", RatingConstants.Units.CN, "AUD", 20);

			RateEntry entry3 = rate.AddRateEntry(Category.FCL, "SEA", "AUMEL", "NZAKL", "STD", "40GP");
			entry3.RateLines.RemoveAndDeleteAll();
			AddUnitCharge(entry3, "BAF", RatingConstants.Units.CN, "AUD", 30);

			Factory.Save();

			PricingPage page = new PricingPage(entry1, Factory, PricingPageStyle.Landscape);
			page.AddRateEntry(entry2);
			page.AddRateEntry(entry3);

			const string expected = @"
[Row 0]
  [Entry STD-GEN-FCL AUMEL => NZAKL]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '20RE' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', 'AUD', '10.00', 'per 20GP Container']
  [Line 1,2, 'Bunker Adjustment Factor', 'AUD', '20.00', 'per 20RE Container']
  [Line 1,3, 'Bunker Adjustment Factor', 'AUD', '30.00', 'per 40GP Container']
";
			AssertExtractedResults(expected, page);
		}

		public void TestPricingPageWithContainersAndContainerClasses()
		{
			const string expected1 = @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AU => CN]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '1,200.00 Flat']
    [Column '40GP' = '2,400.00 Flat']
";

			const string expected2 = @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AU => CN]
  [Entry [Empty Service Level]-GEN-FCL AU => CN]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '1,200.00 Flat']
    [Column '40GP' = '2,400.00 Flat']
    [Column '20RE' = '1,500.00 Flat']
";

			var companyTariff = Factory.New<CompanyTariff>();
			AssertLandscapePricingPageWithContainersAndContainerClasses(companyTariff, expected1, expected2);

			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			AssertLandscapePricingPageWithContainersAndContainerClasses(clientRate, expected1, expected2);

			var quotation = Factory.NewWithValidTestData<Quote>();
			AssertLandscapePricingPageWithContainersAndContainerClasses(quotation, expected1, expected2);
		}

		public void TestEntriesWithNoVisibleLinesAreNotExtracted()
		{
			var helper = new Rating.Business.Testing.TestHelper(Factory);
			var chargeCode1 = helper.ChargeCodes["FRT"];
			chargeCode1.AC_ShowOnQuotation = true;
			chargeCode1.AC_SuppressOnQuoteIfZero = true;

			var chargeCode2 = helper.ChargeCodes["CAF"];
			chargeCode2.AC_ShowOnQuotation = false;

			var tariff = Factory.New<CompanyTariff>();
			var tariffEntry = tariff.AddRateEntry(Category.LCL, "LCL", "AUMEL", "NLAMS", "STD", "");
			tariffEntry.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			tariffEntry.AddRateLine(chargeCode1, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 0m;
			tariffEntry.AddRateLine(chargeCode2, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 200m;
			Factory.Save();

			var page = TableStrategyTestHelper.NewPricingPage(tariffEntry);
			AssertExtractedResults(string.Empty, page, Strategy, "Neither Rate Line is visible, so this rate entry should not be extrated");

			AddCombinedCharge(tariffEntry, chargeCode1.AC_Code, QuantityUnit.KG, Core.Constants.CurrencyCodes.Australia, 9, 10, 8, 50, 7);
			const string expected = @"[Row 0]
  [Entry STD-GEN-LCL AUMEL => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column 'LCL Min.' = '']
    [Column 'LCL per M3' = 'See Below']
  [Line 1,1, 'International Freight', '', '', '']
  [Line 1,2, 'Less than 10 KG', 'AUD', '9.00', 'per KG (1 M3 = 1000 KG)']
  [Line 1,3, '10 KG to less than 50 KG', 'AUD', '8.00', 'per KG (1 M3 = 1000 KG)']
  [Line 1,4, '50 KG and above', 'AUD', '7.00', 'per KG (1 M3 = 1000 KG)']";

			page = TableStrategyTestHelper.NewPricingPage(tariffEntry);
			AssertExtractedResults(expected, page, Strategy);
		}

		public void TestEntryWithNoVisibleLinesAreExtractedIfRelatedEntryIsFound()
		{
			var helper = new Rating.Business.Testing.TestHelper(Factory);
			var chargeCode = helper.ChargeCodes["FRT"];
			var tariff = Factory.New<CompanyTariff>();
			var tariffEntry = tariff.AddRateEntry(Category.LCL, "LCL", "AUMEL", "NLAMS", "STD", "");
			tariffEntry.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			tariffEntry.AddRateLine(chargeCode, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 100m;
			Factory.Save();

			var clientRate = helper.NewClientRate(helper.NewOrgHeader(1));
			var rateEntry = clientRate.AddRateEntry(Category.LCL, "LCL", "AUMEL", "NLAMS", "STD", "");
			rateEntry.RateLines.RemoveAndDeleteAll();
			Factory.Save();

			var page = TableStrategyTestHelper.NewPricingPage(rateEntry);
			const string expected = @"[Row 0]
  [Entry STD-GEN-LCL AUMEL => NLAMS]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column 'LCL Min.' = '']
    [Column 'LCL per M3' = '100.00 Flat']";

			AssertExtractedResults(expected, page, Strategy, "Client Rate has no Rate Entry, but it's linked to a Tariff that does have one.");
		}

		protected override BaseTableStrategy Strategy => new ContainerisedTableStrategy(Factory);

		protected override string PageSetIndex => "ForwardingLandscapeComplexNonLoose";
	}
}
