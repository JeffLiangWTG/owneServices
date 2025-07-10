using System;
using System.Linq;
using System.Text;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Service.Testing
{
	sealed class DocumentDeliveryServiceTest : TestCaseWithFactory
	{
		public void TestBackgroundDeliveryWhenDeliverDocument()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmDocumentDeliverySchema.Constants.TableName);

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[<RecipientNameAndAddress>]
{A}-[#EndOfReport]");
			template.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob);
			var documentCommand = Factory.New<DocumentCommand>();
			var parent = Factory.New<ICommonShipment>() as BusinessObject;
			var document = documentCommand.Documents.AddNew();

			documentCommand.Parent = parent as IDocumentSupportable;
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "Microsoft";
			organization.OH_FullName = "Microsoft";

			var address = organization.Addresses[0];
			AssertNotNull(address);
			address.Address1 = "Microsoft Address";

			var recipient = new DeliveryRecipientBase();
			recipient.OrganizationId = organization.PK.ToGuid();
			recipient.Name = "Bill Gates";
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			recipient.Email = "unit.test@cargowise.com";
			recipient.EmailAttachmentType = Enterprise.MasterFiles.Business.OrgConstants.AttachmentType.PDF;

			var deliveryInstructions = new DeliveryInstructionsBase();
			deliveryInstructions.Recipients = new[] { recipient };
			deliveryInstructions.UseBackgroundDelivery = true;

			Factory.Save();

			var webService = new DocumentDeliveryService();
			webService.DeliverDocument(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid(), deliveryInstructions, null);

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("printJobs.Length", 0, printJobs.Length);

			var documentDeliveries = Factory.Load<StmDocumentDelivery>(new ZQuery());
			AssertEquals("documentDeliveries.Length", 1, documentDeliveries.Length);

			documentDeliveries.DeleteAll();
			deliveryInstructions.UseBackgroundDelivery = false;
			Factory.Save();

			webService.DeliverDocument(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid(), deliveryInstructions, null);

			printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("printJobs.Length", 1, printJobs.Length);

			documentDeliveries = Factory.Load<StmDocumentDelivery>(new ZQuery());
			AssertEquals("documentDeliveries.Length", 0, documentDeliveries.Length);
		}

		public void TestCheckDataStateWhenDeliverDocument()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyDocumentSupportable);
			DummyDocumentSupporter.ReturnInvalidDataStateForTest.Value = true;

			var logs = new StringBuilder();
			var dummy = Factory.New<DummyDocumentSupportable>();

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[<ReportName>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_DocumentTitle = "Document";
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			var recipient = new DeliveryRecipientBase();
			recipient.OrganizationId = dummy.PK.ToGuid();
			recipient.Name = "Jerry";
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			recipient.Email = "unit.test@cargowise.com";
			recipient.EmailAttachmentType = Enterprise.MasterFiles.Business.OrgConstants.AttachmentType.PDF;

			var deliveryInstructions = new DeliveryInstructionsBase();
			deliveryInstructions.Recipients = new[] { recipient };

			Factory.Save();

			var webService = new DocumentDeliveryService
			{
				LogAction = (type, s) => logs.Append($"{type}, {s}")
			};
			var documents = webService.GetDocuments(documentCommand.PK.ToGuid());
			var result = webService.DeliverDocument(documentCommand.PK.ToGuid(), dummy.TablePrefix, dummy.PK.ToGuid(), deliveryInstructions, documents);

			AssertEquals("DocumentSupporterDataState should be invalid", false, result.Success);
			AssertEquals("Error, Unable to run this document, the error reason is Error Test", logs.ToString());
		}

		public void TestDeliverOnlySomeDocumentsOfADocumentCommand()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[<ReportName>]
{A}-[#EndOfReport]");
			template.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Organisation);

			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "Microsoft";
			organization.OH_FullName = "Microsoft";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = organization;

			var document1 = documentCommand.Documents.AddNew();
			document1.SI_DocumentTitle = "Document 1";
			document1.SI_SU = documentCommand.PK;
			document1.SI_SO = template.PK;

			var document2 = documentCommand.Documents.AddNew();
			document2.SI_DocumentTitle = "Document 2";
			document2.SI_SU = documentCommand.PK;
			document2.SI_SO = template.PK;

			var document3 = documentCommand.Documents.AddNew();
			document3.SI_DocumentTitle = "Document 3";
			document3.SI_SU = documentCommand.PK;
			document3.SI_SO = template.PK;

			var recipient = new DeliveryRecipientBase();
			recipient.OrganizationId = organization.PK.ToGuid();
			recipient.Name = "Bill Gates";
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			recipient.Email = "unit.test@cargowise.com";
			recipient.EmailAttachmentType = Enterprise.MasterFiles.Business.OrgConstants.AttachmentType.PDF;

			var deliveryInstructions = new DeliveryInstructionsBase();
			deliveryInstructions.Recipients = new[] { recipient };

			Factory.Save();

			var webService = new DocumentDeliveryService();
			var documents = webService.GetDocuments(documentCommand.PK.ToGuid());

			var documentDetail = documents.First<DocumentDetail>((item) => item.Name == "Document 2");
			documentDetail.ShouldInclude = false;

			webService.DeliverDocument(documentCommand.PK.ToGuid(), organization.TablePrefix, organization.PK.ToGuid(), deliveryInstructions, documents);

			var result = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("result.Length", 1, result.Length);

			var printJob = result[0];
			AssertEquals("printJob.SP_JobType", Enterprise.Core.Constants.ContactNotifyModes.Email, printJob.SP_JobType);
			AssertEquals("printJob.SP_Destination", "unit.test@cargowise.com", printJob.SP_Destination);
			AssertEquals("printJob.SP_EmailAttachmentFormat", "PDF", printJob.SP_EmailAttachmentFormat);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJob.SP_CustomProperties);

				AssertMultilineASCIIEquals("excelInterface.WorkSheets[0].ToString()", @"{B}-[Document 1]", excelInterface.WorkSheets[0].ToString());
				AssertMultilineASCIIEquals("excelInterface.WorkSheets[1].ToString()", @"{B}-[Document 3]", excelInterface.WorkSheets[1].ToString());
			}
		}

		public void TestGetAutoDeliveryRecipients()
		{
			TestGetAutoDeliveryRecipientsCore(
				ContactType.Consignee.Code,
				preventAutoDelivery: false,
				"Service should returned the default contacts for the specified document command.",
				@"Name:[Bill Gates]   Organization:[MyCode]   DeliveryMethod:[PRN]   AttachmentType:[]   Address:[]   CC:[]   BCC:[]
Name:[Sir Ken Robinson]   Organization:[MyCode]   DeliveryMethod:[EML]   AttachmentType:[PDF]   Address:[sir.ken.robinson@cargowise.com]   CC:[cc1@qq.com, cc2@qq.com]   BCC:[bcc1@qq.com, bcc2@qq.com]
Name:[Tony Robbins]   Organization:[MyCode]   DeliveryMethod:[FAX]   AttachmentType:[]   Address:[22222222]   CC:[]   BCC:[]");
		}

		public void TestGetAutoDeliveryRecipientsWhenPreventAutoDelivery()
		{
			TestGetAutoDeliveryRecipientsCore(
				ContactType.Consignee.Code,
				preventAutoDelivery: true,
				"Service should returned default DeliveryMethod PRN for the document command prevents auto delivery.",
				"Name:[]   Organization:[]   DeliveryMethod:[PRN]   AttachmentType:[]   Address:[]   CC:[]   BCC:[]");
		}

		public void TestGetAutoDeliveryRecipientsWhenNoContactType()
		{
			TestGetAutoDeliveryRecipientsCore(
				ContactType.NoContactType.Code,
				preventAutoDelivery: false,
				"Service should returned DeliveryMethod PRN for the document command with NoContactType.",
				@"Name:[]   Organization:[]   DeliveryMethod:[PRN]   AttachmentType:[]   Address:[]   CC:[]   BCC:[]");
		}

		void TestGetAutoDeliveryRecipientsCore(string contactType, bool preventAutoDelivery, string errorMessage, string expectedResult)
		{
			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "MyCode";

			var address = organization.MainAddress;
			address.OA_Address1 = "My Address";

			var emailContact = organization.Contacts.AddNew();
			emailContact.OC_IsActive = ZBool.True;
			emailContact.OC_ContactName = "Sir Ken Robinson";
			emailContact.OC_Email = "sir.ken.robinson@cargowise.com";
			emailContact.OC_Fax = "11111111";
			var emailDocument = emailContact.Documents.AddNew();
			emailDocument.OD_DocumentGroup = ContactType.All.Code;
			emailDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			emailDocument.OD_AttachmentType = "PDF";
			emailDocument.OD_FilterShipmentMode = "ALL";
			emailDocument.OD_FilterDirection = "ALL";
			emailDocument.OD_CarbonCopyRecipientsAsString = "cc1@qq.com, cc2@qq.com";
			emailDocument.OD_BlindCarbonCopyRecipientsAsString = "bcc1@qq.com, bcc2@qq.com";

			var faxContact = organization.Contacts.AddNew();
			faxContact.OC_IsActive = ZBool.True;
			faxContact.OC_ContactName = "Tony Robbins";
			faxContact.OC_Email = "tony.robbins@cargowise.com";
			faxContact.OC_Fax = "22222222";
			var faxDocument = faxContact.Documents.AddNew();
			faxDocument.OD_DocumentGroup = ContactType.All.Code;
			faxDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Fax;
			faxDocument.OD_FilterShipmentMode = "ALL";
			faxDocument.OD_FilterDirection = "ALL";

			var printerContact = organization.Contacts.AddNew();
			printerContact.OC_IsActive = ZBool.True;
			printerContact.OC_ContactName = "Bill Gates";
			printerContact.OC_Email = "bill.gates@cargowise.com";
			printerContact.OC_Fax = "33333333";
			var printerDocument = printerContact.Documents.AddNew();
			printerDocument.OD_DocumentGroup = ContactType.All.Code;
			printerDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Print;
			printerDocument.OD_FilterShipmentMode = "ALL";
			printerDocument.OD_FilterDirection = "ALL";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_ContactType = contactType;
			documentCommand.SU_AddressCategory = OrgAddressCategory.Codes.Office;
			documentCommand.SU_PreventAutoDelivery = preventAutoDelivery;
			documentCommand.Parent = organization;

			Factory.Save();

			var webService = new DocumentDeliveryService();

			var result = webService.GetDeliveryRecipients(documentCommand.PK.ToGuid(), organization.TablePrefix, organization.PK.ToGuid()).OrderBy((RecipientDetail recipient) => { return recipient.Name; });

			AssertMultilineASCIIEquals(errorMessage, expectedResult, result.ToStringForTesting());
		}

		public void TestGetAutoDeliveryRecipientsWhenThereIsNoContactOnOrganization()
		{
			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "MyCode";

			var address = organization.MainAddress;
			address.OA_Address1 = "My Address";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_ContactType = ContactType.All.Code;
			documentCommand.SU_AddressCategory = OrgAddressCategory.Codes.Office;
			documentCommand.SU_PreventAutoDelivery = ZBool.False;
			documentCommand.Parent = organization;

			Factory.Save();

			var webService = new DocumentDeliveryService();

			AssertMultilineASCIIEquals("Service should of returned the default contacts for the specified document command.",
@"Name:[All Documents]   Organization:[MyCode]   DeliveryMethod:[PRN]   AttachmentType:[]   Address:[]   CC:[]   BCC:[]",
				webService.GetDeliveryRecipients(documentCommand.PK.ToGuid(), organization.TablePrefix, organization.PK.ToGuid()).ToStringForTesting());

			address.OA_Fax = "1234567";
			Factory.Save();
			AssertMultilineASCIIEquals("Service should of returned the default contacts for the specified document command.",
@"Name:[All Documents]   Organization:[MyCode]   DeliveryMethod:[FAX]   AttachmentType:[]   Address:[1234567]   CC:[]   BCC:[]",
				webService.GetDeliveryRecipients(documentCommand.PK.ToGuid(), organization.TablePrefix, organization.PK.ToGuid()).ToStringForTesting());

			address.OA_Email = "unit.test@cargowise.com";
			Factory.Save();
			AssertMultilineASCIIEquals("Service should of returned the default contacts for the specified document command.",
@"Name:[All Documents]   Organization:[MyCode]   DeliveryMethod:[EML]   AttachmentType:[PDF]   Address:[unit.test@cargowise.com]   CC:[]   BCC:[]",
				webService.GetDeliveryRecipients(documentCommand.PK.ToGuid(), organization.TablePrefix, organization.PK.ToGuid()).ToStringForTesting());
		}

		public void TestSetAndGetDefaultPrinter()
		{
			var webService = new DocumentDeliveryService();

			var document = Factory.New<StmMenuItem>();
			var parent = Factory.New<ICommonShipment>() as BusinessObject;
			var dummyPrinter = Factory.New<StmPrintQueue>();
			Factory.Save();
			var documentId = document.PK.ToGuid();
			var printerId = dummyPrinter.PK.ToGuid();

			AssertNull("No Default Printer exists for this key.", webService.GetDefaultPrinterKey(documentId));

			var deliveryInstructions = new DeliveryInstructionsBase { PrinterId = printerId, Recipients = Array.Empty<DeliveryRecipientBase>() };
			webService.DeliverDocument(documentId, parent.TablePrefix, parent.PK.ToGuid(), deliveryInstructions, null);
			AssertNull("Default Printer was not saved when no print tasks.", webService.GetDefaultPrinterKey(documentId));

			deliveryInstructions.PrinterId = Guid.Empty;
			deliveryInstructions.Recipients = new[] { new DeliveryRecipientBase { DeliveryMethod = Core.Constants.ContactNotifyModes.Print } };
			webService.DeliverDocument(documentId, parent.TablePrefix, parent.PK.ToGuid(), deliveryInstructions, null);
			AssertNull("Default Printer was not saved for empty printer id.", webService.GetDefaultPrinterKey(documentId));

			deliveryInstructions.PrinterId = printerId;
			webService.DeliverDocument(Guid.Empty, parent.TablePrefix, parent.PK.ToGuid(), deliveryInstructions, null);
			AssertNull("Default Printer was not saved for empty document key.", webService.GetDefaultPrinterKey(documentId));

			webService.DeliverDocument(documentId, parent.TablePrefix, parent.PK.ToGuid(), deliveryInstructions, null);
			AssertEquals("Default Printer should have the correct key.", printerId, webService.GetDefaultPrinterKey(documentId));
		}

		public void TestGetPrinters()
		{
			var printQueue1 = Factory.New<StmPrintQueue>();
			printQueue1.SQ_DisplayName = "Printer 1";
			printQueue1.SQ_ServerName = @"\\syd-printer-server\Printer 1";
			printQueue1.SQ_AllowPrinting = ZBool.True;

			var printQueue2 = Factory.New<StmPrintQueue>();
			printQueue2.SQ_DisplayName = "Printer 2";
			printQueue2.SQ_ServerName = @"\\syd-printer-server\Printer 2";
			printQueue2.SQ_AllowPrinting = ZBool.False;

			var printQueue3 = Factory.New<StmPrintQueue>();
			printQueue3.SQ_DisplayName = "Printer 3";
			printQueue3.SQ_ServerName = @"\\syd-printer-server\Printer 3";
			printQueue3.SQ_AllowPrinting = ZBool.True;

			Factory.Save();

			var webService = new DocumentDeliveryService();
			var printers = webService.GetPrinters();

			AssertMultilineASCIIEquals("If SQ_AllowPrinting is set to false, the printer should not be acquired",
@"Name:[Printer 1]   Location:[\\syd-printer-server\Printer 1]   IsPrintAllowed:[True]
Name:[Printer 3]   Location:[\\syd-printer-server\Printer 3]   IsPrintAllowed:[True]",
				printers.ToStringForTesting());
		}

		public void TestGetPrinters_IsPrintAllowed()
		{
			var printQueue1 = Factory.New<StmPrintQueue>();
			printQueue1.SQ_DisplayName = "Printer 1";
			printQueue1.SQ_ServerName = @"\\syd-printer-server\Printer 1";
			printQueue1.SQ_AllowPrinting = ZBool.True;

			var printQueue2 = Factory.New<StmPrintQueue>();
			printQueue2.SQ_DisplayName = "Printer 2";
			printQueue2.SQ_ServerName = @"\\syd-printer-server\Printer 2";
			printQueue2.SQ_AllowPrinting = ZBool.True;
			Env.Security.GetPrintQueueCheckPoint(printQueue2.PK.ToGuid(), printQueue2.SQ_DisplayName).IsAllowed = false;
			Factory.Save();

			var webService = new DocumentDeliveryService();
			var printers = webService.GetPrinters();

			AssertMultilineASCIIEquals("The printers of webService should have the correct IsPrintAllowed",
@"Name:[Printer 1]   Location:[\\syd-printer-server\Printer 1]   IsPrintAllowed:[True]
Name:[Printer 2]   Location:[\\syd-printer-server\Printer 2]   IsPrintAllowed:[False]",
				printers.ToStringForTesting());
		}

		public void TestGetDocuments()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			var template = Factory.New<StmTemplate>();
			template.SO_DataContext = ".OrgHeader";

			var document1 = documentCommand.Documents.AddNew();
			document1.SI_SU = documentCommand.PK;
			document1.SI_SO = template.PK;
			document1.SI_PrintCopyType = "ALL";
			document1.SI_DocumentTitle = "Document 1";

			var document2 = documentCommand.Documents.AddNew();
			document2.SI_SU = documentCommand.PK;
			document2.SI_SO = template.PK;
			document2.SI_PrintCopyType = "FAX";
			document2.SI_DocumentTitle = "Document 2";

			Factory.Save();

			var webService = new DocumentDeliveryService();
			var documents = webService.GetDocuments(documentCommand.PK.ToGuid());
			AssertEquals("There should be 2 documents for this document command.", 2, documents.Length);

			CombineAssertions(() =>
			{
				var documentDetail1 = documents.First<DocumentDetail>((item) => item.Id == document1.PK);
				AssertEquals("documentDetail1.Id", document1.PK, documentDetail1.Id);
				AssertEquals("documentDetail1.Name", document1.SI_DocumentTitle, documentDetail1.Name);
				AssertEquals("documentDetail1.Mode", "ALL", documentDetail1.Mode);
				AssertEquals("documentDetail1.ShouldInclude", true, documentDetail1.ShouldInclude);

				var documentDetail2 = documents.First<DocumentDetail>((item) => item.Id == document2.PK);
				AssertEquals("documentDetail2.Id", document2.PK, documentDetail2.Id);
				AssertEquals("documentDetail2.Name", document2.SI_DocumentTitle, documentDetail2.Name);
				AssertEquals("documentDetail2.Mode", "FAX", documentDetail2.Mode);
				AssertEquals("documentDetail2.ShouldInclude", true, documentDetail2.ShouldInclude);
			});
		}

		public void TestGetDocuments_ChildDocuments()
		{
			var documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			var template = Factory.NewWithValidTestData<StmTemplate>();
			template.SO_DataContext = ".OrgHeader";

			var document1 = documentCommand.Documents.AddNew();
			document1.SI_SU = documentCommand.PK;
			document1.SI_SO = template.PK;
			document1.SI_PrintCopyType = "ALL";
			document1.SI_DocumentTitle = "Document 1";

			var documentCommandChild = Factory.NewWithValidTestData<DocumentCommand>();
			var template2 = Factory.NewWithValidTestData<StmTemplate>();
			template2.SO_DataContext = ".OrgHeader";

			var document2 = documentCommandChild.Documents.AddNew();
			document2.SI_SU = documentCommandChild.PK;
			document2.SI_SO = template.PK;
			document2.SI_PrintCopyType = "FAX";
			document2.SI_DocumentTitle = "Document 2";

			var pivot = documentCommand.ChildMenus.AddNew();
			pivot.SF_SU_Inward = documentCommand.PK;
			pivot.SF_SU_Outward = documentCommandChild.PK;

			Factory.Save();

			var webService = new DocumentDeliveryService();

			var documents = webService.GetDocuments(documentCommandChild.PK.ToGuid());
			AssertEquals("There should be 1 document for this document command.", 1, documents.Length);

			documents = webService.GetDocuments(documentCommand.PK.ToGuid());
			AssertEquals("There should be 2 documents for this document command.", 2, documents.Length);

			CombineAssertions(() =>
			{
				var documentDetail1 = documents.First<DocumentDetail>((item) => item.Id == document1.PK);
				AssertEquals("documentDetail1.Id", document1.PK, documentDetail1.Id);
				AssertEquals("documentDetail1.Name", document1.SI_DocumentTitle, documentDetail1.Name);
				AssertEquals("documentDetail1.Mode", "ALL", documentDetail1.Mode);
				AssertEquals("documentDetail1.ShouldInclude", true, documentDetail1.ShouldInclude);

				var documentDetail2 = documents.First<DocumentDetail>((item) => item.Id == document2.PK);
				AssertEquals("documentDetail2.Id", document2.PK, documentDetail2.Id);
				AssertEquals("documentDetail2.Name", document2.SI_DocumentTitle, documentDetail2.Name);
				AssertEquals("documentDetail2.Mode", "FAX", documentDetail2.Mode);
				AssertEquals("documentDetail2.ShouldInclude", true, documentDetail2.ShouldInclude);
			});
		}

		public void TestDeliverDocumentViaEmail_WithoutCCAndBCC()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var (documentCommand, parent, deliveryInstructions, orgPK) = PrepareDataForDeliverDocument();

			var recipient = new DeliveryRecipientBase();
			recipient.OrganizationId = orgPK;
			recipient.Name = "Bill Gates";
			recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.Email = "unit.test@cargowise.com";
			recipient.EmailAttachmentType = OrgConstants.AttachmentType.PDF;

			deliveryInstructions.Recipients = new[] { recipient };

			Factory.Save();

			var webService = new DocumentDeliveryService();
			webService.DeliverDocument(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid(), deliveryInstructions, null);

			var result = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("result.Length", 1, result.Length);

			var printJob = result[0];
			AssertEquals("printJob.SP_JobType", Enterprise.Core.Constants.ContactNotifyModes.Email, printJob.SP_JobType);
			AssertEquals("printJob.SP_Destination", "unit.test@cargowise.com", printJob.SP_Destination);
			AssertEquals("have no CarbonCopyRecipients", 0, printJob.CarbonCopyRecipients.Count);
			AssertEquals("have no BlindCarbonCopyRecipients", 0, printJob.BlindCarbonCopyRecipients.Count);
			AssertEquals("printJob.SP_EmailAttachmentFormat", "PDF", printJob.SP_EmailAttachmentFormat);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJob.SP_CustomProperties);
				AssertEquals("excelInterface.WorkSheets.Count", 1, excelInterface.WorkSheets.Count);

				var workSheet = excelInterface.WorkSheets[0];
				AssertEquals("workSheet.ToString()", "{B}-[BILL GATES|>MICROSOFT|>MICROSOFT ADDRESS]", workSheet.ToString());
			}
		}

		public void TestDeliverDocumentViaEmail_WithCCAndBCC()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var (documentCommand, parent, deliveryInstructions, orgPK) = PrepareDataForDeliverDocument();

			var recipient = new DeliveryRecipientBase();
			recipient.OrganizationId = orgPK;
			recipient.Name = "Bill Gates";
			recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.Email = "unit.test@cargowise.com";
			recipient.CC = "cc1.unit.test@cargowise.com, cc2.unit.test@cargowise.com";
			recipient.BCC = "bcc1.unit.test@cargowise.com, bcc2.unit.test@cargowise.com";
			recipient.EmailAttachmentType = OrgConstants.AttachmentType.PDF;

			deliveryInstructions.Recipients = new[] { recipient };

			Factory.Save();

			var webService = new DocumentDeliveryService();
			webService.DeliverDocument(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid(), deliveryInstructions, null);

			var result = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("result.Length", 1, result.Length);

			var printJob = result[0];
			AssertEquals("printJob.SP_JobType", Enterprise.Core.Constants.ContactNotifyModes.Email, printJob.SP_JobType);
			AssertEquals("printJob.SP_Destination", "unit.test@cargowise.com", printJob.SP_Destination);
			AssertEquals("count of printJob.CarbonCopyRecipients", 2, printJob.CarbonCopyRecipients.Count);
			AssertEquals("count of printJob.BlindCarbonCopyRecipients", 2, printJob.BlindCarbonCopyRecipients.Count);
			AssertEquals("first of printJob.CarbonCopyRecipients", "cc1.unit.test@cargowise.com", printJob.CarbonCopyRecipients[0].SPR_EmailAddress);
			AssertEquals("second of printJob.CarbonCopyRecipients", "cc2.unit.test@cargowise.com", printJob.CarbonCopyRecipients[1].SPR_EmailAddress);
			AssertEquals("first of printJob.BlindCarbonCopyRecipients", "bcc1.unit.test@cargowise.com", printJob.BlindCarbonCopyRecipients[0].SPR_EmailAddress);
			AssertEquals("second of printJob.BlindCarbonCopyRecipients", "bcc2.unit.test@cargowise.com", printJob.BlindCarbonCopyRecipients[1].SPR_EmailAddress);
			AssertEquals("printJob.SP_EmailAttachmentFormat", "PDF", printJob.SP_EmailAttachmentFormat);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJob.SP_CustomProperties);
				AssertEquals("excelInterface.WorkSheets.Count", 1, excelInterface.WorkSheets.Count);

				var workSheet = excelInterface.WorkSheets[0];
				AssertEquals("workSheet.ToString()", "{B}-[BILL GATES|>MICROSOFT|>MICROSOFT ADDRESS]", workSheet.ToString());
			}
		}

		public void TestDeliverDocumentViaPrinter()
		{
			var (documentCommand, parent, deliveryInstructions, orgPK) = PrepareDataForDeliverDocument();

			var recipient = new DeliveryRecipientBase();
			recipient.OrganizationId = orgPK;
			recipient.Name = "Bill Gates";
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Print;

			deliveryInstructions.Recipients = new[] { recipient };

			Factory.Save();

			var webService = new DocumentDeliveryService();
			webService.DeliverDocument(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid(), deliveryInstructions, null);

			var result = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("result.Length", 1, result.Length);

			var printJob = result[0];
			AssertEquals("printJob.SP_JobType", Enterprise.Core.Constants.ContactNotifyModes.Print, printJob.SP_JobType);
			AssertEquals("printJob.SP_SQ", deliveryInstructions.PrinterId, printJob.SP_SQ);
			AssertEquals("printJob.SP_Copies", 6, printJob.SP_Copies.ToZInt());

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJob.SP_CustomProperties);
				AssertEquals("excelInterface.WorkSheets.Count", 1, excelInterface.WorkSheets.Count);

				var workSheet = excelInterface.WorkSheets[0];
				AssertEquals("workSheet.ToString()", "{B}-[BILL GATES|>MICROSOFT|>MICROSOFT ADDRESS]", workSheet.ToString());
			}
		}

		public void TestDeliverDocumentViaPrinterWhenPrinterIsNotAllowed()
		{
			var (documentCommand, parent, deliveryInstructions, orgPK) = PrepareDataForDeliverDocument();

			var recipient = new DeliveryRecipientBase();
			recipient.OrganizationId = orgPK;
			recipient.Name = "Bill Gates";
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Print;
			deliveryInstructions.Recipients = new[] { recipient };

			var printQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			printQueue.SQ_AllowPrinting = ZBool.False;
			deliveryInstructions.PrinterId = printQueue.PK.ToGuid();
			Factory.Save();

			var webService = new DocumentDeliveryService();
			var result = webService.DeliverDocument(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid(), deliveryInstructions, null);
			AssertEquals(false, result.Success);
			AssertEquals("The printer you have selected is not currently installed. Please see your system administrator.", result.Message);
			var printJobCount = Factory.Load<StmPrintJob>(new ZQuery()).Length;
			AssertEquals("Printer is not online, print job will not be created", 0, printJobCount);

			var printQueue2 = Factory.NewWithValidTestData<StmPrintQueue>();
			printQueue2.SQ_AllowPrinting = ZBool.True;
			deliveryInstructions.PrinterId = printQueue2.PK.ToGuid();
			Env.Security.GetPrintQueueCheckPoint(printQueue2.PK.ToGuid(), printQueue2.SQ_DisplayName).IsAllowed = false;
			Factory.Save();

			result = webService.DeliverDocument(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid(), deliveryInstructions, null);
			AssertEquals(false, result.Success);
			AssertEquals("You do not have the security rights to print to the selected printer.", result.Message);
			printJobCount = Factory.Load<StmPrintJob>(new ZQuery()).Length;
			AssertEquals("Printer is not allowed, print job will not be created", 0, printJobCount);

			var printQueue3 = Factory.NewWithValidTestData<StmPrintQueue>();
			printQueue3.SQ_AllowPrinting = ZBool.True;
			deliveryInstructions.PrinterId = printQueue3.PK.ToGuid();
			Env.Security.GetPrintQueueCheckPoint(printQueue3.PK.ToGuid(), printQueue3.SQ_DisplayName).IsAllowed = true;
			Factory.Save();

			result = webService.DeliverDocument(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid(), deliveryInstructions, null);
			AssertEquals(true, result.Success);
			printJobCount = Factory.Load<StmPrintJob>(new ZQuery()).Length;
			AssertEquals("Printer is valid, print job created", 1, printJobCount);
		}

		public void TestDeliverDocumentViaFax()
		{
			var (documentCommand, parent, deliveryInstructions, orgPK) = PrepareDataForDeliverDocument();

			var recipient = new DeliveryRecipientBase();
			recipient.OrganizationId = orgPK;
			recipient.Name = "Bill Gates";
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Fax;
			recipient.FaxNumber = "+1234567890";

			deliveryInstructions.Recipients = new[] { recipient };

			Factory.Save();

			var webService = new DocumentDeliveryService();
			webService.DeliverDocument(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid(), deliveryInstructions, null);

			var result = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("result.Length", 1, result.Length);

			var printJob = result[0];
			AssertEquals("printJob.SP_JobType", Enterprise.Core.Constants.ContactNotifyModes.Fax, printJob.SP_JobType);
			AssertEquals("printJob.SP_Destination", "+1234567890", printJob.SP_Destination);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJob.SP_CustomProperties);
				AssertEquals("excelInterface.WorkSheets.Count", 1, excelInterface.WorkSheets.Count);

				var workSheet = excelInterface.WorkSheets[0];
				AssertEquals("workSheet.ToString()", "{B}-[BILL GATES|>MICROSOFT|>MICROSOFT ADDRESS]", workSheet.ToString());
			}
		}

		public void TestDeliverDocumentSaveShowOnlyPrintersUserCanPrintTo()
		{
			var oldShowOnlyPrintersUserCanPrintTo = DocumentsDataRegistry.Instance.ShowOnlyPrintersUserCanPrintTo;

			var (documentCommand, parent, deliveryInstructions, orgPK) = PrepareDataForDeliverDocument();

			var recipient = new DeliveryRecipientBase();
			recipient.Name = "Test Name";
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Print;
			deliveryInstructions.Recipients = new[] { recipient };
			deliveryInstructions.ShowOnlyPrintersUserCanPrintTo = !DocumentsDataRegistry.Instance.ShowOnlyPrintersUserCanPrintTo;
			Factory.Save();

			var webService = new DocumentDeliveryService();
			webService.DeliverDocument(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid(), deliveryInstructions, null);

			var result = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("result.Length", 1, result.Length);
			AssertEquals("ShowOnlyPrintersUserCanPrintTo should be saved after delivery", !oldShowOnlyPrintersUserCanPrintTo, DocumentsDataRegistry.Instance.ShowOnlyPrintersUserCanPrintTo);
		}

		(DocumentCommand documentCommand, BusinessObject parentBizo, DeliveryInstructionsBase deliveryInstructions, Guid organisationPK) PrepareDataForDeliverDocument()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[<RecipientNameAndAddress>]
{A}-[#EndOfReport]");
			template.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob);
			var documentCommand = Factory.New<DocumentCommand>();
			var parent = Factory.New<ICommonShipment>() as BusinessObject;
			var document = documentCommand.Documents.AddNew();

			documentCommand.Parent = parent as IDocumentSupportable;
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			var printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_AllowPrinting = ZBool.True;
			var deliveryInstructions = new DeliveryInstructionsBase();
			deliveryInstructions.PrinterId = printQueue.PK.ToGuid();
			deliveryInstructions.Copies = 6;

			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "Microsoft";
			organization.OH_FullName = "Microsoft";

			var address = organization.Addresses[0];
			AssertNotNull(address);
			address.Address1 = "Microsoft Address";

			Factory.Save();

			return (documentCommand, parent, deliveryInstructions, organization.PK.ToGuid());
		}

		[TestDate(2008, 8, 8, 8, 8, 8)]
		public void TestCoverNote()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[<RecipientNameAndAddress>]
{A}-[#EndOfReport]");
			template.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob);
			var documentCommand = Factory.New<DocumentCommand>();
			var parent = Factory.New<ICommonShipment>() as BusinessObject;
			var document = documentCommand.Documents.AddNew();

			documentCommand.Parent = parent as IDocumentSupportable;
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "Microsoft";
			organization.OH_FullName = "Microsoft";

			var address = organization.Addresses[0];
			AssertNotNull(address);
			address.Address1 = "Microsoft Address";

			var recipient = new DeliveryRecipientBase();
			recipient.OrganizationId = organization.PK.ToGuid();
			recipient.Name = "Bill Gates";
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			recipient.Email = "unit.test@cargowise.com";
			recipient.EmailAttachmentType = Enterprise.MasterFiles.Business.OrgConstants.AttachmentType.PDF;

			var deliveryInstructions = new DeliveryInstructionsBase();
			deliveryInstructions.Recipients = new[] { recipient };
			deliveryInstructions.CoverNote = "Hello World";

			Factory.Save();

			var webService = new DocumentDeliveryService();
			webService.DeliverDocument(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid(), deliveryInstructions, null);

			var result = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("result.Length", 1, result.Length);

			var printJob = result[0];

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJob.SP_CustomProperties);
				AssertEquals("excelInterface.WorkSheets.Count", 3, excelInterface.WorkSheets.Count);

				var coverSheet = excelInterface.WorkSheets[0];

				AssertContains("First worksheet should be the cover sheet.", "Email Cover Sheet", coverSheet.ToString());
				AssertContains("The coversheet should contain our cover note.", "Hello World", coverSheet.ToString());

				var documentWorkSheet = excelInterface.WorkSheets[1];
				AssertEquals("workSheet.ToString()", "{B}-[BILL GATES|>MICROSOFT|>MICROSOFT ADDRESS]", documentWorkSheet.ToString());
			}
		}

		public void TestTestIsDraftSetToTrue()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Draft: <IsDraft>]
{A}-[#EndOfReport]");
			template.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob);
			var documentCommand = Factory.New<DocumentCommand>();
			var parent = Factory.New<ICommonShipment>() as BusinessObject;
			var document = documentCommand.Documents.AddNew();

			documentCommand.Parent = parent as IDocumentSupportable;
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "Microsoft";
			organization.OH_FullName = "Microsoft";
			var recipient = new DeliveryRecipientBase();
			recipient.OrganizationId = organization.PK.ToGuid();
			recipient.Name = "Bill Gates";
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			recipient.Email = "unit.test@cargowise.com";
			recipient.EmailAttachmentType = Enterprise.MasterFiles.Business.OrgConstants.AttachmentType.PDF;

			var deliveryInstructions = new DeliveryInstructionsBase();
			deliveryInstructions.Recipients = new[] { recipient };

			Factory.Save();

			deliveryInstructions.IsDraft = true;

			var webService = new DocumentDeliveryService();
			webService.DeliverDocument(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid(), deliveryInstructions, null);

			var result = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("result.Length", 1, result.Length);

			var printJob = result[0];

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJob.SP_CustomProperties);
				AssertEquals("excelInterface.WorkSheets.Count", 1, excelInterface.WorkSheets.Count);

				var documentWorkSheet = excelInterface.WorkSheets[0];
				AssertEquals("workSheet.ToString()", "{B}-[Draft: Y]", documentWorkSheet.ToString());
			}
		}

		public void TestTestIsDraftSetToFalse()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Draft: <IsDraft>]
{A}-[#EndOfReport]");
			template.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob);
			var documentCommand = Factory.New<DocumentCommand>();
			var parent = Factory.New<ICommonShipment>() as BusinessObject;
			var document = documentCommand.Documents.AddNew();

			documentCommand.Parent = parent as IDocumentSupportable;
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "Microsoft";
			organization.OH_FullName = "Microsoft";
			var recipient = new DeliveryRecipientBase();
			recipient.OrganizationId = organization.PK.ToGuid();
			recipient.Name = "Bill Gates";
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			recipient.Email = "unit.test@cargowise.com";
			recipient.EmailAttachmentType = Enterprise.MasterFiles.Business.OrgConstants.AttachmentType.PDF;

			var deliveryInstructions = new DeliveryInstructionsBase();
			deliveryInstructions.Recipients = new[] { recipient };

			Factory.Save();

			deliveryInstructions.IsDraft = false;

			var webService = new DocumentDeliveryService();
			webService.DeliverDocument(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid(), deliveryInstructions, null);

			var result = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("result.Length", 1, result.Length);

			var printJob = result[0];

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJob.SP_CustomProperties);
				AssertEquals("excelInterface.WorkSheets.Count", 1, excelInterface.WorkSheets.Count);

				var documentWorkSheet = excelInterface.WorkSheets[0];
				AssertEquals("workSheet.ToString()", "{B}-[Draft: N]", documentWorkSheet.ToString());
			}
		}

		public void TestCheckDataState_NoDocumentCommand()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Draft: <IsDraft>]
{A}-[#EndOfReport]");
			template.SO_DataContext = nameof(Core.Constants.DataContext.GenericFreightJob);
			var parent = Factory.New<ICommonShipment>() as BusinessObject;

			Factory.Save();

			var webService = new DocumentDeliveryService();
			var state = webService.CheckDataState(Guid.NewGuid(), parent.TablePrefix, parent.PK.ToGuid());
			AssertEquals("DocumentSupporter Data State", false, state.IsValid);
			AssertEquals("DocumentSupporter Data State", "No Document Menu or Object found.", state.ErrorMessage);
		}

		public void TestCheckDataState_NoMacroExpression()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Draft: <IsDraft>]
{A}-[#EndOfReport]");
			template.SO_DataContext = nameof(Core.Constants.DataContext.GenericFreightJob);
			var documentCommand = Factory.New<DocumentCommand>();
			var parent = Factory.New<ICommonShipment>() as BusinessObject;

			documentCommand.Parent = parent as IDocumentSupportable;

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;

			Factory.Save();

			var webService = new DocumentDeliveryService();
			var state = webService.CheckDataState(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid());
			AssertEquals("DocumentSupporter Data State", true, state.IsValid);
		}

		public void TestCheckDataState_TrueMacroExpression()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Draft: <IsDraft>]
{A}-[#EndOfReport]");
			template.SO_DataContext = nameof(Core.Constants.DataContext.GenericFreightJob);
			var documentCommand = Factory.New<DocumentCommand>();
			var parent = Factory.New<OrgHeader>();
			parent.OH_Code = "MyCode";

			documentCommand.Parent = parent;
			documentCommand.SU_MenuName = "command";
			documentCommand.SU_GS_NKStaffCode = "";
			documentCommand.SU_IsClientSpecific = true;
			documentCommand.SU_IsSystemDefined = true;
			documentCommand.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.UDF);
			documentCommand.SU_DeliveryRestrictionMacro = "'<OH_Code>' == 'MyCode'";

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;

			Factory.Save();

			var webService = new DocumentDeliveryService();
			var state = webService.CheckDataState(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid());
			AssertEquals("DocumentSupporter Data State", true, state.IsValid);
			AssertEquals("DocumentSupporter Data State", string.Empty, state.ErrorMessage);
		}

		public void TestCheckDataState_FalseMacroExpression()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Draft: <IsDraft>]
{A}-[#EndOfReport]");
			template.SO_DataContext = nameof(Core.Constants.DataContext.GenericFreightJob);
			var documentCommand = Factory.New<DocumentCommand>();
			var parent = Factory.New<OrgHeader>();
			parent.OH_Code = "MyCode";

			documentCommand.Parent = parent;
			documentCommand.SU_MenuName = "command";
			documentCommand.SU_GS_NKStaffCode = "";
			documentCommand.SU_IsClientSpecific = true;
			documentCommand.SU_IsSystemDefined = true;
			documentCommand.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.UDF);
			documentCommand.SU_DeliveryRestrictionMacro = "'<OH_Code>' == 'MyCode1'";

			Factory.Save();

			var webService = new DocumentDeliveryService();
			var state = webService.CheckDataState(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid());
			AssertEquals("DocumentSupporter Data State", false, state.IsValid);
			AssertEquals("DocumentSupporter Data State", $"User defined delivery restriction condition is not met. {System.Environment.NewLine}", state.ErrorMessage);
		}

		public void TestCheckDataState_SecurityRight()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Draft: <IsDraft>]
{A}-[#EndOfReport]");
			template.SO_DataContext = nameof(Core.Constants.DataContext.GenericFreightJob);
			var documentCommand = Factory.New<DocumentCommand>();
			var parent = Factory.New<ICommonShipment>() as BusinessObject;
			documentCommand.Parent = parent as IDocumentSupportable;
			documentCommand.SU_IsPublished = true;
			documentCommand.SU_MenuName = "Test Menu Name";

			var documentCommandWithNoSecurity = Factory.New<DocumentCommand>();
			documentCommandWithNoSecurity.Parent = parent as IDocumentSupportable;
			documentCommandWithNoSecurity.SU_IsPublished = true;
			documentCommandWithNoSecurity.SU_MenuName = "Test Menu Name With No Security";

			var privateDocumentCommand = Factory.New<DocumentCommand>();
			privateDocumentCommand.Parent = parent as IDocumentSupportable;
			privateDocumentCommand.SU_IsPublished = false;
			privateDocumentCommand.SU_MenuName = "Test Menu Name Private";

			var controller = Factory.NewWithValidTestData<GlbStaff>();
			controller.GS_IsController = true;

			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			using (Env.SetTemporaryUserContext(controller.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (Env.SetTemporarySecurityInstanceForTest(new SecurityCore(null, controller.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK, Env.CurrentCompanyPK)))
			{
				FindOrCreateCheckpointDocumentCommandForTest(documentCommand);

				var webService = new DocumentDeliveryService();

				var state = webService.CheckDataState(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid());
				AssertEquals("A controller user should has permission granted.", true, state.IsValid);
				AssertEquals("A controller user should has permission granted.", string.Empty, state.ErrorMessage);
			}

			using (Env.SetTemporaryUserContext(glbStaff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var documentCheckpoint = FindOrCreateCheckpointDocumentCommandForTest(documentCommand);
				documentCheckpoint.IsAllowed = true;
				var webService = new DocumentDeliveryService();

				var state = webService.CheckDataState(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid());
				AssertEquals("A user can run document if permission is granted.", true, state.IsValid);
				AssertEquals("A user can run document if permission is granted.", string.Empty, state.ErrorMessage);

				documentCheckpoint.IsAllowed = false;
				state = webService.CheckDataState(documentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid());
				var expectErrorMessage = $"You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights in {BrandingFactory.Instance.ProductName} to allow access to:\r\n\r\nOperate -> Order Manager -> Orders -> Documents -> Test Menu Name";
				AssertEquals("A user cannot run document if permission is ungranted.", false, state.IsValid);
				AssertEquals("A user cannot run document if permission is ungranted.", expectErrorMessage, state.ErrorMessage);

				state = webService.CheckDataState(privateDocumentCommand.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid());
				AssertEquals("A user can run document for a unpublished document.", true, state.IsValid);
				AssertEquals("A user can run document for a unpublished document.", string.Empty, state.ErrorMessage);

				state = webService.CheckDataState(documentCommandWithNoSecurity.PK.ToGuid(), parent.TablePrefix, parent.PK.ToGuid());
				AssertEquals("A document can be run as default if there is no checkpoint found.", true, state.IsValid);
				AssertEquals("A document can be run as default if there is no checkpoint found.", string.Empty, state.ErrorMessage);
			}
		}

		SecurityCheckpoint FindOrCreateCheckpointDocumentCommandForTest(DocumentCommand documentCommand)
		{
			var documentsCheckpoint = Env.Security.FindOrCreateDocumentsCheckpoint(ModuleIDs.Orders, Env.Security.OrderTracking);
			var documentCheckpoint = Env.Security.FindOrCreateDocumentCheckpoint(documentCommand.PK.ToGuid(), documentCommand.SU_MenuNameMultilingual, ModuleIDs.Orders, documentsCheckpoint);
			return documentCheckpoint;
		}

		public void TestCanPreview()
		{
			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_IsController = false;

			var glbSecurity = Factory.New<GlbSecurity>();
			glbSecurity.GU_GS = glbStaff.PK;
			glbSecurity.GU_SecurityItemIsAllowed = false;
			glbSecurity.GU_SecurityRight = "PreviewDocumentButton";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuName = "Test Menu Name";
			documentCommand.SU_IsPublished = true;

			Factory.Save();

			using (Env.SetTemporaryUserContext(glbStaff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var webService = new DocumentDeliveryService();
				var state = webService.CanPreview(documentCommand.PK.ToGuid());
				AssertEquals(false, state);
			}

			glbSecurity.GU_SecurityItemIsAllowed = true;
			Factory.Save();

			using (Env.SetTemporaryUserContext(glbStaff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var webService = new DocumentDeliveryService();
				var state = webService.CanPreview(documentCommand.PK.ToGuid());
				AssertEquals(true, state);
			}
		}
	}
}
