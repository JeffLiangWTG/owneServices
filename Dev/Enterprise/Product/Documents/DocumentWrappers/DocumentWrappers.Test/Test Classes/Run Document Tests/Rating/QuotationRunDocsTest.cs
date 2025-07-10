using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class QuotationRunDocsTest : BaseRunDocumentsTest
	{
		public QuotationRunDocsTest() { }

		#region Company Tariff

		[TestDate(2020, 1, 1)]
		public void TestDocument_ForwardingStandardPricingPage_DocBuilder_CTB_CTG_NoConversionFactor()
		{
			TestHelper.ChargeCodes.New("CHARGE1", "Charge 1", FlatCalculator.Code);

			Factory.Save();

			var client = TestHelper.NewOrgHeader(1);

			var companyTariff = TestHelper.NewCompanyTariff();
			var companyTariffRateEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", removeLines: true);
			var companyTariffRateLine = companyTariffRateEntry.AddRateLine("CHARGE1", CartageCalculator.Code, "KG");
			companyTariffRateLine.ConversionFactor = ConversionFactor.Empty;
			var companyTariffCalculator = companyTariffRateLine.GetCalculator<CartageCalculator>();
			companyTariffCalculator["-100"] = (ZDecimal)5m;
			companyTariffCalculator["+100"] = (ZDecimal)10m;

			companyTariff.Factory.Save();

			var quote = TestHelper.NewQuote(client);
			var quoteRateEntry = quote.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", removeLines: true);
			quoteRateEntry.AddRateLine("CHARGE1", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Standard Pricing Page");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-20  -  01-Feb-20]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Air Freight from Sydney to ]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Charge 1]
{E}-[Less than 100 KG]   {AD}-[AUD]   {AH}-[5.00]   {AM}-[per KG / 6000 CC]
{E}-[100 KG and above]   {AD}-[AUD]   {AH}-[10.00]   {AM}-[per KG / 6000 CC]






{C}-[END OF DOCUMENT]",
					message: "GIVEN CTB and CompanyTariff with CTG and empty ConversionFactor WHEN print THEN ConversionFactor should be the value per the relevant registry setting."
				);
			}
		}

		#endregion

		#region Client Rate

		[TestDate(2020, 1, 1)]
		public void TestDocument_DocBuilder_ClientRates_ForwardingStandardPricingPage()
			=> TestDocument_DocBuilder_ClientRates
			(
				menuName: "Forwarding Standard Pricing Page",
				expected: @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Air Freight from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-20  -  01-Feb-20]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Airline:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[Origin - Sydney]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Origin Charge]   {AD}-[AUD]   {AH}-[10.00]



{C}-[Air Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Charge 1]   {AD}-[AUD]   {AH}-[100.00]



{C}-[Destination - Los Angeles]   {AE}-[Currency]   {AI}-[Rate]

{C}-[Destination Charge]   {AD}-[USD]   {AH}-[20.00]






{C}-[END OF DOCUMENT]");

		[TestDate(2020, 1, 1)]
		public void TestDocument_DocBuilder_ClientRates_ForwardingCompactPricingPage()
			=> TestDocument_DocBuilder_ClientRates
			(
				menuName: "Forwarding Compact Pricing Page",
				expected: @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-20 - 01-Feb-20]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Airline:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[CHARGE1|>(AUD)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[100.00]

{C}-[Origin Rates]

{C}-[Origin]   {I}-[Destination]   {U}-[Container Type]   {Y}-[OCHARGE|>(AUD)]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[10.00]

{C}-[Destination Rates]

{C}-[Origin]   {I}-[Destination]   {U}-[Container Type]   {Y}-[DCHARGE|>(USD)]
{C}-[Sydney]   {I}-[Los Angeles]   {Y}-[20.00]


{C}-[END OF DOCUMENT ]");

		[TestDate(2020, 1, 1)]
		public void TestDocument_DocBuilder_ClientRates_ForwardingLandscapePricingPage()
			=> TestDocument_DocBuilder_ClientRates
			(
				menuName: "Forwarding Landscape Pricing Page",
				expected: @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Cross Trade Air Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-20 - 01-Feb-20]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Australia - United States]

{C}-[Origin - Destination]   {O}-[Airline]
{X}-[Cur.]   {AA}-[Flat]   {BC}-[Transit Time]   {BI}-[Freq.]   {BO}-[W/V Conv.]
{C}-[Sydney - Los Angeles]


{C}-[CHARGE1 - Charge 1]   {X}-[AUD]   {AA}-[100.00]

{C}-[Origin Charges]

{C}-[Sydney (to Los Angeles)]

{C}-[Origin Charge]   {AE}-[AUD]   {AM}-[10.00]

{C}-[Destination Charges]

{C}-[Los Angeles (from Sydney)]

{C}-[Destination Charge]   {AE}-[USD]   {AM}-[20.00]


{C}-[END OF DOCUMENT ]");

		void TestDocument_DocBuilder_ClientRates(string menuName, string expected)
		{
			TestHelper.ChargeCodes.New("OCHARGE", "Origin Charge", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("DCHARGE", "Destination Charge", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE1", "Charge 1", FlatCalculator.Code);

			Factory.Save();

			var client = TestHelper.NewOrgHeader(1);

			var clientRate = TestHelper.NewClientRate(client);
			var clientRateEntry1 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "USLAX", "OCHARGE", 10m);
			var clientRateEntry2 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "AUSYD", "USLAX", "DCHARGE", 20m);

			var quote = TestHelper.NewQuote(client);
			var quoteRateEntry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "CHARGE1", 100m);
			quoteRateEntry.TI_ContractNumber = "CONT1";

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, menuName);

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: expected,
					message: "GIVEN related client-rate WHEN printing quotation THEN client-rate should be shown"
				);
			}
		}

		#endregion

		#region Decimal Comma

		[TestDate(2020, 1, 1)]
		public void TestDocument_ForwardingCompactPricingPage_DocBuilder_DecimalCommaPrintLanguage_DecimalPointLoginCountry()
		{
			TestHelper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE2", "Charge Description 2", FlatCalculator.Code);

			Factory.Save();

			var client = TestHelper.NewOrgHeader(1);
			var quote = TestHelper.NewQuote(client);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LCL, "AUSYD", "USLAX", "CHARGE1", 1000.11m, currency: "EUR");
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LCL, "AUSYD", "SGSIN", "CHARGE2", 2000.22m, currency: "COP");

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Compact Pricing Page");

			var sellRatesDecimals = new SellRatesDecimalsCollection
			{
				new SellRatesDecimals { Code = "ALL", Decimals = "3" }
			};
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sellRatesDecimals))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: @"{AK}-[Cotización para Test Client #2]   {BL}-[Página 1 de 1]


{AK}-[Tarifas de expedición]

{AK}-[№ cotización:]   {AS}-[998 - TESTORG2]
{AK}-[Validez:]   {AS}-[01-ene.-20 - 01-feb.-20]
{AK}-[Nivel servicio:]   {AS}-[No especificado]
{AK}-[Mercancía:]   {AS}-[General]
{AK}-[Aerolínea:]   {AS}-[No especificado]




{C}-[Flete Tarifas]

{C}-[Origen]   {I}-[Destino]   {O}-[Puerto transbordo]   {U}-[Tipo contenedor]   {Y}-[CHARGE1|>(EUR)]   {AC}-[CHARGE2|>(COP)]   {BI}-[Frecuencia]   {BO}-[Hora transporte]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[No en]   {Y}-[1,000.11]
{U}-[contenedo]
{U}-[res]
{C}-[Sydney]   {I}-[Singapore]   {U}-[No en]   {AC}-[2,000.22]
{U}-[contenedo]
{U}-[res]


{C}-[FIN DEL DOCUMENTO ]",
					language: "ES-ES", // Language with decimal comma
					message: "GIVEN decimal-point login company WHEN printing in decimal-comma language THEN should have correct zeros with login company formatting"
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_ForwardingCompactPricingPage_DocBuilder_DecimalCommaPrintLanguage_DecimalCommaLoginCountry()
		{
			TestHelper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE2", "Charge Description 2", FlatCalculator.Code);

			Factory.Save();

			var client = TestHelper.NewOrgHeader(1);
			var quote = TestHelper.NewQuote(client);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LCL, "AUSYD", "USLAX", "CHARGE1", 1000.11m, currency: "EUR");
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LCL, "AUSYD", "SGSIN", "CHARGE2", 2000.22m, currency: "COP");

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Compact Pricing Page");

			var sellRatesDecimals = new SellRatesDecimalsCollection
			{
				new SellRatesDecimals { Code = "ALL", Decimals = "3" }
			};
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Spain))
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sellRatesDecimals))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: @"{AK}-[Cotización para Test Client #2]   {BL}-[Página 1 de 1]


{AK}-[Tarifas de expedición]

{AK}-[№ cotización:]   {AS}-[998 - TESTORG2]
{AK}-[Validez:]   {AS}-[01-ene.-20 - 01-feb.-20]
{AK}-[Nivel servicio:]   {AS}-[No especificado]
{AK}-[Mercancía:]   {AS}-[General]
{AK}-[Aerolínea:]   {AS}-[No especificado]




{C}-[Flete Tarifas]

{C}-[Origen]   {I}-[Destino]   {O}-[Puerto transbordo]   {U}-[Tipo contenedor]   {Y}-[CHARGE1|>(EUR)]   {AC}-[CHARGE2|>(COP)]   {BI}-[Frecuencia]   {BO}-[Hora transporte]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[No en]   {Y}-[1.000,11]
{U}-[contenedo]
{U}-[res]
{C}-[Sydney]   {I}-[Singapore]   {U}-[No en]   {AC}-[2.000,22]
{U}-[contenedo]
{U}-[res]


{C}-[FIN DEL DOCUMENTO ]",
					language: "ES-ES", // Language with decimal comma
					message: "GIVEN decimal-comma login company WHEN printing in decimal-comma language THEN should have correct zeros with login company formatting"
				);
			}
		}

		#endregion

		#region Shipping Compact Pricing Page

		public void TestTemplate_ShippingCompactPricingPage()
		{
			RunDocumentWithAllSections = ZBool.True;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Shipping Compact Pricing Page");
			AssertRunDocument((IDocumentSupportable)GetBusinessObject, @"{U}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Description>]   {AU}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>]   {BC}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.DecimalPlaces>"")>)>]   {BI}-[<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Units>]
{W}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Description>]   {AU}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>]   {BC}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.DecimalPlaces>"")>)>]   {BI}-[<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Units>]
{Y}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Description>]   {AU}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>]   {BC}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.DecimalPlaces>"")>)>]   {BI}-[<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Units>]
{AA}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Description>]   {AU}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>]   {BC}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.DecimalPlaces>"")>)>]   {BI}-[<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Units>]
{AC}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Description>]   {AU}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>]   {BC}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.DecimalPlaces>"")>)>]   {BI}-[<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.Units>]
{C}-[<AutoHeight><If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Index>"" == ""0"", ""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].Origin>"", """")>]   {I}-[<AutoHeight><If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Index>"" == ""0"", ""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].Destination>"", """")>]   {O}-[<AutoHeight><If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Index>"" == ""0"", ""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].Via>"", """")>]   {U}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Split>]   {Y}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[1]>]   {AC}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[2]>]   {AG}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[3]>]   {AK}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[4]>]   {AO}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[5]>]   {AS}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[6]>]   {AW}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[7]>]   {BA}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[8]>]   {BE}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[9]>]   {BI}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].Frequency>]   {BO}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].TransitTime>]
{U}-[Consignor:]   {AC}-[<If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].Consignor.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].Consignor.CompanyName>"")>]   {AU}-[Consignee:]   {BC}-[<If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].Consignee.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].Consignee.CompanyName>"")>]



{U}-[<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.RateLine.ParentEntry.Mode>]




{C}-[<If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Label>"" == ""TableRows"", ""Freight"", ""<If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Label>"" == ""OriginRates"", ""Origin"", ""<If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Label>"" == ""DestinationRates"", ""Destination"", ""???"")>"")>"")> Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[<ExpandToFit><If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].IsSupplementary>"" == ""N"", ""Tranship Port"", """")>]   {U}-[Container Type]   {Y}-[<ExpandToFit><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[1].Heading>]   {AC}-[<ExpandToFit><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[2].Heading>]   {AG}-[<ExpandToFit><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[3].Heading>]   {AK}-[<ExpandToFit><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[4].Heading>]   {AO}-[<ExpandToFit><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[5].Heading>]   {AS}-[<ExpandToFit><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[6].Heading>]   {AW}-[<ExpandToFit><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[7].Heading>]   {BA}-[<ExpandToFit><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[8].Heading>]   {BE}-[<ExpandToFit><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[9].Heading>]   {BI}-[<If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].IsSupplementary>"" == ""N"", ""Frequency"", """")>]   {BO}-[<If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].IsSupplementary>"" == ""N"", ""Transport Time"", """")>]



{C}-[<Image(.Rating.Logo,11,33)>]   {AK}-[<ShrinkToFit><Rating.PrimarySource>]   {BL}-[Page <Current Page> of <TotalPages>]


{AK}-[Shipping Rates]

{AK}-[Quote No:]   {AS}-[<JobNumber> - <JobHeaderLocalClient.CompanyCode>]
{AK}-[Validity:]   {AS}-[<DateTimeAsString('<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].ValidFrom>', 'dd-MMM-yy')> - <DateTimeAsString('<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].ValidUntil>', 'dd-MMM-yy')>]
{AK}-[Service Level:]   {AS}-[<If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].ServiceLevel>"" == """", ""Not Specified"", ""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].ServiceLevel>"")>]
{AK}-[Commodity:]   {AS}-[<If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].CommodityCode.Description>"" == """", ""Not Specified"", ""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].CommodityCode.Description>"")>]
{AK}-[<If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].IfDisplayProvider>"" == ""True"", ""Principal:"", """")>]   {AS}-[<If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].IfDisplayProvider>"" == ""True"", ""<If(""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].Provider.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].Provider.CompanyName>"")>"", """")>]



{C}-[<HideRowIfCellIsEmpty><AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Page.OpeningText>]


{C}-[CFX Information]
{C}-[<Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Page.CFX.Format(""{NameAndValue}"", Newline)>]

{C}-[<AutoHeight><Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows].Page.ClosingText>]

{C}-[A local Value Added Tax charge (equivalent to <if(""<JobHeaderLocalClient.TaxCode>"" != """", ""<JobHeaderLocalClient.TaxCode>"", ""<TaxCode>"")>) may apply to all items marked with an asterisk (*).]


{C}-[Continued Over… ]

{C}-[Continued Over… ]

{C}-[END OF DOCUMENT ]

{C}-[END OF DOCUMENT ]");
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_ShippingCompactPricingPage_ShouldShowAllRatesInColumnsWithCorrectDecimalPoints()
		{
			TestHelper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE2", "Charge Description 2", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE3", "Charge Description 3", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE4", "Charge Description 4", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE5", "Charge Description 5", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE6", "Charge Description 6", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE7", "Charge Description 7", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE8", "Charge Description 8", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE9", "Charge Description 9", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE10", "Charge Description 10", FlatCalculator.Code);

			Factory.Save();

			var client = TestHelper.NewOrgHeader(1);
			var quote = TestHelper.NewQuote(client);
			var rateEntry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, RateMode.LCL, "AUSYD", "USLAX", "CHARGE1", 1.0011m, currency: "LYD");
			rateEntry.AddFlatRateLine("CHARGE2", 2.0012, "AUD");

			var rateLine3 = rateEntry.AddRateLine("CHARGE3", PercentageCalculator.Code, currencyCode: "LYD");
			rateLine3.GetCalculator<PercentageCalculator>().Percent = 3.0013;
			var rateLineItem3 = rateLine3.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			rateLineItem3.TM_Text = "ALL";

			rateEntry.AddFlatRateLine("CHARGE4", 4.0014, "LYD");
			rateEntry.AddFlatRateLine("CHARGE5", 5.0015, "LYD");
			rateEntry.AddFlatRateLine("CHARGE6", 6.0016, "LYD");
			rateEntry.AddFlatRateLine("CHARGE7", 7.0017, "LYD");
			rateEntry.AddFlatRateLine("CHARGE8", 8.0018, "LYD");
			rateEntry.AddFlatRateLine("CHARGE9", 9.0019, "LYD");
			rateEntry.AddFlatRateLine("CHARGE10", 10.0010, "LYD");

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Shipping Compact Pricing Page");

			var sellRatesDecimals = new SellRatesDecimalsCollection
			{
				new SellRatesDecimals { Code = "ALL", Decimals = "3" }
			};
			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sellRatesDecimals))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Shipping Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-20 - 01-Feb-20]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Principal:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[CHARGE1|>(LYD)]   {AC}-[CHARGE2|>(AUD)]   {AG}-[CHARGE3|>(LYD)]   {AK}-[CHARGE4|>(LYD)]   {AO}-[CHARGE5|>(LYD)]   {AS}-[CHARGE6|>(LYD)]   {AW}-[CHARGE7|>(LYD)]   {BA}-[CHARGE8|>(LYD)]   {BE}-[CHARGE9|>(LYD)]   {BI}-[Frequency]   {BO}-[Transport Time]

{C}-[Sydney]   {I}-[Los Angeles]   {U}-[Non-Cont]   {Y}-[1.001]   {AC}-[2.00]   {AG}-[See Below]   {AK}-[4.001]   {AO}-[5.002]   {AS}-[6.002]   {AW}-[7.002]   {BA}-[8.002]   {BE}-[9.002]
{U}-[ainerized]
{U}-[LCL]

{U}-[Charge Description 3]   {BC}-[3.001]   {BI}-[% of all charges]



{C}-[END OF DOCUMENT ]",
					message: "Should show all rates where amount should be formatted with RateLine currency and percentage should be formatted like GUI."
				); // The missing 10th rate will be fixed in WI00433898
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_ShippingCompactPricingPage_ShouldShowAllRatesInRowsWithCorrectDecimalPoints()
		{
			TestHelper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);
			Factory.Save();

			var client = TestHelper.NewOrgHeader(1);
			var quote = TestHelper.NewQuote(client);
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.SNC, RateMode.LCL, "AUSYD", "USLAX");

			var rateLine1 = rateEntry.AddRateLine("CHARGE1", PercentageCalculator.Code, currencyCode: "LYD");
			rateLine1.GetCalculator<PercentageCalculator>().Percent = 101.12345;
			var rateLineItem1 = rateLine1.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			rateLineItem1.TM_Text = "ALL";

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Shipping Compact Pricing Page");

			var sellRatesDecimals = new SellRatesDecimalsCollection
			{
				new SellRatesDecimals { Code = "ALL", Decimals = "3" }
			};
			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sellRatesDecimals))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Shipping Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-20 - 01-Feb-20]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Principal:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[CHARGE1|>(LYD)]   {BI}-[Frequency]   {BO}-[Transport Time]

{C}-[Sydney]   {I}-[Los Angeles]   {U}-[Non-Cont]   {Y}-[See Below]
{U}-[ainerized]
{U}-[LCL]

{U}-[Charge Description 1]   {BC}-[101.124]   {BI}-[% of all charges]



{C}-[END OF DOCUMENT ]",
					message: "Rate amount should be formatted with RateLine currency and rate percent should be formatted like GUI."
				);
			}
		}

		#endregion

		#region Shipping Standard Pricing Page

		public void TestTemplate_ShippingStandardPricingPage_DocBuilder()
		{
			RunDocumentWithAllSections = ZBool.True;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Shipping Standard Pricing Page");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					(IDocumentSupportable)GetBusinessObject,
					@"{C}-[<Image(.Rating.Logo,1,47)>]

{C}-[<ShrinkToFit><ReportName>]   {AO}-[Page <Current Page> of <TotalPages>]



{C}-[<ShrinkToFit><ReportName>]   {AO}-[Page <Current Page> of <TotalPages>]


{C}-[<AutoHeight><Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Description>]   {AE}-[<AutoHeight><Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Currency>]   {AK}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Amount>, <If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Currency>"", ""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.DecimalPlaces>"")>)>]   {AO}-[<AutoHeight><Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Units>]
{E}-[<AutoHeight><Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Description>]   {AE}-[<AutoHeight><Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Currency>]   {AK}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Amount>, <If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Currency>"", ""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.DecimalPlaces>"")>)>]   {AO}-[<AutoHeight><Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Units>]
{G}-[<AutoHeight><Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Description>]   {AE}-[<AutoHeight><Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Currency>]   {AK}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Amount>, <If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Currency>"", ""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.DecimalPlaces>"")>)>]   {AO}-[<AutoHeight><Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Units>]
{I}-[<AutoHeight><Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Description>]   {AE}-[<AutoHeight><Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Currency>]   {AK}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Amount>, <If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Currency>"", ""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.DecimalPlaces>"")>)>]   {AO}-[<AutoHeight><Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Units>]
{K}-[<AutoHeight><Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Description>]   {AE}-[<AutoHeight><Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Currency>]   {AK}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Amount>, <If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Currency>"", ""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.DecimalPlaces>"")>)>]   {AO}-[<AutoHeight><Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Units>]



{C}-[<If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Label>"" == ""FreightRates"", ""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].Mode> Freight from <Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Origin> to <Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Destination>"", ""<If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Label>"" == ""OriginRates"", ""Origin - <Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Origin>"", ""<If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Label>"" == ""DestinationRates"", ""Destination - <Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.Destination>"", ""??"")>"")>"")>]   {AE}-[Currency]   {AK}-[Rate]





{C}-[<AutoHeight><If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].PageHeader>"" != """", ""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].PageHeader>"", ""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].DiscountDescription><If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].Origin>"" == """", """", "" from <Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].Origin>"")><If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].Destination>"" == """", """", "" to <Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].Destination>"")><If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].Via>"" == """", """", "" via <Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].Via>"")>"")><If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.IncoTerm.Code>"" == """", """", "" (<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.RateLine.IncoTerm.Code>)"")>]

{C}-[Quote No:]   {I}-[<JobNumber> - <JobHeaderLocalClient.CompanyCode>]   {AA}-[Validity:]   {AI}-[<DateTimeAsString('<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].ValidFrom>', 'dd-MMM-yy')>  -  <DateTimeAsString('<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].ValidUntil>', 'dd-MMM-yy')>]
{C}-[Frequency:]   {I}-[<If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].Frequency>"" == """", ""Not Specified"", ""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].Frequency>"")>]   {AA}-[Transit Time:]   {AI}-[<If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].TransitTime>"" == """", ""Not Specified"", ""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].TransitTime>"")>]
{C}-[Service Level:]   {I}-[<If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].ServiceLevel>"" == """", ""Not Specified"", ""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].ServiceLevel>"")>]   {AA}-[<If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].IfDisplayProvider>"" == ""True"", ""Principal:"", """")>]   {AI}-[<AutoHeight><If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].IfDisplayProvider>"" == ""True"", ""<If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].Provider.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].Provider.CompanyName>"")>"", """")>]
{C}-[Commodity:]   {I}-[<AutoHeight><If(""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].CommodityCode>"" == """", ""Not Specified"", ""<Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.Entries[1].CommodityCode>"")>]


{C}-[<AutoHeight><Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.OpeningText>]

{C}-[<JobHeaderLocalClient.CompanyName> ]   {AQ}-[<If(""<Rating.IsReprint>"" == ""Y"", ""REPRINT"", """")>]


{C}-[CFX Information]
{C}-[<AutoHeight><Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.CFX.Format(""{NameAndValue}"", NewLine)>]

{C}-[<AutoHeight><Rating.PageSets[ShippingStandard].OriginDestinationAndFreightRates.Page.ClosingText>]

{C}-[A local Value Added Tax charge (equivalent to <if(""<JobHeaderLocalClient.TaxCode>"" != """", ""<JobHeaderLocalClient.TaxCode>"", ""<TaxCode>"")>) may apply to all items marked with an asterisk (*).]


{C}-[<If(""<CurrentPage>"" != ""<TotalPages>"", ""Continued Over…"", """")>]

{C}-[<If(""<CurrentPage>"" != ""<TotalPages>"", ""Continued Over…"", """")>]

{C}-[END OF DOCUMENT]

{C}-[END OF DOCUMENT]"
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_ShippingStandardPricingPage_DocBuilder_ShouldShowCorrectDecimalPoints()
		{
			TestHelper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);
			Factory.Save();

			var client = TestHelper.NewOrgHeader(1);
			var quote = TestHelper.NewQuote(client);
			var rateEntry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SNC, RateMode.LCL, "AUSYD", "USLAX", "CHARGE1", 1.0011m, currency: "LYD");

			var rateLine1 = rateEntry.AddRateLine("CHARGE1", PercentageCalculator.Code, currencyCode: "LYD");
			rateLine1.GetCalculator<PercentageCalculator>().Percent = 101.12345;
			var rateLineItem1 = rateLine1.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			rateLineItem1.TM_Text = "ALL";

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Shipping Standard Pricing Page");

			var sellRatesDecimals = new SellRatesDecimalsCollection
			{
				new SellRatesDecimals { Code = "ALL", Decimals = "3" }
			};

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sellRatesDecimals))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: @"{C}-[Shipping Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[Shipping Non-Containerized Freight Charges from Sydney to Los Angeles]

{C}-[Quote No:]   {I}-[998 - TESTORG2]   {AA}-[Validity:]   {AI}-[01-Jan-20  -  01-Feb-20]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Principal:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]


{C}-[TEST CLIENT #2 ]

{C}-[LCL Freight from Sydney to Los Angeles]   {AE}-[Currency]   {AK}-[Rate]

{C}-[Charge Description 1]
{AE}-[LYD]   {AK}-[1.001]
{AK}-[101.124]   {AO}-[% of all charges]






{C}-[END OF DOCUMENT]",
					message: "Rate amount should be formatted with RateLine currency and percentage should be displayed like GUI."
				);
			}
		}

		#endregion

		#region Cover Page

		public void TestTemplate_CoverPage()
		{
			RunDocumentWithAllSections = ZBool.True;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cover Page");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument((IDocumentSupportable)GetBusinessObject, @"{C}-[<Image(.Rating.Logo,1,47)>]

{C}-[<ShrinkToFit><ReportName>]   {AO}-[Page <Current Page> of <TotalPages>]



{C}-[<ShrinkToFit><ReportName>]   {AO}-[Page <Current Page> of <TotalPages>]


{G}-[<ShrinkToFit><RecipientNameAndAddress>]   {AE}-[<Upper(""<JobNumberHeading>"")>]   {AO}-[<JobNumber>]

{AE}-[<Upper(""<SecondaryHeading>"")>]   {AO}-[<SecondaryNumber>]

{AE}-[DATE]   {AO}-[<DateTimeAsString('<Now>', 'dd-MMM-yy HH:mm')>]







{C}-[<Upper(""<Rating.QuotationTitle>"")> <JobNumber> <JobHeaderLocalClient.CompanyName>]

{C}-[<RecipientSalutation>]

{C}-[<AutoHeight><Rating.CoverPageText>]




{C}-[<UrlHyperlink(<GetTrackingUrl(<RecipientContactPK>, QuotationClientReplyAccept, <TrackingBusinessObjectPK>)>, <Rating.QuotationAcceptText>, <Rating.QuotationAcceptTooltip>)>]
{C}-[<UrlHyperlink(<GetTrackingUrl(<RecipientContactPK>, QuotationClientReplyNotAccept, <TrackingBusinessObjectPK>)>, ""Request further discussion"", ""Request further discussion"")>]


{C}-[<SignOffText>]

{C}-[<Image(SalesRep.SignatureForQuoteDocuments, 3, 19, Y)>]   {AC}-[<Image(Rating.SecondSignatory.SignatureForQuoteDocuments, 3, 19, Y)>]


{C}-[<SalesRep.FullName>]   {AC}-[<Rating.SecondSignatory.FullName>]
{C}-[<SalesRep.Title>]   {AC}-[<Rating.SecondSignatory.Title>]

{C}-[<BrandName>]



{C}-[<If(""<CurrentPage>"" != ""<TotalPages>"", ""Continued Over…"", """")>]

{C}-[<If(""<CurrentPage>"" != ""<TotalPages>"", ""Continued Over…"", """")>]

{C}-[<AutoHeight><Rating.CoverPageFooterText>]

{C}-[END OF DOCUMENT]

{C}-[<AutoHeight><Rating.CoverPageFooterText>]

{C}-[END OF DOCUMENT]");
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_CoverPage_GivenDraftDocumentPrintMode_ThenShouldNotShowQuotationClientReplyLinks()
			=> TestDocument_CoverPage_ShouldNotShowQuotationClientReplyLinks(QuotationDocumentMode.Draft);

		[TestDate(2020, 1, 1)]
		public void TestDocument_CoverPage_GivenUnknownDocumentPrintMode_ThenShouldNotShowQuotationClientReplyLinks()
			=> TestDocument_CoverPage_ShouldNotShowQuotationClientReplyLinks(QuotationDocumentMode.Unknown);

		void TestDocument_CoverPage_ShouldNotShowQuotationClientReplyLinks(QuotationDocumentMode quotationDocumentMode)
		{
			TestHelper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);
			Factory.Save();

			var client = TestHelper.NewOrgHeader(1);
			var quote = TestHelper.NewQuote(client);
			quote.DocumentPrintMode = quotationDocumentMode;
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LCL, "AUSYD", "USLAX", "CHARGE1", 1000.11m);

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cover page");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: @"{C}-[Cover Page]   {AO}-[Page 1 of 1]

{G}-[*** NO ORGANIZATION DETAILS FOUND ***]   {AE}-[QUOTE NO]   {AO}-[998]

{AE}-[DATE]   {AO}-[01-Jan-20 00:00]






{C}-[QUOTATION 998 TEST CLIENT #2]








{C}-[Yours Sincerely,]




{C}-[CargoWise Support]

{C}-[EAGLE DATAMATION INTERNATIONAL]



{C}-[This quotation is subject to our Standard Terms and Conditions which are available on request.]

{C}-[END OF DOCUMENT]",
					message: "Should not show Quotation Client Reply Links"
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_CoverPage_GivenFinalDocumentPrintMode_ThenShouldShowQuotationClientReplyLinks()
			=> TestDocument_CoverPage_ShouldShowQuotationClientReplyLinks(QuotationDocumentMode.Final);

		[TestDate(2020, 1, 1)]
		public void TestDocument_CoverPage_GivenReprintDocumentPrintMode_ThenShouldShowQuotationClientReplyLinks()
			=> TestDocument_CoverPage_ShouldShowQuotationClientReplyLinks(QuotationDocumentMode.Reprint);

		void TestDocument_CoverPage_ShouldShowQuotationClientReplyLinks(QuotationDocumentMode quotationDocumentMode)
		{
			TestHelper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);

			Factory.Save();

			var client = TestHelper.NewOrgHeader(1);
			var quote = TestHelper.NewQuote(client);
			quote.DocumentPrintMode = quotationDocumentMode;
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LCL, "AUSYD", "USLAX", "CHARGE1", 1000.11m);

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cover page");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: @"{C}-[Cover Page]   {AO}-[Page 1 of 1]

{G}-[*** NO ORGANIZATION DETAILS FOUND ***]   {AE}-[QUOTE NO]   {AO}-[998]

{AE}-[DATE]   {AO}-[01-Jan-20 00:00]






{C}-[QUOTATION 998 TEST CLIENT #2]







{C}-[Accept Quotation]
{C}-[Request further discussion]

{C}-[Yours Sincerely,]




{C}-[CargoWise Support]

{C}-[EAGLE DATAMATION INTERNATIONAL]



{C}-[This quotation is subject to our Standard Terms and Conditions which are available on request.]

{C}-[END OF DOCUMENT]",
					message: "Should show Quotation Client Reply Links"
				);
			}
		}

		#endregion

		#region Forwarding Compact Pricing Page

		public void TestTemplate_ForwardingCompactPricingPage()
		{
			RunDocumentWithAllSections = ZBool.True;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Compact Pricing Page");
			AssertRunDocument((IDocumentSupportable)GetBusinessObject, @"{U}-[<AutoHeight><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Description>]   {AU}-[<AutoHeight><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>]   {BC}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.DecimalPlaces>"")>)>]   {BI}-[<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Units>]
{W}-[<AutoHeight><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Description>]   {AU}-[<AutoHeight><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>]   {BC}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.DecimalPlaces>"")>)>]   {BI}-[<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Units>]
{Y}-[<AutoHeight><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Description>]   {AU}-[<AutoHeight><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>]   {BC}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.DecimalPlaces>"")>)>]   {BI}-[<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Units>]
{AA}-[<AutoHeight><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Description>]   {AU}-[<AutoHeight><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>]   {BC}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.DecimalPlaces>"")>)>]   {BI}-[<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Units>]
{AC}-[<AutoHeight><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Description>]   {AU}-[<AutoHeight><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>]   {BC}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.DecimalPlaces>"")>)>]   {BI}-[<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.Units>]
{C}-[<AutoHeight><If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Index>"" == ""0"", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].Origin>"", """")>]   {I}-[<AutoHeight><If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Index>"" == ""0"", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].Destination>"", """")>]   {O}-[<AutoHeight><If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Index>"" == ""0"", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].Via>"", """")>]   {U}-[<AutoHeight><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Split>]   {Y}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[1]>, <Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[1].Currency>)>]   {AC}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[2]>, <Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[2].Currency>)>]   {AG}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[3]>, <Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[3].Currency>)>]   {AK}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[4]>, <Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[4].Currency>)>]   {AO}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[5]>, <Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[5].Currency>)>]   {AS}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[6]>, <Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[6].Currency>)>]   {AW}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[7]>, <Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[7].Currency>)>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[8]>, <Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[8].Currency>)>]   {BE}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[9]>, <Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[9].Currency>)>]   {BI}-[<AutoHeight><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].Frequency>]   {BO}-[<AutoHeight><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].TransitTime>]
{U}-[Consignor:]   {AC}-[<If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].Consignor.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].Consignor.CompanyName>"")>]   {AU}-[Consignee:]   {BC}-[<If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].Consignee.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].Consignee.CompanyName>"")>]



{U}-[<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.RateLine.ParentEntry.Mode>]



{C}-[<If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Label>"" == ""TableRows"", ""Freight"", ""<If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Label>"" == ""OriginRates"", ""Origin"", ""<If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Label>"" == ""DestinationRates"", ""Destination"", ""???"")>"")>"")> Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[<If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].IsSupplementary>"" == ""N"", ""Tranship Port"", """")>]   {U}-[<ExpandToFit>Container Type]   {Y}-[<ExpandToFit><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[1].Heading>]   {AC}-[<ExpandToFit><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[2].Heading>]   {AG}-[<ExpandToFit><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[3].Heading>]   {AK}-[<ExpandToFit><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[4].Heading>]   {AO}-[<ExpandToFit><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[5].Heading>]   {AS}-[<ExpandToFit><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[6].Heading>]   {AW}-[<ExpandToFit><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[7].Heading>]   {BA}-[<ExpandToFit><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[8].Heading>]   {BE}-[<ExpandToFit><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Columns[9].Heading>]   {BI}-[<If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].IsSupplementary>"" == ""N"", ""Frequency"", """")>]   {BO}-[<If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Row.Entries[1].IsSupplementary>"" == ""N"", ""Transport Time"", """")>]



{C}-[<Image(.Rating.Logo,11,33)>]   {AK}-[<ShrinkToFit><Rating.PrimarySource>]   {BL}-[Page <Current Page> of <TotalPages>]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[<JobNumber> - <JobHeaderLocalClient.CompanyCode>]
{AK}-[Validity:]   {AS}-[<DateTimeAsString('<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].ValidFrom>', 'dd-MMM-yy')> - <DateTimeAsString('<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].ValidUntil>', 'dd-MMM-yy')>]
{AK}-[Service Level:]   {AS}-[<If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].ServiceLevel>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].ServiceLevel>"")>]
{AK}-[Commodity:]   {AS}-[<If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].CommodityCode.Description>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].CommodityCode.Description>"")>]
{AK}-[<If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].IfDisplayProvider>"" == ""True"", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].Provider.TypeDescription>:"", """")>]   {AS}-[<If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].IfDisplayProvider>"" == ""True"", ""<If(""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].Provider.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Page.Entries[1].Provider.CompanyName>"")>"", """")>]



{C}-[<HideRowIfCellIsEmpty><AutoHeight><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Page.OpeningText>]


{C}-[CFX Information]
{C}-[<Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.Page.CFX.Format(""{NameAndValue}"", Newline)>]

{C}-[<AutoHeight><Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows].Page.ClosingText>]

{C}-[A local Value Added Tax charge (equivalent to <if(""<JobHeaderLocalClient.TaxCode>"" != """", ""<JobHeaderLocalClient.TaxCode>"", ""<TaxCode>"")>) may apply to all items marked with an asterisk (*).]

{C}-[Continued Over… ]

{C}-[Continued Over… ]

{C}-[END OF DOCUMENT ]

{C}-[END OF DOCUMENT ]");
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_ForwardingCompactPricingPage_ShouldShowCorrectAmountAndFormattingForEUR_DocBuilder()
		{
			var company = (Enterprise.MasterFiles.Integration.IGlbCompany)EnvProxy.Instance.CurrentCompany;
			using (company.TemporarilySetCountry("IT"))
			{
				TestHelper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);

				Factory.Save();

				var client = TestHelper.NewOrgHeader(1);
				var quote = TestHelper.NewQuote(client);
				var rateEntry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LCL, "AUSYD", "USLAX", "CHARGE1", 1.0011m, currency: "EUR");

				RunDocumentWithAllSections = ZBool.False;

				FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Compact Pricing Page");

				var sellRatesDecimals = new SellRatesDecimalsCollection
				{
					new SellRatesDecimals { Code = "ALL", Decimals = "3" }
				};
				using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sellRatesDecimals))
				{
					AssertRunDocument
					(
						quote,
						expectedOutput: @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-20 - 01-Feb-20]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Airline:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[CHARGE1|>(EUR)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[Non-Conta]   {Y}-[1,00]
{U}-[inerized]


{C}-[END OF DOCUMENT ]",
						message: "Should show all rates where amount should be formatted with RateLine currency and percentage should be formatted like GUI."
					);
				}
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_ForwardingCompactPricingPage_DocBuilder_RateLineCurrency()
		{
			TestHelper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE2", "Charge Description 2", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE3", "Charge Description 3", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE4", "Charge Description 4", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE5", "Charge Description 5", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE6", "Charge Description 6", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE7", "Charge Description 7", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE8", "Charge Description 8", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE9", "Charge Description 9", FlatCalculator.Code);

			Factory.Save();

			var client = TestHelper.NewOrgHeader(1);
			var quote = TestHelper.NewQuote(client);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "CHARGE1", 1234.5678, currency: "JPY", container: "20GP");
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "CHARGE2", 2345.6789, currency: "AUD", container: "20GP");
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "CHARGE3", 3456.7891, currency: "LYD", container: "20GP");
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "CHARGE4", 4567.8912, currency: "JPY", container: "20GP");
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "CHARGE5", 5678.9123, currency: "AUD", container: "20GP");
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "CHARGE6", 6789.1234, currency: "LYD", container: "20GP");
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "CHARGE7", 7891.2345, currency: "JPY", container: "20GP");
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "CHARGE8", 8912.3456, currency: "AUD", container: "20GP");
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "CHARGE9", 9123.4567, currency: "LYD", container: "20GP");

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Compact Pricing Page");

			var sellRatesDecimals = new SellRatesDecimalsCollection
			{
				new SellRatesDecimals { Code = "ALL", Decimals = "4" }
			};
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sellRatesDecimals))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-20 - 01-Feb-20]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Shipping Line:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[CHARGE1|>(JPY)]   {AC}-[CHARGE2|>(AUD)]   {AG}-[CHARGE3|>(LYD)]   {AK}-[CHARGE4|>(JPY)]   {AO}-[CHARGE5|>(AUD)]   {AS}-[CHARGE6|>(LYD)]   {AW}-[CHARGE7|>(JPY)]   {BA}-[CHARGE8|>(AUD)]   {BE}-[CHARGE9|>(LYD)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[1,235]   {AC}-[2,345.68]   {AG}-[3,456.789]   {AK}-[4,568]   {AO}-[5,678.91]   {AS}-[6,789.123]   {AW}-[7,891]   {BA}-[8,912.35]   {BE}-[9,123.457]


{C}-[END OF DOCUMENT ]",
					message: "GIVEN rateLine with LCY currency with 3 decimals THEN should show amount with 3 decimals."
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_ForwardingCompactPricingPage_DocBuilder_RateLineCurrency_MultipleContainers()
		{
			TestHelper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE2", "Charge Description 2", FlatCalculator.Code);

			Factory.Save();

			var client = TestHelper.NewOrgHeader(1);
			var quote = TestHelper.NewQuote(client);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "CHARGE1", 1234.5678, currency: "JPY", container: "20GP");
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "CHARGE2", 2345.6789, currency: "AUD", container: "20GP");

			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "CHARGE1", 1234.5678, currency: "AUD", container: "40GP");
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "CHARGE2", 2345.6789, currency: "JPY", container: "40GP");

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Compact Pricing Page");

			var sellRatesDecimals = new SellRatesDecimalsCollection
			{
				new SellRatesDecimals { Code = "ALL", Decimals = "4" }
			};
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sellRatesDecimals))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-20 - 01-Feb-20]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Shipping Line:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[CHARGE1|>(JPY)]   {AC}-[CHARGE2|>(AUD)]   {AG}-[CHARGE1|>(AUD)]   {AK}-[CHARGE2|>(JPY)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Los Angeles]   {U}-[20GP]   {Y}-[1,235]   {AC}-[2,345.68]
{U}-[40GP]   {AG}-[1,234.57]   {AK}-[2,346]


{C}-[END OF DOCUMENT ]",
					message: "GIVEN rateLine with LCY currency with 3 decimals THEN should show amount with 3 decimals."
				);
			}
		}

		#endregion

		#region Forwarding Landscape Pricing Page

		[TestDate(2020, 1, 1)]
		public void TestDocument_ForwardingLandscapePricingPage_DocBuilder_EmptyContainer()
		{
			TestHelper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE2", "Charge Description 2", FlatCalculator.Code);

			Factory.Save();

			var client = TestHelper.NewOrgHeader(1);

			var clientRate = TestHelper.NewClientRate(client);
			var rateEntry11 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", container: "", removeLines: true);
			rateEntry11.AddFlatRateLine("FRT", 100);
			rateEntry11.AddFlatRateLine("CHARGE1", 101);
			rateEntry11.AddFlatRateLine("CHARGE2", 102);
			var rateEntry12 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", container: "40GP", removeLines: true);
			rateEntry12.AddFlatRateLine("FRT", 140);
			rateEntry12.AddFlatRateLine("CHARGE1", 141);
			rateEntry12.AddFlatRateLine("CHARGE2", 142);

			var quote = TestHelper.NewQuote(client);
			var rateEntry21 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", container: "", removeLines: true);
			rateEntry21.AddFlatRateLine("FRT", 200);
			rateEntry21.AddFlatRateLine("CHARGE1", 201);
			var rateEntry22 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", container: "20GP", removeLines: true);
			rateEntry22.AddFlatRateLine("FRT", 220);
			rateEntry22.AddFlatRateLine("CHARGE1", 221);
			rateEntry22.AddFlatRateLine("CHARGE2", 222);

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Landscape Pricing Page");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Cross Trade Sea Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-20 - 01-Feb-20]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Australia - United States]

{C}-[Origin - Destination]   {K}-[Shipping Line]   {S}-[Cur.]   {Y}-[20GP]   {AW}-[Transit Time]   {BA}-[Freq.]   {BG}-[W/V Conv.]   {BM}-[Contract No.]

{C}-[Sydney - Los Angeles]   {S}-[USD]   {U}-[200.00]   {Y}-[220.00]
{U}-[Flat]   {Y}-[Flat]
{S}-[FCL]
{S}-[Charge Description 1]
{U}-[Empty]   {AU}-[USD]   {BC}-[201.00]
{U}-[20GP]   {AU}-[USD]   {BC}-[221.00]
{S}-[Charge Description 2]
{U}-[Empty]   {AU}-[USD]   {BC}-[102.00]
{U}-[20GP]   {AU}-[USD]   {BC}-[222.00]


{C}-[END OF DOCUMENT ]",
					message: "GIVEN FCL rate with empty container THEN it should be printed."
				);
			}
		}

		public void TestTemplate_ForwardingLandscapePricingPage()
		{
			RunDocumentWithAllSections = ZBool.True;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Landscape Pricing Page");
			AssertRunDocument((IDocumentSupportable)GetBusinessObject, @"{S}-[<Image(.Rating.Logo, 1, 37)>]


{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Description>]   {AE}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Amount>]   {AU}-[<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Units>]
{E}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Description>]   {AE}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Amount>]   {AU}-[<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Units>]
{G}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Description>]   {AE}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Amount>]   {AU}-[<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Units>]
{I}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Description>]   {AE}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Amount>]   {AU}-[<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Units>]
{K}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Description>]   {AE}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Amount>]   {AU}-[<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.Units>]
{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Charge>]   {X}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Currency.Code>]   {AA}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[1]>]   {AE}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[2]>]   {AI}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[3]>]   {AM}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[4]>]   {AQ}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[5]>]   {AU}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[6]>]   {AY}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[7]>]   {BC}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].TransitTime>]   {BI}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Frequency>]
{C}-[Consignor:]   {M}-[<AutoHeight><HideRowIfCellIsEmpty><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Consignor.CompanyName>]
{C}-[Consignee:]   {M}-[<AutoHeight><HideRowIfCellIsEmpty><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Consignee.CompanyName>]




{C}-[<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.Origin><AddTitleIfNotEmpty("" (to "", ""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.Destination>"", "")"")><AddTitleIfNotEmpty("" - "", ""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.Provider.CompanyName>"", """")>]
{C}-[<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.Destination><AddTitleIfNotEmpty("" (from "", ""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.Origin>"", "")"")><AddTitleIfNotEmpty("" - "", ""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.Provider.CompanyName>"", """")>]


{X}-[Cur.]   {AA}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[1].Heading>]   {AE}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[2].Heading>]   {AI}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[3].Heading>]   {AM}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[4].Heading>]   {AQ}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[5].Heading>]   {AU}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[6].Heading>]   {AY}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[7].Heading>]   {BC}-[Transit Time]   {BI}-[Freq.]







{C}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Direction.Code>"" == ""IMP"", ""Origin"", ""<If(""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Direction.Code>"" == ""EXP"", ""Destination"", ""Origin - Destination"")>"")>]   {M}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Provider.TypeDescription>]   {X}-[Cur.]   {AA}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[1].Heading>]   {AE}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[2].Heading>]   {AI}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[3].Heading>]   {AM}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[4].Heading>]   {AQ}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[5].Heading>]   {AU}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[6].Heading>]   {AY}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[7].Heading>]   {BC}-[Transit Time]   {BI}-[Freq.]


{C}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.SubRow.Index>"" == ""0"", ""<If(""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Direction.Code>"" == ""IMP"", ""<If(""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Origin.IsCountry>"" == ""N"",""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Origin>"", """")>"", ""<If(""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Direction.Code>"" == ""EXP"", ""<If(""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Destination.IsCountry>"" == ""N"",""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Destination>"", """")>"", ""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Origin> - <Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Destination>"")>"")>"", """")><AddTitleIfNotEmpty("" via "", ""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Via>"", """")>]   {M}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Provider.CompanyName>]



{C}-[<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].OverseasCountries>]

{C}-[<If(""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Label>"" == ""DestinationRates"", ""Destination"", ""<If(""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Label>"" == ""OriginRates"", ""Origin"", ""???"")>"")> Charges]



{C}-[<AutoHeight><Rating.PrimarySource>]
{C}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].IsSupplementary>"" == ""N"", ""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].Direction> Air Freight Rates<If(""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].Direction.Code>"" == ""IMP"", "" to <Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].Destination>"", """")><If(""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].Direction.Code>"" == ""EXP"", "" from <Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].Origin>"", """")>"", ""Air Rates"")>]

{C}-[<HideRowIfCellIsEmpty><AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Page.OpeningText>]

{C}-[<If(""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].ServiceLevel>"" == """", """", ""Service Level:"")>]   {M}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].ServiceLevel>]   {AY}-[Quote No: ]   {BE}-[<JobNumber> - <JobHeaderLocalClient.CompanyCode>]
{C}-[<If(""<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].CommodityCode>"" == """", """", ""Commodity:"")>]   {M}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].CommodityCode.Description>]   {AY}-[Validity:]   {BE}-[<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].Format(""{ValidFrom:Date}"")> - <Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].Format(""{ValidUntil:Date}"")>]



{C}-[CFX Information]
{E}-[<Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Page.CFX.Format(""{NameAndValue}"", Newline)>]

{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows].Page.ClosingText>]

{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows].Page.ClosingTaxText>]


{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AE}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>]   {AU}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{E}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AE}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>]   {AU}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{G}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AE}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>]   {AU}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{I}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AE}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>]   {AU}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{K}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AE}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>]   {AU}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{Y}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {BA}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BG}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>]   {BM}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{Z}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {BA}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BG}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>]   {BM}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{AC}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {BA}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BG}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>]   {BM}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{AE}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {BA}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BG}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>]   {BM}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{AG}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {BA}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BG}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>]   {BM}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{C}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Index>"" == ""0"", ""<If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Direction.Code>"" == ""IMP"", ""<If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Origin.IsCountry>"" == ""N"",""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Origin>"", """")>"", ""<If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Direction.Code>"" == ""EXP"", ""<If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Destination.IsCountry>"" == ""N"",""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Destination>"", """")>"", ""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Origin> - <Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Destination>"")>"")>"", """")><AddTitleIfNotEmpty("" via "", ""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Via>"", """")>]   {M}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Provider.CompanyName>]   {X}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>]   {AA}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[1]>]   {AE}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[2]>]   {AI}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[3]>]   {AM}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[4]>]   {AQ}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[5]>]   {AU}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[6]>]   {AY}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[7]>]   {BC}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[8]>]   {BI}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].TransitTime>]   {BO}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Frequency>]   {BU}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].ContractNumber>]
{X}-[Consignor:]   {AI}-[<AutoHeight><HideRowIfCellIsEmpty><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Consignor.CompanyName>]
{X}-[Consignee:]   {AI}-[<AutoHeight><HideRowIfCellIsEmpty><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Consignee.CompanyName>]




{C}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Mode>]
{Y}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Mode>]


{C}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Origin><AddTitleIfNotEmpty("" (to "", ""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Destination>"", "")"")><AddTitleIfNotEmpty("" - "", ""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Provider.CompanyName>"", """")>]
{C}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Destination><AddTitleIfNotEmpty("" (from "", ""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Origin>"", "")"")><AddTitleIfNotEmpty("" - "", ""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Provider.CompanyName>"", """")>]




{C}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].OverseasCountries>]

{C}-[<If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Direction.Code>"" == ""IMP"", ""Origin"", ""<If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Direction.Code>"" == ""EXP"", ""Destination"", ""Origin - Destination"")>"")>]   {M}-[Shipping Line]   {X}-[Cur.]   {AA}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[1].Heading>]   {AE}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[2].Heading>]   {AI}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[3].Heading>]   {AM}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[4].Heading>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[5].Heading>]   {AU}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[6].Heading>]   {AX}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[7].Heading>]   {BC}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[8].Heading>]   {BI}-[Transit Time]   {BO}-[Freq.]   {BU}-[Contract No.]
{C}-[<If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Label>"" == ""DestinationRates"", ""Destination"", ""<If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Label>"" == ""OriginRates"", ""Origin"", ""???"")>"")> Charges]



{C}-[<AutoHeight><Rating.PrimarySource>]
{C}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].IsSupplementary>"" == ""N"", ""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].Direction> <Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].TransportMode.Description> Freight Rates<If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].Direction.Code>"" == ""IMP"", "" to <Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].Destination>"", """")><If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].Direction.Code>"" == ""EXP"", "" from <Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].Origin>"", """")>"", ""Non-Freight Rates"")>]

{C}-[<HideRowIfCellIsEmpty><AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Page.OpeningText>]

{C}-[Service Level:]   {M}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].ServiceLevel>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].ServiceLevel>"")>]   {AY}-[Quote No: ]   {BE}-[<JobNumber> - <JobHeaderLocalClient.CompanyCode>]
{C}-[<If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].CommodityCode>"" == """", """", ""Commodity:"")>]   {M}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].CommodityCode>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].CommodityCode.Description>"")>]   {AY}-[Validity:]   {BE}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].Format(""{ValidFrom:Date}"")> - <Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].Format(""{ValidUntil:Date}"")>]



{C}-[CFX Information]
{E}-[<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Page.CFX.Format(""{NameAndValue}"", Newline)>]

{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows].Page.ClosingText>]

{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows].Page.ClosingTaxText>]");
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_ForwardingLandscapePricingPage_ShouldShowCorrectAmountAndFormattingForEUR_DocBuilder()
		{
			var company = (Enterprise.MasterFiles.Integration.IGlbCompany)EnvProxy.Instance.CurrentCompany;
			using (company.TemporarilySetCountry("IT"))
			{
				TestHelper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);

				Factory.Save();

				var client = TestHelper.NewOrgHeader(1);
				var quote = TestHelper.NewQuote(client);
				var rateEntry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LCL, "AUSYD", "USLAX", "CHARGE1", 1.0011m, currency: "EUR");

				RunDocumentWithAllSections = ZBool.False;

				FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Landscape Pricing Page");

				var sellRatesDecimals = new SellRatesDecimalsCollection
				{
					new SellRatesDecimals { Code = "ALL", Decimals = "3" }
				};
				using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sellRatesDecimals))
				{
					AssertRunDocument
					(
						quote,
						expectedOutput: @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Cross Trade Air Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-20 - 01-Feb-20]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Australia - United States]

{C}-[Origin - Destination]   {O}-[Airline]
{X}-[Cur.]   {AA}-[Flat]   {BC}-[Transit Time]   {BI}-[Freq.]   {BO}-[W/V Conv.]
{C}-[Sydney - Los Angeles]


{C}-[CHARGE1 - Charge Description 1]   {X}-[EUR]   {AA}-[1,00]


{C}-[END OF DOCUMENT ]",
						message: "Should show all rates where amount should be formatted with RateLine currency and percentage should be formatted like GUI."
					);
				}
			}
		}

		#endregion

		#region Forwarding Standard Pricing page

		[TestDate(2020, 1, 1)]
		public void TestDocument_ForwardingStandardPricingPage_DocBuilder_EmptyContainerSimilarDestinationFreightRates()
		{
			var client = TestHelper.NewOrgHeader(1);
			var quote = TestHelper.NewQuote(client);

			var rateEntry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "NZAKL", "FRT", 10m);
			rateEntry.TI_RH_NKCommodityCode = "";

			var rateEntry1 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LSE, "AUSYD", "NZAKL", "DDOC", 20m);
			rateEntry1.TI_RH_NKCommodityCode = "";
			rateEntry1.TI_RS_NKServiceLevel_NI = "AM";

			var rateEntry2 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LSE, "AUSYD", "NZAKL", "DDOC", 21m);
			rateEntry2.TI_RH_NKCommodityCode = "";
			rateEntry2.TI_RS_NKServiceLevel_NI = "XX";

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Landscape Pricing Page");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: @"{AK}-[Quotation for Test Client #2]   {BL}-[Page 1 of 1]


{AK}-[Cross Trade Air Freight Rates]

{AK}-[Quote No:]   {AS}-[998 - TESTORG2]
{AK}-[Validity:]   {AS}-[01-Jan-20 - 01-Feb-20]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[Not Specified]




{C}-[Australia - New Zealand]

{C}-[Origin - Destination]   {O}-[Airline]
{X}-[Cur.]   {AA}-[Flat]   {BC}-[Transit Time]   {BI}-[Freq.]   {BO}-[W/V Conv.]
{C}-[Sydney - Auckland]


{C}-[FRT - International Freight]   {X}-[AUD]   {AA}-[10.00]

{C}-[Destination Charges]

{C}-[Auckland (from Sydney)]

{C}-[Destination Documentation Fee]
{E}-[ AM]   {AE}-[NZD]   {AM}-[20.00]
{E}-[ XX]   {AE}-[NZD]   {AM}-[21.00]


{C}-[END OF DOCUMENT ]"
				);
			}
		}

		#endregion

		#region Warehouse Rates Pricing Page

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTemplate_WarehouseRatesPricingPage()
		{
			AssertLegacyTemplate
			(
				@"Enterprise\Product\Documents\ExcelTemplates\Documents\Warehouse Rates.xls",
				@"{A}-[#config]
{A}-[Name=WarehouseRates]
{A}-[DataContext=WarehouseRating]
{A}-[#DocumentHeader]
{C}-[<Image(.Logo, 1,47)>]

{C}-[<AutoHeight><MainHeading>]

{A}-[#if ""<IsQuote>"" == ""Y""]
{C}-[<QuotationTitleUpper> DETAILS FOR <ClientFullNameUpper>]

{C}-[<HideRowIfCellIsEmpty><AutoHeight><PageOpeningText>]
{C}-[Validity:]   {G}-[<ValidFrom> - <ValidUntil>]   {AA}-[Quote No:]   {AE}-[<QuoteNumberAndClientCode>]

{A}-[#endif]
{A}-[#SectionPageHeader]
{A}-[#SectionBody:Data=OriginDocRateLineItems]
{C}-[<AutoHeight><OriginDocRateLineItems.Description>]   {Y}-[<OriginDocRateLineItems.Currency>]   {AA}-[<FormatNumber(<OriginDocRateLineItems.Amount>, <If(""<OriginDocRateLineItems.Currency>"" != """", ""<OriginDocRateLineItems.Currency>"", ""<OriginDocRateLineItems.DecimalPlaces>"")>)>]   {AG}-[<OriginDocRateLineItems.Units>]   {AO}-[<OriginDocRateLineItems.Validity>]   {AY}-[<HideRowIf(""<OriginDocRateLineItems.NumOfIndents>"" != ""0"")>]
{E}-[<AutoHeight><OriginDocRateLineItems.Description>]   {Y}-[<OriginDocRateLineItems.Currency>]   {AA}-[<FormatNumber(<OriginDocRateLineItems.Amount>, <If(""<OriginDocRateLineItems.Currency>"" != """", ""<OriginDocRateLineItems.Currency>"", ""<OriginDocRateLineItems.DecimalPlaces>"")>)>]   {AG}-[<OriginDocRateLineItems.Units>]   {AO}-[<OriginDocRateLineItems.Validity>]   {AY}-[<HideRowIf(""<OriginDocRateLineItems.NumOfIndents>"" != ""1"")>]
{G}-[<AutoHeight><OriginDocRateLineItems.Description>]   {Y}-[<OriginDocRateLineItems.Currency>]   {AA}-[<FormatNumber(<OriginDocRateLineItems.Amount>, <If(""<OriginDocRateLineItems.Currency>"" != """", ""<OriginDocRateLineItems.Currency>"", ""<OriginDocRateLineItems.DecimalPlaces>"")>)>]   {AG}-[<OriginDocRateLineItems.Units>]   {AO}-[<OriginDocRateLineItems.Validity>]   {AY}-[<HideRowIf(""<OriginDocRateLineItems.NumOfIndents>"" != ""2"")>]
{I}-[<AutoHeight><OriginDocRateLineItems.Description>]   {Y}-[<OriginDocRateLineItems.Currency>]   {AA}-[<FormatNumber(<OriginDocRateLineItems.Amount>, <If(""<OriginDocRateLineItems.Currency>"" != """", ""<OriginDocRateLineItems.Currency>"", ""<OriginDocRateLineItems.DecimalPlaces>"")>)>]   {AG}-[<OriginDocRateLineItems.Units>]   {AO}-[<OriginDocRateLineItems.Validity>]   {AY}-[<HideRowIf(""<OriginDocRateLineItems.NumOfIndents>"" != ""3"")>]
{K}-[<AutoHeight><OriginDocRateLineItems.Description>]   {Y}-[<OriginDocRateLineItems.Currency>]   {AA}-[<FormatNumber(<OriginDocRateLineItems.Amount>, <If(""<OriginDocRateLineItems.Currency>"" != """", ""<OriginDocRateLineItems.Currency>"", ""<OriginDocRateLineItems.DecimalPlaces>"")>)>]   {AG}-[<OriginDocRateLineItems.Units>]   {AO}-[<OriginDocRateLineItems.Validity>]   {AY}-[<HideRowIf(""<OriginDocRateLineItems.NumOfIndents>"" != ""4"")>]
{A}-[#GroupBy:OriginDocRateLineItems.Warehouse:GroupTitle]

{C}-[<OriginDocRateLineItems.Warehouse>]
{A}-[#GroupBy:OriginDocRateLineItems.Warehouse]
{A}-[#SectionFooter]
{A}-[#PageHeader:StartFromSecondPage]

{C}-[<QuotationTitleUpper> DETAILS FOR <ClientFullNameUpper>]   {AS}-[Continued …]

{A}-[#PageFooter]
{AQ}-[Continued Over…]
{A}-[#DocumentFooter]

{C}-[<HideRowIfCellIsEmpty><AutoHeight><PageClosingText>]
{A}-[#EndOfReport]"
			);
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_WarehouseRatesPricingPage()
		{
			TestHelper.ChargeCodes.New("CHARGEWHS1", "Charge Warehouse Description 1", FlatCalculator.Code, "WOU");
			TestHelper.ChargeCodes.New("CHARGEWHS2", "Charge Warehouse Description 2", FlatCalculator.Code, "WOU");

			Factory.Save();

			var client = TestHelper.NewOrgHeader(1);
			var quote = TestHelper.NewQuote(client);

			var warehouseRateEntry = quote.AddRateEntry(RatingConstants.RateCategory.WHS);
			warehouseRateEntry.AddFlatRateLine("CHARGEWHS1", 2.12345, "LYD");

			var warehouseRateLine = warehouseRateEntry.AddRateLine("CHARGEWHS2", PercentageCalculator.Code, currencyCode: "LYD");
			warehouseRateLine.GetCalculator<PercentageCalculator>().Percent = 10.12345;
			var warehouseRateLineItem = warehouseRateLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			warehouseRateLineItem.TM_Text = "ALL";

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Warehouse Rates Pricing Page");

			var sellRatesDecimals = new SellRatesDecimalsCollection
			{
				new SellRatesDecimals { Code = "ALL", Decimals = "4" }
			};
			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sellRatesDecimals))
			{
				AssertRunDocumentLegacy
				(
					quote,
					expectedOutput: @"{C}-[QUOTATION DETAILS FOR TEST CLIENT #2]

{C}-[Validity:]   {G}-[1 Jan 2020 - 1 Feb 2020]   {AA}-[Quote No:]   {AE}-[998 - TESTORG2]


{C}-[All Warehouses]
{C}-[Charge Warehouse Description 1]   {Y}-[LYD]   {AA}-[2.124]
{C}-[Charge Warehouse Description 2]   {AA}-[10.1235]   {AG}-[% of all charges]",
					message: "Rate amount should be formatted with RateLine currency and percent should be displayed like GUI."
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestForwardingStandardPricingPage_WhenHavingRateEntriesWithDifferentIncoterms_ThenOnlyPrintClientChargedRates()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "SGSIN";
			org.OH_Code = "TESTORG";

			var quote = Factory.New<Quote>();
			quote.TH_OH = org.PK;

			var entry1 = AddUnitCN(quote, RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "SGSIN", "20GP", "FRT", 10m, Factory);
			entry1.TI_QuotePageIncoTerm = "FOB";
			var entry2 = AddUnitCN(quote, RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "SGSIN", "20FR", "FRT", 20m, Factory);
			entry2.TI_QuotePageIncoTerm = "CFR";
			var entry3 = AddUnitCN(quote, RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "SGSIN", "40GP", "FRT", 30m, Factory);
			entry3.TI_QuotePageIncoTerm = "FOB";
			var entry4 = AddUnitCN(quote, RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "SGSIN", "40FR", "FRT", 40m, Factory);
			entry4.TI_QuotePageIncoTerm = "CFR";

			Factory.Save();

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Standard Pricing Page");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: @"{C}-[Forwarding Standard Pricing Page]   {AO}-[Page 1 of 1]

{C}-[FCL Freight from Sydney to Singapore (FOB)]

{C}-[Quote No:]   {I}-[1000 - TESTORG]   {AA}-[Validity:]   {AI}-[01-Jan-20  -  01-Feb-20]
{C}-[Frequency:]   {I}-[Not Specified]   {AA}-[Transit Time:]   {AI}-[Not Specified]
{C}-[Service Level:]   {I}-[Not Specified]   {AA}-[Shipping Line:]   {AI}-[Not Specified]
{C}-[Commodity:]   {I}-[GEN - General]




{C}-[FCL Freight from Sydney to Singapore]   {AE}-[Currency]   {AI}-[Rate]

{C}-[FRT charge]   {AD}-[AUD]   {AH}-[10.00]   {AM}-[per 20GP Container]
{C}-[FRT charge]   {AD}-[AUD]   {AH}-[30.00]   {AM}-[per 40GP Container]






{C}-[END OF DOCUMENT]",
					message: "GIVEN quotation with rate entries with different Incoterm WHEN print standard page THEN only print client charged rates"
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestForwardingLandscapePricingPage_WhenHavingRateEntriesWithDifferentIncoterms_ThenOnlyPrintClientChargedRates()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "SGSIN";
			org.OH_Code = "TESTORG";

			var quote = Factory.New<Quote>();
			quote.TH_OH = org.PK;

			var entry1 = AddUnitCN(quote, RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "SGSIN", "20GP", "FRT", 10m, Factory);
			entry1.TI_QuotePageIncoTerm = "FOB";
			var entry2 = AddUnitCN(quote, RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "SGSIN", "20FR", "FRT", 20m, Factory);
			entry2.TI_QuotePageIncoTerm = "CFR";
			var entry3 = AddUnitCN(quote, RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "SGSIN", "40GP", "FRT", 30m, Factory);
			entry3.TI_QuotePageIncoTerm = "FOB";
			var entry4 = AddUnitCN(quote, RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "SGSIN", "40FR", "FRT", 40m, Factory);
			entry4.TI_QuotePageIncoTerm = "CFR";

			Factory.Save();

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Landscape Pricing Page");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: @"{AK}-[Quotation for ]   {BL}-[Page 1 of 1]


{AK}-[Export Sea Freight Rates from Sydney]

{AK}-[Quote No:]   {AS}-[1000 - TESTORG]
{AK}-[Validity:]   {AS}-[01-Jan-20 - 01-Feb-20]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]




{C}-[Singapore]

{C}-[Destination]   {K}-[Shipping Line]   {S}-[Cur.]   {U}-[20GP]   {Y}-[20FR]   {AC}-[40GP]   {AG}-[40FR]   {AW}-[Transit Time]   {BA}-[Freq.]   {BG}-[W/V Conv.]   {BM}-[Contract No.]

{C}-[Singapore]   {S}-[AUD]   {U}-[10.00]   {AC}-[30.00]   {BG}-[1000 KG/M3]


{C}-[END OF DOCUMENT ]",
					message: "GIVEN quotation with rate entries with different Incoterm WHEN print landscape page THEN only print client charged rates"
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestForwardingCompactPricingPage_WhenHavingRateEntriesWithDifferentIncoterms_ThenOnlyPrintClientChargedRates()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "SGSIN";
			org.OH_Code = "TESTORG";

			var quote = Factory.New<Quote>();
			quote.TH_OH = org.PK;

			var entry1 = AddUnitCN(quote, RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "SGSIN", "20GP", "FRT", 10m, Factory);
			entry1.TI_QuotePageIncoTerm = "FOB";
			var entry2 = AddUnitCN(quote, RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "SGSIN", "20FR", "FRT", 20m, Factory);
			entry2.TI_QuotePageIncoTerm = "CFR";
			var entry3 = AddUnitCN(quote, RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "SGSIN", "40GP", "FRT", 30m, Factory);
			entry3.TI_QuotePageIncoTerm = "FOB";
			var entry4 = AddUnitCN(quote, RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "SGSIN", "40FR", "FRT", 40m, Factory);
			entry4.TI_QuotePageIncoTerm = "CFR";

			Factory.Save();

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Compact Pricing Page");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					quote,
					expectedOutput: @"{AK}-[Quotation for ]   {BL}-[Page 1 of 1]


{AK}-[Forwarding Rates]

{AK}-[Quote No:]   {AS}-[1000 - TESTORG]
{AK}-[Validity:]   {AS}-[01-Jan-20 - 01-Feb-20]
{AK}-[Service Level:]   {AS}-[Not Specified]
{AK}-[Commodity:]   {AS}-[General]
{AK}-[Shipping Line:]   {AS}-[Not Specified]




{C}-[Freight Rates]

{C}-[Origin]   {I}-[Destination]   {O}-[Tranship Port]   {U}-[Container Type]   {Y}-[FRT|>(AUD/CN)]   {BI}-[Frequency]   {BO}-[Transport Time]
{C}-[Sydney]   {I}-[Singapore]   {U}-[20FR]
{U}-[20GP]   {Y}-[10.00]
{U}-[40FR]
{U}-[40GP]   {Y}-[30.00]


{C}-[END OF DOCUMENT ]",
					message: "GIVEN quotation with rate entries with different Incoterm WHEN print compact page THEN only print client charged rates"
				);
			}
		}

		#endregion

		#region Helper

		static RateEntry AddUnitCN(RatingHeader header, string category, string mode, string origin, string destination, string containerType, string charge, decimal amount, BusinessObjectFactory factory)
		{
			var containerPK = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType).PK;

			RateEntry entry = header.ChildRateEntries
				.Cast<RateEntry>()
				.SingleOrDefault(x => x.TI_RateCategory == category
									  && x.TI_Mode == mode
									  && x.TI_OriginLRC == origin
									  && x.TI_DestinationLRC == destination
									  && x.TI_RC == containerPK);
			if (entry == null)
			{
				entry = header.AddRateEntry(category, mode, origin, destination, "", containerType);
				entry.RateLines.RemoveAndDeleteAll();
			}

			RateLine line = entry.AddRateLine(charge, UnitCalculator.Code, RatingConstants.Units.CN, "AUD");
			line.TL_RateDesc = charge + " charge";
			((UnitCalculator)line.Calculator).PerUnit = amount;

			return entry;
		}

		#endregion

		#region Implementation

		public override BusinessObject GetBusinessObject => Quote;

		public override BusinessContext BusinessContext => BusinessContext.Quotation;

		protected override void SetUp()
		{
			base.SetUp();
			var client = TestHelper.NewOrgHeader(1);
			Quote = TestHelper.NewQuote(client);
		}

		Quote Quote;

		TestHelper TestHelper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;

		#endregion
	}
}
