using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(AttachmentInvoiceLineLink))]
	class AttachmentInvoiceLineLinkTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAttachmentType()
		{
			AssertEquals("AttachmentType: from CusEntryInstruction Attachments", CSDDocTypeList.Codes._80000001, AttachmentLink.AttachmentType);
		}

		public void TestAttachmentTypeCaption()
		{
			AssertEquals("Attachment Type", AttachmentLink.AttachmentTypeInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestAttachmentDescription()
		{
			AssertEquals("AttachmentDescription: from CusEntryInstruction Attachments", CSDDocTypeList.Descriptions._80000001, AttachmentLink.AttachmentDescription);
		}

		public void TestAttachmentDescriptionCaption()
		{
			AssertEquals("Attachment Description", AttachmentLink.AttachmentDescriptionInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestEDoc()
		{
			var eDoc = InvoiceLine.Declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			AttachmentLink.CusStorageDocPivot.CSD_StorageDocReference = eDoc.UniqueKey;
			AssertEquals("EDoc: from job EDoc.", eDoc.UniqueKey, AttachmentLink.EDoc);
		}

		public void TestEDocCaption()
		{
			AssertEquals("eDoc", AttachmentLink.EDocInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestIsLinked()
		{
			AssertEquals("IsLinked: false as default", false, AttachmentLink.IsLinked);

			AttachmentLink.IsLinked = true;
			AssertEquals("IsLinked set to true", true, AttachmentLink.IsLinked);
			var invoiceLineLinks = InvoiceLine.EntryInstruction.Attachments[0].CusStorageDocPivot.InvoiceLineLinks;
			AssertEquals("InvoiceLineLink added", 1, invoiceLineLinks.Count);
			var invoiceLineLink = invoiceLineLinks[0];
			AssertEquals("InvoiceLineLink links to Invoice line", InvoiceLine, invoiceLineLink.Relation2Object);

			AttachmentLink.IsLinked = false;
			AssertEquals("IsLinked set to false", false, AttachmentLink.IsLinked);
			AssertEquals("InvoiceLineLink removed", 0, invoiceLineLinks.Count);
			AssertEquals("InvoiceLineLink deleted", true, invoiceLineLink.IsDeleted);
		}

		public void TestIsLinkedCaption()
		{
			AssertEquals("Is For Invoice Line?", AttachmentLink.IsLinkedInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		protected override BusinessObject GetNewBusinessObject() => AttachmentLink;

		AttachmentInvoiceLineLink AttachmentLink => attachmentLink ??= CreateAttachmentLink();
		AttachmentInvoiceLineLink attachmentLink;

		AttachmentInvoiceLineLink CreateAttachmentLink()
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
			invoiceLine.CargoAttributes.AddNew(CargoAttributeList.Codes._31);
			return invoiceLine;
		}

		JobComInvoiceLine InvoiceLine => invoiceLine ??= CreateInvoiceLine();
		JobComInvoiceLine invoiceLine;
	}
}
