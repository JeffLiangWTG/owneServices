using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(DfiaImportItemDetailLookup))]
sealed class DfiaImportItemDetailLookupTest : BusinessObjectLookupsTestCase
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
			AssertSame("cached", Lookups.UnitOfQuantityList, unitOfQuantityList);
		});
	}

	public void TestIssuerTypeList()
	{
		var issuerTypeList = Lookups.IssuerTypeList;
		AssertContainsExactElementsInAnyOrder("IssuerTypeList", new[] { "N", "M" }, issuerTypeList.GetAllCodes());
		AssertSame("cached", Lookups.IssuerTypeList, issuerTypeList);
	}

	CusSupportingInfoLookups Lookups => lookups ??= Factory.New<DfiaImportItemDetail>().Lookups;
	CusSupportingInfoLookups lookups;
}
