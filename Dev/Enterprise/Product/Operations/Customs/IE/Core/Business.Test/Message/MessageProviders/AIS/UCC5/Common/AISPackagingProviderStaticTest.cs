using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	class AISPackagingProviderStaticTest : TestCaseWithFactory
	{
		public void TestCollectionFromEntryLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			entryLine2.CL_LineNumber = 2;

			var pkgVG = declaration.Packages.AddNew();
			pkgVG.CW_PackType = "VG";
			pkgVG.CW_MarksAndNos = "VGMark";
			var pkgNE = declaration.Packages.AddNew();
			pkgNE.CW_PackType = "NE";
			pkgNE.CW_MarksAndNos = "NEMark";
			pkgNE.CW_PackQty = 10;
			var pkg1A = declaration.Packages.AddNew();
			pkg1A.CW_PackType = "1A";
			pkg1A.CW_MarksAndNos = "1AMark";
			pkg1A.CW_PackQty = 20;

			var line1PackLinks = invoiceLine1.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().ToList();
			var vgLink1 = line1PackLinks.First(link => link.Package == pkgVG);
			vgLink1.IsLinked = true;
			var neLink1 = line1PackLinks.First(link => link.Package == pkgNE);
			neLink1.IsLinked = true;
			neLink1.Quantity = 9;

			var line2PackLinks = invoiceLine2.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().ToList();
			var neLink2 = line2PackLinks.First(link => link.Package == pkgNE);
			neLink2.IsLinked = true;
			neLink2.Quantity = 1;
			var otherLink2 = line2PackLinks.First(link => link.Package == pkg1A);
			otherLink2.IsLinked = true;
			otherLink2.Quantity = 1;

			var collection1 = AISPackagingProvider.GetCollection(entryLine1);
			CombineAssertions("Item 1 packagings.", () =>
			{
				AssertEquals("Count of collection: all linked packs.", 2, collection1.Count);
				var vgPackaging = collection1.FirstOrDefault(x => x.PackageType == "VG");
				AssertNotNull("VG packaging should exist.", vgPackaging);
				AssertNull("PackageQuantity should be null for VG(BULK) packaging.", vgPackaging.PackageQuantity);
				AssertEquals("ShippingMarks should be from linked Package.", "VGMark", vgPackaging.ShippingMarks);

				var nePackaging = collection1.FirstOrDefault(x => x.PackageType == "NE");
				AssertNotNull("NE packaging should exist.", nePackaging);
				AssertEquals("PackageQuantity should be CW_PackQty for VG(non-BULK) packaging.", 10, nePackaging.PackageQuantity);
				AssertEquals("ShippingMarks should be from linked Package.", "NEMark", nePackaging.ShippingMarks);
			});

			var collection2 = AISPackagingProvider.GetCollection(entryLine2);
			CombineAssertions("Item 2 packagings.", () =>
			{
				AssertEquals("Count of collection: all linked packs.", 2, collection2.Count);

				var nePackaging = collection2.FirstOrDefault(x => x.PackageType == "NE");
				AssertNotNull("NE packaging should exist.", nePackaging);
				AssertEquals("PackageQuantity 0 as the previous item has the Quantity.", 0, nePackaging.PackageQuantity);
				AssertEquals("ShippingMarks should be from linked Package.", "NEMark", nePackaging.ShippingMarks);

				var otherPackaging = collection2.FirstOrDefault(x => x.PackageType == "1A");
				AssertNotNull("1A packaging should exist.", otherPackaging);
				AssertEquals("PackageQuantity should be CW_PackQty for other packaging.", 20, otherPackaging.PackageQuantity);
				AssertEquals("ShippingMarks should be from linked Package.", "1AMark", otherPackaging.ShippingMarks);
			});
		}

		public void TestCollectionFromTemporaryStorage()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();

			var packageVG = bill.Packs.AddNew();
			packageVG.APA_PackUQ = "VG";
			packageVG.APA_MarksAndNumbers = "VGMark";

			var packageNE = bill.Packs.AddNew();
			packageNE.APA_PackUQ = "NE";
			packageNE.APA_PackQty = 10;
			packageNE.APA_MarksAndNumbers = "NEMark";

			var package1A = bill.Packs.AddNew();
			package1A.APA_PackUQ = "1A";
			package1A.APA_PackQty = 20;
			package1A.APA_MarksAndNumbers = "1AMark";

			var packedItem1 = bill.PackedItems.AddNew();
			packedItem1.API_LineNo = 1;
			var linkNE1 = packedItem1.TemporaryStorageLinkPackages.FirstOrDefault(link => link.Package == packageNE);
			linkNE1.IsLinked = true;
			var link1A1 = packedItem1.TemporaryStorageLinkPackages.FirstOrDefault(link => link.Package == package1A);
			link1A1.IsLinked = true;

			var packedItem2 = bill.PackedItems.AddNew();
			packedItem2.API_LineNo = 2;
			var linkVG2 = packedItem2.TemporaryStorageLinkPackages.FirstOrDefault(link => link.Package == packageVG);
			linkVG2.IsLinked = true;
			var linkNE2 = packedItem2.TemporaryStorageLinkPackages.FirstOrDefault(link => link.Package == packageNE);
			linkNE2.IsLinked = true;

			var collection1 = AISPackagingProvider.GetCollection(packedItem1);
			CombineAssertions("Item 1 packagings.", () =>
			{
				AssertEquals("Count of collection: all linked packs.", 2, collection1.Count);

				var nePackaging = collection1.FirstOrDefault(x => x.PackageType == "NE");
				AssertNotNull("NE packaging should exist.", nePackaging);
				AssertEquals("PackageQuantity should be APA_PackQty for NE packaging.", 10, nePackaging.PackageQuantity);
				AssertEquals("ShippingMarks should be from linked Package.", "NEMark", nePackaging.ShippingMarks);

				var otherPackaging = collection1.FirstOrDefault(x => x.PackageType == "1A");
				AssertNotNull("1A packaging should exist.", otherPackaging);
				AssertEquals("PackageQuantity should be APA_PackQty for other packaging.", 20, otherPackaging.PackageQuantity);
				AssertEquals("ShippingMarks should be from linked Package.", "1AMark", otherPackaging.ShippingMarks);
			});

			var collection2 = AISPackagingProvider.GetCollection(packedItem2);
			CombineAssertions("Item 2 packagings.", () =>
			{
				AssertEquals("Count of collection: all linked packs.", 2, collection2.Count);

				var vgPackaging = collection2.FirstOrDefault(x => x.PackageType == "VG");
				AssertNotNull("VG packaging should exist.", vgPackaging);
				AssertNull("PackageQuantity should be null for VG(BULK) packaging.", vgPackaging.PackageQuantity);
				AssertEquals("ShippingMarks should be from linked Package.", "VGMark", vgPackaging.ShippingMarks);

				var nePackaging = collection2.FirstOrDefault(x => x.PackageType == "NE");
				AssertNotNull("NE packaging should exist.", nePackaging);
				AssertEquals("PackageQuantity should be 0 as the previous item has set the Quantity.", 0, nePackaging.PackageQuantity);
				AssertEquals("ShippingMarks should be from linked Package.", "NEMark", nePackaging.ShippingMarks);
			});
		}

		protected override void SetUp()
		{
			TestDataHelper.SetUpPackageTypes(Factory);
		}
	}
}
