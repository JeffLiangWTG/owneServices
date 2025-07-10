using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageLinkPackage))]
sealed class TemporaryStorageLinkPackageTest : NonPersistentBusinessObjectTestCase
{
	public void TestValidationType()
	{
		var linkPackage = (TemporaryStorageLinkPackage)GetNewBusinessObject();
		AssertType<TemporaryStorageLinkPackageValidation>(linkPackage.Validation);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var temporaryStorage = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		var bill = temporaryStorage.Bills.AddNew();
		packedItem = bill.PackedItems.AddNew();
		var linkPackage = packedItem.TemporaryStorageLinkPackages.AddNew();
		pack = (TemporaryStoragePack)bill.Packs.AddNew();
		pack.APA_PackQty = 10;
		pack.APA_MarksAndNumbers = "mark";
		linkPackage.Package = pack;
		return linkPackage;
	}

	TemporaryStoragePack pack;
	TemporaryStoragePackedItem packedItem;
}
