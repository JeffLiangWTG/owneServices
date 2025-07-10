using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(AttachmentInvoiceLineLinkCollection))]
	class AttachmentInvoiceLineLinkCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AttachmentInvoiceLineLinkCollection>
	{
		public void TestLoad()
		{
			var attribute = InvoiceLine.CargoAttributes.AddNew(CargoAttributeList.Codes._31);

			var collection = new AttachmentInvoiceLineLinkCollection(InvoiceLine);
			collection.Load();
			AssertEquals("No Attachments", 0, collection.Count);

			var attachment = InvoiceLine.EntryInstruction.Attachments.AddNew();
			attachment.AttachmentType = CSDDocTypeList.Codes._00000001;
			collection.Load();
			AssertEquals("No Attachment can link to Invoice Line", 0, collection.Count);

			attachment = InvoiceLine.EntryInstruction.Attachments.AddNew();
			attachment.AttachmentType = CSDDocTypeList.Codes._10000001;
			collection.Load();
			AssertEquals("No Attachment can link to Invoice Line", 0, collection.Count);

			attachment = InvoiceLine.EntryInstruction.Attachments.AddNew();
			attachment.AttachmentType = CSDDocTypeList.Codes._80000001;
			collection.Load();
			AssertEquals("Attachment 80000001 can link to Invoice Line", 1, collection.Count);

			attachment = InvoiceLine.EntryInstruction.Attachments.AddNew();
			attachment.AttachmentType = CSDDocTypeList.Codes._80000002;
			collection.Load();
			AssertEquals("Attachment 80000001, 80000002 can link to Invoice Line", 2, collection.Count);

			var link1 = collection[0];
			var link2 = collection[1];
			attachment.AttachmentType = CSDDocTypeList.Codes._80000003;
			collection.Load();
			AssertContainsExactElementsInExactOrder("Attachment 80000001, 80000003 can link to Invoice Line", new[] { link1, link2 }, collection);

			attachment.AttachmentType = CSDDocTypeList.Codes._50000001;
			collection.Load();
			AssertContainsExactElementsInExactOrder(new[] { link1 }, collection);
			Assert("Obsoleted AttachmentInvoiceLineLink is deleted", link2.IsDeleted);

			attribute.CY_Code = CargoAttributeList.Codes._33;
			collection.Load();
			AssertEquals("Cargo Attribute can link to Invoice Line", 0, collection.Count);
			Assert("Obsoleted AttachmentInvoiceLineLink is deleted", link1.IsDeleted);

			InvoiceLine.JI_CEI = ZGuid.Empty;
			collection.Load();
			AssertEquals("Cargo Attribute can link to Invoice Line", 0, collection.Count);
		}

		public void TestAllowNew()
		{
			Assert(!InvoiceLine.AttachmentLinks.AllowNew);
		}

		public void TestAllowRemove()
		{
			Assert(!InvoiceLine.AttachmentLinks.AllowRemove);
		}

		protected override AttachmentInvoiceLineLinkCollection GetCollectionToTest() => InvoiceLine.AttachmentLinks;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var attachment = InvoiceLine.EntryInstruction.Attachments.AddNew();
			attachment.AttachmentType = CSDDocTypeList.Codes._80000001;
			return new AttachmentInvoiceLineLink(attachment, InvoiceLine);
		}

		JobComInvoiceLine CreateInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CEI = instruction.PK;
			return invoiceLine;
		}

		JobComInvoiceLine InvoiceLine => invoiceLine ??= CreateInvoiceLine();
		JobComInvoiceLine invoiceLine;
	}
}
