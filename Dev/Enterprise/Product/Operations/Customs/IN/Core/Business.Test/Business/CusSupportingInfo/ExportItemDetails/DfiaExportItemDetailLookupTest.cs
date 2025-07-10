using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(DfiaExportItemDetailLookup))]
sealed class DfiaExportItemDetailLookupTest : BusinessObjectLookupsTestCase
{
	public void TestUnitOfQuantityList()
	{
		RefDataSetupTestHelper.SetupCustomsUnitOfQuantityCode(Factory);

		var unitOfQuantityList = Lookups.UnitOfQuantityList;
		var actualsUnits = unitOfQuantityList.GetAllCodes();
		CombineAssertions(() =>
		{
			AssertGreaterThanOrEqualTo("Count", actualsUnits.Length, 2);
			var expectedUnits = new[] { "KGS", "PCS" };
			foreach (var expectedUnit in expectedUnits)
			{
				Assert($"Units does not contain {expectedUnit}", actualsUnits.Contains(expectedUnit));
			}
			AssertSame("Cached", unitOfQuantityList, Lookups.UnitOfQuantityList);
		});
	}

	CusSupportingInfoLookups Lookups => lookups ??= Factory.New<DfiaExportItemDetail>().Lookups;
	CusSupportingInfoLookups lookups;
}
