using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(BillContainerCollection))]
	class BillContainerCollectionTest : Customs.Business.Testing.BaseHouseBillContainerCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new BillContainerCollection(HouseBill);

		protected new BillContainerCollection Containers => (BillContainerCollection)base.Containers;

		protected override Customs.Business.BaseBillContainerCollection CreateContainers() => new BillContainerCollection(HouseBill);

		protected new Bill HouseBill => (Bill)base.HouseBill;
	}
}
