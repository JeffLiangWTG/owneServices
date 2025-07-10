using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	public class AsycudaLinkPackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckIsLinked()
		{
			var messageSelectedPackageIsRequired = "A selected package is required.";
			var messageMoreThanOneSelectedPackageIsProhibited = "Only one package can be linked to a Bill Item.";
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var linkPackage = packedItem.AsycudaLinkPackages.AddNew();
			var linkPackage2 = packedItem.AsycudaLinkPackages.AddNew();

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

			AssertHasRowMessageError("row of 1st linkPackage should have a message error as there are pack but no linkpackage has been selected", linkPackage, messageSelectedPackageIsRequired);
			AssertHasRowMessageError("row of 2nd linkPackage should have a message error as there are pack but no linkpackage has been selected", linkPackage2, messageSelectedPackageIsRequired);
			AssertNoRowMessageError("row of 1st linkPackage should have no message error as there is no more than 1 pack has been selected", linkPackage, messageMoreThanOneSelectedPackageIsProhibited);
			AssertNoRowMessageError("row of 2nd linkPackage should have no message error as there is no more than 1 pack has been selected", linkPackage2, messageMoreThanOneSelectedPackageIsProhibited);

			linkPackage.IsLinked = true;
			linkPackage.Validation.ValidateAll();
			linkPackage2.Validation.ValidateAll();
			AssertNoRowMessageError("row of 1st linkPackage should have no message error as there are pack and 1st linkpackage has been selected", linkPackage, messageSelectedPackageIsRequired);
			AssertNoRowMessageError("row of 2nd linkPackage should have no message error as there are pack and 1st linkpackage has been selected", linkPackage2, messageSelectedPackageIsRequired);
			AssertNoRowMessageError("row of 1st linkPackage should have no message error as there is no more than 1 pack has been selected", linkPackage, messageMoreThanOneSelectedPackageIsProhibited);
			AssertNoRowMessageError("row of 2nd linkPackage should have no message error as there is no more than 1 pack has been selected", linkPackage2, messageMoreThanOneSelectedPackageIsProhibited);

			linkPackage.IsLinked = false;
			linkPackage2.IsLinked = true;
			linkPackage.Validation.ValidateAll();
			linkPackage2.Validation.ValidateAll();
			AssertNoRowMessageError("row of 1st linkPackage should have no message error as there are pack and 2nd linkpackage has been selected", linkPackage, messageSelectedPackageIsRequired);
			AssertNoRowMessageError("row of 2nd linkPackage should have no message error as there are pack and 2nd linkpackage has been selected", linkPackage2, messageSelectedPackageIsRequired);
			AssertNoRowMessageError("row of 1st linkPackage should have no message error as there is no more than 1 pack has been selected", linkPackage, messageMoreThanOneSelectedPackageIsProhibited);
			AssertNoRowMessageError("row of 2nd linkPackage should have no message error as there is no more than 1 pack has been selected", linkPackage2, messageMoreThanOneSelectedPackageIsProhibited);

			linkPackage.IsLinked = true;
			linkPackage2.IsLinked = true;
			linkPackage.Validation.ValidateAll();
			linkPackage2.Validation.ValidateAll();
			AssertNoRowMessageError("row of 1st linkPackage should have no message error as there are pack and 2nd linkpackage has been selected", linkPackage, messageSelectedPackageIsRequired);
			AssertNoRowMessageError("row of 2nd linkPackage should have no message error as there are pack and 2nd linkpackage has been selected", linkPackage2, messageSelectedPackageIsRequired);
			AssertHasRowMessageError("row of 1st linkPackage should have a message error as there are more than 1 pack and have been selected", linkPackage, messageMoreThanOneSelectedPackageIsProhibited);
			AssertHasRowMessageError("row of 2nd linkPackage should have a message error as there are more than 1 pack and have been selected", linkPackage2, messageMoreThanOneSelectedPackageIsProhibited);
		}
	}
}
