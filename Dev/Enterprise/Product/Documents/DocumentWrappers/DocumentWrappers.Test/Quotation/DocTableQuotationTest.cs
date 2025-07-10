using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	sealed class DocTableQuotationTest : RatingTestCase
	{
		public void TestSortCollection()
		{
			RunSortCollectionTest(AccClientInvoiceOrderLookups.InvoiceTypes.All.Code);
			RunSortCollectionTest(ZString.Empty);
		}

		void RunSortCollectionTest(ZString invoiceType)
		{
			AccChargeCode code1 = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "EFAF"));
			code1.AC_PrintSequence = 3;

			AccChargeCode code2 = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "OFORW"));
			code2.AC_PrintSequence = 1;

			AccChargeCode code3 = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FSC"));
			code3.AC_PrintSequence = 4;

			AccChargeCode code4 = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "WAR"));
			code4.AC_PrintSequence = 5;

			OrgHeader testClient = Factory.NewWithValidTestData<OrgHeader>();
			testClient.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();

			OrgInvoiceRollupOrGroup group = testClient.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = JobInvoicingConsumerTypes.AgencyDetentionInvoice.Code;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;

			Quote quote = Helper.NewQuote(testClient);

			RateEntry entry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "ER", "AU");
			entry.TI_RateCategory = RatingConstants.RateCategory.GetRateCategories(RateType.ShippingImportDetention, RateCategoryGroup.Destination)[0];

			PricingPage formatTable = new PricingPage(entry, Factory, PricingPageStyle.Landscape);

			RateLine line1 = entry.RateLines.AddNew();
			line1.TL_AC = code1.PK;
			line1.TL_RateCalculator = FlatCalculator.Code;
			line1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)200m;

			RateLine line2 = entry.RateLines.AddNew();
			line2.TL_AC = code2.PK;
			line2.TL_RateCalculator = FlatCalculator.Code;
			line2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)100m;

			RateLine line3 = entry.RateLines.AddNew();
			line3.TL_AC = code3.PK;
			line3.TL_RateCalculator = FlatCalculator.Code;
			line3.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)300m;

			RateLine line4 = entry.RateLines.AddNew();
			line4.TL_AC = code4.PK;
			line4.TL_RateCalculator = FlatCalculator.Code;
			line4.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)400m;

			AccClientInvoiceOrder order4 = testClient.InvoiceOrders.AddNew();
			order4.AI_InvoiceType = invoiceType;
			order4.AI_AC = code4.PK;
			order4.AI_PrintOrder = 2;

			Factory.Save();

			DocTableQuotation quotation = DocTableQuotation.New(formatTable, Factory);

			AssertEquals(code1.AC_Desc, quotation.DestinationDocRateLineItems[0].Description); // (3) Emergency Fuel Adjustment Factor
			AssertEquals(code2.AC_Desc, quotation.DestinationDocRateLineItems[1].Description); // (1) Onforwarding Charges
			AssertEquals(code3.AC_Desc, quotation.DestinationDocRateLineItems[2].Description); // (4) Fuel Surcharge
			AssertEquals(code4.AC_Desc, quotation.DestinationDocRateLineItems[3].Description); // (2) War Risk Surcharge

			group.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.Sequence;
			Factory.Save();
			quotation = DocTableQuotation.New(formatTable, Factory);

			AssertEquals(code2.AC_Desc, quotation.DestinationDocRateLineItems[0].Description); // (1) Onforwarding Charges
			AssertEquals(code4.AC_Desc, quotation.DestinationDocRateLineItems[1].Description); // (2) War Risk Surcharge
			AssertEquals(code1.AC_Desc, quotation.DestinationDocRateLineItems[2].Description); // (3) Emergency Fuel Adjustment Factor
			AssertEquals(code3.AC_Desc, quotation.DestinationDocRateLineItems[3].Description); // (4) Fuel Surcharge

			group.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical;
			Factory.Save();
			quotation = DocTableQuotation.New(formatTable, Factory);

			AssertEquals(code1.AC_Desc, quotation.DestinationDocRateLineItems[0].Description); // (3) Emergency Fuel Adjustment Factor
			AssertEquals(code3.AC_Desc, quotation.DestinationDocRateLineItems[1].Description); // (4) Fuel Surcharge
			AssertEquals(code2.AC_Desc, quotation.DestinationDocRateLineItems[2].Description); // (1) Onforwarding Charges
			AssertEquals(code4.AC_Desc, quotation.DestinationDocRateLineItems[3].Description); // (2) War Risk Surcharge

			group.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.User;
			Factory.Save();
			quotation = DocTableQuotation.New(formatTable, Factory);

			AssertEquals(code1.AC_Desc, quotation.DestinationDocRateLineItems[0].Description); // (3) Emergency Fuel Adjustment Factor
			AssertEquals(code2.AC_Desc, quotation.DestinationDocRateLineItems[1].Description); // (1) Onforwarding Charges
			AssertEquals(code3.AC_Desc, quotation.DestinationDocRateLineItems[2].Description); // (4) Fuel Surcharge
			AssertEquals(code4.AC_Desc, quotation.DestinationDocRateLineItems[3].Description); // (2) War Risk Surcharge

			group.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence;
			Factory.Save();
			quotation = DocTableQuotation.New(formatTable, Factory);

			AssertEquals(code2.AC_Desc, quotation.DestinationDocRateLineItems[0].Description); // (1) Onforwarding Charges
			AssertEquals(code4.AC_Desc, quotation.DestinationDocRateLineItems[1].Description); // (2) War Risk Surcharge
			AssertEquals(code1.AC_Desc, quotation.DestinationDocRateLineItems[2].Description); // (3) Emergency Fuel Adjustment Factor
			AssertEquals(code3.AC_Desc, quotation.DestinationDocRateLineItems[3].Description); // (4) Fuel Surcharge

			group.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.SubTotal;
			Factory.Save();
			quotation = DocTableQuotation.New(formatTable, Factory);

			AssertEquals(code1.AC_Desc, quotation.DestinationDocRateLineItems[0].Description); // (3) Emergency Fuel Adjustment Factor
			AssertEquals(code2.AC_Desc, quotation.DestinationDocRateLineItems[1].Description); // (1) Onforwarding Charges
			AssertEquals(code3.AC_Desc, quotation.DestinationDocRateLineItems[2].Description); // (4) Fuel Surcharge
			AssertEquals(code4.AC_Desc, quotation.DestinationDocRateLineItems[3].Description); // (2) War Risk Surcharge

			group.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			Factory.Save();
			quotation = DocTableQuotation.New(formatTable, Factory);

			AssertEquals(code1.AC_Desc, quotation.DestinationDocRateLineItems[0].Description); // (3) Emergency Fuel Adjustment Factor
			AssertEquals(code2.AC_Desc, quotation.DestinationDocRateLineItems[1].Description); // (1) Onforwarding Charges
			AssertEquals(code3.AC_Desc, quotation.DestinationDocRateLineItems[2].Description); // (4) Fuel Surcharge
			AssertEquals(code4.AC_Desc, quotation.DestinationDocRateLineItems[3].Description); // (2) War Risk Surcharge

			group.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUpEntireConsol;
			Factory.Save();
			quotation = DocTableQuotation.New(formatTable, Factory);

			AssertEquals(code1.AC_Desc, quotation.DestinationDocRateLineItems[0].Description); // (3) Emergency Fuel Adjustment Factor
			AssertEquals(code2.AC_Desc, quotation.DestinationDocRateLineItems[1].Description); // (1) Onforwarding Charges
			AssertEquals(code3.AC_Desc, quotation.DestinationDocRateLineItems[2].Description); // (4) Fuel Surcharge
			AssertEquals(code4.AC_Desc, quotation.DestinationDocRateLineItems[3].Description); // (2) War Risk Surcharge
		}

		public void TestEntries()
		{
			AccChargeCode code = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FSC"));

			RateEntry testEntry = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			RateLine testRateLine = testEntry.RateLines.AddNew();
			testRateLine.TL_AC = code.PK;
			testRateLine.TL_RateCalculator = FlatCalculator.Code;
			testRateLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)100m;

			PricingPage testFormatTable = new PricingPage(testEntry, Factory, PricingPageStyle.Landscape);
			DocTableQuotation testQuotation = DocTableQuotation.New(testFormatTable, Factory);

			testQuotation.SetReportNameForTesting("Test report");
			AssertEquals(false, testQuotation.Entries[0].ViewAgentRates);
			testQuotation = DocTableQuotation.New(testFormatTable, Factory);
			testQuotation.SetReportNameForTesting("Test report Agent");
			AssertEquals(true, testQuotation.Entries[0].ViewAgentRates);
		}

		public void TestImportAirFreight()
		{
			GlbCompany.CurrentCompany.SetCountry("GB");

			#region Freight Charges

			RateEntry entry = quote.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON", "STD", "");
			entry.TI_RX_NKCurrency = "EUR";
			entry.RateLines.RemoveAndDeleteAll();
			entry.TI_Frequency = 3;
			entry.TI_FrequencyUnit = RatingConstants.FrequencyUnits.Week;
			entry.TI_PageOpeningText = "Test Opening";
			entry.TI_PageClosingText = "Test Closing";

			RateLine line = entry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG);
			line.RateLineItems.RemoveAndDeleteAll();
			((CombinedCalculator)line.Calculator).Minimum = 100m;
			line.Calculator["-45"] = (ZDecimal)5m;
			line.Calculator["+45"] = (ZDecimal)4m;
			line.Calculator["+1000"] = (ZDecimal)3m;

			RateLine line_2 = entry.AddRateLine(TestCAF.AC_Code, UnitCalculator.Code, QuantityUnit.M3);
			((UnitCalculator)line_2.Calculator).PerUnit = 0.55m;

			RateLine line_3 = entry.AddRateLine(TestBAF.AC_Code, TimeCalculator.Code, QuantityUnit.KG);
			line_3.RateLineItems.RemoveAndDeleteAll();
			((TimeCalculator)line_3.Calculator).Minimum = 100m;
			line_3.Calculator["-55"] = (ZDecimal)50m;
			line_3.Calculator["+55"] = (ZDecimal)40m;
			line_3.Calculator["+900"] = (ZDecimal)30m;
			((TimeCalculator)line_3.Calculator).ExcludeHolidays = TimeCalculator.Items.ExcludeWeekendsAndPublicHolidays;

			#endregion

			#region Origin Charges

			RateEntry entryOrigin = quote.AddRateEntry("ORG", "AIR", "AUSYD", "GB", "STD", "");
			entryOrigin.TI_RX_NKCurrency = "EUR";
			RateLine lineOrigin = entryOrigin.AddRateLine(TestAWB.AC_Code, CombinedCalculator.Code, QuantityUnit.KG);
			((CombinedCalculator)lineOrigin.Calculator).Minimum = 49m;
			((CombinedCalculator)lineOrigin.Calculator).PerUnit = 2m;

			#endregion

			#region Destination Charges

			RateEntry entryDestination = quote.AddRateEntry("DST", "AIR", "", "GBLON", "STD", "");
			RateLine lineDestination = entryDestination.AddRateLine(TestADF.AC_Code, FlatCalculator.Code);
			((FlatCalculator)lineDestination.Calculator).BaseRate = 12m;

			#endregion

			PricingPage formatTable = new PricingPage(entry, Factory, PricingPageStyle.Landscape);
			DocTableQuotation quotation = DocTableQuotation.New(formatTable, Factory);
			Factory.Save();
			AssertEquals("Page Headng", "Import Air Freight Rates to London", quotation.PageHeading);
			AssertEquals("Service Level", "Standard", quotation.ServiceLevel);
			AssertEquals("Commodity Code", "General", quotation.CommodityCode);
			AssertEquals("QuoteNo", "999/A - NEWTESSYD", quotation.QuoteNumberAndClientCode);
			AssertEquals("Validity", ZDateTime.Today.AddMonths(1).ToString("d MMM yyyy"), quotation.ValidUntil);

			AssertEquals("Header1", "Origin", quotation.Entries[0].Header1);
			AssertEquals("Header2", "Airline", quotation.Entries[0].Header2);
			AssertEquals("Header3", "", quotation.Entries[0].Header3);
			AssertEquals("Header4", "MIN", quotation.Entries[0].GroupLCLHeader4);
			AssertEquals("Header5", "-45 per KG", quotation.Entries[0].GroupLCLHeader5);
			AssertEquals("Header6", "+45 per KG", quotation.Entries[0].GroupLCLHeader6);
			AssertEquals("Header7", "+1000 per KG", quotation.Entries[0].GroupLCLHeader7);
			AssertEquals("Header8", "", quotation.Entries[0].GroupLCLHeader8);
			AssertEquals("Header9", "", quotation.Entries[0].GroupLCLHeader9);
			AssertEquals("Header10", "", quotation.Entries[0].GroupLCLHeader10);
			AssertEquals("Header11", "", quotation.Entries[0].GroupLCLHeader11);
			AssertEquals("Header12", "W/V conv.", quotation.Entries[0].Header12);
			AssertEquals("Header13", "Transit Time", quotation.Entries[0].Header13);
			AssertEquals("Header14", "Freq.", quotation.Entries[0].Header14);

			AssertEquals("Charge Code", "International Freight", quotation.Entries[0].ChargeCode);
			AssertEquals("Column1", "Sydney", quotation.Entries[0].Column1);
			AssertEquals("Column2", "", quotation.Entries[0].Column2);
			AssertEquals("Column3", "EUR", quotation.Entries[0].Column3);
			AssertEquals("Column4", "100.00", quotation.Entries[0].Column4);
			AssertEquals("Column5", "5.00", quotation.Entries[0].Column5);
			AssertEquals("Column6", "4.00", quotation.Entries[0].Column6);
			AssertEquals("Column7", "3.00", quotation.Entries[0].Column7);
			AssertEquals("Column8", "", quotation.Entries[0].Column8);
			AssertEquals("Column9", "", quotation.Entries[0].Column9);
			AssertEquals("Column10", "", quotation.Entries[0].Column10);
			AssertEquals("Column11", "", quotation.Entries[0].Column11);
			AssertEquals("Column12", "6000 CC/KG", quotation.Entries[0].Column12);
			AssertEquals("Column13", "", quotation.Entries[0].Column13);
			AssertEquals("Column14", "3 per Week", quotation.Entries[0].Column14);
			Assert("Other Charges", quotation.Entries[0].OtherCharges.IsEmpty);

			AssertEquals("Header1", "Origin", quotation.Entries[1].Header1);
			AssertEquals("Header2", "Airline", quotation.Entries[1].Header2);
			AssertEquals("Header3", "", quotation.Entries[1].Header3);
			AssertEquals("Header4", "per M3", quotation.Entries[1].GroupLCLHeader4);
			AssertEquals("Header5", "", quotation.Entries[1].GroupLCLHeader5);
			AssertEquals("Header6", "", quotation.Entries[1].GroupLCLHeader6);
			AssertEquals("Header7", "", quotation.Entries[1].GroupLCLHeader7);
			AssertEquals("Header8", "", quotation.Entries[1].GroupLCLHeader8);
			AssertEquals("Header9", "", quotation.Entries[1].GroupLCLHeader9);
			AssertEquals("Header10", "", quotation.Entries[1].GroupLCLHeader10);
			AssertEquals("Header11", "", quotation.Entries[1].GroupLCLHeader11);
			AssertEquals("Header12", "W/V conv.", quotation.Entries[1].Header12);
			AssertEquals("Header13", "Transit Time", quotation.Entries[1].Header13);
			AssertEquals("Header14", "Freq.", quotation.Entries[1].Header14);

			AssertEquals("Charge Code", "Test CAF", quotation.Entries[1].ChargeCode);
			AssertEquals("Column1", "Sydney", quotation.Entries[1].Column1);
			AssertEquals("Column2", "", quotation.Entries[1].Column2);
			AssertEquals("Column3", "EUR", quotation.Entries[1].Column3);
			AssertEquals("Column4", "0.55", quotation.Entries[1].Column4);
			AssertEquals("Column5", "", quotation.Entries[1].Column5);
			AssertEquals("Column6", "", quotation.Entries[1].Column6);
			AssertEquals("Column7", "", quotation.Entries[1].Column7);
			AssertEquals("Column8", "", quotation.Entries[1].Column8);
			AssertEquals("Column9", "", quotation.Entries[1].Column9);
			AssertEquals("Column10", "", quotation.Entries[1].Column10);
			AssertEquals("Column11", "", quotation.Entries[1].Column11);
			AssertEquals("Column12", "6000 CC/KG", quotation.Entries[1].Column12);
			AssertEquals("Column13", "", quotation.Entries[1].Column13);
			AssertEquals("Column14", "3 per Week", quotation.Entries[1].Column14);
			Assert("Other Charges", quotation.Entries[1].OtherCharges.IsEmpty);

			Assert(quotation.Entries[0].LCLHeaderGroupIndex != quotation.Entries[1].LCLHeaderGroupIndex);

			Assert(quotation.Entries[2].ChargeCode.IsEmpty);
			AssertEquals("Column1", "Sydney", quotation.Entries[2].Column1);
			AssertEquals("Column2", "", quotation.Entries[2].Column2);

			var expectedOtherCharges = @"Test BAF:
Minimum: EUR 100.00
Less than 55 Day(s): EUR 50.00 per KG x Day
55 Day(s) to less than 900 Day(s): EUR 40.00 per KG x Day
900 Day(s) and above: EUR 30.00 per KG x Day";

			AssertEquals("Other Charges", expectedOtherCharges, quotation.Entries[2].OtherCharges.Trim());

			AssertEquals("Test Opening", quotation.PageOpeningText);
			AssertStartsWith("", "Test Closing", quotation.PageClosingText);

			AssertEquals(1, quotation.LocalDocRateLineItems.Count);
			AssertEquals("Local Charges - London", quotation.LocalDocRateLineItems[0].LocalChargesHeading);
			AssertEquals("Test Airline Doc Fee *", quotation.LocalDocRateLineItems[0].Description);
			AssertEquals("12.00", quotation.LocalDocRateLineItems[0].Amount);
			AssertEquals("GBP", quotation.LocalDocRateLineItems[0].Currency);
			AssertEquals("", quotation.LocalDocRateLineItems[0].Units);

			AssertEquals(3, quotation.OverseasDocRateLineItems.Count);
			AssertEquals("Overseas Charges - Sydney to United Kingdom", quotation.OverseasDocRateLineItems[0].OverseasChargesHeading);
			AssertEquals("Test Airway Bill Fee", quotation.OverseasDocRateLineItems[0].Description);
			AssertEquals("", quotation.OverseasDocRateLineItems[0].Amount);
			AssertEquals("", quotation.OverseasDocRateLineItems[0].Currency);
			AssertEquals("", quotation.OverseasDocRateLineItems[0].Units);
			AssertEquals("Minimum", quotation.OverseasDocRateLineItems[1].Description);
			AssertEquals("49.00", quotation.OverseasDocRateLineItems[1].Amount);
			AssertEquals("EUR", quotation.OverseasDocRateLineItems[1].Currency);
			AssertEquals("", quotation.OverseasDocRateLineItems[1].Units);
			AssertEquals("Per Unit", quotation.OverseasDocRateLineItems[2].Description);
			AssertEquals("2.00", quotation.OverseasDocRateLineItems[2].Amount);
			AssertEquals("EUR", quotation.OverseasDocRateLineItems[2].Currency);
			AssertEquals("per KG / 6000 CC", quotation.OverseasDocRateLineItems[2].Units);
		}

		public void TestExportULDFreight()
		{
			RefContainer aIR1 = RefContainer.New(Factory);
			aIR1.RC_ShippingMode = "AIR";
			aIR1.RC_Code = "AIR1";

			RefContainer aIR2 = RefContainer.New(Factory);
			aIR2.RC_ShippingMode = "AIR";
			aIR2.RC_Code = "AIR2";

			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry("AU");

			#region AIR1

			RateEntry entryAIR1 = quote.AddRateEntry("AIR", "ULD", "AUSYD", "GBLON", "STD", "AIR1");
			entryAIR1.TI_PageOpeningText = "Test Opening";
			entryAIR1.TI_PageClosingText = "Test Closing";
			entryAIR1.TI_ViaLRC = "SGSIN";
			entryAIR1.TI_TransitTime = "7";
			RateLine lineAIR1 = entryAIR1.RateLines[0];
			lineAIR1.TL_RateCalculator = "UNT";
			((UnitCalculator)lineAIR1.Calculator).PerUnit = 2000m;

			RateLine lineAIR1_1 = entryAIR1.AddRateLine("TestBAF", UnitCalculator.Code, QuantityUnit.CN);
			((UnitCalculator)lineAIR1_1.Calculator).PerUnit = 12.5m;

			#endregion

			#region AIR12

			RateEntry entryAIR2 = quote.AddRateEntry("AIR", "ULD", "AUSYD", "GBLON", "STD", "AIR2");
			entryAIR2.TI_ViaLRC = "SGSIN";
			entryAIR2.TI_TransitTime = "7";
			RateLine lineAIR2 = entryAIR2.RateLines[0];
			lineAIR2.TL_RateCalculator = "UNT";
			((UnitCalculator)lineAIR2.Calculator).PerUnit = 1400m;

			RateLine lineAIR2_2 = entryAIR2.AddRateLine("TestBAF", UnitCalculator.Code, QuantityUnit.CN);
			((UnitCalculator)lineAIR2_2.Calculator).PerUnit = 21m;

			#endregion

			#region Origin Charges

			RateEntry entryOrigin1 = quote.AddRateEntry("ORG", "AIR", "AUSYD", "", "STD", "");
			entryOrigin1.TI_RX_NKCurrency = "EUR";
			RateLine lineOrigin1 = entryOrigin1.AddRateLine(TestADF.AC_Code, CombinedCalculator.Code, QuantityUnit.CN);
			((CombinedCalculator)lineOrigin1.Calculator).Minimum = 100m;
			((CombinedCalculator)lineOrigin1.Calculator).PerUnit = 50m;

			RateEntry entryOrigin2 = quote.AddRateEntry("ORG", "ULD", "AUSYD", "", "STD", "");
			entryOrigin2.TI_RX_NKCurrency = "EUR";
			RateLine lineOrigin2 = entryOrigin2.AddRateLine(TestAWB.AC_Code, FlatCalculator.Code);
			((FlatCalculator)lineOrigin2.Calculator).BaseRate = 250m;

			#endregion

			#region Destination Charges

			RateEntry entryDestination = quote.AddRateEntry("DST", "ULD", "", "GB", "STD", "");
			RateLine lineDestination = entryDestination.AddRateLine(TestADF.AC_Code, FlatCalculator.Code);
			((FlatCalculator)lineDestination.Calculator).BaseRate = 12m;

			#endregion

			PricingPageCollection pages = new PricingPageCollection(quote);
			pages.Load(PricingPaginationStrategy.LandscapeComplexStyle);
			DocTableQuotation quotation = DocTableQuotation.New(pages[0], Factory);

			AssertEquals("Page Headng", "Export Air Freight Rates from Sydney", quotation.PageHeading);
			AssertEquals("Service Level", "Standard", quotation.ServiceLevel);
			AssertEquals("Commodity Code", "General", quotation.CommodityCode);
			AssertEquals("QuoteNo", "999 - NEWTESSYD", quotation.QuoteNumberAndClientCode);
			AssertEquals("Validity", ZDateTime.Today.AddMonths(1).ToString("d MMM yyyy"), quotation.ValidUntil);

			AssertEquals("Header1", "Destination", quotation.Entries[0].Header1);
			AssertEquals("Header2", "Airline", quotation.Entries[0].Header2);
			AssertEquals("Header3", "", quotation.Entries[0].Header3);
			AssertEquals("Header4", "AIR1", quotation.Entries[0].Header4);
			AssertEquals("Header5", "AIR2", quotation.Entries[0].Header5);
			AssertEquals("Header6", "", quotation.Entries[0].Header6);
			AssertEquals("Header7", "", quotation.Entries[0].Header7);
			AssertEquals("Header8", "", quotation.Entries[0].Header8);
			AssertEquals("Header9", "", quotation.Entries[0].Header9);
			AssertEquals("Header10", "", quotation.Entries[0].Header10);
			AssertEquals("Header11", "", quotation.Entries[0].Header11);
			AssertEquals("Header12", "", quotation.Entries[0].Header12);
			AssertEquals("Header13", "Transit Time", quotation.Entries[0].Header13);
			AssertEquals("Header14", "Freq.", quotation.Entries[0].Header14);

			AssertEquals("Column1", "London via Singapore", quotation.Entries[0].Column1);
			AssertEquals("Column2", "", quotation.Entries[0].Column2);
			AssertEquals("Column3", "AUD", quotation.Entries[0].Column3);
			AssertEquals("Column4", "2000.00", quotation.Entries[0].Column4);
			AssertEquals("Column5", "1400.00", quotation.Entries[0].Column5);
			AssertEquals("Column6", "", quotation.Entries[0].Column6);
			AssertEquals("Column7", "", quotation.Entries[0].Column7);
			AssertEquals("Column8", "", quotation.Entries[0].Column8);
			AssertEquals("Column9", "", quotation.Entries[0].Column9);
			AssertEquals("Column10", "", quotation.Entries[0].Column10);
			AssertEquals("Column10", "", quotation.Entries[0].Column11);
			AssertEquals("Column11", "", quotation.Entries[0].Column12);
			AssertEquals("Column12", "7 Days", quotation.Entries[0].Column13);
			AssertEquals("Column13", "", quotation.Entries[0].Column14);
			Factory.Save();
			AssertEquals("Other Charges",
				"Test BAF:|AIR1: AUD 12.50 per Container|AIR2: AUD 21.00 per Container",
				quotation.Entries[0].OtherCharges.Replace(System.Environment.NewLine, "|"));

			AssertEquals("Test Opening", quotation.PageOpeningText);
			AssertMultilineASCIIEquals("", "Test Closing\r\n\r\nA local Value Added Tax charge (equivalent to GST) may apply to all items marked with an asterisk (*).", quotation.PageClosingText);

			AssertEquals(4, quotation.LocalDocRateLineItems.Count);
			AssertEquals("Local Charges - Sydney", quotation.LocalDocRateLineItems[0].LocalChargesHeading);

			AssertEquals("Test Airline Doc Fee *", quotation.LocalDocRateLineItems[0].Description);
			AssertEquals("", quotation.LocalDocRateLineItems[0].Amount);
			AssertEquals("", quotation.LocalDocRateLineItems[0].Currency);
			AssertEquals("", quotation.LocalDocRateLineItems[0].Units);

			AssertEquals("Minimum", quotation.LocalDocRateLineItems[1].Description);
			AssertEquals("100.00", quotation.LocalDocRateLineItems[1].Amount);
			AssertEquals("EUR", quotation.LocalDocRateLineItems[1].Currency);
			AssertEquals("", quotation.LocalDocRateLineItems[1].Units);

			AssertEquals("Per Unit", quotation.LocalDocRateLineItems[2].Description);
			AssertEquals("50.00", quotation.LocalDocRateLineItems[2].Amount);
			AssertEquals("EUR", quotation.LocalDocRateLineItems[2].Currency);
			AssertEquals("per Container", quotation.LocalDocRateLineItems[2].Units);

			AssertEquals("Test Airway Bill Fee", quotation.LocalDocRateLineItems[3].Description);
			AssertEquals("250.00", quotation.LocalDocRateLineItems[3].Amount);
			AssertEquals("EUR", quotation.LocalDocRateLineItems[3].Currency);
			AssertEquals("", quotation.LocalDocRateLineItems[3].Units);

			AssertEquals(1, quotation.OverseasDocRateLineItems.Count);
			AssertEquals("Overseas Charges - United Kingdom", quotation.OverseasDocRateLineItems[0].OverseasChargesHeading);
			AssertEquals("Test Airline Doc Fee *", quotation.OverseasDocRateLineItems[0].Description);
			AssertEquals("12.00", quotation.OverseasDocRateLineItems[0].Amount);
			AssertEquals("GBP", quotation.OverseasDocRateLineItems[0].Currency);
			AssertEquals("", quotation.OverseasDocRateLineItems[0].Units);
		}

		public void TestFRTChargeCodeCanAlsoEndInOtherCharges()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			RateEntry entry = quote.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "STD", "");
			RateLine line = entry.RateLines[0];
			line.TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)line.Calculator).BaseRate = 200m;

			PricingPageCollection pages = new PricingPageCollection(quote);
			pages.Load(PricingPaginationStrategy.LandscapeComplexStyle);

			DocTableQuotation quotation = DocTableQuotation.New(pages[0], Factory);

			AssertEquals("Page Headng", "Export Sea Freight Rates from Sydney", quotation.PageHeading);
			AssertEquals("Service Level", "Standard", quotation.ServiceLevel);
			AssertEquals("Commodity Code", "General", quotation.CommodityCode);
			AssertEquals("QuoteNo", "999 - NEWTESSYD", quotation.QuoteNumberAndClientCode);
			AssertEquals("Validity", ZDateTime.Today.AddMonths(1).ToString("d MMM yyyy"), quotation.ValidUntil);

			AssertEquals("Header1", "Destination", quotation.Entries[0].Header1);
			AssertEquals("Header2", "Shipping Line", quotation.Entries[0].Header2);
			AssertEquals("Header3", "", quotation.Entries[0].Header3);
			AssertEquals("Header4", "LCL Minimum", quotation.Entries[0].Header4);
			AssertEquals("Header5", "LCL per M3", quotation.Entries[0].Header5);
			AssertEquals("Header6", "", quotation.Entries[0].Header6);
			AssertEquals("Header7", "", quotation.Entries[0].Header7);
			AssertEquals("Header8", "", quotation.Entries[0].Header8);
			AssertEquals("Header9", "", quotation.Entries[0].Header9);
			AssertEquals("Header10", "", quotation.Entries[0].Header10);
			AssertEquals("Header11", "Transit Time", quotation.Entries[0].Header11);
			AssertEquals("Header12", "Freq.", quotation.Entries[0].Header12);
			AssertEquals("Header13", "W/V conv.", quotation.Entries[0].Header13);

			AssertEquals("Column1", "London", quotation.Entries[0].Column1);
			AssertEquals("Column2", "", quotation.Entries[0].Column2);
			AssertEquals("Column3", "USD", quotation.Entries[0].Column3);
			AssertEquals("Column4", "", quotation.Entries[0].Column4);
			AssertEquals("Column5", "", quotation.Entries[0].Column5);
			AssertEquals("Column6", "", quotation.Entries[0].Column6);
			AssertEquals("Column7", "", quotation.Entries[0].Column7);
			AssertEquals("Column8", "", quotation.Entries[0].Column8);
			AssertEquals("Column9", "", quotation.Entries[0].Column9);
			AssertEquals("Column10", "", quotation.Entries[0].Column10);
			AssertEquals("Column11", "", quotation.Entries[0].Column11);
			AssertEquals("Column12", "", quotation.Entries[0].Column12);
			AssertEquals("Column13", "", quotation.Entries[0].Column12);
			AssertEquals("Other Charges", "International Freight: USD 200.00", quotation.Entries[0].OtherCharges);
		}

		public void TestExportSeaFreight()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			#region GP20

			RateEntry entryGP20 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "STD", "20GP");
			entryGP20.TI_ViaLRC = "SGSIN";
			entryGP20.TI_TransitTime = "7";
			RateLine lineGP20 = entryGP20.RateLines[0];
			((UnitCalculator)lineGP20.Calculator).PerUnit = 1000m;

			RateLine lineGP20_2 = entryGP20.AddRateLine(TestBAF.AC_Code, UnitCalculator.Code, QuantityUnit.CN);
			((UnitCalculator)lineGP20_2.Calculator).PerUnit = 12.5m;

			#endregion

			#region GP40

			RateEntry entryGP40 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "STD", "40GP");
			entryGP40.TI_ViaLRC = "SGSIN";
			entryGP40.TI_TransitTime = "7";
			RateLine lineGP40 = entryGP40.RateLines[0];
			((UnitCalculator)lineGP40.Calculator).PerUnit = 1900m;

			RateLine lineGP40_2 = entryGP40.AddRateLine(TestBAF.AC_Code, UnitCalculator.Code, QuantityUnit.CN);
			((UnitCalculator)lineGP40_2.Calculator).PerUnit = 21m;

			#endregion

			#region LCL

			RateEntry entryLCL = quote.AddRateEntry("LCL", "LCL", "AUSYD", "GBLON", "STD", "");
			entryLCL.TI_ViaLRC = "SGSIN";
			entryLCL.TI_TransitTime = "7";
			entryLCL.TI_PageOpeningText = "Test Opening";
			entryLCL.TI_PageClosingText = "Test Closing";
			RateLine lineLCL = entryLCL.RateLines[0];
			((MinimumOrPerUnitCalculator)lineLCL.Calculator).Minimum = 200m;
			((MinimumOrPerUnitCalculator)lineLCL.Calculator).PerUnit = 150m;

			RateLine lineLCL_2 = entryLCL.AddRateLine(TestBAF.AC_Code, UnitCalculator.Code, QuantityUnit.CN);
			((UnitCalculator)lineLCL_2.Calculator).PerUnit = 2.9m;

			#endregion

			#region Origin Charges

			RateEntry entryOrigin1 = quote.AddRateEntry("ORG", "LCL", "AUSYD", "", "STD", "");
			entryOrigin1.TI_RX_NKCurrency = "EUR";
			RateLine lineOrigin1 = entryOrigin1.AddRateLine(TestAWB.AC_Code);
			lineOrigin1.TL_RateCalculator = "CMB";
			lineOrigin1.TL_WeightVolume = "M3";
			((CombinedCalculator)lineOrigin1.Calculator).Minimum = 49m;
			((CombinedCalculator)lineOrigin1.Calculator).PerUnit = 2m;

			RateEntry entryOrigin2 = quote.AddRateEntry("ORG", "FCL", "AUSYD", "", "STD", "");
			entryOrigin2.TI_RX_NKCurrency = "EUR";
			RateLine lineOrigin2 = entryOrigin2.AddRateLine(TestAWB.AC_Code, CombinedCalculator.Code, QuantityUnit.CN);
			((CombinedCalculator)lineOrigin2.Calculator).PerUnit = 25m;

			#endregion

			#region Destination Charges

			RateEntry entryDestination = quote.AddRateEntry("DST", "FCL", "", "GB", "STD", "");
			RateLine lineDestination = entryDestination.AddRateLine(TestADF.AC_Code, FlatCalculator.Code);
			((FlatCalculator)lineDestination.Calculator).BaseRate = 12m;

			#endregion

			PricingPageCollection pages = new PricingPageCollection(quote);
			pages.Load(PricingPaginationStrategy.LandscapeComplexStyle);

			DocTableQuotation quotation = DocTableQuotation.New(pages[0], Factory);

			AssertEquals("Page Headng", "Export Sea Freight Rates from Sydney", quotation.PageHeading);
			AssertEquals("Service Level", "Standard", quotation.ServiceLevel);
			AssertEquals("Commodity Code", "General", quotation.CommodityCode);
			AssertEquals("QuoteNo", "999 - NEWTESSYD", quotation.QuoteNumberAndClientCode);
			AssertEquals("Validity", ZDateTime.Today.AddMonths(1).ToString("d MMM yyyy"), quotation.ValidUntil);

			AssertEquals("Header1", "Destination", quotation.Entries[0].Header1);
			AssertEquals("Header2", "Shipping Line", quotation.Entries[0].Header2);
			AssertEquals("Header3", "", quotation.Entries[0].Header3);
			AssertEquals("Header4", "20GP", quotation.Entries[0].Header4);
			AssertEquals("Header5", "40GP", quotation.Entries[0].Header5);
			AssertEquals("Header6", "", quotation.Entries[0].Header6);
			AssertEquals("Header7", "", quotation.Entries[0].Header7);
			AssertEquals("Header8", "", quotation.Entries[0].Header8);
			AssertEquals("Header9", "LCL Minimum", quotation.Entries[0].Header9);
			AssertEquals("Header10", "LCL per M3", quotation.Entries[0].Header10);
			AssertEquals("Header11", "Transit Time", quotation.Entries[0].Header11);
			AssertEquals("Header12", "Freq.", quotation.Entries[0].Header12);
			AssertEquals("Header13", "W/V conv.", quotation.Entries[0].Header13);

			AssertEquals("Column1", "London via Singapore", quotation.Entries[0].Column1);
			AssertEquals("Column2", "", quotation.Entries[0].Column2);
			AssertEquals("Column3", "USD", quotation.Entries[0].Column3);
			AssertEquals("Column4", "1000.00", quotation.Entries[0].Column4);
			AssertEquals("Column5", "1900.00", quotation.Entries[0].Column5);
			AssertEquals("Column6", "", quotation.Entries[0].Column6);
			AssertEquals("Column7", "", quotation.Entries[0].Column7);
			AssertEquals("Column8", "", quotation.Entries[0].Column8);
			AssertEquals("Column9", "200.00", quotation.Entries[0].Column9);
			AssertEquals("Column10", "150.00", quotation.Entries[0].Column10);
			AssertEquals("Column11", "7 Days", quotation.Entries[0].Column11);
			AssertEquals("Column12", "", quotation.Entries[0].Column12);
			AssertEquals("Column13", "", quotation.Entries[0].Column12.Replace("\n", "|"));
			Factory.Save();
			AssertEquals("Other Charges",
				"LCL|Test BAF: USD 2.90 per Container|FCL|Test BAF:|20GP: USD 12.50 per Container|40GP: USD 21.00 per Container",
				quotation.Entries[0].OtherCharges.Replace(System.Environment.NewLine, "|"));

			AssertEquals("Test Opening", quotation.PageOpeningText);
			AssertStartsWith("", "Test Closing", quotation.PageClosingText);

			AssertEquals(4, quotation.LocalDocRateLineItems.Count);
			AssertEquals("Local Charges - Sydney", quotation.LocalDocRateLineItems[0].LocalChargesHeading);
			AssertEquals("Test Airway Bill Fee", quotation.LocalDocRateLineItems[0].Description);
			AssertEquals("", quotation.LocalDocRateLineItems[0].Amount);
			AssertEquals("", quotation.LocalDocRateLineItems[0].Currency);
			AssertEquals("", quotation.LocalDocRateLineItems[0].Units);
			AssertEquals("Minimum", quotation.LocalDocRateLineItems[1].Description);
			AssertEquals("49.00", quotation.LocalDocRateLineItems[1].Amount);
			AssertEquals("EUR", quotation.LocalDocRateLineItems[1].Currency);
			AssertEquals("", quotation.LocalDocRateLineItems[1].Units);
			AssertEquals("Per Unit", quotation.LocalDocRateLineItems[2].Description);
			AssertEquals("2.00", quotation.LocalDocRateLineItems[2].Amount);
			AssertEquals("EUR", quotation.LocalDocRateLineItems[2].Currency);
			AssertEquals("per M3 / 1000 KG", quotation.LocalDocRateLineItems[2].Units);
			AssertEquals("Test Airway Bill Fee", quotation.LocalDocRateLineItems[3].Description);
			AssertEquals("25.00", quotation.LocalDocRateLineItems[3].Amount);
			AssertEquals("EUR", quotation.LocalDocRateLineItems[3].Currency);
			AssertEquals("per Container", quotation.LocalDocRateLineItems[3].Units);

			AssertEquals(1, quotation.OverseasDocRateLineItems.Count);
			AssertEquals("Overseas Charges - United Kingdom", quotation.OverseasDocRateLineItems[0].OverseasChargesHeading);
			AssertEquals("Test Airline Doc Fee *", quotation.OverseasDocRateLineItems[0].Description);
			AssertEquals("12.00", quotation.OverseasDocRateLineItems[0].Amount);
			AssertEquals("GBP", quotation.OverseasDocRateLineItems[0].Currency);
			AssertEquals("", quotation.OverseasDocRateLineItems[0].Units);
		}

		public void TestExportRoadFreight()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			#region GP20

			RateEntry entryGP20 = quote.AddRateEntry("FCL", "ROA", "AUSYD", "GBLON", "STD", "20GP");
			entryGP20.TI_ViaLRC = "SGSIN";
			entryGP20.TI_TransitTime = "7";
			RateLine lineGP20 = entryGP20.RateLines[0];
			((UnitCalculator)lineGP20.Calculator).PerUnit = 1000m;

			RateLine lineGP20_2 = entryGP20.AddRateLine(TestBAF.AC_Code, UnitCalculator.Code, QuantityUnit.CN);
			((UnitCalculator)lineGP20_2.Calculator).PerUnit = 12.5m;

			#endregion

			#region GP40

			RateEntry entryGP40 = quote.AddRateEntry("FCL", "ROA", "AUSYD", "GBLON", "STD", "40GP");
			entryGP40.TI_ViaLRC = "SGSIN";
			entryGP40.TI_TransitTime = "7";
			RateLine lineGP40 = entryGP40.RateLines[0];
			((UnitCalculator)lineGP40.Calculator).PerUnit = 1900m;

			RateLine lineGP40_2 = entryGP40.AddRateLine(TestBAF.AC_Code, UnitCalculator.Code, QuantityUnit.CN);
			((UnitCalculator)lineGP40_2.Calculator).PerUnit = 21m;

			#endregion

			#region LCL

			RateEntry entryLCL = quote.AddRateEntry("LCL", "LRO", "AUSYD", "GBLON", "STD", "");
			entryLCL.TI_ViaLRC = "SGSIN";
			entryLCL.TI_TransitTime = "7";
			entryLCL.TI_PageOpeningText = "Test Opening";
			entryLCL.TI_PageClosingText = "Test Closing";
			RateLine lineLCL = entryLCL.RateLines[0];
			((MinimumOrPerUnitCalculator)lineLCL.Calculator).Minimum = 200m;
			((MinimumOrPerUnitCalculator)lineLCL.Calculator).PerUnit = 150m;

			RateLine lineLCL_2 = entryLCL.AddRateLine(TestBAF.AC_Code, UnitCalculator.Code, QuantityUnit.CN);
			((UnitCalculator)lineLCL_2.Calculator).PerUnit = 2.9m;

			#endregion

			#region Origin Charges

			RateEntry entryOrigin1 = quote.AddRateEntry("ORG", "LRO", "AUSYD", "", "STD", "");
			entryOrigin1.TI_RX_NKCurrency = "EUR";
			RateLine lineOrigin1 = entryOrigin1.AddRateLine(TestAWB.AC_Code, CombinedCalculator.Code, QuantityUnit.M3);
			((CombinedCalculator)lineOrigin1.Calculator).Minimum = 49m;
			((CombinedCalculator)lineOrigin1.Calculator).PerUnit = 2m;

			RateEntry entryOrigin2 = quote.AddRateEntry("ORG", "ROA", "AUSYD", "", "STD", "");
			entryOrigin2.TI_RX_NKCurrency = "EUR";
			RateLine lineOrigin2 = entryOrigin2.AddRateLine(TestAWB.AC_Code, CombinedCalculator.Code, QuantityUnit.CN);
			((CombinedCalculator)lineOrigin2.Calculator).PerUnit = 25m;

			#endregion

			#region Destination Charges

			RateEntry entryDestination = quote.AddRateEntry("DST", "ROA", "", "GB", "STD", "");
			RateLine lineDestination = entryDestination.AddRateLine(TestADF.AC_Code, FlatCalculator.Code);
			((FlatCalculator)lineDestination.Calculator).BaseRate = 12m;

			#endregion

			PricingPageCollection pages = new PricingPageCollection(quote);
			pages.Load(PricingPaginationStrategy.LandscapeComplexStyle);

			DocTableQuotation quotation = DocTableQuotation.New(pages[0], Factory);

			AssertEquals("Page Headng", "Export Road Freight Rates from Sydney", quotation.PageHeading);
			AssertEquals("Service Level", "Standard", quotation.ServiceLevel);
			AssertEquals("Commodity Code", "General", quotation.CommodityCode);
			AssertEquals("QuoteNo", "999 - NEWTESSYD", quotation.QuoteNumberAndClientCode);
			AssertEquals("Validity", ZDateTime.Today.AddMonths(1).ToString("d MMM yyyy"), quotation.ValidUntil);

			AssertEquals("Header1", "Destination", quotation.Entries[0].Header1);
			AssertEquals("Header2", "Shipping Line", quotation.Entries[0].Header2);
			AssertEquals("Header3", "", quotation.Entries[0].Header3);
			AssertEquals("Header4", "20GP", quotation.Entries[0].Header4);
			AssertEquals("Header5", "40GP", quotation.Entries[0].Header5);
			AssertEquals("Header6", "", quotation.Entries[0].Header6);
			AssertEquals("Header7", "", quotation.Entries[0].Header7);
			AssertEquals("Header8", "", quotation.Entries[0].Header8);
			AssertEquals("Header9", "FTL/LTL Minimum", quotation.Entries[0].Header9);
			AssertEquals("Header10", "FTL/LTL per M3", quotation.Entries[0].Header10);
			AssertEquals("Header11", "Transit Time", quotation.Entries[0].Header11);
			AssertEquals("Header12", "Freq.", quotation.Entries[0].Header12);
			AssertEquals("Header13", "W/V conv.", quotation.Entries[0].Header13);

			AssertEquals("Column1", "London via Singapore", quotation.Entries[0].Column1);
			AssertEquals("Column2", "", quotation.Entries[0].Column2);
			AssertEquals("Column3", "USD", quotation.Entries[0].Column3);
			AssertEquals("Column4", "1000.00", quotation.Entries[0].Column4);
			AssertEquals("Column5", "1900.00", quotation.Entries[0].Column5);
			AssertEquals("Column6", "", quotation.Entries[0].Column6);
			AssertEquals("Column7", "", quotation.Entries[0].Column7);
			AssertEquals("Column8", "", quotation.Entries[0].Column8);
			AssertEquals("Column9", "200.00", quotation.Entries[0].Column9);
			AssertEquals("Column10", "150.00", quotation.Entries[0].Column10);
			AssertEquals("Column11", "7 Days", quotation.Entries[0].Column11);
			AssertEquals("Column12", "", quotation.Entries[0].Column12);
			AssertEquals("Column13", "", quotation.Entries[0].Column12.Replace("\n", "|"));
			Factory.Save();
			AssertEquals("Other Charges",
				"LRO|Test BAF: USD 2.90 per Container|FRO|Test BAF:|20GP: USD 12.50 per Container|40GP: USD 21.00 per Container",
				quotation.Entries[0].OtherCharges.Replace(System.Environment.NewLine, "|"));

			AssertEquals("Test Opening", quotation.PageOpeningText);
			AssertStartsWith("", "Test Closing", quotation.PageClosingText);

			AssertEquals(4, quotation.LocalDocRateLineItems.Count);
			AssertEquals("Local Charges - Sydney", quotation.LocalDocRateLineItems[0].LocalChargesHeading);
			AssertEquals("Test Airway Bill Fee", quotation.LocalDocRateLineItems[0].Description);
			AssertEquals("", quotation.LocalDocRateLineItems[0].Amount);
			AssertEquals("", quotation.LocalDocRateLineItems[0].Currency);
			AssertEquals("", quotation.LocalDocRateLineItems[0].Units);

			AssertEquals("Minimum", quotation.LocalDocRateLineItems[1].Description);
			AssertEquals("49.00", quotation.LocalDocRateLineItems[1].Amount);
			AssertEquals("EUR", quotation.LocalDocRateLineItems[1].Currency);
			AssertEquals("", quotation.LocalDocRateLineItems[1].Units);

			AssertEquals("Per Unit", quotation.LocalDocRateLineItems[2].Description);
			AssertEquals("2.00", quotation.LocalDocRateLineItems[2].Amount);
			AssertEquals("EUR", quotation.LocalDocRateLineItems[2].Currency);
			AssertEquals("per M3 / 333 KG", quotation.LocalDocRateLineItems[2].Units);

			AssertEquals("Test Airway Bill Fee", quotation.LocalDocRateLineItems[3].Description);
			AssertEquals("25.00", quotation.LocalDocRateLineItems[3].Amount);
			AssertEquals("EUR", quotation.LocalDocRateLineItems[3].Currency);
			AssertEquals("per Container", quotation.LocalDocRateLineItems[3].Units);

			AssertEquals(1, quotation.OverseasDocRateLineItems.Count);
			AssertEquals("Overseas Charges - United Kingdom", quotation.OverseasDocRateLineItems[0].OverseasChargesHeading);
			AssertEquals("Test Airline Doc Fee *", quotation.OverseasDocRateLineItems[0].Description);
			AssertEquals("12.00", quotation.OverseasDocRateLineItems[0].Amount);
			AssertEquals("GBP", quotation.OverseasDocRateLineItems[0].Currency);
			AssertEquals("", quotation.OverseasDocRateLineItems[0].Units);
		}

		public void TestImportShippingFreight()
		{
			GlbCompany.CurrentCompany.SetCountry("GB");

			#region GP20

			RateEntry entryGP20 = quote.AddRateEntry("SCO", "SEA", "AUSYD", "GBLON", "STD", "20GP");
			entryGP20.TI_TransitTime = "7";
			RateLine lineGP20 = entryGP20.RateLines[0];
			((UnitCalculator)lineGP20.Calculator).PerUnit = 1000m;

			RateLine lineGP20_2 = entryGP20.AddRateLine(TestBAF.AC_Code, UnitCalculator.Code, QuantityUnit.CN);
			((UnitCalculator)lineGP20_2.Calculator).PerUnit = 12.5m;

			#endregion

			#region GP40

			RateEntry entryGP40 = quote.AddRateEntry("SCO", "SEA", "AUSYD", "GBLON", "STD", "40GP");
			entryGP40.TI_TransitTime = "7";
			RateLine lineGP40 = entryGP40.RateLines[0];
			((UnitCalculator)lineGP40.Calculator).PerUnit = 1900m;

			RateLine lineGP40_2 = entryGP40.AddRateLine(TestBAF.AC_Code, UnitCalculator.Code, QuantityUnit.CN);
			((UnitCalculator)lineGP40_2.Calculator).PerUnit = 21m;

			#endregion

			#region LCL

			RateEntry entryLCL = quote.AddRateEntry("SNC", "LCL", "AUSYD", "GBLON", "STD", "");
			entryLCL.TI_TransitTime = "7";
			entryLCL.TI_PageOpeningText = "Test Opening";
			entryLCL.TI_PageClosingText = "Test Closing";
			RateLine lineLCL = entryLCL.RateLines[0];
			((MinimumOrPerUnitCalculator)lineLCL.Calculator).Minimum = 200m;
			((MinimumOrPerUnitCalculator)lineLCL.Calculator).PerUnit = 150m;

			RateLine lineLCL_2 = entryLCL.AddRateLine(TestBAF.AC_Code, UnitCalculator.Code, QuantityUnit.CN);
			((UnitCalculator)lineLCL_2.Calculator).PerUnit = 2.9m;

			#endregion

			#region Origin Charges

			RateEntry entryOrigin1 = quote.AddRateEntry("SOR", "LCL", "AUSYD", "", "STD", "");
			entryOrigin1.TI_RX_NKCurrency = "EUR";
			RateLine lineOrigin1 = entryOrigin1.AddRateLine(TestAWB.AC_Code, CombinedCalculator.Code, QuantityUnit.M3);
			((CombinedCalculator)lineOrigin1.Calculator).Minimum = 49m;
			((CombinedCalculator)lineOrigin1.Calculator).PerUnit = 2m;

			RateEntry entryOrigin2 = quote.AddRateEntry("SOR", "FCL", "AUSYD", "", "STD", "");
			entryOrigin2.TI_RX_NKCurrency = "EUR";
			RateLine lineOrigin2 = entryOrigin2.AddRateLine(TestAWB.AC_Code, CombinedCalculator.Code, QuantityUnit.CN);
			((CombinedCalculator)lineOrigin2.Calculator).PerUnit = 25m;

			#endregion

			#region Destination Charges

			RateEntry entryDestination = quote.AddRateEntry("SDE", "FCL", "", "GB", "STD", "");
			RateLine lineDestination = entryDestination.AddRateLine(TestADF.AC_Code, FlatCalculator.Code);
			((FlatCalculator)lineDestination.Calculator).BaseRate = 12m;

			#endregion

			PricingPageCollection pages = new PricingPageCollection(quote);
			pages.Load(PricingPaginationStrategy.LandscapeComplexStyle);

			DocTableQuotation quotation = DocTableQuotation.New(pages[0], Factory);

			AssertEquals("Page Headng", "Import Shipping Freight Rates to London", quotation.PageHeading);
			AssertEquals("Commodity Code", "General", quotation.CommodityCode);
			AssertEquals("QuoteNo", "999 - NEWTESSYD", quotation.QuoteNumberAndClientCode);
			AssertEquals("Validity", ZDateTime.Today.AddMonths(1).ToString("d MMM yyyy"), quotation.ValidUntil);

			AssertEquals("Header1", "Origin", quotation.Entries[0].Header1);
			AssertEquals("Header2", "Principal", quotation.Entries[0].Header2);
			AssertEquals("Header3", "", quotation.Entries[0].Header3);
			AssertEquals("Header4", "20GP", quotation.Entries[0].Header4);
			AssertEquals("Header5", "40GP", quotation.Entries[0].Header5);
			AssertEquals("Header6", "", quotation.Entries[0].Header6);
			AssertEquals("Header7", "", quotation.Entries[0].Header7);
			AssertEquals("Header8", "", quotation.Entries[0].Header8);
			AssertEquals("Header9", "LCL Minimum", quotation.Entries[0].Header9);
			AssertEquals("Header10", "LCL per M3", quotation.Entries[0].Header10);
			AssertEquals("Header11", "Transit Time", quotation.Entries[0].Header11);
			AssertEquals("Header12", "Freq.", quotation.Entries[0].Header12);
			AssertEquals("Header13", "W/V conv.", quotation.Entries[0].Header13);

			AssertEquals("Column1", "Sydney", quotation.Entries[0].Column1);
			AssertEquals("Column2", "", quotation.Entries[0].Column2);
			AssertEquals("Column3", "USD", quotation.Entries[0].Column3);
			AssertEquals("Column4", "1000.00", quotation.Entries[0].Column4);
			AssertEquals("Column5", "1900.00", quotation.Entries[0].Column5);
			AssertEquals("Column6", "", quotation.Entries[0].Column6);
			AssertEquals("Column7", "", quotation.Entries[0].Column7);
			AssertEquals("Column8", "", quotation.Entries[0].Column8);
			AssertEquals("Column9", "200.00", quotation.Entries[0].Column9);
			AssertEquals("Column10", "150.00", quotation.Entries[0].Column10);
			AssertEquals("Column11", "7 Days", quotation.Entries[0].Column11);
			AssertEquals("Column12", "", quotation.Entries[0].Column12);
			AssertEquals("Column13", "", quotation.Entries[0].Column12.Replace("\n", "|"));
			Factory.Save();
			AssertEquals("Other Charges",
				"LCL|Test BAF: USD 2.90 per Container|FCL|Test BAF:|20GP: USD 12.50 per Container|40GP: USD 21.00 per Container",
				quotation.Entries[0].OtherCharges.Replace(System.Environment.NewLine, "|"));

			AssertEquals("Test Opening", quotation.PageOpeningText);
			AssertMultilineASCIIEquals("", "Test Closing\r\n\r\nA local Value Added Tax charge (equivalent to VAT) may apply to all items marked with an asterisk (*).", quotation.PageClosingText);

			AssertEquals(4, quotation.OverseasDocRateLineItems.Count);
			AssertEquals("Overseas Charges - Sydney", quotation.OverseasDocRateLineItems[0].OverseasChargesHeading);
			AssertEquals("Test Airway Bill Fee", quotation.OverseasDocRateLineItems[0].Description);
			AssertEquals("", quotation.OverseasDocRateLineItems[0].Amount);
			AssertEquals("", quotation.OverseasDocRateLineItems[0].Currency);
			AssertEquals("", quotation.OverseasDocRateLineItems[0].Units);
			AssertEquals("Minimum", quotation.OverseasDocRateLineItems[1].Description);
			AssertEquals("49.00", quotation.OverseasDocRateLineItems[1].Amount);
			AssertEquals("EUR", quotation.OverseasDocRateLineItems[1].Currency);
			AssertEquals("", quotation.OverseasDocRateLineItems[1].Units);
			AssertEquals("Per Unit", quotation.OverseasDocRateLineItems[2].Description);
			AssertEquals("2.00", quotation.OverseasDocRateLineItems[2].Amount);
			AssertEquals("EUR", quotation.OverseasDocRateLineItems[2].Currency);
			AssertEquals("per M3 / 1000 KG", quotation.OverseasDocRateLineItems[2].Units);
			AssertEquals("Test Airway Bill Fee", quotation.OverseasDocRateLineItems[3].Description);
			AssertEquals("25.00", quotation.OverseasDocRateLineItems[3].Amount);
			AssertEquals("EUR", quotation.OverseasDocRateLineItems[3].Currency);
			AssertEquals("per Container", quotation.OverseasDocRateLineItems[3].Units);

			AssertEquals(1, quotation.LocalDocRateLineItems.Count);
			AssertEquals("Local Charges - United Kingdom", quotation.LocalDocRateLineItems[0].LocalChargesHeading);
			AssertEquals("Test Airline Doc Fee *", quotation.LocalDocRateLineItems[0].Description);
			AssertEquals("12.00", quotation.LocalDocRateLineItems[0].Amount);
			AssertEquals("GBP", quotation.LocalDocRateLineItems[0].Currency);
			AssertEquals("", quotation.LocalDocRateLineItems[0].Units);
		}

		#region Implementation

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			InsertClientChargeCodesForAutoRaterTests(Factory);
			TestBAF.AC_AT_GSTRate = ZGuid.Empty;
			TestAWB.AC_AT_GSTRate = ZGuid.Empty;

			Factory.Save();

			originalAlternateRateFormatValue = DocumentsDataRegistry.Instance.AlternativeRateFormat.Value;
			DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		bool originalAlternateRateFormatValue;

		protected override void TearDown()
		{
			DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalAlternateRateFormatValue);
			base.TearDown();
		}

		Quote fTestQuote;
		Quote quote
		{
			get
			{
				if (fTestQuote == null)
				{
					fTestQuote = Factory.New<Quote>();
					fTestQuote.TH_OH = NewClient.PK;
					fTestQuote.TH_QuoteNumber = "0000999";
					fTestQuote.Header.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
				}
				return fTestQuote;
			}
		}

		#endregion
	}
}
