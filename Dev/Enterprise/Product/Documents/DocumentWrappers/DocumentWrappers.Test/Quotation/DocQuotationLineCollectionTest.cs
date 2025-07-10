using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.Shared;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RatingEnums;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	sealed class DocQuotationLineCollectionTest : TestCaseWithFactory
	{
		public void TestCrossTradeWithCompanyTariff()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUMEL";

			var tariff = Factory.New<CompanyTariff>();
			var tariffEntry = tariff.AddRateEntry("DST", "AIR", "", "AUSYD");
			var tariffLine = tariffEntry.AddRateLine("DCART", UnitCalculator.Code, QuantityUnit.KG);
			tariffLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2m;

			var tariffEntry2 = tariff.AddRateEntry("DST", "AIR", "", "");
			tariffEntry2.TI_IsCrossTrade = true;
			var tariffLine2 = tariffEntry2.AddRateLine("DADF", UnitCalculator.Code, QuantityUnit.KG);
			tariffLine2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)20m;

			var tariffEntry3 = tariff.AddRateEntry("AIR", "LSE", "", "");
			tariffEntry3.TI_IsCrossTrade = true;
			var tariffLine3 = tariffEntry3.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG);
			tariffLine3.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)8m;

			var testQuote = Helper.NewQuote(Helper.NewOrgHeader(1));
			var quoteEntry1 = testQuote.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD");
			quoteEntry1.RateLines[0].TL_WeightVolume = "KG";
			quoteEntry1.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			quoteEntry1.RateLines[0].Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)200m;

			var quoteEntry2 = testQuote.AddRateEntry("AIR", "LSE", "USLAX", "GBLON");
			quoteEntry2.RateLines[0].TL_WeightVolume = "KG";
			quoteEntry2.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			quoteEntry2.RateLines[0].Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)700m;

			var quotation = DocEntryQuotation.New(new PricingPage(quoteEntry1, Factory, PricingPageStyle.Standard), Factory);
			Factory.Save();

			const string expectedDestination1 = @"
Delivery Cartage *|AUD|2.00|per KG / 6000 CC
";

			const string expectedFreight1 = @"
International Freight|USD|200.00|
";

			AssertMultilineASCIIEquals("", expectedDestination1, Render(quotation.DestinationDocRateLineItems));
			AssertMultilineASCIIEquals("", expectedFreight1, Render(quotation.FreightDocRateLineItems));

			quotation = DocEntryQuotation.New(new PricingPage(quoteEntry2, Factory, PricingPageStyle.Standard), Factory);

			const string expectedDestination2 = @"
Destination Airline Document Fee *|AUD|20.00|per KG / 6000 CC
";

			const string expectedFreight2 = @"
International Freight|USD|700.00|
Bunker Adjustment Factor|AUD|8.00|per KG / 6000 CC
";

			AssertMultilineASCIIEquals("", expectedDestination2, Render(quotation.DestinationDocRateLineItems));
			AssertMultilineASCIIEquals("", expectedFreight2, Render(quotation.FreightDocRateLineItems));
		}

		public void TestOriginAIR()
		{
			Quote quote = Helper.NewQuote(Helper.NewOrgHeader());

			RateEntry entry1 = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			RateLine line1 = entry1.RateLines[0];
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			RateEntry entry2 = quote.AddRateEntry("ORG", "AIR", "AUSYD", "");
			RateLine line2 = entry2.AddRateLine("ODOC", FlatCalculator.Code);
			line2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)45m;

			RateEntry entry3 = quote.AddRateEntry("ORG", "AIR", "AU", "");
			RateLine line3 = entry3.AddRateLine("ODOC", FlatCalculator.Code);
			line3.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)30m;

			RateEntry entry4 = quote.AddRateEntry("ORG", "AIR", "AUEC", "");
			RateLine line4 = entry4.AddRateLine("OAWB", FlatCalculator.Code);
			line4.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)15m;

			RateEntry entry5 = quote.AddRateEntry("ORG", "AIR", "AUMEL", "");
			RateLine line5 = entry5.AddRateLine("OCAA", FlatCalculator.Code);
			line5.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)12.5m;

			var quotation = DocEntryQuotation.New(new PricingPage(entry1, Factory, PricingPageStyle.Standard), Factory);
			var collection = new DocQuotationLineCollection(quotation, new PricingPageRateLineFactory(EntryTypes.Origin), new RateEntry[] { entry1 }, Factory);
			Factory.Save();
			collection.Load();

			const string expected1 = @"
Origin Documentation Fee *|AUD|45.00|
Origin Airway Bill Fee *|AUD|15.00|";

			AssertMultilineASCIIEquals("", expected1, Render(collection));
			AssertEquals("Sydney", collection[0].Origin);
			AssertEquals("Sydney", collection[1].Origin);

			quotation = DocEntryQuotation.New(new PricingPage(entry3, Factory, PricingPageStyle.Standard), Factory);
			collection = new DocQuotationLineCollection(quotation, new PricingPageRateLineFactory(EntryTypes.Origin), new RateEntry[] { entry5 }, Factory);
			collection.Load();

			const string expected2 = @"
Origin Documentation Fee *|AUD|30.00|
Origin Airway Bill Fee *|AUD|15.00|
Origin Cargo Automation Fee *|AUD|12.50|
";

			AssertMultilineASCIIEquals("", expected2, Render(collection));
			CombineAssertions(delegate
			{
				AssertEquals("Australia", collection[0].Origin);
				AssertEquals("Australia East Coast", collection[1].Origin);
				AssertEquals("Melbourne", collection[2].Origin);
			});
		}

		public void TestWithCompanyTariffPerUnitQuoteCartageContainerised()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry1 = testRate.AddRateEntry("AIR", "ULD", "USLAX", "AUSYD", "STD", "LD-8");
			entry1.RateLines.RemoveAndDeleteAll();
			var line1 = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)500m;

			var entry2 = testRate.AddRateEntry("DST", "AIR", "", "AUSYD");
			var line2 = entry2.AddRateLine("DCART", UnitCalculator.Code, QuantityUnit.CN);
			line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)300m;

			var entry3 = testRate.AddRateEntry("DST", "AIR", "USLAX", "AUSYD");
			var line3 = entry3.AddRateLine("DCART", CartageCalculator.Code, QuantityUnit.CN);
			line3.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)350m;
			line1.Calculator[CartageCalculator.Items.EquipmentType] = (ZString)"PSL";
			line1.Calculator["String2"] = (ZDecimal)166.667m;

			var pages = new PricingPageCollection(testRate);
			pages.Load(PricingPaginationStrategy.LandscapeSimpleStyle);
			AssertEquals(1, pages.Count);

			var testQuotation = DocTableQuotation.New(pages[0], Factory);
			Factory.Save();

			var collection = testQuotation.DestinationDocRateLineItems;

			AssertContainsExactElementsInExactOrder
			(
				new[]
				{
					"Delivery Cartage *|||",
					"Delivery Cartage *|||",
					"LD-8 (No Equipment Specified)|AUD|300.00|per Container",
					@"LD-8 
-  Premise Supplies Lift|AUD|350.00|per Container"
				},
				collection.Cast<DocRateLineItem>().Select(x => x.QuotationLine.ToString())
			);
		}

		public void TestDestinationOnlyWithEquipment()
		{
			ClientRate testRate = Helper.NewClientRate(Helper.NewOrgHeader());

			RateEntry entry1 = testRate.AddRateEntry("DST", "FCL", "", "AUSYD", "STD", "20GP");
			RateLine line1 = entry1.AddRateLine("DCART", CartageCalculator.Code, QuantityUnit.CN);
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)300m;
			line1.Calculator[CartageCalculator.Items.EquipmentType] = (ZString)"SDL";

			RateEntry entry2 = testRate.AddRateEntry("DST", "SEA", "", "AUSYD");
			RateLine line2 = entry2.AddRateLine("DCART", UnitCalculator.Code, QuantityUnit.KG);
			line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)500m;

			PricingPageCollection pages = new PricingPageCollection(testRate);
			pages.Load(PricingPaginationStrategy.LandscapeSimpleStyle);
			AssertEquals(1, pages.Count);

			DocTableQuotation testQuotation = DocTableQuotation.New(pages[0], Factory);
			Factory.Save();

			DocQuotationLineCollection collection = testQuotation.DestinationDocRateLineItems;

			const string expected = @"
Delivery Cartage *|||
20GP 
-  Drop Container with Sideloader|AUD|300.00|per Container
Delivery Cartage *|AUD|500.00|per KG (1 M3 = 1000 KG)
";

			AssertMultilineASCIIEquals("", expected, Render(collection));
			AssertEquals("", collection[0].Destination);
			AssertEquals("", collection[1].Destination);
			AssertEquals("", collection[2].Destination);
		}

		public void TestDestinationFCL()
		{
			Quote quote = Helper.NewQuote(Helper.NewOrgHeader());

			RateEntry entry1 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "20GP");
			entry1.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1000m;

			RateEntry entry2 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "40GP");
			entry2.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1800m;

			RateEntry entry2A = quote.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "40HC");
			entry2A.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1900m;

			RateEntry entry3 = quote.AddRateEntry("DST", "FCL", "", "USLAX", "", "20GP");
			entry3.AddRateLine("DPCH", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 140m;

			RateEntry entry4 = quote.AddRateEntry("DST", "FCL", "", "USLAX");
			entry4.AddRateLine("DPCH", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 220m;
			entry4.AddRateLine("DDOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 30m;

			RateEntry entry5 = quote.AddRateEntry("DST", "FCL", "", "US");

			Factory.Save();

			var pages = new PricingPageCollection(quote);
			pages.Load(PricingPaginationStrategy.StandardStyle);
			AssertEquals(1, pages.Count);

			DocEntryQuotation quotation = DocEntryQuotation.New(pages[0], Factory);

			var collection = new DocQuotationLineCollection(quotation, new PricingPageRateLineFactory(EntryTypes.Destination), pages[0].RateEntries, Factory);
			collection.Load();

			const string expected = @"
Destination Port Charges *|||
40GP, 40HC|USD|220.00|
20GP|USD|140.00|
Destination Documentation Fee *|USD|30.00|
";

			AssertMultilineASCIIEquals("", expected, Render(collection));
			AssertEquals("Los Angeles", collection[0].Destination);
			AssertEquals("Los Angeles", collection[1].Destination);
			AssertEquals("Los Angeles", collection[2].Destination);
			AssertEquals("Los Angeles", collection[3].Destination);
		}

		public void TestDestinationEntriesFallbackToClientRate()
		{
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var rateEntryAU = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.LCL, "AU", "", "DDOC", 100, "AUD");

			var quote = Helper.NewQuote(client);
			var quoteEntryCN = quote.AddRateEntry(RatingConstants.RateCategory.DST, Constants.RateMode.LCL, "CN", "");
			var quoteEntryAU = quote.AddRateEntry(RatingConstants.RateCategory.DST, Constants.RateMode.LCL, "AU", "");
			Factory.Save();

			var pricingPages = new PricingPageCollection(quote);
			pricingPages.Load(PricingPaginationStrategy.StandardStyle);

			var actualEntries = pricingPages.Cast<PricingPage>().SelectMany(p => p.RateEntries).Select(e => e.PK);
			var expectedEntries = new ZGuid[]
			{
				quoteEntryCN.PK,
				quoteEntryAU.PK,
			};

			var message = "Should include both quote entries even though they have no rate lines.";
			AssertContainsExactElementsInAnyOrder(message, expectedEntries, actualEntries);

			var quotation = DocEntryQuotation.New(pricingPages[0], Factory);
			var lineSetFactory = new PricingPageRateLineFactory(EntryTypes.Destination);
			var collection = new DocQuotationLineCollection(quotation, lineSetFactory, pricingPages[0].RateEntries, Factory);
			collection.Load();

			Assert("CN Pricing page should have no results as there are no rate lines", !collection.Any());

			quotation = DocEntryQuotation.New(pricingPages[1], Factory);
			lineSetFactory = new PricingPageRateLineFactory(EntryTypes.Destination);
			collection = new DocQuotationLineCollection(quotation, lineSetFactory, pricingPages[1].RateEntries, Factory);
			collection.Load();

			var expected = "Destination Documentation Fee *|AUD|100.00|";
			message = "Should find the client rate as it matches the quote entry";

			AssertMultilineASCIIEquals(message, expected, Render(collection).Trim());
		}

		public void TestOriginDestinationWithCountryToCountryFreight()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			Quote testQuote = Helper.NewQuote(Helper.NewOrgHeader());

			RateEntry entry1 = testQuote.AddRateEntry("AIR", "LSE", "AU", "US");
			RateLine line1 = entry1.RateLines[0];
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			RateEntry entry2 = testQuote.AddRateEntry("ORG", "AIR", "AUSYD", "");
			RateLine line2 = entry2.AddRateLine("ODOC", FlatCalculator.Code);
			line2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)45m;

			RateEntry entry3 = testQuote.AddRateEntry("ORG", "ALL", "AUMEL", "");
			RateLine line3 = entry3.AddRateLine("ODOC", FlatCalculator.Code);
			line3.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)30m;

			RateEntry entry4 = testQuote.AddRateEntry("DST", "AIR", "", "USLAX");
			RateLine line4 = entry4.AddRateLine("DDOC", FlatCalculator.Code);
			line4.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)25m;

			RateEntry entry5 = testQuote.AddRateEntry("DST", "ALL", "", "USSFO");
			RateLine line5 = entry5.AddRateLine("DDOC", FlatCalculator.Code);
			line5.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)50m;

			PricingPageCollection pages = new PricingPageCollection(testQuote);
			pages.Load(PricingPaginationStrategy.LandscapeComplexStyle);
			AssertEquals(1, pages.Count);

			DocTableQuotation quotation = DocTableQuotation.New(pages[0], Factory);
			Factory.Save();

			const string expectedLocal = @"
Origin Documentation Fee *|AUD|45.00|
Origin Documentation Fee *|AUD|30.00|
";

			const string expectedOverseas = @"
Destination Documentation Fee *|USD|25.00|
Destination Documentation Fee *|USD|50.00|
";

			AssertMultilineASCIIEquals("", expectedLocal, Render(quotation.LocalDocRateLineItems));
			AssertMultilineASCIIEquals("", expectedOverseas, Render(quotation.OverseasDocRateLineItems));
		}

		public void TestOverridenCountryOrigin()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			Quote quote = Helper.NewQuote(Helper.NewOrgHeader());

			RateEntry entry1 = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			RateLine line1 = entry1.RateLines[0];
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			RateEntry entry2 = quote.AddRateEntry("ORG", "AIR", "AU", "");
			RateLine line2 = entry2.AddRateLine("ODOC", FlatCalculator.Code);
			line2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)45m;

			RateEntry entry3 = quote.AddRateEntry("ORG", "AIR", "AUSYD", "");
			RateLine line3 = entry3.AddRateLine("ODOC", FlatCalculator.Code);
			line3.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)30m;

			Factory.Save();

			var pages = new PricingPageCollection(quote);
			pages.Load(PricingPaginationStrategy.LandscapeComplexStyle);
			AssertEquals(2, pages.Count);

			DocTableQuotation quotation = DocTableQuotation.New(pages[0], Factory);

			const string expectedLocal = @"
Origin Documentation Fee *|AUD|30.00|
";

			const string expectedOverseas = @"
";

			AssertMultilineASCIIEquals("", expectedLocal, Render(quotation.LocalDocRateLineItems));
			AssertMultilineASCIIEquals("", expectedOverseas, Render(quotation.OverseasDocRateLineItems));
		}

		public void TestWithOriginDestinationParentEntries()
		{
			Quote testQuote = Helper.NewQuote(Helper.NewOrgHeader());

			RateEntry entry1 = testQuote.AddRateEntry("ORG", "AIR", "AUSYD", "US");
			RateLine line1 = entry1.AddRateLine("ODOC", FlatCalculator.Code);
			line1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)25m;

			RateEntry entry2 = testQuote.AddRateEntry("DST", "AIR", "AU", "USLAX");
			RateLine line2 = entry2.AddRateLine("DDOC", FlatCalculator.Code);
			line2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)15m;

			RateEntry entry3 = testQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			RateLine line3 = entry3.RateLines[0];
			line3.TL_RateCalculator = UnitCalculator.Code;
			line3.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			RateEntry entry4 = testQuote.AddRateEntry("AIR", "LSE", "AU", "US");
			RateLine line4 = entry4.RateLines[0];
			line4.TL_RateCalculator = UnitCalculator.Code;
			line4.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)8m;

			const string expectedEmpty = @"
";

			const string expectedOrigin = @"
Origin Documentation Fee *|AUD|25.00|
";

			const string expectedDestination = @"
Destination Documentation Fee *|USD|15.00|
";

			const string expectedFreight = @"
International Freight|AUD|5.00|per KG / 6000 CC
";

			DocEntryQuotation quotation = DocEntryQuotation.New(new PricingPage(entry1, Factory, PricingPageStyle.Standard), Factory);
			Factory.Save();
			AssertMultilineASCIIEquals("", expectedEmpty, Render(quotation.FreightDocRateLineItems));
			AssertMultilineASCIIEquals("", expectedOrigin, Render(quotation.OriginDocRateLineItems));
			AssertMultilineASCIIEquals("", expectedEmpty, Render(quotation.DestinationDocRateLineItems));

			quotation = DocEntryQuotation.New(new PricingPage(entry2, Factory, PricingPageStyle.Standard), Factory);
			AssertMultilineASCIIEquals("", expectedEmpty, Render(quotation.FreightDocRateLineItems));
			AssertMultilineASCIIEquals("", expectedEmpty, Render(quotation.OriginDocRateLineItems));
			AssertMultilineASCIIEquals("", expectedDestination, Render(quotation.DestinationDocRateLineItems));

			quotation = DocEntryQuotation.New(new PricingPage(entry3, Factory, PricingPageStyle.Standard), Factory);
			AssertMultilineASCIIEquals("", expectedFreight, Render(quotation.FreightDocRateLineItems));
			AssertMultilineASCIIEquals("", expectedOrigin, Render(quotation.OriginDocRateLineItems));
			AssertMultilineASCIIEquals("", expectedDestination, Render(quotation.DestinationDocRateLineItems));
		}

		public void TestWithCompanyTariffPerUnitQuoteCartage()
		{
			var tariff = Factory.New<CompanyTariff>();
			var tariffEntry = tariff.AddRateEntry("DST", "AIR", "", "AUSYD");
			var tariffLine = tariffEntry.AddRateLine("DCART", UnitCalculator.Code, QuantityUnit.KG);
			tariffLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2m;

			var testQuote = Helper.NewQuote(Helper.NewOrgHeader(1));
			var entry1 = testQuote.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD");
			var entry2 = testQuote.AddRateEntry("DST", "AIR", "USLAX", "AUSYD");
			var line2 = entry2.AddRateLine("DCART", CartageCalculator.Code, QuantityUnit.KG);
			line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2.5m;

			var quotation = DocEntryQuotation.New(new PricingPage(entry1, Factory, PricingPageStyle.Standard), Factory);
			Factory.Save();

			AssertContainsExactElementsInExactOrder
			(
				new[]
				{
					"Delivery Cartage *|||",
					"Delivery Cartage *|||",
					"(No Equipment Specified)|AUD|2.00|per KG / 6000 CC",
					@"
-  Premise Supplies Lift|AUD|2.50|per KG / 6000 CC"
				},
				quotation.DestinationDocRateLineItems.Cast<DocRateLineItem>().Select(x => x.QuotationLine.ToString())
			);
		}

		public void TestConsignorSpecificRates()
		{
			CompanyTariff testTariff = Helper.NewCompanyTariff();

			RateEntry tariffEntry = testTariff.AddRateEntry("FCL", "SEA", "", "AUSYD");
			var tariffFactory = testTariff.Factory;

			tariffEntry.TI_OH_Consignor = tariffFactory.NewWithValidTestData<OrgHeader>().PK;
			RateLine tariffLine = tariffEntry.RateLines[0];
			tariffLine.TL_WeightVolume = "CN";
			tariffLine.TL_RateCalculator = UnitCalculator.Code;
			tariffLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2m;

			RateLine tariffLineSurcharge = tariffEntry.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.CN);
			tariffLineSurcharge.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)20m;

			RateEntry tariffEntry2 = testTariff.AddRateEntry("FCL", "SEA", "", "AUSYD");
			tariffEntry2.TI_OH_Consignor = tariffFactory.NewWithValidTestData<OrgHeader>().PK;
			RateLine tariffLine2 = tariffEntry2.RateLines[0];
			tariffLine2.TL_WeightVolume = "CN";
			tariffLine2.TL_RateCalculator = UnitCalculator.Code;
			tariffLine2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)3m;

			RateLine tariffLine2Surcharge = tariffEntry2.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.CN);
			tariffLine2Surcharge.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)30m;

			tariffFactory.Save();

			DocEntryQuotation quotation = DocEntryQuotation.New(new PricingPage(tariffEntry, tariffFactory, PricingPageStyle.Standard), tariffFactory);

			const string expected1 = @"
International Freight|USD|2.00|per Container
Bunker Adjustment Factor|USD|20.00|per Container
";

			AssertMultilineASCIIEquals("", expected1, Render(quotation.FreightDocRateLineItems));

			quotation = DocEntryQuotation.New(new PricingPage(tariffEntry2, tariffFactory, PricingPageStyle.Standard), tariffFactory);

			const string expected2 = @"
International Freight|USD|3.00|per Container
Bunker Adjustment Factor|USD|30.00|per Container
";

			AssertMultilineASCIIEquals("", expected2, Render(quotation.FreightDocRateLineItems));
		}

		public void TestWarehouseRate()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			BusinessObject warehouse1 = Helper.NewWarehouse();
			BusinessObject warehouse2 = Helper.NewWarehouse();

			ClientRate rate = Helper.NewClientRate(Helper.NewOrgHeader());

			OrgSupplierPart part1 = Helper.NewOrgSupplierPart(rate.Header);
			OrgSupplierPart part2 = Helper.NewOrgSupplierPart(rate.Header);

			RateEntry entryWhs1 = rate.AddRateEntry("WHS", "ALL", "", "");
			entryWhs1.TI_WW_Warehouse = warehouse1.PK;
			entryWhs1.AddRateLine("ODOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)10m;
			entryWhs1.AddRateLine("OCART", UnitCalculator.Code, QuantityUnit.M3).Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1m;
			entryWhs1.RateLines[1].TL_WeightVolume = "M3";
			entryWhs1.RateLines[1].UseOnlyActualWeightMeasure = false;
			entryWhs1.RateLines[1].ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			RateEntry entryWhs2 = rate.AddRateEntry("WHS", "ALL", "", "");
			entryWhs2.TI_WW_Warehouse = warehouse2.PK;
			entryWhs2.AddRateLine("OCART", UnitCalculator.Code, QuantityUnit.M3).Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2m;
			entryWhs2.RateLines[0].TL_WeightVolume = "M3";
			entryWhs2.RateLines[0].UseOnlyActualWeightMeasure = false;
			entryWhs2.RateLines[0].ConversionFactor = new ConversionFactor(333m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			entryWhs2.RateLines[0].TL_OP_ProductNumber = part1.PK;

			RateEntry entryAll = rate.AddRateEntry("WHS", "ALL", "", "");
			entryAll.AddRateLine("ODOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)15m;
			entryAll.AddRateLine("OCART", UnitCalculator.Code, QuantityUnit.M3).Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)3m;
			entryAll.RateLines[1].TL_WeightVolume = "M3";
			entryAll.RateLines[1].TL_OP_ProductNumber = part2.PK;

			PricingPageCollection pages = new PricingPageCollection(rate);
			pages.Load(PricingPaginationStrategy.LandscapeSimpleStyle);
			AssertEquals(1, pages.Count);

			DocTableQuotation quotation = DocTableQuotation.New(pages[0], Factory);
			Factory.Save();

			const string expected1 = @"
Origin Documentation Fee * 
-  Warehouse: WHS1|AUD|10.00|
Pick Up Cartage * 
-  for PROD1 (###1)
-  Warehouse: WHS2|AUD|2.00|per M3 / 333 KG
Origin Documentation Fee *|AUD|15.00|
Pick Up Cartage * 
-  Warehouse: WHS1|AUD|1.00|per M3 / 250 KG
Pick Up Cartage * 
-  for PROD2 (###2)|AUD|3.00|per M3
";

			AssertMultilineASCIIEquals("", expected1, Render(quotation.OriginDocRateLineItems));
		}

		public void TestLinesRemover()
		{
			var org = SetupOrgHeader();
			Quote quote = Helper.NewQuote(org);

			RateEntry entry1 = quote.AddRateEntry("AIR", "LSE", "AU", "US", "STD", "");
			RateLine line1 = entry1.RateLines[0];
			line1.TL_RateCalculator = UnitCalculator.Code;
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)10m;

			quote.AddRateEntry("ORG", "AIR", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)10m;
			quote.AddRateEntry("ORG", "AIR", "AUSYD", "", "STD", "").AddRateLine("ODOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)11m;
			quote.AddRateEntry("ORG", "AIR", "AU", "").AddRateLine("ODOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)12m;
			quote.AddRateEntry("ORG", "ALL", "AU", "").AddRateLine("ODOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)14m;

			QuoteFormatEntryCollection formatEntries = quote.QuoteFormatEntries;
			formatEntries.LoadEntries();
			Factory.Save();
			AssertEquals(3, formatEntries.Count);

			DocEntryQuotation quotation = DocEntryQuotation.New(new PricingPage(formatEntries[0], Factory, PricingPageStyle.Standard), Factory);
			DocQuotationLineCollection collection = quotation.OriginDocRateLineItems;

			const string expected = @"
Origin Documentation Fee *|AUD|11.00|
Origin Documentation Fee *|AUD|12.00|
";

			AssertMultilineASCIIEquals("", expected, Render(collection));
			AssertEquals("Sydney", collection[0].Origin);
			AssertEquals("Australia", collection[1].Origin);
		}

		public void TestWithIncoTerm()
		{
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry("AU");
				var org = SetupOrgHeader();
				Quote testQuote = Helper.NewQuote(org);

				RateEntry entry1 = testQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
				RateLine line1 = entry1.RateLines[0];
				line1.TL_RateCalculator = UnitCalculator.Code;
				line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

				RateEntry entry2 = testQuote.AddRateEntry("AIR", "LSE", "GBLON", "AUMEL");
				RateLine line2 = entry2.RateLines[0];
				line2.TL_RateCalculator = UnitCalculator.Code;
				line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)3m;

				RateEntry entry3 = testQuote.AddRateEntry("AIR", "LSE", "DEFRA", "SGSIN");
				RateLine line3 = entry3.RateLines[0];
				line3.TL_RateCalculator = UnitCalculator.Code;
				line3.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)4m;

				testQuote.AddRateEntry("ORG", "AIR", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)10m;
				testQuote.AddRateEntry("ORG", "AIR", "GBLON", "").AddRateLine("ODOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)11m;
				testQuote.AddRateEntry("ORG", "AIR", "DEFRA", "").AddRateLine("ODOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)12m;
				testQuote.AddRateEntry("DST", "AIR", "", "USLAX").AddRateLine("DDOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)20m;
				testQuote.AddRateEntry("DST", "AIR", "", "AUMEL").AddRateLine("DDOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)21m;
				testQuote.AddRateEntry("DST", "AIR", "", "SGSIN").AddRateLine("DDOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)22m;

				QuoteFormatEntryCollection formatEntries = testQuote.QuoteFormatEntries;
				formatEntries.LoadEntries();

				formatEntries.Sort<QuoteEntry>((t1, t2) => StringComparer.OrdinalIgnoreCase.Compare(t1.TI_OriginLRC, t2.TI_OriginLRC));

				Factory.Save();

				AssertEquals(3, formatEntries.Count);
				AssertQuoteEntry(formatEntries[0], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[1], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[2], "ODOC", "FRT", "DDOC");

				entry1.TI_QuotePageIncoTerm = entry2.TI_QuotePageIncoTerm = entry3.TI_QuotePageIncoTerm = "EXW";
				AssertQuoteEntry(formatEntries[0]);
				AssertQuoteEntry(formatEntries[1], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[2], "ODOC", "FRT", "DDOC");

				entry1.TI_QuotePageIncoTerm = entry2.TI_QuotePageIncoTerm = entry3.TI_QuotePageIncoTerm = "DDP";
				AssertQuoteEntry(formatEntries[0], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[1], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[2]);

				entry1.TI_QuotePageIncoTerm = entry2.TI_QuotePageIncoTerm = entry3.TI_QuotePageIncoTerm = "FOB";
				AssertQuoteEntry(formatEntries[0], "ODOC");
				AssertQuoteEntry(formatEntries[1], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[2], "FRT", "DDOC");

				entry1.TI_QuotePageIncoTerm = entry2.TI_QuotePageIncoTerm = entry3.TI_QuotePageIncoTerm = "CIF";
				AssertQuoteEntry(formatEntries[0], "ODOC", "FRT");
				AssertQuoteEntry(formatEntries[1], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[2], "DDOC");

				testQuote.Header.OH_RL_NKClosestPort = "USSFO";
				entry1.TI_QuotePageIncoTerm = entry2.TI_QuotePageIncoTerm = entry3.TI_QuotePageIncoTerm = "EXW";
				AssertQuoteEntry(formatEntries[0], "ODOC", "FRT");
				AssertQuoteEntry(formatEntries[1], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[2], "ODOC", "FRT", "DDOC");

				entry1.TI_QuotePageIncoTerm = entry2.TI_QuotePageIncoTerm = entry3.TI_QuotePageIncoTerm = "DDP";
				AssertQuoteEntry(formatEntries[0]);
				AssertQuoteEntry(formatEntries[1], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[2], "ODOC", "FRT", "DDOC");

				entry1.TI_QuotePageIncoTerm = entry2.TI_QuotePageIncoTerm = entry3.TI_QuotePageIncoTerm = "FOB";
				AssertQuoteEntry(formatEntries[0], "FRT");
				AssertQuoteEntry(formatEntries[1], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[2], "ODOC", "FRT", "DDOC");

				entry1.TI_QuotePageIncoTerm = entry2.TI_QuotePageIncoTerm = entry3.TI_QuotePageIncoTerm = "CIF";
				AssertQuoteEntry(formatEntries[0]);
				AssertQuoteEntry(formatEntries[1], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[2], "ODOC", "FRT", "DDOC");

				testQuote.Header.OH_RL_NKClosestPort = "GBLON";
				entry1.TI_QuotePageIncoTerm = entry2.TI_QuotePageIncoTerm = entry3.TI_QuotePageIncoTerm = "EXW";
				AssertQuoteEntry(formatEntries[0], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[1], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[2]);

				entry1.TI_QuotePageIncoTerm = entry2.TI_QuotePageIncoTerm = entry3.TI_QuotePageIncoTerm = "DDP";
				AssertQuoteEntry(formatEntries[0], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[1], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[2], "DDOC");

				entry1.TI_QuotePageIncoTerm = entry2.TI_QuotePageIncoTerm = entry3.TI_QuotePageIncoTerm = "FOB";
				AssertQuoteEntry(formatEntries[0], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[1], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[2]);

				entry1.TI_QuotePageIncoTerm = entry2.TI_QuotePageIncoTerm = entry3.TI_QuotePageIncoTerm = "CIF";
				AssertQuoteEntry(formatEntries[0], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[1], "ODOC", "FRT", "DDOC");
				AssertQuoteEntry(formatEntries[2]);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		#region

		public void TestRemovePercentagesOfZeroRateLine()
		{
			var quoteEntry = GetQuoteEntryWithPercentageCalc(Helper.NewOrgHeader());
			var bafLine = quoteEntry.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG);
			bafLine.GetCalculator<UnitCalculator>().PerUnit = 0.8m;
			var applyTo = AddPercentageLine(quoteEntry, "FRT");

			var expected = @"
International Freight|AUD|5.00|per KG / 6000 CC
Bunker Adjustment Factor|AUD|0.80|per KG / 6000 CC
War Risk Surcharge||5.00|% of Freight
";

			var pricingPage = new PricingPage(quoteEntry, Factory, PricingPageStyle.Standard);
			var collection = DocEntryQuotation.New(pricingPage, Factory).FreightDocRateLineItems;

			AssertMultilineASCIIEquals("", expected, Render(collection));

			applyTo.TM_AC = Helper.ChargeCodes["CAF"].PK;

			collection = DocEntryQuotation.New(pricingPage, Factory).FreightDocRateLineItems;
			expected = @"
International Freight|AUD|5.00|per KG / 6000 CC
Bunker Adjustment Factor|AUD|0.80|per KG / 6000 CC
";

			AssertMultilineASCIIEquals("As there is no CAF line so WAR line can't be included", expected, Render(collection));

			applyTo.TM_AC = Helper.ChargeCodes["BAF"].PK;

			collection = DocEntryQuotation.New(pricingPage, Factory).FreightDocRateLineItems;
			expected = @"
International Freight|AUD|5.00|per KG / 6000 CC
Bunker Adjustment Factor|AUD|0.80|per KG / 6000 CC
War Risk Surcharge||5.00|% of Bunker Adjustment Factor
";

			AssertMultilineASCIIEquals("WAR Line should apply to BAF line", expected, Render(collection));

			bafLine.GetCalculator<UnitCalculator>().PerUnit = 0m;

			collection = DocEntryQuotation.New(pricingPage, Factory).FreightDocRateLineItems;
			expected = @"
International Freight|AUD|5.00|per KG / 6000 CC
";

			AssertMultilineASCIIEquals("BAF line has no value so should not be shown. War based on BAF so it can't be shown either", expected, Render(collection));
		}

		public void TestRemovePercentagesOfZeroRateLineWithTariff()
		{
			var tariff = Factory.New<CompanyTariff>();
			var tariffEntry = tariff.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			tariffEntry.RateLines.RemoveAndDeleteAll();
			var tariffLine = tariffEntry.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG);
			tariffLine.GetCalculator<UnitCalculator>().PerUnit = 0.9m;

			var quoteEntry = GetQuoteEntryWithPercentageCalc(Helper.NewOrgHeader(1));
			AddPercentageLine(quoteEntry, "BAF");
			Factory.Save();

			var pricingPage = new PricingPage(quoteEntry, Factory, PricingPageStyle.Standard);
			var collection = DocEntryQuotation.New(pricingPage, Factory).FreightDocRateLineItems;

			var actual = Render(collection);
			AssertContains("Bunker Adjustment Factor|AUD|0.90|per KG / 6000 CC", actual);
			AssertContains("International Freight|AUD|5.00|per KG / 6000 CC", actual);
			AssertContains("War Risk Surcharge||5.00|% of Bunker Adjustment Factor", actual);

			quoteEntry = GetQuoteEntryWithPercentageCalc(Helper.NewOrgHeader(0));
			AddPercentageLine(quoteEntry, "BAF");

			pricingPage = new PricingPage(quoteEntry, Factory, PricingPageStyle.Standard);
			collection = DocEntryQuotation.New(pricingPage, Factory).FreightDocRateLineItems;

			actual = Render(collection);
			AssertContains("International Freight|AUD|5.00|per KG / 6000 CC", actual);
			AssertNotContains("Bunker Adjustment Factor|AUD|0.90|per KG / 6000 CC", actual);
			AssertNotContains("War Risk Surcharge||5.00|% of Bunker Adjustment Factor", actual);
		}

		RateEntry GetQuoteEntryWithPercentageCalc(OrgHeader parent)
		{
			var quote = Helper.NewQuote(parent);
			var quoteEntry = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var unitLine = quoteEntry.RateLines[0];
			unitLine.TL_RateCalculator = UnitCalculator.Code;
			unitLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			return quoteEntry;
		}

		RateLineItem AddPercentageLine(RateEntry parent, string percentageChargeCode)
		{
			var percentageLine = parent.AddRateLine("WAR", PercentageCalculator.Code);
			var percentageCalc = percentageLine.GetCalculator<PercentageCalculator>();
			percentageCalc.Percent = 5m;
			var applyToItem = percentageCalc.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			applyToItem.TM_AC = Helper.ChargeCodes[percentageChargeCode].PK;

			return applyToItem;
		}

		#endregion

		public void TestMultipleAgencyCalculators()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				var org = SetupOrgHeader();
				Quote testQuote = Helper.NewQuote(org);

				RateEntry entry1 = testQuote.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD");
				RateEntry entry2 = testQuote.AddRateEntry("DST", "AIR", "", "AUSYD");
				RateLine line2a = entry2.AddRateLine("CCLR", AgencyCalculator.Code);
				line2a.GetCalculator<AgencyCalculator>().AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
				line2a.GetCalculator<AgencyCalculator>().AgencyLineType = RateLineTypeList.Codes.FLAT;
				line2a.GetCalculator<AgencyCalculator>().AgencyRate = 100m;

				RateLine line2b = entry2.AddRateLine("CCLR", AgencyCalculator.Code);
				line2b.GetCalculator<AgencyCalculator>().MessageType = SharedJobMessageTypeList.Codes.ExWarehouse;
				line2b.GetCalculator<AgencyCalculator>().AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
				line2b.GetCalculator<AgencyCalculator>().AgencyLineType = RateLineTypeList.Codes.FLAT;
				line2b.GetCalculator<AgencyCalculator>().AgencyRate = 110m;

				RateLine line2c = entry2.AddRateLine("CCLR", AgencyCalculator.Code);
				line2c.GetCalculator<AgencyCalculator>().MessageType = SharedJobMessageTypeList.Codes.Import;
				line2c.GetCalculator<AgencyCalculator>().MessageSubType = "FRM";
				line2c.GetCalculator<AgencyCalculator>().AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
				line2c.GetCalculator<AgencyCalculator>().AgencyLineType = RateLineTypeList.Codes.FLAT;
				line2c.GetCalculator<AgencyCalculator>().AgencyRate = 120m;

				RateLine line2d = entry2.AddRateLine("CCLR", AgencyCalculator.Code);
				line2d.GetCalculator<AgencyCalculator>().MessageType = SharedJobMessageTypeList.Codes.Import;
				line2d.GetCalculator<AgencyCalculator>().MessageSubType = "SAC";
				line2d.GetCalculator<AgencyCalculator>().AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
				line2d.GetCalculator<AgencyCalculator>().AgencyLineType = RateLineTypeList.Codes.FLAT;
				line2d.GetCalculator<AgencyCalculator>().AgencyRate = 130m;

				QuoteFormatEntryCollection formatEntries = testQuote.QuoteFormatEntries;
				formatEntries.LoadEntries();
				Factory.Save();
				AssertEquals(1, formatEntries.Count);

				DocEntryQuotation quotation = DocEntryQuotation.New(new PricingPage(formatEntries[0], Factory, PricingPageStyle.Standard), Factory);
				DocQuotationLineCollection collection = quotation.DestinationDocRateLineItems;

				const string expected = @"
Customs Clearance / Agency Fees - Per Shipment, Flat Fee|AUD|100.00|
Customs Clearance / Agency Fees for Ex Warehouse - Per Shipment, Flat Fee|AUD|110.00|
Customs Clearance / Agency Fees for Import Formal Entry - Per Shipment, Flat Fee|AUD|120.00|
Customs Clearance / Agency Fees for Import Self Assessed Clearance - Per Shipment, Flat Fee|AUD|130.00|
";

				AssertMultilineASCIIEquals("", expected, Render(collection));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		public void TestCompanyTarrifBasedAgencyCalculator()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var tariff = Factory.New<CompanyTariff>();

				var tariffEntry = tariff.AddRateEntry("DST", "AIR", "", "AUSYD");
				var tariffLine = tariffEntry.AddRateLine("CCLR", AgencyCalculator.Code);
				tariffLine.Calculator.MessageType = SharedJobMessageTypeList.Codes.Import;
				tariffLine.Calculator.MessageSubType = "FRM";
				tariffLine.GetCalculator<AgencyCalculator>().AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
				tariffLine.GetCalculator<AgencyCalculator>().AgencyLineType = RateLineTypeList.Codes.FLAT;
				tariffLine.GetCalculator<AgencyCalculator>().AgencyRate = 150m;

				var org = SetupOrgHeader(1);

				var quote = Helper.NewQuote(org);

				quote.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD");
				var entry = quote.AddRateEntry("DST", "AIR", "", "AUSYD");
				var line = entry.AddRateLine("CCLR", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
				line.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = -25m;

				var formatEntries = quote.QuoteFormatEntries;
				formatEntries.LoadEntries();
				AssertEquals(1, formatEntries.Count);

				var quotation = DocEntryQuotation.New(new PricingPage(formatEntries[0], Factory, PricingPageStyle.Standard), Factory);
				Factory.Save();
				var collection = quotation.DestinationDocRateLineItems;

				var actual = Render(collection);
				AssertContains("Customs Clearance / Agency Fees - Per Shipment, Flat Fee|AUD|125.00|", actual);
				AssertContains("Customs Clearance / Agency Fees for Import Formal Entry - Per Shipment, Flat Fee|AUD|150.00|", actual);
			}
		}

		public void TestQuotationLinesAlternativeFomat()
		{
			Quote testQuote = Helper.NewQuote(Helper.NewOrgHeader(1));
			RateEntry entry1 = testQuote.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "40GP");
			entry1.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 1000m;
			Factory.Save();

			bool alternativeFormat = DocumentsDataRegistry.Instance.AlternativeRateFormat.Value;
			DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			try
			{
				PricingPageCollection pages = new PricingPageCollection(testQuote);
				pages.Load(PricingPaginationStrategy.StandardStyle);

				DocEntryQuotation quotation = DocEntryQuotation.New(pages[0], Factory);
				DocQuotationLineCollection collection = quotation.FreightDocRateLineItems;

				AssertEquals(1, collection.Count);
				AssertEquals("International Freight|USD|1000.00|per 40GP Container", collection[0].QuotationLine.ToString());
			}
			finally
			{
				DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, alternativeFormat);
			}
		}

		public void TestQuotationLinesDifferentDescriptions()
		{
			AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "DADF"));
			var org = SetupOrgHeader(1);
			Quote testQuote = Helper.NewQuote(org);
			RateEntry entry1 = testQuote.AddRateEntry("DST", "FCL", "AUSYD", "USLAX", "", "");

			RateLine line1 = entry1.RateLines.AddNew();
			line1.TL_AC = chargeCode.PK;
			line1.TL_RateCalculator = UnitCalculator.Code;
			line1.TL_WeightVolume = "M3";
			line1.GetCalculator<UnitCalculator>().PerUnit = 2000m;

			RateLine line2 = entry1.RateLines.AddNew();
			line2.TL_AC = chargeCode.PK;
			line2.TL_RateCalculator = UnitCalculator.Code;
			line2.TL_WeightVolume = "HB";
			line2.GetCalculator<UnitCalculator>().PerUnit = 3000m;

			RateLine line3 = entry1.RateLines.AddNew();
			line3.TL_AC = chargeCode.PK;
			line3.TL_RateCalculator = UnitCalculator.Code;
			line3.TL_WeightVolume = "KG";
			line3.GetCalculator<UnitCalculator>().PerUnit = 4000m;

			Factory.Save();

			QuoteFormatEntryCollection formatEntries = testQuote.QuoteFormatEntries;
			formatEntries.LoadEntries();

			DocEntryQuotation quotation = DocEntryQuotation.New(new PricingPage(formatEntries[0], Factory, PricingPageStyle.Standard), Factory);
			DocQuotationLineCollection collection = quotation.DestinationDocRateLineItems;

			const string expected1 = @"
Destination Airline Document Fee *|||
|USD|2000.00|per M3
|USD|3000.00|per House Bill
|USD|4000.00|per KG
";

			AssertMultilineASCIIEquals("", expected1, Render(collection));

			line1.TL_RateDesc = "Zayden";
			line2.TL_RateDesc = "Lola";
			line3.TL_RateDesc = "Lola";
			Factory.Save();

			formatEntries = testQuote.QuoteFormatEntries;
			formatEntries.LoadEntries();

			quotation = DocEntryQuotation.New(new PricingPage(formatEntries[0], Factory, PricingPageStyle.Standard), Factory);
			collection = quotation.DestinationDocRateLineItems;

			const string expected2 = @"
Zayden *|USD|2000.00|per M3
Lola *|||
|USD|3000.00|per House Bill
|USD|4000.00|per KG
";

			AssertMultilineASCIIEquals("", expected2, Render(collection));
		}

		public void TestQuotationLinesNoAlternativeFomatIfNotContainer()
		{
			Quote testQuote = Helper.NewQuote(Helper.NewOrgHeader(1));
			RateEntry entry1 = testQuote.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "40GP");
			entry1.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 1000m;
			entry1.RateLines[0].TL_WeightVolume = "M3";
			Factory.Save();

			bool alternativeFormat = DocumentsDataRegistry.Instance.AlternativeRateFormat.Value;
			DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			try
			{
				PricingPageCollection pages = new PricingPageCollection(testQuote);
				pages.Load(PricingPaginationStrategy.StandardStyle);

				DocEntryQuotation quotation = DocEntryQuotation.New(pages[0], Factory);
				DocQuotationLineCollection collection = quotation.FreightDocRateLineItems;
				AssertEquals(2, collection.Count);
				AssertEquals("International Freight|||", collection[0].QuotationLine.ToString());
				AssertEquals("40GP|USD|1000.00|per M3", collection[1].QuotationLine.ToString());
			}
			finally
			{
				DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, alternativeFormat);
			}
		}

		public void TestWithContainerClasses()
		{
			Helper.Containers["40GP"].RC_FreightRateClass = "40";
			Helper.Containers["40HC"].RC_FreightRateClass = "40";

			Helper.Containers["20GP"].RC_HandlingRateClass = "GP";
			Helper.Containers["40GP"].RC_HandlingRateClass = "GP";

			var tariff = Factory.New<CompanyTariff>();
			RateEntry tariffEntry1 = tariff.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "40HC");
			tariffEntry1.TI_MatchContainerRateClass = true;
			tariffEntry1.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 900m;
			RateLine tariffLine1b = tariffEntry1.AddRateLine("BAF", PercentageCalculator.Code);
			tariffLine1b.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["FRT"].PK;
			tariffLine1b.GetCalculator<PercentageCalculator>().Percent = 12m;

			RateEntry tariffEntry2 = tariff.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "40GP");
			tariffEntry2.AddRateLine("OPCH", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)200m;

			RateEntry tariffEntry3 = tariff.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "20GP");
			tariffEntry3.TI_MatchContainerRateClass = true;
			tariffEntry3.AddRateLine("ODOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 50m;
			tariffEntry3.AddRateLine("OPCH", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)160m;

			RateEntry tariffEntry4 = tariff.AddRateEntry("DST", "FCL", "", "USLAX", "", "20GP");
			tariffEntry4.TI_MatchContainerRateClass = true;
			tariffEntry4.AddRateLine("DDOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 45m;
			tariffEntry4.AddRateLine("DPCH", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)120m;

			RateEntry tariffEntry5 = tariff.AddRateEntry("DST", "FCL", "", "USLAX", "", "40GP");
			tariffEntry5.AddRateLine("DPCH", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)180m;

			Quote quote = Helper.NewQuote(Helper.NewOrgHeader(1));
			RateEntry entry1 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "40GP");
			entry1.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 1000m;

			Factory.Save();

			PricingPageCollection pages = new PricingPageCollection(quote);
			pages.Load(PricingPaginationStrategy.StandardStyle);
			AssertEquals(1, pages.Count);

			DocEntryQuotation quotation = DocEntryQuotation.New(pages[0], Factory);
			DocQuotationLineCollection collection = quotation.FreightDocRateLineItems;
			AssertEquals(4, collection.Count);
			AssertEquals("International Freight|||", collection[0].QuotationLine.ToString());
			AssertEquals("40GP|USD|1000.00|per Container", collection[1].QuotationLine.ToString());
			AssertEquals("Bunker Adjustment Factor|||", collection[2].QuotationLine.ToString());
			AssertEquals("40HC, 40GP||12.00|% of Freight", collection[3].QuotationLine.ToString());

			collection = quotation.OriginDocRateLineItems;
			AssertEquals(4, collection.Count);
			AssertEquals("Origin Port Charges *|||", collection[0].QuotationLine.ToString());
			AssertEquals("40GP|AUD|200.00|", collection[1].QuotationLine.ToString());
			AssertEquals("Origin Documentation Fee *|||", collection[2].QuotationLine.ToString());
			AssertEquals("20GP, 40GP|AUD|50.00|", collection[3].QuotationLine.ToString());

			collection = quotation.DestinationDocRateLineItems;
			AssertEquals(4, collection.Count);
			AssertEquals("Destination Documentation Fee *|||", collection[0].QuotationLine.ToString());
			AssertEquals("20GP, 40GP|USD|45.00|", collection[1].QuotationLine.ToString());
			AssertEquals("Destination Port Charges *|||", collection[2].QuotationLine.ToString());
			AssertEquals("40GP|USD|180.00|", collection[3].QuotationLine.ToString());
		}

		public void TestTransitTimes()
		{
			Quote testQuote = Helper.NewQuote(Helper.NewOrgHeader(0));

			RateEntry entry1 = testQuote.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			entry1.TI_TransitTime = "10";
			entry1.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 300m;

			RateLine line1b = entry1.AddRateLine("BAF", PercentageCalculator.Code);
			line1b.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["FRT"].PK;
			line1b.GetCalculator<PercentageCalculator>().Percent = 10m;

			RateEntry entry2 = testQuote.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			entry2.TI_TransitTime = "20";
			entry2.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 900m;

			Factory.Save();

			PricingPageCollection pages = new PricingPageCollection(testQuote);
			pages.Load(PricingPaginationStrategy.StandardStyle);
			AssertEquals(2, pages.Count);

			pages.Sort<PricingPage>((p1, p2) => StringComparer.OrdinalIgnoreCase.Compare(p1.RateEntries.FirstOrDefault().TI_TransitTime, p2.RateEntries.FirstOrDefault().TI_TransitTime));

			DocEntryQuotation quotation = DocEntryQuotation.New(pages[0], Factory);
			DocQuotationLineCollection collection = quotation.FreightDocRateLineItems;
			AssertEquals(4, collection.Count);
			AssertEquals("International Freight|||", collection[0].QuotationLine.ToString());
			AssertEquals("20GP|USD|300.00|per Container", collection[1].QuotationLine.ToString());
			AssertEquals("Bunker Adjustment Factor|||", collection[2].QuotationLine.ToString());
			AssertEquals("20GP||10.00|% of Freight", collection[3].QuotationLine.ToString());

			quotation = DocEntryQuotation.New(pages[1], Factory);
			collection = quotation.FreightDocRateLineItems;
			AssertEquals(2, collection.Count);
			AssertEquals("International Freight|||", collection[0].QuotationLine.ToString());
			AssertEquals("20GP|USD|900.00|per Container", collection[1].QuotationLine.ToString());
		}

		public void TestDocRateLineItem()
		{
			Quote quote = Factory.New<Quote>();

			RefContainer gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			RefContainer re20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");
			RefContainer gp40 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			ZQuery filter = new ZQuery();
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, Env.CurrentCompany.PK);
			filter.AddToFilter(AccChargeCodeSchema.AC_Code, "FRT");

			AccChargeCode frt = Factory.LoadTop1<AccChargeCode>(filter);

			RateEntry originEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.SOR, "ALL", "AUBNE", "");
			originEntry1.TI_RC = gp20.PK;
			originEntry1.TI_ContractNumber = "oct1";

			RateEntry originEntry1b = quote.AddRateEntry(RatingConstants.RateCategory.SOR, "ALL", "AUBNE", "");
			originEntry1b.TI_RC = re20.PK;
			originEntry1b.TI_ContractNumber = "oct1";

			RateEntry originEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.SOR, "ALL", "AUBNE", "");
			originEntry2.TI_RC = gp40.PK;
			originEntry2.TI_ContractNumber = "oct2";

			RateEntry freightEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.SCO, "SEA", "AUBNE", "NLAMS");
			freightEntry1.TI_RC = gp20.PK;
			freightEntry1.TI_ContractNumber = "frt1";

			RateEntry freightEntry1b = quote.AddRateEntry(RatingConstants.RateCategory.SCO, "SEA", "AUBNE", "NLAMS");
			freightEntry1b.TI_RC = re20.PK;
			freightEntry1b.TI_ContractNumber = "frt1";

			RateEntry freightEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.SCO, "SEA", "AUBNE", "NLAMS");
			freightEntry2.TI_RC = gp40.PK;
			freightEntry2.TI_ContractNumber = "frt2";

			RateEntry destinationEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.SDE, "ALL", "", "NLAMS");
			destinationEntry1.TI_RC = gp20.PK;
			destinationEntry1.TI_ContractNumber = "dst1";

			RateEntry destinationEntry1b = quote.AddRateEntry(RatingConstants.RateCategory.SDE, "ALL", "", "NLAMS");
			destinationEntry1b.TI_RC = re20.PK;
			destinationEntry1b.TI_ContractNumber = "dst1";

			RateEntry destinationEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.SDE, "ALL", "", "NLAMS");
			destinationEntry2.TI_RC = gp40.PK;
			destinationEntry2.TI_ContractNumber = "dst2";

			AddRateLine(originEntry1, frt, 40, null);
			AddRateLine(originEntry1b, frt, 40, null);
			AddRateLine(originEntry2, frt, 50, null);
			AddRateLine(freightEntry1, frt, 60, null);
			AddRateLine(freightEntry1b, frt, 60, 80);
			AddRateLine(freightEntry2, frt, 70, null);
			AddRateLine(destinationEntry1, frt, 80, null);
			AddRateLine(destinationEntry1b, frt, 80, null);
			AddRateLine(destinationEntry2, frt, 90, null);

			Factory.Save();

			PricingPage page = new PricingPage(freightEntry1, Factory, PricingPageStyle.Standard);
			page.ContainerSet.Add(gp20);
			page.ContainerSet.Add(re20);
			page.ContainerSet.Add(gp40);

			DocEntryQuotation quotation = DocEntryQuotation.New(page, Factory);

			const string expected = @"
[Origin]
International Freight|||
20GP, 20RE|AUD|40.00|per Container
Contract Number: oct1|||
40GP|AUD|50.00|per Container
Contract Number: oct2|||

[Destination]
International Freight|||
20GP, 20RE|EUR|80.00|per Container
Contract Number: dst1|||
40GP|EUR|90.00|per Container
Contract Number: dst2|||

[Freight]
International Freight|||
20GP|USD|60.00|per Container
Contract Number: frt1|||
20RE|||
Minimum|USD|80.00|
Per Unit|USD|60.00|per Container
Contract Number: frt1|||
40GP|USD|70.00|per Container
Contract Number: frt2|||
";

			StringBuilder builder = new StringBuilder();

			builder.AppendLine();
			builder.AppendLine("[Origin]");

			foreach (DocRateLineItem item in quotation.OriginDocRateLineItems)
			{
				builder.AppendLine(item.QuotationLine.ToString());
			}

			builder.AppendLine();
			builder.AppendLine("[Destination]");

			foreach (DocRateLineItem item in quotation.DestinationDocRateLineItems)
			{
				builder.AppendLine(item.QuotationLine.ToString());
			}

			builder.AppendLine();
			builder.AppendLine("[Freight]");

			foreach (DocRateLineItem item in quotation.FreightDocRateLineItems)
			{
				builder.AppendLine(item.QuotationLine.ToString());
			}

			AssertMultilineASCIIEquals("", expected, builder.ToString());
		}

		#region Implementation

		string Render(DocQuotationLineCollection collection)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine();

			foreach (DocRateLineItem item in collection)
			{
				builder.AppendLine(item.QuotationLine.ToString());
			}

			return builder.ToString();
		}

		void AssertQuoteEntry(RateEntry quoteEntry, params string[] chargeCodes)
		{
			DocEntryQuotation quotation = DocEntryQuotation.New(new PricingPage(quoteEntry, Factory, PricingPageStyle.Standard), Factory);

			List<string> actual = new List<string>();

			foreach (DocRateLineItem item in quotation.OriginDocRateLineItems)
			{
				actual.Add(item.QuotationLine.Master.ChargeCode.AC_Code);
			}

			foreach (DocRateLineItem item in quotation.FreightDocRateLineItems)
			{
				actual.Add(item.QuotationLine.Master.ChargeCode.AC_Code);
			}

			foreach (DocRateLineItem item in quotation.DestinationDocRateLineItems)
			{
				actual.Add(item.QuotationLine.Master.ChargeCode.AC_Code);
			}

			AssertContainsExactElementsInAnyOrder(chargeCodes, actual);
		}

		static void AddRateLine(RateEntry entry, AccChargeCode chargeCode, int cost, int? min)
		{
			RateLine line = entry.RateLines.AddNew();
			line.TL_AC = chargeCode.PK;
			line.TL_WeightVolume = RatingConstants.Units.CN;

			if (min.HasValue)
			{
				line.TL_RateCalculator = MinimumOrPerUnitCalculator.Code;
				((MinimumOrPerUnitCalculator)line.Calculator).Minimum = min.Value;
				((MinimumOrPerUnitCalculator)line.Calculator).PerUnit = cost;
			}
			else
			{
				line.TL_RateCalculator = UnitCalculator.Code;
				((UnitCalculator)line.Calculator).PerUnit = cost;
			}
		}

		public OrgHeader SetupOrgHeader(int? companyTariffDefault = null)
		{
			var org = companyTariffDefault == null ? Helper.NewOrgHeader() : Helper.NewOrgHeader(companyTariffDefault.Value);
			org.OH_IsConsignee = true;
			Factory.Save();
			return org;
		}

		TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		protected override void SetUp()
		{
			base.SetUp();

			originalAlternateRateFormatValue = DocumentsDataRegistry.Instance.AlternativeRateFormat.Value;
			DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var gst = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "GST").AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, Core.Constants.CountryCodes.Australia));
			if (gst != null)
			{
				gst.SetRate_ForTestOnly(10, 1);

				Factory.Save();
			}
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
