using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class QuotationRunDocsForwardingLandscapePricingPageFreightTest : QuotationRunDocForwardingLandscapePricingPageChargeBaseTest
	{
		#region Global Sell Rates Override Local

		//TODO: 23 should be 103 (ClientRate Global should be prioritized over CompanyTariff Local)
		protected override string TestGlobalSellRatesOverrideLocal_QuotationAndClientRateAndCompanyTariffHaveSameCharges_Disable_ExpectedResult =>
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
{S}-[Currency Adjustment Factor]
{U}-[20GP]   {AU}-[USD]   {BC}-[202.00]
{S}-[War Risk Surcharge]
{U}-[20GP]   {AU}-[USD]   {BC}-[23.00]
{S}-[Fuel Surcharge]
{U}-[20GP]   {AU}-[USD]   {BC}-[24.00]
{S}-[FRT5 Global Charge]
{U}-[20GP]   {AU}-[USD]   {BC}-[15.00]


{C}-[END OF DOCUMENT ]";

		//TODO: 202 should be 102 (ClientRate Global should be prioritized over ClientRate Local), 23 should be 103 (ClientRate Global should be prioritized over CompanyTariff Local), 24 should be 14 (CompanyTariff Global should be prioritized over CompanyTariff Local)
		protected override string TestGlobalSellRatesOverrideLocal_QuotationAndClientRateAndCompanyTariffHaveSameCharges_Enable_ExpectedResult =>
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
{S}-[Currency Adjustment Factor]
{U}-[20GP]   {AU}-[USD]   {BC}-[202.00]
{S}-[War Risk Surcharge]
{U}-[20GP]   {AU}-[USD]   {BC}-[23.00]
{S}-[Fuel Surcharge]
{U}-[20GP]   {AU}-[USD]   {BC}-[24.00]
{S}-[FRT5 Global Charge]
{U}-[20GP]   {AU}-[USD]   {BC}-[15.00]


{C}-[END OF DOCUMENT ]";

		//TODO: 23 should be 103 (ClientRate Global should be prioritized over CompanyTariff Local)
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
{S}-[Bunker Adjustment Factor]
{U}-[20GP]   {AU}-[USD]   {BC}-[201.00]
{S}-[Currency Adjustment Factor]
{U}-[20GP]   {AU}-[USD]   {BC}-[202.00]
{S}-[War Risk Surcharge]
{U}-[20GP]   {AU}-[USD]   {BC}-[23.00]
{S}-[Fuel Surcharge]
{U}-[20GP]   {AU}-[USD]   {BC}-[24.00]
{S}-[FRT5 Global Charge]
{U}-[20GP]   {AU}-[USD]   {BC}-[15.00]


{C}-[END OF DOCUMENT ]";

		//TODO: 201 and 202 should be 101 and 102 (ClientRate Global should be prioritized over ClientRate Local), 23 should be 103 (ClientRate Global should be prioritized over CompanyTariff Local), 24 should be 14 (CompanyTariff Global should be prioritized over CompanyTariff Local)
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
{S}-[Bunker Adjustment Factor]
{U}-[20GP]   {AU}-[USD]   {BC}-[201.00]
{S}-[Currency Adjustment Factor]
{U}-[20GP]   {AU}-[USD]   {BC}-[202.00]
{S}-[War Risk Surcharge]
{U}-[20GP]   {AU}-[USD]   {BC}-[23.00]
{S}-[Fuel Surcharge]
{U}-[20GP]   {AU}-[USD]   {BC}-[24.00]
{S}-[FRT5 Global Charge]
{U}-[20GP]   {AU}-[USD]   {BC}-[15.00]


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


{C}-[END OF DOCUMENT ]";

		protected override string ContractNumberExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


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


{C}-[END OF DOCUMENT ]";

		protected override string FrequencyExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


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


{C}-[END OF DOCUMENT ]";

		#region Override

		protected override string RateCategory => RatingConstants.RateCategory.FCL;

		protected override string RateMode => Core.Constants.RateMode.SEA;

		protected override IChargeStrategyForTest ChargeStrategy => chargeStrategy ?? (chargeStrategy = new FreightChargeStrategyForTest(TestHelper, Factory));
		IChargeStrategyForTest chargeStrategy;

		protected override void SetUp()
		{
			base.SetUp();
			ChargeStrategy.Setup();
		}

		#endregion
	}
}
