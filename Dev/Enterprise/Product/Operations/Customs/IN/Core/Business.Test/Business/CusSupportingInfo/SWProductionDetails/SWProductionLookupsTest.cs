using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(SWProductionLookups))]
sealed class SWProductionLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestUnitOfQuantityList()
	{
		RefDataSetupTestHelper.SetupCustomsUnitOfQuantityCode(Factory);
		var swProductionLine = Factory.New<SWProduction>();
		var actualList = swProductionLine.Lookups.UnitOfQuantityList;
		var actualCodes = actualList.GetAllCodes();

		AssertEquals("Count", actualCodes.Length, 2);

		var expectedCodes = new[] { "KGS", "PCS" };
		AssertContainsExactElementsInExactOrder(expectedCodes, actualCodes);
		AssertSame("Cached", actualList, swProductionLine.Lookups.UnitOfQuantityList);
	}
}

