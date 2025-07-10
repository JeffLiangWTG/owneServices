using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class QuotationRunDocsForwardingLandscapePricingPageOriginTest : QuotationRunDocForwardingLandscapePricingPageChargeBaseTest
	{
		#region Global Sell Rates Override Local

		protected override string TestGlobalSellRatesOverrideLocal_QuotationAndClientRateAndCompanyTariffHaveSameCharges_Disable_ExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Non-Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Origin Charges]

{C}-[Sydney (to Los Angeles)]
{C}-[Sea]

{C}-[ORG1 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[1,001.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[202.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[103.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[24.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[15.00]


{C}-[END OF DOCUMENT ]";

		protected override string TestGlobalSellRatesOverrideLocal_QuotationAndClientRateAndCompanyTariffHaveSameCharges_Enable_ExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Non-Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Origin Charges]

{C}-[Sydney (to Los Angeles)]
{C}-[Sea]

{C}-[ORG1 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[1,001.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[102.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[103.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[14.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[15.00]


{C}-[END OF DOCUMENT ]";

		protected override string TestGlobalSellRatesOverrideLocal_QuoteWithFRT_DisableExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Cross Trade Sea Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Australia - United States]

{C}-[Origin - Destination]   {K}-[Shipping Line]   {S}-[Cur.]   {U}-[20GP]   {AW}-[Transit Time]   {BA}-[Freq.]   {BG}-[W/V Conv.]   {BM}-[Contract No.]

{C}-[Sydney - Los Angeles]
{S}-[FCL]
{S}-[FRT100 Global Charge]
{U}-[20GP]   {AU}-[USD]   {BC}-[1,001.00]

{C}-[Origin Charges]

{C}-[Sydney (to Los Angeles)]
{C}-[Sea]

{C}-[ORG1 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[201.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[202.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[103.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[24.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[15.00]


{C}-[END OF DOCUMENT ]";

		protected override string TestGlobalSellRatesOverrideLocal_QuoteWithFRT_EnableExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Cross Trade Sea Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Australia - United States]

{C}-[Origin - Destination]   {K}-[Shipping Line]   {S}-[Cur.]   {U}-[20GP]   {AW}-[Transit Time]   {BA}-[Freq.]   {BG}-[W/V Conv.]   {BM}-[Contract No.]

{C}-[Sydney - Los Angeles]
{S}-[FCL]
{S}-[FRT100 Global Charge]
{U}-[20GP]   {AU}-[USD]   {BC}-[1,001.00]

{C}-[Origin Charges]

{C}-[Sydney (to Los Angeles)]
{C}-[Sea]

{C}-[ORG1 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[101.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[102.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[103.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[14.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[15.00]


{C}-[END OF DOCUMENT ]";

		protected override string TestGlobalSellRatesOverrideLocal_QuoteWithORG_DisableExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Non-Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Origin Charges]

{C}-[Sydney (to Los Angeles)]
{C}-[Sea]

{C}-[Origin Documentation Fee]
{E}-[20GP]   {AE}-[AUD]   {AM}-[1,001.00]

{C}-[ORG1 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[201.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[202.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[103.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[24.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[15.00]


{C}-[END OF DOCUMENT ]";

		protected override string TestGlobalSellRatesOverrideLocal_QuoteWithORG_EnableExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Non-Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Origin Charges]

{C}-[Sydney (to Los Angeles)]
{C}-[Sea]

{C}-[Origin Documentation Fee]
{E}-[20GP]   {AE}-[AUD]   {AM}-[1,001.00]

{C}-[ORG1 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[101.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[102.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[103.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[14.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[15.00]


{C}-[END OF DOCUMENT ]";

		#endregion

		protected override string TransitTimeExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Cross Trade Sea Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Australia - United States]

{C}-[Origin - Destination]   {K}-[Shipping Line]   {S}-[Cur.]   {U}-[20GP]   {AW}-[Transit Time]   {BA}-[Freq.]   {BG}-[W/V Conv.]   {BM}-[Contract No.]

{C}-[Sydney - Los Angeles]   {AW}-[1 Day]
{S}-[FCL]
{S}-[Bunker Adjustment Factor]
{U}-[20GP]   {AU}-[USD]   {BC}-[1,001.00]

{C}-[Sydney - Los Angeles]   {AW}-[2 Days]
{S}-[FCL]
{S}-[Currency Adjustment Factor]
{U}-[20GP]   {AU}-[USD]   {BC}-[1,002.00]

{C}-[Sydney - Los Angeles]   {AW}-[3 Days]
{S}-[FCL]
{S}-[War Risk Surcharge]
{U}-[20GP]   {AU}-[USD]   {BC}-[1,003.00]

{C}-[Sydney - Los Angeles]   {AW}-[4 Days]
{S}-[FCL]
{S}-[Fuel Surcharge]
{U}-[20GP]   {AU}-[USD]   {BC}-[1,004.00]

{C}-[Origin Charges]

{C}-[Sydney (to Los Angeles)]
{C}-[Sea]

{C}-[ORG0 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[100.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[102.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[104.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[13.00]


{C}-[END OF DOCUMENT ]";

		protected override string ContractNumberExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 2]


{AK}-[Cross Trade Sea Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Australia - United States]

{C}-[Origin - Destination]   {K}-[Shipping Line]   {S}-[Cur.]   {U}-[20GP]   {AW}-[Transit Time]   {BA}-[Freq.]   {BG}-[W/V Conv.]   {BM}-[Contract No.]

{C}-[Sydney - Los Angeles]   {BM}-[CONTRACT1]
{S}-[FCL]
{S}-[Bunker Adjustment Factor]
{U}-[20GP]   {AU}-[USD]   {BC}-[1,001.00]

{C}-[Sydney - Los Angeles]   {BM}-[CONTRACT2]
{S}-[FCL]
{S}-[Currency Adjustment Factor]
{U}-[20GP]   {AU}-[USD]   {BC}-[1,002.00]

{C}-[Sydney - Los Angeles]   {BM}-[CONTRACT3]
{S}-[FCL]
{S}-[War Risk Surcharge]
{U}-[20GP]   {AU}-[USD]   {BC}-[1,003.00]

{C}-[Sydney - Los Angeles]   {BM}-[CONTRACT4]
{S}-[FCL]
{S}-[Fuel Surcharge]
{U}-[20GP]   {AU}-[USD]   {BC}-[1,004.00]


{C}-[Continued Over… ]
{C}-[Origin Charges]

{C}-[Sydney (to Los Angeles)]
{C}-[Sea]

{C}-[ORG0 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[100.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[102.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[104.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[105.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[13.00]

{C}-[ORG6 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[16.00]


{C}-[END OF DOCUMENT ]";

		protected override string MatchContainerRateClassExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Cross Trade Sea Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Australia - United States]

{C}-[Origin - Destination]   {K}-[Shipping Line]   {S}-[Cur.]   {U}-[20GP]   {AW}-[Transit Time]   {BA}-[Freq.]   {BG}-[W/V Conv.]   {BM}-[Contract No.]

{C}-[Sydney - Los Angeles]
{S}-[FCL]
{S}-[Bunker Adjustment Factor]
{U}-[20GP]   {AU}-[USD]   {BC}-[1,001.00]

{C}-[Origin Charges]

{C}-[Sydney (to Los Angeles)]
{C}-[Sea]

{C}-[ORG1 Global Charge]
{E}-[20GP]   {AE}-[USD]   {AM}-[101.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AE}-[USD]   {AM}-[12.00]


{C}-[END OF DOCUMENT ]";

		// For destination, each row represent different charges that prioritized ClientRate over CompanyTariff
		protected override string FrequencyExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 2]


{AK}-[Cross Trade Sea Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Australia - United States]

{C}-[Origin - Destination]   {K}-[Shipping Line]   {S}-[Cur.]   {U}-[20GP]   {AW}-[Transit Time]   {BA}-[Freq.]   {BG}-[W/V Conv.]   {BM}-[Contract No.]

{C}-[Sydney - Los Angeles]   {BA}-[Every Day]
{S}-[FCL]
{S}-[Bunker Adjustment Factor]
{U}-[20GP]   {AU}-[USD]   {BC}-[1,001.00]

{C}-[Sydney - Los Angeles]   {BA}-[Every 2 Days]
{S}-[FCL]
{S}-[Currency Adjustment Factor]
{U}-[20GP]   {AU}-[USD]   {BC}-[1,002.00]

{C}-[Sydney - Los Angeles]   {BA}-[Every 3 Days]
{S}-[FCL]
{S}-[War Risk Surcharge]
{U}-[20GP]   {AU}-[USD]   {BC}-[1,003.00]

{C}-[Sydney - Los Angeles]   {BA}-[Every 4 Days]
{S}-[FCL]
{S}-[Fuel Surcharge]
{U}-[20GP]   {AU}-[USD]   {BC}-[1,004.00]


{C}-[Continued Over… ]
{C}-[Origin Charges]

{C}-[Sydney (to Los Angeles)]
{C}-[Sea]

{C}-[ORG0 Global Charge]
{E}-[20GP]   {AE}-[USD]   {AM}-[100.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AE}-[USD]   {AM}-[102.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AE}-[USD]   {AM}-[104.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AE}-[USD]   {AM}-[105.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AE}-[USD]   {AM}-[13.00]

{C}-[ORG6 Global Charge]
{E}-[20GP]   {AE}-[USD]   {AM}-[16.00]


{C}-[END OF DOCUMENT ]";

		#region Air LSE

		[TestDate(2023, 01, 01)]
		public void TestAIR_LSE_SameRates()
		{
			var companyTariff = Factory.New<CompanyTariff>();
			companyTariff.TH_GlobalRateLevel = 1;
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", ChargeStrategy.Charge1, 11, container: "20GP", contractNumber: "", lineOrder: 4);
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", ChargeStrategy.Charge2, 12, container: "20GP", contractNumber: "", lineOrder: 5);
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", ChargeStrategy.Charge3, 13, container: "20GP", contractNumber: "", lineOrder: 6);

			var client = TestHelper.NewOrgHeader(1);
			var clientRate = TestHelper.NewClientRate(client);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", ChargeStrategy.Charge1, 101m, container: "20GP", contractNumber: "", lineOrder: 2);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", ChargeStrategy.Charge2, 102m, container: "20GP", contractNumber: "", lineOrder: 3);

			var quote = TestHelper.NewQuote(client);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", "BAF", 1001m, container: "20GP", contractNumber: "", lineOrder: 1);

			RunDocumentWithAllSections = ZBool.False;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, MenuName);
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Non-Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Origin Charges]

{C}-[Sydney (to Los Angeles)]
{C}-[LSE]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AE}-[AUD]   {AM}-[1,001.00]

{C}-[ORG1 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[101.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[102.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[13.00]


{C}-[END OF DOCUMENT ]",
					message: "GIVEN related company-tariff and client-rate WHEN printing quotation THEN client-rate should be prioritized"
				);
			}
		}

		[TestDate(2023, 01, 01)]
		public void TestAIR_LSE_DifferentRates()
		{
			var companyTariff = Factory.New<CompanyTariff>();
			companyTariff.TH_GlobalRateLevel = 1;
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", ChargeStrategy.Charge1, 11, container: "20GP", contractNumber: "");
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", ChargeStrategy.Charge2, 12, container: "20GP", contractNumber: "");
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", ChargeStrategy.Charge3, 13, container: "20GP", contractNumber: "");

			var client = TestHelper.NewOrgHeader(1);
			var clientRate = TestHelper.NewClientRate(client);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", ChargeStrategy.Charge1, 101m, container: "20GP", contractNumber: "");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", ChargeStrategy.Charge2, 102m, container: "20GP", contractNumber: "");

			var quote = TestHelper.NewQuote(client);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", "BAF", 1001m, container: "20GP", contractNumber: "");

			RunDocumentWithAllSections = ZBool.False;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, MenuName);
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Cross Trade Air Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Australia - United States]

{C}-[Origin - Destination]   {K}-[Airline]   {S}-[Cur.]   {U}-[20GP]   {AW}-[Transit Time]   {BA}-[Freq.]   {BG}-[W/V Conv.]   {BM}-[Contract No.]

{C}-[Sydney - Los Angeles]
{S}-[Air]
{S}-[Bunker Adjustment Factor]
{U}-[20GP]   {AU}-[AUD]   {BC}-[1,001.00]

{C}-[Origin Charges]

{C}-[Sydney (to Los Angeles)]
{C}-[LSE]

{C}-[ORG1 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[101.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[102.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AE}-[AUD]   {AM}-[13.00]


{C}-[END OF DOCUMENT ]",
					message: "GIVEN related company-tariff and client-rate WHEN printing quotation THEN client-rate should be prioritized"
				);
			}
		}

		#endregion

		#region Implementation

		protected override string RateCategory => RatingConstants.RateCategory.ORG;

		protected override string RateMode => Core.Constants.RateMode.SEA;

		protected override IChargeStrategyForTest ChargeStrategy => chargeStrategy ?? (chargeStrategy = new OriginChargeStrategyForTest(TestHelper, Factory));
		IChargeStrategyForTest chargeStrategy;

		protected override void SetUp()
		{
			base.SetUp();
			ChargeStrategy.Setup();
		}

		#endregion
	}
}
