using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class CACusRulingFilterLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRulingTypeList()
		{
			var expectedValues = new[] { "2", "6", "R", "T", "W", "DD", "G", "SP", "SW" };
			var rulingFilter = new CACusRulingFilterStripBusinessObject();
			AssertContainsExactElementsInAnyOrder(expectedValues, rulingFilter.Lookups.RulingTypeList.GetAllCodes());
		}
	}
}
