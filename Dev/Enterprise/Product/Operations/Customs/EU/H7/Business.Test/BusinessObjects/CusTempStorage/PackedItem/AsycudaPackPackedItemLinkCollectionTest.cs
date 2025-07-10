using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Test
{
	[TestedType(typeof(AsycudaPackPackedItemLinkCollection<>))]
	public class AsycudaPackPackedItemLinkCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AsycudaPackPackedItemLinkCollection<AsycudaPackPackedItemLink>>
	{
		public void TestHasAtLeastOneLinkedRecord()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			bill.Packs.AddNew();
			bill.Packs.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var collection = packedItem.AsycudaPackPackedItemLinks;
			var item = collection[1];
			AssertEquals(false, collection.HasAtLeastOneLinkedRecord);
			item.IsLinked = true;
			AssertEquals(true, collection.HasAtLeastOneLinkedRecord);
		}

		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowRemove);
		}

		public void TestLoad()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			var asycudaPackPackedItemLinkCollection = new AsycudaPackPackedItemLinkCollection<AsycudaPackPackedItemLink>(packedItem);
			asycudaPackPackedItemLinkCollection.Load();

			AssertEquals("There should be no link package as there is no pack", 0, asycudaPackPackedItemLinkCollection.Count);

			var pack = bill.Packs.AddNew();
			asycudaPackPackedItemLinkCollection.Load();
			AssertEquals("There should be 1 link package as there is 1 pack", 1, asycudaPackPackedItemLinkCollection.Count);
			Assert("There should be 1 link package link to pack", asycudaPackPackedItemLinkCollection.Cast<AsycudaPackPackedItemLink>().Any(x => x.Package.PK == pack.PK));

			var pack2 = bill.Packs.AddNew();
			AssertEquals("Sync should be done thus there should be 2 link packages as there are 2 packs", 2, asycudaPackPackedItemLinkCollection.Count);
			Assert("There should be 1 link package link to pack2", asycudaPackPackedItemLinkCollection.Cast<AsycudaPackPackedItemLink>().Any(x => x.Package.PK == pack2.PK));

			bill.Packs.RemoveAndDeleteAll();
			AssertEquals("There should be 0 link package as there is no more pack", 0, asycudaPackPackedItemLinkCollection.Count);
		}

		protected override AsycudaPackPackedItemLinkCollection<AsycudaPackPackedItemLink> GetCollectionToTest()
		{
			return new AsycudaPackPackedItemLinkCollection<AsycudaPackPackedItemLink>(packedItem);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new AsycudaPackPackedItemLink(packedItem);

		AsycudaPackedItem packedItem
		{
			get
			{
				var manifestHeader = Factory.New<AsycudaManifestHeader>();
				var bill = manifestHeader.Bills.AddNew();
				return bill.PackedItems.AddNew();
			}
		}
	}
}
