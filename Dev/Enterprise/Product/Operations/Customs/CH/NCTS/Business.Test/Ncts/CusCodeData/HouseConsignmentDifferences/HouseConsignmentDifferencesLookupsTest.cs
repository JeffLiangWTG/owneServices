using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(HouseConsignmentDifferencesLookups))]
sealed class HouseConsignmentDifferencesLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCY_CodeList() => CombineAssertions(() =>
	{
		AssertEquals("Codes", "1, 2, 3, 4", Lookups.CY_CodeList.CodesAsString);
		AssertSame("Cached", Lookups.CY_CodeList, Lookups.CY_CodeList);
	});

	HouseConsignmentDifferences HouseConsignmentDifference => houseConsignmentDifference ?? (houseConsignmentDifference = CreateHouseConsignmentDifference());
	HouseConsignmentDifferences houseConsignmentDifference;

	HouseConsignmentDifferences CreateHouseConsignmentDifference()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.Bills.AddNew().HouseConsignmentDifference;
	}

	HouseConsignmentDifferencesLookups Lookups => HouseConsignmentDifference.Lookups;
}
