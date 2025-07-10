using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(BillContainerCollection))]
	class BillContainerCollectionTest : Customs.Business.Testing.BaseHouseBillContainerCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new BillContainerCollection(HouseBill);

		protected override Customs.Business.BaseBillContainerCollection CreateContainers() => new BillContainerCollection(HouseBill);

		new Bill HouseBill => (Bill)base.HouseBill;
	}
}
