using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;
using Mode = Enterprise.Core.Constants.RateMode;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	internal abstract class TableStrategyTest : TestCaseWithFactory
	{
		#region Frequency

		public void TestExtract_GivenFrequency_20GP_ThenShouldGroupRatesPerFrequency()
		{
			var quotation = Factory.NewWithValidTestData<Quote>();
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 200, currency: "AUD", container: "20GP");
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 201, currency: "AUD", container: "20GP", frequency: 1, frequencyUnit: FrequencyList.Codes.Days);
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 202, currency: "AUD", container: "20GP", frequency: 2, frequencyUnit: FrequencyList.Codes.Days);

			Factory.Save();

			AssertExtractedResultsInString
			(
				TestFrequency_20GPExpectedResult,
				TableStrategyTestHelper.GetPricingPages(Factory, quotation, PageSetIndex)
			);
		}
		protected abstract string TestFrequency_20GPExpectedResult { get; }

		public void TestExtract_GivenFrequency_20GPAnd40GP_ThenShouldGroupRatesPerFrequencyAndContainer()
		{
			var quotation = Factory.NewWithValidTestData<Quote>();
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 200, currency: "AUD", container: "20GP");
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 201, currency: "AUD", container: "20GP", frequency: 1, frequencyUnit: FrequencyList.Codes.Days);
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 202, currency: "AUD", container: "20GP", frequency: 2, frequencyUnit: FrequencyList.Codes.Days);

			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 400, currency: "AUD", container: "40GP");
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 401, currency: "AUD", container: "40GP", frequency: 1, frequencyUnit: FrequencyList.Codes.Days);
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 403, currency: "AUD", container: "40GP", frequency: 3, frequencyUnit: FrequencyList.Codes.Days);

			Factory.Save();

			AssertExtractedResultsInString
			(
				TestFrequency_20GPAnd40GPExpectedResult,
				TableStrategyTestHelper.GetPricingPages(Factory, quotation, PageSetIndex)
			);
		}
		protected abstract string TestFrequency_20GPAnd40GPExpectedResult { get; }

		public void TestExtract_GivenFrequency_Container_ContractNumber_ThenShouldGroupRatesPerFrequencyAndContainerAndContractNumber()
		{
			var quotation = Factory.NewWithValidTestData<Quote>();
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 2001, currency: "AUD", container: "20GP", contractNumber: "CONTRACT1");
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 2011, currency: "AUD", container: "20GP", contractNumber: "CONTRACT1", frequency: 1, frequencyUnit: FrequencyList.Codes.Days);
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 2021, currency: "AUD", container: "20GP", contractNumber: "CONTRACT1", frequency: 2, frequencyUnit: FrequencyList.Codes.Days);

			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 2002, currency: "AUD", container: "20GP", contractNumber: "CONTRACT2");
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 2012, currency: "AUD", container: "20GP", contractNumber: "CONTRACT2", frequency: 1, frequencyUnit: FrequencyList.Codes.Days);
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 2032, currency: "AUD", container: "20GP", contractNumber: "CONTRACT2", frequency: 3, frequencyUnit: FrequencyList.Codes.Days);

			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 4001, currency: "AUD", container: "40GP", contractNumber: "CONTRACT1");
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 4011, currency: "AUD", container: "40GP", contractNumber: "CONTRACT1", frequency: 1, frequencyUnit: FrequencyList.Codes.Days);
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 4021, currency: "AUD", container: "40GP", contractNumber: "CONTRACT1", frequency: 2, frequencyUnit: FrequencyList.Codes.Days);

			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 4002, currency: "AUD", container: "40GP", contractNumber: "CONTRACT2");
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 4012, currency: "AUD", container: "40GP", contractNumber: "CONTRACT2", frequency: 1, frequencyUnit: FrequencyList.Codes.Days);
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 4032, currency: "AUD", container: "40GP", contractNumber: "CONTRACT2", frequency: 3, frequencyUnit: FrequencyList.Codes.Days);

			Factory.Save();

			AssertExtractedResultsInString
			(
				TestFrequency_ContractNumber_Container_ExpectedResult,
				TableStrategyTestHelper.GetPricingPages(Factory, quotation, PageSetIndex)
			);
		}
		protected abstract string TestFrequency_ContractNumber_Container_ExpectedResult { get; }

		#endregion

		#region Company Tariffs & ClientRates

		public void TestExtract_CompanyTariffAndClientRate_DestinationCharges()
		{
			var helper = new Rating.Business.Testing.TestHelper(Factory);

			var companyTariff = Factory.New<CompanyTariff>();
			companyTariff.TH_GlobalRateLevel = 1;
			companyTariff.AddRateEntryWithFlatRateLine(Category.DST, Mode.FCL, "AUSYD", "USLAX", "DDOC", 10, currency: "AUD", container: "20GP", lineOrder: 1);

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			var clientRate = helper.NewClientRate(client);
			clientRate.AddRateEntryWithFlatRateLine(Category.DST, Mode.FCL, "AUSYD", "USLAX", "DDOC", 100m, currency: "AUD", container: "20GP", lineOrder: 2);

			var quotation = Factory.New<Quote>();
			quotation.QuotationClientAddress.OrganisationPK = client.PK;
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "WAR", 1000, currency: "AUD", container: "20GP", lineOrder: 3);

			Factory.Save();

			AssertExtractedResultsInString
			(
				TestCompanyTariffAndClientRate_DestinationCharges_ExpectedResult,
				TableStrategyTestHelper.GetPricingPages(Factory, quotation, PageSetIndex),
				strategy: DestinationStrategy
			);
		}
		protected abstract string TestCompanyTariffAndClientRate_DestinationCharges_ExpectedResult { get; }

		#endregion

		#region Company Tariffs

		public void TestExtract_GivenCompanyTariff_QuoteWithBlankFrequency()
		{
			var companyTariff = Factory.New<CompanyTariff>();
			companyTariff.TH_GlobalRateLevel = 1;
			companyTariff.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 10, currency: "AUD", container: "20GP", lineOrder: 1);
			companyTariff.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "BAF", 11, currency: "AUD", container: "20GP", frequency: 1, frequencyUnit: FrequencyList.Codes.Days, lineOrder: 2);
			companyTariff.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "CAF", 12, currency: "AUD", container: "20GP", frequency: 2, frequencyUnit: FrequencyList.Codes.Days, lineOrder: 3);

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			var quotation = Factory.New<Quote>();
			quotation.QuotationClientAddress.OrganisationPK = client.PK;
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "WAR", 20, currency: "AUD", container: "20GP", lineOrder: 4);

			Factory.Save();

			AssertExtractedResultsInString
			(
				TestCompanyTariff_QuoteWithBlankFrequency_ExpectedResult,
				TableStrategyTestHelper.GetPricingPages(Factory, quotation, PageSetIndex)
			);
		}
		protected abstract string TestCompanyTariff_QuoteWithBlankFrequency_ExpectedResult { get; }

		public void TestExtract_GivenCompanyTariff_QuoteWithFrequency_ThenShouldReturnCompanyTariffRatesRegardlessFrequency()
		{
			var companyTariff = Factory.New<CompanyTariff>();
			companyTariff.TH_GlobalRateLevel = 1;
			companyTariff.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 10, currency: "AUD", container: "20GP", lineOrder: 1);
			companyTariff.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "BAF", 11, currency: "AUD", container: "20GP", frequency: 1, frequencyUnit: FrequencyList.Codes.Days, lineOrder: 2);
			companyTariff.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "CAF", 12, currency: "AUD", container: "20GP", frequency: 2, frequencyUnit: FrequencyList.Codes.Days, lineOrder: 3);

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			var quotation = Factory.New<Quote>();
			quotation.QuotationClientAddress.OrganisationPK = client.PK;
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "WAR", 20, currency: "AUD", container: "20GP", lineOrder: 4);
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "WAR", 21, currency: "AUD", container: "20GP", frequency: 1, frequencyUnit: FrequencyList.Codes.Days, lineOrder: 5);
			quotation.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "WAR", 23, currency: "AUD", container: "20GP", frequency: 3, frequencyUnit: FrequencyList.Codes.Days, lineOrder: 6);

			Factory.Save();

			AssertExtractedResultsInString
			(
				TestCompanyTariff_QuoteWithFrequency_ExpectedResult,
				TableStrategyTestHelper.GetPricingPages(Factory, quotation, PageSetIndex)
			);
		}
		protected abstract string TestCompanyTariff_QuoteWithFrequency_ExpectedResult { get; }

		public void TestExtract_GivenCompanyTariff_SameContractNumber() => TestCompanyTariffs(companyTariffContractNumber: "CONTRACT1", quotationContractNumber: "CONTRACT1", expectedResults: TestCompanyTariff_SameContractNumberExpectedResult);
		abstract protected string TestCompanyTariff_SameContractNumberExpectedResult { get; }

		public void TestExtract_GivenCompanyTariff_DifferentContractNumber() => TestCompanyTariffs(companyTariffContractNumber: "CONTRACT-COMPANYTARIFF", quotationContractNumber: "CONTRACT-QUOTATION", expectedResults: TestCompanyTariff_DifferentContractNumberExpectedResult);
		abstract protected string TestCompanyTariff_DifferentContractNumberExpectedResult { get; }

		public void TestExtract_GivenCompanyTariff_EmptyContractNumber() => TestCompanyTariffs(companyTariffContractNumber: "", quotationContractNumber: "CONTRACT-QUOTATION", expectedResults: TestCompanyTariff_EmptyContractNumberExpectedResult);
		abstract protected string TestCompanyTariff_EmptyContractNumberExpectedResult { get; }

		void TestCompanyTariffs(string companyTariffContractNumber, string quotationContractNumber, string expectedResults)
		{
			var companyTariff = Factory.New<CompanyTariff>();
			companyTariff.TH_GlobalRateLevel = 1;
			var rateEntry1 = AddRateEntry(companyTariff, Category.FCL, Mode.SEA, "AUSYD", "USLAX", container: "20GP", contractNumber: companyTariffContractNumber, removeLines: true);
			AddFlatRateLine(rateEntry1, "FRT", 11, lineOrder: 11);
			AddFlatRateLine(rateEntry1, "BAF", 12, lineOrder: 12);

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			var quotation = Factory.New<Quote>();
			quotation.QuotationClientAddress.OrganisationPK = client.PK;
			var rateEntry2 = AddRateEntry(quotation, Category.FCL, Mode.SEA, "AUSYD", "USLAX", container: "20GP", contractNumber: quotationContractNumber, removeLines: true);
			AddFlatRateLine(rateEntry2, "FRT", 21, lineOrder: 21);
			AddFlatRateLine(rateEntry2, "CAF", 22, lineOrder: 22);

			Factory.Save();

			AssertExtractedResultsInString
			(
				expectedResults,
				TableStrategyTestHelper.GetPricingPages(Factory, quotation, PageSetIndex)
			);
		}

		RateEntry AddRateEntry(RatingHeader ratingHeader, ZString category, string mode = "", string origin = "", string destination = "", string serviceLevel = "", string container = "", string commodity = "", bool removeLines = false, string contractNumber = default)
		{
			var rateEntry = ratingHeader.AddRateEntry(category, mode, origin, destination, serviceLevel, container, commodity, removeLines);
			if (contractNumber != default)
			{
				rateEntry.TI_ContractNumber = contractNumber;
			}

			return rateEntry;
		}

		RateLine AddFlatRateLine(RateEntry rateEntry, ZString chargeCode, ZDecimal amount, string currency = "", string container = "", string description = "", string unitFactor = "", ZByte? lineOrder = default)
		{
			var rateLine = rateEntry.AddFlatRateLine(chargeCode, amount, currency, container, description, unitFactor);

			if (lineOrder != null)
			{
				rateLine.TL_LineOrder = lineOrder.Value;
			}

			return rateLine;
		}

		protected void AssertExtractedResultsInString(string expected, IEnumerable<PricingPage> pages, BaseTableStrategy strategy = null, string message = "")
		{
			if (strategy == null)
			{
				strategy = Strategy;
			}

			var actual = new StringBuilder();
			foreach (var page in pages)
			{
				var extractedPage = strategy.Extract(page);
				actual.Append(TableStrategyTestHelper.Render(extractedPage));
			}
			AssertContainsExactLinesInExactOrder(message, expected, actual.ToString());
		}

		#endregion

		#region Tests

		public void TestSimilarRates_DifferentTransitTime() => AssertSimilarRates(RateEntrySchema.TI_TransitTime, "1", "2", ExpectedForSimilarRates);

		public void TestSimilarRates_DifferentContractNumber() => AssertSimilarRates(RateEntrySchema.TI_ContractNumber, "contract1", "contract2", ExpectedForSimilarRates_DifferentContractNumber);
		protected abstract string ExpectedForSimilarRates_DifferentContractNumber { get; }

		public void TestSimilarRates_DifferentPaymentTerm() => AssertSimilarRates(RateEntrySchema.TI_PaymentTerm, "CCX", "PPD", ExpectedForSimilarRates);

		void AssertSimilarRates<T>(SchemaColumn schemaColumn, T value1, T value2, string expected)
		{
			var tariff = Factory.New<CompanyTariff>();
			var rateEntry1 = tariff.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 10m, schemaColumn, value1, currency: "AUD", container: "20GP");
			var rateEntry2 = tariff.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 20m, schemaColumn, value2, currency: "AUD", container: "20GP");

			var page = TableStrategyTestHelper.NewPricingPage(rateEntry1, rateEntry2);
			AssertExtractedResultsInMultilineASCII(expected, page);
		}

		protected abstract string ExpectedForSimilarRates { get; }

		public void TestSimilarRates_DifferentContainer()
		{
			var tariff = Factory.New<CompanyTariff>();
			var rateEntry1 = tariff.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 10m, currency: "AUD", container: "20GP");
			var rateEntry2 = tariff.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "FRT", 20m, currency: "AUD", container: "40GP");

			var page = TableStrategyTestHelper.NewPricingPage(rateEntry1, rateEntry2);
			AssertExtractedResultsInMultilineASCII(ExpectedForSimilarRates_DifferentContainer, page);
		}

		protected abstract string ExpectedForSimilarRates_DifferentContainer { get; }

		#region Blank Contract Number

		public void TestSimilarRates_DifferentContainerWithBlankContractNumber() => TestSimilarRates_DifferentContainerWithBlankContractNumber(Category.FCL, Mode.SEA, "FRT", strategy: null, ExpectedForSimilarRates_DifferentContainerWithBlankContractNumber);

		protected abstract string ExpectedForSimilarRates_DifferentContainerWithBlankContractNumber { get; }

		#region Blank Conctract Number - Origin Charges

		public void TestSimilarRates_DifferentContainerWithBlankContractNumber_OriginCharges() => TestSimilarRates_DifferentContainerWithBlankContractNumber(Category.ORG, Mode.FCL, "ODOC", strategy: OriginStrategy, ExpectedForSimilarRates_DifferentContainerWithBlankContractNumber_OriginCharges);

		protected abstract string ExpectedForSimilarRates_DifferentContainerWithBlankContractNumber_OriginCharges { get; }

		protected virtual BaseTableStrategy OriginStrategy => null;

		#endregion

		#region Blank Conctract Number - Destination Charges

		public void TestSimilarRates_DifferentContainerWithBlankContractNumber_DestinationCharges() => TestSimilarRates_DifferentContainerWithBlankContractNumber(Category.DST, Mode.FCL, "DDOC", strategy: DestinationStrategy, ExpectedForSimilarRates_DifferentContainerWithBlankContractNumber_DestinationCharges);

		protected abstract string ExpectedForSimilarRates_DifferentContainerWithBlankContractNumber_DestinationCharges { get; }

		protected virtual BaseTableStrategy DestinationStrategy => null;

		#endregion

		void TestSimilarRates_DifferentContainerWithBlankContractNumber(string category, string mode, string chargeCode, BaseTableStrategy strategy, string expectedOutput)
		{
			var companyTariff = Factory.New<CompanyTariff>();
			var rateEntry1 = companyTariff.AddRateEntryWithFlatRateLine(category, mode, "AUSYD", "USLAX", chargeCode, 10m, currency: "AUD", container: "20GP", contractNumber: "CONT1", lineOrder: 1);
			var rateEntry2 = companyTariff.AddRateEntryWithFlatRateLine(category, mode, "AUSYD", "USLAX", chargeCode, 20m, currency: "AUD", container: "20GP", contractNumber: "", lineOrder: 2);
			var rateEntry3 = companyTariff.AddRateEntryWithFlatRateLine(category, mode, "AUSYD", "USLAX", chargeCode, 30m, currency: "AUD", container: "40GP", contractNumber: "", lineOrder: 3);

			var page = TableStrategyTestHelper.NewPricingPage(rateEntry1, rateEntry2, rateEntry3);
			AssertExtractedResults(expectedOutput, page, strategy);
		}

		#endregion

		#endregion

		#region Implementation

		protected void AssertLandscapePricingPageWithContainersAndContainerClasses(RatingHeader ratingHeader, string expected1, string expected2)
		{
			var entry1 = AddFreightRateEntry(ratingHeader, "20GP", "20GN", 1200);
			var entry2 = AddFreightRateEntry(ratingHeader, "40GP", "40GN", 2400);
			Factory.Save();

			var message = "Expect each container to print as a separate line exactly once";
			AssertExtractedResults(expected1, TableStrategyTestHelper.NewPricingPage(entry1, entry2), message: message);
			AssertExtractedResults(expected1, TableStrategyTestHelper.NewPricingPage(entry2, entry1), message: message);

			entry1.TI_MatchContainerRateClass = true;
			entry2.TI_MatchContainerRateClass = true;

			message = "Changing both rates to check container class should not change the results";
			AssertExtractedResults(expected1, TableStrategyTestHelper.NewPricingPage(entry1, entry2), message: message);
			AssertExtractedResults(expected1, TableStrategyTestHelper.NewPricingPage(entry2, entry1), message: message);

			var entry3 = AddFreightRateEntry(ratingHeader, "20RE", "20GN", 1500);

			message = "Still expect each rate entry to appear exactly once regardless of the order of the pricing page";
			AssertExtractedResults(expected2, TableStrategyTestHelper.NewPricingPage(entry1, entry2, entry3), message: message);
			AssertExtractedResults(expected2, TableStrategyTestHelper.NewPricingPage(entry1, entry3, entry2), message: message);

			AssertExtractedResults(expected2, TableStrategyTestHelper.NewPricingPage(entry2, entry1, entry3), message: message);
			AssertExtractedResults(expected2, TableStrategyTestHelper.NewPricingPage(entry2, entry3, entry1), message: message);

			AssertExtractedResults(expected2, TableStrategyTestHelper.NewPricingPage(entry3, entry1, entry2), message: message);
			AssertExtractedResults(expected2, TableStrategyTestHelper.NewPricingPage(entry3, entry2, entry1), message: message);
		}

		protected RateEntry AddFreightRateEntry(RatingHeader ratingHeader, string containerCode, string containerFreightRateClass, decimal flatAmount)
		{
			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerCode);
			container.RC_FreightRateClass = containerFreightRateClass;

			var rateEntry = ratingHeader.AddRateEntry(Category.FCL, Mode.SEA, "AU", "CN");
			rateEntry.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			rateEntry.TI_RC = container.PK;
			rateEntry.TI_MatchContainerRateClass = false;
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = flatAmount;

			return rateEntry;
		}

		protected void AssertExtractedResults(string expected, PricingPage page, BaseTableStrategy strategy = null, string message = "")
		{
			var delimiters = new[] { '\r', '\n' };
			var expectedLines = expected.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

			if (strategy == null)
			{
				strategy = Strategy;
			}
			var pricingPageTableRowWrappers = strategy.Extract(page);
			var actualLinesTemp = TableStrategyTestHelper.Render(pricingPageTableRowWrappers);
			var actualLines = actualLinesTemp.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

			var comparer = StringComparer.OrdinalIgnoreCase;
			AssertContainsExactElementsInAnyOrder($"{message}\r\n{expected}", comparer, expectedLines, actualLines);
		}

		protected void AssertExtractedResultsInMultilineASCII(string expected, PricingPage page, BaseTableStrategy strategy = null, string message = "")
		{
			if (strategy == null)
			{
				strategy = Strategy;
			}

			var extractedPage = strategy.Extract(page);
			var actual = TableStrategyTestHelper.Render(extractedPage);
			AssertMultilineASCIIEquals(message, expected, actual);
		}

		protected static RateLine AddUnitCharge(RateEntry entry, ZString chargecode, ZString unit, ZString currency, ZDecimal amount)
		{
			var line = entry.AddRateLine(chargecode, UnitCalculator.Code, unit, currency);
			line.GetCalculator<UnitCalculator>().PerUnit = amount;

			return line;
		}

		protected static RateLine AddMinOrUnitCharge(RateEntry entry, ZString chargecode, ZString unit, ZString currency, ZDecimal minAmount, ZDecimal unitAmount)
		{
			var line = entry.AddRateLine(chargecode, MinimumOrPerUnitCalculator.Code, unit, currency);
			var calc = line.GetCalculator<MinimumOrPerUnitCalculator>();
			calc.Minimum = minAmount;
			calc.PerUnit = unitAmount;

			return line;
		}

		protected static RateLine AddFlatCharge(RateEntry entry, ZString chargeCode, ZString currency, ZDecimal amount)
		{
			return AddFlatCharge(entry, chargeCode, ZString.Empty, currency, amount);
		}

		protected static RateLine AddFlatCharge(RateEntry entry, ZString chargeCode, ZString chargeDescription, ZString currency, ZDecimal amount)
		{
			var line = entry.AddRateLine(chargeCode, FlatCalculator.Code, "", currency);
			line.TL_RateDesc = chargeDescription;
			line.GetCalculator<FlatCalculator>().BaseRate = amount;

			return line;
		}

		protected static RateLine AddFlatPlusPerUnitCharge(RateEntry entry, ZString chargecode, ZString unit, ZString currency, ZDecimal baseAmount, ZDecimal perUnitAmount)
		{
			var line = entry.AddRateLine(chargecode, FlatPlusPerUnitCalculator.Code, unit, currency);
			var calc = line.GetCalculator<FlatPlusPerUnitCalculator>();
			calc.BaseRate = baseAmount;
			calc.PerUnit = perUnitAmount;

			return line;
		}

		protected static RateLine AddFirstPlusAdditionalCharge(RateEntry entry, ZString chargeCode, ZString unit, ZString currency, ZDecimal first, ZDecimal additional)
		{
			var line = entry.AddRateLine(chargeCode, FirstPlusAdditionalCalculator.Code, unit, currency);
			var calc = line.GetCalculator<FirstPlusAdditionalCalculator>();
			calc.First = first;
			calc.Additional = additional;

			return line;
		}

		protected static RateLine AddCombinedCharge(RateEntry entry, ZString chargecode, ZString unit, ZString currency, params ZDecimal[] amounts)
		{
			var line = entry.AddRateLine(chargecode, CombinedCalculator.Code, unit, currency);
			var calc = line.GetCalculator<CombinedCalculator>();

			switch (amounts.Length)
			{
				case 0:
					break;

				case 1:
					calc.PerUnit = amounts[0];
					break;

				default:
					int i = 0;

					if ((amounts.Length & 1) == 1)
					{
						calc["-" + amounts[1].ToString(0)] = amounts[0];
						i++;
					}

					for (; i < amounts.Length; i++)
					{
						calc["+" + amounts[i].ToString(0)] = amounts[i + 1];
						i++;
					}
					break;
			}

			return line;
		}

		protected static RateLine AddCompanyTariffBased(RateEntry entry, ZString chargecode, ZString unit, ZString currency, ZDecimal minimum, ZDecimal baseRate, ZDecimal percent, ZDecimal perUnit)
		{
			var line = entry.AddRateLine(chargecode, CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, unit, currency);
			var calc = line.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			calc.Minimum = minimum;
			calc.BaseRate = baseRate;
			calc.Percent = percent;
			calc.PerUnitPercent = percent;
			calc.PerUnit = perUnit;
			calc.PerUnitPercent = percent;

			return line;
		}

		protected AccChargeCode GetChargeCode(string chargeCode)
		{
			var filter = new ZQuery();
			filter.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			return Factory.LoadTop1<AccChargeCode>(filter);
		}

		#endregion

		#region Set up

		protected override void SetUp()
		{
			base.SetUp();

			var gst = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "GST").AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, Core.Constants.CountryCodes.Australia));
			if (gst != null)
			{
				gst.SetRate_ForTestOnly(10, 1);

				Factory.Save();
			}
		}

		protected abstract BaseTableStrategy Strategy { get; }

		protected abstract string PageSetIndex { get; }

		#endregion
	}
}
