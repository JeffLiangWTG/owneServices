using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	sealed class DocTableQuotationTest2 : RatingTestCase
	{
		public void TestCrossTrade()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			Quote quote = Helper.NewQuote(Helper.NewOrgHeader());

			RateEntry entry1 = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			RateLine line1 = entry1.RateLines[0];
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			RateEntry entry2 = quote.AddRateEntry("AIR", "LSE", "USLAX", "GBLON");
			RateLine line2 = entry2.RateLines[0];
			line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)4m;

			RateEntry entry3 = quote.AddRateEntry("ORG", "AIR", "USLAX", "");
			RateLine line3 = entry3.AddRateLine("ODOC", FlatCalculator.Code);
			line3.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)35m;

			RateEntry entry4 = quote.AddRateEntry("DST", "AIR", "USLAX", "GBLON");
			RateLine line4 = entry4.AddRateLine("DDOC", FlatCalculator.Code);
			line4.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)15m;

			var pages = new PricingPageCollection(quote);
			pages.Load(PricingPaginationStrategy.LandscapeComplexStyle);
			AssertEquals(2, pages.Count);

			DocTableQuotation quotation = DocTableQuotation.New(pages[0], Factory);
			AssertEquals("Page Headng", "Export Air Freight Rates from Sydney", quotation.PageHeading);
			AssertEquals(0, quotation.OriginDocRateLineItems.Count);
			AssertEquals(0, quotation.OriginDocRateLineItems.Count);

			quotation = DocTableQuotation.New(pages[1], Factory);
			AssertEquals("Page Headng", "Cross Trade Air Freight Rates", quotation.PageHeading);
			Factory.Save();
			AssertEquals(1, quotation.OriginDocRateLineItems.Count);
			AssertEquals("Origin Charges - Los Angeles", quotation.OriginDocRateLineItems[0].LocalChargesHeading);
			AssertEquals("Origin Documentation Fee *|USD|35.00|", quotation.OriginDocRateLineItems[0].QuotationLine.ToString());
			AssertEquals(1, quotation.DestinationDocRateLineItems.Count);
			AssertEquals("Destination Charges - London from Los Angeles", quotation.DestinationDocRateLineItems[0].OverseasChargesHeading);
			AssertEquals("Destination Documentation Fee *|GBP|15.00|", quotation.DestinationDocRateLineItems[0].QuotationLine.ToString());
		}

		public void TestAirOriginOnly()
		{
			var tariff = Factory.New<CompanyTariff>();

			RateEntry entry1 = tariff.AddRateEntry("ORG", "AIR", "AUSYD", "");
			RateLine line1 = entry1.AddRateLine("ODOC", FlatCalculator.Code);
			line1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)50m;

			RateEntry entry2 = tariff.AddRateEntry("ORG", "AIR", "AUMEL", "");
			RateLine line2 = entry2.AddRateLine("ODOC", FlatCalculator.Code);
			line2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)45m;

			Factory.Save();

			var pages = new PricingPageCollection(tariff);
			pages.Load(PricingPaginationStrategy.LandscapeSimpleStyle);
			AssertEquals(1, pages.Count);

			DocTableQuotation testQuotation = DocTableQuotation.New(pages[0], Factory);

			AssertEquals("Air Freight Rates", testQuotation.PageHeading);
			AssertEquals(2, testQuotation.OriginDocRateLineItems.Count);
			AssertEquals("Origin Charges - Sydney", testQuotation.OriginDocRateLineItems.Find(line1)[0].OriginChargesHeading);
			AssertEquals("Origin Documentation Fee *|AUD|50.00|", testQuotation.OriginDocRateLineItems.Find(line1)[0].QuotationLine.ToString());
			AssertEquals("Origin Charges - Melbourne", testQuotation.OriginDocRateLineItems.Find(line2)[0].OriginChargesHeading);
			AssertEquals("Origin Documentation Fee *|AUD|45.00|", testQuotation.OriginDocRateLineItems.Find(line2)[0].QuotationLine.ToString());
			AssertEquals(0, testQuotation.DestinationDocRateLineItems.Count);

			ClientRate rate = Helper.NewClientRate(Helper.NewOrgHeader(1));

			RateEntry entry3 = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			RateLine line3 = entry3.AddRateLine("OAWB", FlatCalculator.Code);
			line3.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)25m;

			pages = new PricingPageCollection(rate);
			pages.Load(PricingPaginationStrategy.LandscapeSimpleStyle);

			AssertEquals(1, pages.Count);

			testQuotation = DocTableQuotation.New(pages[0], Factory);
			Factory.Save();
			AssertEquals("Air Freight Rates", testQuotation.PageHeading);
			AssertEquals(2, testQuotation.OriginDocRateLineItems.Count);
			AssertEquals("Origin Charges - Sydney", testQuotation.OriginDocRateLineItems.Find(line1)[0].OriginChargesHeading);
			AssertEquals("Origin Documentation Fee *|AUD|50.00|", testQuotation.OriginDocRateLineItems.Find(line1)[0].QuotationLine.ToString());
			AssertEquals("Origin Charges - Sydney to Los Angeles", testQuotation.OriginDocRateLineItems.Find(line3)[0].OriginChargesHeading);
			AssertEquals("Origin Airway Bill Fee *|AUD|25.00|", testQuotation.OriginDocRateLineItems.Find(line3)[0].QuotationLine.ToString());
			AssertEquals(0, testQuotation.DestinationDocRateLineItems.Count);
		}

		public void TestSeaDestinationOnly()
		{
			var tariff = Factory.New<CompanyTariff>();

			RateEntry entry1 = tariff.AddRateEntry("DST", "FCL", "", "AUSYD");
			RateLine line1 = entry1.AddRateLine("DPCH", UnitCalculator.Code, "CN");
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)50m;

			RateEntry entry2 = tariff.AddRateEntry("DST", "LCL", "", "AUSYD");
			RateLine line2 = entry2.AddRateLine("DPCH", FlatCalculator.Code);
			line2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)45m;

			RateEntry entry3 = tariff.AddRateEntry("DST", "LCL", "", "AUMEL");
			RateLine line3 = entry3.AddRateLine("DPCH", FlatCalculator.Code);
			line3.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)30m;

			RateEntry entry4 = tariff.AddRateEntry("DST", "LCL", "", "AUBNE");
			RateLine line4 = entry4.AddRateLine("DPCH", FlatCalculator.Code);
			line4.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)40m;

			Factory.Save();

			var pages = new PricingPageCollection(tariff);
			pages.Load(PricingPaginationStrategy.LandscapeSimpleStyle);
			AssertEquals(1, pages.Count);

			DocTableQuotation testQuotation = DocTableQuotation.New(pages[0], Factory);

			AssertEquals("Sea Freight Rates", testQuotation.PageHeading);
			AssertEquals(0, testQuotation.OriginDocRateLineItems.Count);
			AssertEquals(4, testQuotation.DestinationDocRateLineItems.Count);
			AssertEquals("FCL", testQuotation.DestinationDocRateLineItems.Find(line1)[0].Mode);
			AssertEquals("Destination Charges - Sydney", testQuotation.DestinationDocRateLineItems.Find(line1)[0].DestinationChargesHeading);
			AssertEquals("Destination Port Charges *|AUD|50.00|per Container", testQuotation.DestinationDocRateLineItems.Find(line1)[0].QuotationLine.ToString());
			AssertEquals("LCL", testQuotation.DestinationDocRateLineItems.Find(line2)[0].Mode);
			AssertEquals("Destination Charges - Sydney", testQuotation.DestinationDocRateLineItems.Find(line2)[0].DestinationChargesHeading);
			AssertEquals("Destination Port Charges *|AUD|45.00|", testQuotation.DestinationDocRateLineItems.Find(line2)[0].QuotationLine.ToString());
			AssertEquals("LCL", testQuotation.DestinationDocRateLineItems.Find(line3)[0].Mode);
			AssertEquals("Destination Charges - Melbourne", testQuotation.DestinationDocRateLineItems.Find(line3)[0].DestinationChargesHeading);
			AssertEquals("Destination Port Charges *|AUD|30.00|", testQuotation.DestinationDocRateLineItems.Find(line3)[0].QuotationLine.ToString());
			AssertEquals("LCL", testQuotation.DestinationDocRateLineItems.Find(line4)[0].Mode);
			AssertEquals("Destination Charges - Brisbane", testQuotation.DestinationDocRateLineItems.Find(line4)[0].DestinationChargesHeading);
			AssertEquals("Destination Port Charges *|AUD|40.00|", testQuotation.DestinationDocRateLineItems.Find(line4)[0].QuotationLine.ToString());

			ClientRate rate = Helper.NewClientRate(Helper.NewOrgHeader(1));

			RateEntry entry5 = rate.AddRateEntry("DST", "LCL", "USLAX", "AUMEL");
			RateLine line5 = entry5.AddRateLine("DDOC", FlatCalculator.Code);
			line5.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)15m;

			RateEntry entry6 = rate.AddRateEntry("DST", "FCL", "", "AUBNE");
			RateLine line6 = entry6.AddRateLine("DPCH", UnitCalculator.Code, "CN");
			line6.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)60m;

			pages = new PricingPageCollection(rate);
			pages.Load(PricingPaginationStrategy.LandscapeSimpleStyle);
			AssertEquals(1, pages.Count);

			testQuotation = DocTableQuotation.New(pages[0], Factory);

			AssertEquals("Sea Freight Rates", testQuotation.PageHeading);
			Factory.Save();
			AssertEquals(0, testQuotation.OriginDocRateLineItems.Count);
			AssertEquals(3, testQuotation.DestinationDocRateLineItems.Count);
			AssertEquals("LCL", testQuotation.DestinationDocRateLineItems.Find(line3)[0].Mode);
			AssertEquals("Destination Charges - Melbourne", testQuotation.DestinationDocRateLineItems.Find(line3)[0].DestinationChargesHeading);
			AssertEquals("Destination Port Charges *|AUD|30.00|", testQuotation.DestinationDocRateLineItems.Find(line3)[0].QuotationLine.ToString());
			AssertEquals("LCL", testQuotation.DestinationDocRateLineItems.Find(line5)[0].Mode);
			AssertEquals("Destination Charges - Melbourne from Los Angeles", testQuotation.DestinationDocRateLineItems.Find(line5)[0].DestinationChargesHeading);
			AssertEquals("Destination Documentation Fee *|AUD|15.00|", testQuotation.DestinationDocRateLineItems.Find(line5)[0].QuotationLine.ToString());
			AssertEquals("FCL", testQuotation.DestinationDocRateLineItems.Find(line6)[0].Mode);
			AssertEquals("Destination Charges - Brisbane", testQuotation.DestinationDocRateLineItems.Find(line6)[0].DestinationChargesHeading);
			AssertEquals("Destination Port Charges *|AUD|60.00|per Container", testQuotation.DestinationDocRateLineItems.Find(line6)[0].QuotationLine.ToString());
		}

		public void TestOriginDestinationWithFreight()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			Quote testQuote = Helper.NewQuote(Helper.NewOrgHeader());

			RateEntry entry1 = testQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			RateLine line1 = entry1.RateLines[0];
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			RateEntry entry2 = testQuote.AddRateEntry("ORG", "AIR", "AUSYD", "");
			RateLine line2 = entry2.AddRateLine("ODOC", FlatCalculator.Code);
			line2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)45m;

			RateEntry entry3 = testQuote.AddRateEntry("ORG", "AIR", "AUMEL", "");
			RateLine line3 = entry3.AddRateLine("ODOC", FlatCalculator.Code);
			line3.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)30m;

			RateEntry entry4 = testQuote.AddRateEntry("DST", "LCL", "", "AUBNE");
			RateLine line4 = entry4.AddRateLine("DPCH", FlatCalculator.Code);
			line4.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)40m;

			RateEntry entry5 = testQuote.AddRateEntry("ORG", "AIR", "AUBNE", "SGSIN");
			RateLine line5 = entry5.AddRateLine("ODOC", FlatCalculator.Code);
			line5.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)42m;

			RateEntry entry6 = testQuote.AddRateEntry("ORG", "FCL", "AUBNE", "");
			RateLine line6 = entry6.AddRateLine("ODOC", FlatCalculator.Code);
			line6.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)160m;

			PricingPageCollection pages = new PricingPageCollection(testQuote);
			pages.Load(PricingPaginationStrategy.LandscapeComplexStyle);

			DocTableQuotation[] wrappers = Array.ConvertAll(pages.ToArray<PricingPage>(), (t) => DocTableQuotation.New(t, Factory));
			Array.Sort(wrappers, (w1, w2) => w1.PageHeading.CompareTo(w2.PageHeading));

			AssertEquals(3, wrappers.Length);

			var quotation = wrappers[0];
			AssertEquals("Page Headng", "Air Freight Rates", quotation.PageHeading);
			AssertEquals(2, quotation.OriginDocRateLineItems.Count);
			AssertEquals("Origin Charges - Melbourne", quotation.OriginDocRateLineItems.Find(line3)[0].LocalChargesHeading);
			AssertEquals("Origin Documentation Fee *|AUD|30.00|", quotation.OriginDocRateLineItems.Find(line3)[0].QuotationLine.ToString());
			AssertEquals("Origin Charges - Brisbane to Singapore", quotation.OriginDocRateLineItems.Find(line5)[0].LocalChargesHeading);
			AssertEquals("Origin Documentation Fee *|AUD|42.00|", quotation.OriginDocRateLineItems.Find(line5)[0].QuotationLine.ToString());
			AssertEquals(0, quotation.DestinationDocRateLineItems.Count);

			quotation = wrappers[1];
			Factory.Save();
			AssertEquals("Page Headng", "Export Air Freight Rates from Sydney", quotation.PageHeading);
			AssertEquals(1, quotation.OriginDocRateLineItems.Count);
			AssertEquals("Local Charges - Sydney", quotation.OriginDocRateLineItems[0].LocalChargesHeading);
			AssertEquals("Origin Documentation Fee *|AUD|45.00|", quotation.OriginDocRateLineItems[0].QuotationLine.ToString());
			AssertEquals(0, quotation.DestinationDocRateLineItems.Count);

			quotation = wrappers[2];
			AssertEquals("Page Headng", "Sea Freight Rates", quotation.PageHeading);
			AssertEquals(1, quotation.OriginDocRateLineItems.Count);
			AssertEquals("Origin Charges - Brisbane", quotation.OriginDocRateLineItems[0].LocalChargesHeading);
			AssertEquals("Origin Documentation Fee *|AUD|160.00|", quotation.OriginDocRateLineItems[0].QuotationLine.ToString());
			AssertEquals(1, quotation.DestinationDocRateLineItems.Count);
			AssertEquals("Destination Charges - Brisbane", quotation.DestinationDocRateLineItems[0].OverseasChargesHeading);
			AssertEquals("Destination Port Charges *|AUD|40.00|", quotation.DestinationDocRateLineItems[0].QuotationLine.ToString());
		}

		public void TestCFSRates()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			ClientRate testCFSRate = Helper.NewClientRate(Helper.NewOrgHeader());

			#region Packing Charges

			RateEntry entryOrigin1 = testCFSRate.AddRateEntry("PAC", "LCL", "AUSYD", "");
			RateLine lineOrigin1 = entryOrigin1.AddRateLine("CFSPACK", UnitCalculator.Code, "M3");
			lineOrigin1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)49m;

			RateEntry entryOrigin2 = testCFSRate.AddRateEntry("PAC", "FCL", "AUSYD", "");
			RateLine lineOrigin2 = entryOrigin2.AddRateLine("CFSINSP", FlatCalculator.Code);
			lineOrigin2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)40m;

			RateEntry entryOrigin3 = testCFSRate.AddRateEntry("PAC", "FCL", "AUSYD", "", "", "20GP");
			RateLine lineOrigin3 = entryOrigin3.AddRateLine("CFSPACK", UnitCalculator.Code, "CN");
			lineOrigin3.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)80m;

			RateEntry entryOrigin4 = testCFSRate.AddRateEntry("PAC", "AIR", "AUSYD", "");
			RateLine lineOrigin4 = entryOrigin4.AddRateLine("CFSPACK", UnitCalculator.Code, "KG");
			lineOrigin4.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)0.5m;

			#endregion

			#region Unpacking Charges

			RateEntry entryDest1 = testCFSRate.AddRateEntry("UNP", "FCL", "", "AUMEL");
			RateLine lineDest1 = entryDest1.AddRateLine("CFSUNPA", UnitCalculator.Code, "CN", "");
			lineDest1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)25m;

			#endregion

			PricingPageCollection pages = new PricingPageCollection(testCFSRate);
			pages.Load(PricingPaginationStrategy.LandscapeSimpleStyle);

			AssertEquals(1, pages.Count);

			DocTableQuotation quotation = DocTableQuotation.New(pages[0], Factory);

			AssertEquals("Main Headng", "Test Client #1 Client Rate", quotation.MainHeading);
			Factory.Save();
			AssertEquals(5, quotation.OriginDocRateLineItems.Count);
			AssertEquals("Packing Charges - Sydney", quotation.OriginDocRateLineItems[0].OriginChargesHeading);
			AssertEquals("CFS Packing Charges *|AUD|49.00|per M3 / 1000 KG", quotation.OriginDocRateLineItems[0].QuotationLine.ToString());

			AssertEquals("Packing Charges - Sydney", quotation.OriginDocRateLineItems[1].OriginChargesHeading);
			AssertEquals("CFS Extra Inspection *|AUD|40.00|", quotation.OriginDocRateLineItems[1].QuotationLine.ToString());

			AssertEquals("Packing Charges - Sydney", quotation.OriginDocRateLineItems[2].OriginChargesHeading);
			AssertEquals("CFS Packing Charges *|||", quotation.OriginDocRateLineItems[2].QuotationLine.ToString());
			AssertEquals("20GP|AUD|80.00|per Container", quotation.OriginDocRateLineItems[3].QuotationLine.ToString());

			AssertEquals("Packing Charges - Sydney", quotation.OriginDocRateLineItems[4].OriginChargesHeading);
			AssertEquals("CFS Packing Charges *|AUD|0.50|per KG / 6000 CC", quotation.OriginDocRateLineItems[4].QuotationLine.ToString());

			AssertEquals(1, quotation.DestinationDocRateLineItems.Count);
			AssertEquals("Unpacking Charges - Melbourne", quotation.DestinationDocRateLineItems[0].DestinationChargesHeading);
			AssertEquals("CFS Unpacking Charges *|AUD|25.00|per Container", quotation.DestinationDocRateLineItems[0].QuotationLine.ToString());
		}

		public void TestImportShippingDetentionRates()
		{
			ClientRate rate = Helper.NewClientRate(Helper.NewOrgHeader());

			RateEntry importEntry = rate.AddRateEntry(RatingConstants.RateCategory.SID, "SEA", "", "AU");
			RateLine importLine = importEntry.AddRateLine("OPCH", UnitCalculator.Code, "D");
			importLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)30m;

			PricingPageCollection pages = new PricingPageCollection(rate);
			pages.Load(PricingPaginationStrategy.LandscapeSimpleStyle);
			AssertEquals(1, pages.Count);

			DocTableQuotation quotation = DocTableQuotation.New(pages[0], Factory);

			CombineAssertions(delegate
			{
				AssertEquals("MainHeading", "Shipping Detention Rates", quotation.MainHeading);
				AssertEquals("DestinationChargesHeading", "Import Detention Charges - Australia", quotation.DestinationDocRateLineItems[0].DestinationChargesHeading);
			});
		}

		public void TestExportShippingDetentionRates()
		{
			ClientRate testRate = Helper.NewClientRate(Helper.NewOrgHeader());

			RateEntry exportEntry = testRate.AddRateEntry(RatingConstants.RateCategory.SED, "SEA", "AU", "");
			RateLine exportLine = exportEntry.AddRateLine("DPCH", UnitCalculator.Code, "D");
			exportLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)25m;

			PricingPageCollection pages = new PricingPageCollection(testRate);
			pages.Load(PricingPaginationStrategy.LandscapeSimpleStyle);
			AssertEquals(1, pages.Count);

			DocTableQuotation quotation = DocTableQuotation.New(pages[0], Factory);

			CombineAssertions(delegate
			{
				AssertEquals("MainHeading", "Shipping Detention Rates", quotation.MainHeading);
				AssertEquals("OriginChargesHeading", "Export Detention Charges - Australia", quotation.OriginDocRateLineItems[0].OriginChargesHeading);
			});
		}

		public void TestAllModeSupplementalCharges()
		{
			Quote testQuote = Helper.NewQuote(Helper.NewOrgHeader());

			RateEntry entry1 = testQuote.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			entry1.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)100m;

			RateEntry entry2 = testQuote.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			entry2.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)100m;

			RateEntry entry3 = testQuote.AddRateEntry("ORG", "ALL", "AUSYD", "USLAX");
			RateLine line3 = entry3.AddRateLine("ODOC", FirstPlusAdditionalCalculator.Code, "HB");
			line3.Calculator[FirstPlusAdditionalCalculator.Items.FST] = (ZDecimal)45m;
			line3.Calculator[FirstPlusAdditionalCalculator.Items.ADD] = (ZDecimal)15m;

			Factory.Save();

			PricingPageCollection pages = new PricingPageCollection(testQuote);
			pages.Load(PricingPaginationStrategy.LandscapeComplexStyle);

			AssertEquals(1, pages.Count);

			DocTableQuotation quotation = DocTableQuotation.New(pages[0], Factory);

			AssertEquals(3, quotation.LocalDocRateLineItems.Count);
			AssertEquals("Origin Documentation Fee *|||", quotation.LocalDocRateLineItems[0].QuotationLine.ToString());
			AssertEquals("First House Bill|AUD|45.00|", quotation.LocalDocRateLineItems[1].QuotationLine.ToString());
			AssertEquals("Thereafter|AUD|15.00|per House Bill", quotation.LocalDocRateLineItems[2].QuotationLine.ToString());
		}

		public void TestWithDifferentGlbCompany()
		{
			var company1 = new BusinessObjectFactory().NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "NEW";
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			var branch1 = company1.Factory.NewWithValidTestData<GlbBranch>();
			company1.Branches.Add(branch1);
			company1.Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var defaultTaxRate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainGSTTaxRegistryID, Env.CurrentCompanyPK);
				defaultTaxRate.SetRateNumerator_ForTestOnly(10);
				defaultTaxRate.Factory.Save();

				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "DEFRA";
				CompanyTariff tariff1 = Helper.NewCompanyTariff();
				tariff1.AddRateEntry("ORG", "AIR", "DEFRA", "").AddRateLine("ODOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)15m;

				ClientRate rate = Helper.NewClientRate(Helper.NewOrgHeader());
				rate.AddRateEntry("DST", "AIR", "", "USLAX").AddRateLine("DDOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)10m;

				Quote quote = Helper.NewQuote(rate.Header);
				RateLine freightLine = quote.AddRateEntry("AIR", "LSE", "DEFRA", "USLAX").RateLines[0];
				freightLine.TL_RateCalculator = UnitCalculator.Code;
				freightLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)3m;

				Factory.Save();

				PricingPageCollection pages = new PricingPageCollection(quote);
				pages.Load(PricingPaginationStrategy.LandscapeComplexStyle);
				AssertEquals(1, pages.Count);

				DocTableQuotation testQuotation = DocTableQuotation.New(pages[0], Factory);
				AssertEquals("Export Air Freight Rates from Frankfurt am Main", testQuotation.PageHeading);
				AssertEquals("A local Value Added Tax charge (equivalent to MST) may apply to all items marked with an asterisk (*).", testQuotation.PageClosingText);

				AccTaxRate taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
				taxRate.SetRateNumerator_ForTestOnly(15);

				GlbCompany company = Factory.Load<GlbCompany>(quote.TH_GC);
				taxRate.AT_RN_NKCountry = company.GC_RN_NKCountryCode;
				taxRate.AT_Description = "Test VAT";
				taxRate.AT_IsActive = true;
				freightLine.ChargeCode.AC_AT_GSTRate = taxRate.PK;

				AssertEquals("A local Value Added Tax charge (equivalent to MST) may apply to freight and all items marked with an asterisk (*).", testQuotation.PageClosingText);
			}
		}

		public void TestWithEmptyFCLContainer()
		{
			ClientRate testRate = Helper.NewClientRate(Helper.NewOrgHeader());

			RateEntry entry1 = testRate.AddRateEntry("FCL", "SEA", "USLAX", "AUSYD", "", "20GP");
			RateLine line1 = entry1.RateLines[0];
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2000m;

			RateEntry entry2 = testRate.AddRateEntry("DST", "FCL", "", "AUMEL");
			RateLine line2 = entry2.AddRateLine("DDOC", FlatCalculator.Code);
			line2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)30m;

			Factory.Save();

			PricingPageCollection pages = new PricingPageCollection(testRate);
			pages.Load(PricingPaginationStrategy.LandscapeSimpleStyle);

			AssertEquals(1, pages.Count);

			DocTableQuotation quotation = DocTableQuotation.New(pages[0], Factory);
			AssertEquals(1, quotation.Entries.Count);
			AssertEquals("20GP", quotation.Entries[0].Header7);
			AssertEquals("", quotation.Entries[0].Header8);

			AssertEquals(1, quotation.DestinationDocRateLineItems.Count);
			AssertEquals("Destination Charges - Melbourne", quotation.DestinationDocRateLineItems[0].OverseasChargesHeading);
			AssertEquals("Destination Documentation Fee *|AUD|30.00|", quotation.DestinationDocRateLineItems[0].QuotationLine.ToString());
		}

		public void TestWithFreightConsignorConsignee()
		{
			ClientRate testRate = Helper.NewClientRate(Helper.NewOrgHeader());

			RateEntry entry1 = testRate.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			entry1.RateLines[0].GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 120m;
			RateEntry entry2 = testRate.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			entry2.TI_OH_Consignor = Helper.NewOrgHeader().PK;
			entry2.RateLines[0].GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 110m;
			RateEntry entry3 = testRate.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			entry3.TI_OH_Consignee = Helper.NewOrgHeader().PK;
			entry3.RateLines[0].GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 100m;

			Factory.Save();

			PricingPageCollection pages = new PricingPageCollection(testRate);
			pages.Load(PricingPaginationStrategy.LandscapeSimpleStyle);
			AssertEquals(1, pages.Count);

			DocTableQuotation quotation = DocTableQuotation.New(pages[0], Factory);
			AssertEquals(3, quotation.Entries.Count);
		}

		public void TestDomesticFreight()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry("AU");

				Quote quote = Helper.NewQuote(Helper.NewOrgHeader());

				RateEntry entry1 = quote.AddRateEntry("AIR", "LSE", "AUSYD", "AUMEL");
				entry1.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
				entry1.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 2m;

				RateEntry entry2 = quote.AddRateEntry("ORG", "AIR", "AUSYD", "");
				entry2.AddRateLine("ODOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 20m;

				RateEntry entry3 = quote.AddRateEntry("DST", "AIR", "", "AUMEL");
				entry3.AddRateLine("DDOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 15m;

				Factory.Save();

				PricingPageCollection pages = new PricingPageCollection(quote);
				pages.Load(PricingPaginationStrategy.LandscapeComplexStyle);
				AssertEquals(1, pages.Count);

				DocTableQuotation quotation = DocTableQuotation.New(pages[0], Factory);

				AssertEquals("Domestic Air Freight Rates", quotation.PageHeading);
				AssertEquals("Sydney - Melbourne", quotation.Entries[0].OriginDestination);
				AssertEquals("Origin Charges - Sydney", quotation.OriginDocRateLineItems[0].LocalChargesHeading);
				AssertEquals("Destination Charges - Melbourne", quotation.DestinationDocRateLineItems[0].OverseasChargesHeading);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			base.SetUp();

			originalAlternateRateFormatValue = DocumentsDataRegistry.Instance.AlternativeRateFormat.Value;
			DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainGSTTaxRegistryID, Env.CurrentCompanyPK);
			rate.SetRateNumerator_ForTestOnly(10);
			rate.Factory.Save();
		}

		bool originalAlternateRateFormatValue;

		protected override void TearDown()
		{
			DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalAlternateRateFormatValue);
			base.TearDown();
		}

		#endregion
	}
}
