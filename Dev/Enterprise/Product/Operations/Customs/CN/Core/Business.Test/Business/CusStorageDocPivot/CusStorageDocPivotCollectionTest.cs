using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CusStorageDocPivotCollection))]
	class CusStorageDocPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestFindByInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();

			instruction.CusStorageDocPivots.AddNew().InvoiceLineLinks.AddNew().Relation2Object = invoiceLine1;

			var storageDoc1 = instruction.CusStorageDocPivots.AddNew();
			storageDoc1.CSD_DocType = CSDDocTypeList.Codes._80000001;
			storageDoc1.InvoiceLineLinks.AddNew().Relation2Object = invoiceLine1;
			storageDoc1.InvoiceLineLinks.AddNew().Relation2Object = invoiceLine2;

			var storageDoc2 = instruction.CusStorageDocPivots.AddNew();
			storageDoc2.CSD_DocType = CSDDocTypeList.Codes._80000002;
			storageDoc2.InvoiceLineLinks.AddNew().Relation2Object = invoiceLine1;

			var storageDoc3 = instruction.CusStorageDocPivots.AddNew();
			storageDoc3.CSD_DocType = CSDDocTypeList.Codes._80000003;
			storageDoc3.InvoiceLineLinks.AddNew().Relation2Object = invoiceLine2;

			AssertContainsExactElementsInAnyOrder(new[] { storageDoc1, storageDoc2 }, instruction.CusStorageDocPivots.FindByInvoiceLine(invoiceLine1));
			AssertContainsExactElementsInAnyOrder(new[] { storageDoc1, storageDoc3 }, instruction.CusStorageDocPivots.FindByInvoiceLine(invoiceLine2));
		}

		public void TestUnlinkInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var storageDoc1 = AddStorageDocAndLinkToInvoiceLines();
			var storageDoc2 = AddStorageDocAndLinkToInvoiceLines();

			instruction.CusStorageDocPivots.UnlinkInvoiceLine(invoiceLine1);
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine2.PK }, storageDoc1.InvoiceLineLinks.Select(x => x.Relation2ID));
			AssertContainsExactElementsInAnyOrder(new[] { invoiceLine2.PK }, storageDoc2.InvoiceLineLinks.Select(x => x.Relation2ID));

			CusStorageDocPivot AddStorageDocAndLinkToInvoiceLines()
			{
				var storageDoc = instruction.CusStorageDocPivots.AddNew();
				storageDoc.CSD_DocType = CSDDocTypeList.Codes._80000001;
				storageDoc.InvoiceLineLinks.AddNew().Relation2Object = invoiceLine1;
				storageDoc.InvoiceLineLinks.AddNew().Relation2Object = invoiceLine2;
				return storageDoc;
			}
		}

		public void TestReloadAttachmentLinksOnRemoved()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.CargoAttributes.AddNew(CargoAttributeList.Codes._31);
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.CargoAttributes.AddNew(CargoAttributeList.Codes._32);
			var storageDoc1 = AddStorageDocAndLinkToInvoiceLines();
			var storageDoc2 = AddStorageDocAndLinkToInvoiceLines();

			CusStorageDocPivot AddStorageDocAndLinkToInvoiceLines()
			{
				var storageDoc = instruction.CusStorageDocPivots.AddNew();
				storageDoc.CSD_DocType = CSDDocTypeList.Codes._80000001;
				storageDoc.InvoiceLineLinks.AddNew().Relation2Object = invoiceLine1;
				storageDoc.InvoiceLineLinks.AddNew().Relation2Object = invoiceLine2;
				return storageDoc;
			}

			AssertEquals("2 AttachmentLinks on Invoice Line 1", 2, invoiceLine1.AttachmentLinks.Count);
			AssertEquals("2 AttachmentLinks on Invoice Line 2", 2, invoiceLine2.AttachmentLinks.Count);

			instruction.CusStorageDocPivots.Remove(storageDoc1);
			AssertEquals("1 AttachmentLink left on Invoice Line 1", 1, invoiceLine1.AttachmentLinks.Count);
			AssertEquals("1 AttachmentLink left on Invoice Line 2", 1, invoiceLine2.AttachmentLinks.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var instruction = Factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew();
			return new CusStorageDocPivotCollection(instruction);
		}
	}
}
