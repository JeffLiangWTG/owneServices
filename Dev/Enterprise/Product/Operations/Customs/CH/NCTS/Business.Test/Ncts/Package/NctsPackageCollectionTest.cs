using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsPackageCollection))]
sealed class NctsPackageCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestNewPackageLinkedToSingleContainer() => CombineAssertions(() =>
	{
		var departurePackages = GetDepartureCollectionToTest();
		var departureNctsHeader = departurePackages.Master.Header;

		var departurePackage0 = departurePackages.AddNew();
		AssertEquals("Departure: No containers", 0, departurePackage0.ContainersPivot.Count);

		var departureContainer1 = departureNctsHeader.DepartureHeaderContainers.AddNew();
		var departurePackage1 = departurePackages.AddNew();
		AssertSame("Departure: Linked to single container", departureContainer1, departurePackage1.ContainersPivot.FirstOrDefault().Container);

		var departureContainer2 = departureNctsHeader.DepartureHeaderContainers.AddNew();
		var departurePackage2 = departurePackages.AddNew();
		AssertEquals("Departure: Not linked if multiple containers", 0, departurePackage2.ContainersPivot.Count);
		departurePackage2.ContainersPivot.Contains(departureContainer1);

		var arrivalPackages = GetArrivalCollectionToTest();
		var arrivalNctsHeader = arrivalPackages.Master.Header;

		var arrivalPackage0 = arrivalPackages.AddNew();
		AssertEquals("Arrival: No containers", 0, arrivalPackage0.ContainersPivot.Count);

		var arrivalContainer1 = arrivalNctsHeader.DepartureHeaderContainers.AddNew();
		var arrivalPackage1 = arrivalPackages.AddNew();
		AssertEquals("Arrival: Not linked to single container", 0, arrivalPackage0.ContainersPivot.Count);
	});

	protected override BusinessObjectCollection GetCollectionToTest() => GetDepartureCollectionToTest();

	NctsPackageCollection GetDepartureCollectionToTest() => new NctsPackageCollection(GetBill(NctsMovementType.Codes.Departure).GoodsItems.AddNew());

	NctsPackageCollection GetArrivalCollectionToTest() => new NctsPackageCollection(GetBill(NctsMovementType.Codes.Arrival).ArrivalGoodsItems.AddNew());

	NctsBill GetBill(string movementType)
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(movementType);
		return nctsHeader.Bills.AddNew();
	}
}
