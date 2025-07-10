using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	internal class TemporaryStorageLinkPackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckIsLinked()
		{
			var message = "At least one selected package is required.";
			var temporaryStorage = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = temporaryStorage.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var linkPackage = packedItem.TemporaryStorageLinkPackages.AddNew();
			var linkPackage2 = packedItem.TemporaryStorageLinkPackages.AddNew();

			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = 10;
			pack.APA_MarksAndNumbers = "mark";

			var pack2 = bill.Packs.AddNew();
			pack2.APA_PackQty = 20;
			pack2.APA_MarksAndNumbers = "mark2";
			linkPackage.Package = pack;
			linkPackage2.Package = pack2;

			linkPackage.Validation.ValidateAll();
			linkPackage2.Validation.ValidateAll();

			AssertHasRowMessageError("row of 1st linkPackage should have a message error as there are pack but no linkpackage has been selected", linkPackage, message);
			AssertHasRowMessageError("row of 2nd linkPackage should have a message error as there are pack but no linkpackage has been selected", linkPackage2, message);

			linkPackage.IsLinked = true;
			linkPackage.Validation.ValidateAll();
			linkPackage2.Validation.ValidateAll();
			AssertNoRowMessageError("row of 1st linkPackage should have no message error as there are pack and 1st linkpackage has been selected", linkPackage, message);
			AssertNoRowMessageError("row of 2nd linkPackage should have no message error as there are pack and 1st linkpackage has been selected", linkPackage2, message);

			linkPackage.IsLinked = false;
			linkPackage2.IsLinked = true;
			linkPackage.Validation.ValidateAll();
			linkPackage2.Validation.ValidateAll();
			AssertNoRowMessageError("row of 1st linkPackage should have no message error as there are pack and 2nd linkpackage has been selected", linkPackage, message);
			AssertNoRowMessageError("row of 2nd linkPackage should have no message error as there are pack and 2nd linkpackage has been selected", linkPackage2, message);

			linkPackage.IsLinked = true;
			linkPackage2.IsLinked = true;
			linkPackage.Validation.ValidateAll();
			linkPackage2.Validation.ValidateAll();
			AssertNoRowMessageError("row of 1st linkPackage should have no message error as there are pack and both linkpackage have been selected", linkPackage, message);
			AssertNoRowMessageError("row of 2nd linkPackage should have no message error as there are pack and both linkpackage has been selected", linkPackage2, message);
		}
	}
}
