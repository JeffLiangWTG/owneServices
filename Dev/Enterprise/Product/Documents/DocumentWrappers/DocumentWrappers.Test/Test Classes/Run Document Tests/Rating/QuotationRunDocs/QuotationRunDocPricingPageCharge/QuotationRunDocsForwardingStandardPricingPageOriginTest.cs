using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class QuotationRunDocsForwardingStandardPricingPageOriginTest : QuotationRunDocForwardingStandardPricingPageChargeBaseTest
	{
		#region Global Sell Rates Override Local

		protected override string TestGlobalSellRatesOverrideLocal_QuotationAndClientRateAndCompanyTariffHaveSameCharges_Disable_ExpectedResult =>
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Origin Charges from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Carrier:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[ORG1 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[1,001.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[202.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[103.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[24.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[15.00]





{C}-[END OF DOCUMENT]";

		protected override string TestGlobalSellRatesOverrideLocal_QuotationAndClientRateAndCompanyTariffHaveSameCharges_Enable_ExpectedResult =>
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Origin Charges from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Carrier:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[ORG1 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[1,001.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[102.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[103.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[14.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[15.00]





{C}-[END OF DOCUMENT]";

		protected override string TestGlobalSellRatesOverrideLocal_QuoteWithFRT_DisableExpectedResult =>
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[ORG1 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[201.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[202.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[103.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[24.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[15.00]



{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[FRT100 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]





{C}-[END OF DOCUMENT]";

		protected override string TestGlobalSellRatesOverrideLocal_QuoteWithFRT_EnableExpectedResult =>
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[ORG1 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[101.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[102.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[103.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[14.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[15.00]



{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[FRT100 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]





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

{C}-[ORG1 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[201.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[202.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[103.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[24.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[15.00]





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

{C}-[ORG1 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[101.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[102.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[103.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[14.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[15.00]





{C}-[END OF DOCUMENT]";

		#endregion

		//TODO: duplication
		protected override string TransitTimeExpectedResult =>
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 4]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[1 Day]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[ORG0 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[100.00]



{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]






{C}-[Continued Over…]

{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 2 of 4]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[2 Days]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[ORG0 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[100.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[102.00]



{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Currency Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,002.00]






{C}-[Continued Over…]

{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 3 of 4]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[3 Days]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[ORG0 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[100.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[13.00]



{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[War Risk Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,003.00]






{C}-[Continued Over…]

{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 4 of 4]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[4 Days]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[ORG0 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[100.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[104.00]



{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Fuel Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,004.00]






{C}-[END OF DOCUMENT]";

		protected override string ContractNumberExpectedResult =>
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[ORG0 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[100.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[102.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[104.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[105.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[13.00]

{C}-[ORG6 Global Charge]
{E}-[20GP]   {AD}-[AUD]   {AH}-[16.00]



{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]

{C}-[Currency Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,002.00]

{C}-[War Risk Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,003.00]

{C}-[Fuel Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,004.00]





{C}-[END OF DOCUMENT]";

		protected override string MatchContainerRateClassExpectedResult =>
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[ORG1 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[101.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[12.00]



{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]






{C}-[END OF DOCUMENT]";

		//TODO: duplication
		//3 pages for each quote rates and in each page, it has the same exact rates showing quotation, clientRate and companyTariff
		protected override string FrequencyExpectedResult =>
@"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 4]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Every Day]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[ORG0 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[100.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[102.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[104.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[105.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[13.00]

{C}-[ORG6 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[16.00]



{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]

{C}-[Currency Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,002.00]

{C}-[War Risk Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,003.00]

{C}-[Fuel Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,004.00]





{C}-[Continued Over…]

{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 2 of 4]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Every 2 Days]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[ORG0 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[100.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[102.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[104.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[105.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[13.00]

{C}-[ORG6 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[16.00]



{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]

{C}-[Currency Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,002.00]

{C}-[War Risk Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,003.00]

{C}-[Fuel Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,004.00]





{C}-[Continued Over…]

{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 3 of 4]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Every 3 Days]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[ORG0 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[100.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[102.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[104.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[105.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[13.00]

{C}-[ORG6 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[16.00]



{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]

{C}-[Currency Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,002.00]

{C}-[War Risk Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,003.00]

{C}-[Fuel Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,004.00]





{C}-[Continued Over…]

{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 4 of 4]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Every 4 Days]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[ORG0 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[100.00]

{C}-[ORG2 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[102.00]

{C}-[ORG4 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[104.00]

{C}-[ORG5 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[105.00]

{C}-[ORG3 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[13.00]

{C}-[ORG6 Global Charge]
{E}-[20GP]   {AD}-[USD]   {AH}-[16.00]



{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]

{C}-[Currency Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,002.00]

{C}-[War Risk Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,003.00]

{C}-[Fuel Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,004.00]





{C}-[END OF DOCUMENT]";

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
