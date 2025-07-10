using System.Linq;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackageWrapperForLoadManifest))]
	internal class PackageWrapperForLoadManifestTest : PackageWrapperFromPkgPackageTest
	{
		#region TestPackedItems

		protected override void TestPackedItemsCore()
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
			//      1x Box B5

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var order1OuterPackWith0 = PackingHelper.CreatePackage(packageJob1, "B1", 1, Constants.PkgUnit.Box);

			var order1OuterPackWith1 = PackingHelper.CreatePackage(packageJob1, "B2", 1, Constants.PkgUnit.Box);
			var order1OuterPackedItem1 = order1OuterPackWith1.Pack(order1Line1.ReleaseLines[0], 1m).Single();
			var order1OuterPackedItem2 = order1OuterPackWith1.Pack(order1Line1.ReleaseLines[0], 1m).Single();

			var order1OuterPackWith3 = PackingHelper.CreatePackage(packageJob1, "B3", 1, Constants.PkgUnit.Box);
			order1OuterPackWith3.Pack(order1Line2.ReleaseLines[0], 1);
			order1OuterPackWith3.Pack(order1Line3.ReleaseLines[0], 1);
			order1OuterPackWith3.Pack(order1Line4.ReleaseLines[0], 1);

			var order1InnerPackWith4 = PackingHelper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			var order1InnerPackedItem1 = order1InnerPackWith4.Pack_ForTesting(order1Line1.ReleaseLines[0], 1);
			var order1InnerPackedItem2 = order1InnerPackWith4.Pack_ForTesting(order1Line1.ReleaseLines[0], 1);
			var order1InnerPackedItem3 = order1InnerPackWith4.Pack_ForTesting(order1Line2.ReleaseLines[0], 1);
			var order1InnerPackedItem4 = order1InnerPackWith4.Pack_ForTesting(order1Line3.ReleaseLines[0], 1);
			var order1InnerPackedItem5 = order1InnerPackWith4.Pack_ForTesting(order1Line4.ReleaseLines[0], 1);

			var order1InnerPackWith1 = PackingHelper.CreatePackage(order1OuterPackWith3, 1, Constants.PkgUnit.Box, "B5");
			order1InnerPackWith1.Pack(order1Line2.ReleaseLines[0], 1);
			order1InnerPackWith1.Pack(order1Line2.ReleaseLines[0], 1);

			// pack an empty inner to test 'HasSingleProduct'
			PackingHelper.CreatePackage(order1InnerPackWith1, 1, Constants.PkgUnit.Box, "B6");
			Factory.Save();

			#endregion

			var wrapper1 = new PackageWrapperForLoadManifest(order1OuterPackWith0, Factory);
			AssertEquals(nameof(wrapper1.HasPackedItem), false, wrapper1.HasPackedItem);
			AssertEquals("There are 0 units Packed.", 0, wrapper1.PackedItemCount);

			var wrapper2 = new PackageWrapperForLoadManifest(order1OuterPackWith1, Factory);
			AssertEquals(nameof(wrapper2.HasPackedItem), true, wrapper2.HasPackedItem);
			AssertCollectionContains(data.Part1.OP_Desc, wrapper2.PackedItems.Select(p => ((PackedItemWrapper)p).Description).ToList());
			AssertEquals("There should be 1 wrappers.", 1, wrapper2.PackedItems.Count);
			AssertEquals(nameof(wrapper2.IsExclusive), true, wrapper2.IsExclusive);
			AssertEquals("Has 4 Products, so HasSingleProduct should be false.", false, wrapper2.HasSingleProduct);
			AssertEquals("There are 7 units Packed.", 7, wrapper2.PackedItemCount);

			var wrapper3 = new PackageWrapperForLoadManifest(order1OuterPackWith3, Factory);
			AssertEquals(nameof(wrapper3.HasPackedItem), true, wrapper3.HasPackedItem);
			AssertContainsExactElementsInAnyOrder(new[] { data.Part2.OP_Desc, part3.OP_Desc, part4.OP_Desc, }, wrapper3.PackedItems.Select(p => ((PackedItemWrapper)p).Description).ToList());
			AssertEquals("There should be 3 wrappers.", 3, wrapper3.PackedItems.Count);
			AssertEquals(nameof(wrapper2.IsExclusive), false, wrapper3.IsExclusive);
			AssertEquals("Has 3 Products, so HasSingleProduct should be false.", false, wrapper3.HasSingleProduct);
			AssertEquals("There are 5 units Packed.", 5, wrapper3.PackedItemCount);
		}

		#endregion

		#region Implementation

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new PackageWrapperForLoadManifest(null, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var pallet = Factory.New<PkgPackage>();
			pallet.KP_PackageQty = 1;
			pallet.KP_F3_NKPackType = Constants.PkgUnit.Pallet;
			pallet.KP_PackageID = "GTS2";

			return new PackageWrapperForLoadManifest(pallet, Factory);
		}

		#endregion
	}
}
