using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageLinkPackageCollection<>))]
sealed class TemporaryStorageLinkPackageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TemporaryStorageLinkPackageCollection<TemporaryStorageLinkPackage>>
{
	public void TestHasMismatchingPackUQLinkedRows()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();
		var packedItem = bill.PackedItems.AddNew();

		var pack1 = bill.Packs.AddNew();
		pack1.APA_PackUQ = "BOX";

		var pack2 = bill.Packs.AddNew();
		pack2.APA_PackUQ = "CRT";

		var collection = new TemporaryStorageLinkPackageCollection<TemporaryStorageLinkPackage>(packedItem);
		collection.Load();

		var item1 = collection[0];
		var item2 = collection[1];

		item1.IsLinked = true;
		item2.IsLinked = true;
		AssertEquals("Both linked with different APA_PackUQ should result in mismatch", true, collection.HasMismatchingPackUQLinkedRows);

		item1.IsLinked = true;
		item2.IsLinked = false;
		AssertEquals("Only item1 linked should not result in mismatch", false, collection.HasMismatchingPackUQLinkedRows);

		item1.IsLinked = false;
		item2.IsLinked = true;
		AssertEquals("Only item2 linked should not result in mismatch", false, collection.HasMismatchingPackUQLinkedRows);

		item1.IsLinked = false;
		item2.IsLinked = false;
		AssertEquals("Neither item linked should not result in mismatch", false, collection.HasMismatchingPackUQLinkedRows);
	}

	protected override TemporaryStorageLinkPackageCollection<TemporaryStorageLinkPackage> GetCollectionToTest()
	{
		return new TemporaryStorageLinkPackageCollection<TemporaryStorageLinkPackage>(packedItem);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection() => new TemporaryStorageLinkPackage(packedItem);

	TemporaryStoragePackedItem packedItem => Factory.NewWithValidTestData<TemporaryStoragePackedItem>();
}
