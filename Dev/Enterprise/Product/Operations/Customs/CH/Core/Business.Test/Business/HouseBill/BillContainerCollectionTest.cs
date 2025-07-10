using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(BillContainerCollection))]
class BillContainerCollectionTest : Customs.Business.Testing.BaseHouseBillContainerCollectionTest
{
	protected override BusinessObjectCollection GetCollectionToTest() => CreateContainers();

	protected override Customs.Business.BaseBillContainerCollection CreateContainers() => new BillContainerCollection((Bill)HouseBill);
}
