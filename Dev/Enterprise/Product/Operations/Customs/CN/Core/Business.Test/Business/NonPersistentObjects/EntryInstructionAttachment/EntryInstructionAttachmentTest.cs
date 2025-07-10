using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(EntryInstructionAttachment))]
	class EntryInstructionAttachmentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAttachmentTypeDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachment = instruction.Attachments.AddNew();
			var zGuid = ZGuid.NewZGuid();
			attachment.EDoc = zGuid;
			attachment.AttachmentType = CSDDocTypeList.Codes._00000001;
			AssertEquals(CSDDocTypeList.Descriptions._00000001, attachment.AttachmentTypeDescription);
			attachment.AttachmentType = "12345678";
			AssertEquals(ZString.Empty, attachment.AttachmentTypeDescription);
		}

		public void TestEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachment = instruction.Attachments.AddNew();
			AssertSame(instruction, attachment.EntryInstruction);
		}

		public void TestDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachment = instruction.Attachments.AddNew();
			attachment.EDoc = eDoc.UniqueKey;
			AssertEquals(eDoc, attachment.Document);
		}

		public void TestFileName()
		{
			var declaration = Factory.New<JobDeclaration>();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachment = instruction.Attachments.AddNew();
			attachment.EDoc = eDoc.UniqueKey;
			AssertEquals("Invoice.pdf", attachment.FileName);
		}

		public void TestReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachment = instruction.Attachments.AddNew();
			var zGuid = ZGuid.NewZGuid();
			attachment.EDoc = zGuid;

			attachment.AttachmentType = CSDDocTypeList.Codes._00000001;
			AssertEquals("AttachmentNumber should be readonly for 00000001.", true, attachment.AttachmentNumberInfo.ReadOnly);
			AssertEquals("EDoc should be writable for 00000001", false, attachment.EDocInfo.ReadOnly);

			attachment.AttachmentType = CSDDocTypeList.Codes._10000001;
			AssertEquals("AttachmentNumber should be writable for 10000001.", false, attachment.AttachmentNumberInfo.ReadOnly);
			AssertEquals("EDoc should be readonly for 10000001", true, attachment.EDocInfo.ReadOnly);

			attachment.AttachmentType = CSDDocTypeList.Codes._10000002;
			AssertEquals("AttachmentNumber should be writable for 10000002.", false, attachment.AttachmentNumberInfo.ReadOnly);
			AssertEquals("EDoc should be readonly for 10000002", true, attachment.EDocInfo.ReadOnly);

			attachment.AttachmentType = CSDDocTypeList.Codes._10000003;
			AssertEquals("AttachmentNumber should be writable for 10000003.", false, attachment.AttachmentNumberInfo.ReadOnly);
			AssertEquals("EDoc should be readonly for 10000003", true, attachment.EDocInfo.ReadOnly);

			attachment.AttachmentType = CSDDocTypeList.Codes._10000004;
			AssertEquals("AttachmentNumber should be writable for 10000004.", false, attachment.AttachmentNumberInfo.ReadOnly);
			AssertEquals("EDoc should be readonly for 10000004", true, attachment.EDocInfo.ReadOnly);
		}

		public void TestAttachmentNumberInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachment = instruction.Attachments.AddNew();
			var targetInfo = attachment.AttachmentNumberInfo;

			CombineAssertions(() =>
			{
				AssertEquals("MaxLength", 255, targetInfo.MaxLength);

				var resourceStringDataAttribute = targetInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Caption", "Attachment Number", resourceStringDataAttribute.Caption);
				AssertEquals("MediumCaption", "Number", resourceStringDataAttribute.MediumCaption);
				AssertEquals("ShortCaption", "Num.", resourceStringDataAttribute.ShortCaption);
			});
		}

		public void TestCanLinkToInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachment = instruction.Attachments.AddNew();

			AssertEquals("AttachmentType is empty", false, attachment.CanLinkToInvoiceLine);

			var canLinkToInvoiceLineTypes = new[] {
				CSDDocTypeList.Codes._80000001,
				CSDDocTypeList.Codes._80000002,
				CSDDocTypeList.Codes._80000003,
				CSDDocTypeList.Codes._80000004
			};

			foreach (var code in new CSDDocTypeList().GetAllCodes())
			{
				attachment.AttachmentType = code;
				AssertEquals($"AttachmentType={code}", canLinkToInvoiceLineTypes.Contains(code), attachment.CanLinkToInvoiceLine);
			}
		}

		public void TestGetLinkedEntryLineNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachment = instruction.Attachments.AddNew();
			attachment.AttachmentType = CSDDocTypeList.Codes._80000001;

			JobComInvoiceLine CreateInvoiceLine()
			{
				var invoiceLine = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.CargoAttributes.AddNew(CargoAttributeList.Codes._31);
				invoiceLine.AttachmentLinks.Cast<AttachmentInvoiceLineLink>().First().IsLinked = true;
				return invoiceLine;
			}

			var invoiceLine1 = CreateInvoiceLine();
			var invoiceLine2 = CreateInvoiceLine();
			var invoiceLine3 = CreateInvoiceLine();
			var invoiceLine4 = CreateInvoiceLine();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine1.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine3.JI_CL = entryLine2.PK;

			AssertArrayEqualsByElements("GetLinkedEntryLines returns empty", new ZShort[] { 1, 2 }, attachment.GetLinkedEntryLineNumbers());

			attachment.AttachmentType = CSDDocTypeList.Codes._10000001;
			AssertArrayEqualsByElements("No LinkedEntryLineNumbers", Array.Empty<ZShort>(), attachment.GetLinkedEntryLineNumbers());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EntryInstructionAttachment(new EntryInstructionAttachmentCollection(Factory.New<CusEntryInstruction>()));
		}
	}
}
