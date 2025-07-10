using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(HouseConsignmentDifferencesCollection))]
sealed class HouseConsignmentDifferencesCollectionTest : SingleCusCodeDataCollectionTest<HouseConsignmentDifferences>
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.Bills.AddNew().HouseConsignmentDifferences;
	}
}
