using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NonPersistentContainerPivotPhase5Collection))]
	sealed class NonPersistentContainerPivotPhase5CollectionTests : NonPersistentBusinessObjectCollectionTestCase<NonPersistentContainerPivotPhase5Collection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, GetCollectionToTest().AllowNew);
		}

		public void TestLoadCollection()
		{
			var container = Header.DepartureHeaderContainers.AddNew();
			container.BC_ContainerNum = "AAA";
			var container2 = Header.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "BBB";

			var collection = GetCollectionToTest();
			CombineAssertions(() =>
			{
				AssertEquals("Count", 2, collection.Count);
				AssertEquals("HasChanges", false, collection.HasChanges);
			});
		}

		public void TestContainers_CountChanged()
		{
			var collection = GetCollectionToTest();
			CombineAssertions(() =>
			{
				AssertEquals("Count initial", 0, collection.Count);
				var container = Header.DepartureHeaderContainers.AddNew();
				container.BC_ContainerNum = "AAA";
				AssertEquals("Count increased", 1, collection.Count);
				Header.DepartureHeaderContainers.RemoveAndDelete(container);
				AssertEquals("Count decreased", 0, collection.Count);
			});
		}

		public void TestLoadCollection_Arrival()
		{
			var nctsHeaderArrival = Factory.New<NctsHeader>();
			nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderArrival.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;

			var container = nctsHeaderArrival.ArrivalHeaderContainers.AddNew();
			container.BC_ContainerNum = "AAA";
			var container2 = nctsHeaderArrival.ArrivalHeaderContainers.AddNew();
			container2.BC_ContainerNum = "BBB";
			var container3 = nctsHeaderArrival.ArrivalHeaderContainers.AddNew();
			container3.BC_ContainerNum = "CCC";
			container3.BC_UnloadedState = "MIS";

			var collection = new NonPersistentContainerPivotPhase5Collection(nctsHeaderArrival.Bills.AddNew().ArrivalGoodsItems.AddNew().Packages.AddNew());
			CombineAssertions(() =>
			{
				AssertEquals("Count", 2, collection.Count);
				AssertEquals("HasChanges", false, collection.HasChanges);
			});
		}

		public void TestContainers_CountChanged_Arrival()
		{
			var nctsHeaderArrival = Factory.New<NctsHeader>();
			nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderArrival.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;

			var collection = new NonPersistentContainerPivotPhase5Collection(nctsHeaderArrival.Bills.AddNew().ArrivalGoodsItems.AddNew().Packages.AddNew());
			CombineAssertions(() =>
			{
				AssertEquals("Count initial", 0, collection.Count);
				var container = nctsHeaderArrival.ArrivalHeaderContainers.AddNew();
				container.BC_ContainerNum = "AAA";
				AssertEquals("Count increased", 1, collection.Count);
				nctsHeaderArrival.ArrivalHeaderContainers.Remove(container);
				AssertEquals("Count decreased", 0, collection.Count);
			});
		}

		public void TestContainers_BC_UnloadedState_ValueChanged_Arrival()
		{
			var nctsHeaderArrival = Factory.New<NctsHeader>();
			nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderArrival.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;

			var container1 = nctsHeaderArrival.ArrivalHeaderContainers.AddNew();
			container1.BC_ContainerNum = "AAA";
			container1.BC_UnloadedState = "DEC";

			var container2 = nctsHeaderArrival.ArrivalHeaderContainers.AddNew();
			container2.BC_ContainerNum = "BBB";
			container2.BC_UnloadedState = "MIS";

			var collection = new NonPersistentContainerPivotPhase5Collection(nctsHeaderArrival.Bills.AddNew().ArrivalGoodsItems.AddNew().Packages.AddNew());

			CombineAssertions(() =>
			{
				AssertEquals("Count initial", 1, collection.Count);
				container1.BC_UnloadedState = "MIS";
				AssertEquals("Count decreased", 0, collection.Count);
				var container3 = nctsHeaderArrival.ArrivalHeaderContainers.AddNew();
				container3.BC_ContainerNum = "CCC";
				container3.BC_UnloadedState = "NEW";
				AssertEquals("Count increased", 1, collection.Count);
				container1.BC_UnloadedState = "DEC";
				AssertEquals("Count increased", 2, collection.Count);
				nctsHeaderArrival.ArrivalHeaderContainers.Remove(container3);
				AssertEquals("Count decreased", 1, collection.Count);
				container2.BC_UnloadedState = "DEC";
				AssertEquals("Count increased", 2, collection.Count);
			});
		}

		public override void TestTypedget_Item()
		{
			Assert("Pass", true);
		}

		public override void TestAdd()
		{
			Assert("Pass", true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert("Pass", true);
		}

		protected override NonPersistentContainerPivotPhase5Collection GetCollectionToTest() => new NonPersistentContainerPivotPhase5Collection(Package);

		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection() => throw new NotSupportedException();

		NctsPackage Package
		{
			get
			{
				if (package == null)
				{
					package = Header.Bills.AddNew().GoodsItems.AddNew().Packages.AddNew();
				}
				return package;
			}
		}
		NctsPackage package;

		NctsHeader Header
		{
			get
			{
				if (header == null)
				{
					header = Factory.New<NctsHeader>();
					header.SetMovementType(NctsMovementType.Codes.Departure);
					header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				}
				return header;
			}
		}
		NctsHeader header;
	}
}
