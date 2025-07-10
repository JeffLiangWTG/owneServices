using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	internal abstract class QuotationRunDocPricingPageChargeBaseTest : BaseRunDocumentsTest
	{
		#region Global Sell Rates Override Local

		[TestDate(2023, 01, 01)]
		public void TestGlobalSellRatesOverrideLocal_QuotationAndClientRateAndCompanyTariffHaveSameCharges_Enable()
			=> TestGlobalSellRatesOverrideLocal
			(
				RateCategory,
				RateMode,
				ChargeStrategy.Charge1,
				isGlobalSellRatesOverrideLocal: true,
				expectedResult: TestGlobalSellRatesOverrideLocal_QuotationAndClientRateAndCompanyTariffHaveSameCharges_Enable_ExpectedResult
			);
		protected abstract string TestGlobalSellRatesOverrideLocal_QuotationAndClientRateAndCompanyTariffHaveSameCharges_Enable_ExpectedResult { get; }

		[TestDate(2023, 01, 01)]
		public void TestGlobalSellRatesOverrideLocal_QuotationAndClientRateAndCompanyTariffHaveSameCharges_Disable()
			=> TestGlobalSellRatesOverrideLocal
			(
				RateCategory,
				RateMode,
				ChargeStrategy.Charge1,
				isGlobalSellRatesOverrideLocal: false,
				expectedResult: TestGlobalSellRatesOverrideLocal_QuotationAndClientRateAndCompanyTariffHaveSameCharges_Disable_ExpectedResult
			);
		protected abstract string TestGlobalSellRatesOverrideLocal_QuotationAndClientRateAndCompanyTariffHaveSameCharges_Disable_ExpectedResult { get; }

		#region Freight

		[TestDate(2023, 01, 01)]
		public void TestGlobalSellRatesOverrideLocal_QuoteWithFRT_Enable()
			=> TestGlobalSellRatesOverrideLocal
			(
				RatingConstants.RateCategory.FCL,
				Core.Constants.RateMode.SEA,
				"FRT100",
				isGlobalSellRatesOverrideLocal: true,
				expectedResult: TestGlobalSellRatesOverrideLocal_QuoteWithFRT_EnableExpectedResult
			);
		protected abstract string TestGlobalSellRatesOverrideLocal_QuoteWithFRT_EnableExpectedResult { get; }

		[TestDate(2023, 01, 01)]
		public void TestGlobalSellRatesOverrideLocal_QuoteWithFRT_Disable()
			=> TestGlobalSellRatesOverrideLocal
			(
				RatingConstants.RateCategory.FCL,
				Core.Constants.RateMode.SEA,
				"FRT100",
				isGlobalSellRatesOverrideLocal: false,
				expectedResult: TestGlobalSellRatesOverrideLocal_QuoteWithFRT_DisableExpectedResult
			);
		protected abstract string TestGlobalSellRatesOverrideLocal_QuoteWithFRT_DisableExpectedResult { get; }

		#endregion

		#region Non Freight (Origin)

		[TestDate(2023, 01, 01)]
		public void TestGlobalSellRatesOverrideLocal_QuoteWithORG_Enable()
			=> TestGlobalSellRatesOverrideLocal
			(
				RatingConstants.RateCategory.ORG,
				Core.Constants.RateMode.SEA,
				"ODOC",
				isGlobalSellRatesOverrideLocal: true,
				expectedResult: TestGlobalSellRatesOverrideLocal_QuoteWithORG_EnableExpectedResult
			);
		protected abstract string TestGlobalSellRatesOverrideLocal_QuoteWithORG_EnableExpectedResult { get; }

		[TestDate(2023, 01, 01)]
		public void TestGlobalSellRatesOverrideLocal_QuoteWithORG_Disable()
			=> TestGlobalSellRatesOverrideLocal
			(
				RatingConstants.RateCategory.ORG,
				Core.Constants.RateMode.SEA,
				"ODOC",
				isGlobalSellRatesOverrideLocal: false,
				expectedResult: TestGlobalSellRatesOverrideLocal_QuoteWithORG_DisableExpectedResult
			);
		protected abstract string TestGlobalSellRatesOverrideLocal_QuoteWithORG_DisableExpectedResult { get; }

		#endregion

		void TestGlobalSellRatesOverrideLocal(string quoteRateCategory, string quoteRateMode, string quoteCharge, bool isGlobalSellRatesOverrideLocal, string expectedResult)
		{
			var globalCompanyTariff = Factory.New<GlobalTariff>();
			globalCompanyTariff.TH_GlobalRateLevel = 1;
			globalCompanyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge1, 11, container: "20GP", lineOrder: 11);
			globalCompanyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge2, 12, container: "20GP", lineOrder: 12);
			globalCompanyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge3, 13, container: "20GP", lineOrder: 13);
			globalCompanyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge4, 14, container: "20GP", lineOrder: 14);
			globalCompanyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge5, 15, container: "20GP", lineOrder: 15);

			var localCompanyTariff = Factory.New<CompanyTariff>();
			localCompanyTariff.TH_GlobalRateLevel = 1;
			localCompanyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge1, 21, container: "20GP", lineOrder: 7);
			localCompanyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge2, 22, container: "20GP", lineOrder: 8);
			localCompanyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge3, 23, container: "20GP", lineOrder: 9);
			localCompanyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge4, 24, container: "20GP", lineOrder: 10);

			var client = TestHelper.NewOrgHeader(1);
			var globalClientRate = TestHelper.NewGlobalClientRate(client);
			globalClientRate.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge1, 101m, container: "20GP", lineOrder: 4);
			globalClientRate.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge2, 102m, container: "20GP", lineOrder: 5);
			globalClientRate.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge3, 103m, container: "20GP", lineOrder: 6);

			var localClientRate = TestHelper.NewClientRate(client);
			localClientRate.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge1, 201m, container: "20GP", lineOrder: 2);
			localClientRate.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge2, 202m, container: "20GP", lineOrder: 3);

			var quote = TestHelper.NewQuote(client);
			quote.AddRateEntryWithFlatRateLine(quoteRateCategory, quoteRateMode, "AUSYD", "USLAX", quoteCharge, 1001m, container: "20GP", lineOrder: 1);

			AssertRunDocument(MenuName, useDocBuilder: true, isGlobalSellRatesOverrideLocal: isGlobalSellRatesOverrideLocal, quote, expected: expectedResult);
		}

		protected abstract string MenuName { get; }

		protected abstract string RateCategory { get; }

		protected abstract string RateMode { get; }

		protected abstract IChargeStrategyForTest ChargeStrategy { get; }

		#endregion

		[TestDate(2023, 01, 01)]
		public void TestContractNumber()
		{
			var companyTariff = Factory.New<CompanyTariff>();
			companyTariff.TH_GlobalRateLevel = 1;
			companyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge0, 10, container: "20GP", lineOrder: 9);
			companyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge3, 13, container: "20GP", lineOrder: 10, contractNumber: "CONTRACT3");
			companyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge4, 14, container: "20GP", lineOrder: 11, contractNumber: "CONTRACT4");
			companyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge6, 16, container: "20GP", lineOrder: 12, contractNumber: "CONTRACT6"); // Only CompanyTariff

			var client = TestHelper.NewOrgHeader(1);
			var clientRate = TestHelper.NewClientRate(client);
			clientRate.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge0, 100m, container: "20GP", lineOrder: 5);
			clientRate.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge2, 102m, container: "20GP", lineOrder: 6, contractNumber: "CONTRACT2");
			clientRate.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge4, 104m, container: "20GP", lineOrder: 7, contractNumber: "CONTRACT4");
			clientRate.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge5, 105m, container: "20GP", lineOrder: 8, contractNumber: "CONTRACT5"); // Only ClientRate

			var quote = TestHelper.NewQuote(client);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "BAF", 1001m, container: "20GP", lineOrder: 1, contractNumber: "CONTRACT1"); // Only quotation
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "CAF", 1002m, container: "20GP", lineOrder: 2, contractNumber: "CONTRACT2"); // Quotation and ClientRate
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "WAR", 1003m, container: "20GP", lineOrder: 3, contractNumber: "CONTRACT3"); // Quotation and CompanyTariff
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "FSC", 1004m, container: "20GP", lineOrder: 4, contractNumber: "CONTRACT4"); // Quotation and ClientRate and CompanyTariff

			AssertRunDocument(MenuName, useDocBuilder: true, isGlobalSellRatesOverrideLocal: false, quote, expected: ContractNumberExpectedResult);
		}

		protected abstract string ContractNumberExpectedResult { get; }

		[TestDate(2023, 01, 01)]
		public void TestTransitTime()
		{
			var companyTariff = Factory.New<CompanyTariff>();
			companyTariff.TH_GlobalRateLevel = 1;
			companyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge0, 10, container: "20GP", lineOrder: 9);
			companyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge3, 13, container: "20GP", lineOrder: 10, transitTime: "3");
			companyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge4, 14, container: "20GP", lineOrder: 11, transitTime: "4");
			companyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge6, 16, container: "20GP", lineOrder: 12, transitTime: "6"); // Only CompanyTariff

			var client = TestHelper.NewOrgHeader(1);
			var clientRate = TestHelper.NewClientRate(client);
			clientRate.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge0, 100m, container: "20GP", lineOrder: 5);
			clientRate.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge2, 102m, container: "20GP", lineOrder: 6, transitTime: "2");
			clientRate.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge4, 104m, container: "20GP", lineOrder: 7, transitTime: "4");
			clientRate.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge5, 105m, container: "20GP", lineOrder: 8, transitTime: "5"); // Only ClientRate

			var quote = TestHelper.NewQuote(client);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "BAF", 1001m, container: "20GP", lineOrder: 1, transitTime: "1"); // Only quotation
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "CAF", 1002m, container: "20GP", lineOrder: 2, transitTime: "2"); // Quotation and ClientRate
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "WAR", 1003m, container: "20GP", lineOrder: 3, transitTime: "3"); // Quotation and CompanyTariff
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "FSC", 1004m, container: "20GP", lineOrder: 4, transitTime: "4"); // Quotation and ClientRate and CompanyTariff

			AssertRunDocument(MenuName, useDocBuilder: true, isGlobalSellRatesOverrideLocal: false, quote, expected: TransitTimeExpectedResult);
		}

		protected abstract string TransitTimeExpectedResult { get; }

		[TestDate(2023, 01, 01)]
		public void TestMatchContainerRateClass()
		{
			var companyTariff = Factory.New<CompanyTariff>();
			companyTariff.TH_GlobalRateLevel = 1;
			companyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge1, 11, currency: "USD", container: "20GP", lineOrder: 3, matchContainerRateClass: false);
			companyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge2, 12, currency: "USD", container: "20GP", lineOrder: 4, matchContainerRateClass: true);

			var client = TestHelper.NewOrgHeader(1);
			var clientRate = TestHelper.NewClientRate(client);
			clientRate.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge1, 101m, currency: "USD", container: "20GP", lineOrder: 2, matchContainerRateClass: false);

			var quote = TestHelper.NewQuote(client);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "BAF", 1001m, currency: "USD", container: "20GP", lineOrder: 1, matchContainerRateClass: false);

			AssertRunDocument(MenuName, useDocBuilder: true, isGlobalSellRatesOverrideLocal: false, quote, expected: MatchContainerRateClassExpectedResult);
		}

		protected abstract string MatchContainerRateClassExpectedResult { get; }

		[TestDate(2023, 01, 01)]
		public void TestFrequency()
		{
			var companyTariff = Factory.New<CompanyTariff>();
			companyTariff.TH_GlobalRateLevel = 1;
			companyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge0, 10, currency: "USD", container: "20GP", lineOrder: 9);
			companyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge3, 13, currency: "USD", container: "20GP", lineOrder: 10, frequency: 3, frequencyUnit: FrequencyList.Codes.Days);
			companyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge4, 14, currency: "USD", container: "20GP", lineOrder: 11, frequency: 4, frequencyUnit: FrequencyList.Codes.Days);
			companyTariff.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge6, 16, currency: "USD", container: "20GP", lineOrder: 12, frequency: 6, frequencyUnit: FrequencyList.Codes.Days); // Only CompanyTariff

			var client = TestHelper.NewOrgHeader(1);
			var clientRate = TestHelper.NewClientRate(client);
			clientRate.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge0, 100m, currency: "USD", container: "20GP", lineOrder: 5);
			clientRate.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge2, 102m, currency: "USD", container: "20GP", lineOrder: 6, frequency: 2, frequencyUnit: FrequencyList.Codes.Days);
			clientRate.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge4, 104m, currency: "USD", container: "20GP", lineOrder: 7, frequency: 4, frequencyUnit: FrequencyList.Codes.Days);
			clientRate.AddRateEntryWithFlatRateLine(RateCategory, RateMode, "AUSYD", "USLAX", ChargeStrategy.Charge5, 105m, currency: "USD", container: "20GP", lineOrder: 8, frequency: 5, frequencyUnit: FrequencyList.Codes.Days); // Only ClientRate

			var quote = TestHelper.NewQuote(client);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "BAF", 1001m, currency: "USD", container: "20GP", lineOrder: 1, frequency: 1, frequencyUnit: FrequencyList.Codes.Days); // Only quotation
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "CAF", 1002m, currency: "USD", container: "20GP", lineOrder: 2, frequency: 2, frequencyUnit: FrequencyList.Codes.Days); // Quotation and ClientRate
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "WAR", 1003m, currency: "USD", container: "20GP", lineOrder: 3, frequency: 3, frequencyUnit: FrequencyList.Codes.Days); // Quotation and CompanyTariff
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "FSC", 1004m, currency: "USD", container: "20GP", lineOrder: 4, frequency: 4, frequencyUnit: FrequencyList.Codes.Days); // Quotation and ClientRate and CompanyTariff

			AssertRunDocument(MenuName, useDocBuilder: true, isGlobalSellRatesOverrideLocal: false, quote, expected: FrequencyExpectedResult);
		}

		void AssertRunDocument(string menuName, bool useDocBuilder, bool isGlobalSellRatesOverrideLocal, IDocumentSupportable documentSupportable, string expected)
		{
			RunDocumentWithAllSections = ZBool.False;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, menuName);
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, useDocBuilder))
			using (RatingDataRegistry.Instance.GlobalSellRatesOverrideLocal.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isGlobalSellRatesOverrideLocal))
			{
				AssertRunDocument(documentSupportable, expectedOutput: expected);
			}
		}

		protected abstract string FrequencyExpectedResult { get; }

		#region Implementation

		public override BusinessObject GetBusinessObject => Quote;

		public override BusinessContext BusinessContext => BusinessContext.Quotation;

		protected override void SetUp()
		{
			base.SetUp();
			var client = TestHelper.NewOrgHeader(1);
			Quote = TestHelper.NewQuote(client);

			TestHelper.ChargeCodes.CreateGlobalCharge("FRT100");
			Factory.Save();
		}

		Quote Quote;

		protected TestHelper TestHelper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;

		#endregion
	}
}
