using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCostsComparer))]
	sealed class DocCostsComparerTest : DocumentWrapperTestCase
	{
		public void TestSummaryColumnsCaptions()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			RateLine testLine1 = TestRateEntry.AddRateLine("WAR");
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

			TestComparer.LoadCosts();

			DocCostsComparer testWrapper = (DocCostsComparer)GetNewBusinessObject();

			AssertEquals("Min", testWrapper.SummaryColumn1Caption);
			AssertEquals("Base Rate", testWrapper.SummaryColumn2Caption);
			AssertEquals("Per Unit", testWrapper.SummaryColumn3Caption);
			AssertEquals("", testWrapper.SummaryColumn4Caption);
		}

		public void TestModeDescription()
		{
			AssertEquals("Air Freight (LSE)", ((DocCostsComparer)GetNewBusinessObject()).ModeDescription);
		}

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

		TestHelper RatingTestHelper
		{
			get { return ratingTestHelper ?? (ratingTestHelper = new TestHelper(Factory)); }
		}
		TestHelper ratingTestHelper;

		CostsComparer TestComparer
		{
			get { return testComparer ?? (testComparer = new CostsComparer { Mode = "LSE" }); }
		}

		CostsComparer testComparer;

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new[] { CreateDocumentWrapperFromStaticNewMethod() };
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocCostsComparer.New(TestComparer, Factory);
		}
	}
}
