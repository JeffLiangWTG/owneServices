using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStorageLinkPackageValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckIsLinked_PackUnit()
	{
		var message = "Only rows with the same Pack Unit are allowed to be selected.";
		var temporaryStorage = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		var bill = temporaryStorage.Bills.AddNew();
		var packedItem = bill.PackedItems.AddNew();

		var pack1 = bill.Packs.AddNew();
		pack1.APA_PackQty = 10;
		pack1.APA_MarksAndNumbers = "mark";
		pack1.APA_PackUQ = "UQ1";

		var pack2 = bill.Packs.AddNew();
		pack2.APA_PackQty = 5;
		pack2.APA_MarksAndNumbers = "mark2";
		pack2.APA_PackUQ = "UQ2";

		var linkPackage1 = packedItem.TemporaryStorageLinkPackages[0];
		var linkPackage2 = packedItem.TemporaryStorageLinkPackages[1];

		linkPackage1.IsLinked = true;
		linkPackage1.Validation.ValidateAll();
		linkPackage2.Validation.ValidateAll();
		AssertNoRowMessageError(linkPackage1, message);
		AssertNoRowMessageError(linkPackage2, message);

		linkPackage1.IsLinked = true;
		linkPackage2.IsLinked = true;
		linkPackage1.Validation.ValidateAll();
		linkPackage2.Validation.ValidateAll();
		AssertHasRowError(linkPackage1, message);
		AssertHasRowError(linkPackage2, message);

		linkPackage1.IsLinked = true;
		linkPackage2.IsLinked = false;
		linkPackage1.Validation.ValidateAll();
		linkPackage2.Validation.ValidateAll();
		AssertNoRowError(linkPackage1, message);
		AssertNoRowError(linkPackage2, message);
	}
}
