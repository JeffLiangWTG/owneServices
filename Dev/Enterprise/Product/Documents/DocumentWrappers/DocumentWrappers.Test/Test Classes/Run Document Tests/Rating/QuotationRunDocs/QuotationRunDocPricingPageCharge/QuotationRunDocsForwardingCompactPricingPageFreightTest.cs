using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class QuotationRunDocsForwardingCompactPricingPageFreightTest : QuotationRunDocForwardingCompactPricingPageChargeBaseTest
	{
		#region Global Sell Rates Override Local

		//TODO: 23 should be 103 (ClientRate Global should be prioritized CompayTariff Local)
		protected override string TestGlobalSellRatesOverrideLocal_QuotationAndClientRateAndCompanyTariffHaveSameCharges_Disable_ExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Shipping Line:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[BAF|>(USD)]   {AC}-[CAF|>(USD)]   {AG}-[WAR|>(USD)]   {AK}-[FSC|>(USD)]   {AO}-[FRT5|>(USD)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[1,001.00]   {AC}-[202.00]   {AG}-[23.00]   {AK}-[24.00]   {AO}-[15.00]


{C}-[END OF DOCUMENT ]";

		//TODO: 202 should be 102 (ClientRate Global should be prioritized over ClientRate Local), 23 should be 103 (ClientRate Global should be prioritized over CompanyTariff Local), 24 should be 14 (CompanyTariff Global Should be prioritized over CompanyTariff Local)
		protected override string TestGlobalSellRatesOverrideLocal_QuotationAndClientRateAndCompanyTariffHaveSameCharges_Enable_ExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Shipping Line:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[BAF|>(USD)]   {AC}-[CAF|>(USD)]   {AG}-[WAR|>(USD)]   {AK}-[FSC|>(USD)]   {AO}-[FRT5|>(USD)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[1,001.00]   {AC}-[202.00]   {AG}-[23.00]   {AK}-[24.00]   {AO}-[15.00]


{C}-[END OF DOCUMENT ]";

		//TODO: 23 should be 103 (ClientRate Global should be prioritized over companyTariff local)
		protected override string TestGlobalSellRatesOverrideLocal_QuoteWithFRT_DisableExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Shipping Line:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[FRT100|>(USD)]   {AC}-[BAF|>(USD)]   {AG}-[CAF|>(USD)]   {AK}-[WAR|>(USD)]   {AO}-[FSC|>(USD)]   {AS}-[FRT5|>(USD)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[1,001.00]   {AC}-[201.00]   {AG}-[202.00]   {AK}-[23.00]   {AO}-[24.00]   {AS}-[15.00]


{C}-[END OF DOCUMENT ]";

		//TODO: 201 and 202 should be 101 and 102 (ClientRate Global should be prioritized over ClientRate Local), 23 should be 103 (ClientRate Global should be prioritized over CompanyTariff Local), 24 should be 14 (CompanyTariff Global should be prioritized over CompanyTariff Local)
		protected override string TestGlobalSellRatesOverrideLocal_QuoteWithFRT_EnableExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Shipping Line:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[FRT100|>(USD)]   {AC}-[BAF|>(USD)]   {AG}-[CAF|>(USD)]   {AK}-[WAR|>(USD)]   {AO}-[FSC|>(USD)]   {AS}-[FRT5|>(USD)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[1,001.00]   {AC}-[201.00]   {AG}-[202.00]   {AK}-[23.00]   {AO}-[24.00]   {AS}-[15.00]


{C}-[END OF DOCUMENT ]";

		protected override string TestGlobalSellRatesOverrideLocal_QuoteWithORG_DisableExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Carrier:]   {AS}-[Not Specified]




{C}-[Origin Rates]

{C}-[Origin]   {I}-[Destination]   {U}-[Container Type]   {Y}-[ODOC|>(AUD)]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[1,001.00]


{C}-[END OF DOCUMENT ]";

		protected override string TestGlobalSellRatesOverrideLocal_QuoteWithORG_EnableExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Carrier:]   {AS}-[Not Specified]




{C}-[Origin Rates]

{C}-[Origin]   {I}-[Destination]   {U}-[Container Type]   {Y}-[ODOC|>(AUD)]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[1,001.00]


{C}-[END OF DOCUMENT ]";

		#endregion

		protected override string TransitTimeExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Shipping Line:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[BAF|>(USD)]   {AC}-[CAF|>(USD)]   {AG}-[WAR|>(USD)]   {AK}-[FSC|>(USD)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[1,001.00]   {BO}-[1 Day]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {AC}-[1,002.00]   {BO}-[2 Days]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {AG}-[1,003.00]   {BO}-[3 Days]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {AK}-[1,004.00]   {BO}-[4 Days]


{C}-[END OF DOCUMENT ]";

		protected override string ContractNumberExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Shipping Line:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[BAF|>(USD)]   {AC}-[CAF|>(USD)]   {AG}-[WAR|>(USD)]   {AK}-[FSC|>(USD)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[1,001.00]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {AC}-[1,002.00]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {AG}-[1,003.00]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {AK}-[1,004.00]


{C}-[END OF DOCUMENT ]";

		protected override string MatchContainerRateClassExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Shipping Line:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[BAF|>(USD)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[1,001.00]


{C}-[END OF DOCUMENT ]";

		protected override string FrequencyExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Shipping Line:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[BAF|>(USD)]   {AC}-[CAF|>(USD)]   {AG}-[WAR|>(USD)]   {AK}-[FSC|>(USD)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[1,001.00]   {BI}-[Every Day]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {AC}-[1,002.00]   {BI}-[Every 2 Days]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {AG}-[1,003.00]   {BI}-[Every 3 Days]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {AK}-[1,004.00]   {BI}-[Every 4 Days]


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
