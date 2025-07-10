using CargoWise.EntityFramework.Testing;
using Enterprise.LandedCosting.Business;
using Enterprise.LandedCosting.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocLandedCostHistoryComparerTest : TestCaseWithFactory
	{
		public void TestCompare()
		{
			var dummyHost = Factory.New<DummyLandedCostHeader>();
			dummyHost.LineComparerExposed = new DistributeeComparerTest.DummyLineComparer();

			DummyIUltimateDistributee dummyDistributee1 = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee1.HumanReadableCodeExposed = "ZZZ";

			DummyIUltimateDistributee dummyDistributee2 = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee2.HumanReadableCodeExposed = "AAA";

			DummyIUltimateDistributee dummyDistributee3 = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee3.HumanReadableCodeExposed = "CCC";

			var lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_ParentID = dummyHost.PK;
			lCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			LandedCostHistory lCLine1 = lCHeader.Histories.AddNew();
			lCLine1.UltimateDistributee = dummyDistributee1;

			LandedCostHistory lCLine2 = lCHeader.Histories.AddNew();
			lCLine2.UltimateDistributee = dummyDistributee2;

			LandedCostHistory lCLine3 = lCHeader.Histories.AddNew();
			lCLine3.UltimateDistributee = dummyDistributee3;

			DocLandedCostHistory docLCLine1 = DocLandedCostHistory.New(lCLine1, Factory);
			DocLandedCostHistory docLCLine2 = DocLandedCostHistory.New(lCLine2, Factory);
			DocLandedCostHistory docLCLine3 = DocLandedCostHistory.New(lCLine3, Factory);

			DocLandedCostHistoryComparer comparer = new DocLandedCostHistoryComparer();
			int result = comparer.Compare(docLCLine1, docLCLine2);
			AssertEquals("ZZZ > AAA", true, result > 0);

			comparer = new DocLandedCostHistoryComparer();
			result = comparer.Compare(docLCLine2, docLCLine3);
			AssertEquals("AAA < CCC", true, result < 0);

			comparer = new DocLandedCostHistoryComparer();
			result = comparer.Compare(docLCLine3, docLCLine1);
			AssertEquals("CCC < ZZZ", true, result < 0);
		}
	}
}
