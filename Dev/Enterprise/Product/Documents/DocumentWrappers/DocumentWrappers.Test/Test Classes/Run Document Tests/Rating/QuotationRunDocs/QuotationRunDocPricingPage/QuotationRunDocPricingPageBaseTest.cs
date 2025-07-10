using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	internal abstract class QuotationRunDocPricingPageBaseTest : BaseRunDocumentsTest
	{
		[TestDate(2023, 01, 01)]
		public void Test_Currency()
		{
			var client = TestHelper.NewOrgHeader(1);
			var quote = TestHelper.NewQuote(client);
			var rateEntry1 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 10m);
			rateEntry1.AddFlatRateLine("BAF", 11m);
			rateEntry1.AddUnitRateLine("WAR", 12m, lineUnit: "KG");
			AddPackageCountCalculator(rateEntry1, "CAF", baseRate: 13m, firstPackageRate: 14m, additionalPackageRate: 15m);

			var rateEntry2 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.ULD, "AUSYD", "USLAX", "FRT", 20m, container: "LD-7");
			rateEntry2.AddFlatRateLine("BAF", 21m);
			rateEntry2.AddUnitRateLine("WAR", 22m, lineUnit: "KG");
			AddPackageCountCalculator(rateEntry2, "CAF", baseRate: 23m, firstPackageRate: 24m, additionalPackageRate: 25m);

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, MenuName);
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var printTask = GetPrintTask(quote, language: "EN-US");

				AssertPrintTaskRun
				(
					printTask,
					expectedOutput: Test_CurrencyENUSExpectedResult,
					language: "EN-US",
					message: "ClientRate should be shown because it is prioritized over CompanyTariff"
				);

				var printJobs = new StmPrintJobCollection(Factory);
				printJobs.Load();
				printJobs.RemoveAndDeleteAll();

				AssertPrintTaskRun
				(
					printTask,
					expectedOutput: Test_CurrencyITITExpectedResult,
					language: "IT-IT",
					message: "ClientRate should be shown because it is prioritized over CompanyTariff"
				);
			}
		}

		protected abstract string Test_CurrencyENUSExpectedResult { get; }

		protected abstract string Test_CurrencyITITExpectedResult { get; }

		RateLine AddPackageCountCalculator(RateEntry rateEntry, string charge, decimal baseRate, decimal firstPackageRate, decimal additionalPackageRate)
		{
			var rateLine = rateEntry.AddRateLine("CAF", PackageCountCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			var calculator = rateLine.GetCalculator<PackageCountCalculator>();
			calculator.BaseRate = baseRate;
			calculator.FirstPackageRate = firstPackageRate;
			calculator.AddtionalPackageRate = additionalPackageRate;

			return rateLine;
		}

		PrintTask GetPrintTask(IDocumentSupportable documentSupportable, string language)
		{
			var documentCommand = GetDocumentCommand();
			var documentPack = new DocumentPack(documentCommand, documentSupportable, null, null, language: language);
			var printTask = new PrintTask();
			printTask.Add(documentPack);

			return printTask;
		}

		[TestDate(2023, 01, 01)]
		public void TestContractNumber_QuotationWithFreightChargeAndClientRateAndCompanyTariffWithOriginCharge()
		{
			var companyTariff = Factory.New<CompanyTariff>();
			companyTariff.TH_GlobalRateLevel = 1;
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.FCL, "AUSYD", "USLAX", "ODOC", 1m, container: "20GP", lineOrder: 3);

			var client = TestHelper.NewOrgHeader(1);
			var clientRate = TestHelper.NewClientRate(client);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.FCL, "AUSYD", "USLAX", "ODOC", 10m, container: "20GP", lineOrder: 4);

			var quote = TestHelper.NewQuote(client);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "FRT", 1000m, container: "20GP", lineOrder: 1);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "FRT", 1001m, container: "20GP", contractNumber: "CONTRACT1", lineOrder: 2);

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, MenuName);
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: TestContractNumber_QuotationWithFreightChargeAndClientRateAndCompanyTariffWithOriginChargeExpectedResult,
					message: "ClientRate should be shown because it is prioritized over CompanyTariff"
				);
			}
		}

		protected abstract string TestContractNumber_QuotationWithFreightChargeAndClientRateAndCompanyTariffWithOriginChargeExpectedResult { get; }

		[TestDate(2023, 01, 01)]
		public void TestFrequency_QuotationWithFreightChargeAndCompanyTariffWithOriginCharges()
		{
			var companyTariff = Factory.New<CompanyTariff>();
			companyTariff.TH_GlobalRateLevel = 1;
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.FCL, "AUSYD", "USLAX", "ODOC", 10, container: "20GP", lineOrder: 2);
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.FCL, "AUSYD", "USLAX", "ODOC", 11, container: "20GP", frequency: 1, frequencyUnit: FrequencyList.Codes.Days, lineOrder: 3);
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.FCL, "AUSYD", "USLAX", "ODOC", 12, container: "20GP", frequency: 2, frequencyUnit: FrequencyList.Codes.Days, lineOrder: 4);

			var client = TestHelper.NewOrgHeader(1);
			var quote = TestHelper.NewQuote(client);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "FRT", 1000m, container: "20GP", frequency: 1, frequencyUnit: FrequencyList.Codes.Days, lineOrder: 1);

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, MenuName);
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: TestFrequency_QuotationWithFreightChargeAndCompanyTariffWithOriginChargesExpectedResult,
					message: "CompanyTariff should be shown regardless its frequency"
				);
			}
		}

		protected abstract string TestFrequency_QuotationWithFreightChargeAndCompanyTariffWithOriginChargesExpectedResult { get; }

		[TestDate(2023, 01, 01)]
		public void TestFrequency_FreightCharges()
		{
			var companyTariff = Factory.New<CompanyTariff>();
			companyTariff.TH_GlobalRateLevel = 1;
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "FRT", 10, container: "20GP", lineOrder: 5);
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "FRT", 11, container: "20GP", frequency: 1, frequencyUnit: FrequencyList.Codes.Days, lineOrder: 6);
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "IDJKT", "SGSIN", "BAF", 21, container: "20GP", frequency: 1, frequencyUnit: FrequencyList.Codes.Days, lineOrder: 7);
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "USCHI", "NZAKL", "CAF", 30, container: "20GP", lineOrder: 8);
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "CNSHA", "AUBNE", "WAR", 40, container: "20GP", lineOrder: 9);
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "CNSHA", "AUBNE", "WAR", 41, container: "20GP", frequency: 1, frequencyUnit: FrequencyList.Codes.Days, lineOrder: 10);
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "CNSHA", "AUBNE", "WAR", 42, container: "20GP", frequency: 2, frequencyUnit: FrequencyList.Codes.Days, lineOrder: 11);

			var client = TestHelper.NewOrgHeader(1);
			var quote = TestHelper.NewQuote(client);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "FRT", 1000m, container: "20GP", lineOrder: 1); // Matching CompanyTariff with blank and has frequency
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "IDJKT", "SGSIN", "BAF", 2000m, container: "20GP", lineOrder: 2); // Matching CompanyTariff with frequency
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "USCHI", "NZAKL", "CAF", 3000m, container: "20GP", frequency: 1, frequencyUnit: FrequencyList.Codes.Days, lineOrder: 3); // Matching Company Tariff with blank frequency
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "CNSHA", "AUBNE", "WAR", 4000m, container: "20GP", frequency: 1, frequencyUnit: FrequencyList.Codes.Days, lineOrder: 4); // Matching Company Tariff with blank, has same frequency and has different frequency

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, MenuName);
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: TestFrequency_FreightChargesExpectedResult,
					message: "CompanyTariff should not be shown because it is overriden by Quotation"
				);
			}
		}

		protected abstract string TestFrequency_FreightChargesExpectedResult { get; }

		[TestDate(2020, 1, 1)]
		public void TestDocument_DocBuilder_ClientRateWithSimilarOriginAndDestination()
		{
			var client = TestHelper.NewOrgHeader(1);
			var clientRate = TestHelper.NewClientRate(client);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "ID", "FRT", 101m, container: "20GP", lineOrder: 5);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "ID", "FRT", 102m, container: "40GP", lineOrder: 6);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "IDJKT", "FRT", 103m, container: "40HC", lineOrder: 7);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "ID", "ODOC", 201m, lineOrder: 8);

			var quote = TestHelper.NewQuote(client);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "ID", "FRT", 1001m, container: "20GP", lineOrder: 1);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "ID", "FRT", 1002m, container: "40GP", lineOrder: 2);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "IDJKT", "FRT", 1003m, container: "40HC", lineOrder: 3);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "ID", "ODOC", 2001m, lineOrder: 4);

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, MenuName);

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: TestDocument_DocBuilder_ClientRateWithSimilarOriginAndDestinationExpectedResult
				);
			}
		}

		protected abstract string TestDocument_DocBuilder_ClientRateWithSimilarOriginAndDestinationExpectedResult { get; }

		protected abstract string MenuName { get; }

		#region Implementation

		public override BusinessObject GetBusinessObject => Quote;

		public override BusinessContext BusinessContext => BusinessContext.Quotation;

		protected override void SetUp()
		{
			base.SetUp();
			var client = TestHelper.NewOrgHeader(1);
			Quote = TestHelper.NewQuote(client);
		}

		Quote Quote;

		protected TestHelper TestHelper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;

		#endregion
	}
}
