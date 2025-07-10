using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	sealed class DocAirLCLRateEntryCollectionSplittingTest : RatingTestCase
	{
		public void TestSplitingIntoMatrix2()
		{
			OrgCarrierServiceLevel newLevel = NewClient.MiscServ.CarrierServiceLevels.AddNew();
			newLevel.PL_Code = "EXP";
			newLevel.PL_CarrierServiceLevelDescription = "Export Service Level";

			Factory.Save();

			Costing testCosting = Factory.New<Costing>();
			testCosting.TH_OH = NewClient.PK;

			RateEntry entry = testCosting.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON");
			entry.TI_PL_NKCarrierServiceLevel = "EXP";
			entry.RateLines.RemoveAndDeleteAll();
			entry.TI_OH_Consignee = NewClient2.PK;

			RateEntry entry2 = testCosting.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON");
			entry2.RateLines.RemoveAndDeleteAll();
			entry2.TI_OH_Consignor = Factory.NewWithValidTestData<OrgHeader>().PK;

			var startDate = ZDate.Today;
			var expireDate = ZDate.Today.AddDays(30);

			entry.TI_RateStartDate = startDate;
			entry.TI_RateEndDate = expireDate;
			entry2.TI_RateStartDate = startDate;
			entry2.TI_RateEndDate = expireDate;

			string expectedValidity = startDate.ToString("d") + "-" + expireDate.ToString("d");

			RateLine line1 = entry.AddRateLine(TestBAF.AC_Code, CombinedCalculator.Code, QuantityUnit.KG);
			line1.RateLineItems.RemoveAndDeleteAll();
			((CombinedCalculator)line1.Calculator).Minimum = 50m;
			line1.Calculator.AddRateLineItem("-", 100m, 49m, 0m);
			line1.Calculator.AddRateLineItem("+", 100m, 48m, 0m);

			RateLine line2 = entry.AddRateLine(TestFRT.AC_Code, UnitCalculator.Code, QuantityUnit.KG);
			line2.RateLineItems.RemoveAndDeleteAll();
			((UnitCalculator)line2.Calculator).PerUnit = 15m;

			RateLine line3 = entry.AddRateLine(TestCAF.AC_Code, CombinedCalculator.Code, QuantityUnit.KG);
			line3.RateLineItems.RemoveAndDeleteAll();
			((CombinedCalculator)line3.Calculator).Minimum = 300m;
			line3.Calculator.AddRateLineItem("-", 150m, 39m, 0m);
			line3.Calculator.AddRateLineItem("+", 150m, 38m, 0m);

			RateLine line4 = entry2.AddRateLine(TestCAF.AC_Code, CombinedCalculator.Code, QuantityUnit.KG);
			line4.RateLineItems.RemoveAndDeleteAll();
			((CombinedCalculator)line4.Calculator).Minimum = 300m;
			line4.Calculator["-45"] = (ZDecimal)20m;
			line4.Calculator["+45"] = (ZDecimal)19m;
			line4.Calculator["+100"] = (ZDecimal)18m;
			line4.Calculator["+250"] = (ZDecimal)18m;
			line4.Calculator["+500"] = (ZDecimal)18m;
			line4.Calculator["+1000"] = (ZDecimal)18m;

			PricingPage formatTable = new PricingPage(entry, Factory, PricingPageStyle.Landscape);
			formatTable.AddRateEntry(entry2);
			DocTableQuotation testTabeleQuotation = DocTableQuotation.New(formatTable, Factory);
			Factory.Save();

			AssertEquals("Two rate lines should be meanly hidden!", 6, testTabeleQuotation.Entries.Count);

			AssertEquals("Header1", "Origin", testTabeleQuotation.Entries[0].Header1);
			AssertEquals("Header2", "Destination", testTabeleQuotation.Entries[0].Header2);
			AssertEquals("Header3", "Airline", testTabeleQuotation.Entries[0].Header3);
			AssertEquals("Header4", "Serv. Level", testTabeleQuotation.Entries[0].Header4);
			AssertEquals("Header5", "Comm. Code", testTabeleQuotation.Entries[0].Header5);
			AssertEquals("Header7", "MIN", testTabeleQuotation.Entries[0].GroupLCLHeader4);
			AssertEquals("Header8", "-100 per KG", testTabeleQuotation.Entries[0].GroupLCLHeader5);
			AssertEquals("Header9", "+100 per KG", testTabeleQuotation.Entries[0].GroupLCLHeader6);
			AssertEquals("Header10", "", testTabeleQuotation.Entries[0].GroupLCLHeader7);
			AssertEquals("Header11", "", testTabeleQuotation.Entries[0].GroupLCLHeader8);
			AssertEquals("Header12", "", testTabeleQuotation.Entries[0].GroupLCLHeader9);
			AssertEquals("Header13", "", testTabeleQuotation.Entries[0].Header13);
			AssertEquals("Header14", "W/V conv.", testTabeleQuotation.Entries[0].Header14);
			AssertEquals("Header15", "Validity", testTabeleQuotation.Entries[0].Header15);

			AssertEquals("Charge Code", "Test BAF", testTabeleQuotation.Entries[0].ChargeCode);
			AssertEquals("Column1", "Sydney", testTabeleQuotation.Entries[0].Column1);
			AssertEquals("Column2", "London", testTabeleQuotation.Entries[0].Column2);
			AssertEquals("Column3", "", testTabeleQuotation.Entries[0].Column3);
			AssertEquals("Column4", "Export Service Level", testTabeleQuotation.Entries[0].Column4);
			AssertEquals("Column5", "General", testTabeleQuotation.Entries[0].Column5);
			AssertEquals("Column6", "AUD", testTabeleQuotation.Entries[0].Column6);
			AssertEquals("Column7", "50.00", testTabeleQuotation.Entries[0].Column7);
			AssertEquals("Column8", "49.00", testTabeleQuotation.Entries[0].Column8);
			AssertEquals("Column9", "48.00", testTabeleQuotation.Entries[0].Column9);
			AssertEquals("Column10", "", testTabeleQuotation.Entries[0].Column10);
			AssertEquals("Column11", "", testTabeleQuotation.Entries[0].Column11);
			AssertEquals("Column12", "", testTabeleQuotation.Entries[0].Column12);
			AssertEquals("Column13", "", testTabeleQuotation.Entries[0].Column13);
			AssertEquals("Column14", "6000 CC/KG", testTabeleQuotation.Entries[0].Column14);
			AssertEquals("Column14", expectedValidity, testTabeleQuotation.Entries[0].Column15);
			AssertEquals("GroupIndex", 0, testTabeleQuotation.Entries[0].LCLHeaderGroupIndex);
			Assert("Other Charges", testTabeleQuotation.Entries[0].OtherCharges.IsEmpty);
			Assert("IsOtherChargesEntry", !testTabeleQuotation.Entries[0].IsOtherChargesEntry);

			//--

			AssertEquals("Header1", "Origin", testTabeleQuotation.Entries[1].Header1);
			AssertEquals("Header2", "Destination", testTabeleQuotation.Entries[1].Header2);
			AssertEquals("Header3", "Airline", testTabeleQuotation.Entries[1].Header3);
			AssertEquals("Header4", "Serv. Level", testTabeleQuotation.Entries[1].Header4);
			AssertEquals("Header5", "Comm. Code", testTabeleQuotation.Entries[1].Header5);
			AssertEquals("Header7", "MIN", testTabeleQuotation.Entries[1].GroupLCLHeader4);
			AssertEquals("Header8", "-45 per KG", testTabeleQuotation.Entries[1].GroupLCLHeader5);
			AssertEquals("Header9", "+45 per KG", testTabeleQuotation.Entries[1].GroupLCLHeader6);
			AssertEquals("Header10", "+100 per KG", testTabeleQuotation.Entries[1].GroupLCLHeader7);
			AssertEquals("Header11", "+250 per KG", testTabeleQuotation.Entries[1].GroupLCLHeader8);
			AssertEquals("Header12", "+500 per KG", testTabeleQuotation.Entries[1].GroupLCLHeader9);
			AssertEquals("Header13", "+1000 per KG", testTabeleQuotation.Entries[1].Header13);
			AssertEquals("Header14", "W/V conv.", testTabeleQuotation.Entries[1].Header14);
			AssertEquals("Header15", "Validity", testTabeleQuotation.Entries[1].Header15);

			AssertEquals("Charge Code", "Test CAF", testTabeleQuotation.Entries[1].ChargeCode);
			AssertEquals("Column1", "Sydney", testTabeleQuotation.Entries[1].Column1);
			AssertEquals("Column2", "London", testTabeleQuotation.Entries[1].Column2);
			AssertEquals("Column3", "", testTabeleQuotation.Entries[1].Column3);
			AssertEquals("Column4", "", testTabeleQuotation.Entries[1].Column4);
			AssertEquals("Column5", "General", testTabeleQuotation.Entries[1].Column5);
			AssertEquals("Column6", "AUD", testTabeleQuotation.Entries[1].Column6);
			AssertEquals("Column7", "300.00", testTabeleQuotation.Entries[1].Column7);
			AssertEquals("Column8", "20.00", testTabeleQuotation.Entries[1].Column8);
			AssertEquals("Column9", "19.00", testTabeleQuotation.Entries[1].Column9);
			AssertEquals("Column10", "18.00", testTabeleQuotation.Entries[1].Column10);
			AssertEquals("Column11", "18.00", testTabeleQuotation.Entries[1].Column11);
			AssertEquals("Column12", "18.00", testTabeleQuotation.Entries[1].Column12);
			AssertEquals("Column13", "18.00", testTabeleQuotation.Entries[1].Column13);
			AssertEquals("Column14", "6000 CC/KG", testTabeleQuotation.Entries[1].Column14);
			AssertEquals("Column14", expectedValidity, testTabeleQuotation.Entries[1].Column15);
			AssertEquals("GroupIndex", 2, testTabeleQuotation.Entries[1].LCLHeaderGroupIndex);
			Assert("Other Charges", testTabeleQuotation.Entries[1].OtherCharges.IsEmpty);
			Assert("IsOtherChargesEntry", !testTabeleQuotation.Entries[1].IsOtherChargesEntry);

			//--

			AssertEquals("Header1", "Origin", testTabeleQuotation.Entries[2].Header1);
			AssertEquals("Header2", "Destination", testTabeleQuotation.Entries[2].Header2);
			AssertEquals("Header3", "Airline", testTabeleQuotation.Entries[2].Header3);
			AssertEquals("Header4", "Serv. Level", testTabeleQuotation.Entries[2].Header4);
			AssertEquals("Header5", "Comm. Code", testTabeleQuotation.Entries[2].Header5);
			AssertEquals("Header7", "MIN", testTabeleQuotation.Entries[2].GroupLCLHeader4);
			AssertEquals("Header8", "-100 per KG", testTabeleQuotation.Entries[2].GroupLCLHeader5);
			AssertEquals("Header9", "+100 per KG", testTabeleQuotation.Entries[2].GroupLCLHeader6);
			AssertEquals("Header10", "", testTabeleQuotation.Entries[2].GroupLCLHeader7);
			AssertEquals("Header11", "", testTabeleQuotation.Entries[2].GroupLCLHeader8);
			AssertEquals("Header12", "", testTabeleQuotation.Entries[2].GroupLCLHeader9);
			AssertEquals("Header13", "", testTabeleQuotation.Entries[2].Header13);
			AssertEquals("Header14", "W/V conv.", testTabeleQuotation.Entries[2].Header14);
			AssertEquals("Header15", "Validity", testTabeleQuotation.Entries[2].Header15);

			AssertEquals("Charge Code", "Test Freight", testTabeleQuotation.Entries[2].ChargeCode);
			AssertEquals("Column1", "Sydney", testTabeleQuotation.Entries[2].Column1);
			AssertEquals("Column2", "London", testTabeleQuotation.Entries[2].Column2);
			AssertEquals("Column3", "", testTabeleQuotation.Entries[2].Column3);
			AssertEquals("Column4", "Export Service Level", testTabeleQuotation.Entries[2].Column4);
			AssertEquals("Column5", "General", testTabeleQuotation.Entries[2].Column5);
			AssertEquals("Column6", "AUD", testTabeleQuotation.Entries[2].Column6);
			AssertEquals("Column7", "", testTabeleQuotation.Entries[2].Column7);
			AssertEquals("Column8", "15.00", testTabeleQuotation.Entries[2].Column8);
			AssertEquals("Column9", "15.00", testTabeleQuotation.Entries[2].Column9);
			AssertEquals("Column10", "", testTabeleQuotation.Entries[2].Column10);
			AssertEquals("Column11", "", testTabeleQuotation.Entries[2].Column11);
			AssertEquals("Column12", "", testTabeleQuotation.Entries[2].Column12);
			AssertEquals("Column13", "", testTabeleQuotation.Entries[2].Column13);
			AssertEquals("Column14", "6000 CC/KG", testTabeleQuotation.Entries[2].Column14);
			AssertEquals("Column14", expectedValidity, testTabeleQuotation.Entries[2].Column15);
			AssertEquals("GroupIndex", 0, testTabeleQuotation.Entries[2].LCLHeaderGroupIndex);
			Assert("Other Charges", testTabeleQuotation.Entries[2].OtherCharges.IsEmpty);
			Assert("IsOtherChargesEntry", !testTabeleQuotation.Entries[2].IsOtherChargesEntry);

			//--

			AssertEquals("Header1", "Origin", testTabeleQuotation.Entries[3].Header1);
			AssertEquals("Header2", "Destination", testTabeleQuotation.Entries[3].Header2);
			AssertEquals("Header3", "Airline", testTabeleQuotation.Entries[3].Header3);
			AssertEquals("Header4", "Serv. Level", testTabeleQuotation.Entries[3].Header4);
			AssertEquals("Header5", "Comm. Code", testTabeleQuotation.Entries[3].Header5);
			AssertEquals("Header7", "MIN", testTabeleQuotation.Entries[3].GroupLCLHeader4);
			AssertEquals("Header8", "-150 per KG", testTabeleQuotation.Entries[3].GroupLCLHeader5);
			AssertEquals("Header9", "+150 per KG", testTabeleQuotation.Entries[3].GroupLCLHeader6);
			AssertEquals("Header10", "", testTabeleQuotation.Entries[3].GroupLCLHeader7);
			AssertEquals("Header11", "", testTabeleQuotation.Entries[3].GroupLCLHeader8);
			AssertEquals("Header12", "", testTabeleQuotation.Entries[3].GroupLCLHeader9);
			AssertEquals("Header13", "", testTabeleQuotation.Entries[3].Header13);
			AssertEquals("Header14", "W/V conv.", testTabeleQuotation.Entries[3].Header14);
			AssertEquals("Header15", "Validity", testTabeleQuotation.Entries[3].Header15);

			AssertEquals("Charge Code", "Test CAF", testTabeleQuotation.Entries[3].ChargeCode);
			AssertEquals("Column1", "Sydney", testTabeleQuotation.Entries[3].Column1);
			AssertEquals("Column2", "London", testTabeleQuotation.Entries[3].Column2);
			AssertEquals("Column3", "", testTabeleQuotation.Entries[3].Column3);
			AssertEquals("Column4", "Export Service Level", testTabeleQuotation.Entries[3].Column4);
			AssertEquals("Column5", "General", testTabeleQuotation.Entries[3].Column5);
			AssertEquals("Column6", "AUD", testTabeleQuotation.Entries[3].Column6);
			AssertEquals("Column7", "300.00", testTabeleQuotation.Entries[3].Column7);
			AssertEquals("Column8", "39.00", testTabeleQuotation.Entries[3].Column8);
			AssertEquals("Column9", "38.00", testTabeleQuotation.Entries[3].Column9);
			AssertEquals("Column10", "", testTabeleQuotation.Entries[3].Column10);
			AssertEquals("Column11", "", testTabeleQuotation.Entries[3].Column11);
			AssertEquals("Column12", "", testTabeleQuotation.Entries[3].Column12);
			AssertEquals("Column13", "", testTabeleQuotation.Entries[3].Column13);
			AssertEquals("Column14", "6000 CC/KG", testTabeleQuotation.Entries[3].Column14);
			AssertEquals("Column14", expectedValidity, testTabeleQuotation.Entries[3].Column15);
			AssertEquals("GroupIndex", 1, testTabeleQuotation.Entries[3].LCLHeaderGroupIndex);
			Assert("Other Charges", testTabeleQuotation.Entries[3].OtherCharges.IsEmpty);
			Assert("IsOtherChargesEntry", !testTabeleQuotation.Entries[3].IsOtherChargesEntry);

			//--

			AssertEquals("Header1", "Origin", testTabeleQuotation.Entries[4].Header1);
			AssertEquals("Header2", "Destination", testTabeleQuotation.Entries[4].Header2);
			AssertEquals("Header3", "Airline", testTabeleQuotation.Entries[4].Header3);
			AssertEquals("Header4", "Serv. Level", testTabeleQuotation.Entries[4].Header4);
			AssertEquals("Header5", "Comm. Code", testTabeleQuotation.Entries[4].Header5);
			AssertEquals("Header7", "", testTabeleQuotation.Entries[4].GroupLCLHeader4);
			AssertEquals("Header8", "", testTabeleQuotation.Entries[4].GroupLCLHeader5);
			AssertEquals("Header9", "", testTabeleQuotation.Entries[4].GroupLCLHeader6);
			AssertEquals("Header10", "", testTabeleQuotation.Entries[4].GroupLCLHeader7);
			AssertEquals("Header11", "", testTabeleQuotation.Entries[4].GroupLCLHeader8);
			AssertEquals("Header12", "", testTabeleQuotation.Entries[4].GroupLCLHeader9);
			AssertEquals("Header13", "", testTabeleQuotation.Entries[4].Header13);
			AssertEquals("Header14", "W/V conv.", testTabeleQuotation.Entries[4].Header14);
			AssertEquals("Header15", "Validity", testTabeleQuotation.Entries[4].Header15);

			AssertEquals("Charge Code", "", testTabeleQuotation.Entries[4].ChargeCode);
			AssertEquals("Column1", "Sydney", testTabeleQuotation.Entries[4].Column1);
			AssertEquals("Column2", "London", testTabeleQuotation.Entries[4].Column2);
			AssertEquals("Column3", "", testTabeleQuotation.Entries[4].Column3);
			AssertEquals("Column4", "Export Service Level", testTabeleQuotation.Entries[4].Column4);
			AssertEquals("Column5", "General", testTabeleQuotation.Entries[4].Column5);
			AssertEquals("Column6", "AUD", testTabeleQuotation.Entries[4].Column6);
			AssertEquals("Column7", "", testTabeleQuotation.Entries[4].Column7);
			AssertEquals("Column8", "", testTabeleQuotation.Entries[4].Column8);
			AssertEquals("Column9", "", testTabeleQuotation.Entries[4].Column9);
			AssertEquals("Column10", "", testTabeleQuotation.Entries[4].Column10);
			AssertEquals("Column11", "", testTabeleQuotation.Entries[4].Column11);
			AssertEquals("Column12", "", testTabeleQuotation.Entries[4].Column12);
			AssertEquals("Column13", "", testTabeleQuotation.Entries[4].Column13);
			AssertEquals("Column14", "", testTabeleQuotation.Entries[4].Column14);
			AssertEquals("Column14", expectedValidity, testTabeleQuotation.Entries[4].Column15);
			AssertEquals("GroupIndex", 1, testTabeleQuotation.Entries[4].LCLHeaderGroupIndex);
			AssertEquals("Other Charges", "Consignee: New Test Client 2", testTabeleQuotation.Entries[4].OtherCharges);
			Assert("IsOtherChargesEntry", testTabeleQuotation.Entries[4].IsOtherChargesEntry);

			//--

			AssertEquals("Header1", "Origin", testTabeleQuotation.Entries[5].Header1);
			AssertEquals("Header2", "Destination", testTabeleQuotation.Entries[5].Header2);
			AssertEquals("Header3", "Airline", testTabeleQuotation.Entries[5].Header3);
			AssertEquals("Header4", "Serv. Level", testTabeleQuotation.Entries[5].Header4);
			AssertEquals("Header5", "Comm. Code", testTabeleQuotation.Entries[5].Header5);
			AssertEquals("Header7", "", testTabeleQuotation.Entries[5].GroupLCLHeader4);
			AssertEquals("Header8", "", testTabeleQuotation.Entries[5].GroupLCLHeader5);
			AssertEquals("Header9", "", testTabeleQuotation.Entries[5].GroupLCLHeader6);
			AssertEquals("Header10", "", testTabeleQuotation.Entries[5].GroupLCLHeader7);
			AssertEquals("Header11", "", testTabeleQuotation.Entries[5].GroupLCLHeader8);
			AssertEquals("Header12", "", testTabeleQuotation.Entries[5].GroupLCLHeader9);
			AssertEquals("Header13", "", testTabeleQuotation.Entries[5].Header13);
			AssertEquals("Header14", "W/V conv.", testTabeleQuotation.Entries[5].Header14);
			AssertEquals("Header15", "Validity", testTabeleQuotation.Entries[5].Header15);

			AssertEquals("Charge Code", "", testTabeleQuotation.Entries[5].ChargeCode);
			AssertEquals("Column1", "Sydney", testTabeleQuotation.Entries[5].Column1);
			AssertEquals("Column2", "London", testTabeleQuotation.Entries[5].Column2);
			AssertEquals("Column3", "", testTabeleQuotation.Entries[5].Column3);
			AssertEquals("Column4", "", testTabeleQuotation.Entries[5].Column4);
			AssertEquals("Column5", "General", testTabeleQuotation.Entries[5].Column5);
			AssertEquals("Column6", "AUD", testTabeleQuotation.Entries[5].Column6);
			AssertEquals("Column7", "", testTabeleQuotation.Entries[5].Column7);
			AssertEquals("Column8", "", testTabeleQuotation.Entries[5].Column8);
			AssertEquals("Column9", "", testTabeleQuotation.Entries[5].Column9);
			AssertEquals("Column10", "", testTabeleQuotation.Entries[5].Column10);
			AssertEquals("Column11", "", testTabeleQuotation.Entries[5].Column11);
			AssertEquals("Column12", "", testTabeleQuotation.Entries[5].Column12);
			AssertEquals("Column13", "", testTabeleQuotation.Entries[5].Column13);
			AssertEquals("Column14", "", testTabeleQuotation.Entries[5].Column14);
			AssertEquals("Column14", expectedValidity, testTabeleQuotation.Entries[5].Column15);
			AssertEquals("GroupIndex", 2, testTabeleQuotation.Entries[5].LCLHeaderGroupIndex);
			AssertEquals("Other Charges", "Consignor: ", testTabeleQuotation.Entries[5].OtherCharges);
			Assert("IsOtherChargesEntry", testTabeleQuotation.Entries[5].IsOtherChargesEntry);
		}

		public void TestSplitingIntoMatrix()
		{
			Quote testQuote = Factory.New<Quote>();
			testQuote.TH_OH = NewClient.PK;
			testQuote.TH_QuoteNumber = "0000999";
			testQuote.Header.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			RateEntry entry = testQuote.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON", "STD", "");
			entry.TI_RX_NKCurrency = "EUR";
			entry.RateLines.RemoveAndDeleteAll();
			entry.TI_Frequency = 3;
			entry.TI_FrequencyUnit = RatingConstants.FrequencyUnits.Week;
			entry.TI_PageOpeningText = "Test Opening";
			entry.TI_PageClosingText = "Test Closing";

			RateEntry entry2 = testQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD", "");
			entry2.TI_RX_NKCurrency = "EUR";
			entry2.RateLines.RemoveAndDeleteAll();

			RateEntry entry3 = testQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USNYC", "STD", "");
			entry3.TI_RX_NKCurrency = "USD";
			entry3.RateLines.RemoveAndDeleteAll();

			RateLine line1 = entry.AddRateLine(TestBAF.AC_Code, CombinedCalculator.Code, QuantityUnit.M3);
			line1.RateLineItems.RemoveAndDeleteAll();
			((CombinedCalculator)line1.Calculator).Minimum = 100m;
			line1.Calculator.AddRateLineItem("-", 45m, 5m, 2m);
			line1.Calculator.AddRateLineItem("+", 45m, 4m, 1m);

			RateLine line2 = entry.AddRateLine(TestCAF.AC_Code, HighestRateCalculator.Code);
			line2.RateLineItems.RemoveAndDeleteAll();
			((HighestRateCalculator)line2.Calculator).Minimum = 500m;
			line2.Calculator.AddRateLineItem("UNT", 0m, 3m, 0m).TM_BreakWeightVolume = "KG";

			RateLine line3 = entry.AddRateLine(TestFRT.AC_Code, CombinedCalculator.Code, QuantityUnit.M3);
			line3.RateLineItems.RemoveAndDeleteAll();
			((CombinedCalculator)line3.Calculator).Minimum = 1000m;
			line3.Calculator["-45"] = (ZDecimal)100m;
			line3.Calculator["+45"] = (ZDecimal)99m;
			line3.Calculator["+90"] = (ZDecimal)98m;

			RateLine line4 = entry.AddRateLine(TestLOL.AC_Code, FlatCalculator.Code);
			line4.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)line4.Calculator).BaseRate = 500m;

			RateLine line5 = entry.AddRateLine(TestBBK.AC_Code, FlatPlusPerUnitCalculator.Code, QuantityUnit.KG);
			line5.RateLineItems.RemoveAndDeleteAll();
			((FlatPlusPerUnitCalculator)line5.Calculator).BaseRate = 1000m;
			((FlatPlusPerUnitCalculator)line5.Calculator).PerUnit = 100m;

			RateLine line6 = entry.AddRateLine(TestCTG.AC_Code, UnitCalculator.Code, QuantityUnit.M3);
			line6.RateLineItems.RemoveAndDeleteAll();
			((UnitCalculator)line6.Calculator).PerUnit = 5m;

			RateLine line7 = entry.AddRateLine(TestAWB.AC_Code, PercentageCalculator.Code);
			line7.RateLineItems.RemoveAndDeleteAll();
			((PercentageCalculator)line7.Calculator).AddApplyToItem(CalculatorConstants.Text.AllCharges);
			((PercentageCalculator)line7.Calculator).Percent = 5m;

			RateLine line8 = entry.AddRateLine(TestTHC.AC_Code, UnitCalculator.Code, QuantityUnit.M3);
			line8.RateLineItems.RemoveAndDeleteAll();
			((UnitCalculator)line8.Calculator).PerUnit = 5m;
			TestTHC.AC_ShowOnQuotation = false;

			RateLine line9 = entry2.AddRateLine(TestTHC.AC_Code, UnitCalculator.Code, QuantityUnit.M3);
			line9.RateLineItems.RemoveAndDeleteAll();
			((UnitCalculator)line9.Calculator).PerUnit = 5m;

			RateLine line10 = entry3.AddRateLine(TestAWB.AC_Code, UnitCalculator.Code, QuantityUnit.M3);
			line10.RateLineItems.RemoveAndDeleteAll();
			((UnitCalculator)line10.Calculator).PerUnit = 5m;

			PricingPage formatTable = new PricingPage(entry, Factory, PricingPageStyle.Landscape);
			formatTable.AddRateEntry(entry2);
			formatTable.AddRateEntry(entry3);
			DocTableQuotation testTabeleQuotation = DocTableQuotation.New(formatTable, Factory);
			Factory.Save();

			AssertEquals("Two rate lines should be meanly hidden!", 9, testTabeleQuotation.Entries.Count);

			AssertEquals("Header1", "Origin - Destination", testTabeleQuotation.Entries[0].Header1);
			AssertEquals("Header2", "Airline", testTabeleQuotation.Entries[0].Header2);
			AssertEquals("Header3", "", testTabeleQuotation.Entries[0].Header3);
			AssertEquals("Header4", "MIN", testTabeleQuotation.Entries[0].GroupLCLHeader4);
			AssertEquals("Header5", "-45 per M3", testTabeleQuotation.Entries[0].GroupLCLHeader5);
			AssertEquals("Header6", "+45 per M3", testTabeleQuotation.Entries[0].GroupLCLHeader6);
			AssertEquals("Header7", "+90 per M3", testTabeleQuotation.Entries[0].GroupLCLHeader7);
			AssertEquals("Header8", "", testTabeleQuotation.Entries[0].GroupLCLHeader8);
			AssertEquals("Header9", "", testTabeleQuotation.Entries[0].GroupLCLHeader9);
			AssertEquals("Header10", "", testTabeleQuotation.Entries[0].GroupLCLHeader10);
			AssertEquals("Header11", "", testTabeleQuotation.Entries[0].GroupLCLHeader11);
			AssertEquals("Header12", "W/V conv.", testTabeleQuotation.Entries[0].Header12);
			AssertEquals("Header13", "Transit Time", testTabeleQuotation.Entries[0].Header13);
			AssertEquals("Header14", "Freq.", testTabeleQuotation.Entries[0].Header14);

			AssertEquals("Charge Code", "Test BAF", testTabeleQuotation.Entries[0].ChargeCode);
			AssertEquals("Column1", "Sydney - London", testTabeleQuotation.Entries[0].Column1);
			AssertEquals("Column2", "", testTabeleQuotation.Entries[0].Column2);
			AssertEquals("Column3", "EUR", testTabeleQuotation.Entries[0].Column3);
			AssertEquals("Column4", "100.00", testTabeleQuotation.Entries[0].Column4);
			AssertEquals("Column5", "5.00\r\n2.00", testTabeleQuotation.Entries[0].Column5);
			AssertEquals("Column6", "4.00\r\n1.00", testTabeleQuotation.Entries[0].Column6);
			AssertEquals("Column7", "", testTabeleQuotation.Entries[0].Column7);
			AssertEquals("Column8", "", testTabeleQuotation.Entries[0].Column8);
			AssertEquals("Column9", "", testTabeleQuotation.Entries[0].Column9);
			AssertEquals("Column10", "", testTabeleQuotation.Entries[0].Column10);
			AssertEquals("Column11", "", testTabeleQuotation.Entries[0].Column11);
			AssertEquals("Column12", "6000 CC/KG", testTabeleQuotation.Entries[0].Column12);
			AssertEquals("Column13", "", testTabeleQuotation.Entries[0].Column13);
			AssertEquals("Column14", "3 per Week", testTabeleQuotation.Entries[0].Column14);
			AssertEquals("GroupIndex", 0, testTabeleQuotation.Entries[0].LCLHeaderGroupIndex);
			Assert("Other Charges", testTabeleQuotation.Entries[0].OtherCharges.IsEmpty);
			Assert("IsOtherChargesEntry", !testTabeleQuotation.Entries[0].IsOtherChargesEntry);

			//--

			AssertEquals("Header1", "Origin - Destination", testTabeleQuotation.Entries[1].Header1);
			AssertEquals("Header2", "Airline", testTabeleQuotation.Entries[1].Header2);
			AssertEquals("Header3", "", testTabeleQuotation.Entries[1].Header3);
			AssertEquals("Header4", "per M3", testTabeleQuotation.Entries[1].GroupLCLHeader4);
			AssertEquals("Header5", "", testTabeleQuotation.Entries[1].GroupLCLHeader5);
			AssertEquals("Header6", "", testTabeleQuotation.Entries[1].GroupLCLHeader6);
			AssertEquals("Header7", "", testTabeleQuotation.Entries[1].GroupLCLHeader7);
			AssertEquals("Header8", "", testTabeleQuotation.Entries[1].GroupLCLHeader8);
			AssertEquals("Header9", "", testTabeleQuotation.Entries[1].GroupLCLHeader9);
			AssertEquals("Header10", "", testTabeleQuotation.Entries[1].GroupLCLHeader10);
			AssertEquals("Header11", "", testTabeleQuotation.Entries[1].GroupLCLHeader11);
			AssertEquals("Header12", "W/V conv.", testTabeleQuotation.Entries[1].Header12);
			AssertEquals("Header13", "Transit Time", testTabeleQuotation.Entries[1].Header13);
			AssertEquals("Header14", "Freq.", testTabeleQuotation.Entries[1].Header14);

			AssertEquals("Charge Code", "Test Airway Bill Fee", testTabeleQuotation.Entries[1].ChargeCode);
			AssertEquals("Column1", "Sydney - New York", testTabeleQuotation.Entries[1].Column1);
			AssertEquals("Column2", "", testTabeleQuotation.Entries[1].Column2);
			AssertEquals("Column3", "USD", testTabeleQuotation.Entries[1].Column3);
			AssertEquals("Column4", "5.00", testTabeleQuotation.Entries[1].Column4);
			AssertEquals("Column5", "", testTabeleQuotation.Entries[1].Column5);
			AssertEquals("Column6", "", testTabeleQuotation.Entries[1].Column6);
			AssertEquals("Column7", "", testTabeleQuotation.Entries[1].Column7);
			AssertEquals("Column8", "", testTabeleQuotation.Entries[1].Column8);
			AssertEquals("Column9", "", testTabeleQuotation.Entries[1].Column9);
			AssertEquals("Column10", "", testTabeleQuotation.Entries[1].Column10);
			AssertEquals("Column11", "", testTabeleQuotation.Entries[1].Column11);
			AssertEquals("Column12", "6000 CC/KG", testTabeleQuotation.Entries[1].Column12);
			AssertEquals("Column13", "", testTabeleQuotation.Entries[1].Column13);
			AssertEquals("Column14", "", testTabeleQuotation.Entries[1].Column14);
			AssertEquals("GroupIndex", 3, testTabeleQuotation.Entries[1].LCLHeaderGroupIndex);
			Assert("Other Charges", testTabeleQuotation.Entries[1].OtherCharges.IsEmpty);
			Assert("IsOtherChargesEntry", !testTabeleQuotation.Entries[1].IsOtherChargesEntry);

			//--

			AssertEquals("Header1", "Origin - Destination", testTabeleQuotation.Entries[2].Header1);
			AssertEquals("Header2", "Airline", testTabeleQuotation.Entries[2].Header2);
			AssertEquals("Header3", "", testTabeleQuotation.Entries[2].Header3);
			AssertEquals("Header4", "MIN", testTabeleQuotation.Entries[2].GroupLCLHeader4);
			AssertEquals("Header5", "-45 per M3", testTabeleQuotation.Entries[2].GroupLCLHeader5);
			AssertEquals("Header6", "+45 per M3", testTabeleQuotation.Entries[2].GroupLCLHeader6);
			AssertEquals("Header7", "+90 per M3", testTabeleQuotation.Entries[2].GroupLCLHeader7);
			AssertEquals("Header8", "", testTabeleQuotation.Entries[2].GroupLCLHeader8);
			AssertEquals("Header9", "", testTabeleQuotation.Entries[2].GroupLCLHeader9);
			AssertEquals("Header10", "", testTabeleQuotation.Entries[2].GroupLCLHeader10);
			AssertEquals("Header11", "", testTabeleQuotation.Entries[2].GroupLCLHeader11);
			AssertEquals("Header12", "W/V conv.", testTabeleQuotation.Entries[2].Header12);
			AssertEquals("Header13", "Transit Time", testTabeleQuotation.Entries[2].Header13);
			AssertEquals("Header14", "Freq.", testTabeleQuotation.Entries[2].Header14);

			AssertEquals("Charge Code", "Test Freight", testTabeleQuotation.Entries[2].ChargeCode);
			AssertEquals("Column1", "Sydney - London", testTabeleQuotation.Entries[2].Column1);
			AssertEquals("Column2", "", testTabeleQuotation.Entries[2].Column2);
			AssertEquals("Column3", "EUR", testTabeleQuotation.Entries[2].Column3);
			AssertEquals("Column4", "1000.00", testTabeleQuotation.Entries[2].Column4);
			AssertEquals("Column5", "100.00", testTabeleQuotation.Entries[2].Column5);
			AssertEquals("Column6", "99.00", testTabeleQuotation.Entries[2].Column6);
			AssertEquals("Column7", "98.00", testTabeleQuotation.Entries[2].Column7);
			AssertEquals("Column8", "", testTabeleQuotation.Entries[2].Column8);
			AssertEquals("Column9", "", testTabeleQuotation.Entries[2].Column9);
			AssertEquals("Column10", "", testTabeleQuotation.Entries[2].Column10);
			AssertEquals("Column11", "", testTabeleQuotation.Entries[2].Column11);
			AssertEquals("Column12", "6000 CC/KG", testTabeleQuotation.Entries[2].Column12);
			AssertEquals("Column13", "", testTabeleQuotation.Entries[2].Column13);
			AssertEquals("Column14", "3 per Week", testTabeleQuotation.Entries[2].Column14);
			AssertEquals("GroupIndex", 0, testTabeleQuotation.Entries[2].LCLHeaderGroupIndex);
			Assert("Other Charges", testTabeleQuotation.Entries[2].OtherCharges.IsEmpty);
			Assert("IsOtherChargesEntry", !testTabeleQuotation.Entries[2].IsOtherChargesEntry);

			//--

			AssertEquals("Header1", "Origin - Destination", testTabeleQuotation.Entries[3].Header1);
			AssertEquals("Header2", "Airline", testTabeleQuotation.Entries[3].Header2);
			AssertEquals("Header3", "", testTabeleQuotation.Entries[3].Header3);
			AssertEquals("Header4", "MIN", testTabeleQuotation.Entries[3].GroupLCLHeader4);
			AssertEquals("Header5", "-45 per M3", testTabeleQuotation.Entries[3].GroupLCLHeader5);
			AssertEquals("Header6", "+45 per M3", testTabeleQuotation.Entries[3].GroupLCLHeader6);
			AssertEquals("Header7", "+90 per M3", testTabeleQuotation.Entries[3].GroupLCLHeader7);
			AssertEquals("Header8", "", testTabeleQuotation.Entries[3].GroupLCLHeader8);
			AssertEquals("Header9", "", testTabeleQuotation.Entries[3].GroupLCLHeader9);
			AssertEquals("Header10", "", testTabeleQuotation.Entries[3].GroupLCLHeader10);
			AssertEquals("Header11", "", testTabeleQuotation.Entries[3].GroupLCLHeader11);
			AssertEquals("Header12", "W/V conv.", testTabeleQuotation.Entries[3].Header12);
			AssertEquals("Header13", "Transit Time", testTabeleQuotation.Entries[3].Header13);
			AssertEquals("Header14", "Freq.", testTabeleQuotation.Entries[3].Header14);

			AssertEquals("Charge Code", "Test Cartage Charge Code", testTabeleQuotation.Entries[3].ChargeCode);
			AssertEquals("Column1", "Sydney - London", testTabeleQuotation.Entries[3].Column1);
			AssertEquals("Column2", "", testTabeleQuotation.Entries[3].Column2);
			AssertEquals("Column3", "EUR", testTabeleQuotation.Entries[3].Column3);
			AssertEquals("Column4", "", testTabeleQuotation.Entries[3].Column4);
			AssertEquals("Column5", "5.00", testTabeleQuotation.Entries[3].Column5);
			AssertEquals("Column6", "5.00", testTabeleQuotation.Entries[3].Column6);
			AssertEquals("Column7", "5.00", testTabeleQuotation.Entries[3].Column7);
			AssertEquals("Column8", "", testTabeleQuotation.Entries[3].Column8);
			AssertEquals("Column9", "", testTabeleQuotation.Entries[3].Column9);
			AssertEquals("Column10", "", testTabeleQuotation.Entries[3].Column10);
			AssertEquals("Column11", "", testTabeleQuotation.Entries[3].Column11);
			AssertEquals("Column12", "6000 CC/KG", testTabeleQuotation.Entries[3].Column12);
			AssertEquals("Column13", "", testTabeleQuotation.Entries[3].Column13);
			AssertEquals("Column14", "3 per Week", testTabeleQuotation.Entries[3].Column14);
			AssertEquals("GroupIndex", 0, testTabeleQuotation.Entries[3].LCLHeaderGroupIndex);
			Assert("Other Charges", testTabeleQuotation.Entries[3].OtherCharges.IsEmpty);
			Assert("IsOtherChargesEntry", !testTabeleQuotation.Entries[3].IsOtherChargesEntry);

			//--

			AssertEquals("Header1", "Origin - Destination", testTabeleQuotation.Entries[4].Header1);
			AssertEquals("Header2", "Airline", testTabeleQuotation.Entries[4].Header2);
			AssertEquals("Header3", "", testTabeleQuotation.Entries[4].Header3);
			AssertEquals("Header4", "BAS", testTabeleQuotation.Entries[4].GroupLCLHeader4);
			AssertEquals("Header5", "per KG", testTabeleQuotation.Entries[4].GroupLCLHeader5);
			AssertEquals("Header6", "", testTabeleQuotation.Entries[4].GroupLCLHeader6);
			AssertEquals("Header7", "", testTabeleQuotation.Entries[4].GroupLCLHeader7);
			AssertEquals("Header8", "", testTabeleQuotation.Entries[4].GroupLCLHeader8);
			AssertEquals("Header9", "", testTabeleQuotation.Entries[4].GroupLCLHeader9);
			AssertEquals("Header10", "", testTabeleQuotation.Entries[4].GroupLCLHeader10);
			AssertEquals("Header11", "", testTabeleQuotation.Entries[4].GroupLCLHeader11);
			AssertEquals("Header12", "W/V conv.", testTabeleQuotation.Entries[4].Header12);
			AssertEquals("Header13", "Transit Time", testTabeleQuotation.Entries[4].Header13);
			AssertEquals("Header14", "Freq.", testTabeleQuotation.Entries[4].Header14);

			AssertEquals("Charge Code", "Test Lift On/Lift Off", testTabeleQuotation.Entries[4].ChargeCode);
			AssertEquals("Column1", "Sydney - London", testTabeleQuotation.Entries[4].Column1);
			AssertEquals("Column2", "", testTabeleQuotation.Entries[4].Column2);
			AssertEquals("Column3", "EUR", testTabeleQuotation.Entries[4].Column3);
			AssertEquals("Column4", "500.00", testTabeleQuotation.Entries[4].Column4);
			AssertEquals("Column5", "", testTabeleQuotation.Entries[4].Column5);
			AssertEquals("Column6", "", testTabeleQuotation.Entries[4].Column6);
			AssertEquals("Column7", "", testTabeleQuotation.Entries[4].Column7);
			AssertEquals("Column8", "", testTabeleQuotation.Entries[4].Column8);
			AssertEquals("Column9", "", testTabeleQuotation.Entries[4].Column9);
			AssertEquals("Column10", "", testTabeleQuotation.Entries[4].Column10);
			AssertEquals("Column11", "", testTabeleQuotation.Entries[4].Column11);
			AssertEquals("Column12", "", testTabeleQuotation.Entries[4].Column12);
			AssertEquals("Column13", "", testTabeleQuotation.Entries[4].Column13);
			AssertEquals("Column14", "3 per Week", testTabeleQuotation.Entries[4].Column14);
			AssertEquals("GroupIndex", 1, testTabeleQuotation.Entries[4].LCLHeaderGroupIndex);
			Assert("Other Charges", testTabeleQuotation.Entries[4].OtherCharges.IsEmpty);
			Assert("IsOtherChargesEntry", !testTabeleQuotation.Entries[4].IsOtherChargesEntry);

			//--

			AssertEquals("Header1", "Origin - Destination", testTabeleQuotation.Entries[5].Header1);
			AssertEquals("Header2", "Airline", testTabeleQuotation.Entries[5].Header2);
			AssertEquals("Header3", "", testTabeleQuotation.Entries[5].Header3);
			AssertEquals("Header4", "BAS", testTabeleQuotation.Entries[5].GroupLCLHeader4);
			AssertEquals("Header5", "per KG", testTabeleQuotation.Entries[5].GroupLCLHeader5);
			AssertEquals("Header6", "", testTabeleQuotation.Entries[5].GroupLCLHeader6);
			AssertEquals("Header7", "", testTabeleQuotation.Entries[5].GroupLCLHeader7);
			AssertEquals("Header8", "", testTabeleQuotation.Entries[5].GroupLCLHeader8);
			AssertEquals("Header9", "", testTabeleQuotation.Entries[5].GroupLCLHeader9);
			AssertEquals("Header10", "", testTabeleQuotation.Entries[5].GroupLCLHeader10);
			AssertEquals("Header11", "", testTabeleQuotation.Entries[5].GroupLCLHeader11);
			AssertEquals("Header12", "W/V conv.", testTabeleQuotation.Entries[5].Header12);
			AssertEquals("Header13", "Transit Time", testTabeleQuotation.Entries[5].Header13);
			AssertEquals("Header14", "Freq.", testTabeleQuotation.Entries[5].Header14);

			AssertEquals("Charge Code", "Test Breakbulk", testTabeleQuotation.Entries[5].ChargeCode);
			AssertEquals("Column1", "Sydney - London", testTabeleQuotation.Entries[5].Column1);
			AssertEquals("Column2", "", testTabeleQuotation.Entries[5].Column2);
			AssertEquals("Column3", "EUR", testTabeleQuotation.Entries[5].Column3);
			AssertEquals("Column4", "1000.00", testTabeleQuotation.Entries[5].Column4);
			AssertEquals("Column5", "100.00", testTabeleQuotation.Entries[5].Column5);
			AssertEquals("Column6", "", testTabeleQuotation.Entries[5].Column6);
			AssertEquals("Column7", "", testTabeleQuotation.Entries[5].Column7);
			AssertEquals("Column8", "", testTabeleQuotation.Entries[5].Column8);
			AssertEquals("Column9", "", testTabeleQuotation.Entries[5].Column9);
			AssertEquals("Column10", "", testTabeleQuotation.Entries[5].Column10);
			AssertEquals("Column11", "", testTabeleQuotation.Entries[5].Column11);
			AssertEquals("Column12", "6000 CC/KG", testTabeleQuotation.Entries[5].Column12);
			AssertEquals("Column13", "", testTabeleQuotation.Entries[5].Column13);
			AssertEquals("Column14", "3 per Week", testTabeleQuotation.Entries[5].Column14);
			AssertEquals("GroupIndex", 1, testTabeleQuotation.Entries[5].LCLHeaderGroupIndex);
			Assert("Other Charges", testTabeleQuotation.Entries[5].OtherCharges.IsEmpty);
			Assert("IsOtherChargesEntry", !testTabeleQuotation.Entries[5].IsOtherChargesEntry);

			//--

			AssertEquals("Header1", "Origin - Destination", testTabeleQuotation.Entries[6].Header1);
			AssertEquals("Header2", "Airline", testTabeleQuotation.Entries[6].Header2);
			AssertEquals("Header3", "", testTabeleQuotation.Entries[6].Header3);
			AssertEquals("Header4", "MIN", testTabeleQuotation.Entries[6].GroupLCLHeader4);
			AssertEquals("Header5", "per KG", testTabeleQuotation.Entries[6].GroupLCLHeader5);
			AssertEquals("Header6", "", testTabeleQuotation.Entries[6].GroupLCLHeader6);
			AssertEquals("Header7", "", testTabeleQuotation.Entries[6].GroupLCLHeader7);
			AssertEquals("Header8", "", testTabeleQuotation.Entries[6].GroupLCLHeader8);
			AssertEquals("Header9", "", testTabeleQuotation.Entries[6].GroupLCLHeader9);
			AssertEquals("Header10", "", testTabeleQuotation.Entries[6].GroupLCLHeader10);
			AssertEquals("Header11", "", testTabeleQuotation.Entries[6].GroupLCLHeader11);
			AssertEquals("Header12", "W/V conv.", testTabeleQuotation.Entries[6].Header12);
			AssertEquals("Header13", "Transit Time", testTabeleQuotation.Entries[6].Header13);
			AssertEquals("Header14", "Freq.", testTabeleQuotation.Entries[6].Header14);

			AssertEquals("Charge Code", "Test CAF", testTabeleQuotation.Entries[6].ChargeCode);
			AssertEquals("Column1", "Sydney - London", testTabeleQuotation.Entries[6].Column1);
			AssertEquals("Column2", "", testTabeleQuotation.Entries[6].Column2);
			AssertEquals("Column3", "EUR", testTabeleQuotation.Entries[6].Column3);
			AssertEquals("Column4", "500.00", testTabeleQuotation.Entries[6].Column4);
			AssertEquals("Column5", "3.00", testTabeleQuotation.Entries[6].Column5);
			AssertEquals("Column6", "", testTabeleQuotation.Entries[6].Column6);
			AssertEquals("Column7", "", testTabeleQuotation.Entries[6].Column7);
			AssertEquals("Column8", "", testTabeleQuotation.Entries[6].Column8);
			AssertEquals("Column9", "", testTabeleQuotation.Entries[6].Column9);
			AssertEquals("Column10", "", testTabeleQuotation.Entries[6].Column10);
			AssertEquals("Column11", "", testTabeleQuotation.Entries[6].Column11);
			AssertEquals("Column12", "", testTabeleQuotation.Entries[6].Column12);
			AssertEquals("Column13", "", testTabeleQuotation.Entries[6].Column13);
			AssertEquals("Column14", "3 per Week", testTabeleQuotation.Entries[6].Column14);
			AssertEquals("GroupIndex", 2, testTabeleQuotation.Entries[6].LCLHeaderGroupIndex);
			Assert("Other Charges", testTabeleQuotation.Entries[6].OtherCharges.IsEmpty);
			Assert("IsOtherChargesEntry", !testTabeleQuotation.Entries[6].IsOtherChargesEntry);

			//--

			AssertEquals("Header1", "Origin - Destination", testTabeleQuotation.Entries[7].Header1);
			AssertEquals("Header2", "Airline", testTabeleQuotation.Entries[7].Header2);
			AssertEquals("Header3", "", testTabeleQuotation.Entries[7].Header3);
			AssertEquals("Header4", "", testTabeleQuotation.Entries[7].GroupLCLHeader4);
			AssertEquals("Header5", "", testTabeleQuotation.Entries[7].GroupLCLHeader5);
			AssertEquals("Header6", "", testTabeleQuotation.Entries[7].GroupLCLHeader6);
			AssertEquals("Header7", "", testTabeleQuotation.Entries[7].GroupLCLHeader7);
			AssertEquals("Header8", "", testTabeleQuotation.Entries[7].GroupLCLHeader8);
			AssertEquals("Header9", "", testTabeleQuotation.Entries[7].GroupLCLHeader9);
			AssertEquals("Header10", "", testTabeleQuotation.Entries[7].GroupLCLHeader10);
			AssertEquals("Header11", "", testTabeleQuotation.Entries[7].GroupLCLHeader11);
			AssertEquals("Header12", "W/V conv.", testTabeleQuotation.Entries[7].Header12);
			AssertEquals("Header13", "Transit Time", testTabeleQuotation.Entries[7].Header13);
			AssertEquals("Header14", "Freq.", testTabeleQuotation.Entries[7].Header14);

			Assert("Charge Code", testTabeleQuotation.Entries[7].ChargeCode.IsEmpty);
			AssertEquals("Column1", "Sydney - London", testTabeleQuotation.Entries[7].Column1);
			AssertEquals("Column2", "", testTabeleQuotation.Entries[7].Column2);
			AssertEquals("Column3", "EUR", testTabeleQuotation.Entries[7].Column3);
			AssertEquals("Column4", "", testTabeleQuotation.Entries[7].Column4);
			AssertEquals("Column5", "", testTabeleQuotation.Entries[7].Column5);
			AssertEquals("Column6", "", testTabeleQuotation.Entries[7].Column6);
			AssertEquals("Column7", "", testTabeleQuotation.Entries[7].Column7);
			AssertEquals("Column8", "", testTabeleQuotation.Entries[7].Column8);
			AssertEquals("Column9", "", testTabeleQuotation.Entries[7].Column9);
			AssertEquals("Column10", "", testTabeleQuotation.Entries[7].Column10);
			AssertEquals("Column11", "", testTabeleQuotation.Entries[7].Column11);
			AssertEquals("Column12", "", testTabeleQuotation.Entries[7].Column12);
			AssertEquals("Column13", "", testTabeleQuotation.Entries[7].Column13);
			AssertEquals("Column14", "3 per Week", testTabeleQuotation.Entries[7].Column14);
			AssertEquals("GroupIndex", 2, testTabeleQuotation.Entries[7].LCLHeaderGroupIndex);
			AssertEquals("Other Charges", "Test Airway Bill Fee: 5.00 % of all charges", testTabeleQuotation.Entries[7].OtherCharges);
			Assert("IsOtherChargesEntry", testTabeleQuotation.Entries[7].IsOtherChargesEntry);

			//--

			AssertEquals("Header1", "Origin - Destination", testTabeleQuotation.Entries[8].Header1);
			AssertEquals("Header2", "Airline", testTabeleQuotation.Entries[8].Header2);
			AssertEquals("Header3", "", testTabeleQuotation.Entries[8].Header3);
			AssertEquals("Header4", "", testTabeleQuotation.Entries[8].GroupLCLHeader4);
			AssertEquals("Header5", "", testTabeleQuotation.Entries[8].GroupLCLHeader5);
			AssertEquals("Header6", "", testTabeleQuotation.Entries[8].GroupLCLHeader6);
			AssertEquals("Header7", "", testTabeleQuotation.Entries[8].GroupLCLHeader7);
			AssertEquals("Header8", "", testTabeleQuotation.Entries[8].GroupLCLHeader8);
			AssertEquals("Header9", "", testTabeleQuotation.Entries[8].GroupLCLHeader9);
			AssertEquals("Header10", "", testTabeleQuotation.Entries[8].GroupLCLHeader10);
			AssertEquals("Header11", "", testTabeleQuotation.Entries[8].GroupLCLHeader11);
			AssertEquals("Header12", "W/V conv.", testTabeleQuotation.Entries[8].Header12);
			AssertEquals("Header13", "Transit Time", testTabeleQuotation.Entries[8].Header13);
			AssertEquals("Header14", "Freq.", testTabeleQuotation.Entries[8].Header14);

			Assert("Charge Code", testTabeleQuotation.Entries[8].ChargeCode.IsEmpty);
			AssertEquals("Column1", "Sydney - New York", testTabeleQuotation.Entries[8].Column1);
			AssertEquals("Column2", "", testTabeleQuotation.Entries[8].Column2);
			AssertEquals("Column3", "USD", testTabeleQuotation.Entries[8].Column3);
			AssertEquals("Column4", "", testTabeleQuotation.Entries[8].Column4);
			AssertEquals("Column5", "", testTabeleQuotation.Entries[8].Column5);
			AssertEquals("Column6", "", testTabeleQuotation.Entries[8].Column6);
			AssertEquals("Column7", "", testTabeleQuotation.Entries[8].Column7);
			AssertEquals("Column8", "", testTabeleQuotation.Entries[8].Column8);
			AssertEquals("Column9", "", testTabeleQuotation.Entries[8].Column9);
			AssertEquals("Column10", "", testTabeleQuotation.Entries[8].Column10);
			AssertEquals("Column11", "", testTabeleQuotation.Entries[8].Column11);
			AssertEquals("Column12", "", testTabeleQuotation.Entries[8].Column12);
			AssertEquals("Column13", "", testTabeleQuotation.Entries[8].Column13);
			AssertEquals("Column14", "", testTabeleQuotation.Entries[8].Column14);
			AssertEquals("GroupIndex", 3, testTabeleQuotation.Entries[8].LCLHeaderGroupIndex);
			AssertEquals("Other Charges", "", testTabeleQuotation.Entries[8].OtherCharges);
			Assert("IsOtherChargesEntry", testTabeleQuotation.Entries[8].IsOtherChargesEntry);
		}

		public void TestWithCostingsTariffsRatesAndQuotes()
		{
			var costing = Factory.NewWithValidTestData<Costing>();
			costing.TH_QuoteNumber = "";
			costing.TH_OH = NewClient2.PK;

			var costEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON");
			costEntry.TI_RateStartDate = ZDate.Today.AddMonths(-6);
			costEntry.RateLines.RemoveAndDeleteAll();

			var costLine1 = costEntry.AddRateLine(TestFRT.AC_Code, FlatCalculator.Code);
			costLine1.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)costLine1.Calculator).BaseRate = 10000m;

			var costLine2 = costEntry.AddRateLine(TestBAF.AC_Code, FlatCalculator.Code);
			costLine2.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)costLine2.Calculator).BaseRate = 9999m;

			var costLine3 = costEntry.AddRateLine(TestCAF.AC_Code, FlatCalculator.Code);
			costLine3.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)costLine3.Calculator).BaseRate = 9998m;

			var costLine4 = costEntry.AddRateLine(TestLOL.AC_Code, FlatCalculator.Code);
			costLine4.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)costLine4.Calculator).BaseRate = 9997m;

			var tariff = Factory.NewWithValidTestData<CompanyTariff>();
			NewClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			tariff.TH_QuoteNumber = "";

			var tariffEntry = tariff.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON");
			tariffEntry.TI_RateStartDate = ZDate.Today.AddMonths(-6);
			tariffEntry.RateLines.RemoveAndDeleteAll();

			var tariffLine1 = tariffEntry.AddRateLine(TestBAF.AC_Code, FlatCalculator.Code);
			tariffLine1.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)tariffLine1.Calculator).BaseRate = 1000m;

			var tariffLine2 = tariffEntry.AddRateLine(TestCAF.AC_Code, FlatCalculator.Code);
			tariffLine2.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)tariffLine2.Calculator).BaseRate = 999m;

			var tariffLine3 = tariffEntry.AddRateLine(TestLOL.AC_Code, FlatCalculator.Code);
			tariffLine3.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)tariffLine3.Calculator).BaseRate = 998m;

			var rate = Factory.NewWithValidTestData<ClientRate>();
			rate.TH_OH = NewClient.PK;
			rate.TH_QuoteNumber = "";

			var rateEntry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON");
			rateEntry.TI_RateStartDate = ZDate.Today.AddMonths(-6);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine(TestCAF.AC_Code, FlatCalculator.Code);
			rateLine1.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)rateLine1.Calculator).BaseRate = 100m;

			var rateLine2 = rateEntry.AddRateLine(TestLOL.AC_Code, FlatCalculator.Code);
			rateLine2.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)rateLine2.Calculator).BaseRate = 99m;

			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OH = NewClient.PK;
			quote.TH_QuoteDate = ZDate.Today.AddMonths(-6);

			var quoteEntry = quote.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON");
			quoteEntry.TI_RateStartDate = ZDate.Today.AddMonths(-6);
			quoteEntry.RateLines.RemoveAndDeleteAll();

			var quoteLine1 = quoteEntry.AddRateLine(TestLOL.AC_Code, FlatCalculator.Code);
			quoteLine1.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)quoteLine1.Calculator).BaseRate = 10m;

			var formatTable = new PricingPage(costEntry, Factory, PricingPageStyle.Landscape);
			var testTabeleQuotation = DocTableQuotation.New(formatTable, Factory);
			Factory.Save();

			var entries = testTabeleQuotation.Entries.Cast<DocRateEntry>().ToArray();
			AssertEquals(5, entries.Length);
			AssertEquals("10000.00", entries[0].Column7);
			AssertEquals("Test Freight", entries[0].ChargeCode);
			AssertEquals("9999.00", entries[1].Column7);
			AssertEquals("Test BAF", entries[1].ChargeCode);
			AssertEquals("9998.00", entries[2].Column7);
			AssertEquals("Test CAF", entries[2].ChargeCode);
			AssertEquals("9997.00", entries[3].Column7);
			AssertEquals("Test Lift On/Lift Off", entries[3].ChargeCode);
			AssertEquals("", entries[4].Column7);
			AssertEquals("", entries[4].ChargeCode);
			Assert("IsOtherChargesEntry", entries[4].IsOtherChargesEntry);

			formatTable = new PricingPage(tariffEntry, Factory, PricingPageStyle.Landscape);
			testTabeleQuotation = DocTableQuotation.New(formatTable, Factory);
			entries = testTabeleQuotation.Entries.Cast<DocRateEntry>().ToArray();
			Factory.Save();

			AssertEquals(4, entries.Length);
			AssertEquals("1000.00", entries[0].Column7);
			AssertEquals("Test BAF", entries[0].ChargeCode);
			AssertEquals("999.00", entries[1].Column7);
			AssertEquals("Test CAF", entries[1].ChargeCode);
			AssertEquals("998.00", entries[2].Column7);
			AssertEquals("Test Lift On/Lift Off", entries[2].ChargeCode);
			AssertEquals("", entries[3].Column7);
			AssertEquals("", entries[3].ChargeCode);
			Assert("IsOtherChargesEntry", entries[3].IsOtherChargesEntry);

			formatTable = new PricingPage(rateEntry, Factory, PricingPageStyle.Landscape);
			testTabeleQuotation = DocTableQuotation.New(formatTable, Factory);
			entries = testTabeleQuotation.Entries.Cast<DocRateEntry>().OrderByDescending(x => x.ChargeCode).ToArray();
			Factory.Save();

			AssertEquals("99.00", entries[0].Column7);
			AssertEquals("Test Lift On/Lift Off", entries[0].ChargeCode);
			AssertEquals("100.00", entries[1].Column7);
			AssertEquals("Test CAF", entries[1].ChargeCode);
			AssertEquals("1000.00", entries[2].Column7);
			AssertEquals("Test BAF", entries[2].ChargeCode);
			AssertEquals("", entries[3].Column7);
			AssertEquals("", entries[3].ChargeCode);
			Assert("IsOtherChargesEntry", entries[3].IsOtherChargesEntry);

			formatTable = new PricingPage(quoteEntry, Factory, PricingPageStyle.Landscape);
			testTabeleQuotation = DocTableQuotation.New(formatTable, Factory);
			entries = testTabeleQuotation.Entries.Cast<DocRateEntry>().OrderByDescending(x => x.ChargeCode).ToArray();
			Factory.Save();

			AssertEquals(4, entries.Length);
			AssertEquals("10.00", entries[0].Column4);
			AssertEquals("Test Lift On/Lift Off", entries[0].ChargeCode);
			AssertEquals("100.00", entries[1].Column4);
			AssertEquals("Test CAF", entries[1].ChargeCode);
			AssertEquals("1000.00", entries[2].Column4);
			AssertEquals("Test BAF", entries[2].ChargeCode);
			AssertEquals("", entries[3].Column4);
			AssertEquals("", entries[3].ChargeCode);
			Assert("IsOtherChargesEntry", entries[3].IsOtherChargesEntry);
		}

		#region Implementation

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
			InsertClientChargeCodesForAutoRaterTests(Factory);
			TestBAF.AC_AT_GSTRate = ZGuid.Empty;
			TestAWB.AC_AT_GSTRate = ZGuid.Empty;

			Factory.Save();
		}

		#endregion
	}
}
