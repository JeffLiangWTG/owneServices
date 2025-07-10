using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using FlexCel.XlsAdapter;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DeliveryMethods.Testing
{
	class QueuedForBatchProcessorTest : DeliveryMethodTest
	{
		public void TestNoBranch()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				AssertCreatedDoc("DOS", "NON");
				AssertCreatedDoc("PFX", "PFX");
				AssertCreatedDoc("NON", "NON");
			}
		}

		public void TestSignByIsSetOnlyForEnabledBranch()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "BR1";

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "BR2";

			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			var branch3 = company3.Branches.AddNew();
			branch3.GB_Code = "BR3";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			DocumentsDataRegistry.Instance.EnableDocumentSigningService.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			DocumentsDataRegistry.Instance.EnableDocumentSigningService.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch1.PK.ToGuid(), Guid.Empty))
			{
				AssertCreatedDoc("DOS", "DOS");
				AssertCreatedDoc("PFX", "PFX");
				AssertCreatedDoc("NON", "NON");
			}
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch2.PK.ToGuid(), Guid.Empty))
			{
				AssertCreatedDoc("DOS", "NON");
				AssertCreatedDoc("PFX", "PFX");
				AssertCreatedDoc("NON", "NON");
			}

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch3.PK.ToGuid(), Guid.Empty))
			{
				AssertCreatedDoc("DOS", "NON");
				AssertCreatedDoc("PFX", "PFX");
				AssertCreatedDoc("NON", "NON");
			}
		}

		void AssertCreatedDoc(string initialSignBy, string expectedSignBy)
		{
			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info.SignBy = initialSignBy;
			info.SetFileContents(SimpleTestXls, "xls");

			var savedPK = testMethod.CreateAndSaveUnprocessedJob(info);
			var savedJob = Factory.Load<StmPrintJob>(savedPK);
			AssertEquals(expectedSignBy, savedJob.SP_SignBy);
		}

		public void TestFailedToDeliver()
		{
			var actionCalled = false;
			var instruction = new DeliveryInstructions();
			instruction.ActionForFailedToDeliver += () => { actionCalled = true; };

			testMethod.Instructions = instruction;
			testMethod.Deliver();
			Assert(actionCalled);
		}

		public void TestFactorySaveStrategy()
		{
			AssertNotNull("Default strategy always available", testMethod.SaveStrategy);
			AssertEquals("Right default", typeof(FactoryStrategy.SaveInChunks).FullName, testMethod.SaveStrategy.GetType().FullName);
			FactoryStrategy strategy = new FactoryStrategy.PopulateButDoNotSave(new BusinessObjectFactory());
			testMethod.SaveStrategy = strategy;
			AssertEquals("Get/Set working", strategy, testMethod.SaveStrategy);
		}

		public void TestTruncateSubjectLine()
		{
			var testInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			testInfo.SetFileContents(SimpleTestXls, "xls");
			testInfo.EmailSubjectLine = new string('x', StmPrintJob.Schema.SP_EmailSubjectLineMaxLength + 1);
			testMethod.DeliveryInfosForTesting.Add(testInfo);
			testMethod.Deliver();

			var job = Factory.Load<StmPrintJob>(new ZQuery())[0];
			AssertEquals("Subject should have been truncated", new string('x', StmPrintJob.Schema.SP_EmailSubjectLineMaxLength), job.SP_EmailSubjectLine);
		}

		public void TestTruncateDocumentName()
		{
			var testInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			testInfo.Name = new string('x', StmPrintJob.Schema.SP_DocumentNameMaxLength + 1);
			testInfo.SetFileContents(SimpleTestXls, "xls");
			testMethod.DeliveryInfosForTesting.Add(testInfo);
			testMethod.Deliver();

			var job = Factory.Load<StmPrintJob>(new ZQuery())[0];
			AssertEquals("DocumetName should have been truncated", new string('x', StmPrintJob.Schema.SP_DocumentNameMaxLength), job.SP_DocumentName);
		}

		public void TestCreateAndSaveUnprocessedJob_Culture()
		{
			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);

			info.IsLocalDocument = true;
			info.SetFileContents(SimpleTestXls, "xls");
			var savedPK = testMethod.CreateAndSaveUnprocessedJob(info);
			var savedJob = Factory.Load<StmPrintJob>(savedPK);
			AssertEquals(true, savedJob.SP_IsLocalCulture);

			info.IsLocalDocument = false;
			savedPK = testMethod.CreateAndSaveUnprocessedJob(info);
			savedJob = Factory.Load<StmPrintJob>(savedPK);
			AssertEquals(false, savedJob.SP_IsLocalCulture);
		}

		public void TestCreateAndSaveUnprocessedJob_WithInvalidDeliveryGroupID()
		{
			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info.SetFileContents(SimpleTestXls, "xls");

			info.DeliveryGroupID = ZGuid.Empty;
			AssertNoExceptionThrown(() => testMethod.CreateAndSaveUnprocessedJob(info));

			info.DeliveryGroupID = ZGuid.Invalid;
			AssertExceptionThrown<InvalidDeliveryGroupException>("Invalid Delivery Group Exception should be clearly described.", expectedExceptionMessage: $"'' cannot be delivered, delivery group ID is '{info.DeliveryGroupID}', reason: 'Delivery group id is invalid'.", () => testMethod.CreateAndSaveUnprocessedJob(info));

			info.DeliveryGroupID = Guid.NewGuid();
			AssertExceptionThrown<InvalidDeliveryGroupException>("Invalid Delivery Group Exception should be clearly described.", expectedExceptionMessage: $"'' cannot be delivered, delivery group ID is '{info.DeliveryGroupID}', reason: 'Delivery group id does not exist in db'.", () => testMethod.CreateAndSaveUnprocessedJob(info));

			var group = Factory.New<StmDeliveryGroup>();
			info.DeliveryGroupID = group.PK;
			group.Delete();
			Factory.Save();
			AssertExceptionThrown<InvalidDeliveryGroupException>("Invalid Delivery Group Exception should be clearly described.", expectedExceptionMessage: $"'' cannot be delivered, delivery group ID is '{info.DeliveryGroupID}', reason: 'Delivery group id does not exist in db'.", () => testMethod.CreateAndSaveUnprocessedJob(info));
		}

		public void TestCreateAndSaveUnprocessedJob_FlexCelLineSpacing()
		{
			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info.SetFileContents(SimpleTestXls, "xls");

			var savedJob = Factory.Load<StmPrintJob>(testMethod.CreateAndSaveUnprocessedJob(info));
			AssertEquals(1m, savedJob.SP_FlexCelLineSpacing);

			info.LineSpacing = 3m;
			savedJob = Factory.Load<StmPrintJob>(testMethod.CreateAndSaveUnprocessedJob(info));
			AssertEquals(3m, savedJob.SP_FlexCelLineSpacing);
		}

		public void TestCreateAndSaveUnprocessedJob_PDFEncryptionPassword()
		{
			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info.SetFileContents(SimpleTestXls, "xls");

			var job = Factory.Load<StmPrintJob>(testMethod.CreateAndSaveUnprocessedJob(info));
			AssertNullOrEmpty(job.SP_PDFEncryptedPassword);

			info.PDFEncryptionPassword = "AAAA";
			job = Factory.Load<StmPrintJob>(testMethod.CreateAndSaveUnprocessedJob(info));
			AssertEquals("AAAA", job.SP_PDFEncryptedPassword);
		}

		public void TestLanguage()
		{
			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info.Name = "Test name";
			info.SetFileContents(SimpleTestXls, "xls");
			var instructions = new DeliveryInstructions();
			instructions.Language = "EN-US";
			testMethod.SetPropertiesFromDeliveryInstructions(instructions);

			var savedJob = Factory.Load<StmPrintJob>(testMethod.CreateAndSaveUnprocessedJob(info));
			AssertEquals("Correct name", "Test name" + StmPrintJob.LanguageDelimiter + "EN-US", savedJob.SP_DocumentName);
		}

		public void TestCreateAndSaveUnprocessedJob_ExceptionAfterCommit()
		{
			var notifications = new NotificationCollection();
			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info.SetFileContents(SimpleTestXls, "xls");

			var savedJob = Factory.Load<StmPrintJob>(testMethod.CreateAndSaveUnprocessedJob(info));
			AssertEquals("Pre-condition", StmPrintJob.LanguageDelimiter, savedJob.SP_DocumentName);

			info.Name = "Test Name";

			testMethod.FactoryOverrideForTest = new BusinessObjectFactory();

			// Set up the error
			void failureCauser(object o_, EventArgs x_)
			{
				var error = SqlExceptionBuilder.CreateSqlError(-2, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, "Timeout expired", "", 0);
				var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				var sqlException = SqlExceptionBuilder.CreateSqlException(errors);

				throw new ZDataException(sqlException, ((INeedRow)Factory.New<DummyBusinessObject>()).Row, Db.Connection);
			}

			testMethod.FactoryOverrideForTest.CheckSaveButNotUpdatedInSameConnectionForTest = true;
			((IBusinessObjectFactoryInternals)testMethod.FactoryOverrideForTest).RowFactory.CommittingTransaction += failureCauser;

			var loadedPrintJob = Factory.Load<StmPrintJob>(testMethod.CreateAndSaveUnprocessedJob(info, false, notifications));

			// Check logs
			var warningNotification = notifications.GetWarnings().FirstOrDefault(x => x.Message == "An error occurred after the Print Job was saved to the database, but the process will continue as normal.");
			AssertNotNull("Warning Notification should exist.", warningNotification);

			// Check StmPrintJob saved successfully
			AssertEquals("Name change should have been saved in DB", "Test Name" + StmPrintJob.LanguageDelimiter, loadedPrintJob.SP_DocumentName);
		}

		public void TestCreateAndSaveUnprocessedJob()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info.FileFormat = "XLS";
			info.EmailSubjectLine = "This is a test email subject line";
			info.ParentTableName = "JobShipment";
			info.ParentGuid = ZGuid.NewZGuid();
			info.RelatedBusinessContext = "SHP";
			info.DocumentType = "ABC";
			info.JobSubmittedBy = GlbStaff.CurrentUser.GS_Code;
			info.DeliveryGroupID = CreateNewDeliveryGroupNOTProcessed().PK;
			info.EmailSignature = "EmailSignature";
			info.EmailFromAddress = "email@abc.com";
			info.AttachedFilename = "blah.txt";
			info.SignBy = DocumentsSignBy.NON;
			info.SetFileContents(SimpleTestXls, "xls");

			var savedPK = testMethod.CreateAndSaveUnprocessedJob(info);
			var savedJob = Factory.Load(typeof(StmPrintJob), savedPK) as StmPrintJob;
			AssertNotNull("Job should exist", savedJob);
			AssertEquals("Saved job should have a RunDateTime", false, savedJob.SP_RunDateTime.IsEmpty);
			AssertEquals("BlobType", info.FileFormat, savedJob.BlobType);
			AssertEquals("Email subject line", info.EmailSubjectLine, savedJob.SP_EmailSubjectLine);
			AssertEquals("Parent table", info.ParentTableName, savedJob.SP_ParentTableName);
			AssertEquals("Parent guid", info.ParentGuid, savedJob.SP_ParentGuid);
			AssertEquals("Related business context", info.RelatedBusinessContext, savedJob.SP_RelatedBusinessContext);
			AssertEquals("Document Type", info.DocumentType, savedJob.SP_DocumentType);
			AssertEquals("Job Submitted By", info.JobSubmittedBy, savedJob.SP_GS_NKJobSubmittedBy);
			AssertEquals("Delivery Group", info.DeliveryGroupID, savedJob.SP_SB_DeliveryGroup);
			AssertEquals("Should be unprocessed", false, savedJob.DeliveryGroup.SB_IsProcessed);
			Assert("Watermark text should be blank", savedJob.SP_WatermarkText.IsEmpty);
			Assert("Watermark image should be blank", savedJob.SP_WatermarkImage.IsEmpty);
			AssertEquals("Email Signature", "EmailSignature", savedJob.SP_EmailSignature);
			AssertEquals("Email From Address", "email@abc.com", savedJob.SP_EmailFromAddress);
			AssertEquals("blah.txt.XLS", savedJob.SP_EmailAttachments);
			AssertEquals("Should NOT have SendToEDocs flag set", false, savedJob.SP_SendToEDocs);
			AssertEquals("Should not be signed", false, savedJob.ShouldSign);
			AssertEquals("Should not be signed", "NON", savedJob.SP_SignBy);

			info.ShowDraftWatermark = true;
			var newPrintJobPK = testMethod.CreateAndSaveUnprocessedJob(info);
			var newPrintJob = Factory.Load<StmPrintJob>(newPrintJobPK);
			AssertEquals("Watermark text should be set", DocumentsDataRegistry.Instance.Watermark.Value.TextWatermark, newPrintJob.SP_WatermarkText);
			Assert("Watermark image should be blank", newPrintJob.SP_WatermarkImage.IsEmpty);

			var previousValue = DocumentsDataRegistry.Instance.Watermark.Value;
			using (var image = Image.FromFile(resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.TransparentPNG.png")))
			{
				try
				{
					var newRegistry = new Watermark();
					newRegistry.UseTextWatermark = false;
					newRegistry.TextWatermark = "Hello";
					newRegistry.HorizontalAlignment = "Left";
					newRegistry.VerticalAlignment = "Top";
					newRegistry.HorizontalOffset = 10;
					newRegistry.VerticalOffset = 20;
					newRegistry.Rotation = 65;
					newRegistry.FontSize = 90;
					newRegistry.Opacity = 30;
					newRegistry.ImageWatermark = image;
					DocumentsDataRegistry.Instance.Watermark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newRegistry);

					info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
					info.FileFormat = "XLS";
					info.EmailSubjectLine = "This is a test email subject line";
					info.ParentTableName = "JobShipment";
					info.ParentGuid = ZGuid.NewZGuid();
					info.RelatedBusinessContext = "SHP";
					info.DocumentType = "ABC";
					info.JobSubmittedBy = GlbStaff.CurrentUser.GS_Code;
					info.ShowDraftWatermark = true;
					info.SetFileContents(SimpleTestXls, "xls");

					newPrintJobPK = testMethod.CreateAndSaveUnprocessedJob(info);
					newPrintJob = Factory.Load<StmPrintJob>(newPrintJobPK);
					Assert("Watermark text should be blank", newPrintJob.SP_WatermarkText.IsEmpty);
					Assert("Watermark image should be set", newPrintJob.SP_WatermarkImage.Length > 0);
				}
				finally
				{
					DocumentsDataRegistry.Instance.Watermark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, previousValue);
				}
			}
		}

		public void TestCreateAndSaveUnprocessedJobAlwaysSavingToEDocs()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info.FileFormat = "XLS";
			info.EmailSubjectLine = "This is a test email subject line";
			info.ParentTableName = "JobShipment";
			info.ParentGuid = ZGuid.NewZGuid();
			info.RelatedBusinessContext = "SHP";
			info.DocumentType = "ABC";
			info.JobSubmittedBy = GlbStaff.CurrentUser.GS_Code;
			info.DeliveryGroupID = CreateNewDeliveryGroupNOTProcessed().PK;
			info.EmailSignature = "EmailSignature";
			info.EmailFromAddress = "email@abc.com";
			info.AttachedFilename = "blah.txt";
			info.SignBy = DocumentsSignBy.PFX;
			info.SetFileContents(SimpleTestXls, "xls");

			var savedPK = testMethod.CreateAndSaveUnprocessedJob(info, true);
			var savedJob = Factory.Load(typeof(StmPrintJob), savedPK) as StmPrintJob;
			AssertNotNull("Job should exist", savedJob);
			AssertEquals("Saved job should have a RunDateTime", false, savedJob.SP_RunDateTime.IsEmpty);
			AssertEquals("BlobType", info.FileFormat, savedJob.BlobType);
			AssertEquals("Email subject line", info.EmailSubjectLine, savedJob.SP_EmailSubjectLine);
			AssertEquals("Parent table", info.ParentTableName, savedJob.SP_ParentTableName);
			AssertEquals("Parent guid", info.ParentGuid, savedJob.SP_ParentGuid);
			AssertEquals("Related business context", info.RelatedBusinessContext, savedJob.SP_RelatedBusinessContext);
			AssertEquals("Document Type", info.DocumentType, savedJob.SP_DocumentType);
			AssertEquals("Job Submitted By", info.JobSubmittedBy, savedJob.SP_GS_NKJobSubmittedBy);
			AssertEquals("Delivery Group", info.DeliveryGroupID, savedJob.SP_SB_DeliveryGroup);
			AssertEquals("Should be unprocessed", false, savedJob.DeliveryGroup.SB_IsProcessed);
			Assert("Watermark text should be blank", savedJob.SP_WatermarkText.IsEmpty);
			Assert("Watermark image should be blank", savedJob.SP_WatermarkImage.IsEmpty);
			AssertEquals("Email Signature", "EmailSignature", savedJob.SP_EmailSignature);
			AssertEquals("Email From Address", "email@abc.com", savedJob.SP_EmailFromAddress);
			AssertEquals("blah.txt.XLS", savedJob.SP_EmailAttachments);
			AssertEquals("Should have SendToEDocs flag set", true, savedJob.SP_SendToEDocs);
			AssertEquals("Should have DigitalSignature flag set", true, savedJob.ShouldSign);
			AssertEquals("Should have DigitalSignature flag set", "PFX", savedJob.SP_SignBy);
		}

		public void TestCreateAndSaveUnprocessedJobWithEmptyCustomProperties()
		{
			var template1 = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test1",
				@"{A}-[#Config]
{A}-[#EndOfReport]");

			var template2 = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test2",
	@"{A}-[#Config]
{A}-[#EndOfReport]");
			var documentSupportable = Factory.New<DummyBODocSupportable>();

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "TestMenu";
			command.Parent = documentSupportable;

			var pivot1 = command.Documents.AddNew();
			pivot1.SI_SU = command.PK;
			pivot1.SI_SO = template1.PK;

			var pivot2 = command.Documents.AddNew();
			pivot2.SI_SU = command.PK;
			pivot2.SI_SO = template2.PK;

			var documentPack = new DocumentPack(command);

			var stmPrintQueue1 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue1.SQ_DisplayName = "Printer 1";
			stmPrintQueue1.SQ_ServerName = "TEST1";

			var stmPrintQueue2 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue2.SQ_DisplayName = "Printer 2";
			stmPrintQueue2.SQ_ServerName = "TEST2";

			var stmPrintQueue3 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue3.SQ_DisplayName = "Printer 3";
			stmPrintQueue3.SQ_ServerName = "TEST3";

			Factory.Save();

			using (var printTask = new PrintTask(command))
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, command, new UserControlProviderList());
				loader.LoadAll();
				var deliveryInstructions = new DeliveryInstructions(printTask[0]);
				deliveryInstructions.PrinterDelivery.PrintQueuePK = stmPrintQueue3.PK;
				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;

				var document1 = (IDeliverable)deliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(document => ((IDeliverable)document).MenuTemplatePivotPK == pivot1.PK);
				document1.PrinterDetails.PrintQueuePK = stmPrintQueue1.PK;
				document1.PrinterDetails.NumberOfCopies = 2;
				var document2 = (IDeliverable)deliveryInstructions.DocumentsToBeDelivered.FirstOrDefault(document => ((IDeliverable)document).MenuTemplatePivotPK == pivot2.PK);
				document2.PrinterDetails.PrintQueuePK = stmPrintQueue2.PK;
				document2.PrinterDetails.NumberOfCopies = 3;

				printTask.SavePrinterDeliveryDefaults(deliveryInstructions);

				var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
				var docDeliveryContact = new DocDeliveryContact(Factory);
				docDeliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				docDeliveryContact.AttachmentType = Core.Constants.FileFormats.PDF;
				var email = new Email(docDeliveryContact);
				info.DocumentPack = documentPack;
				info.Instructions = deliveryInstructions;
				info.Instructions.DeliveryMethod = email;
				info.EmailSubjectLine = "This is a test email subject line";
				info.ParentTableName = "JobShipment";
				info.SetFileContents(Stream.Null, ExcelFileFormatOptionList.Codes.XLSX);

				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
				AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

				var method = new MockQueueEMLMethod();
				method.CreateAndSaveUnprocessedJob(info);
			}
			var errorMessage = "PrintJob's CustomProperties is empty: MenuItemName: [TestMenu]\r\n" +
				String.Format("TemplateList: [PK : {0}, Name : Test1]\r\n[PK : {1}, Name : Test2]\r\n", template1.PK, template2.PK) +
				"Email Subject: [This is a test email subject line]\r\n" +
				"Parent Table Name: [JobShipment]";

			var errorKey = "SP_CustomProperties is empty";

			AssertEquals(errorMessage, ErrorReporter.LastMessageReported);
			AssertEquals("Should send Job Information.", ErrorReporter.LastKeyReported, errorKey);

			ErrorReporter.Clear();
		}

		public void TestCreateAndSaveUnprocessedJobWithNotLimitedAttachmentExtension()
		{
			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info.SetFileContents(SimpleTestXls, "XLSXZZZZZZZZZZZZ");
			info.AttachedFilename = "TestFileName";

			var savedPK = testMethod.CreateAndSaveUnprocessedJob(info);
			var savedJob = Factory.Load<StmPrintJob>(savedPK);
			AssertEquals("BlobType should be truncated", "XLSX", savedJob.BlobType);
			AssertEquals("TestFileName.XLSXZZZZZZZZZZZZ", savedJob.SP_EmailAttachments);
		}

		public void TestCreateAndSaveUnprocessedJobWithNoFilecontents()
		{
			DeliveryInfo info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info.DeliveryGroupID = CreateNewDeliveryGroupNOTProcessed().PK;
			string contents = "abcdefg";

			using (StreamWriter writer = new StreamWriter(info.FileContents, System.Text.Encoding.ASCII, 10))
			{
				writer.Write(contents);
				writer.Flush();

				ZGuid savedPK = testMethod.CreateAndSaveUnprocessedJob(info);
				StmPrintJob savedJob = Factory.Load(typeof(StmPrintJob), savedPK) as StmPrintJob;

				info.FileContents.Position = 0;
				using (StreamReader reader = new StreamReader(info.FileContents))
				{
					string lineReturned = reader.ReadLine();

					AssertEquals("Should not be processed", false, savedJob.DeliveryGroup.SB_IsProcessed);
					Assert("Saved Job Custom Properties shouldn't be empty", !savedJob.SP_CustomProperties.IsEmpty);
					AssertEquals("Saved Job custom properties should be the same as the file contents", contents, lineReturned);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateAndSaveUnprocessedJobWithPdfFormatPrinting()
		{
			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document)
			{
				DeliveryGroupID = CreateNewDeliveryGroupNOTProcessed().PK,
				AttachedFilename = "TestFileName"
			};
			info.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "SimpleTestWithOLEObject.xls", FileMode.Open, FileAccess.Read), "XLS");

			TestPdfFormat(true, "TestFileName.PDF", true, info);
			TestPdfFormat(false, "TestFileName.XLS", false, info);
			TestPdfFormat(true, "TestFileName.XLS", false, info, new MockQueueEMLMethod());

			info.SetFileContents(TIFPage, "TIF");
			TestPdfFormat(true, "TestFileName.TIF", false, info);
		}

		void TestPdfFormat(bool shouldUsePdfFormatForPrinting, string attachmentName, bool isPdfExpected, DeliveryInfo info, MockQueueMethod method = null)
		{
			if (method == null)
			{
				method = new MockQueueMethod();
			}

			using (DocumentsDataRegistry.Instance.DeliverDocumentsToPrintersInPdfFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, shouldUsePdfFormatForPrinting))
			{
				var savedPk = method.CreateAndSaveUnprocessedJob(info);
				var savedJob = Factory.Load<StmPrintJob>(savedPk);

				AssertEquals(attachmentName, savedJob.SP_EmailAttachments);
				AssertEquals(isPdfExpected, ImageToPDFConverter.IsPDF(savedJob.SP_CustomProperties));
			}
		}

		public void TestConsolidateReports()
		{
			var infos = new DeliveryInfo[10];
			var deliveryGroup = CreateNewDeliveryGroupNOTProcessed();
			for (var i = 0; i < infos.Length; i++)
			{
				infos[i] = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
				infos[i].SetFileContents(AutoHeightLinesMakePageBreakWrongXls, "xls");
				infos[i].DeliveryGroupID = deliveryGroup.PK;
			}

			try
			{
				testMethod.DeliveryInfosForTesting.AddRange(infos);
				testMethod.EmailSubjectForConsolidateReports = "this is a test";
				testMethod.AttachedFileNameForConsolidateReports = "this is also a test";
				testMethod.Deliver();

				var orderBy = new ZDBOnlyQuery(typeof(StmPrintJob));
				orderBy.OrderBy = StmPrintJob.Schema.SP_Group + ", " + StmPrintJob.Schema.SP_Sequence + ", newid()"; // newid() checks that there are no duplicates.
				var savedJobs = Factory.Load<StmPrintJob>(orderBy);
				AssertEquals("Load count", 1, savedJobs.Length);

				var firstGroupId = savedJobs[0].SP_Group;

				Assert("Group ID 1 should not be empty", !firstGroupId.IsEmpty);
				AssertEquals("Should not be processed", false, savedJobs[0].DeliveryGroup.SB_IsProcessed);
				AssertEquals("Group ID", firstGroupId, savedJobs[0].SP_Group);
				AssertEquals("this is a test", savedJobs[0].SP_EmailSubjectLine);
				AssertEquals("this is also a test.XLS", savedJobs[0].SP_EmailAttachments);
			}
			finally
			{
				CleanUpDeliveryInfos(infos);
			}
		}

		public void TestConsolidateReportsWithTIFAndExcelFiles()
		{
			AssertConsolidateReportsWithTIFAndExcelFiles(false);
		}

		public void TestConsolidateReportsWithTIFAndExcelFilesWithMergeTIFs()
		{
			testMethod = new MockQueueMethod { MergeTiffs = true };
			AssertConsolidateReportsWithTIFAndExcelFiles(true);
		}

		public void TestConsolidateReportsWithTIFAndExcelFilesAndCoverSheets()
		{
			AssertConsolidateReportsWithTIFAndExcelFilesAndCoverSheets(false);
		}

		public void TestConsolidateReportsWithTIFAndExcelFilesAndCoverSheetsWithMergeTIFs()
		{
			testMethod = new MockQueueMethod { MergeTiffs = true };
			AssertConsolidateReportsWithTIFAndExcelFilesAndCoverSheets(true);
		}

		public void TestFileAttachmentsAreDelivered()
		{
			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.File);
			info.SetFileContents(SimpleTestXls, "XYZ");
			testMethod.DeliveryInfosForTesting.Add(info);
			testMethod.Deliver();
			var printJobs = new StmPrintJobCollection(Factory);
			printJobs.Load();
			AssertEquals("StmPrintJob Count", 1, printJobs.Count);
			AssertEquals("Print Job's BlobType", "XYZ", printJobs[0].BlobType);
		}

		public void TestSavedInOrder()
		{
			var infos = new DeliveryInfo[10];
			var deliveryGroup = CreateNewDeliveryGroupNOTProcessed();
			int sequenceAtStart = QueuedForBatchProcessor.Sequence;
			try
			{
				for (var i = 0; i < infos.Length; i++)
				{
					infos[i] = CreateNewDeliveryInfo(deliveryGroup.PK);
				}

				testMethod.DeliveryInfosForTesting.AddRange(infos);
				testMethod.Deliver();

				testMethod = new MockQueueMethod();
				testMethod.DeliveryInfosForTesting.AddRange(infos);
				testMethod.Deliver();

				var orderBy = new ZDBOnlyQuery(typeof(StmPrintJob));
				orderBy.OrderBy = StmPrintJob.Schema.SP_Group + ", " + StmPrintJob.Schema.SP_Sequence + ", newid()"; // newid() checks that there are no duplicates.
				var savedJobs = Factory.Load<StmPrintJob>(orderBy);
				AssertEquals("Load count", 2, savedJobs.Length);

				var firstGroupId = savedJobs[0].SP_Group;
				var secondGroupId = savedJobs[1].SP_Group;
				Assert("Group IDs should be the same", firstGroupId == secondGroupId);
				Assert("Group ID 1 should not be empty", !firstGroupId.IsEmpty);
				Assert("Group ID 2 should not be empty", !secondGroupId.IsEmpty);

				for (var i = 0; i < savedJobs.Length; i++)
				{
					AssertEquals("Should not be processed", false, savedJobs[i].DeliveryGroup.SB_IsProcessed);
					AssertEquals("SP_Sequence out of order", sequenceAtStart + i, savedJobs[i].SP_Sequence);
					AssertEquals("Group ID", firstGroupId, savedJobs[i].SP_Group);
				}
			}
			finally
			{
				CleanUpDeliveryInfos(infos);
			}
		}

		public void TestDeliveryDoesNotSetProcessedFlagByDefault()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			var infos = new DeliveryInfo[5];
			var deliveryGroup = CreateNewDeliveryGroupNOTProcessed();
			try
			{
				for (var i = 0; i < infos.Length; i++)
				{
					infos[i] = CreateNewDeliveryInfo(deliveryGroup.PK);
				}

				testMethod.DeliveryInfosForTesting.AddRange(infos);
				testMethod.Deliver();

				var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
				foreach (var printJob in printJobs)
				{
					AssertEquals("PrintJobs shouldn't be processed by default - it should be handled by the docpack", false, printJob.DeliveryGroup.SB_IsProcessed);
				}
			}
			finally
			{
				CleanUpDeliveryInfos(infos);
			}
		}

		public void TestMergeTiffsOnDelivery()
		{
			DeliveryInstructions instructions = new DeliveryInstructions();
			DocDeliveryContact docContact = new DocDeliveryContact(Factory);
			docContact.Name = "Zeus";
			docContact.CompanyName = "Company Co.";
			docContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			docContact.AttachmentType = "XLS";
			docContact.Email = "test@edi.com.au";
			docContact.Fax = "324324324234";
			instructions.Recipients.Add(docContact);
			DocDeliveryPrintDetails printDetails = new DocDeliveryPrintDetails(Factory);

			Fax method1 = new Fax(docContact);
			docContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			Email method2 = new Email(docContact);
			Printer method3 = new Printer(printDetails);

			Assert(method1.MergeTiffsOnDeliveryForTesting);
			Assert(!method2.MergeTiffsOnDeliveryForTesting);
			Assert(!method3.MergeTiffsOnDeliveryForTesting);
		}

		public void TestDeliveryDoesSetProcessedFlagIfSetProcessedOnDeliveryIsTrue()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			DeliveryInfo[] infos = new DeliveryInfo[5];
			StmDeliveryGroup deliveryGroup = CreateNewDeliveryGroupNOTProcessed();
			try
			{
				for (int i = 0; i < infos.Length; i++)
				{
					infos[i] = CreateNewDeliveryInfo(deliveryGroup.PK);
				}

				testMethod.DeliveryInfosForTesting.AddRange(infos);
				testMethod.Deliver();

				StmPrintJob[] printJobs = (StmPrintJob[])Factory.Load(typeof(StmPrintJob), new ZQuery());
				foreach (StmPrintJob printJob in printJobs)
				{
					Assert("DeliveryGroup should be processed because SetProcessedOnDelivery was false", !printJob.DeliveryGroup.SB_IsProcessed);
				}
			}
			finally
			{
				CleanUpDeliveryInfos(infos);
			}
		}

		public void TestLogDelivery_Consolidate()
		{
			AssertLogDelivery(new[]
			{
				"PRN|info 1|info 2|info 3"
			},
			true);
		}

		public void TestLogDelivery_DoNotConsolidate()
		{
			AssertLogDelivery(new[]
			{
				"PRN|info 1",
				"PRN|info 2",
				"PRN|info 3"
			},
			false);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);
			testMethod = new MockQueueMethod();
		}

		MockQueueMethod testMethod;

		void AssertLogDelivery(string[] expected, bool consolidate)
		{
			var factory = new BusinessObjectFactory();
			var deliveryInstructions = new DeliveryInstructions(new FactoryStrategy.PopulateButDoNotSave(factory));

			var batchProcessor = new MockQueueMethod();
			batchProcessor.Consolidate = consolidate;
			batchProcessor.SetPropertiesFromDeliveryInstructions(deliveryInstructions);

			var excelFile = new Mock<XlsFile>();

			batchProcessor.XlsFileGenerator = () => excelFile.Object;

			var infos = new DeliveryInfo[3];
			var deliveryGroup = CreateNewDeliveryGroupNOTProcessed();

			try
			{
				infos[0] = CreateNewDeliveryInfo(deliveryGroup.PK);
				infos[0].Name = "info 1";

				infos[1] = CreateNewDeliveryInfo(deliveryGroup.PK);
				infos[1].Name = "info 2";

				infos[2] = CreateNewDeliveryInfo(deliveryGroup.PK);
				infos[2].Name = "info 3";

				batchProcessor.DeliveryInfosForTesting.AddRange(infos);

				excelFile.Setup(m => m.SheetCount).Returns(3);

				batchProcessor.Deliver();

				var loggedJobs = new List<string>();

				var link = PrintJobDeliveryInfosLink.GetInstance(factory);

				foreach (var printJob in link.PrintJobs)
				{
					var deliveries = link.Get(printJob);

					loggedJobs.Add($"{printJob.SP_JobType}|{string.Join("|", deliveries.Select(d => d.Name))}");
				}

				AssertContainsExactElementsInAnyOrder("logged jobs",
					expected,
					loggedJobs);
			}
			finally
			{
				CleanUpDeliveryInfos(infos);
				ErrorReporter.Clear();
			}
		}

		DeliveryInfo CreateNewDeliveryInfo(ZGuid deliveryGroupPK)
		{
			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var templateStream = embeddedResourceRetriever.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls");
				var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
				Assert("PreCondition: FileStream Length > 0", templateStream.Length > 0);
				info.SetFileContents(templateStream, "xls");
				info.DeliveryGroupID = deliveryGroupPK;
				return info;
			}
		}

		StmDeliveryGroup CreateNewDeliveryGroupNOTProcessed()
		{
			StmDeliveryGroup deliveryGroup = Factory.New<StmDeliveryGroup>();
			Factory.Save();
			return deliveryGroup;
		}

		void CleanUpDeliveryInfos(DeliveryInfo[] infos)
		{
			for (int i = 0; i < infos.Length; i++)
			{
				if (infos[i] != null && infos[i].FileContents != null)
				{
					infos[i].FileContents.Close();
				}
			}
		}

		void AssertConsolidateReportsWithTIFAndExcelFiles(bool mergeTiffs)
		{
			var infos = new DeliveryInfo[10];
			var deliveryGroup = CreateNewDeliveryGroupNOTProcessed();
			for (var i = 0; i < infos.Length; i++)
			{
				infos[i] = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
				infos[i].SetFileContents(AutoHeightLinesMakePageBreakWrongXls, "xls");
				infos[i].DeliveryGroupID = deliveryGroup.PK;
			}

			try
			{
				// randomly pick #3 and #8 to be TIF files
				infos[3].SetFileContents(TIFPage, "tif");
				infos[3].DeliveryFormat = DeliveryInfo.DeliveryFormats.TIFF;
				infos[8].SetFileContents(TIFPage, "tif");
				infos[8].DeliveryFormat = DeliveryInfo.DeliveryFormats.TIFF;

				testMethod.DeliveryInfosForTesting.AddRange(infos);
				testMethod.EmailSubjectForConsolidateReports = "this is a test";
				testMethod.Deliver();

				var orderBy = new ZDBOnlyQuery(typeof(StmPrintJob));
				orderBy.OrderBy = StmPrintJob.Schema.SP_Group + ", " + StmPrintJob.Schema.SP_Sequence + ", newid()"; // newid() checks that there are no duplicates.
				var savedJobs = Factory.Load<StmPrintJob>(orderBy);
				AssertEquals("Load count - 1 merged print job, 2 tif print jobs (or 1 merged tif print job)", mergeTiffs ? 2 : 3, savedJobs.Length);

				var firstGroupId = savedJobs[0].SP_Group;
				Assert("Group ID 1 should not be empty", !firstGroupId.IsEmpty);
				AssertEquals("Group ID", firstGroupId, (savedJobs[0]).SP_Group);

				AssertEquals("Should not be processed", false, savedJobs[1].DeliveryGroup.SB_IsProcessed);
				AssertEquals("Saved jobs for TIF Files should have TIF file contents", "TIF", savedJobs[1].BlobType);
				AssertEquals("Saved jobs for TIF Files should be delivered as TIF", "TIF", savedJobs[1].SP_EmailAttachmentFormat.ToUpper());
			}
			finally
			{
				CleanUpDeliveryInfos(infos);
			}
		}

		void AssertConsolidateReportsWithTIFAndExcelFilesAndCoverSheets(bool mergeTiffs)
		{
			var infos = new DeliveryInfo[10];
			var deliveryGroup = CreateNewDeliveryGroupNOTProcessed();
			for (var i = 0; i < infos.Length; i++)
			{
				infos[i] = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
				var fileStream = AutoHeightLinesMakePageBreakWrongXls;
				Assert("Precondition: FileStream.Length must be > 0", fileStream.Length > 0);
				infos[i].SetFileContents(fileStream, "xls");
				infos[i].EmailSubjectLine = "XLS File subject line";
				infos[i].ParentTableName = "JobShipment";
				infos[i].RelatedBusinessContext = "SHP";
				infos[i].DeliveryGroupID = deliveryGroup.PK;
			}

			try
			{
				// randomly pick #3 and #8 to be TIF files
				infos[3].SetFileContents(TIFPage, "tif");
				infos[3].DeliveryFormat = DeliveryInfo.DeliveryFormats.TIFF;
				infos[8].SetFileContents(TIFPage, "tif");
				infos[8].DeliveryFormat = DeliveryInfo.DeliveryFormats.TIFF;

				// make the first a 'cover sheet' that shouldn't get picked up for merge
				infos[0].IsCoverSheet = true;
				infos[0].EmailSubjectLine = "Cover sheet subject line";
				infos[0].ParentTableName = ZString.Empty;
				infos[0].RelatedBusinessContext = ZString.Empty;

				testMethod.DeliveryInfosForTesting.AddRange(infos);
				testMethod.EmailSubjectForConsolidateReports = "this is a test";
				testMethod.Deliver();

				var orderBy = new ZDBOnlyQuery(typeof(StmPrintJob));
				orderBy.OrderBy = StmPrintJob.Schema.SP_Group + ", " + StmPrintJob.Schema.SP_Sequence + ", newid()"; // newid() checks that there are no duplicates.
				var savedJobs = Factory.Load<StmPrintJob>(orderBy);
				AssertEquals("Load count - 1 merged print job, 2 tif print jobs (or 1 merged tif print job)", mergeTiffs ? 2 : 3, savedJobs.Length);

				AssertEquals("Should not be processed", false, savedJobs[0].DeliveryGroup.SB_IsProcessed);
				AssertEquals("Should not be processed", false, savedJobs[1].DeliveryGroup.SB_IsProcessed);
				AssertEquals("First job is the merged xls", "XLS", savedJobs[0].BlobType);
				Assert("First job should have a related business context - should have ignored the first delivery info which didn't have a business context because it was a cover sheet", !savedJobs[0].SP_RelatedBusinessContext.IsEmpty);
				AssertEquals("Saved jobs for TIF Files should have TIF file contents", "TIF", savedJobs[1].BlobType);
				AssertEquals("Saved jobs for TIF Files should be delivered as TIF", "TIF", savedJobs[1].SP_EmailAttachmentFormat.ToUpper());
			}
			finally
			{
				CleanUpDeliveryInfos(infos);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		Stream SimpleTestXls => resourceRetriever.Value.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.SimpleTest.xls");

		Stream TIFPage => resourceRetriever.Value.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.TIFPage.tif");

		Stream AutoHeightLinesMakePageBreakWrongXls => resourceRetriever.Value.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.AutoHeightLinesMakePageBreakWrong.xls");

		class MockQueueMethod : QueuedForBatchProcessor
		{
			public MockQueueMethod()
				: base()
			{
				MergeTiffs = base.MergeTiffsOnDelivery;
				Consolidate = base.ConsolidateReports;
			}

			public ZGuid CreateAndSaveUnprocessedJob(DeliveryInfo info, bool sendToEDocs = false, INotifications notifications = null)
			{
				return base.CreateAndSaveUnprocessedJob(info, sendToEDocs, false, notifications);
			}

			public new void SetPropertiesFromDeliveryInstructions(DeliveryInstructions instructions)
			{
				base.SetPropertiesFromDeliveryInstructions(instructions);
			}

			protected override PrintType PrintType
			{
				get { return PrintType.PRN; }
			}

			public bool MergeTiffs { get; set; }

			protected override bool MergeTiffsOnDelivery => MergeTiffs;

			public bool Consolidate { get; set; }

			protected override bool ConsolidateReports => Consolidate;
		}

		sealed class MockQueueEMLMethod : MockQueueMethod
		{
			protected override PrintType PrintType => PrintType.EML;
		}
	}
}
