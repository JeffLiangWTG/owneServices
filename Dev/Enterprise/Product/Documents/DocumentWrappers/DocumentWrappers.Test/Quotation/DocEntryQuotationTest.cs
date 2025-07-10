using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	sealed class DocEntryQuotationTest : RatingTestCase
	{
		public void TestSortCollection()
		{
			RunSortCollectionTest(AccClientInvoiceOrderLookups.InvoiceTypes.All.Code);
			RunSortCollectionTest(ZString.Empty);
		}

		void RunSortCollectionTest(string invoiceType)
		{
			AccChargeCode code1 = Helper.ChargeCodes["EFAF"];
			code1.AC_PrintSequence = 3;

			AccChargeCode code2 = Helper.ChargeCodes["OFORW"];
			code2.AC_PrintSequence = 1;

			AccChargeCode code3 = Helper.ChargeCodes["FSC"];
			code3.AC_PrintSequence = 4;

			AccChargeCode code4 = Helper.ChargeCodes["WAR"];
			code4.AC_PrintSequence = 5;

			OrgHeader testClient = Factory.NewWithValidTestData<OrgHeader>();
			testClient.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();

			OrgInvoiceRollupOrGroup group = testClient.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			group.PG_TransportMode = Core.Constants.RateMode.LSE;

			Quote quote = Helper.NewQuote(testClient);

			RateEntry entry = quote.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "ER", "AU");

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

			PricingPage page = new PricingPage(entry, Factory, PricingPageStyle.Standard);
			DocEntryQuotation quotation = DocEntryQuotation.New(page, Factory);

			AssertEquals(code1.AC_Desc, quotation.FreightDocRateLineItems[0].Description); // (3) Emergency Fuel Adjustment Factor
			AssertEquals(code2.AC_Desc, quotation.FreightDocRateLineItems[1].Description); // (1) Onforwarding Charges
			AssertEquals(code3.AC_Desc, quotation.FreightDocRateLineItems[2].Description); // (4) Fuel Surcharge
			AssertEquals(code4.AC_Desc, quotation.FreightDocRateLineItems[3].Description); // (2) War Risk Surcharge

			group.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.Sequence;
			Factory.Save();
			quotation = DocEntryQuotation.New(page, Factory);

			AssertEquals(code2.AC_Desc, quotation.FreightDocRateLineItems[0].Description); // (1) Onforwarding Charges
			AssertEquals(code4.AC_Desc, quotation.FreightDocRateLineItems[1].Description); // (2) War Risk Surcharge
			AssertEquals(code1.AC_Desc, quotation.FreightDocRateLineItems[2].Description); // (3) Emergency Fuel Adjustment Factor
			AssertEquals(code3.AC_Desc, quotation.FreightDocRateLineItems[3].Description); // (4) Fuel Surcharge

			group.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical;
			Factory.Save();
			quotation = DocEntryQuotation.New(page, Factory);

			AssertEquals(code1.AC_Desc, quotation.FreightDocRateLineItems[0].Description); // (3) Emergency Fuel Adjustment Factor
			AssertEquals(code3.AC_Desc, quotation.FreightDocRateLineItems[1].Description); // (4) Fuel Surcharge
			AssertEquals(code2.AC_Desc, quotation.FreightDocRateLineItems[2].Description); // (1) Onforwarding Charges
			AssertEquals(code4.AC_Desc, quotation.FreightDocRateLineItems[3].Description); // (2) War Risk Surcharge

			group.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.User;
			Factory.Save();
			quotation = DocEntryQuotation.New(page, Factory);

			AssertEquals(code1.AC_Desc, quotation.FreightDocRateLineItems[0].Description); // (3) Emergency Fuel Adjustment Factor
			AssertEquals(code2.AC_Desc, quotation.FreightDocRateLineItems[1].Description); // (1) Onforwarding Charges
			AssertEquals(code3.AC_Desc, quotation.FreightDocRateLineItems[2].Description); // (4) Fuel Surcharge
			AssertEquals(code4.AC_Desc, quotation.FreightDocRateLineItems[3].Description); // (2) War Risk Surcharge

			group.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence;
			Factory.Save();
			quotation = DocEntryQuotation.New(page, Factory);

			AssertEquals(code2.AC_Desc, quotation.FreightDocRateLineItems[0].Description); // (1) Onforwarding Charges
			AssertEquals(code4.AC_Desc, quotation.FreightDocRateLineItems[1].Description); // (2) War Risk Surcharge
			AssertEquals(code1.AC_Desc, quotation.FreightDocRateLineItems[2].Description); // (3) Emergency Fuel Adjustment Factor
			AssertEquals(code3.AC_Desc, quotation.FreightDocRateLineItems[3].Description); // (4) Fuel Surcharge

			group.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.SubTotal;
			Factory.Save();
			quotation = DocEntryQuotation.New(page, Factory);

			AssertEquals(code1.AC_Desc, quotation.FreightDocRateLineItems[0].Description); // (3) Emergency Fuel Adjustment Factor
			AssertEquals(code2.AC_Desc, quotation.FreightDocRateLineItems[1].Description); // (1) Onforwarding Charges
			AssertEquals(code3.AC_Desc, quotation.FreightDocRateLineItems[2].Description); // (4) Fuel Surcharge
			AssertEquals(code4.AC_Desc, quotation.FreightDocRateLineItems[3].Description); // (2) War Risk Surcharge

			group.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			Factory.Save();
			quotation = DocEntryQuotation.New(page, Factory);

			AssertEquals(code1.AC_Desc, quotation.FreightDocRateLineItems[0].Description); // (3) Emergency Fuel Adjustment Factor
			AssertEquals(code2.AC_Desc, quotation.FreightDocRateLineItems[1].Description); // (1) Onforwarding Charges
			AssertEquals(code3.AC_Desc, quotation.FreightDocRateLineItems[2].Description); // (4) Fuel Surcharge
			AssertEquals(code4.AC_Desc, quotation.FreightDocRateLineItems[3].Description); // (2) War Risk Surcharge

			group.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUpEntireConsol;
			Factory.Save();
			quotation = DocEntryQuotation.New(page, Factory);

			AssertEquals(code1.AC_Desc, quotation.FreightDocRateLineItems[0].Description); // (3) Emergency Fuel Adjustment Factor
			AssertEquals(code2.AC_Desc, quotation.FreightDocRateLineItems[1].Description); // (1) Onforwarding Charges
			AssertEquals(code3.AC_Desc, quotation.FreightDocRateLineItems[2].Description); // (4) Fuel Surcharge
			AssertEquals(code4.AC_Desc, quotation.FreightDocRateLineItems[3].Description); // (2) War Risk Surcharge
		}

		[ExpectNoExceptions]
		public void TestSortCollection_MultiCountryZone()
		{
			var zone = Factory.NewWithValidTestData<RefZoneHeader>();
			zone.Code = "ZAYD";
			zone.UNLOCOs.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"));
			zone.UNLOCOs.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "INBOM"));
			Factory.Save();

			var testClient = Factory.NewWithValidTestData<OrgHeader>();
			var quote = Helper.NewQuote(testClient);
			var entry = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "ZAYD", "AU");

			var line = entry.RateLines.AddNew();
			line.TL_AC = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_ChargeGroup, ChargeCodeGroupList.Codes.Origin)).PK;
			line.TL_RateCalculator = FlatCalculator.Code;
			line.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)200m;

			Factory.Save();

			PricingPage page = new PricingPage(entry, entry.Factory, PricingPageStyle.Standard);
			var quotation = DocEntryQuotation.New(page, Factory);
			var entries = quotation.OriginDocRateLineItems;
		}

		public void TestShowLocalCurrencyonSpotQuotePricingPage()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			PricingPage page = new PricingPage(quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX"), Factory, PricingPageStyle.Standard);

			DocEntryQuotation docQuotation = DocEntryQuotation.New(page, Factory);
			DocumentsDataRegistry.Instance.ShowLocalCurrencyonSpotQuotePricingPage.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(false, docQuotation.ShowLocalCurrencyonSpotQuotePricingPage);
			DocumentsDataRegistry.Instance.ShowLocalCurrencyonSpotQuotePricingPage.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(true, docQuotation.ShowLocalCurrencyonSpotQuotePricingPage);
		}

		public void TestOneOffQuoteCharges()
		{
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_ShowOnQuotation = true;
			chargeCode1.AC_SuppressOnQuoteIfZero = false;

			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_ShowOnQuotation = true;
			chargeCode2.AC_SuppressOnQuoteIfZero = false;

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			Factory.Save();

			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			quotedBooking.TryLoadOrCreateJob();
			using (Job header = (Job)quotedBooking.Job)
			{
				Charge charge1 = header.Charges.AddNew();
				charge1.JR_AC = chargeCode1.PK;
				Charge charge2 = header.Charges.AddNew();
				charge2.JR_AC = chargeCode2.PK;

				RateEntry quoteEntry = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
				quoteEntry.TI_OH_Consignee = Helper.NewOrgHeader().PK;
				quoteEntry.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
				((FlatCalculator)quoteEntry.RateLines[0].Calculator).BaseRate = 100m;

				PricingPage page = new PricingPage(quoteEntry, quoteEntry.Factory, PricingPageStyle.Standard);

				DocEntryQuotation docQuotation = DocEntryQuotation.New(page, Factory);
				AssertEquals("Should contain 2 charges", 2, docQuotation.OneOffQuoteCharges.Count);

				chargeCode1.AC_ShowOnQuotation = false;
				docQuotation = DocEntryQuotation.New(page, Factory);
				AssertEquals("Should contain 1 charge", 1, docQuotation.OneOffQuoteCharges.Count);
			}
		}

		public void TestQuoteWithConsigneeClientRateWithout()
		{
			Quote quote = Helper.NewQuote(Helper.NewOrgHeader());
			RateEntry quoteEntry = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			quoteEntry.TI_OH_Consignee = Helper.NewOrgHeader().PK;
			quoteEntry.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)quoteEntry.RateLines[0].Calculator).BaseRate = 100m;

			ClientRate rate = Helper.NewClientRate(quote.Header);
			RateEntry rateEntry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			rateEntry.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)rateEntry.RateLines[0].Calculator).BaseRate = 300m;

			Factory.Save();

			DocEntryQuotation docQuotation = DocEntryQuotation.New(new PricingPage(quoteEntry, Factory, PricingPageStyle.Standard), Factory);
			DocQuotationLineCollection freightCollection = docQuotation.FreightDocRateLineItems;

			AssertEquals(1, freightCollection.Count);
			AssertDocRateLineItem(freightCollection[0], @"INTERNATIONAL FREIGHT 
-  TO TEST CLIENT #2", Constants.CurrencyCodes.Australia, "100.00", "");
		}

		public void TestPageHeading()
		{
			Env.Registry.Rating.QuoteHeaderText = "This is the registry heading";

			Quote quote = Factory.New<Quote>();
			RateEntry entry1 = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			PricingPage page = new PricingPage(entry1, Factory, PricingPageStyle.Standard);

			AssertEquals("Precondition: Field is blank", ZString.Empty, entry1.TI_PageHeading);
			AssertEquals("Precondition: Bound field shows registry value", "This is the registry heading", ((QuoteEntry)entry1).PageHeader);

			DocEntryQuotation testDocQuotation = DocEntryQuotation.New(page, Factory);
			AssertEquals("Page Open Text uses bound field - registry value", "This is the registry heading", testDocQuotation.PageHeading);

			entry1.TI_PageHeading = "Something else";
			testDocQuotation = DocEntryQuotation.New(page, Factory);
			AssertEquals("Page Open Text uses bound field - overriden value", "Something else", testDocQuotation.PageHeading);
		}

		[TestDate(2014, 10, 17, 12, 11, 0)]
		public void TestDocQuotation()
		{
			#region Setup

			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			SetupSuppliers();
			SetupOriginCharges();
			SetupFreightCharges();
			SetupDestinationCharges();
			SetupGlobalCharges();

			DocEntryQuotation testDocQuotation = DocEntryQuotation.New(new PricingPage(LCLFreightEntry, Factory, PricingPageStyle.Standard), Factory);

			Factory.Save();

			DocQuotationLineCollection originCollection = testDocQuotation.OriginDocRateLineItems;
			DocQuotationLineCollection freightCollection = testDocQuotation.FreightDocRateLineItems;
			DocQuotationLineCollection destinationCollection = testDocQuotation.DestinationDocRateLineItems;

			#endregion

			AssertEquals("Page Open Text", "LCL Freight from Sydney to Los Angeles, US", testDocQuotation.PageHeading);
			AssertEquals("Valid Until", ZDateTime.Today.AddMonths(1).ToString("d MMM yyyy"), testDocQuotation.ValidUntil);
			AssertEquals("Transit Time", "14 Days", testDocQuotation.TransitTime);
			AssertEquals("Date", ZDateTime.Today.ToString("d MMM yyyy"), testDocQuotation.Date);
			AssertEquals("Service Level", "Standard", testDocQuotation.ServiceLevel);
			AssertEquals("Commodity Code", "General", testDocQuotation.CommodityCode);
			AssertEquals("Frequency", "2 per Week", testDocQuotation.Frequency);
			AssertEquals("Quote Number", "999/A - NEWTESSYD", testDocQuotation.QuoteNumberAndClientCode);
			AssertEquals("Quote Number", "999/A - New Test Client", testDocQuotation.QuoteNumberAndClientFullName);

			#region Origin Collection Assertions

			#region Documentation

			DocRateLineItem item = originCollection.FindByDescAndAmount(DocumentationPK, "Documentation", "75");
			AssertDocRateLineItem(item, "Documentation", Constants.CurrencyCodes.Australia, "75.00", "per House Bill");

			#endregion

			#region Cartage (Alexandria)

			item = originCollection.FindByDescAndAmount(CartAlexandriaPK, "From Alexandria", "");
			AssertDocRateLineItem(item, @"
-  From Alexandria, NSW For Alexandria Supplier", "", "", "");

			item = originCollection.FindByDescAndAmount(CartAlexandriaPK, "Minimum", "30");
			AssertDocRateLineItem(item, "Minimum", Constants.CurrencyCodes.Australia, "30.00", "");

			item = originCollection.FindByDescAndAmount(CartAlexandriaPK, "Per Unit", "1.50");
			AssertDocRateLineItem(item, "Per Unit", Constants.CurrencyCodes.Australia, "1.50", "per KG (1 M3 = 1000 KG)");

			#endregion

			#region Italian Airport Tax

			item = originCollection.FindByDescAndAmount(ItalianAirportTaxPK, "Italian Airport Tax", "");
			AssertDocRateLineItem(item, "Italian Airport Tax", "", "", "");

			item = originCollection.FindByDescAndAmount(ItalianAirportTaxPK, "Base Rate", "23.00");
			AssertDocRateLineItem(item, "Base Rate", "EUR", "23.00", "");

			item = originCollection.FindByDescAndAmount(ItalianAirportTaxPK, "Flat Rate", ".05");
			AssertDocRateLineItem(item, "Flat Rate", "EUR", "0.05", "per kg");

			item = originCollection.FindByDescAndAmount(ItalianAirportTaxPK, "First Package", ".25");
			AssertDocRateLineItem(item, "First Package", "EUR", "0.25", "");

			item = originCollection.FindByDescAndAmount(ItalianAirportTaxPK, "Additional Packages", ".12");
			AssertDocRateLineItem(item, "Additional Packages", "EUR", "0.12", "per package");

			#endregion

			#region Cartage (Hornsby)

			item = originCollection.FindByDescAndAmount(CartHornsbyPK, "Hornsby Supplier", "");
			AssertDocRateLineItem(item, @"
-  From Hornsby, NSW for Hornsby Supplier", "", "", "");

			item = originCollection.FindByDescAndAmount(CartHornsbyPK, "Minimum", "60");
			AssertDocRateLineItem(item, @"Minimum", Constants.CurrencyCodes.Australia, "60.00", "");

			item = originCollection.FindByDescAndAmount(CartHornsbyPK, "Less than 500 KG", "3.50");
			AssertDocRateLineItem(item, "Less than 500 KG", Constants.CurrencyCodes.Australia, "3.50", "per KG (1 M3 = 1000 KG)");

			item = originCollection.FindByDescAndAmount(CartHornsbyPK, "500 KG and above", "2.50");
			AssertDocRateLineItem(item, "500 KG and above", Constants.CurrencyCodes.Australia, "2.50", "per KG (1 M3 = 1000 KG)");

			#endregion

			#region Export Customs Formalities

			item = originCollection.FindByDescAndAmount(ExportCustomsPK, "Export Customs Formalities", "");
			AssertDocRateLineItem(item, "Export Customs Formalities", "", "", "");

			item = originCollection.FindByDescAndAmount(ExportCustomsPK, "Minimum", "50");
			AssertDocRateLineItem(item, "Minimum", Constants.CurrencyCodes.Australia, "50.00", "");

			item = originCollection.FindByDescAndAmount(ExportCustomsPK, "Less than 4 M3", "78");
			AssertDocRateLineItem(item, "Less than 4 M3", Constants.CurrencyCodes.Australia, "78.00", "");

			item = originCollection.FindByDescAndAmount(ExportCustomsPK, "4 M3 to Less than 8 M3", "87.5");
			AssertDocRateLineItem(item, "4 M3 to Less than 8 M3", Constants.CurrencyCodes.Australia, "87.50", "");

			item = originCollection.FindByDescAndAmount(ExportCustomsPK, "8 M3 and above", "105.5");
			AssertDocRateLineItem(item, "8 M3 and above", Constants.CurrencyCodes.Australia, "105.50", "");

			#endregion

			#region Security Tax

			item = originCollection.FindByDescAndAmount(SecurityTaxPK, "Security Tax", "");
			AssertDocRateLineItem(item, "Security Tax *", "", "", "");

			item = originCollection.FindByDescAndAmount(SecurityTaxPK, "Minimum", "11");
			AssertDocRateLineItem(item, "Minimum", Constants.CurrencyCodes.Australia, "11.00", "");

			item = originCollection.FindByDescAndAmount(SecurityTaxPK, "Per Unit", ".08");
			AssertDocRateLineItem(item, "Per Unit", Constants.CurrencyCodes.Australia, "0.08", "per m3 / 1000 KG");

			#endregion

			#region Fumigation

			item = originCollection.FindByDescAndAmount(FumigationPK, "Fumigation", "200");
			AssertDocRateLineItem(item, "Fumigation", Constants.CurrencyCodes.Australia, "200.00", "");

			#endregion

			#region Housebill Release Type

			item = originCollection.FindByDescAndAmount(HousebillReleaseTypePK, "Housebill", "");
			AssertDocRateLineItem(item, "Housebill", "", "", "");

			item = originCollection.FindByDescAndAmount(SecurityTaxPK, "Letter of Credit (Bank Release)", "32");
			AssertDocRateLineItem(item, "Letter of Credit (Bank Release)", Constants.CurrencyCodes.Australia, "32.00", "");

			item = originCollection.FindByDescAndAmount(SecurityTaxPK, "Letter of Indemnity", "20");
			AssertDocRateLineItem(item, "Letter of Indemnity", Constants.CurrencyCodes.Australia, "20.00", "");

			item = originCollection.FindByDescAndAmount(SecurityTaxPK, "Company/Cashier Check", "14");
			AssertDocRateLineItem(item, "Company/Cashier Check", Constants.CurrencyCodes.Australia, "14.00", "");

			item = originCollection.FindByDescAndAmount(SecurityTaxPK, "Standard Release Type", "10");
			AssertDocRateLineItem(item, "Standard Release Type", Constants.CurrencyCodes.Australia, "10.00", "");

			#endregion

			#region Export Security Fee

			item = originCollection.FindByDescAndAmount(ExportSecurityPK, "Export Security Fee", "");
			AssertDocRateLineItem(item, "Export Security Fee - Credit Terms: 6 Days From Date Of Invoice", "", "1.50", "% + Prime Rate");

			#endregion

			#region Dummy Charge Code

			item = originCollection.FindByDescAndAmount(ZGuid.Empty, "Dummy Charge Code", "");
			AssertDocRateLineItem(item, "Dummy Charge Code", "", "Not Charged", "");

			#endregion

			#region Global Origin 1

			item = originCollection.FindByDescAndAmount(GlobalOrigin1PK, "Global Origin 1", "10");
			AssertDocRateLineItem(item, "Global Origin 1", Constants.CurrencyCodes.Australia, "10.00", "");

			#endregion

			#region Global Origin 2

			item = originCollection.FindByDescAndAmount(GlobalOrigin2PK, "Global Origin 2", "");
			AssertDocRateLineItem(item, "Global Origin 2", "", "", "");

			item = originCollection.FindByDescAndAmount(GlobalOrigin2PK, "Base Rate", "14.00");
			AssertDocRateLineItem(item, "Base Rate", Constants.CurrencyCodes.Australia, "14.00", "");

			item = originCollection.FindByDescAndAmount(GlobalOrigin2PK, "Per Unit", "0.45");
			AssertDocRateLineItem(item, "Per Unit", Constants.CurrencyCodes.Australia, "0.45", "per m3 / 1000 KG");

			#endregion

			#region Flat and Per Unit

			item = originCollection.FindByDescAndAmount(FlatPlusPerUnitWithNoFlatPK, "Flat Per Unit With No Flat", "45.25");
			AssertDocRateLineItem(item, "Flat Per Unit With No Flat", Constants.CurrencyCodes.Australia, "45.25", "per M3 / 1000 KG");

			#endregion

			#endregion

			#region Freight Collection Assertions

			#region Freight

			item = freightCollection.FindByDescAndAmount(ZGuid.Empty, "Freight", "");
			AssertDocRateLineItem(item, "Freight", "", "", "");

			item = freightCollection.FindByDescAndAmount(ZGuid.Empty, "Minimum", "150");
			AssertDocRateLineItem(item, "Minimum", Constants.CurrencyCodes.Australia, "150.00", "");

			item = freightCollection.FindByDescAndAmount(ZGuid.Empty, "Less than 5 M3", "90");
			AssertDocRateLineItem(item, "Less than 5 M3", Constants.CurrencyCodes.Australia, "90.00", "per m3 / 1000 KG");

			item = freightCollection.FindByDescAndAmount(ZGuid.Empty, "5 M3 to Less than 8 M3", "84");
			AssertDocRateLineItem(item, "5 M3 to Less than 8 M3", Constants.CurrencyCodes.Australia, "84.00", "per m3 / 1000 KG");

			item = freightCollection.FindByDescAndAmount(ZGuid.Empty, "8 M3 and above", "75");
			AssertDocRateLineItem(item, "8 M3 and above", Constants.CurrencyCodes.Australia, "75.00", "per m3 / 1000 KG");

			#endregion

			#region Fuel Surcharge

			item = freightCollection.FindByDescAndAmount(FuelSurchargePK, "Fuel Surcharge", "13.26");
			AssertDocRateLineItem(item, "Fuel Surcharge *", "", "13.26", "% of freight");

			item = freightCollection.FindByDescAndAmount(FuelSurchargePK2, "Fuel Surcharge 2", Env.Registry.NotChargedText);
			AssertDocRateLineItem(item, "Fuel Surcharge 2", "", Env.Registry.NotChargedText, "");

			item = freightCollection.FindByDescAndAmount(FuelSurchargePK3, "Fuel Surcharge 3", "2.00");
			AssertDocRateLineItem(item, "Fuel Surcharge 3", "", "2.00", "% of all charges");

			#endregion

			#region BAF

			item = freightCollection.FindByDescAndAmount(BAFPK, "Bunker Adjustment Fee", Env.Registry.NotChargedText);
			AssertDocRateLineItem(item, "Bunker Adjustment Fee", "", Env.Registry.NotChargedText, "");

			#endregion

			#region BAF - Zero (Not Shown)

			item = (DocRateLineItem)freightCollection.FindByPK(BAFNotShownIfZeroPK);
			AssertNull("Charge Code has been setup to not be shown if zero. Should not be included in quote.", item);

			#endregion

			#endregion

			#region Destination Collection Assertions

			item = destinationCollection.FindByDescAndAmount(DeliveryOrdeFeePK, "Delivery Order Fee", "40");
			AssertDocRateLineItem(item, "Delivery Order Fee *", Constants.CurrencyCodes.UnitedStates, "40.00", "per House Bill");

			item = destinationCollection.FindByDescAndAmount(CustomsEntryPK, "Customs Entry", "120");
			AssertDocRateLineItem(item, "Customs Entry", Constants.CurrencyCodes.UnitedStates, "120.00", "");

			item = destinationCollection.FindByDescAndAmount(PortChargesPK, "Port Charges", "55");
			AssertDocRateLineItem(item, "Port Charges", Constants.CurrencyCodes.UnitedStates, "55.00", "per M3 / 1000 KG");

			item = destinationCollection.FindByDescAndAmount(TerminalHandlingChargePK, "Terminal Handling Fee", "5");
			AssertDocRateLineItem(item, "Terminal Handling Fee", Constants.CurrencyCodes.UnitedStates, "5.00", "per M3 / 1000 KG");

			item = destinationCollection.FindByDescAndAmount(ProfessionalIndemnityFeePK, "Professional Indemnity Fee", Env.Registry.NotChargedText);
			AssertDocRateLineItem(item, "Professional Indemnity Fee", "", Env.Registry.NotChargedText, "");

			item = destinationCollection.FindByDescAndAmount(CargoAutomationPK, "Cargo Automation Fee", "5");
			AssertDocRateLineItem(item, "Cargo Automation Fee", Constants.CurrencyCodes.UnitedStates, "5.00", "");

			item = destinationCollection.FindByDescAndAmount(VehicleBookingFeePK, "Vehicle Booking Fee", Env.Registry.NotChargedText);
			AssertDocRateLineItem(item, "Vehicle Booking Fee", "", Env.Registry.NotChargedText, "");

			item = destinationCollection.FindByDescAndAmount(PostagePK, "Postage", Env.Registry.NotChargedText);
			AssertDocRateLineItem(item, "Postage *", "", Env.Registry.NotChargedText, "");

			#region Agency Charges

			#region Per Shipment, Flat Fee

			item = destinationCollection.FindByDescAndAmount(Agency1PK, "Agency", "50");
			AssertDocRateLineItem(item, "Agency Charges 1 - Per Shipment, Flat Fee", Constants.CurrencyCodes.UnitedStates, "50.00", "");

			#endregion

			#region Per Entry, Flat Fee

			item = destinationCollection.FindByDescAndAmount(Agency2PK, "Agency", "");
			AssertDocRateLineItem(item, "Agency Charges 2 - Per Entry, Flat Fee", "", "", "");

			item = destinationCollection.FindByDescAndAmount(Agency2PK, "First", "20");
			AssertDocRateLineItem(item, "First 1 Entry", Constants.CurrencyCodes.UnitedStates, "20.00", "");

			item = destinationCollection.FindByDescAndAmount(Agency2PK, "Additional", "10");
			AssertDocRateLineItem(item, "Additional Entry", Constants.CurrencyCodes.UnitedStates, "10.00", "");

			#endregion

			#region Per Shipment, Per Invoice

			item = destinationCollection.FindByDescAndAmount(Agency3PK, "Agency", "");
			AssertDocRateLineItem(item, "Agency Charges 3 - Per Shipment, Per Invoice Line Per Shipment", "", "", "");

			item = destinationCollection.FindByDescAndAmount(Agency3PK, "Base", "15");
			AssertDocRateLineItem(item, "Base Rate (3 Invoice Lines for Shipment included)", Constants.CurrencyCodes.UnitedStates, "15.00", "");

			item = destinationCollection.FindByDescAndAmount(Agency3PK, "Thereafter", "4.50");
			AssertDocRateLineItem(item, "Thereafter", Constants.CurrencyCodes.UnitedStates, "4.50", "per line");

			item = destinationCollection.FindByDescAndAmount(Agency3PK, "Maximum Lines", "25");
			AssertDocRateLineItem(item, "Maximum Lines", "", "25", "lines");

			#endregion

			#region Per Entry, Per Tariff

			item = destinationCollection.FindByDescAndAmount(Agency4PK, "Agency", "");
			AssertDocRateLineItem(item, "Agency Charges 4 - Per Entry, Per Tariff Line Per Entry", "", "", "");

			item = destinationCollection.FindByDescAndAmount(Agency4PK, "First", "19");
			AssertDocRateLineItem(item, "First 1 Entry, First 5 Tariff Lines for each entry", Constants.CurrencyCodes.UnitedStates, "19.00", "");

			item = destinationCollection.FindByDescAndAmount(Agency4PK, "Additional", "8.50");
			AssertDocRateLineItem(item, "Additional Entry", Constants.CurrencyCodes.UnitedStates, "8.50", "");

			item = destinationCollection.FindByDescAndAmount(Agency4PK, "Thereafter", "3.50");
			AssertDocRateLineItem(item, "Thereafter", Constants.CurrencyCodes.UnitedStates, "3.50", "per line");

			item = destinationCollection.FindByDescAndAmount(Agency4PK, "Maximum Lines", "40");
			AssertDocRateLineItem(item, "Maximum Lines", "", "40", "lines");

			#endregion

			#region Per Entry, Per Tariff - Hidden on Quote

			item = destinationCollection.FindByDescAndAmount(Agency5PK, "Agency", "");
			AssertDocRateLineItem(item, "Agency Charges 5", "", "", "");

			item = destinationCollection.FindByDescAndAmount(Agency5PK, "First", "19");
			AssertDocRateLineItem(item, "First 1 Entry, First 5 Tariff Lines For Each Entry", Constants.CurrencyCodes.UnitedStates, "19.00", "");

			item = destinationCollection.FindByDescAndAmount(Agency5PK, "Additional", "8.50");
			AssertDocRateLineItem(item, "Additional Entry", Constants.CurrencyCodes.UnitedStates, "8.50", "");

			item = destinationCollection.FindByDescAndAmount(Agency5PK, "Thereafter", "3.50");
			AssertDocRateLineItem(item, "Thereafter", Constants.CurrencyCodes.UnitedStates, "3.50", "per line");

			item = destinationCollection.FindByDescAndAmount(Agency5PK, "Maximum Lines", "40");
			AssertDocRateLineItem(item, "Maximum Lines", "", "40", "lines");

			#endregion

			#endregion

			#region Cartage

			item = destinationCollection.FindByDescAndAmount(CartLosAngelesPK, "Cartage", "");
			AssertDocRateLineItem(item, "Cartage", "", "", "");     // no addresses for supplier, therefore no need to display FROM / TO details

			item = destinationCollection.FindByDescAndAmount(CartLosAngelesPK, "Minimum", "153.00");
			AssertDocRateLineItem(item, "Minimum", Constants.CurrencyCodes.UnitedStates, "153.00", "");

			item = destinationCollection.FindByDescAndAmount(CartLosAngelesPK, "Per Unit", "21.50");
			AssertDocRateLineItem(item, "Per Unit", Constants.CurrencyCodes.UnitedStates, "21.50", "per M3 / 1000 KG");

			#endregion

			item = destinationCollection.FindByDescAndAmount(SingleEntryBondFeePartPK, "Single Entry Bond", "12.50");
			AssertDocRateLineItem(item, "Single Entry Bond Fee", Constants.CurrencyCodes.UnitedStates, "12.50", "per $1000.00 Invoice Value");

			item = destinationCollection.FindByDescAndAmount(SingleEntryBondFeePercentPK, "Single Entry Bond", "5.5");
			AssertDocRateLineItem(item, "Single Entry Bond Fee", "", "5.50", "% of Invoice Value");

			item = destinationCollection.FindByDescAndAmount(GlobalDestination1PK, "Global Destination 1", "78.00");
			AssertDocRateLineItem(item, "Global Destination 1", Constants.CurrencyCodes.UnitedStates, "78.00", "");

			item = destinationCollection.FindByDescAndAmount(GlobalDestination2PK, "Global Destination 2", "");
			AssertDocRateLineItem(item, "Global Destination 2", "", "", "");

			item = destinationCollection.FindByDescAndAmount(GlobalDestination2PK, "Minimum", "50.00");
			AssertDocRateLineItem(item, "Minimum", Constants.CurrencyCodes.UnitedStates, "50.00", "");

			item = destinationCollection.FindByDescAndAmount(GlobalDestination2PK, "Per Unit", "1.25");
			AssertDocRateLineItem(item, "Per Unit", Constants.CurrencyCodes.UnitedStates, "1.25", "per m3 / 1000 KG");

			item = destinationCollection.FindByDescAndAmount(OverrideGlobalPK, "Global Override", "45");
			AssertDocRateLineItem(item, "Global Override", Constants.CurrencyCodes.UnitedStates, "45.00", "");

			item = destinationCollection.FindByDescAndAmount(FlatPlusPerUnitWithFlatPK, "Flat Per Unit With Flat", "");
			AssertDocRateLineItem(item, "Flat Per Unit With Flat", "", "", "");

			item = destinationCollection.FindByDescAndAmount(FlatPlusPerUnitWithFlatPK, "Base Rate", "65.00");
			AssertDocRateLineItem(item, "Base Rate", Constants.CurrencyCodes.UnitedStates, "65.00", "");

			item = destinationCollection.FindByDescAndAmount(FlatPlusPerUnitWithFlatPK, "Per Unit", "2.50");
			AssertDocRateLineItem(item, "Per Unit", Constants.CurrencyCodes.UnitedStates, "2.50", "per M3 / 1000 KG");

			item = destinationCollection.FindByDescAndAmount(NoteCalculatorPK, "Test1", "1.01");
			AssertDocRateLineItem(item, "Test1", Constants.CurrencyCodes.UnitedStates, "1.01", "");

			item = destinationCollection.FindByDescAndAmount(NoteCalculatorPK, "Test2", "2.02");
			AssertDocRateLineItem(item, "Test2", Constants.CurrencyCodes.UnitedStates, "2.02", "");

			#endregion

			AssertContains("Page footer text contains GST Applicability text", " may apply to all items marked with an asterisk", testDocQuotation.PageClosingText);
		}

		public void TestQuoteOveridesClientRatesOverideCompanyTariff_NoRatesMatch()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);

			SetupGlobalCharges();

			NewClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 0);

			var quote = Helper.NewQuote(NewClient);
			var quoteEntry = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var page = new PricingPage(quoteEntry, Factory, PricingPageStyle.Standard);

			var docEntryQuotation = DocEntryQuotation.New(page, Factory);
			var collection = docEntryQuotation.DestinationDocRateLineItems;

			AssertEquals("No rate line items as client is not set up to use tariff", 0, collection.Count);
		}

		public void TestQuoteOveridesClientRatesOverideCompanyTariff_TariffMatch()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);

			SetupGlobalCharges();

			NewClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			var quote = Helper.NewQuote(NewClient);
			var quoteEntry = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var page = new PricingPage(quoteEntry, Factory, PricingPageStyle.Standard);

			var docEntryQuotation = DocEntryQuotation.New(page, Factory);
			var collection = docEntryQuotation.DestinationDocRateLineItems;

			AssertEquals("3 rate line items", 3, collection.Count);
			AssertEquals("from the company tariff", "78.00", collection.FindByDescAndAmount(ZGuid.Empty, GlobalDestination1.AC_Desc, "").Amount);
			AssertNotNull("line item exists", collection.FindByDescAndAmount(ZGuid.Empty, GlobalDestination3.AC_Desc, ""));
			AssertNotNull("line item exists", collection.FindByDescAndAmount(ZGuid.Empty, GlobalOverride.AC_Desc, ""));
		}

		public void TestQuoteOveridesClientRatesOverideCompanyTariff_ClientRate()
		{
			#region Set up

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);

			SetupGlobalCharges();

			NewClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			RateEntry clientEntry = Helper.NewClientRate(NewClient).AddRateEntry("DST", "ALL", "", "USLAX");

			RateLine clientDestinationLine1 = AddNewRateLine(clientEntry, GlobalDestination1, Constants.CurrencyCodes.UnitedStates, "");
			clientDestinationLine1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)131m;

			RateLine clientDestinationLine2 = AddNewRateLine(clientEntry, GlobalDestination2, Constants.CurrencyCodes.UnitedStates, "");
			clientDestinationLine2.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)24m;

			#endregion

			var quote = Helper.NewQuote(NewClient);
			var quoteEntry = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var page = new PricingPage(quoteEntry, Factory, PricingPageStyle.Standard);

			var docEntryQuotation = DocEntryQuotation.New(page, Factory);
			var collection = docEntryQuotation.DestinationDocRateLineItems;

			AssertEquals("rate line items from tariff + client rate", 4, collection.Count);
			AssertEquals("Amount taken from client rate", "131.00", collection.FindByDescAndAmount(ZGuid.Empty, GlobalDestination1.AC_Desc, "").Amount);
			AssertNotNull("line item exists from client rate", collection.FindByDescAndAmount(ZGuid.Empty, GlobalDestination2.AC_Desc, "24.00"));
			AssertNotNull("line item exists from tariff", collection.FindByDescAndAmount(ZGuid.Empty, GlobalDestination3.AC_Desc, ""));
			AssertNotNull("line item exists from tariff", collection.FindByDescAndAmount(ZGuid.Empty, GlobalOverride.AC_Desc, ""));
		}

		public void TestQuoteOveridesClientRatesOverideCompanyTariff_ClientRateAndQuote()
		{
			#region Set up

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);

			SetupGlobalCharges();

			NewClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			var clientEntry = Helper.NewClientRate(NewClient).AddRateEntry("DST", "ALL", "", "USLAX");
			var clientLine1 = AddNewRateLine(clientEntry, GlobalDestination1, Constants.CurrencyCodes.UnitedStates, "");
			clientLine1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)131m;
			var clientLine2 = AddNewRateLine(clientEntry, GlobalDestination2, Constants.CurrencyCodes.UnitedStates, "");
			clientLine2.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)24m;

			var quote = Helper.NewQuote(NewClient);
			var quoteEntry = quote.AddRateEntry("DST", "ALL", "", "USLAX");
			var quoteLine = AddNewRateLine(quoteEntry, GlobalDestination1, Constants.CurrencyCodes.UnitedStates, "");
			quoteLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)100m;

			#endregion

			var quoteEntryForPricingPage = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var page = new PricingPage(quoteEntryForPricingPage, Factory, PricingPageStyle.Standard);

			var docEntryQuotation = DocEntryQuotation.New(page, Factory);
			var collection = docEntryQuotation.DestinationDocRateLineItems;

			AssertEquals("rate line items from tariff + rate + quote", 4, collection.Count);
			AssertEquals("Amount taken from quote", "100.00", collection.FindByDescAndAmount(ZGuid.Empty, GlobalDestination1.AC_Desc, "").Amount);
			AssertNotNull("line item exists from client rate", collection.FindByDescAndAmount(ZGuid.Empty, GlobalDestination2.AC_Desc, "24.00"));
			AssertNotNull("line item exists from tariff", collection.FindByDescAndAmount(ZGuid.Empty, GlobalDestination3.AC_Desc, ""));
			AssertNotNull("line item exists from tariff", collection.FindByDescAndAmount(ZGuid.Empty, GlobalOverride.AC_Desc, ""));
		}

		public void TestQuotationHeadingIncludesIncoTerm()
		{
			GlbCompany.CurrentCompany.SetCountry("AU"); // relies on Sydney/Melbourne being in Aus - has affect on heading text

			Quote testQuote = Factory.New<Quote>();
			RateEntry airEntry1 = testQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			airEntry1.TI_QuotePageIncoTerm = "EXW";
			DocEntryQuotation testDocQuotation1 = DocEntryQuotation.New(new PricingPage(airEntry1, Factory, PricingPageStyle.Standard), Factory);
			AssertEquals("Heading includes INCOTERM", "Air Freight from Sydney to Los Angeles, US (EXW)", testDocQuotation1.PageHeading);
			Factory.Save();
			testQuote.TH_OneTimeQuote = true;
			testQuote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "AUSYD";
			testQuote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = "USLAX";
			AssertEquals("Heading includes INCOTERM", "Air Freight from Sydney to Los Angeles, US", testDocQuotation1.PageHeading);

			testQuote.CurrentOneOffQuote.TT_IncoTerm = "FOB";
			AssertEquals("Heading includes INCOTERM", "Air Freight from Sydney to Los Angeles, US (FOB)", testDocQuotation1.PageHeading);
			testQuote.TH_OneTimeQuote = false;

			RateEntry airEntry2 = testQuote.AddRateEntry("AIR", "LSE", "AUMEL", "USLAX");
			DocEntryQuotation testDocQuotation2 = DocEntryQuotation.New(new PricingPage(airEntry2, Factory, PricingPageStyle.Standard), Factory);
			AssertEquals("Heading includes INCOTERM", "Air Freight from Melbourne to Los Angeles, US", testDocQuotation2.PageHeading);

			RateEntry airEntry3 = testQuote.AddRateEntry("AIR", "LSE", "AUMEL", "USLAX");
			airEntry3.TI_ViaLRC = "SGSIN";
			airEntry3.TI_QuotePageIncoTerm = "FOB";
			DocEntryQuotation testDocQuotation3 = DocEntryQuotation.New(new PricingPage(airEntry3, Factory, PricingPageStyle.Standard), Factory);
			AssertEquals("Heading includes INCOTERM", "Air Freight from Melbourne to Los Angeles, US via Singapore (FOB)", testDocQuotation3.PageHeading);

			RateEntry airEntry4 = testQuote.AddRateEntry("AIR", "LSE", "AUMEL", "USLAX");
			airEntry4.TI_ViaLRC = "SGSIN";
			airEntry4.TI_QuotePageIncoTerm = "CIF";
			airEntry4.TI_PageHeading = "My Custom Heading";
			DocEntryQuotation testDocQuotation4 = DocEntryQuotation.New(new PricingPage(airEntry4, Factory, PricingPageStyle.Standard), Factory);
			AssertEquals("Custom Heading includes INCOTERM", "My Custom Heading (CIF)", testDocQuotation4.PageHeading);
		}

		public void TestIncoTermForOneOffQuote()
		{
			Quote testQuote = Factory.New<Quote>();
			RateEntry airEntry1 = testQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			airEntry1.TI_QuotePageIncoTerm = "EXW";

			DocEntryQuotation testDocQuotation = DocEntryQuotation.New(new PricingPage(airEntry1, Factory, PricingPageStyle.Standard), Factory);
			AssertEquals("EXW", testDocQuotation.IncoTerm);
			AssertEquals("Ex Works", testDocQuotation.IncoTermDescription);
			Factory.Save();
			testQuote.TH_OneTimeQuote = true;
			AssertEquals("", testDocQuotation.IncoTerm);
			AssertEquals("", testDocQuotation.IncoTermDescription);

			testQuote.OneOffQuote[0].TT_IncoTerm = "FOB";
			AssertEquals("FOB", testDocQuotation.IncoTerm);
			AssertEquals("Free On Board", testDocQuotation.IncoTermDescription);

			testQuote.TH_OneTimeQuote = false;
			AssertEquals("EXW", testDocQuotation.IncoTerm);
			AssertEquals("Ex Works", testDocQuotation.IncoTermDescription);
		}

		public void TestCarrierForOneOffQuote()
		{
			DocumentsDataRegistry.Instance.AirFreightIncludeTransportProviderOnQuotation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			OrgHeader carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_FullName = "Carrier 1";

			OrgHeader carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_FullName = "Carrier 2";

			Quote testQuote = Factory.New<Quote>();
			RateEntry airEntry1 = testQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			airEntry1.TI_OH_TransportProvider = carrier1.PK;

			DocEntryQuotation testDocQuotation = DocEntryQuotation.New(new PricingPage(airEntry1, Factory, PricingPageStyle.Standard), Factory);
			AssertEquals("Carrier 1", testDocQuotation.Provider);
			Factory.Save();
			testQuote.TH_OneTimeQuote = true;

			testQuote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "AUSYD";
			testQuote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = "USLAX";
			testQuote.CurrentOneOffQuote.TT_OH_Carrier = carrier1.PK;

			AssertEquals("Defaulted when marking the quote as a one off", "Carrier 1", testDocQuotation.Provider);

			testQuote.OneOffQuote[0].TT_OH_Carrier = carrier2.PK;
			AssertEquals("Carrier 2", testDocQuotation.Provider);

			testQuote.TH_OneTimeQuote = false;
			AssertEquals("Carrier 1", testDocQuotation.Provider);
		}

		public void TestFrequencyTransitFromHeader()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			SetupFCLOriginCharges();
			SetupFCLQuoteEntries();
			SetupFCLGlobalCharges();

			QuoteFormatEntryCollection collection = TestQuote.QuoteFormatEntries;
			collection.LoadEntries();
			Factory.Save();

			DocEntryQuotation testDocQuotation = DocEntryQuotation.New(new PricingPage(collection[0], Factory, PricingPageStyle.Standard), Factory);
			DocQuotationLineCollection originCollection = testDocQuotation.OriginDocRateLineItems;
			DocQuotationLineCollection freightCollection = testDocQuotation.FreightDocRateLineItems;
			DocQuotationLineCollection destinationCollection = testDocQuotation.DestinationDocRateLineItems;
			Factory.Save();

			AssertEquals("Transit Time", "14 Days", testDocQuotation.TransitTime);
			AssertEquals("Frequencey", "2 per Week", testDocQuotation.Frequency);

			collection[0].TI_TransitTime = "";
			collection[0].TI_Frequency = 0;
			collection[0].TI_FrequencyUnit = "";

			testDocQuotation = DocEntryQuotation.New(new PricingPage(collection[0], Factory, PricingPageStyle.Standard), Factory);

			AssertEquals("Transit Time", "", testDocQuotation.TransitTime);
			AssertEquals("Frequencey", "", testDocQuotation.Frequency);

			Quote quote = Factory.New<Quote>();

			RateOneOffShipment oneOff = Factory.New<RateOneOffShipment>();
			oneOff.TT_Frequency = 7;
			oneOff.TT_FrequencyUnit = RatingConstants.FrequencyUnits.Fortnight;
			oneOff.TT_TransitTime = RatingConstants.TransitTimes.Overnight;

			quote.OneOffQuote.Add(oneOff);
			collection[0].TI_TH = quote.PK;
			testDocQuotation = DocEntryQuotation.New(new PricingPage(collection[0], Factory, PricingPageStyle.Standard), Factory);

			AssertEquals("Transit Time", "Overnight", testDocQuotation.TransitTime);
			AssertEquals("Frequencey", "7 per Fortnight", testDocQuotation.Frequency);

			oneOff.TT_TransitTime = "8";
			AssertEquals("Transit Time", "8 Days", testDocQuotation.TransitTime);
		}

		[TestDate(2014, 10, 17, 12, 11, 0)]
		public void TestQuotationHeaderForFCL()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			SetupFCLOriginCharges();
			SetupFCLQuoteEntries();
			SetupFCLGlobalCharges();

			Factory.Save();

			PricingPageCollection collection = new PricingPageCollection(TestQuote);
			collection.Load(PricingPaginationStrategy.StandardStyle);

			DocEntryQuotation testDocQuotation = DocEntryQuotation.New(collection[0], Factory);
			DocQuotationLineCollection originCollection = testDocQuotation.OriginDocRateLineItems;
			DocQuotationLineCollection freightCollection = testDocQuotation.FreightDocRateLineItems;
			DocQuotationLineCollection destinationCollection = testDocQuotation.DestinationDocRateLineItems;

			AssertEquals("Page Open Text", "FCL Freight from Sydney to Los Angeles, US", testDocQuotation.PageHeading);
			AssertEquals("Valid Until", ZDateTime.Today.AddMonths(1).ToString("d MMM yyyy"), testDocQuotation.ValidUntil);
			AssertEquals("Transit Time", "14 Days", testDocQuotation.TransitTime);
			AssertEquals("Date", ZDateTime.Today.ToString("d MMM yyyy"), testDocQuotation.Date);
			AssertEquals("Service Level", "Standard", testDocQuotation.ServiceLevel);
			AssertEquals("Commodity Code", "General", testDocQuotation.CommodityCode);
			AssertEquals("Frequency", "2 per Week", testDocQuotation.Frequency);
			AssertEquals("Quote Number", "999/A - NEWTESSYD", testDocQuotation.QuoteNumberAndClientCode);

			AssertEquals("Origin Doc Rate Lines Count", 7, originCollection.Count);
			AssertEquals("Freight Doc Rate Lines Count", 3, freightCollection.Count);
			AssertEquals("Destination Doc Rate Lines Count", 3, destinationCollection.Count);

			DocRateLineItem item = originCollection.FindByDescAndAmount(ExportCustomsPK, "Customs Entry", "50");
			AssertDocRateLineItem(item, "Customs Entry", Constants.CurrencyCodes.Australia, "50.00", "");

			item = originCollection.FindByDescAndAmount(DocumentationPK, "Documentation", "30");
			AssertDocRateLineItem(item, "Documentation", Constants.CurrencyCodes.Australia, "30.00", "per container");

			item = originCollection.FindByDescAndAmount(ZGuid.Empty, "Lift On/Lift Off", "");
			AssertDocRateLineItem(item, "Lift On/Lift Off", "", "", "");

			item = originCollection.FindByDescAndAmount(LiftOnLiftOff20GPPK, "20GP", "125");
			AssertDocRateLineItem(item, "20GP", Constants.CurrencyCodes.Australia, "125.00", "per container");

			item = originCollection.FindByDescAndAmount(LiftOnLiftOff40GPPK, "40GP", "");
			AssertDocRateLineItem(item, "40GP", "", "", "");

			item = originCollection.FindByDescAndAmount(LiftOnLiftOff40GPPK, "Minimum", "500");
			AssertDocRateLineItem(item, "Minimum", Constants.CurrencyCodes.Australia, "500.00", "");

			item = originCollection.FindByDescAndAmount(LiftOnLiftOff40GPPK, "Per Unit", "250");
			AssertDocRateLineItem(item, "Per Unit", Constants.CurrencyCodes.Australia, "250.00", "per container");

			item = freightCollection.FindByDescAndAmount(ZGuid.Empty, "Freight", "");
			AssertDocRateLineItem(item, "Freight", "", "", "");

			item = freightCollection.FindByDescAndAmount(FCLFreightPK1, "20GP", "2500.00");
			AssertDocRateLineItem(item, "20GP", Constants.CurrencyCodes.Australia, "2500.00", "per container");

			item = freightCollection.FindByDescAndAmount(FCLFreightPK2, "40GP", "4000.00");
			AssertDocRateLineItem(item, "40GP", Constants.CurrencyCodes.Australia, "4000.00", "per container");

			item = destinationCollection.FindByDescAndAmount(ZGuid.Empty, "Global FCL Charge", "");
			AssertDocRateLineItem(item, "Global FCL Charge", "", "", "");

			item = destinationCollection.FindByDescAndAmount(GlobalFCLLine1PK, "20GP", "60.00");
			AssertDocRateLineItem(item, "20GP", Constants.CurrencyCodes.UnitedStates, "60.00", "per container");

			item = destinationCollection.FindByDescAndAmount(GlobalFCLLine2PK, "40GP", "120.00");
			AssertDocRateLineItem(item, "40GP", Constants.CurrencyCodes.UnitedStates, "120.00", "per container");
		}

		public void TestQuotationDifferentOriginDestination()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			RateEntry entry = AddEntryToQuote("AIR", "LSE", "AU", "USCA", "STD", "GEN", "", Freight, UnitCalculator.Code, 5m);
			AddEntryToQuote("ORG", "AIR", "AUEC", "", "", "", "", Documentation, FlatCalculator.Code, 50m);
			AddEntryToQuote("ORG", "AIR", "AUSYD", "", "", "", "", Documentation, FlatCalculator.Code, 80m);
			AddEntryToQuote("DST", "AIR", "", "USLAX", "", "", "", Fumigation, FlatCalculator.Code, 50m);
			AddEntryToQuote("DST", "AIR", "", "US", "", "", "", Fumigation, FlatCalculator.Code, 80m);

			DocEntryQuotation testDocQuotation = DocEntryQuotation.New(new PricingPage(entry, Factory, PricingPageStyle.Standard), Factory);
			Factory.Save();
			DocQuotationLineCollection originCollection = testDocQuotation.OriginDocRateLineItems;
			DocQuotationLineCollection freightCollection = testDocQuotation.FreightDocRateLineItems;
			DocQuotationLineCollection destinationCollection = testDocQuotation.DestinationDocRateLineItems;

			AssertEquals(1, freightCollection.Count);
			AssertEquals(2, originCollection.Count);
			AssertEquals(2, destinationCollection.Count);

			if (originCollection[0].Origin == "Sydney")
			{
				AssertEquals("Australia East Coast", originCollection[1].Origin);
			}
			else
			{
				AssertEquals("Australia East Coast", originCollection[0].Origin);
				AssertEquals("Sydney", originCollection[1].Origin);
			}

			if (destinationCollection[0].Destination == "Los Angeles")
			{
				AssertEquals("US West Coast (California)", destinationCollection[1].Destination);
			}
			else
			{
				AssertEquals("US West Coast (California)", destinationCollection[0].Destination);
				AssertEquals("Los Angeles", destinationCollection[1].Destination);
			}
		}

		public void TestQuotationFreightRegionsOriginDestPorts()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			RateEntry entry = AddEntryToQuote("AIR", "LSE", "AUEC", "USCA", "STD", "GEN", "", Freight, UnitCalculator.Code, 5m);
			AddEntryToQuote("ORG", "AIR", "AUSYD", "USLAX", "", "", "", Documentation, FlatCalculator.Code, 50m);
			AddEntryToQuote("DST", "AIR", "AUSYD", "USLAX", "", "", "", Fumigation, FlatCalculator.Code, 50m);

			DocEntryQuotation testDocQuotation = DocEntryQuotation.New(new PricingPage(entry, Factory, PricingPageStyle.Standard), Factory);
			Factory.Save();
			DocQuotationLineCollection originCollection = testDocQuotation.OriginDocRateLineItems;
			DocQuotationLineCollection freightCollection = testDocQuotation.FreightDocRateLineItems;
			DocQuotationLineCollection destinationCollection = testDocQuotation.DestinationDocRateLineItems;

			AssertEquals(1, freightCollection.Count);
			AssertEquals(1, originCollection.Count);
			AssertEquals(1, destinationCollection.Count);

			AssertEquals("Sydney", originCollection[0].Origin);
			AssertEquals("Los Angeles", destinationCollection[0].Destination);
		}

		public void TestQuotationFreightRegionsOriginDestPortsAndRegions()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			RateEntry entry = AddEntryToQuote("AIR", "LSE", "AUEC", "USCA", "STD", "GEN", "", Freight, UnitCalculator.Code, 5m);
			AddEntryToQuote("ORG", "AIR", "AUEC", "", "", "", "", Documentation, FlatCalculator.Code, 50m);
			AddEntryToQuote("ORG", "AIR", "AUSYD", "", "", "", "", Documentation, FlatCalculator.Code, 80m);
			AddEntryToQuote("DST", "AIR", "", "USLAX", "", "", "", Fumigation, FlatCalculator.Code, 50m);
			AddEntryToQuote("DST", "AIR", "", "US", "", "", "", Fumigation, FlatCalculator.Code, 80m);

			DocEntryQuotation testDocQuotation = DocEntryQuotation.New(new PricingPage(entry, Factory, PricingPageStyle.Standard), Factory);
			Factory.Save();
			DocQuotationLineCollection originCollection = testDocQuotation.OriginDocRateLineItems;
			DocQuotationLineCollection freightCollection = testDocQuotation.FreightDocRateLineItems;
			DocQuotationLineCollection destinationCollection = testDocQuotation.DestinationDocRateLineItems;

			AssertEquals(1, freightCollection.Count);
			AssertEquals(2, originCollection.Count);
			AssertEquals(2, destinationCollection.Count);

			if (originCollection[0].Origin == "Sydney")
			{
				AssertEquals("Australia East Coast", originCollection[1].Origin);
			}
			else
			{
				AssertEquals("Australia East Coast", originCollection[0].Origin);
				AssertEquals("Sydney", originCollection[1].Origin);
			}

			if (destinationCollection[0].Destination == "Los Angeles")
			{
				AssertEquals("US West Coast (California)", destinationCollection[1].Destination);
			}
			else
			{
				AssertEquals("US West Coast (California)", destinationCollection[0].Destination);
				AssertEquals("Los Angeles", destinationCollection[1].Destination);
			}
		}

		public void TestQuotationFreightCountriesOriginDestPortsAndCountries()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			RateEntry entry = AddEntryToQuote("AIR", "LSE", "AU", "US", "STD", "GEN", "", Freight, UnitCalculator.Code, 5m);
			AddEntryToQuote("ORG", "AIR", "AUSYD", "", "", "", "", Documentation, FlatCalculator.Code, 80m);
			AddEntryToQuote("DST", "AIR", "", "USLAX", "", "", "", Fumigation, FlatCalculator.Code, 50m);
			AddEntryToQuote("DST", "AIR", "", "US", "", "", "", Fumigation, FlatCalculator.Code, 80m);

			DocEntryQuotation testDocQuotation = DocEntryQuotation.New(new PricingPage(entry, Factory, PricingPageStyle.Standard), Factory);
			Factory.Save();
			DocQuotationLineCollection originCollection = testDocQuotation.OriginDocRateLineItems;
			DocQuotationLineCollection freightCollection = testDocQuotation.FreightDocRateLineItems;
			DocQuotationLineCollection destinationCollection = testDocQuotation.DestinationDocRateLineItems;

			AssertEquals(1, freightCollection.Count);
			AssertEquals(1, originCollection.Count);
			AssertEquals(2, destinationCollection.Count);

			AssertEquals("Sydney", originCollection[0].Origin);

			if (destinationCollection[0].Destination == "Los Angeles")
			{
				AssertEquals("United States", destinationCollection[1].Destination);
			}
			else
			{
				AssertEquals("United States", destinationCollection[0].Destination);
				AssertEquals("Los Angeles", destinationCollection[1].Destination);
			}
		}

		public void TestQuotationFreightPortsOriginDestPortsAndRegions()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			RateEntry entry = AddEntryToQuote("AIR", "LSE", "AUSYD", "USLAX", "STD", "GEN", "", Freight, UnitCalculator.Code, 5m);
			AddEntryToQuote("ORG", "AIR", "AUEC", "", "", "", "", Documentation, FlatCalculator.Code, 50m);
			AddEntryToQuote("ORG", "AIR", "AUSYD", "", "", "", "", Documentation, FlatCalculator.Code, 80m);
			AddEntryToQuote("DST", "AIR", "", "USLAX", "", "", "", Fumigation, FlatCalculator.Code, 50m);
			AddEntryToQuote("DST", "AIR", "", "US", "", "", "", Fumigation, FlatCalculator.Code, 80m);

			DocEntryQuotation testDocQuotation = DocEntryQuotation.New(new PricingPage(entry, Factory, PricingPageStyle.Standard), Factory);
			Factory.Save();
			DocQuotationLineCollection originCollection = testDocQuotation.OriginDocRateLineItems;
			DocQuotationLineCollection freightCollection = testDocQuotation.FreightDocRateLineItems;
			DocQuotationLineCollection destinationCollection = testDocQuotation.DestinationDocRateLineItems;

			AssertEquals(1, freightCollection.Count);
			AssertEquals(1, originCollection.Count);
			AssertEquals(1, destinationCollection.Count);

			AssertEquals("Sydney", originCollection[0].Origin);
			AssertEquals("Los Angeles", destinationCollection[0].Destination);
		}

		public void TestQuotationFreightPortsOriginDestRegionsOnly()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			RateEntry entry = AddEntryToQuote("AIR", "LSE", "AUSYD", "USLAX", "STD", "GEN", "", Freight, UnitCalculator.Code, 5m);
			AddEntryToQuote("ORG", "AIR", "AUEC", "", "", "", "", Documentation, FlatCalculator.Code, 50m);
			AddEntryToQuote("DST", "AIR", "", "US", "", "", "", Fumigation, FlatCalculator.Code, 80m);

			DocEntryQuotation testDocQuotation = DocEntryQuotation.New(new PricingPage(entry, Factory, PricingPageStyle.Standard), Factory);
			Factory.Save();
			DocQuotationLineCollection originCollection = testDocQuotation.OriginDocRateLineItems;
			DocQuotationLineCollection freightCollection = testDocQuotation.FreightDocRateLineItems;
			DocQuotationLineCollection destinationCollection = testDocQuotation.DestinationDocRateLineItems;

			AssertEquals(1, freightCollection.Count);
			AssertEquals(1, originCollection.Count);
			AssertEquals(1, destinationCollection.Count);

			AssertEquals("Sydney", originCollection[0].Origin);
			AssertEquals("Los Angeles", destinationCollection[0].Destination);
		}

		public void TestQuotationDifferentOriginDestinationFCL()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			#region Freight

			RateEntry fclEntry1 = TestQuote.AddRateEntry("FCL", "SEA", "AU", "USCA", "STD", "20GP");
			fclEntry1.RateLines.RemoveAndDeleteAll();

			RateLine fclEntry1a = AddNewRateLine(fclEntry1, Freight, Constants.CurrencyCodes.Australia, RatingConstants.Units.KG);
			fclEntry1a.TL_RateCalculator = UnitCalculator.Code;
			fclEntry1a.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1000m;

			RateEntry fclEntry2 = TestQuote.AddRateEntry("FCL", "SEA", "AU", "USCA", "STD", "40GP");
			fclEntry2.RateLines.RemoveAndDeleteAll();

			RateLine fclEntry2a = AddNewRateLine(fclEntry2, Freight, Constants.CurrencyCodes.Australia, RatingConstants.Units.KG);
			fclEntry2a.TL_RateCalculator = UnitCalculator.Code;
			fclEntry2a.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2000m;

			#endregion

			#region Origin

			RateEntry oRGEntry3 = TestQuote.AddRateEntry("ORG", "FCL", "AUEC", "", "", "20GP");
			RateLine oRGLine3a = AddNewRateLine(oRGEntry3, Documentation, Constants.CurrencyCodes.Australia, "");
			oRGLine3a.TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)oRGLine3a.Calculator).BaseRate = 50m;

			RateEntry oRGEntry4 = TestQuote.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "20GP");
			RateLine oRGLine4a = AddNewRateLine(oRGEntry4, Documentation, Constants.CurrencyCodes.Australia, "");
			oRGLine4a.TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)oRGLine4a.Calculator).BaseRate = 80m;

			RateEntry oRGEntry5 = TestQuote.AddRateEntry("ORG", "FCL", "AUEC", "", "", "40GP");
			RateLine oRGLine5a = AddNewRateLine(oRGEntry5, Documentation, Constants.CurrencyCodes.Australia, "");
			oRGLine5a.TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)oRGLine5a.Calculator).BaseRate = 100m;

			RateEntry oRGEntry6 = TestQuote.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "40GP");
			RateLine oRGLine6a = AddNewRateLine(oRGEntry6, Documentation, Constants.CurrencyCodes.Australia, "");
			oRGLine6a.TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)oRGLine6a.Calculator).BaseRate = 120m;

			#endregion

			#region Destination

			RateEntry dSTEntry3 = TestQuote.AddRateEntry("DST", "FCL", "", "USLAX");
			RateLine dSTLine3a = AddNewRateLine(dSTEntry3, Fumigation, Constants.CurrencyCodes.UnitedStates, "");
			dSTLine3a.TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)dSTLine3a.Calculator).BaseRate = 50m;

			RateEntry dSTEntry4 = TestQuote.AddRateEntry("DST", "FCL", "", "US");
			RateLine dSTLine4a = AddNewRateLine(dSTEntry4, Fumigation, Constants.CurrencyCodes.UnitedStates, "");
			dSTLine4a.TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)dSTLine4a.Calculator).BaseRate = 80m;

			#endregion

			QuoteFormatEntryCollection collection = TestQuote.QuoteFormatEntries;
			collection.LoadEntries();
			Factory.Save();

			PricingPage page = new PricingPage(collection[0], Factory, PricingPageStyle.Standard);
			page.ContainerSet.Add(fclEntry1.Container);
			page.ContainerSet.Add(fclEntry2.Container);

			DocEntryQuotation testDocQuotation = DocEntryQuotation.New(page, Factory);
			DocQuotationLineCollection originCollection = testDocQuotation.OriginDocRateLineItems;
			DocQuotationLineCollection freightCollection = testDocQuotation.FreightDocRateLineItems;
			DocQuotationLineCollection destinationCollection = testDocQuotation.DestinationDocRateLineItems;

			AssertEquals(6, originCollection.Count);
			AssertEquals(2, destinationCollection.Count);

			if (originCollection[0].Origin == "Sydney")
			{
				AssertEquals("Sydney", originCollection[1].Origin);
				AssertEquals("Sydney", originCollection[2].Origin);
				AssertEquals("Australia East Coast", originCollection[3].Origin);
				AssertEquals("Australia East Coast", originCollection[4].Origin);
				AssertEquals("Australia East Coast", originCollection[5].Origin);
			}
			else
			{
				AssertEquals("Australia East Coast", originCollection[0].Origin);
				AssertEquals("Australia East Coast", originCollection[1].Origin);
				AssertEquals("Australia East Coast", originCollection[2].Origin);
				AssertEquals("Sydney", originCollection[3].Origin);
				AssertEquals("Sydney", originCollection[4].Origin);
				AssertEquals("Sydney", originCollection[5].Origin);
			}

			if (destinationCollection[0].Destination == "Los Angeles")
			{
				AssertEquals("US West Coast (California)", destinationCollection[1].Destination);
			}
			else
			{
				AssertEquals("US West Coast (California)", destinationCollection[0].Destination);
				AssertEquals("Los Angeles", destinationCollection[1].Destination);
			}
		}

		#region Implementation

		void AssertDocRateLineItem(DocRateLineItem lineItem, ZString description, ZString currencyCode, ZString amount, ZString units)
		{
			AssertEquals("Description", description.ToUpper(), lineItem.Description.ToUpper());
			AssertEquals("CurrencyCode", currencyCode.ToUpper(), lineItem.Currency.ToUpper());
			AssertEquals("Amount", amount.ToUpper(), lineItem.Amount.ToUpper());
			AssertEquals("Units", units.ToUpper(), lineItem.Units.ToUpper());
		}

		RateEntry AddEntryToQuote(string mode, string fCL_LCL, string origin, string dest, string svcLvl, string commodity, string container, AccChargeCode chargeCode, string calcCode, ZDecimal rate)
		{
			RateEntry entry = TestQuote.AddRateEntry(mode, fCL_LCL, origin, dest, svcLvl, container);
			entry.RateLines.RemoveAndDeleteAll();

			RateLine line = AddNewRateLine(entry, chargeCode, Constants.CurrencyCodes.Australia, RatingConstants.Units.KG);
			line.TL_RateCalculator = calcCode;
			if (calcCode == UnitCalculator.Code)
			{
				line.Calculator[Calculator.Items.Operator.UNT] = rate;
			}
			else if (calcCode == FlatCalculator.Code)
			{
				line.Calculator[Calculator.Items.Operator.BAS] = rate;
			}

			return entry;
		}

		protected override void SetUp()
		{
			base.SetUp();
			string query = @"	DELETE FROM dbo.RateAttachment; 
								DELETE FROM dbo.RateOneOffContainers; 
								DELETE FROM dbo.RateOneOffShipment; 
								DELETE FROM dbo.RateLineItems; 
								DELETE FROM dbo.RateLines; 
								DELETE FROM dbo.RateEntry; 
								DELETE FROM dbo.RatingHeader;";

			CargoWise.Data.Db.Connection.ExecuteNonQuery(query); // Need to delete data for tests to work
			SetupChargeCodes();
			SetupCostingRates();

			NewClient.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceDays = 6;
			NewClient.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = "INV";
			NewClient.OH_IsConsignor = true;
			Factory.Save();

			TestQuote = Factory.New<Quote>();
			TestQuote.TH_OH = NewClient.PK;
			TestQuote.TH_QuoteNumber = "0000999";
			TestQuote.Header.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			originalAlternateRateFormatValue = DocumentsDataRegistry.Instance.AlternativeRateFormat.Value;
			DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalAlternateRateFormatValue);
			base.TearDown();
		}

		AccTaxRate TaxRate
		{
			get
			{
				if (taxRate == null)
				{
					taxRate = Factory.NewWithValidTestData<AccTaxRate>();
					taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					taxRate.AT_Description = "Test GST Rate";
					taxRate.AT_IsActive = true;
					taxRate.SetRateNumerator_ForTestOnly(10);
				}
				return taxRate;
			}
		}
		AccTaxRate taxRate;

		OrgHeader CreateOrgHeader(ZString name, ZString streetAddress, ZString city, ZString closestPort)
		{
			var newOrgHeader = Factory.New<OrgHeader>();
			newOrgHeader.OH_FullName = name;
			newOrgHeader.MainAddress.OA_Address1 = streetAddress;
			newOrgHeader.MainAddress.OA_City = city;
			newOrgHeader.OH_RL_NKClosestPort = closestPort;
			return newOrgHeader;
		}

		void SetupChargeCodes()
		{
			Documentation = Helper.ChargeCodes.New("TDOCFEE", "Documentation", UnitCalculator.Code, "ORG");
			CartageChargeCode = Helper.ChargeCodes.New("TCARTAGE", "Cartage", CartageCalculator.Code);
			ExportCustoms = Helper.ChargeCodes.New("TECF", "Export Customs Formalities", CombinedCalculator.Code);
			SecurityTax = Helper.ChargeCodes.New("TSECTAX", "Security Tax", MinimumOrPerUnitCalculator.Code);
			SecurityTax.AC_AT_GSTRate = TaxRate.PK;
			Fumigation = Helper.ChargeCodes.New("TFUMIGA", "Fumigation", FlatCalculator.Code, "DST");
			HousebillReleaseType = Helper.ChargeCodes.New("TESTHRT", "Housebill", HousebillReleaseTypeCalculator.Code);

			ExportSecurity = Helper.ChargeCodes.New("TESF", "Export Security Fee", DisbursementInterestCalculator.Code);
			DummyChargeCode = Helper.ChargeCodes.New("TDUMMY", "Dummy Charge Code", CombinedCalculator.Code);
			LiftOnLiftOff = Helper.ChargeCodes.New("TLILO", "Lift On/Lift Off", MinimumOrPerUnitCalculator.Code, "ORG");
			ItalianAirportTax = Helper.ChargeCodes.New("TIAT", "Italian Airport Tax", PackageCountCalculator.Code);

			Freight = Helper.ChargeCodes.New("TFREIGHT", "Freight", CombinedCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			FuelSurchage = Helper.ChargeCodes.New("TFUEL", "Fuel Surcharge", PercentageCalculator.Code);
			FuelSurchage.AC_AT_GSTRate = TaxRate.PK;
			FuelSurchage2 = Helper.ChargeCodes.New("TFUEL2", "Fuel Surcharge 2", PercentageCalculator.Code);
			FuelSurchage3 = Helper.ChargeCodes.New("TFUEL3", "Fuel Surcharge 3", PercentageCalculator.Code);
			BAF = Helper.ChargeCodes.New("TBAF", "Bunker Adjustment Fee", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			BAFNotShownIfZero = Helper.ChargeCodes.New("BAFZ", "Bunker Adjustment Fee Not Shown", FlatCalculator.Code, suppressIfZero: true);

			DeliveryOrderFee = Helper.ChargeCodes.New("TDOF", "Delivery Order Fee", UnitCalculator.Code);
			DeliveryOrderFee.AC_AT_GSTRate = TaxRate.PK;
			CustomsEntry = Helper.ChargeCodes.New("TCUSENTRY", "Customs Entry", FlatCalculator.Code, "ORG");
			PortCharges = Helper.ChargeCodes.New("TPORT", "Port Charges", UnitCalculator.Code);
			TerminalHandlingCharge = Helper.ChargeCodes.New("TTHC", "Terminal Handling Fee", UnitCalculator.Code);
			ProfessionalIndemnityFee = Helper.ChargeCodes.New("TPIF", "Professional Indemnity Fee", FlatCalculator.Code);
			CargoAutomationFee = Helper.ChargeCodes.New("TCAF", "Cargo Automation Fee", FlatCalculator.Code);
			FPUWithNoFlatFee = Helper.ChargeCodes.New("TFPU1", "Flat Per Unit With No Flat", FlatPlusPerUnitCalculator.Code);
			FPUWithFlatFee = Helper.ChargeCodes.New("TFPU2", "Flat Per Unit With Flat", FlatPlusPerUnitCalculator.Code);
			Helper.ChargeCodes.New("TWTRHB", "Weight Range Calculator (House Bill)", CombinedCalculator.Code);

			VehicleBookingFee = Helper.ChargeCodes.New("TVBF", "Vehicle Booking Fee", UnitCalculator.Code);
			Postage = Helper.ChargeCodes.New("TPOSTAGE", "Postage", UnitCalculator.Code);
			Postage.AC_AT_GSTRate = TaxRate.PK;
			Agency1 = Helper.ChargeCodes.New("TAGENCY1", "Agency Charges 1", AgencyCalculator.Code);
			Agency2 = Helper.ChargeCodes.New("TAGENCY2", "Agency Charges 2", AgencyCalculator.Code);
			Agency3 = Helper.ChargeCodes.New("TAGENCY3", "Agency Charges 3", AgencyCalculator.Code);
			Agency4 = Helper.ChargeCodes.New("TAGENCY4", "Agency Charges 4", AgencyCalculator.Code);
			Agency5 = Helper.ChargeCodes.New("TAGENCY5", "Agency Charges 5", AgencyCalculator.Code);
			SingleEntryBond = Helper.ChargeCodes.New("TSEB", "Single Entry Bond Fee", PercentageCalculator.Code);
			SingleEntryBond2 = Helper.ChargeCodes.New("TSEB2", "Single Entry Bond Fee", PercentageCalculator.Code);

			GlobalOrigin1 = Helper.ChargeCodes.New("TGO1", "Global Origin 1", FlatCalculator.Code);
			GlobalOrigin2 = Helper.ChargeCodes.New("TGO2", "Global Origin 2", FlatPlusPerUnitCalculator.Code);
			GlobalOrigin3 = Helper.ChargeCodes.New("TGO3", "Global Origin 3", CombinedCalculator.Code);

			GlobalDestination1 = Helper.ChargeCodes.New("TGD1", "Global Destination 1", FlatCalculator.Code);
			GlobalDestination2 = Helper.ChargeCodes.New("TGD2", "Global Destination 2", MinimumOrPerUnitCalculator.Code);
			GlobalDestination3 = Helper.ChargeCodes.New("TGD3", "Global Destination 3", UnitCalculator.Code);
			GlobalOverride = Helper.ChargeCodes.New("TGD4", "Global Override", FlatCalculator.Code);
			GlobalFCL = Helper.ChargeCodes.New("TGDF", "Global FCL Charge", UnitCalculator.Code);
			NoteCode = Helper.ChargeCodes.New("NOTE", "Note Charge", NoteCalculator.Code);

			DontPrintChargeCode = Helper.ChargeCodes.New("DONTPRNT", "Don't Print On Quote", FlatCalculator.Code);
			DontPrintChargeCode.AC_ShowOnQuotation = false;

			Factory.Save();
		}
		void SetupSuppliers()
		{
			AlexandriaSupplier = CreateOrgHeader("Alexandria Supplier", "123 Fake Street", "Alexandria", "AUSYD");

			OrgAddress mainAddress = AlexandriaSupplier.MainAddress;
			mainAddress.OA_Address1 = "MainAddress";
			OrgAddress pickupAddress2 = AlexandriaSupplier.Addresses.AddNew();
			pickupAddress2.AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));
			pickupAddress2.OA_City = "Randwick";
			pickupAddress2.OA_Address1 = "PickupAddress2";

			HornsbySupplier = CreateOrgHeader("Hornsby Supplier", "456 Fake Street", "Hornsby", "AUSYD");
		}
		void SetupOriginCharges()
		{
			RateEntry lCLOriginEntry = TestQuote.AddRateEntry("ORG", "LCL", "AUSYD", "");

			RateEntry lCLOriginEntryAlexandria = TestQuote.AddRateEntry("ORG", "LCL", "AUSYD", "");
			lCLOriginEntryAlexandria.TI_OH_Consignor = AlexandriaSupplier.PK;
			lCLOriginEntryAlexandria.TI_OA_CartagePickupAddressOverride = AlexandriaSupplier.Addresses[0].PK;

			RateEntry lCLOriginEntryHornsby = TestQuote.AddRateEntry("ORG", "LCL", "AUSYD", "");
			lCLOriginEntryHornsby.TI_OH_Consignor = HornsbySupplier.PK;
			lCLOriginEntryHornsby.TI_OA_CartagePickupAddressOverride = HornsbySupplier.Addresses[0].PK;

			RateEntry aLLOriginEntry = TestQuote.AddRateEntry("ORG", "ALL", "AUSYD", "");
			RateEntry aIROriginEntry = TestQuote.AddRateEntry("ORG", "AIR", "AUSYD", "");

			#region Italian Airport Tax

			RateLine italianAirportTaxLine = AddNewRateLine(lCLOriginEntry, ItalianAirportTax, Constants.CurrencyCodes.EuropeanUnion, "");
			ItalianAirportTaxPK = italianAirportTaxLine.PK;
			((PackageCountCalculator)italianAirportTaxLine.Calculator).BaseRate = 23m;
			((PackageCountCalculator)italianAirportTaxLine.Calculator).PerKG = 0.05m;
			((PackageCountCalculator)italianAirportTaxLine.Calculator).FirstPackageRate = 0.25m;
			((PackageCountCalculator)italianAirportTaxLine.Calculator).AddtionalPackageRate = 0.12m;

			#endregion

			#region Documentation

			RateLine documentationLine = AddNewRateLine(lCLOriginEntry, Documentation, Constants.CurrencyCodes.Australia, RatingConstants.Units.HB);
			DocumentationPK = documentationLine.PK;
			documentationLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)75m;

			#endregion

			#region Cartage Ex Alexandria

			RateLine cartAlexandria = AddNewRateLine(lCLOriginEntryAlexandria, CartageChargeCode, Constants.CurrencyCodes.Australia, RatingConstants.Units.KG);
			CartAlexandriaPK = cartAlexandria.PK;
			((CartageCalculator)cartAlexandria.Calculator).EquipmentType = "STD";
			((CartageCalculator)cartAlexandria.Calculator).Minimum = 30m;
			((CartageCalculator)cartAlexandria.Calculator).PerUnit = 1.5m;

			#endregion

			#region Cartage Ex Hornsby

			RateLine cartHornsby = AddNewRateLine(lCLOriginEntryHornsby, CartageChargeCode, Constants.CurrencyCodes.Australia, RatingConstants.Units.KG);
			CartHornsbyPK = cartHornsby.PK;
			((CartageCalculator)cartHornsby.Calculator).EquipmentType = "SDL";
			((CartageCalculator)cartHornsby.Calculator).Minimum = 60m;
			cartHornsby.Calculator["-500"] = (ZDecimal)3.5m;
			cartHornsby.Calculator["+500"] = (ZDecimal)2.5m;

			#endregion

			#region Export Customs Formalities

			RateLine exportCustomsLine = AddNewRateLine(lCLOriginEntry, ExportCustoms, Constants.CurrencyCodes.Australia, RatingConstants.Units.M3);
			exportCustomsLine.UseOnlyActualWeightMeasure = true;
			ExportCustomsPK = exportCustomsLine.PK;
			exportCustomsLine.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)50M;
			exportCustomsLine.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 4m, 0m, 78m);
			exportCustomsLine.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 4m, 0m, 87.5m);
			exportCustomsLine.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 8m, 0m, 105.5m);

			#endregion

			#region Security Tax

			RateLine securityTaxLine = AddNewRateLine(lCLOriginEntry, SecurityTax, Constants.CurrencyCodes.Australia, RatingConstants.Units.M3);
			securityTaxLine.ConversionFactor = ConversionFactor.Standard.Metric.Sea;
			SecurityTaxPK = securityTaxLine.PK;

			securityTaxLine.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)11m;
			securityTaxLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal).08m;

			#endregion

			#region Fumigation

			RateLine fumigationLine = AddNewRateLine(lCLOriginEntry, Fumigation, Constants.CurrencyCodes.Australia, "");
			FumigationPK = fumigationLine.PK;
			fumigationLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)200m;

			#endregion

			#region Export Security

			RatingDataRegistry.Instance.CurrentPrimeRate.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 5.5M);

			RateLine exportSecurityLine = AddNewRateLine(lCLOriginEntry, ExportSecurity, Constants.CurrencyCodes.Australia, "");
			ExportSecurityPK = exportSecurityLine.PK;
			((DisbursementInterestCalculator)exportSecurityLine.Calculator).RateLineItems.AddNew().TM_Text = Calculator.Items.Value.DisbursementApplyToTypes.Disbursements;
			((DisbursementInterestCalculator)exportSecurityLine.Calculator).Uplift = 1.5m;

			#endregion

			#region Flat Plus Per Unit With No Flat

			RateLine fPUWithNoFlatLine = AddNewRateLine(lCLOriginEntry, FPUWithNoFlatFee, Constants.CurrencyCodes.Australia, RatingConstants.Units.M3);
			FlatPlusPerUnitWithNoFlatPK = fPUWithNoFlatLine.PK;
			fPUWithNoFlatLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)45.25m;

			#endregion

			#region Dummy Charge

			RateLine dummyChargeLine = AddNewRateLine(lCLOriginEntry, DummyChargeCode, Constants.CurrencyCodes.Australia, "");

			#endregion

			#region Fumigation (ALL)

			RateLine fumigationLineALL = AddNewRateLine(aLLOriginEntry, Fumigation, Constants.CurrencyCodes.Australia, "");
			fumigationLineALL.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)150m;

			#endregion

			#region Export Security (AIR)

			RateLine exportSecurityLineAIR = AddNewRateLine(aIROriginEntry, ExportSecurity, Constants.CurrencyCodes.Australia, "");
			exportSecurityLineAIR.TL_RateCalculator = FlatCalculator.Code;
			exportSecurityLineAIR.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)20m;

			#endregion

			#region Housebill Release Type

			RateLine housebillReleaseTypeLine = AddNewRateLine(aLLOriginEntry, HousebillReleaseType, Constants.CurrencyCodes.Australia, "");
			HousebillReleaseTypePK = housebillReleaseTypeLine.PK;
			AddNewRateLineItem(housebillReleaseTypeLine, "STD", 0, 10m, "");
			AddNewRateLineItem(housebillReleaseTypeLine, "LOI", 0, 20m, "");
			AddNewRateLineItem(housebillReleaseTypeLine, "BRR", 0, 32m, "");
			AddNewRateLineItem(housebillReleaseTypeLine, "CSH", 0, 14m, "");

			#endregion
		}

		void SetupFreightCharges()
		{
			LCLFreightEntry = TestQuote.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX", "STD", "");
			LCLFreightEntry.TI_TransitTime = "14";
			LCLFreightEntry.TI_Frequency = 2;
			LCLFreightEntry.TI_FrequencyUnit = RatingConstants.FrequencyUnits.Week;
			LCLFreightEntry.TI_OH_TransportProvider = TransportProvider1.PK;
			LCLFreightEntry.RateLines.RemoveAndDeleteAll();

			#region Freight

			RateLine freightLine = AddNewRateLine(LCLFreightEntry, Freight, Constants.CurrencyCodes.Australia, RatingConstants.Units.M3);
			freightLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			freightLine.TL_RX_NKCurrency = Constants.CurrencyCodes.Australia;
			((CompanyTariffOrCostBasedCalculator)freightLine.Calculator).Percent = 50m;
			((CompanyTariffOrCostBasedCalculator)freightLine.Calculator).PerUnitPercent = 50m;

			#endregion

			#region Fuel Surcharge

			RateLine fuelSurchargeLine = AddNewRateLine(LCLFreightEntry, FuelSurchage, Constants.CurrencyCodes.Australia, RatingConstants.Units.M3);
			FuelSurchargePK = fuelSurchargeLine.PK;
			RateLineItem fuelSurchargeLineItem1a = AddNewRateLineItem(fuelSurchargeLine, CalculatorConstants.Type.ApplyTo, 0, 0M, CalculatorConstants.Text.ChargeCode);
			fuelSurchargeLineItem1a.TM_AC = Freight.PK;
			((PercentageCalculator)fuelSurchargeLine.Calculator).Percent = 13.26m;

			RateLine fuelSurchargeLine2 = AddNewRateLine(LCLFreightEntry, FuelSurchage2, Constants.CurrencyCodes.Australia, RatingConstants.Units.M3);
			FuelSurchargePK2 = fuelSurchargeLine2.PK;
			RateLineItem fuelSurchargeLineItem2a = AddNewRateLineItem(fuelSurchargeLine2, CalculatorConstants.Type.ApplyTo, 0, 0M, CalculatorConstants.Text.ChargeCode);
			fuelSurchargeLineItem2a.TM_AC = BAF.PK;
			((PercentageCalculator)fuelSurchargeLine2.Calculator).Percent = 10m;

			RateLine fuelSurchargeLine3 = AddNewRateLine(LCLFreightEntry, FuelSurchage3, Constants.CurrencyCodes.Australia, RatingConstants.Units.M3);
			FuelSurchargePK3 = fuelSurchargeLine3.PK;
			AddNewRateLineItem(fuelSurchargeLine3, CalculatorConstants.Type.ApplyTo, 0, 0M, CalculatorConstants.Text.AllCharges);
			((PercentageCalculator)fuelSurchargeLine3.Calculator).Percent = 2m;

			#endregion

			#region BAF

			RateLine bAFLine = AddNewRateLine(LCLFreightEntry, BAF, Constants.CurrencyCodes.Australia, RatingConstants.Units.M3);
			BAFPK = bAFLine.PK;

			#endregion

			#region BAF (Not Shown if Zero Amount)

			RateLine bAFNotShownIfZeroLine = AddNewRateLine(LCLFreightEntry, BAFNotShownIfZero, Constants.CurrencyCodes.Australia, RatingConstants.Units.M3);
			BAFNotShownIfZeroPK = bAFNotShownIfZeroLine.PK;
			bAFNotShownIfZeroLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)0m;

			#endregion
		}

		void SetupDestinationCharges()
		{
			RateEntry lCLDestinationEntry = TestQuote.AddRateEntry("DST", "LCL", "", "USLAX");
			RateEntry lCLDestinationEntryALL = TestQuote.AddRateEntry("DST", "ALL", "", "USLAX");

			#region Single Bond Entry Fee (Part Thereof)

			RateLine singleBondEntryFeePartLine = AddNewRateLine(lCLDestinationEntry, SingleEntryBond, Constants.CurrencyCodes.UnitedStates, RatingConstants.Units.M3);
			SingleEntryBondFeePartPK = singleBondEntryFeePartLine.PK;
			singleBondEntryFeePartLine.GetCalculator<PercentageCalculator>().IsPartThereof = true;
			singleBondEntryFeePartLine.GetCalculator<PercentageCalculator>().Rate = 12.5m;
			singleBondEntryFeePartLine.GetCalculator<PercentageCalculator>().ValueOrPartThereOf = 1000m;
			singleBondEntryFeePartLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ApplyTo.Value.InvoiceValue);

			#endregion

			#region Single Bond Entry Fee (Percentage)

			RateLine singleBondEntryFeePercentLine = AddNewRateLine(lCLDestinationEntry, SingleEntryBond2, Constants.CurrencyCodes.UnitedStates, RatingConstants.Units.M3);
			SingleEntryBondFeePercentPK = singleBondEntryFeePercentLine.PK;
			singleBondEntryFeePercentLine.GetCalculator<PercentageCalculator>().Percent = 5.5m;
			singleBondEntryFeePercentLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ApplyTo.Value.InvoiceValue);

			#endregion

			#region Delivery Order Fee

			RateLine deliveryOrderFeeLine = AddNewRateLine(lCLDestinationEntry, DeliveryOrderFee, Constants.CurrencyCodes.UnitedStates, RatingConstants.Units.HB);
			DeliveryOrdeFeePK = deliveryOrderFeeLine.PK;
			deliveryOrderFeeLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)40m;

			#endregion

			#region Customs Entry

			RateLine customsEntryLine = AddNewRateLine(lCLDestinationEntry, CustomsEntry, Constants.CurrencyCodes.UnitedStates, "");
			CustomsEntryPK = customsEntryLine.PK;
			customsEntryLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)120m;

			#endregion

			#region Port Charges

			RateLine portChargesLine = AddNewRateLine(lCLDestinationEntry, PortCharges, Constants.CurrencyCodes.UnitedStates, RatingConstants.Units.M3);
			PortChargesPK = portChargesLine.PK;
			portChargesLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)55m;

			#endregion

			#region Terminal Handling Charge

			RateLine terminalHandlingChargeLine = AddNewRateLine(lCLDestinationEntry, TerminalHandlingCharge, Constants.CurrencyCodes.UnitedStates, RatingConstants.Units.M3);
			TerminalHandlingChargePK = terminalHandlingChargeLine.PK;
			terminalHandlingChargeLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			#endregion

			#region Professional Indemnity Fee

			RateLine professionalIndemnityFeeLine = AddNewRateLine(lCLDestinationEntry, ProfessionalIndemnityFee, Constants.CurrencyCodes.UnitedStates, "");
			ProfessionalIndemnityFeePK = professionalIndemnityFeeLine.PK;

			#endregion

			#region Cargo Automation

			RateLine cargoAutomationLine = AddNewRateLine(lCLDestinationEntry, CargoAutomationFee, Constants.CurrencyCodes.UnitedStates, "");
			CargoAutomationPK = cargoAutomationLine.PK;
			cargoAutomationLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)5m;

			#endregion

			#region Vehicle Booking Fee

			RateLine vehicleBookingFeeLine = AddNewRateLine(lCLDestinationEntry, VehicleBookingFee, Constants.CurrencyCodes.UnitedStates, RatingConstants.Units.M3);
			VehicleBookingFeePK = vehicleBookingFeeLine.PK;

			#endregion

			#region Postage

			RateLine postageLine = AddNewRateLine(lCLDestinationEntry, Postage, Constants.CurrencyCodes.UnitedStates, RatingConstants.Units.M3);
			PostagePK = postageLine.PK;

			#endregion

			#region Agency Charges

			#region Per Shipment, Flat Fee

			RateLine agencyLine1 = AddNewRateLine(lCLDestinationEntry, Agency1, Constants.CurrencyCodes.UnitedStates, "");
			Agency1PK = agencyLine1.PK;
			((AgencyCalculator)agencyLine1.Calculator).AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
			((AgencyCalculator)agencyLine1.Calculator).AgencyLineType = RateLineTypeList.Codes.FLAT;
			((AgencyCalculator)agencyLine1.Calculator).AgencyRate = 50m;

			#endregion

			#region Per Entry, Flat Fee

			RateLine agencyLine2 = AddNewRateLine(lCLDestinationEntry, Agency2, Constants.CurrencyCodes.UnitedStates, "");
			Agency2PK = agencyLine2.PK;
			((AgencyCalculator)agencyLine2.Calculator).AgencyFeeType = RateFeeTypeList.Codes.PerEntry;
			((AgencyCalculator)agencyLine2.Calculator).AgencyLineType = RateLineTypeList.Codes.FLAT;
			((AgencyCalculator)agencyLine2.Calculator).AgencyRate = 20m;
			((AgencyCalculator)agencyLine2.Calculator).AdditionalRate = 10m;

			#endregion

			#region Per Shipment, Per Invoice

			RateLine agencyLine3 = AddNewRateLine(lCLDestinationEntry, Agency3, Constants.CurrencyCodes.UnitedStates, "");
			Agency3PK = agencyLine3.PK;
			((AgencyCalculator)agencyLine3.Calculator).AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
			((AgencyCalculator)agencyLine3.Calculator).AgencyLineType = RateLineTypeList.Codes.PerInvoiceLinePerShipment;
			((AgencyCalculator)agencyLine3.Calculator).AgencyRate = 15m;
			((AgencyCalculator)agencyLine3.Calculator).AdditionalRate = 7.5m;
			((AgencyCalculator)agencyLine3.Calculator).IncludedLines = 3;
			((AgencyCalculator)agencyLine3.Calculator).PerAdditionalLine = 4.5m;
			((AgencyCalculator)agencyLine3.Calculator).MaximumLines = 25;

			#endregion

			#region Per Entry, Per Tariff

			RateLine agencyLine4 = AddNewRateLine(lCLDestinationEntry, Agency4, Constants.CurrencyCodes.UnitedStates, "");
			Agency4PK = agencyLine4.PK;
			((AgencyCalculator)agencyLine4.Calculator).AgencyFeeType = RateFeeTypeList.Codes.PerEntry;
			((AgencyCalculator)agencyLine4.Calculator).AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerEntry;
			((AgencyCalculator)agencyLine4.Calculator).AgencyRate = 19m;
			((AgencyCalculator)agencyLine4.Calculator).AdditionalRate = 8.5m;
			((AgencyCalculator)agencyLine4.Calculator).IncludedLines = 5;
			((AgencyCalculator)agencyLine4.Calculator).PerAdditionalLine = 3.5m;
			((AgencyCalculator)agencyLine4.Calculator).MaximumLines = 40;

			#endregion

			#region Per Entry, Per Tariff - Hide on Quote

			RateLine agencyLine5 = AddNewRateLine(lCLDestinationEntry, Agency5, Constants.CurrencyCodes.UnitedStates, "");
			Agency5PK = agencyLine5.PK;
			((AgencyCalculator)agencyLine5.Calculator).AgencyFeeType = RateFeeTypeList.Codes.PerEntry;
			((AgencyCalculator)agencyLine5.Calculator).AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerEntry;
			((AgencyCalculator)agencyLine5.Calculator).AgencyRate = 19m;
			((AgencyCalculator)agencyLine5.Calculator).AdditionalRate = 8.5m;
			((AgencyCalculator)agencyLine5.Calculator).IncludedLines = 5;
			((AgencyCalculator)agencyLine5.Calculator).PerAdditionalLine = 3.5m;
			((AgencyCalculator)agencyLine5.Calculator).MaximumLines = 40;
			((AgencyCalculator)agencyLine5.Calculator).HideFeeLineTypeOnQuote = true;

			#endregion

			#endregion

			#region Flat Plus Per Unit With Flat

			RateLine fPUWithFlatLine = AddNewRateLine(lCLDestinationEntry, FPUWithFlatFee, Constants.CurrencyCodes.UnitedStates, RatingConstants.Units.M3);
			FlatPlusPerUnitWithFlatPK = fPUWithFlatLine.PK;
			fPUWithFlatLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)65m;
			fPUWithFlatLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2.5m;

			#endregion

			#region Cartage Ex Los Angeles

			RateLine cartLosAngelesLine = AddNewRateLine(lCLDestinationEntry, CartageChargeCode, Constants.CurrencyCodes.UnitedStates, RatingConstants.Units.M3);
			CartLosAngelesPK = cartLosAngelesLine.PK;
			((CartageCalculator)cartLosAngelesLine.Calculator).EquipmentType = "TRL";
			((CartageCalculator)cartLosAngelesLine.Calculator).Minimum = 153m;
			((CartageCalculator)cartLosAngelesLine.Calculator).PerUnit = 21.5m;

			#endregion

			#region Global Override

			RateLine globalOverrideLine = AddNewRateLine(lCLDestinationEntryALL, GlobalOverride, Constants.CurrencyCodes.UnitedStates, RatingConstants.Units.M3);
			OverrideGlobalPK = globalOverrideLine.PK;
			globalOverrideLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)45m;

			#endregion

			#region Note Calculator

			RateLine noteCalculatorLine = AddNewRateLine(lCLDestinationEntryALL, NoteCode, Constants.CurrencyCodes.UnitedStates, RatingConstants.Units.M3);
			NoteCalculatorPK = noteCalculatorLine.PK;
			AddNewRateLineItem(noteCalculatorLine, "", 0, 1.01M, "Test1");
			AddNewRateLineItem(noteCalculatorLine, "", 0, 2.02M, "Test2");

			#endregion

			#region Dont Print On Quote

			RateLine dontPrintOnQuoteLine = AddNewRateLine(lCLDestinationEntry, DontPrintChargeCode, Constants.CurrencyCodes.UnitedStates, "");
			dontPrintOnQuoteLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)100m;

			#endregion
		}

		void SetupGlobalCharges()
		{
			BusinessObjectFactory globalFactory = new BusinessObjectFactory();
			var globalTariff = globalFactory.New<CompanyTariff>();
			globalTariff.TH_GlobalRateDescription = "Test Global Tariff";

			#region Origin Charges

			RateEntry originEntry1 = globalTariff.AddRateEntry("ORG", "ALL", "AUSYD", "");
			RateLine originLine1 = AddNewRateLine(originEntry1, GlobalOrigin1, Constants.CurrencyCodes.Australia, "");
			GlobalOrigin1PK = originLine1.PK;
			originLine1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)10m;

			RateEntry originEntry2 = globalTariff.AddRateEntry("ORG", "LCL", "AUSYD", "");
			RateLine originLine2 = AddNewRateLine(originEntry2, GlobalOrigin2, Constants.CurrencyCodes.Australia, "M3");
			originLine2.ConversionFactor = ConversionFactor.Standard.Metric.Sea;

			GlobalOrigin2PK = originLine2.PK;
			originLine2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)14m;
			originLine2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)0.45m;

			RateEntry originEntry3 = globalTariff.AddRateEntry("ORG", "AIR", "AUSYD", "");
			RateLine originLine3 = AddNewRateLine(originEntry3, GlobalOrigin3, Constants.CurrencyCodes.Australia, "");

			#endregion

			#region Destination Charges

			RateEntry destinationEntry1 = globalTariff.AddRateEntry("DST", "ALL", "", "USLAX");

			RateLine destinationLine1 = AddNewRateLine(destinationEntry1, GlobalDestination1, Constants.CurrencyCodes.UnitedStates, "");
			GlobalDestination1PK = destinationLine1.PK;
			destinationLine1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)78m;

			RateLine globalOverrideLine = AddNewRateLine(destinationEntry1, GlobalOverride, Constants.CurrencyCodes.UnitedStates, "");
			globalOverrideLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)65m;

			RateEntry destinationEntry3 = globalTariff.AddRateEntry("DST", "AIR", "", "USLAX");
			RateLine destinationLine3 = AddNewRateLine(destinationEntry3, GlobalDestination3, Constants.CurrencyCodes.UnitedStates, "");

			RateEntry destinationEntry2 = globalTariff.AddRateEntry("DST", "LCL", "", "USLAX");
			RateLine destinationLine2 = AddNewRateLine(destinationEntry2, GlobalDestination2, Constants.CurrencyCodes.UnitedStates, "M3");
			destinationLine2.ConversionFactor = ConversionFactor.Standard.Metric.Sea;

			GlobalDestination2PK = destinationLine2.PK;
			destinationLine2.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)50m;
			destinationLine2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1.25m;

			#endregion

			globalFactory.Save();
		}

		void SetupCostingRates()
		{
			var costs = Factory.New<Costing>();
			costs.TH_OH = TransportProvider1.PK;

			#region LCL Freight

			RateEntry costsEntry = costs.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX", "STD", "");

			RateLine freightLine = AddNewRateLine(costsEntry, Freight, Constants.CurrencyCodes.Australia, RatingConstants.Units.M3);
			freightLine.TL_RateCalculator = CombinedCalculator.Code;
			freightLine.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)100m;
			freightLine.Calculator["-5"] = (ZDecimal)60m;
			freightLine.Calculator["+5"] = (ZDecimal)56m;
			freightLine.Calculator["+8"] = (ZDecimal)50m;

			#endregion

			Factory.Save();
		}

		void SetupFCLQuoteEntries()
		{
			FCLFreightEntry1 = TestQuote.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "20GP");
			FCLFreightEntry1.TI_TransitTime = "14";
			FCLFreightEntry1.TI_Frequency = 2;
			FCLFreightEntry1.TI_FrequencyUnit = RatingConstants.FrequencyUnits.Week;
			FCLFreightEntry1.RateLines.RemoveAndDeleteAll();

			FCLFreightEntry2 = TestQuote.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "40GP");
			FCLFreightEntry2.TI_TransitTime = "14";
			FCLFreightEntry2.TI_Frequency = 2;
			FCLFreightEntry2.TI_FrequencyUnit = RatingConstants.FrequencyUnits.Week;
			FCLFreightEntry2.RateLines.RemoveAndDeleteAll();

			FCLFreightEntry3 = TestQuote.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "D2D", "40HC");
			FCLFreightEntry3.TI_TransitTime = "14";
			FCLFreightEntry3.TI_Frequency = 2;
			FCLFreightEntry3.TI_FrequencyUnit = RatingConstants.FrequencyUnits.Week;
			FCLFreightEntry3.RateLines.RemoveAndDeleteAll();

			RateLine fCLFreightLine1 = AddNewRateLine(FCLFreightEntry1, Freight, Constants.CurrencyCodes.Australia, RatingConstants.Units.CN);
			FCLFreightPK1 = fCLFreightLine1.PK;
			fCLFreightLine1.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			fCLFreightLine1.TL_RX_NKCurrency = Constants.CurrencyCodes.Australia;
			fCLFreightLine1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)500M;

			RateLine fCLFreightLine2 = AddNewRateLine(FCLFreightEntry2, Freight, Constants.CurrencyCodes.Australia, RatingConstants.Units.CN);
			FCLFreightPK2 = fCLFreightLine2.PK;
			fCLFreightLine2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)4000M;

			RateLine fCLFreightLine3 = AddNewRateLine(FCLFreightEntry3, Freight, Constants.CurrencyCodes.Australia, RatingConstants.Units.CN);
			fCLFreightLine3.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)4500M;
		}

		void SetupFCLOriginCharges()
		{
			#region ALL

			RateEntry aLLOrigin = TestQuote.AddRateEntry("ORG", "ALL", "AUSYD", "");

			RateLine customsEntryLine = AddNewRateLine(aLLOrigin, CustomsEntry, Constants.CurrencyCodes.Australia, RatingConstants.Units.CN);
			CustomsEntryPK = customsEntryLine.PK;
			customsEntryLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)50M;

			RateLine documentationALLLine = AddNewRateLine(aLLOrigin, Documentation, Constants.CurrencyCodes.Australia, RatingConstants.Units.CN);
			documentationALLLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)90M;

			#endregion

			#region FCL

			RateEntry fCLOrigin = TestQuote.AddRateEntry("ORG", "FCL", "AUSYD", "");

			RateLine documentationLine = AddNewRateLine(fCLOrigin, Documentation, Constants.CurrencyCodes.Australia, RatingConstants.Units.CN);
			DocumentationPK = documentationLine.PK;
			documentationLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)30M;

			#endregion

			#region 20GP

			RateEntry fCLOrigin20GP = TestQuote.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "20GP");

			RateLine liftOnLifeOff20GPLine = AddNewRateLine(fCLOrigin20GP, LiftOnLiftOff, Constants.CurrencyCodes.Australia, RatingConstants.Units.CN);
			LiftOnLiftOff20GPPK = liftOnLifeOff20GPLine.PK;
			liftOnLifeOff20GPLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)125M;

			#endregion

			#region 40GP

			RateEntry fCLOrigin40GP = TestQuote.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "40GP");

			RateLine liftOnLifeOff40GPLine = AddNewRateLine(fCLOrigin40GP, LiftOnLiftOff, Constants.CurrencyCodes.Australia, RatingConstants.Units.CN);
			LiftOnLiftOff40GPPK = liftOnLifeOff40GPLine.PK;
			liftOnLifeOff40GPLine.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)500M;
			liftOnLifeOff40GPLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)250M;

			#endregion

			#region 40HC

			RateEntry fCLOrigin40HC = TestQuote.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "40HC");

			RateLine liftOnLifeOff40HCLine = AddNewRateLine(fCLOrigin40HC, LiftOnLiftOff, Constants.CurrencyCodes.Australia, RatingConstants.Units.CN);
			liftOnLifeOff40HCLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)325M;

			#endregion
		}

		void SetupFCLGlobalCharges()
		{
			BusinessObjectFactory globalFactory = new BusinessObjectFactory();
			var globalTariff = globalFactory.New<CompanyTariff>();

			RateEntry globalFCLEntry1 = globalTariff.AddRateEntry("DST", "FCL", "", "USLAX", "", "20GP");
			RateLine globalFCLLine1 = AddNewRateLine(globalFCLEntry1, GlobalFCL, Constants.CurrencyCodes.UnitedStates, "CN");
			GlobalFCLLine1PK = globalFCLLine1.PK;
			globalFCLLine1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)60M;

			RateEntry globalFCLEntry2 = globalTariff.AddRateEntry("DST", "FCL", "", "USLAX", "", "40GP");
			RateLine globalFCLLine2 = AddNewRateLine(globalFCLEntry2, GlobalFCL, Constants.CurrencyCodes.UnitedStates, "CN");
			GlobalFCLLine2PK = globalFCLLine2.PK;
			globalFCLLine2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)120M;

			RateEntry globalFCLEntry3 = globalTariff.AddRateEntry("DST", "FCL", "", "USLAX", "", "40HC");
			RateLine globalFCLLine3 = AddNewRateLine(globalFCLEntry3, GlobalFCL, Constants.CurrencyCodes.UnitedStates, "CN");
			globalFCLLine3.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)135M;

			#region FCL Freight

			RateEntry fCLFreightEntry = globalTariff.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "20GP");
			fCLFreightEntry.RateLines.RemoveAndDeleteAll();

			RateLine fCLFreightLine = AddNewRateLine(fCLFreightEntry, Freight, Constants.CurrencyCodes.Australia, RatingConstants.Units.CN);
			fCLFreightLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2000M;

			#endregion

			globalFactory.Save();
		}

		RateLine AddNewRateLine(RateEntry entry, AccChargeCode chargeCode, ZString currency, ZString weightVolume)
		{
			var newRateLine = entry.RateLines.AddNew();
			newRateLine.TL_AC = chargeCode.PK;
			newRateLine.TL_RX_NKCurrency = currency;
			newRateLine.TL_WeightVolume = weightVolume;

			return newRateLine;
		}

		RateLineItem AddNewRateLineItem(RateLine line, ZString type, ZDecimal @break, ZDecimal amount, ZString text)
		{
			var newRateLineItem = line.RateLineItems.AddNew();
			newRateLineItem.TM_Type = type;
			newRateLineItem.TM_Break = @break;
			newRateLineItem.TM_Value = amount;
			newRateLineItem.TM_Text = text;

			return newRateLineItem;
		}

		#region Fields

		bool originalAlternateRateFormatValue;

		Quote TestQuote;
		RateEntry LCLFreightEntry;
		AccChargeCode Documentation;
		AccChargeCode CartageChargeCode;
		AccChargeCode ExportCustoms;
		AccChargeCode SecurityTax;
		AccChargeCode Fumigation;
		AccChargeCode HousebillReleaseType;
		AccChargeCode ExportSecurity;
		AccChargeCode DummyChargeCode;
		AccChargeCode ItalianAirportTax;
		AccChargeCode LiftOnLiftOff;

		AccChargeCode Freight;
		AccChargeCode FuelSurchage;
		AccChargeCode FuelSurchage2;
		AccChargeCode FuelSurchage3;
		AccChargeCode BAF;
		AccChargeCode BAFNotShownIfZero;

		AccChargeCode DeliveryOrderFee;
		AccChargeCode CustomsEntry;
		AccChargeCode PortCharges;
		AccChargeCode TerminalHandlingCharge;
		AccChargeCode ProfessionalIndemnityFee;
		AccChargeCode CargoAutomationFee;
		AccChargeCode FPUWithNoFlatFee;
		AccChargeCode FPUWithFlatFee;

		AccChargeCode VehicleBookingFee;
		AccChargeCode Postage;
		AccChargeCode Agency1;
		AccChargeCode Agency2;
		AccChargeCode Agency3;
		AccChargeCode Agency4;
		AccChargeCode Agency5;
		AccChargeCode SingleEntryBond;
		AccChargeCode SingleEntryBond2;

		AccChargeCode GlobalOrigin1;
		AccChargeCode GlobalOrigin2;
		AccChargeCode GlobalOrigin3;

		AccChargeCode GlobalDestination1;
		AccChargeCode GlobalDestination2;
		AccChargeCode GlobalDestination3;
		AccChargeCode GlobalFCL;
		AccChargeCode GlobalOverride;
		AccChargeCode NoteCode;

		AccChargeCode DontPrintChargeCode;
		OrgHeader AlexandriaSupplier;
		OrgHeader HornsbySupplier;

		RateEntry FCLFreightEntry1;
		RateEntry FCLFreightEntry2;
		RateEntry FCLFreightEntry3;

		ZGuid DocumentationPK;
		ZGuid CartAlexandriaPK;
		ZGuid CartHornsbyPK;
		ZGuid ExportCustomsPK;
		ZGuid SecurityTaxPK;
		ZGuid FumigationPK;
		ZGuid HousebillReleaseTypePK;
		ZGuid ExportSecurityPK;
		ZGuid FlatPlusPerUnitWithNoFlatPK;
		ZGuid ItalianAirportTaxPK;

		ZGuid FuelSurchargePK;
		ZGuid FuelSurchargePK2;
		ZGuid FuelSurchargePK3;
		ZGuid BAFPK;
		ZGuid BAFNotShownIfZeroPK;

		ZGuid DeliveryOrdeFeePK;
		ZGuid CustomsEntryPK;
		ZGuid PortChargesPK;
		ZGuid TerminalHandlingChargePK;
		ZGuid ProfessionalIndemnityFeePK;
		ZGuid CargoAutomationPK;
		ZGuid VehicleBookingFeePK;
		ZGuid PostagePK;
		ZGuid Agency1PK;
		ZGuid Agency2PK;
		ZGuid Agency3PK;
		ZGuid Agency4PK;
		ZGuid Agency5PK;
		ZGuid CartLosAngelesPK;
		ZGuid OverrideGlobalPK;
		ZGuid FlatPlusPerUnitWithFlatPK;
		ZGuid SingleEntryBondFeePartPK;
		ZGuid SingleEntryBondFeePercentPK;
		ZGuid NoteCalculatorPK;

		ZGuid GlobalOrigin1PK;
		ZGuid GlobalOrigin2PK;
		ZGuid GlobalDestination1PK;
		ZGuid GlobalDestination2PK;

		ZGuid FCLFreightPK1;
		ZGuid FCLFreightPK2;

		ZGuid LiftOnLiftOff20GPPK;
		ZGuid LiftOnLiftOff40GPPK;

		ZGuid GlobalFCLLine1PK;
		ZGuid GlobalFCLLine2PK;

		#endregion

		#endregion
	}
}
