using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CusEntryHeaderAttachmentGeneratorTest : TestCaseWithFactory
	{
		public void TestGenerateAttachmentsByCustomsRegistrySetting()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var registryTemplates = new CNDocTemplateForAttachmentCollection();
			var registryTemplate = registryTemplates.AddNew();
			registryTemplate.OrganizationPK = importer.PK;
			registryTemplate.DocumentTemplate = "CN Purchase Order(System)";
			registryTemplate.DataContext = CusEntryHeaderDocumentSupporter.CustomsDeclarationDocument;
			registryTemplate.DocumentType = Core.Constants.RefDocTypes.Invoice;
			registryTemplate.AttachmentType = CSDDocTypeList.Codes._00000003;
			registryTemplate.DocumentDescription = Core.Constants.RefDocTypeDescriptions.Invoice;
			using (CNCustomsDataRegistry.Instance.CNDocTemplateForAttachment.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryTemplates))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				declaration.JE_OH_Importer = importer.PK;
				var entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
				entryInstruction.CEI_JE = declaration.PK;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				var entryLine = entryHeader.MergedLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "TTT";
				invoice.JZ_InvoiceDate = ZDateTime.Today;
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				var storageMain = ((IDocManagerSupport)entryHeader).DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(entryHeader, Core.Constants.DocManagerCodes.CustomsEntry);
				Factory.Save();
				AssertEquals(0, storageMain.Files.Count);
				AssertEquals(0, entryInstruction.Attachments.Count);
				var eEntryHeaderAttachmentGenerator = new CusEntryHeaderAttachmentGenerator(entryHeader);
				var messages = eEntryHeaderAttachmentGenerator.GenerateAttachments();
				AssertContains("Messages", "Document CN Purchase Order.pdf has been generated in eDocs.", messages);
				AssertEquals(1, storageMain.Files.Count);
				AssertEquals(1, entryInstruction.Attachments.Count);
				AssertEquals("New Attachment1 Type", "00000003", entryInstruction.Attachments[0].AttachmentType);
				AssertEquals("New Attachment1 StorageDocReference", storageMain.Files[0].UniqueKey, entryInstruction.Attachments[0].EDoc);
				declaration.JE_MessageType = "EXP";
				var exporter = Factory.NewWithValidTestData<OrgHeader>();
				declaration.JE_OH_Exporter = exporter.PK;
				eEntryHeaderAttachmentGenerator = new CusEntryHeaderAttachmentGenerator(entryHeader);
				messages = eEntryHeaderAttachmentGenerator.GenerateAttachments();
				AssertContains("Messages", "No document templates found for generating attachments. Please go to Registry -> {path of the registry} to specify which document templates you want to used for generating attachments for the entry.", messages);
			}
		}

		public void TestGenerateAttachmentsByDefaultRegistrySetting()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TTT";
			invoice.JZ_InvoiceDate = ZDateTime.Today;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var instruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var attachment = instruction.Attachments.AddNew();
			attachment.AttachmentType = "00000001";
			attachment.EDoc = ZGuid.Empty;
			var storageMain = ((IDocManagerSupport)entryHeader).DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(entryHeader, Core.Constants.DocManagerCodes.CustomsEntry);
			Factory.Save();
			AssertEquals(0, storageMain.Files.Count);
			AssertEquals(1, instruction.Attachments.Count);
			AssertEquals("Existed Attachment1 Type", "00000001", instruction.Attachments[0].AttachmentType);
			AssertEquals("Existed Attachment1 StorageDocReference", ZGuid.Empty, instruction.Attachments[0].EDoc);
			var eEntryHeaderAttachmentGenerator = new CusEntryHeaderAttachmentGenerator(entryHeader);
			var messages = eEntryHeaderAttachmentGenerator.GenerateAttachments();
			Factory.Save();
			AssertContains("Messages", "Document CN Customs Invoice.pdf has been generated in eDocs.\r\nDocument CN Purchase Order.pdf has been generated in eDocs.", messages);
			AssertEquals("Files Count", 2, storageMain.Files.Count);
			var file1 = storageMain.Files[0];
			AssertEquals("File1 Name", "CN Customs Invoice.pdf", file1.FileName);
			AssertEquals("File1 DocType", "MSC", file1.DocType);
			var file2 = storageMain.Files[1];
			AssertEquals("File2 Name", "CN Purchase Order.pdf", file2.FileName);
			AssertEquals("File2 DocType", "MSC", file2.DocType);
			AssertEquals(2, instruction.Attachments.Count);
			AssertEquals("Existed Attachment1 Type", "00000001", instruction.Attachments[0].AttachmentType);
			AssertEquals("Existed Attachment1 StorageDocReference", file1.UniqueKey, instruction.Attachments[0].EDoc);
			AssertEquals("New Attachment2 Type", "00000004", instruction.Attachments[1].AttachmentType);
			AssertEquals("New Attachment2 StorageDocReference", file2.UniqueKey, instruction.Attachments[1].EDoc);
			messages = eEntryHeaderAttachmentGenerator.GenerateAttachments();
			AssertContains("Has Errors", @"Document CN Customs Invoice.pdf could not be generated because another document with the same name exists in eDocs. You may need to delete or rename it first.
Document CN Purchase Order.pdf could not be generated because another document with the same name exists in eDocs. You may need to delete or rename it first.", messages.ToString());
		}
	}
}
