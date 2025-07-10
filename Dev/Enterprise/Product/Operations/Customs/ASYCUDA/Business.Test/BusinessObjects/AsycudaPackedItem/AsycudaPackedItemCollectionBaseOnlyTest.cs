using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItemCollection<AsycudaPackedItem, AsycudaPack>))]
	sealed class AsycudaPackedItemCollectionBaseOnlyTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ClusterKey = 1;
			var pack = bill.Packs.AddNew();
			var packedItems = pack.PackedItemsForBinding;
			var packedItem = packedItems.AddNew();
			var pivot = (AsycudaPackPackedItemPivot)pack.PackedItems.Single();

			CombineAssertions(() =>
			{
				AssertEquals("API_ABL_Bill", pack.APA_ABL_Bill, packedItem.API_ABL_Bill);
				AssertEquals("API_ClusterKey", pack.APA_ClusterKey, packedItem.API_ClusterKey);
				AssertEquals("Pivot APP_API_Item", packedItem.PK, pivot.APP_API_Item);
				AssertEquals("Pivot APP_APA_Pack", pack.PK, pivot.APP_APA_Pack);
			});
		}

		public void TestAPI_LineNoIsUpdatedWhenItemIsAddedOrRemoved()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();
			var packedItems = pack.PackedItemsForBinding;
			var item1 = packedItems.AddNew();
			var item2 = packedItems.AddNew();
			var item3 = packedItems.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("item1.API_LineNo", (ZShort)1, item1.API_LineNo);
				AssertEquals("item2.API_LineNo", (ZShort)2, item2.API_LineNo);
				AssertEquals("item3.API_LineNo", (ZShort)3, item3.API_LineNo);

				packedItems.RemoveFromRelationship(item2);
				AssertEquals("item1.API_LineNo after item1 is removed", (ZShort)1, item1.API_LineNo);
				AssertEquals("item3.API_LineNo after item1 is removed", (ZShort)2, item3.API_LineNo);
			});
		}

		public void TestRemovedItemAlsoRmovesFromBill()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemsForBinding.AddNew();
			bill.PackedItems.Load();

			CombineAssertions("Removing PackedItem from Pack should also remove the target from Bill PackedItems as well, without exception throw.", () =>
			{
				AssertEquals("To make sure the PackedItem form Pack also contained in Bill PackedItem.", true, bill.PackedItems.Contains(packedItem));
				AssertNoExceptionThrown("No exception", () => bill.Packs.RemoveAndDelete(pack));
				AssertEquals("Should have been remove from Bill PackedItems.", false, bill.PackedItems.Contains(packedItem));
			});
		}

		protected override Type GetExpectedCollectionType() => typeof(AsycudaPackedItemCollection<AsycudaPackedItem, AsycudaPack>);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();
			return new AsycudaPackedItemCollection<AsycudaPackedItem, AsycudaPack>(pack);
		}

		public void TestOnRemoved()
		{
			var pack = Factory.New<AsycudaPack>();
			var packedItem = pack.PackedItemsForBinding.AddNew();

			packedItem.Delete();

			AssertNoExceptionThrown("No Exception on remove without bill", () => packedItem.Delete());
		}
	}
}
