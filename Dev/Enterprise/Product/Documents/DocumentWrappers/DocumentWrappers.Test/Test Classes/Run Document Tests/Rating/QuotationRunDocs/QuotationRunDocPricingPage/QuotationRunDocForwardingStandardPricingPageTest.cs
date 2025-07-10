namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class QuotationRunDocForwardingStandardPricingPageTest : QuotationRunDocPricingPageBaseTest
	{
		protected override string Test_CurrencyENUSExpectedResult => @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 2]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[International Freight]   {AD}-[AUD]   {AH}-[10.00]

{C}-[Bunker Adjustment Factor]   {AD}-[AUD]   {AH}-[11.00]

{C}-[War Risk Surcharge]   {AD}-[AUD]   {AH}-[12.00]   {AM}-[per KG / 6000 CC]

{C}-[Currency Adjustment Factor]
{E}-[Base Rate]   {AD}-[AUD]   {AH}-[13.00]
{E}-[First Package]   {AD}-[AUD]   {AH}-[14.00]
{E}-[Additional Packages]   {AD}-[AUD]   {AH}-[15.00]   {AM}-[per Package]






{C}-[Continued Over…]

{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 2 of 2]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[International Freight]
{E}-[LD-7]   {AD}-[AUD]   {AH}-[20.00]

{C}-[Bunker Adjustment Factor]
{E}-[LD-7]   {AD}-[AUD]   {AH}-[21.00]

{C}-[War Risk Surcharge]
{E}-[LD-7]   {AD}-[AUD]   {AH}-[22.00]   {AM}-[per KG / 6000 CC]

{C}-[Currency Adjustment Factor]
{E}-[LD-7]
{G}-[Base Rate]   {AD}-[AUD]   {AH}-[23.00]
{G}-[First Package]   {AD}-[AUD]   {AH}-[24.00]
{G}-[Additional Packages]   {AD}-[AUD]   {AH}-[25.00]   {AM}-[per Package]






{C}-[END OF DOCUMENT]";

		protected override string Test_CurrencyITITExpectedResult => @"{C}-[Pagina prezzi standard spedizione]   {AO}-[Pagina 1 di 2]

{C}-[Nolo Aereo da Sydney a Los Angeles]

{C}-[Preventivo N°:]   {I}-[998 - TESTORG2]   {AA}-[Validità:]   {AI}-[01-gen-23  -  01-feb-23]
{C}-[Frequenza:]   {I}-[Non specificato]   {AA}-[Tempo di transito:]   {AI}-[Non specificato]
{C}-[Livello servizio:]   {I}-[Non specificato]   {AA}-[Compagnia aerea:]   {AI}-[Non specificato]
{C}-[Merce:]   {I}-[GEN - Generale]


{C}-[TEST CLIENT #2 ]

{C}-[Nolo Aereo da Sydney a Los Angeles]   {AE}-[Valuta]   {AI}-[Tariffa]

{C}-[Nolo internazionale]   {AD}-[AUD]   {AH}-[10.00]

{C}-[BAF (fattore rettif. costo carbur.)]   {AD}-[AUD]   {AH}-[11.00]

{C}-[Supplem. rischio bellico]   {AD}-[AUD]   {AH}-[12.00]   {AM}-[per KG / 6000 CC]

{C}-[Fattore rettifica valuta]
{E}-[Base Rate]   {AD}-[AUD]   {AH}-[13.00]
{E}-[First Package]   {AD}-[AUD]   {AH}-[14.00]
{E}-[Additional Packages]   {AD}-[AUD]   {AH}-[15.00]   {AM}-[per collo]






{C}-[Segue…]

{C}-[Pagina prezzi standard spedizione]   {AO}-[Pagina 2 di 2]

{C}-[Nolo Aereo da Sydney a Los Angeles]

{C}-[Preventivo N°:]   {I}-[998 - TESTORG2]   {AA}-[Validità:]   {AI}-[01-gen-23  -  01-feb-23]
{C}-[Frequenza:]   {I}-[Non specificato]   {AA}-[Tempo di transito:]   {AI}-[Non specificato]
{C}-[Livello servizio:]   {I}-[Non specificato]   {AA}-[Compagnia aerea:]   {AI}-[Non specificato]
{C}-[Merce:]   {I}-[GEN - Generale]


{C}-[TEST CLIENT #2 ]

{C}-[Nolo Aereo da Sydney a Los Angeles]   {AE}-[Valuta]   {AI}-[Tariffa]

{C}-[Nolo internazionale]
{E}-[LD-7]   {AD}-[AUD]   {AH}-[20.00]

{C}-[BAF (fattore rettif. costo carbur.)]
{E}-[LD-7]   {AD}-[AUD]   {AH}-[21.00]

{C}-[Supplem. rischio bellico]
{E}-[LD-7]   {AD}-[AUD]   {AH}-[22.00]   {AM}-[per KG / 6000 CC]

{C}-[Fattore rettifica valuta]
{E}-[LD-7]
{G}-[Base Rate]   {AD}-[AUD]   {AH}-[23.00]
{G}-[First Package]   {AD}-[AUD]   {AH}-[24.00]
{G}-[Additional Packages]   {AD}-[AUD]   {AH}-[25.00]   {AM}-[per collo]






{C}-[FINE DEL DOCUMENTO]";

		protected override string TestContractNumber_QuotationWithFreightChargeAndClientRateAndCompanyTariffWithOriginChargeExpectedResult => @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Origin Documentation Fee]
{E}-[20GP]   {AD}-[AUD]   {AH}-[10.00]



{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[International Freight]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,000.00]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]






{C}-[END OF DOCUMENT]";

		protected override string TestFrequency_QuotationWithFreightChargeAndCompanyTariffWithOriginChargesExpectedResult => @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Every Day]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Origin Documentation Fee]
{E}-[20GP]   {AD}-[AUD]   {AH}-[10.00]
{E}-[20GP]   {AD}-[AUD]   {AH}-[11.00]
{E}-[20GP]   {AD}-[AUD]   {AH}-[12.00]



{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[International Freight]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,000.00]






{C}-[END OF DOCUMENT]";

		protected override string TestFrequency_FreightChargesExpectedResult => @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 4]

{C}-[FCL Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[FCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[International Freight]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,000.00]






{C}-[Continued Over…]

{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 2 of 4]

{C}-[FCL Freight from Jakarta, Java to Singapore]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[FCL Freight from Jakarta, Java to Singapore]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Bunker Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[2,000.00]






{C}-[Continued Over…]

{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 3 of 4]

{C}-[FCL Freight from Chicago to Auckland]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Every Day]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[FCL Freight from Chicago to Auckland]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Currency Adjustment Factor]
{E}-[20GP]   {AD}-[USD]   {AH}-[3,000.00]






{C}-[Continued Over…]

{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 4 of 4]

{C}-[FCL Freight from Shanghai Hongqiao International Apt to Brisbane]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-23  -  01-Feb-23]
{C}-[Frequency:]   {I}-[Every Day]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[FCL Freight from Shanghai Hongqiao International Apt to Brisbane]   {AE}-[Currency]   {AI}-[Rate]

{C}-[War Risk Surcharge]
{E}-[20GP]   {AD}-[USD]   {AH}-[4,000.00]






{C}-[END OF DOCUMENT]";

		//TODO: Origin charge is shown multiple times
		protected override string TestDocument_DocBuilder_ClientRateWithSimilarOriginAndDestinationExpectedResult => @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 3]

{C}-[FCL Freight from Australia to Indonesia]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-20  -  01-Feb-20]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Australia]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Origin Documentation Fee]   {AD}-[AUD]   {AH}-[2,001.00]



{C}-[FCL Freight from Australia to Indonesia]   {AE}-[Currency]   {AI}-[Rate]

{C}-[International Freight]
{E}-[20GP]   {AD}-[USD]   {AH}-[1,001.00]






{C}-[Continued Over…]

{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 2 of 3]

{C}-[FCL Freight from Sydney to Indonesia]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-20  -  01-Feb-20]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Origin Documentation Fee]   {AD}-[AUD]   {AH}-[2,001.00]



{C}-[FCL Freight from Sydney to Indonesia]   {AE}-[Currency]   {AI}-[Rate]

{C}-[International Freight]
{E}-[40GP]   {AD}-[USD]   {AH}-[1,002.00]






{C}-[Continued Over…]

{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 3 of 3]

{C}-[FCL Freight from Sydney to Jakarta, Java]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-20  -  01-Feb-20]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Origin Documentation Fee]   {AD}-[AUD]   {AH}-[2,001.00]



{C}-[FCL Freight from Sydney to Jakarta, Java]   {AE}-[Currency]   {AI}-[Rate]

{C}-[International Freight]
{E}-[40HC]   {AD}-[USD]   {AH}-[1,003.00]






{C}-[END OF DOCUMENT]";

		protected override string MenuName => "Forwarding Standard Pricing Page";
	}
}
