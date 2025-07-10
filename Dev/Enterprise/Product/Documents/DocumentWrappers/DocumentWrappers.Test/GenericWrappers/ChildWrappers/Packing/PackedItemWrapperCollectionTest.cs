using System.Linq;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackedItemWrapperCollection))]
	sealed class PackedItemWrapperCollectionTest : GenericWrapperCollectionTest<PackedItemWrapperCollection>
	{
		#region TestPackedItems

		public void TestPackedItems()
		{
			#region SetUpData
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = helper.CreateProduct(data.Org1, "part3");
			var part4 = helper.CreateProduct(data.Org1, "part4");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "r1", new TestNotificationBuffer());
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			helper.CreateWhsReceiveInventoryLine(receive, part3, 10m);
			helper.CreateWhsReceiveInventoryLine(receive, part4, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			AssertEquals("Check receive is finalised", true, receive.IsFinalised);
			var order1 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order1Line1 = helper.CreateWhsOrderLine(order1, data.Part1, 5);
			var order1Line2 = helper.CreateWhsOrderLine(order1, data.Part2, 5);
			var order1Line3 = helper.CreateWhsOrderLine(order1, part3, 5);
			var order1Line4 = helper.CreateWhsOrderLine(order1, part4, 5);
			Factory.Save();

			var pick = helper.CreatePickNew(order1);

			// Package Job 1
			//    1x Box B1
			//    1x Box B2
			//       Part1
			//		 1x Box B4
			//          Part1
			//          Part2
			//          Part3
			//          Part4
			//    1x Box B3
			//       Part2
			//       Part3
			//       Part4

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var order1OuterPackWith0 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B1");

			var order1OuterPackWith1 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B2");
			order1OuterPackWith1.Pack(order1Line1.ReleaseLines[0], 1m);
			order1OuterPackWith1.Pack(order1Line1.ReleaseLines[0], 1m);

			var order1OuterPackWith3 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B3");
			order1OuterPackWith3.Pack(order1Line2.ReleaseLines[0], 1);
			order1OuterPackWith3.Pack(order1Line3.ReleaseLines[0], 1);
			order1OuterPackWith3.Pack(order1Line4.ReleaseLines[0], 1);

			var order1InnerPackWith4 = PackingHelper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			var order1InnerPackedItem1 = order1InnerPackWith4.Pack_ForTesting(order1Line1.ReleaseLines[0], 1);
			var order1InnerPackedItem2 = order1InnerPackWith4.Pack_ForTesting(order1Line2.ReleaseLines[0], 1);
			var order1InnerPackedItem3 = order1InnerPackWith4.Pack_ForTesting(order1Line2.ReleaseLines[0], 1);
			var order1InnerPackedItem4 = order1InnerPackWith4.Pack_ForTesting(order1Line3.ReleaseLines[0], 1);
			var order1InnerPackedItem5 = order1InnerPackWith4.Pack_ForTesting(order1Line4.ReleaseLines[0], 1);
			#endregion

			var collection = new PackedItemWrapperCollection(new[] { order1InnerPackedItem2, order1InnerPackedItem3, order1InnerPackedItem4 }, Factory);
			var wrappedPackableItemParents = collection.Cast<PackedItemWrapper>().Select(w => (IPackableItemParent)w.WrappedObject);
			AssertContainsExactElementsInAnyOrder(new[] { order1Line2.ReleaseLines[0], order1Line3.ReleaseLines[0] }, wrappedPackableItemParents);

			var wrapper1 = collection.Cast<PackedItemWrapper>().Single(w => w.WrappedObject == order1Line2.ReleaseLines[0]);
			var wrapper2 = collection.Cast<PackedItemWrapper>().Single(w => w.WrappedObject == order1Line3.ReleaseLines[0]);
			AssertContainsExactElementsInAnyOrder(new[] { order1InnerPackedItem2, order1InnerPackedItem3 }, wrapper1.PackedItems);
			AssertContainsExactElementsInAnyOrder(new[] { order1InnerPackedItem4 }, wrapper2.PackedItems);
		}

		#endregion

		#region PackingHelper

		PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;

		#endregion

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return PackedItemWrapper.New(null, Factory);
		}

		protected override PackedItemWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new PackedItemWrapperCollection(Factory);
		}
	}
}
