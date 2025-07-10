namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class QuotationRunDocForwardingCompactPricingPageTest : QuotationRunDocPricingPageBaseTest
	{
		protected override string Test_CurrencyENUSExpectedResult => @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 2]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Airline:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[FRT|>(AUD)]   {AC}-[BAF|>(AUD)]   {AG}-[WAR|>(AUD/KG)]   {AK}-[CAF|>(AUD/KG)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[10.00]   {AC}-[11.00]   {AG}-[12.00]   {AK}-[See Below]
{U}-[Air]

{U}-[Currency Adjustment Factor]
{W}-[Base Rate]   {AU}-[AUD]   {BC}-[13.00]
{W}-[First Package]   {AU}-[AUD]   {BC}-[14.00]
{W}-[Additional Packages]   {AU}-[AUD]   {BC}-[15.00]   {BI}-[per Package]


{C}-[Continued Over… ]
{AK}-[Quotation for Test Client #2]   {BL}-[Page 2 of 2]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Airline:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[FRT|>(AUD)]   {AC}-[BAF|>(AUD)]   {AG}-[WAR|>(AUD/KG)]   {AK}-[CAF|>(AUD/KG)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[20.00]   {AC}-[21.00]   {AG}-[22.00]   {AK}-[See Below]
{U}-[Air]

{U}-[Currency Adjustment Factor]
{W}-[LD-7]
{Y}-[Base Rate]   {AU}-[AUD]   {BC}-[23.00]
{Y}-[First Package]   {AU}-[AUD]   {BC}-[24.00]
{Y}-[Additional Packages]   {AU}-[AUD]   {BC}-[25.00]   {BI}-[per Package]


{C}-[END OF DOCUMENT ]";

		protected override string Test_CurrencyITITExpectedResult => @"{AK}-[Quotazione per Test Client #2]   {BL}-[Pagina 1 di 2]


{AK}-[Tariffe di spedizione]

{AK}-[Preventivo N°:]   {AS}-[998 - TESTORG2]
{AK}-[Validità:]   {AS}-[01-gen-23 - 01-feb-23]
{AK}-[Livello servizio:]   {AS}-[Non specificato]
{AK}-[Merce:]   {AS}-[Generale]
{AK}-[Compagnia aerea:]   {AS}-[Non specificato]




{C}-[Tariffe Nolo]

{C}-[Origine]   {I}-[Destinazione]   {O}-[Porto trasbordo]   {U}-[Tipo container]   {Y}-[FRT|>(AUD)]   {AC}-[BAF|>(AUD)]   {AG}-[WAR|>(AUD/KG)]   {AK}-[CAF|>(AUD/KG)]   {BI}-[Frequenza]   {BO}-[Tempo di transito]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[10.00]   {AC}-[11.00]   {AG}-[12.00]   {AK}-[See Below]
{U}-[Aereo]

{U}-[Fattore rettifica valuta]
{W}-[Base Rate]   {AU}-[AUD]   {BC}-[13.00]
{W}-[First Package]   {AU}-[AUD]   {BC}-[14.00]
{W}-[Additional Packages]   {AU}-[AUD]   {BC}-[15.00]   {BI}-[per collo]


{C}-[Segue… ]
{AK}-[Quotazione per Test Client #2]   {BL}-[Pagina 2 di 2]


{AK}-[Tariffe di spedizione]

{AK}-[Preventivo N°:]   {AS}-[998 - TESTORG2]
{AK}-[Validità:]   {AS}-[01-gen-23 - 01-feb-23]
{AK}-[Livello servizio:]   {AS}-[Non specificato]
{AK}-[Merce:]   {AS}-[Generale]
{AK}-[Compagnia aerea:]   {AS}-[Non specificato]




{C}-[Tariffe Nolo]

{C}-[Origine]   {I}-[Destinazione]   {O}-[Porto trasbordo]   {U}-[Tipo container]   {Y}-[FRT|>(AUD)]   {AC}-[BAF|>(AUD)]   {AG}-[WAR|>(AUD/KG)]   {AK}-[CAF|>(AUD/KG)]   {BI}-[Frequenza]   {BO}-[Tempo di transito]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[20.00]   {AC}-[21.00]   {AG}-[22.00]   {AK}-[See Below]
{U}-[Aereo]

{U}-[Fattore rettifica valuta]
{W}-[LD-7]
{Y}-[Base Rate]   {AU}-[AUD]   {BC}-[23.00]
{Y}-[First Package]   {AU}-[AUD]   {BC}-[24.00]
{Y}-[Additional Packages]   {AU}-[AUD]   {BC}-[25.00]   {BI}-[per collo]


{C}-[FINE DEL DOCUMENTO ]";

		//TODO: ClientRate should be shown because it is prioritized over CompanyTariff but CompanyTariff is also shown.
		//This is a known issue in Compact where Quotation has Freight Charge but ClientRate and CompanyTariff has Origin/Destination Charges.
		protected override string TestContractNumber_QuotationWithFreightChargeAndClientRateAndCompanyTariffWithOriginChargeExpectedResult => @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Shipping Line:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[FRT|>(USD)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[1,000.00]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[1,001.00]

{C}-[Origin Rates]

{C}-[Origin]   {I}-[Destination]   {U}-[Container Type]   {Y}-[ODOC|>(AUD)]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[1.00]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[10.00]


{C}-[END OF DOCUMENT ]";

		protected override string TestFrequency_QuotationWithFreightChargeAndCompanyTariffWithOriginChargesExpectedResult => @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Shipping Line:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[FRT|>(USD)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[1,000.00]   {BI}-[Every Day]

{C}-[Origin Rates]

{C}-[Origin]   {I}-[Destination]   {U}-[Container Type]   {Y}-[ODOC|>(AUD)]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[10.00]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[11.00]   {BI}-[Every Day]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[12.00]   {BI}-[Every 2 Days]


{C}-[END OF DOCUMENT ]";

		protected override string TestFrequency_FreightChargesExpectedResult => @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Shipping Line:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[FRT|>(USD)]   {AC}-[BAF|>(USD)]   {AG}-[CAF|>(USD)]   {AK}-[WAR|>(USD)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Chicago]   {I}-[Auckland]   {U}-[20GP]   {AG}-[3,000.00]   {BI}-[Every Day]
{C}-[Jakarta, Java]   {I}-[Singapore]   {U}-[20GP]   {AC}-[2,000.00]
{C}-[Shanghai]   {I}-[Brisbane]   {U}-[20GP]   {AK}-[4,000.00]   {BI}-[Every Day]
{C}-[Hongqiao]
{C}-[International]
{C}-[Apt]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[1,000.00]


{C}-[END OF DOCUMENT ]";

		// TODO: Freight charges should show one row for each 20GP, 40GP and 40HC but EntriesLoader.GetRelatedRateEntries > BuildLocationFilter expand AUSYD location to also include AU that would bring the other charges
		protected override string TestDocument_DocBuilder_ClientRateWithSimilarOriginAndDestinationExpectedResult => @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-20 - 01-Feb-20]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Shipping Line:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[FRT|>(USD)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Australia]   {I}-[Indonesia]   {U}-[20GP]   {Y}-[1,001.00]
{C}-[Sydney]   {I}-[Indonesia]   {U}-[20GP]   {Y}-[1,001.00]
{U}-[40GP]   {Y}-[1,002.00]
{C}-[Sydney]   {I}-[Jakarta, Java]   {U}-[20GP]   {Y}-[1,001.00]
{U}-[40GP]   {Y}-[1,002.00]
{U}-[40HC]   {Y}-[1,003.00]

{C}-[Origin Rates]

{C}-[Origin]   {I}-[Destination]   {U}-[Container Type]   {Y}-[ODOC|>(AUD)]
{C}-[Australia]   {I}-[Indonesia]   {Y}-[2,001.00]
{C}-[Australia]   {I}-[Indonesia]   {Y}-[201.00]


{C}-[END OF DOCUMENT ]";

		protected override string MenuName => "Forwarding Compact Pricing Page";
	}
}
