using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageLinkPackageCollection<>))]
	internal class TemporaryStorageLinkPackageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TemporaryStorageLinkPackageCollection<TemporaryStorageLinkPackage>>
	{
		public void TestHasAtLeastOneLinkedRecord()
		{
			var temporaryStorage = Factory.New<TemporaryStorageHeader>();
			var bill = temporaryStorage.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			var pack2 = bill.Packs.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var collection = packedItem.TemporaryStorageLinkPackages;
			var item1 = collection[0];
			var item2 = collection[1];
			AssertEquals("No data", false, collection.HasAtLeastOneLinkedRecord);
			item2.IsLinked = true;
			AssertEquals("HasAtLeastOneLinkedRecord", true, collection.HasAtLeastOneLinkedRecord);
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
			var temporaryStorage = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = temporaryStorage.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			var temporaryStorageLinkPackageCollection = new TemporaryStorageLinkPackageCollection<TemporaryStorageLinkPackage>(packedItem);
			temporaryStorageLinkPackageCollection.Load();

			AssertEquals("There should be no link package as there is no pack", 0, temporaryStorageLinkPackageCollection.Count);

			var pack = bill.Packs.AddNew();
			temporaryStorageLinkPackageCollection.Load();
			AssertEquals("There should be 1 link package as there is 1 pack", 1, temporaryStorageLinkPackageCollection.Count);
			Assert("There should be 1 link package link to pack", temporaryStorageLinkPackageCollection.Cast<TemporaryStorageLinkPackage>().Any(x => x.Package.PK == pack.PK));

			var pack2 = bill.Packs.AddNew();
			AssertEquals("Sync should be done thus there should be 2 link packages as there are 2 packs", 2, temporaryStorageLinkPackageCollection.Count);
			Assert("There should be 1 link package link to pack2", temporaryStorageLinkPackageCollection.Cast<TemporaryStorageLinkPackage>().Any(x => x.Package.PK == pack2.PK));

			bill.Packs.RemoveAndDeleteAll();
			AssertEquals("There should be 0 link package as there is no more pack", 0, temporaryStorageLinkPackageCollection.Count);
		}
		protected override TemporaryStorageLinkPackageCollection<TemporaryStorageLinkPackage> GetCollectionToTest()
		{
			return new TemporaryStorageLinkPackageCollection<TemporaryStorageLinkPackage>(packedItem);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new TemporaryStorageLinkPackage(packedItem);

		TemporaryStoragePackedItem packedItem => Factory.NewWithValidTestData<TemporaryStoragePackedItem>();
	}
}
