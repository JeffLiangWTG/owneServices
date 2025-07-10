using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Test
{
	[TestedType(typeof(AsycudaPackPackedItemLink))]
	public class AsycudaPackPackedItemLinkTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPackage()
		{
			var linkPackage = (AsycudaPackPackedItemLink)GetNewBusinessObject();
			AssertEquals("Package should equal Pack entered via the setter.", pack, linkPackage.Package);
		}

		public void TestPackageNumber()
		{
			var linkPackage = (AsycudaPackPackedItemLink)GetNewBusinessObject();

			linkPackage.Package.APA_PackUQ = "UQ";
			linkPackage.Package.APA_MarksAndNumbers = ZString.Empty;
			AssertEquals("PackageNumber should be APA_PackUQ", "UQ", linkPackage.PackageNumber);

			linkPackage.Package.APA_PackUQ = ZString.Empty;
			linkPackage.Package.APA_MarksAndNumbers = "marks";
			AssertEquals("PackageNumber should be marked [APA_MarksAndNumbers]", "marked marks", linkPackage.PackageNumber);

			linkPackage.Package.APA_PackUQ = "UQ";
			linkPackage.Package.APA_MarksAndNumbers = "marks";
			AssertEquals("PackageNumber should be [APA_PackUQ value] marked [APA_MarksAndNumbers value]", "UQ marked marks", linkPackage.PackageNumber);

			linkPackage.Package = null;
			AssertEquals("PackageNumber should be empty", ZString.Empty, linkPackage.PackageNumber);
		}

		public void TestPackQty()
		{
			var linkPackage = (AsycudaPackPackedItemLink)GetNewBusinessObject();

			AssertEquals("PackQty should be default value.", 0, linkPackage.PackQty);

			linkPackage.IsLinked = true;
			AssertEquals("PackQty should be APA_PackQty.", 10, linkPackage.PackQty);

			var packedItem2 = bill.PackedItems.AddNew();
			linkPackage.IsLinked = true;
			packedItem.API_LineNo = 2;
			packedItem2.API_LineNo = 1;
			var linkPackage2 = packedItem2.AsycudaPackPackedItemLinks.AddNew();
			linkPackage2.Package = pack;
			linkPackage2.IsLinked = true;

			AssertEquals("PackQty should be be default value.", 0, linkPackage.PackQty);
		}

		public void TestIsLinked()
		{
			var linkPackage = (AsycudaPackPackedItemLink)GetNewBusinessObject();
			AssertEquals("Prerequisite:", 0, packedItem.PackagesPivot.Count);
			Assert(!linkPackage.IsLinked);
			linkPackage.IsLinked = true;
			AssertEquals("A pivot should have been created.", 1, packedItem.PackagesPivot.Count);
			Assert(linkPackage.IsLinked);
		}

		public void TestCaption()
		{
			var linkPackage = (AsycudaPackPackedItemLink)GetNewBusinessObject();
			CombineAssertions("Check all captions to be correct", () =>
				{
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(linkPackage.IsLinkedInfo, multipleResourceKeys: null, "Is Linked?", "Is Linked?", "Is Linked?", "Indicates whether the item is a stand-alone item or linked to a pack.");
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(linkPackage.PackQtyInfo, multipleResourceKeys: null, "Quantity", "Qty.", "Qty.", "Item quantity within linked pack.");
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(linkPackage.PackageNumberInfo, multipleResourceKeys: null, "Package Number", "Package No.", "Package No.", "Package number of linked pack.");
				}
			);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			bill = manifestHeader.Bills.AddNew();
			packedItem = bill.PackedItems.AddNew();
			var linkPackage = packedItem.AsycudaPackPackedItemLinks.AddNew();
			pack = bill.Packs.AddNew();
			pack.APA_PackQty = 10;
			pack.APA_MarksAndNumbers = "mark";
			linkPackage.Package = pack;
			return linkPackage;
		}

		AsycudaPack pack;
		AsycudaPackedItem packedItem;
		AsycudaBill bill;
	}
}
