using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill>))]
	sealed class AsycudaBillPackedItemCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAPI_LineNoIsUpdatedWhenItemIsRemoved()
		{
			var bill = Factory.New<AsycudaBill>();
			var packedItems = bill.PackedItems;
			var item1 = packedItems.AddNew();
			var item2 = packedItems.AddNew();
			var item3 = packedItems.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("item1.API_LineNo", (ZShort)1, item1.API_LineNo);
				AssertEquals("item2.API_LineNo", (ZShort)2, item2.API_LineNo);
				AssertEquals("item3.API_LineNo", (ZShort)3, item3.API_LineNo);

				packedItems.RemoveAndDelete(item2);
				AssertEquals("item1.API_LineNo after item2 is removed", (ZShort)1, item1.API_LineNo);
				AssertEquals("item3.API_LineNo after item2 is removed", (ZShort)2, item3.API_LineNo);
			});
		}

		public void TestSequenceNumberCalculator()
		{
			var bill = Factory.New<AsycudaBill>();
			var packedItems = bill.PackedItems;
			AssertType<HugeSequenceNumberGenerator>(packedItems.SequenceNumberCalculator);
		}

		protected override Type GetExpectedCollectionType() => typeof(AsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill>);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var bill = Factory.New<AsycudaBill>();
			return (BusinessObjectCollection)bill.PackedItems;
		}
	}
}
