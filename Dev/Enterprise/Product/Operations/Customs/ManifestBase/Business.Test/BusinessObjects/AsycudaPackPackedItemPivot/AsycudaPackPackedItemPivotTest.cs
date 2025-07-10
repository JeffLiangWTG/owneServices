using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaPackPackedItemPivot))]
	sealed class AsycudaPackPackedItemPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAPP_APA_Pack()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_AMA = manifestHeader.PK;
			var packedItem = Factory.New<AsycudaPackedItem>();
			packedItem.API_ABL_Bill = bill.PK;
			var pack = Factory.New<AsycudaPack>();
			pack.APA_ABL_Bill = bill.PK;
			var pivot = Factory.New<AsycudaPackPackedItemPivot>();
			pivot.APP_API_Item = packedItem.PK;
			pivot.APP_APA_Pack = pack.PK;
			AssertEquals(0, pivot.APP_ClusterKey);

			Factory.Save();
			AssertEquals("ClusterKey generated onSaving.", 1, pivot.APP_ClusterKey);
		}

		public void TestPack()
		{
			var pivot = Factory.New<AsycudaPackPackedItemPivot>();
			var pack = Factory.New<AsycudaPack>();
			CombineAssertions(() =>
			{
				AssertNull("APP_APA_Pack empty", pivot.Pack);

				pivot.APP_APA_Pack = pack.PK;
				AssertEquals("APP_APA_Pack not empty", pack, pivot.Pack);
			});
		}

		public void TestPackedItem()
		{
			var pivot = Factory.New<AsycudaPackPackedItemPivot>();
			var packedItem = Factory.New<AsycudaPackedItem>();
			CombineAssertions(() =>
			{
				AssertNull("APP_API_Item empty", pivot.PackedItem);

				pivot.APP_API_Item = packedItem.PK;
				AssertEquals("APP_API_Item not empty", packedItem, pivot.PackedItem);
			});
		}

		public void TestPackFromGrid()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ClusterKey = 42;
			var bill = Factory.New<AsycudaBillForTest>();
			header.Bills.Add(bill);

			var packsForBinding = bill.Packs as IBindingList;
			var uncommittedPack = packsForBinding.AddNew() as AsycudaPackForTest;
			uncommittedPack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.One;

			var packedItem = uncommittedPack.PackedItem;
			AssertNotNull("'uncommitted' Pack (of derived type) should be accessible through Pivot", packedItem.Pack);
			AssertEquals("Pack ClusterKey", 42, uncommittedPack.APA_ClusterKey);
			AssertEquals("Item ClusterKey", 42, packedItem.API_ClusterKey);
			AssertEquals("Pivot ClusterKey", 42, packedItem.AsycudaPackPackedItemPivots[0].APP_ClusterKey);
			AssertEquals("Pivots count", 1, packedItem.AsycudaPackPackedItemPivots.Count);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var packedItem = factory.New<AsycudaPackedItem>();
			packedItem.API_ABL_Bill = bill.PK;
			var pivot = factory.New<AsycudaPackPackedItemPivot>();
			pivot.APP_APA_Pack = pack.PK;
			pivot.APP_API_Item = packedItem.PK;
			return pivot;
		}

		public class AsycudaBillForTest : AsycudaBill
		{
			public AsycudaBillForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{ }

			protected override Type GetPackTypeCore() => typeof(AsycudaPackForTest);

			public new AsycudaPackCollectionForTest Packs => (AsycudaPackCollectionForTest)base.Packs;
			protected override IAsycudaPackCollection<AsycudaPack, AsycudaBill> CreateNewAsycudaPackCollection() => new AsycudaPackCollectionForTest(this);
		}

		public class AsycudaPackForTest : AsycudaPack
		{
			public AsycudaPackForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{ }
		}

		public class AsycudaPackCollectionForTest : AsycudaPackCollection<AsycudaPackForTest, AsycudaBillForTest>
		{
			public AsycudaPackCollectionForTest(AsycudaBillForTest master)
				: base(master)
			{ }
		}
	}
}
