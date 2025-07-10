using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NonPersistentDepartureContainerPivotCollection))]
	class NonPersistentContainerPivotCollectionTests : NonPersistentBusinessObjectCollectionTestCase<NonPersistentDepartureContainerPivotCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, GetCollectionToTest().AllowNew);
		}

		public void TestLoadCollection()
		{
			var headerContainer = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer.BC_ContainerNum = "AAA";
			var headerContainer2 = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer2.BC_ContainerNum = "BBB";

			var collection = GetCollectionToTest();
			CombineAssertions(() =>
			{
				AssertEquals("Count", 2, collection.Count);
				AssertEquals("HasChanges", false, collection.HasChanges);
			});
		}

		public void TestRemoveNonPersistentContainer()
		{
			var collection = GetCollectionToTest();
			AssertEquals("Count initial", 0, collection.Count);

			var headerContainer1 = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer1.BC_ContainerNum = "AAA";
			var headerContainer2 = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer2.BC_ContainerNum = "AAA";
			var headerContainer3 = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer3.BC_ContainerNum = "BBB";

			CombineAssertions(() =>
			{
				AssertEquals("Count after adding containers", 3, collection.Count);
				AssertEquals("headerContainer1 is present", collection[0].Container, headerContainer1);
				AssertEquals("headerContainer2 is present", collection[1].Container, headerContainer2);
				AssertEquals("headerContainer3 is present", collection[2].Container, headerContainer3);
			});

			collection.RemoveNonPersistentContainer(headerContainer2);
			CombineAssertions(() =>
			{
				AssertEquals("Count after removal", 2, collection.Count);
				AssertEquals("headerContainer1 is present", collection[0].Container, headerContainer1);
				AssertEquals("headerContainer3 is present", collection[1].Container, headerContainer3);
			});
		}

		protected override NonPersistentDepartureContainerPivotCollection GetCollectionToTest() => new NonPersistentDepartureContainerPivotCollection(nctsHeader.Bills.AddNew().GoodsItems.AddNew());

		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection() => new NonPersistentDepartureContainerPivot(nctsHeader.Bills.AddNew().GoodsItems.AddNew());

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}
		NctsHeader nctsHeader;
	}

	[TestedType(typeof(NonPersistentDepartureContainerPivotCollection))]
	class NonPersistentContainerPivotCollectionInBillTest : NonPersistentBusinessObjectCollectionTestCase<NonPersistentDepartureContainerPivotCollection>
	{
		public void TestLoadCollection()
		{
			var headerContainer = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer.BC_ContainerNum = "AAA";
			var headerContainer2 = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer2.BC_ContainerNum = "BBB";

			AssertEquals(2, GetCollectionToTest().Count);
		}

		protected override NonPersistentDepartureContainerPivotCollection GetCollectionToTest() => new NonPersistentDepartureContainerPivotCollection(nctsBill.GoodsItems.AddNew());

		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection() => new NonPersistentDepartureContainerPivot(nctsBill.GoodsItems.AddNew());

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsBill = nctsHeader.Bills.AddNew();
		}
		NctsHeader nctsHeader;
		NctsBill nctsBill;
	}
}
