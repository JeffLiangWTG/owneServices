using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class QuotationRunDocsForwardingStandardPricingPageFreightTest : QuotationRunDocForwardingStandardPricingPageChargeBaseTest
	{
		#region Global Sell Rates Override Local

		//TODO: 23 should be 103 (ClientRate Global prioritized of CompanyTariff Local)
		protected override string TestGlobalSellRatesOverrideLocal_QuotationAndClientRateAndCompanyTariffHaveSameCharges_Disable_ExpectedResult =>
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]

{C}-[Currency Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[202.00]

{C}-[War Risk Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[23.00]

{C}-[Fuel Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[24.00]

{C}-[FRT5 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[15.00]





{C}-[END OF DOCUMENT]";

		//TODO: 202 should be 102 (ClientRate Global prioritized over ClientRate Local), 23 should be 103 (ClientRate Global should be prioritized over CompanyTariff Local), 24 should be 14 (ClientRate Global should be prioritized over CompanyTariff Local)
		protected override string TestGlobalSellRatesOverrideLocal_QuotationAndClientRateAndCompanyTariffHaveSameCharges_Enable_ExpectedResult =>
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]

{C}-[Currency Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[202.00]

{C}-[War Risk Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[23.00]

{C}-[Fuel Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[24.00]

{C}-[FRT5 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[15.00]





{C}-[END OF DOCUMENT]";

		//TODO: 23 should 103 (ClientRate Global should be prioritized over CompanyTariff Local)
		protected override string TestGlobalSellRatesOverrideLocal_QuoteWithFRT_DisableExpectedResult =>
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[FRT100 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[201.00]

{C}-[Currency Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[202.00]

{C}-[War Risk Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[23.00]

{C}-[Fuel Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[24.00]

{C}-[FRT5 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[15.00]





{C}-[END OF DOCUMENT]";

		//TODO: 201 should be 101 and 202 should be 102 (ClientRate Global should be prioritized over ClientRate Local), 23 should be 103 (ClientRate Global should be prioritized over CompanyTariff Local), 24 should be 14 (CompanyTariff Global shoudl be prioritized over CompanyTariff Local)
		protected override string TestGlobalSellRatesOverrideLocal_QuoteWithFRT_EnableExpectedResult =>
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[FRT100 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[201.00]

{C}-[Currency Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[202.00]

{C}-[War Risk Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[23.00]

{C}-[Fuel Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[24.00]

{C}-[FRT5 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[15.00]





{C}-[END OF DOCUMENT]";

		protected override string TestGlobalSellRatesOverrideLocal_QuoteWithORG_DisableExpectedResult =>
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Origin Charges from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Carrier:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Origin Documentation Fee]
{E}-[20GP]   {AD}-[AUD]   {AH}-[1,001.00]






{C}-[END OF DOCUMENT]";

		protected override string TestGlobalSellRatesOverrideLocal_QuoteWithORG_EnableExpectedResult =>
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Origin Charges from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Carrier:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Origin Documentation Fee]
{E}-[20GP]   {AD}-[AUD]   {AH}-[1,001.00]






{C}-[END OF DOCUMENT]";

		#endregion

		protected override string TransitTimeExpectedResult =>
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 4]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[1 Day]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]

{C}-[International Freight]
{E}-[20GP]   {AD}-[USD]   {AH}-[100.00]






{C}-[Continued Over…]

{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 2 of 4]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[2 Days]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Currency Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,002.00]

{C}-[International Freight]
{E}-[20GP]   {AD}-[USD]   {AH}-[100.00]






{C}-[Continued Over…]

{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 3 of 4]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[3 Days]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[War Risk Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,003.00]

{C}-[International Freight]
{E}-[20GP]   {AD}-[USD]   {AH}-[100.00]






{C}-[Continued Over…]

{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 4 of 4]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[4 Days]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Fuel Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,004.00]

{C}-[International Freight]
{E}-[20GP]   {AD}-[USD]   {AH}-[100.00]






{C}-[END OF DOCUMENT]";

		protected override string ContractNumberExpectedResult =>
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]

{C}-[Currency Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,002.00]

{C}-[War Risk Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,003.00]

{C}-[Fuel Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,004.00]

{C}-[International Freight]
{E}-[20GP]   {AD}-[USD]   {AH}-[100.00]

{C}-[FRT5 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[105.00]

{C}-[FRT6 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[16.00]





{C}-[END OF DOCUMENT]";

		protected override string MatchContainerRateClassExpectedResult =>
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]

{C}-[Currency Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[12.00]






{C}-[END OF DOCUMENT]";

		//TODO: duplication
		// 3 pages for each quote rates and in each page, it has the same exact rates showing quotation, clientRate and companyTariff
		protected override string FrequencyExpectedResult =>
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 4]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Every Day]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]

{C}-[Currency Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,002.00]

{C}-[War Risk Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,003.00]

{C}-[Fuel Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,004.00]

{C}-[International Freight]
{E}-[20GP]   {AD}-[USD]   {AH}-[100.00]

{C}-[FRT5 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[105.00]

{C}-[FRT6 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[16.00]





{C}-[Continued Over…]

{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 2 of 4]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Every 2 Days]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]

{C}-[Currency Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,002.00]

{C}-[War Risk Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,003.00]

{C}-[Fuel Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,004.00]

{C}-[International Freight]
{E}-[20GP]   {AD}-[USD]   {AH}-[100.00]

{C}-[FRT5 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[105.00]

{C}-[FRT6 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[16.00]






{C}-[Continued Over…]

{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 3 of 4]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Every 3 Days]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]

{C}-[Currency Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,002.00]

{C}-[War Risk Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,003.00]

{C}-[Fuel Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,004.00]

{C}-[International Freight]
{E}-[20GP]   {AD}-[USD]   {AH}-[100.00]

{C}-[FRT5 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[105.00]

{C}-[FRT6 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[16.00]






{C}-[Continued Over…]

{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 4 of 4]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Every 4 Days]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]

{C}-[Currency Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,002.00]

{C}-[War Risk Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,003.00]

{C}-[Fuel Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,004.00]

{C}-[International Freight]
{E}-[20GP]   {AD}-[USD]   {AH}-[100.00]

{C}-[FRT5 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[105.00]

{C}-[FRT6 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[16.00]






{C}-[END OF DOCUMENT]";

		#region Implementation

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
