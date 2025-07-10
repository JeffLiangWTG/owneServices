namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class QuotationRunDocForwardingLandscapePricingPageTest : QuotationRunDocPricingPageBaseTest
	{
		protected override string Test_CurrencyENUSExpectedResult => @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 2]


{AK}-[Cross Trade Air Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Australia - United States]

{C}-[Origin - Destination]   {O}-[Airline]
{X}-[Cur.]   {AA}-[Flat]   {BC}-[Transit Time]   {BI}-[Freq.]   {BO}-[W/V Conv.]
{C}-[Sydney - Los Angeles]


{C}-[FRT - International Freight]   {X}-[AUD]   {AA}-[10.00]
{C}-[BAF - Bunker Adjustment Factor]   {X}-[AUD]   {AA}-[11.00]

{X}-[Cur.]   {AA}-[Per KG]   {BC}-[Transit Time]   {BI}-[Freq.]   {BO}-[W/V Conv.]



{C}-[WAR - War Risk Surcharge]   {X}-[AUD]   {AA}-[12.00]   {BO}-[6000 CC/KG]

{C}-[Currency Adjustment Factor]
{E}-[Base Rate]   {AE}-[AUD]   {AM}-[13.00]
{E}-[First Package]   {AE}-[AUD]   {AM}-[14.00]
{E}-[Additional Packages]   {AE}-[AUD]   {AM}-[15.00]   {AU}-[per Package]


{C}-[Continued Over… ]
{AK}-[Quotation for Test Client #2]   {BL}-[Page 2 of 2]


{AK}-[Cross Trade Air Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Australia - United States]

{C}-[Origin - Destination]   {K}-[Airline]   {S}-[Cur.]   {U}-[LD-7]   {AW}-[Transit Time]   {BA}-[Freq.]   {BG}-[W/V Conv.]   {BM}-[Contract No.]

{C}-[Sydney - Los Angeles]   {S}-[AUD]   {U}-[20.00 Flat]
{S}-[Air]
{S}-[Bunker Adjustment Factor]
{U}-[LD-7]   {AU}-[AUD]   {BC}-[21.00]
{S}-[War Risk Surcharge]
{U}-[LD-7]   {AU}-[AUD]   {BC}-[22.00]   {BI}-[per KG / 6000 CC]
{S}-[Currency Adjustment Factor]
{U}-[LD-7]
{W}-[Base Rate]   {AU}-[AUD]   {BC}-[23.00]
{W}-[First Package]   {AU}-[AUD]   {BC}-[24.00]
{W}-[Additional Packages]   {AU}-[AUD]   {BC}-[25.00]   {BI}-[per Package]


{C}-[END OF DOCUMENT ]";

		protected override string Test_CurrencyITITExpectedResult => @"{AK}-[Quotazione per Test Client #2]   {BL}-[Pagina 1 di 2]


{AK}-[Tariffe aeree Estero su estero  ]

{AK}-[Preventivo N°:]   {AS}-[998 - TESTORG2]
{AK}-[Validità:]   {AS}-[01-gen-23 - 01-feb-23]
{AK}-[Livello servizio:]   {AS}-[Non specificato]
{AK}-[Merce:]   {AS}-[Generale]




{C}-[Australia - Stati Uniti]

{C}-[Origine - Destinazione]   {O}-[Compagnia aerea]
{X}-[Val.]   {AA}-[Flat]   {BC}-[Tempo di transito]   {BI}-[Freq.]   {BO}-[Conv. P/V]
{C}-[Sydney - Los Angeles]


{C}-[FRT - International Freight]   {X}-[AUD]   {AA}-[10.00]
{C}-[BAF - Bunker Adjustment Factor]   {X}-[AUD]   {AA}-[11.00]

{X}-[Val.]   {AA}-[Per KG]   {BC}-[Tempo di transito]   {BI}-[Freq.]   {BO}-[Conv. P/V]



{C}-[WAR - War Risk Surcharge]   {X}-[AUD]   {AA}-[12.00]   {BO}-[6000 CC/KG]

{C}-[Fattore rettifica valuta]
{E}-[Base Rate]   {AE}-[AUD]   {AM}-[13.00]
{E}-[First Package]   {AE}-[AUD]   {AM}-[14.00]
{E}-[Additional Packages]   {AE}-[AUD]   {AM}-[15.00]   {AU}-[per collo]


{C}-[Segue… ]
{AK}-[Quotazione per Test Client #2]   {BL}-[Pagina 2 di 2]


{AK}-[Tariffe nolo Estero su estero Aereo  ]

{AK}-[Preventivo N°:]   {AS}-[998 - TESTORG2]
{AK}-[Validità:]   {AS}-[01-gen-23 - 01-feb-23]
{AK}-[Livello servizio:]   {AS}-[Non specificato]
{AK}-[Merce:]   {AS}-[Generale]




{C}-[Australia - Stati Uniti]

{C}-[Origine - Destinazione]   {K}-[Compagnia aerea]   {S}-[Val.]   {U}-[LD-7]   {AW}-[Tempo di transito]   {BA}-[Freq.]   {BG}-[Conv. P/V]   {BM}-[N° contratto]

{C}-[Sydney - Los Angeles]   {S}-[AUD]   {U}-[20.00 Flat]
{S}-[Aereo]
{S}-[BAF (fattore rettif. costo carbur.)]
{U}-[LD-7]   {AU}-[AUD]   {BC}-[21.00]
{S}-[Supplem. rischio bellico]
{U}-[LD-7]   {AU}-[AUD]   {BC}-[22.00]   {BI}-[per KG / 6000 CC]
{S}-[Fattore rettifica valuta]
{U}-[LD-7]
{W}-[Base Rate]   {AU}-[AUD]   {BC}-[23.00]
{W}-[First Package]   {AU}-[AUD]   {BC}-[24.00]
{W}-[Additional Packages]   {AU}-[AUD]   {BC}-[25.00]   {BI}-[per collo]


{C}-[FINE DEL DOCUMENTO ]";

		protected override string TestContractNumber_QuotationWithFreightChargeAndClientRateAndCompanyTariffWithOriginChargeExpectedResult => @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Cross Trade Sea Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Australia - United States]

{C}-[Origin - Destination]   {K}-[Shipping Line]   {S}-[Cur.]   {U}-[20GP]   {AW}-[Transit Time]   {BA}-[Freq.]   {BG}-[W/V Conv.]   {BM}-[Contract No.]

{C}-[Sydney - Los Angeles]   {S}-[USD]   {U}-[1,000.00]
{U}-[Flat]

{C}-[Sydney - Los Angeles]   {S}-[USD]   {U}-[1,001.00]   {BM}-[CONTRACT1]
{U}-[Flat]

{C}-[Origin Charges]

{C}-[Sydney (to Los Angeles)]
{C}-[FCL]

{C}-[Origin Documentation Fee]
{E}-[20GP]   {AE}-[AUD]   {AM}-[10.00]


{C}-[END OF DOCUMENT ]";

		protected override string TestFrequency_QuotationWithFreightChargeAndCompanyTariffWithOriginChargesExpectedResult => @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Cross Trade Sea Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Australia - United States]

{C}-[Origin - Destination]   {K}-[Shipping Line]   {S}-[Cur.]   {U}-[20GP]   {AW}-[Transit Time]   {BA}-[Freq.]   {BG}-[W/V Conv.]   {BM}-[Contract No.]

{C}-[Sydney - Los Angeles]   {S}-[USD]   {U}-[1,000.00]   {BA}-[Every Day]
{U}-[Flat]

{C}-[Origin Charges]

{C}-[Sydney (to Los Angeles)]
{C}-[FCL]

{C}-[Origin Documentation Fee]
{E}-[20GP]   {AE}-[AUD]   {AM}-[10.00]
{E}-[20GP]   {AE}-[AUD]   {AM}-[11.00]
{E}-[20GP]   {AE}-[AUD]   {AM}-[12.00]


{C}-[END OF DOCUMENT ]";

		protected override string TestFrequency_FreightChargesExpectedResult => @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Cross Trade Sea Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-23 - 01-Feb-23]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Australia - United States]

{C}-[Origin - Destination]   {K}-[Shipping Line]   {S}-[Cur.]   {U}-[20GP]   {AW}-[Transit Time]   {BA}-[Freq.]   {BG}-[W/V Conv.]   {BM}-[Contract No.]

{C}-[Sydney - Los Angeles]   {S}-[USD]   {U}-[1,000.00]
{U}-[Flat]

{C}-[China - Australia]

{C}-[Origin - Destination]   {K}-[Shipping Line]   {S}-[Cur.]   {U}-[20GP]   {AW}-[Transit Time]   {BA}-[Freq.]   {BG}-[W/V Conv.]   {BM}-[Contract No.]

{C}-[Shanghai Hongqiao]   {BA}-[Every Day]
{C}-[International Apt -]
{C}-[Brisbane]
{S}-[FCL]
{S}-[War Risk Surcharge]
{U}-[20GP]   {AU}-[USD]   {BC}-[4,000.00]

{C}-[Indonesia - Singapore]

{C}-[Origin - Destination]   {K}-[Shipping Line]   {S}-[Cur.]   {U}-[20GP]   {AW}-[Transit Time]   {BA}-[Freq.]   {BG}-[W/V Conv.]   {BM}-[Contract No.]

{C}-[Jakarta, Java -]
{C}-[Singapore]
{S}-[FCL]
{S}-[Bunker Adjustment Factor]
{U}-[20GP]   {AU}-[USD]   {BC}-[2,000.00]

{C}-[United States - New Zealand]

{C}-[Origin - Destination]   {K}-[Shipping Line]   {S}-[Cur.]   {U}-[20GP]   {AW}-[Transit Time]   {BA}-[Freq.]   {BG}-[W/V Conv.]   {BM}-[Contract No.]

{C}-[Chicago - Auckland]   {BA}-[Every Day]
{S}-[FCL]
{S}-[Currency Adjustment Factor]
{U}-[20GP]   {AU}-[USD]   {BC}-[3,000.00]


{C}-[END OF DOCUMENT ]";

		//TODO: Freight charges should show one row for each 20GP, 40GP and 40HC but EntriesLoader.GetRelatedRateEntries > BuildLocationFilter expand AUSYD location to also include AU that would bring the other charges
		protected override string TestDocument_DocBuilder_ClientRateWithSimilarOriginAndDestinationExpectedResult => @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Cross Trade Sea Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-20 - 01-Feb-20]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Australia - Indonesia]

{C}-[Origin - Destination]   {K}-[Shipping Line]   {S}-[Cur.]   {U}-[20GP]   {Y}-[40GP]   {AC}-[40HC]   {AW}-[Transit Time]   {BA}-[Freq.]   {BG}-[W/V Conv.]   {BM}-[Contract No.]

{C}-[Australia - Indonesia]   {S}-[USD]   {U}-[1,001.00]
{U}-[Flat]

{C}-[Sydney - Indonesia]   {S}-[USD]   {U}-[1,001.00]   {Y}-[1,002.00]
{U}-[Flat]   {Y}-[Flat]

{C}-[Sydney - Jakarta,]   {S}-[USD]   {U}-[1,001.00]   {Y}-[1,002.00]   {AC}-[1,003.00]
{C}-[Java]   {U}-[Flat]   {Y}-[Flat]   {AC}-[Flat]

{C}-[Origin Charges]

{C}-[Australia (to Indonesia)]
{C}-[ALL]

{C}-[Origin Documentation Fee]   {AE}-[AUD]   {AM}-[2,001.00]


{C}-[END OF DOCUMENT ]";

		protected override string MenuName => "Forwarding Landscape Pricing Page";
	}
}
