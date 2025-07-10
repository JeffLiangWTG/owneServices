using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class QuotationRunDocsForwardingCompactPricingPageOriginTest : QuotationRunDocForwardingCompactPricingPageChargeBaseTest
	{
		#region Global Sell Rates Override Local

		//TODO: duplication
		protected override string TestGlobalSellRatesOverrideLocal_QuotationAndClientRateAndCompanyTariffHaveSameCharges_Disable_ExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Carrier:]   {AS}-[Not Specified]




{C}-[Origin Rates]

{C}-[Origin]   {I}-[Destination]   {U}-[Container Type]   {Y}-[ORG1|>(AUD)]   {AC}-[ORG2|>(AUD)]   {AG}-[ORG3|>(AUD)]   {AK}-[ORG4|>(AUD)]   {AO}-[ORG5|>(AUD)]   {AS}-[ORG3|>(AUD)]   {AW}-[ORG1|>(AUD)]   {BA}-[ORG2|>(AUD)]   {BE}-[ORG4|>(AUD)]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[1,001.00]   {AC}-[202.00]   {AG}-[103.00]   {AK}-[24.00]   {AO}-[15.00]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[201.00]   {AC}-[202.00]   {AK}-[24.00]   {AO}-[15.00]   {AS}-[23.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AG}-[103.00]   {AO}-[15.00]   {AW}-[101.00]   {BA}-[102.00]   {BE}-[14.00]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[21.00]   {AC}-[22.00]   {AK}-[24.00]   {AS}-[23.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AG}-[13.00]   {AO}-[15.00]   {AW}-[11.00]   {BA}-[12.00]   {BE}-[14.00]


{C}-[END OF DOCUMENT ]";

		//TODO: duplication
		protected override string TestGlobalSellRatesOverrideLocal_QuotationAndClientRateAndCompanyTariffHaveSameCharges_Enable_ExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Carrier:]   {AS}-[Not Specified]




{C}-[Origin Rates]

{C}-[Origin]   {I}-[Destination]   {U}-[Container Type]   {Y}-[ORG1|>(AUD)]   {AC}-[ORG2|>(AUD)]   {AG}-[ORG3|>(AUD)]   {AK}-[ORG4|>(AUD)]   {AO}-[ORG5|>(AUD)]   {AS}-[ORG2|>(AUD)]   {AW}-[ORG1|>(AUD)]   {BA}-[ORG3|>(AUD)]   {BE}-[ORG4|>(AUD)]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[1,001.00]   {AC}-[102.00]   {AG}-[103.00]   {AK}-[14.00]   {AO}-[15.00]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[201.00]   {AG}-[13.00]   {AK}-[14.00]   {AO}-[15.00]   {AS}-[202.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AC}-[102.00]   {AG}-[103.00]   {AK}-[14.00]   {AO}-[15.00]   {AW}-[101.00]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[21.00]   {AS}-[22.00]   {BA}-[23.00]   {BE}-[24.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AC}-[12.00]   {AG}-[13.00]   {AK}-[14.00]   {AO}-[15.00]   {AW}-[11.00]


{C}-[END OF DOCUMENT ]";

		//TODO: duplication
		protected override string TestGlobalSellRatesOverrideLocal_QuoteWithFRT_DisableExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Shipping Line:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[FRT100|>(USD)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[1,001.00]

{C}-[Origin Rates]

{C}-[Origin]   {I}-[Destination]   {U}-[Container Type]   {Y}-[ORG1|>(AUD)]   {AC}-[ORG2|>(AUD)]   {AG}-[ORG3|>(AUD)]   {AK}-[ORG4|>(AUD)]   {AO}-[ORG5|>(AUD)]   {AS}-[ORG1|>(AUD)]   {AW}-[ORG2|>(AUD)]   {BA}-[ORG3|>(AUD)]   {BE}-[ORG4|>(AUD)]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[201.00]   {AC}-[202.00]   {AG}-[23.00]   {AK}-[24.00]   {AO}-[15.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AO}-[15.00]   {AS}-[101.00]   {AW}-[102.00]   {BA}-[103.00]   {BE}-[14.00]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[21.00]   {AC}-[22.00]   {AG}-[23.00]   {AK}-[24.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AO}-[15.00]   {AS}-[11.00]   {AW}-[12.00]   {BA}-[13.00]   {BE}-[14.00]


{C}-[END OF DOCUMENT ]";

		//TODO: duplication
		protected override string TestGlobalSellRatesOverrideLocal_QuoteWithFRT_EnableExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Shipping Line:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[FRT100|>(USD)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[1,001.00]

{C}-[Origin Rates]

{C}-[Origin]   {I}-[Destination]   {U}-[Container Type]   {Y}-[ORG1|>(AUD)]   {AC}-[ORG2|>(AUD)]   {AG}-[ORG3|>(AUD)]   {AK}-[ORG4|>(AUD)]   {AO}-[ORG5|>(AUD)]   {AS}-[ORG1|>(AUD)]   {AW}-[ORG2|>(AUD)]   {BA}-[ORG3|>(AUD)]   {BE}-[ORG4|>(AUD)]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[201.00]   {AC}-[202.00]   {AG}-[13.00]   {AK}-[14.00]   {AO}-[15.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AG}-[103.00]   {AK}-[14.00]   {AO}-[15.00]   {AS}-[101.00]   {AW}-[102.00]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[21.00]   {AC}-[22.00]   {BA}-[23.00]   {BE}-[24.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AG}-[13.00]   {AK}-[14.00]   {AO}-[15.00]   {AS}-[11.00]   {AW}-[12.00]


{C}-[END OF DOCUMENT ]";

		//TODO: duplication
		protected override string TestGlobalSellRatesOverrideLocal_QuoteWithORG_DisableExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Carrier:]   {AS}-[Not Specified]




{C}-[Origin Rates]

{C}-[Origin]   {I}-[Destination]   {U}-[Container Type]   {Y}-[ODOC|>(AUD)]   {AC}-[ORG1|>(AUD)]   {AG}-[ORG2|>(AUD)]   {AK}-[ORG3|>(AUD)]   {AO}-[ORG4|>(AUD)]   {AS}-[ORG5|>(AUD)]   {AW}-[ORG3|>(AUD)]   {BA}-[ORG1|>(AUD)]   {BE}-[ORG2|>(AUD)]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[1,001.00]   {AC}-[201.00]   {AG}-[202.00]   {AK}-[103.00]   {AO}-[24.00]   {AS}-[15.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AC}-[201.00]   {AG}-[202.00]   {AO}-[24.00]   {AS}-[15.00]   {AW}-[23.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AK}-[103.00]   {AS}-[15.00]   {BA}-[101.00]   {BE}-[102.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AC}-[21.00]   {AG}-[22.00]   {AO}-[24.00]   {AW}-[23.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AK}-[13.00]   {AS}-[15.00]   {BA}-[11.00]   {BE}-[12.00]


{C}-[END OF DOCUMENT ]";

		//TODO: duplication
		protected override string TestGlobalSellRatesOverrideLocal_QuoteWithORG_EnableExpectedResult =>
@"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Carrier:]   {AS}-[Not Specified]




{C}-[Origin Rates]

{C}-[Origin]   {I}-[Destination]   {U}-[Container Type]   {Y}-[ODOC|>(AUD)]   {AC}-[ORG1|>(AUD)]   {AG}-[ORG2|>(AUD)]   {AK}-[ORG3|>(AUD)]   {AO}-[ORG4|>(AUD)]   {AS}-[ORG5|>(AUD)]   {AW}-[ORG1|>(AUD)]   {BA}-[ORG2|>(AUD)]   {BE}-[ORG3|>(AUD)]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[1,001.00]   {AC}-[101.00]   {AG}-[102.00]   {AK}-[103.00]   {AO}-[14.00]   {AS}-[15.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AK}-[13.00]   {AO}-[14.00]   {AS}-[15.00]   {AW}-[201.00]   {BA}-[202.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AC}-[101.00]   {AG}-[102.00]   {AK}-[103.00]   {AO}-[14.00]   {AS}-[15.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AW}-[21.00]   {BA}-[22.00]   {BE}-[23.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AC}-[11.00]   {AG}-[12.00]   {AK}-[13.00]   {AO}-[14.00]   {AS}-[15.00]


{C}-[END OF DOCUMENT ]";

		#endregion

		//TODO: ClientRate should be prioritized over CompanyTariff
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

{C}-[Origin Rates]

{C}-[Origin]   {I}-[Destination]   {U}-[Container Type]   {Y}-[ORG0|>(AUD)]   {AC}-[ORG2|>(AUD)]   {AG}-[ORG3|>(AUD)]   {AK}-[ORG4|>(AUD)]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[100.00]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[10.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AC}-[102.00]   {BO}-[2 Days]
{C}-[Sydney]   {I}-[Los Angeles]   {AG}-[13.00]   {BO}-[3 Days]
{C}-[Sydney]   {I}-[Los Angeles]   {AK}-[104.00]   {BO}-[4 Days]
{C}-[Sydney]   {I}-[Los Angeles]   {AK}-[14.00]   {BO}-[4 Days]


{C}-[END OF DOCUMENT ]";

		//TODO: ClientRate should be prioritized over CompanyTariff
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

{C}-[Origin Rates]

{C}-[Origin]   {I}-[Destination]   {U}-[Container Type]   {Y}-[ORG0|>(AUD)]   {AC}-[ORG2|>(AUD)]   {AG}-[ORG4|>(AUD)]   {AK}-[ORG5|>(AUD)]   {AO}-[ORG3|>(AUD)]   {AS}-[ORG6|>(AUD)]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[100.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AC}-[102.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AG}-[104.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AK}-[105.00]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[10.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AO}-[13.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AG}-[14.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AS}-[16.00]


{C}-[END OF DOCUMENT ]";

		//TODO: ClientRate should be prioritized over CompanyTariff
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

{C}-[Origin Rates]

{C}-[Origin]   {I}-[Destination]   {U}-[Container Type]   {Y}-[ORG1|>(USD)]   {AC}-[ORG2|>(USD)]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[101.00]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[11.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AC}-[12.00]


{C}-[END OF DOCUMENT ]";

		//TODO: ClientRate should be prioritized over CompanyTariff
		//The reason it is not is because for origin, get similar related-rates (exactMatch=false) and display them per row. For each of row, get exact related-rates (exactMatch=true) and show prioritized rates (clientRate over companyTariff)
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

{C}-[Origin Rates]

{C}-[Origin]   {I}-[Destination]   {U}-[Container Type]   {Y}-[ORG0|>(USD)]   {AC}-[ORG2|>(USD)]   {AG}-[ORG4|>(USD)]   {AK}-[ORG5|>(USD)]   {AO}-[ORG3|>(USD)]   {AS}-[ORG6|>(USD)]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[100.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AC}-[102.00]   {BI}-[Every 2 Days]
{C}-[Sydney]   {I}-[Los Angeles]   {AG}-[104.00]   {BI}-[Every 4 Days]
{C}-[Sydney]   {I}-[Los Angeles]   {AK}-[105.00]   {BI}-[Every 5 Days]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[10.00]
{C}-[Sydney]   {I}-[Los Angeles]   {AO}-[13.00]   {BI}-[Every 3 Days]
{C}-[Sydney]   {I}-[Los Angeles]   {AG}-[14.00]   {BI}-[Every 4 Days]
{C}-[Sydney]   {I}-[Los Angeles]   {AS}-[16.00]   {BI}-[Every 6 Days]


{C}-[END OF DOCUMENT ]";

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
