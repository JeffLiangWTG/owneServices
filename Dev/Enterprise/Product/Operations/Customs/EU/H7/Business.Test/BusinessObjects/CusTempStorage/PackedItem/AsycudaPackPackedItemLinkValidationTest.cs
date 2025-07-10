using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.H7.Business.Test
{
	public class AsycudaPackPackedItemLinkValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckIsLinked()
		{
			var message = "At least one selected package is required.";
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var linkPackage1 = packedItem.AsycudaPackPackedItemLinks.AddNew();
			var linkPackage2 = packedItem.AsycudaPackPackedItemLinks.AddNew();

			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = 10;
			pack.APA_MarksAndNumbers = "mark";

			var pack2 = bill.Packs.AddNew();
			pack2.APA_PackQty = 20;
			pack2.APA_MarksAndNumbers = "mark2";

			linkPackage1.Package = pack;
			linkPackage2.Package = pack2;
			linkPackage1.Validation.ValidateAll();
			linkPackage2.Validation.ValidateAll();

			AssertHasRowMessageError("linkPackage1 should have a message error as there are pack but no linkpackage has been selected", linkPackage1, message);
			AssertHasRowMessageError("linkPackage2 should have a message error as there are pack but no linkpackage has been selected", linkPackage2, message);

			linkPackage1.IsLinked = true;
			linkPackage1.Validation.ValidateAll();
			linkPackage2.Validation.ValidateAll();
			AssertNoRowMessageError("linkPackage1 should have no message error as there are pack and linkPackage1 has been selected", linkPackage1, message);
			AssertNoRowMessageError("linkPackage2 should have no message error as there are pack and linkPackage1 has been selected", linkPackage2, message);

			linkPackage1.IsLinked = false;
			linkPackage2.IsLinked = true;
			linkPackage1.Validation.ValidateAll();
			linkPackage2.Validation.ValidateAll();
			AssertNoRowMessageError("linkPackage1 should have no message error as there are pack and linkPackage2 has been selected", linkPackage1, message);
			AssertNoRowMessageError("linkPackage2 should have no message error as there are pack and linkPackage2 has been selected", linkPackage2, message);

			linkPackage1.IsLinked = true;
			linkPackage2.IsLinked = true;
			linkPackage1.Validation.ValidateAll();
			linkPackage2.Validation.ValidateAll();
			AssertNoRowMessageError("linkPackage1 should have no message error as there are pack and both linkpackage have been selected", linkPackage1, message);
			AssertNoRowMessageError("linkPackage2 should have no message error as there are pack and both linkpackage have been selected", linkPackage2, message);
		}
	}
}
