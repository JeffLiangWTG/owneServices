using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using PaymentTerms = Enterprise.Core.Constants.PaymentType;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	internal abstract class TableStrategyExtractBaseTest : TestCaseWithFactory
	{
		protected abstract string Category { get; }

		protected abstract string Mode { get; }

		protected abstract string ChargeCode { get; }

		protected abstract BaseTableStrategy Strategy { get; }

		#region Match container Rate Class

		public void TestExtract_MatchContainerRateClass_Complex()
		{
			var quotation = Factory.NewWithValidTestData<Quote>();
			var entry11 = quotation.AddRateEntryWithFlatRateLine(Category, Mode, "AUSYD", "USLAX", ChargeCode, 11m, currency: "AUD", container: "20GP", contractNumber: "CONTRACT1", matchContainerRateClass: true);
			var entry12 = quotation.AddRateEntryWithFlatRateLine(Category, Mode, "AUSYD", "USLAX", ChargeCode, 12m, currency: "AUD", container: "40GP", contractNumber: "CONTRACT1", matchContainerRateClass: false);
			var entry21 = quotation.AddRateEntryWithFlatRateLine(Category, Mode, "AUSYD", "USLAX", ChargeCode, 21m, currency: "AUD", container: "20GP", contractNumber: "CONTRACT2", matchContainerRateClass: false);
			var entry22 = quotation.AddRateEntryWithFlatRateLine(Category, Mode, "AUSYD", "USLAX", ChargeCode, 22m, currency: "AUD", container: "40GP", contractNumber: "CONTRACT2", matchContainerRateClass: false);
			var entry31 = quotation.AddRateEntryWithFlatRateLine(Category, Mode, "AUSYD", "USLAX", ChargeCode, 31m, currency: "AUD", container: "20GP", contractNumber: "CONTRACT3", matchContainerRateClass: false);
			var entry32 = quotation.AddRateEntryWithFlatRateLine(Category, Mode, "AUSYD", "USLAX", ChargeCode, 32m, currency: "AUD", container: "40GP", contractNumber: "CONTRACT3", matchContainerRateClass: true);

			Factory.Save();

			AssertExtractedResultsInString
			(
				TestExtract_MatchContainerRateClassComplexExpectedResult,
				TableStrategyTestHelper.NewPricingPage(entry11, entry12, entry21, entry22, entry31, entry32)
			);
		}
		protected abstract string TestExtract_MatchContainerRateClassComplexExpectedResult { get; }

		#endregion

		public void TestExtract_Container()
		{
			var tariff = Factory.New<CompanyTariff>();
			var rateEntry1 = tariff.AddRateEntryWithFlatRateLine(Category, Mode, "AUSYD", "USLAX", ChargeCode, 10m, currency: "AUD", container: "20GP");
			var rateEntry2 = tariff.AddRateEntryWithFlatRateLine(Category, Mode, "AUSYD", "USLAX", ChargeCode, 20m, currency: "AUD", container: "40GP");

			AssertExtractedResultsInString
			(
				TestExtract_ContainerExpectedResult,
				TableStrategyTestHelper.NewPricingPage(rateEntry1, rateEntry2)
			);
		}
		protected abstract string TestExtract_ContainerExpectedResult { get; }

		#region Quotation with similar rateEntries

		public void TestExtract_Quotation_SimilarRateEntries_MatchContainerRateClass()
			=> TestExtract_Quotation_SimilarRateEntries
			(
				RateEntrySchema.TI_MatchContainerRateClass,
				value1: true,
				value2: false,
				expectedResults: TestExtract_Quotation_SimilarRateEntries_MatchContainerRateClassExpectedResult
			);
		protected abstract string TestExtract_Quotation_SimilarRateEntries_MatchContainerRateClassExpectedResult { get; }

		public void TestExtract_Quotation_SimilarRateEntries_ContractNumber()
			=> TestExtract_Quotation_SimilarRateEntries
			(
				RateEntrySchema.TI_ContractNumber,
				value1: "CONTRACT1",
				value2: "CONTRACT2",
				expectedResults: TestExtract_Quotation_SimilarRateEntries_ContractNumberExpectedResult
			);
		protected abstract string TestExtract_Quotation_SimilarRateEntries_ContractNumberExpectedResult { get; }

		public void TestExtract_Quotation_SimilarRateEntries_PaymentTerm()
			=> TestExtract_Quotation_SimilarRateEntries
			(
				RateEntrySchema.TI_PaymentTerm,
				value1: PaymentTerms.Collect,
				value2: PaymentTerms.Prepaid,
				expectedResults: TestExtract_Quotation_SimilarRateEntries_PaymentTermExpectedResult
			);
		protected abstract string TestExtract_Quotation_SimilarRateEntries_PaymentTermExpectedResult { get; }

		public void TestExtract_Quotation_SimilarRateEntries_Commodity()
			=> TestExtract_Quotation_SimilarRateEntries
			(
				RateEntrySchema.TI_RH_NKCommodityCode,
				value1: "GEN",
				value2: "HAZ",
				expectedResults: TestExtract_Quotation_SimilarRateEntries_CommodityExpectedResult
			);
		protected abstract string TestExtract_Quotation_SimilarRateEntries_CommodityExpectedResult { get; }

		public void TestExtract_Quotation_SimilarRateEntries_TransitTime()
			=> TestExtract_Quotation_SimilarRateEntries
			(
				RateEntrySchema.TI_TransitTime,
				value1: "1",
				value2: "2",
				expectedResults: TestExtract_Quotation_SimilarRateEntries_TransitTimeExpectedResult
			);
		protected abstract string TestExtract_Quotation_SimilarRateEntries_TransitTimeExpectedResult { get; }

		public void TestExtract_Quotation_SimilarRateEntries_ServiceLevel()
			=> TestExtract_Quotation_SimilarRateEntries
			(
				RateEntrySchema.TI_RS_NKServiceLevel_NI,
				value1: "STD",
				value2: "EXP",
				expectedResults: TestExtract_Quotation_SimilarRateEntries_ServiceLevelExpectedResult
			);
		protected abstract string TestExtract_Quotation_SimilarRateEntries_ServiceLevelExpectedResult { get; }

		public void TestExtract_Quotation_SimilarRateEntries_CarrierServiceLevel()
			=> TestExtract_Quotation_SimilarRateEntries
			(
				RateEntrySchema.TI_PL_NKCarrierServiceLevel,
				value1: "STD",
				value2: "EXP",
				expectedResults: TestExtract_Quotation_SimilarRateEntries_CarrierServiceLevelExpectedResult
			);
		protected abstract string TestExtract_Quotation_SimilarRateEntries_CarrierServiceLevelExpectedResult { get; }

		public void TestExtract_Quotation_SimilarRateEntries_CartageDeliveryAddressPostCode()
			=> TestExtract_Quotation_SimilarRateEntries
			(
				RateEntrySchema.TI_CartageDeliveryAddressPostCode,
				value1: "1111",
				value2: "2222",
				expectedResults: TestExtract_Quotation_SimilarRateEntries_CartageDeliveryAddressPostCodeExpectedResult
			);
		protected abstract string TestExtract_Quotation_SimilarRateEntries_CartageDeliveryAddressPostCodeExpectedResult { get; }

		public void TestExtract_Quotation_SimilarRateEntries_CartagePickupAddressPostCode()
			=> TestExtract_Quotation_SimilarRateEntries
			(
				RateEntrySchema.TI_CartagePickupAddressPostCode,
				value1: "1111",
				value2: "2222",
				expectedResults: TestExtract_Quotation_SimilarRateEntries_CartagePickupAddressPostCodeExpectedResult
			);
		protected abstract string TestExtract_Quotation_SimilarRateEntries_CartagePickupAddressPostCodeExpectedResult { get; }

		public void TestExtract_Quotation_SimilarRateEntries_Via()
			=> TestExtract_Quotation_SimilarRateEntries
			(
				RateEntrySchema.TI_ViaLRC,
				value1: "SGSIN",
				value2: "NZAKL",
				expectedResults: TestExtract_Quotation_SimilarRateEntries_ViaExpectedResult
			);
		protected abstract string TestExtract_Quotation_SimilarRateEntries_ViaExpectedResult { get; }

		public void TestExtract_Quotation_SimilarRateEntries_Supplier()
			=> TestExtract_Quotation_SimilarRateEntries
			(
				RateEntrySchema.TI_OH_Supplier,
				value1: TestHelper.TransportProvider1.PK,
				value2: TestHelper.TransportProvider2.PK,
				expectedResults: TestExtract_Quotation_SimilarRateEntries_SupplierExpectedResult
			);
		protected abstract string TestExtract_Quotation_SimilarRateEntries_SupplierExpectedResult { get; }

		public void TestExtract_Quotation_SimilarRateEntries_TransportProvider()
			=> TestExtract_Quotation_SimilarRateEntries
			(
				RateEntrySchema.TI_OH_TransportProvider,
				value1: TestHelper.TransportProvider1.PK,
				value2: TestHelper.TransportProvider2.PK,
				expectedResults: TestExtract_Quotation_SimilarRateEntries_TransportProviderExpectedResult
			);
		protected abstract string TestExtract_Quotation_SimilarRateEntries_TransportProviderExpectedResult { get; }

		public void TestExtract_Quotation_SimilarRateEntries_ControllingCustomer()
			=> TestExtract_Quotation_SimilarRateEntries
			(
				RateEntrySchema.TI_OH_ControllingCustomer,
				value1: TestHelper.NewClient1.PK,
				value2: TestHelper.NewClient2.PK,
				expectedResults: TestExtract_Quotation_SimilarRateEntries_ControllingCustomerExpectedResult
			);
		protected abstract string TestExtract_Quotation_SimilarRateEntries_ControllingCustomerExpectedResult { get; }

		void TestExtract_Quotation_SimilarRateEntries<T>(SchemaColumn schemaColumn, T value1, T value2, string expectedResults)
		{
			var quotation = Factory.NewWithValidTestData<Quote>();
			var rateEntry11 = quotation.AddRateEntryWithFlatRateLine(Category, Mode, "AUSYD", "USLAX", ChargeCode, 201, schemaColumn, value1, currency: "AUD", container: "20GP");
			var rateEntry12 = quotation.AddRateEntryWithFlatRateLine(Category, Mode, "AUSYD", "USLAX", ChargeCode, 202, schemaColumn, value2, currency: "AUD", container: "20GP");
			var rateEntry21 = quotation.AddRateEntryWithFlatRateLine(Category, Mode, "AUSYD", "USLAX", ChargeCode, 401, schemaColumn, value1, currency: "AUD", container: "40GP");
			var rateEntry22 = quotation.AddRateEntryWithFlatRateLine(Category, Mode, "AUSYD", "USLAX", ChargeCode, 402, schemaColumn, value2, currency: "AUD", container: "40GP");

			Factory.Save();

			AssertExtractedResultsInString
			(
				expectedResults,
				TableStrategyTestHelper.NewPricingPage(rateEntry11, rateEntry12, rateEntry21, rateEntry22)
			);
		}

		#endregion

		#region Quotation and ClientRate with similar RateEntries

		#region RateEntry field trigger new page

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_CarrierOrganization()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_OH_TransportProvider,
				TestHelper.OrgHeader1.PK,
				TestHelper.OrgHeader2.PK,
				TestHelper.OrgHeader3.PK,
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_CarrierOrganization_ExpectedResult
			);
		protected abstract string TestExtract_QuotationAndClientRate_SimilarRateEntries_CarrierOrganization_ExpectedResult { get; }

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_Commodity()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_RH_NKCommodityCode,
				"HAZ",
				"GEN",
				"ALUM",
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_Commodity_ExpectedResult
			);
		protected abstract string TestExtract_QuotationAndClientRate_SimilarRateEntries_Commodity_ExpectedResult { get; }

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_ServiceLevel()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_RS_NKServiceLevel_NI,
				"DEF",
				"DIR",
				"TSP",
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_ServiceLevel_ExpectedResult
			);
		protected abstract string TestExtract_QuotationAndClientRate_SimilarRateEntries_ServiceLevel_ExpectedResult { get; }

		#endregion

		#region RateEntry field in PricingPageLineSetFactory.DistinguishingColumnsForDocumentGrouping

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_CarrierServiceLevel()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_PL_NKCarrierServiceLevel,
				"STD",
				"EXP",
				"XXX",
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_CarrierServiceLevel_ExpectedResult
			);
		protected virtual string TestExtract_QuotationAndClientRate_SimilarRateEntries_CarrierServiceLevel_ExpectedResult => TestExtract_QuotationAndClientRate_SimilarRateEntries_ExpectedResult;

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_Consignee()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_OH_Consignee,
				TestHelper.OrgHeader1.PK,
				TestHelper.OrgHeader2.PK,
				TestHelper.OrgHeader3.PK,
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_ExpectedResult
			);

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_Consignor()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_OH_Consignor,
				TestHelper.OrgHeader1.PK,
				TestHelper.OrgHeader2.PK,
				TestHelper.OrgHeader3.PK,
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_ExpectedResult
			);

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_ControllingCustomer()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_OH_ControllingCustomer,
				TestHelper.OrgHeader1.PK,
				TestHelper.OrgHeader2.PK,
				TestHelper.OrgHeader3.PK,
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_ExpectedResult
			);

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_DestinationPostCode()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_CartageDeliveryAddressPostCode,
				"1111",
				"2222",
				"3333",
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_DestinationPostCode_ExpectedResult
			);
		protected virtual string TestExtract_QuotationAndClientRate_SimilarRateEntries_DestinationPostCode_ExpectedResult => TestExtract_QuotationAndClientRate_SimilarRateEntries_ExpectedResult;

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_OriginPostCode()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_CartagePickupAddressPostCode,
				"1111",
				"2222",
				"3333",
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_OriginPostCode_ExpectedResult
			);
		protected virtual string TestExtract_QuotationAndClientRate_SimilarRateEntries_OriginPostCode_ExpectedResult => TestExtract_QuotationAndClientRate_SimilarRateEntries_ExpectedResult;

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_TransitTime()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_TransitTime,
				"1",
				"2",
				"3",
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_ExpectedResult
			);

		#endregion

		#region RateEntry field in RelatedEntriesLoader.DistinguishingColumnsForDocumentGroupingExactMatch

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_MatchContainerRateClass()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_MatchContainerRateClass,
				true,
				false,
				false,
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_MatchContainerRateClass_ExpectedResult
			);
		protected abstract string TestExtract_QuotationAndClientRate_SimilarRateEntries_MatchContainerRateClass_ExpectedResult { get; }

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_ClientContractNumber()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_ContractNumber,
				"CONT1",
				"CONT2",
				"CONT3",
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_ClientContractNumber_ExpectedResult
			);
		protected abstract string TestExtract_QuotationAndClientRate_SimilarRateEntries_ClientContractNumber_ExpectedResult { get; }

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_Frequency()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_Frequency,
				1,
				2,
				3,
				setRateEntry: (rateEntry) => rateEntry.TI_FrequencyUnit = RatingConstants.FrequencyUnits.Days,
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_Frequency_ExpectedResult
			);
		protected abstract string TestExtract_QuotationAndClientRate_SimilarRateEntries_Frequency_ExpectedResult { get; }

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_FrequencyUnit()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_FrequencyUnit,
				RatingConstants.FrequencyUnits.Days,
				RatingConstants.FrequencyUnits.Fortnight,
				RatingConstants.FrequencyUnits.Week,
				setRateEntry: (rateEntry) => rateEntry.TI_Frequency = 1,
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_FrequencyUnit_ExpectedResults
			);
		protected abstract string TestExtract_QuotationAndClientRate_SimilarRateEntries_FrequencyUnit_ExpectedResults { get; }

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_PaymentTerm()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_PaymentTerm,
				"CCX",
				"PPD",
				"XXX",
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_NonDistinguishingColumnsForDocumentGrouping_ExpectedResult
			);

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_PlannedDischarge()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_PlannedDischargeLRC,
				"AUSYD",
				"USLAX",
				"SGSIN",
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_NonDistinguishingColumnsForDocumentGrouping_ExpectedResult
			);

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_PlannedLoad()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_PlannedLoadLRC,
				"AUSYD",
				"USLAX",
				"SGSIN",
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_NonDistinguishingColumnsForDocumentGrouping_ExpectedResult
			);

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_RateDestination()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_RateDestination,
				"AUSYD",
				"USLAX",
				"SGSIN",
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_NonDistinguishingColumnsForDocumentGrouping_ExpectedResult
			);

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_RateOrigin()
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries
			(
				RateEntrySchema.TI_RateOrigin,
				"AUSYD",
				"USLAX",
				"SGSIN",
				expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_NonDistinguishingColumnsForDocumentGrouping_ExpectedResult
			);

		#endregion

		void TestExtract_QuotationAndClientRate_SimilarRateEntries<T>(SchemaColumn schemaColumn, T value0, T value1, T value2, string expected, Action<RateEntry> setRateEntry = default)
		{
			var helper = new Rating.Business.Testing.TestHelper(Factory);
			var client = helper.NewOrgHeader(1);

			var clientRate = helper.NewClientRate(client);
			var clientRateEntry1 = clientRate.AddRateEntryWithFlatRateLine(Category, Mode, "AUSYD", "USLAX", ChargeCode, 20, schemaColumn, value0, currency: "AUD", lineOrder: 1, container: "20GP");
			setRateEntry?.Invoke(clientRateEntry1);
			var clientRateEntry2 = clientRate.AddRateEntryWithFlatRateLine(Category, Mode, "AUSYD", "USLAX", ChargeCode, 21, schemaColumn, value1, currency: "AUD", lineOrder: 2, container: "20GP");
			setRateEntry?.Invoke(clientRateEntry2);
			var clientRateEntry3 = clientRate.AddRateEntryWithFlatRateLine(Category, Mode, "AUSYD", "USLAX", ChargeCode, 40, schemaColumn, value0, currency: "AUD", lineOrder: 3, container: "40GP");
			setRateEntry?.Invoke(clientRateEntry3);

			var clientRateEntry4 = clientRate.AddRateEntryWithFlatRateLine(Category, Mode, "AUSYD", "USLAX", ChargeCode, 22, schemaColumn, value2, currency: "AUD", lineOrder: 4, container: "20GP");
			setRateEntry?.Invoke(clientRateEntry4);

			var quote = helper.NewQuote(client);
			var quoteRateEntry1 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "FRT", 120m, schemaColumn, value0, currency: "AUD", lineOrder: 5, container: "20GP");
			setRateEntry?.Invoke(quoteRateEntry1);
			var quoteRateEntry2 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "FRT", 121m, schemaColumn, value1, currency: "AUD", lineOrder: 6, container: "20GP");
			setRateEntry?.Invoke(quoteRateEntry2);
			var quoteRateEntry3 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "FRT", 140m, schemaColumn, value0, currency: "AUD", lineOrder: 7, container: "40GP");
			setRateEntry?.Invoke(quoteRateEntry3);

			var pricingPageCollection = new PricingPageCollection(quote);
			pricingPageCollection.Load(PricingPaginationStrategy.LandscapeCompactStyle);

			AssertContainsExactLinesInExactOrder
			(
				expected,
				TableStrategyTestHelper.ExtractAsString(pricingPageCollection, Strategy)
			);
		}

		protected abstract string TestExtract_QuotationAndClientRate_SimilarRateEntries_ExpectedResult { get; }

		#endregion

		#region Other

		public void TestExtract_QuotationAndClientRate_SimilarRateEntries_Supplier() => TestExtract_QuotationAndClientRate_SimilarRateEntries
		(
			RateEntrySchema.TI_OH_Supplier,
			TestHelper.OrgHeader1.PK,
			TestHelper.OrgHeader2.PK,
			TestHelper.OrgHeader3.PK,
			expected: TestExtract_QuotationAndClientRate_SimilarRateEntries_NonDistinguishingColumnsForDocumentGrouping_ExpectedResult
		);

		protected abstract string TestExtract_QuotationAndClientRate_SimilarRateEntries_NonDistinguishingColumnsForDocumentGrouping_ExpectedResult { get; }

		#endregion

		#region Implementation

		TableStrategyTestHelper TestHelper => testHelper ?? (testHelper = new TableStrategyTestHelper(Factory));
		TableStrategyTestHelper testHelper;

		void AssertExtractedResultsInString(string expected, PricingPage page, string message = "")
		{
			var extractedPage = Strategy.Extract(page);
			var actual = TableStrategyTestHelper.Render(extractedPage);
			AssertContainsExactLinesInExactOrder(message, expected, actual);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			gp20.RC_FreightRateClass = "20GN";
			gp20.RC_HandlingRateClass = "20GN";

			var gp40 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			gp40.RC_FreightRateClass = "40GN";
			gp40.RC_HandlingRateClass = "40GN";
		}

		#endregion
	}
}
