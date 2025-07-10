using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	[TestedType(typeof(DocRateEntry))]
	sealed class DocRateEntryTest : DocumentWrapperTestCase
	{
		public void TestUniqueFreightEntriesIgnoringMode()
		{
			Quote quote = Factory.New<Quote>();
			RateEntry entry1 = quote.AddRateEntry("FCL", "FCL", "AU", "US");
			RateEntry entry2 = quote.AddRateEntry("LCL", "LCL", "AU", "US");
			RateEntry entry3 = quote.AddRateEntry("FCL", "FCL", "AU", "SG");
			RateEntry entry4 = quote.AddRateEntry("ORG", "ALL", "AU", "US");
			RateEntry entry5 = quote.AddRateEntry("DST", "ALL", "AU", "US");

			RateEntry[] entries = { entry1, entry2, entry3, entry4, entry5 };

			Dictionary<ZGuid, string> names = new Dictionary<ZGuid, string>();
			names[entry1.PK] = "entry1";
			names[entry2.PK] = "entry2";
			names[entry3.PK] = "entry3";
			names[entry4.PK] = "entry4";
			names[entry5.PK] = "entry5";

			AssertContainsExactElementsInAnyOrder(
				(e) =>
				{
					string name;
					return names.TryGetValue(e.PK, out name) ? e.PK.ToString() + ": " + name : e.PK.ToString();
				},
				new RateEntry[] { entry1, entry3 },
				DocRateEntry.UniqueFreightEntriesIgnoringMode(entries));
		}

		public void TestGetRelevantValueDecimalPlaces()
		{
			var entry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("FCL", "LSE", "AUSYD", "USLAX");
			var testDocRateEntry = new DocRateEntryForTest(Factory);

			AssertEquals(string.Empty, testDocRateEntry.GetRelevantValueOverride(null));

			var rateLine = entry.RateLines.AddNew();
			var item = rateLine.RateLineItems.AddNew();

			item.TM_Value = 0m;
			AssertEquals(string.Empty, testDocRateEntry.GetRelevantValueOverride(item));

			item.TM_Value = 100m;
			AssertEquals("100.00", testDocRateEntry.GetRelevantValueOverride(item));
			item.TM_Value = 100.1m;
			AssertEquals("100.10", testDocRateEntry.GetRelevantValueOverride(item));
			item.TM_Value = 100.12m;
			AssertEquals("100.12", testDocRateEntry.GetRelevantValueOverride(item));
			item.TM_Value = 100.123m;
			AssertEquals("100.123", testDocRateEntry.GetRelevantValueOverride(item));
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
				{
					DocRateEntry.New(Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX"), Factory)
				};
		}

		public void TestContractNumber()
		{
			RateEntry entry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("FCL", "LSE", "AUSYD", "USLAX");
			DocRateEntry docEntry = DocRateEntry.New(entry, Factory);

			AssertEquals(ZString.Empty, docEntry.ContractNumber);

			entry.TI_ContractNumber = "12345";
			AssertEquals("Wrong contract number", "12345", docEntry.ContractNumber);
		}

		public void TestAIRWeightBreaksWithMinimumOrPerUnitCalculator()
		{
			AIRWeightBreaksWithMinimumOrPerUnitCalculator(true);
			AIRWeightBreaksWithMinimumOrPerUnitCalculator(false);
		}

		void AIRWeightBreaksWithMinimumOrPerUnitCalculator(bool agentDeclaredRate)
		{
			RateEntry testEntry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			testEntry.RateLines[0].TL_RateCalculator = MinimumOrPerUnitCalculator.Code;
			testEntry.RateLines[0].Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)10m;
			testEntry.RateLines[0].RateLineItems.FindByTM_Type("UNT").TM_Value = 5m;
			testEntry.RateLines[0].RateLineItems.FindByTM_Type("UNT").TM_AgentDeclaredRate = 100m;

			PricingPage formatTable = new PricingPage(testEntry, Factory, PricingPageStyle.Landscape);
			DocTableQuotation testQuotation = DocTableQuotation.New(formatTable, Factory);
			DocRateEntry docEntry = testQuotation.Entries[0];

			docEntry.ViewAgentRates = agentDeclaredRate;
			docEntry.SetAirColumnDefinitions();

			AssertEquals("MIN", docEntry.GroupLCLHeader4);
			AssertEquals("per KG", docEntry.GroupLCLHeader5);
			Assert(docEntry.GroupLCLHeader6.IsEmpty);
			Assert(docEntry.GroupLCLHeader7.IsEmpty);
			Assert(docEntry.GroupLCLHeader8.IsEmpty);
			Assert(docEntry.GroupLCLHeader9.IsEmpty);
			Assert(docEntry.GroupLCLHeader10.IsEmpty);
			Assert(docEntry.GroupLCLHeader11.IsEmpty);

			AssertEquals("10.00", docEntry.Column4);
			if (agentDeclaredRate)
			{
				AssertEquals("100.00", docEntry.Column5);
				Assert(docEntry.Column6.IsEmpty);
				Assert(docEntry.Column7.IsEmpty);
				Assert(docEntry.Column8.IsEmpty);
				Assert(docEntry.Column9.IsEmpty);
				Assert(docEntry.Column10.IsEmpty);
				Assert(docEntry.Column11.IsEmpty);
			}
			else
			{
				AssertEquals("5.00", docEntry.Column5);
				Assert(docEntry.Column6.IsEmpty);
				Assert(docEntry.Column7.IsEmpty);
				Assert(docEntry.Column8.IsEmpty);
				Assert(docEntry.Column9.IsEmpty);
				Assert(docEntry.Column10.IsEmpty);
				Assert(docEntry.Column11.IsEmpty);
			}
		}

		public void TestAIRWeightBreaksWithFlatAmounts()
		{
			AIRWeightBreaksWithFlatAmounts(true);
			AIRWeightBreaksWithFlatAmounts(false);
		}
		void AIRWeightBreaksWithFlatAmounts(bool agentDeclaredRate)
		{
			RateEntry testEntry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			testEntry.RateLines[0].Calculator["+1000"] = (ZDecimal)5m;
			RateLineItem testRateLineItem = testEntry.RateLines[0].RateLineItems.FindByTM_Type("-");
			testRateLineItem.TM_FlatAmount = 100m;
			testRateLineItem.TM_AgentDeclaredRate = 50m;
			testRateLineItem = (RateLineItem)testRateLineItem.NextRateLineItem(testEntry.RateLines[0].RateLineItems.Cast<RateLineItem>());
			testRateLineItem.TM_Value = 120m;
			testRateLineItem.TM_AgentDeclaredRate = 60m;
			testRateLineItem = (RateLineItem)testRateLineItem.NextRateLineItem(testEntry.RateLines[0].RateLineItems.Cast<RateLineItem>());
			testRateLineItem.TM_Value = 150m;
			testRateLineItem.TM_AgentDeclaredRate = 75m;
			testRateLineItem = (RateLineItem)testRateLineItem.NextRateLineItem(testEntry.RateLines[0].RateLineItems.Cast<RateLineItem>());
			testRateLineItem.TM_Value = 200m;
			testRateLineItem.TM_AgentDeclaredRate = 100m;
			testRateLineItem = (RateLineItem)testRateLineItem.NextRateLineItem(testEntry.RateLines[0].RateLineItems.Cast<RateLineItem>());
			testRateLineItem.TM_FlatAmount = 300m;
			testRateLineItem.TM_Value = 55m;
			testRateLineItem.TM_AgentDeclaredRate = 0m;

			PricingPage formatTable = new PricingPage(testEntry, Factory, PricingPageStyle.Landscape);
			DocTableQuotation testQuotation = DocTableQuotation.New(formatTable, Factory);
			DocRateEntry docEntry = testQuotation.Entries[0];
			docEntry.ViewAgentRates = agentDeclaredRate;
			docEntry.SetAirColumnDefinitions();

			AssertEquals("-45 per KG", docEntry.GroupLCLHeader5);
			AssertEquals("+45 per KG", docEntry.GroupLCLHeader6);
			AssertEquals("+100 per KG", docEntry.GroupLCLHeader7);
			AssertEquals("+250 per KG", docEntry.GroupLCLHeader8);
			AssertEquals("+500 per KG", docEntry.GroupLCLHeader9);
			AssertEquals("+1000 per KG", docEntry.GroupLCLHeader10);

			if (agentDeclaredRate)
			{
				AssertEquals("50.00", docEntry.Column5);
				AssertEquals("60.00", docEntry.Column6);
				AssertEquals("75.00", docEntry.Column7);
				AssertEquals("100.00", docEntry.Column8);
			}
			else
			{
				AssertEquals("0.00" + System.Environment.NewLine + "100.00", docEntry.Column5);
				AssertEquals("120.00", docEntry.Column6);
				AssertEquals("150.00", docEntry.Column7);
				AssertEquals("200.00", docEntry.Column8);
			}
			AssertEquals("55.00" + System.Environment.NewLine + "300.00", docEntry.Column9);
			AssertEquals("Value should be defaulted here from column #9. Flat amount not.", "55.00", docEntry.Column10);
		}

		public void TestCostingColumnsFit()
		{
			ClientRate clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			RateLine rateLine = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			rateLine.RateLineItems.RemoveAndDeleteAll();
			rateLine.Calculator["MIN"] = (ZDecimal)500m;
			rateLine.Calculator["-45"] = (ZDecimal)5m;
			rateLine.Calculator["+45"] = (ZDecimal)4m;
			rateLine.Calculator["+100"] = (ZDecimal)3.5m;
			rateLine.Calculator["+300"] = (ZDecimal)3.4m;
			rateLine.Calculator["+450"] = (ZDecimal)3.3m;
			rateLine.Calculator["+600"] = (ZDecimal)3.2m;

			Factory.Save();

			PricingPageCollection pages = new PricingPageCollection(clientRate);
			pages.Load(PricingPaginationStrategy.LandscapeSimpleStyle);
			AssertEquals(1, pages.Count);

			DocTableQuotation testQuotation = DocTableQuotation.New(pages[0], Factory);
			AssertEquals(2, testQuotation.Entries.Count);

			DocRateEntry docEntry = testQuotation.Entries[0];

			AssertEquals(DocRateEntry.AirLCLDocRateEntryTypes.WeightBreaksMatrixLine, docEntry.AirLCLType);
			AssertEquals("MIN", docEntry.GroupLCLHeader4);
			AssertEquals("-45 per KG", docEntry.GroupLCLHeader5);
			AssertEquals("+45 per KG", docEntry.GroupLCLHeader6);
			AssertEquals("+100 per KG", docEntry.GroupLCLHeader7);
			AssertEquals("+300 per KG", docEntry.GroupLCLHeader8);
			AssertEquals("+450 per KG", docEntry.GroupLCLHeader9);
			AssertEquals("+600 per KG", docEntry.GroupLCLHeader10);

			AssertEquals("500.00", docEntry.Column7);
			AssertEquals("5.00", docEntry.Column8);
			AssertEquals("4.00", docEntry.Column9);
			AssertEquals("3.50", docEntry.Column10);
			AssertEquals("3.40", docEntry.Column11);
			AssertEquals("3.30", docEntry.Column12);
			AssertEquals("3.20", docEntry.Column13);

			AssertEquals(DocRateEntry.AirLCLDocRateEntryTypes.OtherChargesLine, testQuotation.Entries[1].AirLCLType);
		}

		public void TestAIRWeightBreaksWithCompanyTariffOrCostBasedCalculator()
		{
			RateLine tariffLine = Factory.New<CompanyTariff>().AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			tariffLine.RateLineItems.RemoveAndDeleteAll();
			tariffLine.Calculator["-45"] = (ZDecimal)5m;
			tariffLine.Calculator["+45"] = (ZDecimal)4m;
			tariffLine.Calculator["+100"] = (ZDecimal)3.5m;
			tariffLine.Calculator["+300"] = (ZDecimal)3.4m;

			OrgHeader client = Helper.NewOrgHeader(1);
			Factory.Save();

			Quote quote = Helper.NewQuote(client);
			RateEntry testQuoteEntry = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			testQuoteEntry.RateLines[0].TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;

			Factory.Save();

			var pages = new PricingPageCollection(quote);
			pages.Load(PricingPaginationStrategy.LandscapeSimpleStyle);
			AssertEquals(1, pages.Count);

			DocTableQuotation testQuotation = DocTableQuotation.New(pages[0], Factory);
			AssertEquals(2, testQuotation.Entries.Count);

			DocRateEntry docEntry = testQuotation.Entries[0];

			AssertEquals(DocRateEntry.AirLCLDocRateEntryTypes.WeightBreaksMatrixLine, docEntry.AirLCLType);

			AssertEquals("-45 per KG", docEntry.GroupLCLHeader4);
			AssertEquals("+45 per KG", docEntry.GroupLCLHeader5);
			AssertEquals("+100 per KG", docEntry.GroupLCLHeader6);
			AssertEquals("+300 per KG", docEntry.GroupLCLHeader7);

			AssertEquals("5.00", docEntry.Column4);
			AssertEquals("4.00", docEntry.Column5);
			AssertEquals("3.50", docEntry.Column6);
			AssertEquals("3.40", docEntry.Column7);

			AssertEquals(DocRateEntry.AirLCLDocRateEntryTypes.OtherChargesLine, testQuotation.Entries[1].AirLCLType);
		}

		public void TestConsignorConsignee()
		{
			OrgHeader consignor = Helper.NewOrgHeader();
			OrgHeader consignee = Helper.NewOrgHeader();

			RateEntry entry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_OH_Consignor = consignor.PK;

			DocRateEntry docEntry = DocRateEntry.New(entry, Factory);
			AssertEquals("Consignor: " + consignor.OH_FullName, docEntry.OtherCharges);

			entry.TI_OH_Consignee = consignee.PK;
			docEntry = DocRateEntry.New(entry, Factory);
			AssertEquals("Consignor: " + consignor.OH_FullName + System.Environment.NewLine + "Consignee: " + consignee.OH_FullName, docEntry.OtherCharges);

			entry.TI_OH_Consignor = ZGuid.Empty;
			docEntry = DocRateEntry.New(entry, Factory);
			AssertEquals("Consignee: " + consignee.OH_FullName, docEntry.OtherCharges);

			entry.TI_OH_Consignee = ZGuid.Empty;
			docEntry = DocRateEntry.New(entry, Factory);
			AssertEquals("", docEntry.OtherCharges);
		}

		public void TestChargeCodeGetsOverriddenDescripiton()
		{
			RateEntry testEntry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			testEntry.RateLines[0].TL_RateCalculator = FlatCalculator.Code;
			testEntry.RateLines[0].Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)10m;
			testEntry.RateLines[0].TL_RateDesc = "blablabla";

			PricingPage formatTable = new PricingPage(testEntry, Factory, PricingPageStyle.Standard);
			DocTableQuotation testQuotation = DocTableQuotation.New(formatTable, Factory);
			DocRateEntry docEntry = testQuotation.Entries[0];

			docEntry.SetAirColumnDefinitions();

			AssertEquals("blablabla", docEntry.ChargeCode);
		}

		public void TestOriginDestionationForOnforwarding()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry("AU");

				RateEntry entry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "", "");
				entry.TI_ViaLRC = "SGSIN";
				DocRateEntry docEntry = DocRateEntry.New(entry, Factory);

				AssertEquals("Via Singapore", docEntry.OriginDestination);
				AssertEquals("Via Singapore", docEntry.OverseasPortVia);
				AssertEquals("Singapore", docEntry.Via);

				entry.TI_OriginLRC = "AUMEL";
				AssertEquals("From Melbourne via Singapore", docEntry.OriginDestination);
				AssertEquals("Via Singapore", docEntry.OverseasPortVia);

				entry.TI_OriginLRC = "USLAX";
				AssertEquals("From Los Angeles via Singapore", docEntry.OriginDestination);
				AssertEquals("Los Angeles via Singapore", docEntry.OverseasPortVia);

				entry.TI_OriginLRC = "";
				entry.TI_DestinationLRC = "AUMEL";
				AssertEquals("To Melbourne via Singapore", docEntry.OriginDestination);
				AssertEquals("Via Singapore", docEntry.OverseasPortVia);

				entry.TI_DestinationLRC = "USLAX";
				AssertEquals("To Los Angeles via Singapore", docEntry.OriginDestination);
				AssertEquals("Los Angeles via Singapore", docEntry.OverseasPortVia);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		public void TestOverseasCountry()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry("AU");

				RateEntry entry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
				DocRateEntry docEntry = DocRateEntry.New(entry, Factory);

				AssertEquals("United States", docEntry.OverseasCountry);

				entry.TI_OriginLRC = "GBLON";
				AssertEquals("United Kingdom - United States", docEntry.OverseasCountry);

				entry.TI_DestinationLRC = "AUMEL";
				AssertEquals("United Kingdom", docEntry.OverseasCountry);

				entry.TI_OriginLRC = "AUSYD";
				AssertEquals("", docEntry.OverseasCountry);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		public void TestTransitTimeAndFrequency()
		{
			RateEntry entry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_TransitTime = RatingConstants.TransitTimes.Overnight;
			entry.TI_Frequency = 3;
			entry.TI_FrequencyUnit = RatingConstants.FrequencyUnits.Week;

			DocRateEntry docEntry = DocRateEntry.New(entry, Factory);
			docEntry.SetAirColumnDefinitions();
			AssertEquals("", docEntry.OtherCharges);

			entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_TransitTime = RatingConstants.TransitTimes.Overnight;
			entry.TI_Frequency = 3;
			entry.TI_FrequencyUnit = RatingConstants.FrequencyUnits.Week;

			docEntry = DocRateEntry.New(entry, Factory);
			docEntry.SetAirColumnDefinitions();
			AssertEquals("Transit Time: Overnight" + System.Environment.NewLine + "Frequency: 3 per Week", docEntry.OtherCharges);

			entry.TI_TransitTime = ZString.Empty;
			entry.TI_Frequency = ZInt.Zero;
			entry.TI_FrequencyUnit = ZString.Empty;

			docEntry = DocRateEntry.New(entry, Factory);
			docEntry.SetAirColumnDefinitions();
			AssertEquals("", docEntry.OtherCharges);

			entry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("FCL", "SEA", "AUMEL", "SGSIN", "STD", "20GP");
			entry.TI_TransitTime = "12";
			entry.TI_Frequency = 2;
			entry.TI_FrequencyUnit = RatingConstants.FrequencyUnits.Monthly;

			docEntry = DocRateEntry.New(entry, Factory);
			docEntry.SetSeaColumnDefinitions();
			AssertEquals("", docEntry.OtherCharges);

			entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("FCL", "SEA", "AUMEL", "SGSIN", "STD", "20GP");
			entry.TI_TransitTime = "12";
			entry.TI_Frequency = 2;
			entry.TI_FrequencyUnit = RatingConstants.FrequencyUnits.Monthly;

			docEntry = DocRateEntry.New(entry, Factory);
			docEntry.SetSeaColumnDefinitions();
			AssertEquals("Transit Time: 12 Days" + System.Environment.NewLine + "Frequency: 2 per Month", docEntry.OtherCharges);

			entry.TI_TransitTime = ZString.Empty;
			entry.TI_Frequency = ZInt.Zero;
			entry.TI_FrequencyUnit = ZString.Empty;

			docEntry = DocRateEntry.New(entry, Factory);
			docEntry.SetSeaColumnDefinitions();
			AssertEquals("", docEntry.OtherCharges);
		}

		public void TestWithContainerClass()
		{
			RefContainer refContainer1 = Helper.Containers["40GP"];
			RefContainer refContainer2 = Helper.Containers["40HC"];
			refContainer1.RC_FreightRateClass = "40";
			refContainer2.RC_FreightRateClass = "40";

			Quote quote = Helper.NewQuote(Helper.NewOrgHeader());

			RateEntry entry1 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry1.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 1000m;
			entry1.TI_RC = refContainer1.PK;
			entry1.TI_MatchContainerRateClass = true;

			RateEntry entry2 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry2.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 1200m;
			entry2.TI_RC = refContainer2.PK;

			Factory.Save();

			PricingPageCollection pages = new PricingPageCollection(quote);
			pages.Load(PricingPaginationStrategy.LandscapeComplexStyle);
			AssertEquals(1, pages.Count);

			DocTableQuotation testQuotation = DocTableQuotation.New(pages[0], Factory);
			AssertEquals(1, testQuotation.Entries.Count);

			DocRateEntry docEntry = testQuotation.Entries[0];
			AssertEquals("40GP", docEntry.Header4);
			AssertEquals("40HC", docEntry.Header5);
			AssertEquals(ZString.Empty, docEntry.Header6);

			AssertEquals("1000.00", docEntry.Column4);
			AssertEquals("1200.00", docEntry.Column5);
			AssertEquals(ZString.Empty, docEntry.Column6);
		}

		public void TestAirFreightIncludeTransportProviderOnQuotationRegistry()
		{
			DocumentsDataRegistry.Instance.AirFreightIncludeTransportProviderOnQuotation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			RateEntry entry1 = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			OrgHeader transportProvider1 = Helper.NewOrgHeader();
			transportProvider1.OH_FullName = "Transport Provider";
			entry1.TI_OH_TransportProvider = transportProvider1.PK;
			DocRateEntry docEntryAir1 = DocRateEntry.New(entry1, Factory);
			docEntryAir1.SetAirColumnDefinitions();
			AssertEquals("Airline", docEntryAir1.Header2);
			AssertEquals("", docEntryAir1.Carrier);

			DocumentsDataRegistry.Instance.AirFreightIncludeTransportProviderOnQuotation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			RateEntry entry2 = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			OrgHeader transportProvider2 = Helper.NewOrgHeader();
			transportProvider2.OH_FullName = "Transport Provider";
			entry2.TI_OH_TransportProvider = transportProvider2.PK;
			DocRateEntry docEntryAir2 = DocRateEntry.New(entry2, Factory);
			docEntryAir2.SetAirColumnDefinitions();
			AssertEquals("Airline", docEntryAir2.Header2);
			AssertEquals("Transport Provider", docEntryAir2.Carrier);
		}

		public void TestSeaFreightIncludeTransportProviderOnQuotationRegistry()
		{
			DocumentsDataRegistry.Instance.SeaFreightIncludeTransportProviderOnQuotation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			RateEntry entry1 = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			OrgHeader transportProvider1 = Helper.NewOrgHeader();
			transportProvider1.OH_FullName = "Transport Provider";
			entry1.TI_OH_TransportProvider = transportProvider1.PK;
			DocRateEntry docEntrySea1 = DocRateEntry.New(entry1, Factory);
			docEntrySea1.SetSeaColumnDefinitions();
			AssertEquals("Shipping Line", docEntrySea1.Header2);
			AssertEquals("", docEntrySea1.Carrier);

			DocumentsDataRegistry.Instance.SeaFreightIncludeTransportProviderOnQuotation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			RateEntry entry2 = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			OrgHeader transportProvider2 = Helper.NewOrgHeader();
			transportProvider2.OH_FullName = "Transport Provider";
			entry2.TI_OH_TransportProvider = transportProvider2.PK;
			DocRateEntry docEntrySea2 = DocRateEntry.New(entry2, Factory);
			docEntrySea2.SetSeaColumnDefinitions();
			AssertEquals("Shipping Line", docEntrySea2.Header2);
			AssertEquals("Transport Provider", docEntrySea2.Carrier);
		}

		#region Implementation

		class DocRateEntryForTest : DocRateEntry
		{
			public DocRateEntryForTest(BusinessObjectFactory factory) : base(null, factory) { }

			public ZString GetRelevantValueOverride(RateLineItem item)
			{
				return GetRelevantValue(item);
			}
		}

		TestHelper Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new TestHelper(Factory);
				}

				return helper;
			}
		}
		TestHelper helper;

		#endregion
	}
}
