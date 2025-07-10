using CargoWise.EntityFramework.Testing;
using Enterprise.LandedCosting.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocLandedCostInputTestCase : TestCaseWithFactory
	{
		public void TestRoundingHelperReturnsHeaderRoundingHelper()
		{
			LandedCostHeader header = Factory.New<LandedCostHeader>();
			LandCostInput lCInput = header.CostInputs.AddNew();
			DocLandedCostInput docLCInput = new DocLandedCostInput(lCInput, Factory);
			AssertEquals(header.RoundingHelper, docLCInput.RoundingHelper);
		}
	}
}
