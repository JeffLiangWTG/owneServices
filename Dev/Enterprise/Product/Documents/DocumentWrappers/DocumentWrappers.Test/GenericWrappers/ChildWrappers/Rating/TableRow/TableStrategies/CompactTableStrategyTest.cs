using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RatingEnums;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;
using Mode = Enterprise.Core.Constants.RateMode;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class CompactTableStrategyTest : TableStrategyTest
	{
		public void TestExtract_GivenRateLineCurrency_ThenEachColumnShouldBeFormattedUsingRateLineCurrency()
		{
			var quotation = Factory.New<Quote>();
			var rateEntry1 = quotation.AddRateEntry(Category.FCL, Mode.SEA, "AUSYD", "USLAX", container: "20GP", removeLines: true);
			rateEntry1.AddFlatRateLine("BAF", 1234.5678, currency: "AUD");
			rateEntry1.AddFlatRateLine("CAF", 2345.6789, currency: "LYD");

			var rateEntry2 = quotation.AddRateEntry(Category.FCL, Mode.SEA, "AUSYD", "USLAX", container: "40GP", removeLines: true);
			rateEntry2.AddFlatRateLine("BAF", 3456.7891, currency: "LYD");
			rateEntry2.AddFlatRateLine("CAF", 4567.8912, currency: "AUD");

			Factory.Save();

			var pricingPages = new PricingPageCollection(quotation);
			pricingPages.Load(PricingPaginationStrategy.LandscapeCompactStyle);
			var pricingPage = pricingPages.Cast<PricingPage>().Single();
			AssertNotNull("Pre-Condition: only one page is expected for these supplementary rates", pricingPage);
			var strategy = NewFreightStrategy();
			var extractedResults = strategy.Extract(pricingPage).Cast<PricingPageTableRowWrapper>();

			var actualSubRowLines = new List<string>();
			foreach (PricingPageTableSubRowWrapper subRow in extractedResults.SelectMany(x => x.SubRows))
			{
				foreach (PricingPageColumnWrapper column in subRow.Columns)
				{
					actualSubRowLines.Add($"{column.Heading.Replace('\n', ' ')} {column.Value}, Currency Format: {column.Currency}");
				}
			}

			AssertContainsExactElementsInAnyOrder
			(
				new[]
				{
					"BAF (AUD) 1234.57, Currency Format: AUD",
					"CAF (LYD) 2345.679, Currency Format: LYD",
					"BAF (LYD) , Currency Format: ",
					"CAF (AUD) , Currency Format: ",
					"BAF (AUD) , Currency Format: AUD",
					"CAF (LYD) , Currency Format: LYD",
					"BAF (LYD) 3456.789, Currency Format: LYD",
					"CAF (AUD) 4567.89, Currency Format: AUD",
				},
				actualSubRowLines
			);
		}

		#region Frequency

		protected override string TestFrequency_20GPExpectedResult => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '200.00']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every Day]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '201.00']

[Row 2]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every 2 Days]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '202.00']
";

		protected override string TestFrequency_20GPAnd40GPExpectedResult => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '200.00']
  [SubRow 1, '[Empty Currency]', '[Empty Charge]']
    [Split 40GP]
    [Column 'FRT(AUD)' = '400.00']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every Day]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '201.00']
  [SubRow 1, '[Empty Currency]', '[Empty Charge]']
    [Split 40GP]
    [Column 'FRT(AUD)' = '401.00']

[Row 2]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every 2 Days]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '202.00']

[Row 3]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every 3 Days]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 40GP]
    [Column 'FRT(AUD)' = '403.00']
";

		protected override string TestFrequency_ContractNumber_Container_ExpectedResult => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT1]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '2001.00']
  [SubRow 1, '[Empty Currency]', '[Empty Charge]']
    [Split 40GP]
    [Column 'FRT(AUD)' = '4001.00']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every Day, Contract Number: CONTRACT1]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '2011.00']
  [SubRow 1, '[Empty Currency]', '[Empty Charge]']
    [Split 40GP]
    [Column 'FRT(AUD)' = '4011.00']

[Row 2]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every 2 Days, Contract Number: CONTRACT1]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '2021.00']
  [SubRow 1, '[Empty Currency]', '[Empty Charge]']
    [Split 40GP]
    [Column 'FRT(AUD)' = '4021.00']

[Row 3]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT2]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '2002.00']
  [SubRow 1, '[Empty Currency]', '[Empty Charge]']
    [Split 40GP]
    [Column 'FRT(AUD)' = '4002.00']

[Row 4]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every Day, Contract Number: CONTRACT2]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '2012.00']
  [SubRow 1, '[Empty Currency]', '[Empty Charge]']
    [Split 40GP]
    [Column 'FRT(AUD)' = '4012.00']

[Row 5]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every 3 Days, Contract Number: CONTRACT2]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '2032.00']
  [SubRow 1, '[Empty Currency]', '[Empty Charge]']
    [Split 40GP]
    [Column 'FRT(AUD)' = '4032.00']
";

		#endregion

		// ClientRate should be prioritized over CompanyTariff but this is a known behaviour for Compact because 
		// GetRateEntriesToGroup for Non-Freight charge  would get 2 rows i.e.ClientRate and CompanyTariff and not prioritizing.
		// Prioritizing happens on CreateTableRow > GetRelatedLineSets because it can only prioritized once it know the Charges in RateLines 
		protected override string TestCompanyTariffAndClientRate_DestinationCharges_ExpectedResult => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'DDOC(AUD)' = '10.00']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'DDOC(AUD)' = '100.00']
";

		#region Company Tariff

		protected override string TestCompanyTariff_QuoteWithFrequency_ExpectedResult => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '10.00']
    [Column 'WAR(AUD)' = '20.00']
    [Column 'BAF(AUD)' = '']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every Day]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '']
    [Column 'WAR(AUD)' = '21.00']
    [Column 'BAF(AUD)' = '11.00']

[Row 2]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every 3 Days]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '']
    [Column 'WAR(AUD)' = '23.00']
    [Column 'BAF(AUD)' = '']
";

		protected override string TestCompanyTariff_QuoteWithBlankFrequency_ExpectedResult => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '10.00']
    [Column 'WAR(AUD)' = '20.00']
";

		protected override string TestCompanyTariff_SameContractNumberExpectedResult => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT1]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'BAF(USD)' = '12.00']
    [Column 'FRT(USD)' = '21.00']
    [Column 'CAF(USD)' = '22.00']
";

		protected override string TestCompanyTariff_DifferentContractNumberExpectedResult => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT-QUOTATION]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(USD)' = '21.00']
    [Column 'CAF(USD)' = '22.00']
";

		override protected string TestCompanyTariff_EmptyContractNumberExpectedResult => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT-QUOTATION]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(USD)' = '21.00']
    [Column 'CAF(USD)' = '22.00']
";

		#endregion

		public void TestDecimalComma()
		{
			var helper = new Rating.Business.Testing.TestHelper(Factory);
			var client = helper.NewOrgHeader(1);
			var quote = helper.NewQuote(client);
			var quoteRateEntry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Mode.LSE, "AUSYD", "USLAX", "BAF", 10.0018m, currency: "LYD");
			AddUnitCharge(quoteRateEntry, "CAF", "KG", "LYD", 11.0018);

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(Core.SharedConstants.Languages.German)))
			{
				var pricingPageCollection = new PricingPageCollection(quote);
				pricingPageCollection.Load(PricingPaginationStrategy.LandscapeCompactStyle);
				AssertExtractedResultsInMultilineASCII
				(
					@"
[Row 0]
  [Entry [Empty Service Level]-GEN-Luft AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column 'BAF(LYD)' = '10,002']
    [Column 'CAF(LYD/KG)' = '11,002']",
					pricingPageCollection.Cast<PricingPage>().Single(),
					NewFreightStrategy(),
					"Should have point in its CurrentCulture for FormatNumber to parse it correclty."
				);
			}
		}

		public void TestRounding()
		{
			var helper = new Rating.Business.Testing.TestHelper(Factory);
			var client = helper.NewOrgHeader(1);
			var quote = helper.NewQuote(client);
			var quoteRateEntry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Mode.LSE, "AUSYD", "USLAX", "BAF", 10.0018m, currency: "LYD");
			AddUnitCharge(quoteRateEntry, "CAF", "KG", "LYD", 11.0018);

			var pricingPageCollection = new PricingPageCollection(quote);
			pricingPageCollection.Load(PricingPaginationStrategy.LandscapeCompactStyle);

			var pricingPage = pricingPageCollection.Cast<PricingPage>().Single();
			AssertNotNull("Pre-Condition: only one page is expected for these supplementary rates", pricingPage);

			var strategy = NewFreightStrategy();
			AssertExtractedResults
			(
				@"
[Row 0]
  [Entry [Empty Service Level]-GEN-Air AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column 'BAF(LYD)' = '10.002']
    [Column 'CAF(LYD/KG)' = '11.002']",
				pricingPage,
				strategy
			);
		}

		public void TestDontAddRowsIfNoChargesToShow()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();

			RateEntry frtEntry = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "SEA", "AU", "NL", "", "20GP");
			frtEntry.TI_RH_NKCommodityCode = ZString.Empty;
			frtEntry.RateLines.RemoveAndDeleteAll();
			AddUnitCharge(frtEntry, "FRT", "CN", "AUD", 500);

			RateEntry ori1Entry = tariff.AddRateEntry(RatingConstants.RateCategory.SOR, "ALL", "AUBNE", "", "", "");
			ori1Entry.TI_RH_NKCommodityCode = ZString.Empty;
			ori1Entry.RateLines.RemoveAndDeleteAll();
			AddFlatCharge(ori1Entry, "ODOC", "AUD", 50);

			RateEntry ori2Entry = tariff.AddRateEntry(RatingConstants.RateCategory.SOR, "ALL", "AUSYD", "", "", "");
			ori2Entry.TI_RH_NKCommodityCode = ZString.Empty;
			ori2Entry.RateLines.RemoveAndDeleteAll();
			AddFlatCharge(ori2Entry, "ODOC", "AUD", 0);

			PricingPage page = new PricingPage(frtEntry, Factory, PricingPageStyle.Standard);

			const string expected1 = @"
[Row 0]
  [Entry [Empty Service Level]-[Empty Commodity]-ALL AUBNE => ]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column 'ODOC(AUD)' = '50.00']

[Row 1]
  [Entry [Empty Service Level]-[Empty Commodity]-ALL AUSYD => ]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column 'ODOC(AUD)' = '0.00']
";

			const string expected2 = @"
[Row 0]
  [Entry [Empty Service Level]-[Empty Commodity]-ALL AUBNE => ]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column 'ODOC(AUD)' = '50.00']

[Row 1]
  [Entry [Empty Service Level]-[Empty Commodity]-ALL AUSYD => ]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column 'ODOC(AUD)' = '']
";

			const string expected3 = @"
";

			CombineAssertions(delegate
			{
				AccChargeCode charge = GetChargeCode("ODOC");
				CompactTableStrategy strategy = NewOriginStrategy();

				charge.AC_ShowOnQuotation = true;
				charge.AC_SuppressOnQuoteIfZero = false;
				AssertExtractedResults(expected1, page, strategy, "Origin Table (Show)");

				charge.AC_SuppressOnQuoteIfZero = true;
				AssertExtractedResults(expected2, page, strategy, "Origin Table (Suppress If Zero)");

				charge.AC_ShowOnQuotation = false;
				AssertExtractedResults(expected3, page, strategy, "Origin Table (Hide)");
			});
		}

		public void TestExtractFreightRates()
		{
			PricingPage page = SetupForExtractByPort();

			const string expected = @"
[Row 0]
  [Entry [Empty Service Level]-[Empty Commodity]-LCL AU => NL]
  [Entry [Empty Service Level]-[Empty Commodity]-FCL AU => NL]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split Non-Containerized]
    [Column 'FRT(USD/CN)' = '503.00']
    [Column 'BAF(USD/CN)' = '103.00']
  [SubRow 1, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(USD/CN)' = '501.00']
    [Column 'BAF(USD/CN)' = '101.00']

[Row 1]
  [Entry [Empty Service Level]-[Empty Commodity]-LCL NL => AU]
  [Entry [Empty Service Level]-[Empty Commodity]-FCL NL => AU]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split Non-Containerized]
    [Column 'FRT(USD/CN)' = '504.00']
    [Column 'BAF(USD/CN)' = '104.00']
  [SubRow 1, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(USD/CN)' = '502.00']
    [Column 'BAF(USD/CN)' = '102.00']
";

			AssertExtractedResults(expected, page);
		}

		public void TestExtractOriginRatesByPort()
		{
			PricingPage page = SetupForExtractByPort();
			CompactTableStrategy strategy = NewOriginStrategy();

			const string expected = @"
[Row 0]
  [Entry [Empty Service Level]-[Empty Commodity]-ALL AU => ]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column 'ODOC(AUD)' = '50.00']
    [Column 'OPCH(AUD)' = '200.00']

[Row 1]
  [Entry [Empty Service Level]-[Empty Commodity]-ALL AUBNE => ]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column 'ODOC(AUD)' = '50.00']
    [Column 'OPCH(AUD)' = '180.00']

[Row 2]
  [Entry [Empty Service Level]-[Empty Commodity]-ALL AUSYD => ]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column 'ODOC(AUD)' = '50.00']
    [Column 'OPCH(AUD)' = '220.00']
";

			AssertExtractedResults(expected, page, strategy);
		}

		public void TestExtractDestinationRatesByPort()
		{
			PricingPage page = SetupForExtractByPort();
			CompactTableStrategy strategy = NewDestinationStrategy();

			const string expected = @"
[Row 0]
  [Entry [Empty Service Level]-[Empty Commodity]-ALL  => AU]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column 'DDOC(AUD)' = '50.00']
    [Column 'DPCH(AUD)' = '200.00']

[Row 1]
  [Entry [Empty Service Level]-[Empty Commodity]-ALL  => AUBNE]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column 'DDOC(AUD)' = '50.00']
    [Column 'DPCH(AUD)' = '180.00']

[Row 2]
  [Entry [Empty Service Level]-[Empty Commodity]-ALL  => AUSYD]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column 'DDOC(AUD)' = '50.00']
    [Column 'DPCH(AUD)' = '220.00']
";

			AssertExtractedResults(expected, page, strategy);
		}

		public void TestExtractRatesByContainer()
		{
			PricingPage page = SetupForExtractByContainer();
			CompactTableStrategy strategy = NewOriginStrategy();

			const string expected = @"
[Row 0]
  [Entry [Empty Service Level]-[Empty Commodity]-ALL AU => ]
  [Entry [Empty Service Level]-[Empty Commodity]-FCL AU => ]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split Containerized]
    [Column 'ODOC(AUD)' = '51.00']
  [SubRow 1, '[Empty Currency]', '[Empty Charge]']
    [Split Non-Containerized]
    [Column 'ODOC(AUD)' = '50.00']
  [SubRow 2, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'ODOC(AUD)' = '52.00']
";

			AssertExtractedResults(expected, page, strategy);
		}

		public void TestRateOverrides()
		{
			PricingPage page = SetupForRateOverride();
			CompactTableStrategy strategy = NewOriginStrategy();

			const string expected = @"
[Row 0]
  [Entry [Empty Service Level]-[Empty Commodity]-ALL AU => ]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column 'ODOC(AUD)' = '50.00']
    [Column 'OPCH(AUD)' = '60.00']
    [Column 'ODOC(USD)' = '']

[Row 1]
  [Entry [Empty Service Level]-[Empty Commodity]-ALL AUBNE => ]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column 'ODOC(AUD)' = '']
    [Column 'OPCH(AUD)' = '60.00']
    [Column 'ODOC(USD)' = '']

[Row 2]
  [Entry [Empty Service Level]-[Empty Commodity]-ALL AUSYD => ]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column 'ODOC(AUD)' = '']
    [Column 'OPCH(AUD)' = '60.00']
    [Column 'ODOC(USD)' = '50.00']
";

			AssertExtractedResults(expected, page, strategy);
		}

		public void TestOnlySupplimentaryRatesWithLinesToPrintArePrinted()
		{
			var helper = new Rating.Business.Testing.TestHelper(Factory);
			var chargeCode = helper.ChargeCodes.New("ORGQTE", "Always Visible on Quote", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var nonZeroChargeCode = helper.ChargeCodes.New("ORGNOZERO", "Visible if not Zero on Quote", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			nonZeroChargeCode.AC_SuppressOnQuoteIfZero = true;

			var nonQuoteChargeCode = helper.ChargeCodes.New("ORGNONQTE", "Never Visible on Quote", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			nonQuoteChargeCode.AC_ShowOnQuotation = false;

			Assert("Pre-Condition", chargeCode.AC_ShowOnQuotation);
			Assert("Pre-Condition", !chargeCode.AC_SuppressOnQuoteIfZero);

			Assert("Pre-Condition", nonZeroChargeCode.AC_ShowOnQuotation);
			Assert("Pre-Condition", nonZeroChargeCode.AC_SuppressOnQuoteIfZero);

			Assert("Pre-Condition", !nonQuoteChargeCode.AC_ShowOnQuotation);

			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var tariffEntryAE = companyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, Mode.LCL, "AE", "");
			AddFlatCharge(tariffEntryAE, chargeCode.AC_Code, CurrencyCodes.Australia, 10m);

			var tariffEntryBE = companyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, Mode.LCL, "AU", "");
			AddFlatCharge(tariffEntryBE, nonQuoteChargeCode.AC_Code, CurrencyCodes.Australia, 20m);

			var tariffEntryCA = companyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, Mode.LCL, "CA", "");
			AddFlatCharge(tariffEntryCA, chargeCode.AC_Code, CurrencyCodes.Australia, 30m);
			Factory.Save();

			var client = helper.NewOrgHeader(1);
			var quote = helper.NewQuote(client);
			var quoteEntryAE = quote.AddRateEntry(RatingConstants.RateCategory.ORG, Mode.LCL, "AE", "");
			var quoteEntryBE = quote.AddRateEntry(RatingConstants.RateCategory.ORG, Mode.LCL, "BE", "");
			var quoteEntryCA = quote.AddRateEntry(RatingConstants.RateCategory.ORG, Mode.LCL, "CA", "");
			AddFlatCharge(quoteEntryCA, nonZeroChargeCode.AC_Code, CurrencyCodes.Australia, 0m);
			AddFlatCharge(quoteEntryCA, chargeCode.AC_Code, CurrencyCodes.Australia, 40m);

			var quoteEntryDE = quote.AddRateEntry(RatingConstants.RateCategory.ORG, Mode.LCL, "DE", "");
			AddFlatCharge(quoteEntryDE, nonZeroChargeCode.AC_Code, CurrencyCodes.Australia, 0m);
			AddFlatCharge(quoteEntryDE, nonQuoteChargeCode.AC_Code, CurrencyCodes.Australia, 50m);

			var quoteEntryEE = quote.AddRateEntry(RatingConstants.RateCategory.ORG, Mode.LCL, "EE", "");
			AddFlatCharge(quoteEntryEE, chargeCode.AC_Code, CurrencyCodes.Australia, 60m);
			AddFlatCharge(quoteEntryEE, nonZeroChargeCode.AC_Code, CurrencyCodes.Australia, 70m);
			Factory.Save();

			var pricingPages = new PricingPageCollection(quote);
			pricingPages.Load(PricingPaginationStrategy.LandscapeCompactStyle);

			var pricingPage = pricingPages.Cast<PricingPage>().Single();
			AssertNotNull("Pre-Condition: only one page is expected for these supplementary rates", pricingPage);

			var strategy = NewOriginStrategy();
			var extractedResults = strategy.Extract(pricingPage).Cast<PricingPageTableRowWrapper>();

			var message = @"The following rates should or shouldn't appear on the Compact Pricing Page:
tariffEntryAE = INCLUDED because it has lines that can be printed and is linked to the quote via quoteEntryAE
tariffEntryBE = EXCLUDED because it has no lines that can be printed even though it is linked to the quote via quoteEntryBE
tariffEntryCA = INCLUDED because it has a line that can be included even though it will probably be overriden by the quote (this looks odd but let's not change this too much)
quoteEntryAE = EXCLUDED because it has no lines
quoteEntryBE = EXCLUDED because it has no lines
quoteEntryCA = INCLUDED because it has lines to display, and should override the quote
quoteEntryDE = EXCLUDED because it has no lines that can be displayed on a quote
quoteEntryEE = INCLUDED";

			var expectedEntries = new[]
			{
				tariffEntryAE.PK,
				tariffEntryCA.PK,
				quoteEntryCA.PK,
				quoteEntryDE.PK,
				quoteEntryEE.PK
			};
			var actualEntries = new List<ZGuid>();
			foreach (RatingEntryWrapper wrapper in extractedResults.SelectMany(x => x.Entries))
			{
				var actualRateEntry = (RateEntry)wrapper.WrappedObject;
				actualEntries.Add(actualRateEntry.PK);
			}

			AssertContainsExactElementsInAnyOrder(message, expectedEntries, actualEntries);

			var actualSubRowLines = new List<string>();
			foreach (PricingPageTableSubRowWrapper subRow in extractedResults.SelectMany(x => x.SubRows))
			{
				foreach (PricingPageColumnWrapper column in subRow.Columns)
				{
					actualSubRowLines.Add($"{column.Heading.Replace('\n', ' ')} {column.Value}");
				}
			}

			var expectedLines = @"ORGQTE (AUD) 10.00
ORGNOZERO (AUD) 
ORGQTE (AUD) 30.00
ORGNOZERO (AUD) 
ORGQTE (AUD) 40.00
ORGNOZERO (AUD) 
ORGQTE (AUD) 
ORGNOZERO (AUD) 
ORGQTE (AUD) 60.00
ORGNOZERO (AUD) 70.00".Split(new[] { System.Environment.NewLine }, StringSplitOptions.None);

			AssertContainsExactElementsInAnyOrder(message, expectedLines, actualSubRowLines);
		}

		protected override string ExpectedForSimilarRates => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '10.00']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '20.00']
";

		protected override string ExpectedForSimilarRates_DifferentContractNumber => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: contract1]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '10.00']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: contract2]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '20.00']
";

		protected override string ExpectedForSimilarRates_DifferentContainerWithBlankContractNumber => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONT1]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '10.00']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '20.00']
  [SubRow 1, '[Empty Currency]', '[Empty Charge]']
    [Split 40GP]
    [Column 'FRT(AUD)' = '30.00']
";

		protected override BaseTableStrategy OriginStrategy => NewOriginStrategy();

		protected override string ExpectedForSimilarRates_DifferentContainerWithBlankContractNumber_OriginCharges => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONT1]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'ODOC(AUD)' = '10.00']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'ODOC(AUD)' = '20.00']
  [SubRow 1, '[Empty Currency]', '[Empty Charge]']
    [Split 40GP]
    [Column 'ODOC(AUD)' = '30.00']
";

		protected override BaseTableStrategy DestinationStrategy => NewDestinationStrategy();

		protected override string ExpectedForSimilarRates_DifferentContainerWithBlankContractNumber_DestinationCharges => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONT1]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'DDOC(AUD)' = '10.00']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'DDOC(AUD)' = '20.00']
  [SubRow 1, '[Empty Currency]', '[Empty Charge]']
    [Split 40GP]
    [Column 'DDOC(AUD)' = '30.00']
";

		protected override string ExpectedForSimilarRates_DifferentContainer => @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '10.00']
  [SubRow 1, '[Empty Currency]', '[Empty Charge]']
    [Split 40GP]
    [Column 'FRT(AUD)' = '20.00']
";

		public void TestComplexRates()
		{
			PricingPage page = SetupForComplexRates();
			CompactTableStrategy strategy = NewOriginStrategy();

			const string expected = @"
[Row 0]
  [Entry [Empty Service Level]-[Empty Commodity]-FCL AU => ]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split Containerized]
    [Column 'ODOC(AUD/CN)' = 'See Below']
    [Column 'OPCH(AUD)' = '']
  [SubRow 1, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'ODOC(AUD/CN)' = 'See Below']
    [Column 'OPCH(AUD)' = '71.00']
  [SubRow 2, '[Empty Currency]', '[Empty Charge]']
    [Split 40GP]
    [Column 'ODOC(AUD/CN)' = 'See Below']
    [Column 'OPCH(AUD)' = '72.00']
  [Line 1,1, 'Origin Documentation Fee *', '', '', '']
  [Line 1,2, 'Minimum', 'AUD', '100.00', '']
  [Line 1,3, 'Per Unit', 'AUD', '25.00', 'per Container']
";

			AssertExtractedResults(expected, page, strategy);
		}

		public void TestPricingPageWithContainersAndContainerClasses()
		{
			const string expected1 = @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AU => CN]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '1200.00']
  [SubRow 1, '[Empty Currency]', '[Empty Charge]']
    [Split 40GP]
    [Column 'FRT(AUD)' = '2400.00']";

			const string expected2 = @"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AU => CN]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20GP]
    [Column 'FRT(AUD)' = '1200.00']
  [SubRow 1, '[Empty Currency]', '[Empty Charge]']
    [Split 40GP]
    [Column 'FRT(AUD)' = '2400.00']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AU => CN]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Split 20RE]
    [Column 'FRT(AUD)' = '1500.00']
";

			var companyTariff = Factory.New<CompanyTariff>();
			AssertLandscapePricingPageWithContainersAndContainerClasses(companyTariff, expected1, expected2);

			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			AssertLandscapePricingPageWithContainersAndContainerClasses(clientRate, expected1, expected2);

			var quotation = Factory.NewWithValidTestData<Quote>();
			AssertLandscapePricingPageWithContainersAndContainerClasses(quotation, expected1, expected2);
		}

		#region Implementation

		protected override BaseTableStrategy Strategy => NewFreightStrategy();

		protected override string PageSetIndex => "ForwardingCompact";

		CompactTableStrategy NewFreightStrategy() => new CompactTableStrategy(Factory, new PricingPageRateLineFactory(EntryTypes.Freight));

		CompactTableStrategy NewOriginStrategy() => new CompactTableStrategy(Factory, new PricingPageRateLineFactory(EntryTypes.Origin));

		CompactTableStrategy NewDestinationStrategy() => new CompactTableStrategy(Factory, new PricingPageRateLineFactory(EntryTypes.Destination));

		PricingPage SetupForExtractByPort()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();

			RateEntry fclEntry1 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL", "", "20GP");
			fclEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			AddUnitCharge(fclEntry1, "FRT", "CN", "USD", 501);
			AddUnitCharge(fclEntry1, "BAF", "CN", "USD", 101);

			RateEntry fclEntry2 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "NL", "AU", "", "20GP");
			fclEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			AddUnitCharge(fclEntry2, "FRT", "CN", "USD", 502);
			AddUnitCharge(fclEntry2, "BAF", "CN", "USD", 102);

			RateEntry lclEntry1 = tariff.AddRateEntry(RatingConstants.RateCategory.LCL, "LCL", "AU", "NL", "", "");
			lclEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			AddUnitCharge(lclEntry1, "FRT", "CN", "USD", 503);
			AddUnitCharge(lclEntry1, "BAF", "CN", "USD", 103);

			RateEntry lclEntry2 = tariff.AddRateEntry(RatingConstants.RateCategory.LCL, "LCL", "NL", "AU", "", "");
			lclEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			AddUnitCharge(lclEntry2, "FRT", "CN", "USD", 504);
			AddUnitCharge(lclEntry2, "BAF", "CN", "USD", 104);

			RateEntry orgEntry1 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AU", "", "", "");
			orgEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			AddFlatCharge(orgEntry1, "ODOC", "AUD", 50);
			AddFlatCharge(orgEntry1, "OPCH", "AUD", 200);

			RateEntry orgEntry2 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUBNE", "", "", "");
			orgEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			AddFlatCharge(orgEntry2, "OPCH", "AUD", 180);

			RateEntry orgEntry3 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUSYD", "", "", "");
			orgEntry3.TI_RH_NKCommodityCode = ZString.Empty;
			AddFlatCharge(orgEntry3, "OPCH", "AUD", 220);

			RateEntry dstEntry1 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "ALL", "", "AU", "", "");
			dstEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			AddFlatCharge(dstEntry1, "DDOC", "AUD", 50);
			AddFlatCharge(dstEntry1, "DPCH", "AUD", 200);

			RateEntry dstEntry2 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "ALL", "", "AUBNE", "", "");
			dstEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			AddFlatCharge(dstEntry2, "DPCH", "AUD", 180);

			RateEntry dstEntry3 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "ALL", "", "AUSYD", "", "");
			dstEntry3.TI_RH_NKCommodityCode = ZString.Empty;
			AddFlatCharge(dstEntry3, "DPCH", "AUD", 220);

			return TableStrategyTestHelper.NewPricingPage(fclEntry1, fclEntry2, lclEntry1, lclEntry2);
		}

		PricingPage SetupForExtractByContainer()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();

			RateEntry fclEntry = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL", "", "20GP");
			fclEntry.TI_RH_NKCommodityCode = ZString.Empty;

			RateEntry orgEntry1 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AU", "", "", "");
			orgEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			AddFlatCharge(orgEntry1, "ODOC", "AUD", 50);

			RateEntry orgEntry2 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AU", "", "", "");
			orgEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			AddFlatCharge(orgEntry2, "ODOC", "AUD", 51);

			RateEntry orgEntry3 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AU", "", "", "20GP");
			orgEntry3.TI_RH_NKCommodityCode = ZString.Empty;
			AddFlatCharge(orgEntry3, "ODOC", "AUD", 52);

			return TableStrategyTestHelper.NewPricingPage(fclEntry);
		}

		PricingPage SetupForRateOverride()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();

			RateEntry fclEntry = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL", "", "20GP");
			fclEntry.TI_RH_NKCommodityCode = ZString.Empty;

			RateEntry orgEntry1 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AU", "", "", "");
			orgEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			AddFlatCharge(orgEntry1, "ODOC", "AUD", 50);
			AddFlatCharge(orgEntry1, "OPCH", "AUD", 60);

			RateEntry orgEntry2 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUBNE", "", "", "");
			orgEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			AddFlatCharge(orgEntry2, "ODOC", "AUD", 0);

			RateEntry orgEntry3 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUSYD", "", "", "");
			orgEntry3.TI_RH_NKCommodityCode = ZString.Empty;
			AddFlatCharge(orgEntry3, "ODOC", "USD", 50);

			return TableStrategyTestHelper.NewPricingPage(fclEntry);
		}

		PricingPage SetupForComplexRates()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();

			RateEntry fclEntry1 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL", "", "20GP");
			fclEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			RateEntry fclEntry2 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL", "", "40GP");
			fclEntry2.TI_RH_NKCommodityCode = ZString.Empty;

			RateEntry orgEntry1 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AU", "", "", "");
			orgEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			AddMinOrUnitCharge(orgEntry1, "ODOC", "CN", "AUD", 100, 25);

			RateEntry orgEntry2 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AU", "", "", "20GP");
			orgEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			AddFlatCharge(orgEntry2, "OPCH", "AUD", 71);

			RateEntry orgEntry3 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AU", "", "", "40GP");
			orgEntry3.TI_RH_NKCommodityCode = ZString.Empty;
			AddFlatCharge(orgEntry3, "OPCH", "AUD", 72);

			return TableStrategyTestHelper.NewPricingPage(fclEntry1, fclEntry2);
		}

		#endregion
	}
}
