using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCostsComparerEntry))]
	sealed class DocCostsComparerEntryTest : DocumentWrapperTestCase
	{
		public void TestGetRateLineAndSummaryColumnValues()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var providerOrg = Factory.NewWithValidTestData<OrgHeader>();
			providerOrg.OH_FullName = "Maersk Org";

			var airline = TestObjectCreator.CreateAirLine("077");
			airline.RM_TwoCharacterCode = "MS";

			providerOrg.MiscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			providerOrg.MiscServ.OM_OH = providerOrg.PK;
			providerOrg.MiscServ.OM_RM_Airline = airline.PK;

			Factory.Save();

			RateLine testLine1 = TestRateEntry.AddRateLine("WAR");
			TestRateEntry.TI_Frequency = 1;
			TestRateEntry.TI_FrequencyUnit = RatingConstants.FrequencyUnits.Days;
			TestRateEntry.TI_TransitTime = RatingConstants.TransitTimes.Overnight;
			TestRateEntry.TI_OH_TransportProvider = providerOrg.PK;
			testRateEntry.TI_PL_NKCarrierServiceLevel = "STD";

			testLine1.TL_WeightVolume = RatingConstants.Units.KG;
			testLine1.TL_RateCalculator = MinimumCalculator.Code;
			((MinimumCalculator)testLine1.Calculator).MinimumValue = 100m;

			RateLine testLine2 = TestRateEntry.AddRateLine("BAF");
			testLine2.TL_WeightVolume = RatingConstants.Units.KG;
			testLine2.TL_RateCalculator = FlatPlusPerUnitCalculator.Code;
			((FlatPlusPerUnitCalculator)testLine2.Calculator).BaseRate = 50m;
			((FlatPlusPerUnitCalculator)testLine2.Calculator).PerUnit = 5m;

			RateLine testLine3 = TestRateEntry.RateLines[0];
			testLine3.TL_WeightVolume = RatingConstants.Units.KG;
			testLine3.TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)testLine3.Calculator).BaseRate = 40m;

			Factory.Save();

			CostsComparer testComparer = new CostsComparer { Mode = "LSE" };
			testComparer.LoadCosts();
			testCostsComparerEntry = testComparer.Costs[0];

			var testWrapper = (DocCostsComparerEntry)GetNewBusinessObject();

			AssertEquals("Provider Column Value", "Maersk Org (MS)", testWrapper.Provider);
			AssertEquals("Service Level Code Column Value", "STD", testWrapper.ServiceLevelCode);
			AssertEquals("Transit Time Column Value", "Overnight", testWrapper.TransitTime);
			AssertEquals("Frequency Column Value", "Every 1 Day", testWrapper.Frequency);

			TestRateEntry.TI_Frequency = 2;
			TestRateEntry.TI_FrequencyUnit = RatingConstants.FrequencyUnits.Days;

			Factory.Save();

			testComparer = new CostsComparer { Mode = "LSE" };
			testComparer.LoadCosts();
			testCostsComparerEntry = testComparer.Costs[0];

			testWrapper = (DocCostsComparerEntry)GetNewBusinessObject();

			AssertEquals("Frequency Column Value", "Every 2 Days", testWrapper.Frequency);

			TestRateEntry.TI_TransitTime = "";
			TestRateEntry.TI_FrequencyUnit = "";
			TestRateEntry.TI_Frequency = 0;

			Factory.Save();

			testComparer = new CostsComparer { Mode = "LSE" };
			testComparer.LoadCosts();
			testCostsComparerEntry = testComparer.Costs[0];

			testWrapper = (DocCostsComparerEntry)GetNewBusinessObject();

			AssertEquals("Transit Time Column Value should be empty", "", testWrapper.TransitTime);
			AssertEquals("Frequency Column Value should be empty ", "", testWrapper.Frequency);

			var actual = new List<string>();

			foreach (RateLine line in testComparer.Costs[0].RateLines)
			{
				for (int i = 1; i < 9; i++)
				{
					string value = testWrapper.GetRateLineColumnValue(i, line);
					if (!string.IsNullOrEmpty(value))
					{
						actual.Add(string.Format("ChargeCode is {0}, Column{1} value is {2}", line.ChargeCode.AC_Code, i, testWrapper.GetRateLineColumnValue(i, line)));
					}
				}
			}

			var expected = new[]
			{
				"ChargeCode is WAR, Column1 value is 100.00",
				"ChargeCode is BAF, Column2 value is 50.00",
				"ChargeCode is BAF, Column3 value is 5.00",
				"ChargeCode is FRT, Column2 value is 40.00"
			};

			AssertContainsExactElementsInAnyOrder(expected, actual);

			AssertEquals("100.00", testWrapper.SummaryColumn1);
			AssertEquals("90.00", testWrapper.SummaryColumn2);
			AssertEquals("5.00", testWrapper.SummaryColumn3);
			AssertEquals("", testWrapper.SummaryColumn4);
		}

		public void TestGetRateLineAndSummaryColumnValues_DifferentContractNumbers()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var entry1 = TestCosting.AddRateEntryWithUnitRateLine("AIR", "LSE", "AUSYD", "USLAX", "FRT", 10m, "KG");
			entry1.TI_ContractNumber = "AAA";

			var entry2 = TestCosting.AddRateEntryWithUnitRateLine("AIR", "LSE", "AUSYD", "USLAX", "FRT", 20m, "KG");
			entry2.TI_ContractNumber = "BBB";

			var entry3 = TestCosting.AddRateEntryWithUnitRateLine("AIR", "LSE", "AUSYD", "USLAX", "FRT", 30m, "KG");
			entry3.TI_ContractNumber = "CCC";

			Factory.Save();

			CostsComparer testComparer = new CostsComparer { Mode = "LSE" };
			testComparer.LoadCosts();
			var costs = testComparer.Costs.Cast<CostsComparerEntry>();

			AssertEquals(3, costs.Count(x => !x.Entry.TI_ContractNumber.IsEmpty));

			var cost1 = costs.Single(x => x.Entry.TI_ContractNumber == "AAA");
			var cost2 = costs.Single(x => x.Entry.TI_ContractNumber == "BBB");
			var cost3 = costs.Single(x => x.Entry.TI_ContractNumber == "CCC");

			var testWrapper = DocCostsComparerEntry.New(cost1, Factory);
			AssertEquals("10.00", testWrapper.GetRateLineColumnValue(1, cost1.RateLines[0]));

			testWrapper = DocCostsComparerEntry.New(cost2, Factory);
			AssertEquals("20.00", testWrapper.GetRateLineColumnValue(1, cost2.RateLines[0]));

			testWrapper = DocCostsComparerEntry.New(cost3, Factory);
			AssertEquals("30.00", testWrapper.GetRateLineColumnValue(1, cost3.RateLines[0]));
		}

		public void TestPerUnitValueForSlidingChargeColumns()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			Costing costing = RatingTestHelper.NewCosting(RatingTestHelper.NewOrgHeader());
			RateEntry entry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "AUMEL");
			entry.RateLines[0].TL_RateCalculator = CombinedCalculator.Code;
			entry.RateLines[0].Calculator["MIN"] = (ZDecimal)100m;
			entry.RateLines[0].Calculator["-45"] = (ZDecimal)5m;
			entry.RateLines[0].Calculator["+45"] = (ZDecimal)4m;
			entry.RateLines[0].Calculator["+100"] = (ZDecimal)3m;
			entry.RateLines[0].Calculator["+250"] = (ZDecimal)2m;
			entry.RateLines[0].Calculator["+500"] = (ZDecimal)1.9m;
			entry.RateLines[0].Calculator["+1000"] = (ZDecimal)1.85m;
			entry.AddRateLine("WAR");
			entry.RateLines[1].TL_RateCalculator = UnitCalculator.Code;
			entry.RateLines[1].Calculator["UNT"] = (ZDecimal)0.1m;

			Factory.Save();

			CostsComparer testComparer = new CostsComparer();
			testComparer.Mode = "LSE";
			testComparer.Origin = "AUSYD";
			testComparer.Destination = "AUMEL";
			testComparer.LoadCosts();
			CostsComparerEntry comparerEntry = testComparer.Costs[0];
			DocCostsComparerEntry testWrapper = DocCostsComparerEntry.New(comparerEntry, Factory);

			RateLine line1 = testComparer.Costs[0].RateLines[0];
			AssertEquals("100.00", testWrapper.GetRateLineColumnValue(1, line1));
			AssertEquals("5.00", testWrapper.GetRateLineColumnValue(2, line1));
			AssertEquals("4.00", testWrapper.GetRateLineColumnValue(3, line1));
			AssertEquals("3.00", testWrapper.GetRateLineColumnValue(4, line1));
			AssertEquals("2.00", testWrapper.GetRateLineColumnValue(5, line1));
			AssertEquals("1.90", testWrapper.GetRateLineColumnValue(6, line1));
			AssertEquals("1.85", testWrapper.GetRateLineColumnValue(7, line1));

			RateLine line2 = testComparer.Costs[0].RateLines[1];
			AssertEquals("", testWrapper.GetRateLineColumnValue(1, line2));
			AssertEquals("0.10", testWrapper.GetRateLineColumnValue(2, line2));
			AssertEquals("0.10", testWrapper.GetRateLineColumnValue(3, line2));
			AssertEquals("0.10", testWrapper.GetRateLineColumnValue(4, line2));
			AssertEquals("0.10", testWrapper.GetRateLineColumnValue(5, line2));
			AssertEquals("0.10", testWrapper.GetRateLineColumnValue(6, line2));
			AssertEquals("0.10", testWrapper.GetRateLineColumnValue(7, line2));
		}

		#region Implementation

		TestHelper RatingTestHelper
		{
			get { return ratingTestHelper ?? (ratingTestHelper = new TestHelper(Factory)); }
		}

		TestHelper ratingTestHelper;

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new[] { CreateDocumentWrapperFromStaticNewMethod() };
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocCostsComparerEntry.New(TestCostsComparerEntry, Factory);
		}

		CostsComparerEntry TestCostsComparerEntry
		{
			get { return testCostsComparerEntry ?? (testCostsComparerEntry = new CostsComparerEntry(new CostsComparer(), TestRateEntry, new List<RateLine>())); }
		}

		CostsComparerEntry testCostsComparerEntry;

		Costing TestCosting
		{
			get { return testCosting ?? (testCosting = RatingTestHelper.NewCosting(ratingTestHelper.NewOrgHeader())); }
		}
		Costing testCosting;

		RateEntry TestRateEntry
		{
			get { return testRateEntry ?? (testRateEntry = TestCosting.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX")); }
		}
		RateEntry testRateEntry;

		#endregion
	}
}
