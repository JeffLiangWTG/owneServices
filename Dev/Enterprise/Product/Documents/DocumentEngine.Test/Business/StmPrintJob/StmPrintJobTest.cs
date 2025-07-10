using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.FlexCelInterface.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.Exceptions;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineCore.Registry.Testing;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.RemotePrinting.Engine;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using PdfSharp.Pdf.IO;
using static Enterprise.Core.Constants;
using DocumentConverter = Enterprise.DocumentEngine.FileFormatUtilities.DocumentConverter;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestedType(typeof(StmPrintJob))]
	sealed class StmPrintJobTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTrigger_WhenSP_JobTypeUpdated_StmPrintJobQueueRecordIsDeleted()
		{
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_JobType = nameof(PrintJobType.EML);

			AssertPrintJobIsDeletedFromQueue(printJob, job =>
			{
				job.SP_JobType = nameof(PrintJobType.DDS);
			});
		}

		public void TestTrigger_WhenSP_StatusUpdated_StmPrintJobQueueRecordIsDeleted()
		{
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_Status = nameof(PrintJobStatus.QUE);

			AssertPrintJobIsDeletedFromQueue(printJob, job =>
			{
				job.SP_Status = nameof(PrintJobStatus.FAL);
			});
		}

		public void TestTrigger_WhenSP_RetryAttemptsUpdated_StmPrintJobQueueRecordIsDeleted()
		{
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_RetryAttempts = 0;

			AssertPrintJobIsDeletedFromQueue(printJob, job =>
			{
				job.SP_RetryAttempts = 2;
			});
		}

		public void TestTrigger_WhenSP_EDocsProcessedUpdated_StmPrintJobQueueRecordIsDeleted()
		{
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_EDocsProcessed = ZBool.False;

			AssertPrintJobIsDeletedFromQueue(printJob,
				job =>
				{
					job.SP_EDocsProcessed = ZBool.True;
				},
				true);
		}

		public void TestCascadeDelete_WhenDeleted_StmPrintJobQueueRecordIsDeleted()
		{
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_RunDateTime = ZDateTime.UtcNow;

			AssertPrintJobIsDeletedFromQueue(printJob, job =>
			{
				job.Delete();
			});
		}

		public void TestReportErrorWithoutDestination()
		{
			ErrorReporter.Clear();
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = "EML";
			Factory.Save();

			AssertEquals("Should no error", string.Empty, ErrorReporter.LastMessageReported);

			var factory = new BusinessObjectFactory();
			var jobInDb = factory.Load<StmPrintJob>(printJob.PK);
			jobInDb.ReportErrorWithoutDestinationForTest = true;
			jobInDb.SP_EmailSubjectLine = "Jerry test change";
			factory.Save();

			AssertEquals("Should no error", string.Empty, ErrorReporter.LastMessageReported);

			var newPrintJob = Factory.New<StmPrintJob>();
			newPrintJob.SP_JobType = "EML";
			newPrintJob.ReportErrorWithoutDestinationForTest = true;
			Factory.Save();

			AssertStartsWith("Should have an error", "Destination is not specified when a StmPrintJob is saving. SP_JobType: EML", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestSP_EmailSignatureShouldSupportChinese()
		{
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_EmailSignature = "Jerry 简体中文";
			Factory.Save();

			AssertEquals("SP_EmailSignatureInfo has no errors", false, printJob.SP_EmailSignatureInfo.HasErrors());

			var factory = new BusinessObjectFactory();
			var loadJob = factory.Load<StmPrintJob>(printJob.PK);

			AssertEquals("SP_EmailSignature should support Chinese", "Jerry 简体中文", loadJob.SP_EmailSignature);
		}

		void Test_EmailAttachementsShouldHaveLowerCase(string jobType)
		{
			var testParentGuid = ZGuid.NewZGuid();

			var printJob1 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob1.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			printJob1.SP_JobType = jobType;
			printJob1.SP_Destination = "test@example.com";
			printJob1.SP_DocumentName = "Test";
			printJob1.SP_EmailSubjectLine = "email subject";
			printJob1.SP_ParentTableName = "JobShipment";
			printJob1.SP_ParentGuid = testParentGuid;
			printJob1.SP_RunDateTime = ZDateTime.UtcNow;
			printJob1.SP_SQ = ZGuid.Empty;
			printJob1.SP_CustomProperties = ReportTestXls;
			printJob1.SP_EmailAttachments = "test.XLS";
			printJob1.SP_EmailAttachmentFormat = "PDF";

			var printJob2 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob2.SP_EmailAttachments = "test.TIF";
			printJob2.SP_EmailAttachmentFormat = "JPG";
			printJob2.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			using (var bitmap = new Bitmap(1, 1))
			{
				using (var stream = new MemoryStream())
				{
					bitmap.Save(stream, ImageFormat.Jpeg);

					printJob2.SP_CustomProperties = stream.CopyToByteArray();
				}
			}

			var printJob3 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob3.SP_JobType = jobType;
			printJob3.SP_EmailAttachments = "test.XLS";
			printJob3.SP_EmailAttachmentFormat = "PNG";
			printJob3.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			printJob3.SP_CustomProperties = ReportTestXls;

			AssertExtensionIsLowercase(printJob1);
			AssertExtensionIsLowercase(printJob2);
			AssertExtensionIsLowercase(printJob3);
		}

		void AssertExtensionIsLowercase(StmPrintJob printJob)
		{
			try
			{
				printJob.SaveAttachmentToFilesystem(0);
				var extension = Path.GetExtension(printJob.StoredAttachmentFilename);
				AssertEquals("Extension printJob should be lowercase", extension.ToLower(), extension);
			}
			finally
			{
				File.Delete(printJob.StoredAttachmentFilename);
			}
		}

		public void Test_EmailAttachementsShouldHaveLowerCase_FromEmail()
		{
			Test_EmailAttachementsShouldHaveLowerCase(nameof(PrintType.EML));
		}

		public void Test_EmailAttachementsShouldHaveLowerCase_FromFax()
		{
			Test_EmailAttachementsShouldHaveLowerCase(nameof(PrintType.FAX));
		}

		[ExpectNoExceptions()]
		public void TestSQ_ServerName()
		{
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			AssertEquals(ZString.Empty, printJob.SQ_ServerName);

			var printQueue = Factory.New<StmPrintQueue>();
			printJob.SP_SQ = printQueue.PK;
			AssertEquals(ZString.Empty, printJob.SQ_ServerName);

			printQueue.SQ_ServerName = "Anton";
			AssertEquals("Anton", printJob.SQ_ServerName);
		}

		public void TestPRNJobDestinationNotClearedAfterDelivered()
		{
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			AssertEquals(ZString.Empty, printJob.SQ_ServerName);

			var printQueue = Factory.New<StmPrintQueue>();
			printJob.SP_SQ = printQueue.PK;
			printJob.SP_JobType = "PRN";
			printJob.SP_RelatedBusinessContext = Core.Constants.DocManagerCodes.Shipment;
			printQueue.SQ_DisplayName = "Dummy Printer";
			AssertEquals("Dummy Printer", printJob.SP_Destination);

			printJob.NotifyDelivered();
			AssertEquals("Dummy Printer", printJob.SP_Destination);
		}

		[ExpectNoExceptions]
		public void TestSaveWithTooLongFileName()
		{
			var fileName = new string('X', 250);
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_EmailAttachments = fileName + ".TIF";
			printJob.SP_EmailAttachmentFormat = "JPG";
			printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			using (var bitmap = new Bitmap(1, 1))
			{
				using (var stream = new MemoryStream())
				{
					bitmap.Save(stream, ImageFormat.Jpeg);

					printJob.SP_CustomProperties = stream.CopyToByteArray();
				}
			}

			try
			{
				printJob.SaveAttachmentToFilesystem(0);
			}
			finally
			{
				if (File.Exists(printJob.StoredAttachmentFilename))
				{
					File.Delete(printJob.StoredAttachmentFilename);
				}
			}
		}

		public void TestSaveTwoAttachmentsToFileSystemWithSameNameThatIsConvertableToJPG()
		{
			var printJob1 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob1.SP_EmailAttachments = "test.TIF";
			printJob1.SP_EmailAttachmentFormat = "JPG";
			printJob1.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			using (var bitmap = new Bitmap(1, 1))
			{
				using (var stream = new MemoryStream())
				{
					bitmap.Save(stream, ImageFormat.Jpeg);

					printJob1.SP_CustomProperties = stream.CopyToByteArray();
				}
			}

			var printJob2 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob2.SP_EmailAttachments = "test.TIF";
			printJob2.SP_EmailAttachmentFormat = "JPG";
			printJob2.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			using (var bitmap = new Bitmap(1, 1))
			{
				using (var stream = new MemoryStream())
				{
					bitmap.Save(stream, ImageFormat.Jpeg);

					printJob2.SP_CustomProperties = stream.CopyToByteArray();
				}
			}

			try
			{
				printJob1.SaveAttachmentToFilesystem(0);
				Assert(printJob1.StoredAttachmentFilename, printJob1.StoredAttachmentFilename.EndsWith("test.jpg"));

				printJob2.SaveAttachmentToFilesystem(0);
				Assert(printJob2.StoredAttachmentFilename, printJob2.StoredAttachmentFilename.EndsWith("test[1].jpg"));
			}
			finally
			{
				File.Delete(printJob1.StoredAttachmentFilename);
				File.Delete(printJob2.StoredAttachmentFilename);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestXLSFileShouldBeEncryptedIfExistsPassword()
		{
			var file = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestXLSFileName;
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_EmailAttachments = "test" + ".XLS";
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			printJob.SP_CustomProperties = File.ReadAllBytes(file);
			printJob.SP_ExcelEncryptedPassword = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt("password");

			printJob.SaveAttachmentToFilesystem(0);
			AssertExcelFileIsEncrypted(printJob.StoredAttachmentFilename, TwoWayEncoder.NewWithStandardInitialisationVector().Decrypt(printJob.SP_ExcelEncryptedPassword));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestXLSXFileShouldBeEncryptedIfExistsPassword()
		{
			var file = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestXLSXFileName;
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_EmailAttachments = "test" + ".XLS";
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			printJob.SP_CustomProperties = File.ReadAllBytes(file);
			printJob.SP_ExcelEncryptedPassword = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt("password");

			printJob.SaveAttachmentToFilesystem(0);
			AssertExcelFileIsEncrypted(printJob.StoredAttachmentFilename, TwoWayEncoder.NewWithStandardInitialisationVector().Decrypt(printJob.SP_ExcelEncryptedPassword));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPDFShouldBeEncryptedIfExistsPassword()
		{
			var file = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFFileName;
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_EmailAttachments = "test" + ".PDF";
			printJob.SP_EmailAttachmentFormat = "PDF";
			printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			printJob.SP_CustomProperties = File.ReadAllBytes(file);

			printJob.SaveAttachmentToFilesystem(0);
			AssertPDFFileIsEncrypted(printJob.StoredAttachmentFilename, printJob.SP_PDFEncryptedPassword);

			printJob.SP_PDFEncryptedPassword = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt("password");

			printJob.SaveAttachmentToFilesystem(0);
			AssertPDFFileIsEncrypted(printJob.StoredAttachmentFilename, printJob.SP_PDFEncryptedPassword);
		}

		public void TestPDFConvertedFromXlsShouldBeEncryptedIfExistsPassword()
		{
			var testParentGuid = ZGuid.NewZGuid();

			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "PDF";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = ReportTestXls;
			printJob.SP_EmailAttachments = "test.xls";
			printJob.SP_EmailSubjectLine = "email subject";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SQ = ZGuid.Empty;

			printJob.SaveAttachmentToFilesystem(0);
			AssertPDFFileIsEncrypted(printJob.StoredAttachmentFilename, printJob.SP_PDFEncryptedPassword);

			printJob.SP_PDFEncryptedPassword = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt("password");

			printJob.SaveAttachmentToFilesystem(0);
			AssertPDFFileIsEncrypted(printJob.StoredAttachmentFilename, printJob.SP_PDFEncryptedPassword);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPDFPasswordMaxLength()
		{
			var file = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFFileName;
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_EmailAttachments = "test" + ".PDF";
			printJob.SP_EmailAttachmentFormat = "PDF";
			printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			printJob.SP_CustomProperties = File.ReadAllBytes(file);

			var toolongPassword = new String('X', 40);
			printJob.SP_PDFEncryptedPassword = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(toolongPassword);

			printJob.SaveAttachmentToFilesystem(0);

			var actualPassword = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(new String('X', 32));
			AssertPDFFileIsEncrypted(printJob.StoredAttachmentFilename, actualPassword);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPDFPasswordWithFullWidthCharacters()
		{
			var file = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFFileName;
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_EmailAttachments = "test" + ".PDF";
			printJob.SP_EmailAttachmentFormat = "PDF";
			printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			printJob.SP_CustomProperties = File.ReadAllBytes(file);

			var fullWidthCharactors = new String('密', 32);
			printJob.SP_PDFEncryptedPassword = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(fullWidthCharactors);

			Factory.Save();

			printJob.SaveAttachmentToFilesystem(0);
			AssertPDFFileIsEncrypted(printJob.StoredAttachmentFilename, printJob.SP_PDFEncryptedPassword);
		}

		public void TestConvertXLSToHTML()
		{
			var testParentGuid = ZGuid.NewZGuid();

			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "HTML";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = ReportTestXls;
			printJob.SP_EmailAttachments = "test.xls";
			printJob.SP_EmailSubjectLine = "email subject";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SQ = ZGuid.Empty;

			Factory.Save();

			try
			{
				printJob.SaveAttachmentToFilesystem(0);
				Assert(printJob.StoredAttachmentFilename.EndsWith("HTML", StringComparison.OrdinalIgnoreCase));
				var htmlAsString = File.ReadAllText(printJob.StoredAttachmentFilename);
				AssertContains("<meta name=\"Generator\" content=\"FlexCel", htmlAsString);
				var imagesDirectory = Path.Combine(Path.GetDirectoryName(printJob.StoredAttachmentFilename), DocumentConverter.GetSafeImagesDirectoryName(printJob.StoredAttachmentFilename));
				AssertEquals(8, Directory.EnumerateFiles(imagesDirectory).Count());
				foreach (var fileName in Directory.EnumerateFiles(imagesDirectory))
				{
					AssertContains("<img src='" + DocumentConverter.GetSafeImagesDirectoryName(printJob.StoredAttachmentFilename) + "/" + Path.GetFileName(fileName) + "' ", htmlAsString);
				}
			}
			finally
			{
				printJob.DeleteStoredAttachment();
			}
		}

		public void TestConvertXLSToHTML_NoImages()
		{
			var testParentGuid = ZGuid.NewZGuid();

			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "HTML";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = DocumentTestXls;
			printJob.SP_EmailAttachments = "test.xls";
			printJob.SP_EmailSubjectLine = "email subject";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SQ = ZGuid.Empty;

			Factory.Save();

			try
			{
				printJob.SaveAttachmentToFilesystem(0);
				Assert(printJob.StoredAttachmentFilename.EndsWith("HTML", StringComparison.OrdinalIgnoreCase));
				var htmlAsString = File.ReadAllText(printJob.StoredAttachmentFilename);
				AssertContains("<meta name=\"Generator\" content=\"FlexCel", htmlAsString);
				var imagesDirectory = Path.Combine(Path.GetDirectoryName(printJob.StoredAttachmentFilename), DocumentConverter.GetSafeImagesDirectoryName(printJob.StoredAttachmentFilename));
				Assert(!Directory.Exists(imagesDirectory));
			}
			finally
			{
				printJob.DeleteStoredAttachment();
			}
		}

		public void TestConvertXLSToHTMF()
		{
			var testParentGuid = ZGuid.NewZGuid();

			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "HTMF";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = DocumentTestXls;
			printJob.SP_EmailAttachments = "test.xls";
			printJob.SP_EmailSubjectLine = "email subject";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SQ = ZGuid.Empty;

			Factory.Save();

			try
			{
				printJob.SaveAttachmentToFilesystem(0);
				Assert(printJob.StoredAttachmentFilename.EndsWith("HTML", StringComparison.OrdinalIgnoreCase));
				var htmlAsString = File.ReadAllText(printJob.StoredAttachmentFilename);
				AssertContains("<meta name=\"Generator\" content=\"FlexCel", htmlAsString);
				var imagesDirectory = Path.Combine(Path.GetDirectoryName(printJob.StoredAttachmentFilename), DocumentConverter.GetSafeImagesDirectoryName(printJob.StoredAttachmentFilename));
				Assert(Directory.Exists(imagesDirectory));
				Assert(Directory.EnumerateFiles(imagesDirectory).First().EndsWith(".pdf"));
			}
			finally
			{
				printJob.DeleteStoredAttachment();
			}

			try
			{
				printJob.SaveAttachmentToFilesystem(1);
				Assert(printJob.StoredAttachmentFilename.EndsWith("PDF", StringComparison.OrdinalIgnoreCase));
			}
			finally
			{
				printJob.DeleteStoredAttachment();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAttachmentToFilesystemWithDudTIFFileThrowsImageFormatException()
		{
			var tempFilesBefore = Directory.GetFiles(Temp.TempPath);

			var tIFFile = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFFileName;
			var testParentGuid = ZGuid.NewZGuid();
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "TIF";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(tIFFile);
			printJob.SP_EmailAttachments = "test.TIF";
			printJob.SP_EmailSubjectLine = "email subject";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SQ = ZGuid.Empty;
			Assert("Precondition: StoredAttachmentFilename is empty", printJob.StoredAttachmentFilename.IsEmpty);

			Factory.Save();

			AssertExceptionThrown(typeof(ImageFormatException), () => { printJob.SaveAttachmentToFilesystem(0); });
		}

		public void TestNew()
		{
			AssertNotNull(StmPrintJob.New(Factory));
		}

		public void TestSetingSP_ParentTableNameToPrefix()
		{
			ErrorReporter.Clear();
			Job.SP_ParentTableName = "JE";
			AssertEquals("Setting StmPrintJob.SP_ParentTableName to a TablePrefix", ErrorReporter.LastKeyReported);
			AssertEquals("StmPrintJob.SP_ParentTableName should not be set to TablePrefix (" + Job.SP_ParentTableName + "). It should be set to the TableName", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestDefaults()
		{
			AssertEquals("Should default to current branch", GlbBranch.CurrentBranch.PK, Job.SP_GB);
			AssertEquals("Should set email attachments to default.XLS", "default.XLS", Job.SP_EmailAttachments);
			AssertEquals("Should default to current user", GlbStaff.CurrentUser.GS_Code, Job.SP_GS_NKJobSubmittedBy);
		}

		public void TestDefaults_WithNoUserContext()
		{
			using (Env.SetTemporaryUserContext(null))
			{
				var printJob = Factory.New<StmPrintJob>();
				Assert(printJob.SP_GB.IsEmpty);
				Assert(printJob.SP_GS_NKJobSubmittedBy.IsEmpty);
				AssertEquals("Should set email attachments to default.XLS", "default.XLS", Job.SP_EmailAttachments);
			}
		}

		public void TestSupportsNotes()
		{
			AssertEquals("Job should support notes", true, Job.SupportsNotes);
		}

		public void TestDeleteStoredAttachment()
		{
			StmPrintJob printJob = Factory.NewWithValidTestData<StmPrintJob>();
			using (TempFile tempFile = TempFile.New())
			{
				printJob.StoredAttachmentFilename = tempFile.ToString();
				Assert("File should exist", File.Exists(printJob.StoredAttachmentFilename));
				printJob.DeleteStoredAttachment();
				Thread.Sleep(3000);
				Assert("DeleteStoredAttachment() should have deleted the file", !File.Exists(tempFile.Filename));
				Assert("StoredAttachmentFilename should be empty", printJob.StoredAttachmentFilename.IsEmpty);
				AssertEquals("SizeInKB should be 0", 0, printJob.StoredAttachmentSizeKB);
			}
		}

		public void TestDeleteStoredAttachmentDeletesLaterWhenLocked()
		{
			StmPrintJob printJob = Factory.NewWithValidTestData<StmPrintJob>();
			using (TempFile tempFile = TempFile.New())
			{
				printJob.StoredAttachmentFilename = tempFile.Filename;
				Assert("File should exist", File.Exists(printJob.StoredAttachmentFilename));

				string fileName = tempFile.Filename;
				Stream stream = File.OpenRead(fileName);
				printJob.DeleteStoredAttachment();
				Thread.Sleep(1500);
				Assert(" File should not have been deleted as it was locked.", File.Exists(tempFile.Filename));
				stream.Close();
				Thread.Sleep(8000);
				Assert("DeleteStoredAttachment() should have deleted the file as it's not locked now", !File.Exists(tempFile.Filename));
				Assert("StoredAttachmentFilename should be empty", printJob.StoredAttachmentFilename.IsEmpty);
				AssertEquals("SizeInKB should be 0", 0, printJob.StoredAttachmentSizeKB);
			}
		}

		public void TestJobTypeDisplayNameNotEmpty()
		{
			Job.SP_JobType = "EML";
			AssertEquals("Expected user-friendly name", "Email", Job.SP_JobTypeDisplayName);

			Job.SP_JobType = "PRN";
			AssertEquals("Expected user-friendly name", "Print", Job.SP_JobTypeDisplayName);

			Job.SP_JobType = "FAX";
			AssertEquals("Expected user-friendly name", "Fax", Job.SP_JobTypeDisplayName);

			Job.SP_JobType = "FAA";
			AssertEquals("Expected user-friendly name", "Fax awaiting acknowledgement", Job.SP_JobTypeDisplayName);
		}

		public void TestJobTypeDisplayNameUnknownCode()
		{
			Job.SP_JobType = "ZZZ";
			AssertEquals("Expected code to display", "ZZZ", Job.SP_JobTypeDisplayName);
		}

		public void TestStatusDisplayName()
		{
			Job.SP_Status = nameof(PrintJobStatus.QUE);
			AssertEquals("Queued", Job.SP_StatusDisplayName);

			Job.SP_Status = nameof(PrintJobStatus.FAL);
			AssertEquals("Failed", Job.SP_StatusDisplayName);

			Job.SP_Status = "ZZZ";
			AssertEquals("ZZZ", Job.SP_StatusDisplayName);
		}

		public void TestCorrectDestinationForJobTypeEML()
		{
			AssertEquals("Blank by default", "", Job.EmailToRecipients.Value);

			Job.SP_JobType = nameof(PrintType.EML);
			Job.SP_Destination = "unit.test@cargowise.com";
			AssertEquals("Email address", "unit.test@cargowise.com", Job.SP_Destination);

			Job.SP_SQ = ZGuid.NewZGuid();
			var destination = string.Empty;
			AssertNoExceptionThrown("Destination shouldn't throw a exception", () => { destination = Job.SP_Destination; });
			AssertEquals("Should return EmailToRecipients.Value", "unit.test@cargowise.com", destination);
		}

		public void TestCorrectDestinationPrintJob()
		{
			AssertEquals("Blank by default", "", Job.EmailToRecipients.Value);

			var printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_DisplayName = "Printer on the roof";
			Job.SP_SQ = printQueue.PK;
			AssertEquals("Printer name", "Printer on the roof", Job.SP_Destination);
		}

		public void TestCorrectDestinationForJobTypeFAX()
		{
			AssertEquals("Blank by default", "", Job.EmailToRecipients.Value);

			Job.SP_JobType = nameof(PrintType.FAX);
			Job.SP_Destination = "0498765432";
			AssertEquals("Fax Destination", "0498765432", Job.SP_FaxDestination);

			Job.SP_SQ = ZGuid.NewZGuid();
			var destination = string.Empty;
			AssertNoExceptionThrown("Destination shouldn't throw a exception", () => { destination = Job.SP_Destination; });
			AssertEquals("Should return Fax Destination", "0498765432", destination);
		}

		public void TestUsername()
		{
			Factory.Save(); // populate user nk value
			AssertEquals("Adding user's login name", GlbStaff.CurrentUser.GS_LoginName, Job.SP_UserLoginName);
			AssertEquals("Adding user's full name", GlbStaff.CurrentUser.GS_FullName, Job.SP_UserFullName);
		}

		public void TestStaff()
		{
			Factory.Save(); // Get an ADD event in the database
			AssertEquals("Staff", GlbStaff.CurrentUser.PK, Job.Staff.PK);
		}

		[TestDate(2005, 09, 08, 07, 06, 00)]
		public void TestCreateLogOnParent()
		{
			Job.SP_EmailSubjectLine = GlbCompany.CurrentCompany.GC_Name + " (" + GlbBranch.CurrentBranch.GB_BranchName + ") - Some Doc";
			Job.SP_ParentGuid = ZGuid.NewZGuid();
			Job.SP_ParentTableName = "SomeTable";
			Job.SP_GS_NKJobSubmittedBy = "HFE";
			Job.SP_FaxDestination = "+61 (2) 9025-1199";
			var r1 = Job.CarbonCopyRecipients.AddNew();
			r1.SPR_EmailAddress = "a@b.com";
			var r2 = Job.CarbonCopyRecipients.AddNew();
			r2.SPR_EmailAddress = "b@c.com";
			var r3 = Job.BlindCarbonCopyRecipients.AddNew();
			r3.SPR_EmailAddress = "c@d.com";
			var r4 = Job.BlindCarbonCopyRecipients.AddNew();
			r4.SPR_EmailAddress = "d@e.com";

			Job.CreateLogOnParent(Events.DocumentDelivered);
			AssertEquals(typeof(StmALog), Job.LastLogCreatedOnParent.GetType());
			AssertEquals("User", "HFE", Job.LastLogCreatedOnParent.SL_GS_NKUser);
			AssertEquals("Event", Events.DocumentDelivered.Code, Job.LastLogCreatedOnParent.SL_SE_NKEvent);
			AssertEquals("Reference", "Main: +61 (2) 9025-1199, CC: a@b.com, b@c.com, BCC: c@d.com, d@e.com - " + Job.SP_AbbreviatedEmailSubjectLine, Job.LastLogCreatedOnParent.SL_Reference);
			AssertEquals("Parent", Job.SP_ParentGuid, Job.LastLogCreatedOnParent.SL_Parent);
			AssertEquals("Table", "SomeTable", Job.LastLogCreatedOnParent.SL_Table);
			AssertEquals("Event Time", new ZDateTime(2005, 09, 08, 07, 06, 00), Job.LastLogCreatedOnParent.SL_EventTime);
		}

		[TestDate(2017, 01, 17, 07, 06, 00)]
		public void TestCreateLogOnParent_FromLinkedEvents()
		{
			Job.SP_EmailSubjectLine = GlbCompany.CurrentCompany.GC_Name + " (" + GlbBranch.CurrentBranch.GB_BranchName + ") - Some Doc";
			Job.SP_ParentGuid = ZGuid.NewZGuid();
			Job.SP_ParentTableName = "SomeTable";
			Job.SP_GS_NKJobSubmittedBy = "HFE";
			Job.SP_FaxDestination = "Narnia";

			var logParent = Factory.New<DummyEnterpriseBusinessObject>();

			var log1 = CreateDDVLog(logParent, "aaa|TYP=aaa");
			var log2 = CreateDDVLog(logParent, "bbb|TYP=bbb");

			Job.CreateLogTemplates(log1, log2);

			Factory.Save();

			Job.CreateLogOnParent(Events.DocumentSent);

			var logs = logParent
				.GetLogs()
				.GetAllLogs()
				.OfType<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.DocumentSentCode);

			AssertContainsExactElementsInAnyOrder(new[]
				{
					"Narnia - EDI (BNE) - Some Doc|TYP=aaa",
					"Narnia - EDI (BNE) - Some Doc|TYP=bbb"
				},
				logs.Select(log => log.SL_Reference.ToString()).ToArray());
		}

		public void TestCreateLogOnParent_FromLinkedEvents_MergeParameters()
		{
			Job.SP_EmailSubjectLine = GlbCompany.CurrentCompany.GC_Name + " (" + GlbBranch.CurrentBranch.GB_BranchName + ") - Some Doc";
			Job.SP_ParentGuid = ZGuid.NewZGuid();
			Job.SP_ParentTableName = "SomeTable";
			Job.SP_GS_NKJobSubmittedBy = "HFE";
			Job.SP_FaxDestination = "Narnia";

			var logParent = Factory.New<DummyEnterpriseBusinessObject>();

			var log1 = CreateDDVLog(logParent, "aaa", new[]
			{
				new KeyValuePair<string, string>("TYP", "aaa"),
				new KeyValuePair<string, string>("CMP", "aaa")
			});

			var log2 = CreateDDVLog(logParent, string.Empty, new[]
			{
				new KeyValuePair<string, string>("WHS", "bbb")
			});

			Job.CreateLogTemplates(log1, log2);

			Factory.Save();

			Job.CreateLogOnParent(Events.DocumentSent, new[]
			{
				new KeyValuePair<string, string>("TYP", "xxx"),
				new KeyValuePair<string, string>("NAM", "xxx")
			});

			var logs = logParent
				.GetLogs()
				.GetAllLogs()
				.OfType<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.DocumentSentCode);

			AssertContainsExactElementsInAnyOrder(new[]
				{
					"Narnia - EDI (BNE) - Some Doc|CMP=aaa|NAM=xxx|TYP=aaa",
					"Narnia - EDI (BNE) - Some Doc|NAM=xxx|TYP=xxx|WHS=bbb"
				},
				logs.Select(log => log.SL_Reference.ToString()).ToArray());
		}

		public void TestCreateLogOnParent_BadlyFormattedXml()
		{
			var logParent = Factory.New<DummyEnterpriseBusinessObject>();

			Job.SP_EmailSubjectLine = GlbCompany.CurrentCompany.GC_Name + " (" + GlbBranch.CurrentBranch.GB_BranchName + ") - Some Doc";
			Job.SP_ParentGuid = logParent.PK;
			Job.SP_ParentTableName = logParent.TableName;
			Job.SP_GS_NKJobSubmittedBy = "HFE";
			Job.SP_FaxDestination = "Narnia";

			Job.SP_DSNTemplate = "aaa";

			Factory.Save();

			Job.CreateLogOnParent(Events.DocumentSent, new[]
			{
				new KeyValuePair<string, string>("TYP", "xxx"),
				new KeyValuePair<string, string>("CMP", "xxx")
			});

			AssertEquals("ErrorReporter reported the xml", "Misformatted DSNTemplate", ErrorReporter.LastKeyReported);

			var logs = logParent
				.GetLogs()
				.GetAllLogs()
				.OfType<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.DocumentSentCode);

			AssertContainsExactElementsInAnyOrder(new[]
				{
					"Narnia - EDI (BNE) - Some Doc|CMP=xxx|TYP=xxx"
				},
				logs.Select(log => log.SL_Reference.ToString()).ToArray());

			ErrorReporter.Clear();
		}

		public void TestCreateLogOnParentDoesNotTruncateSubjectLine()
		{
			Job.SP_EmailSubjectLine = new string('x', StmPrintJobSchema.SP_EmailSubjectLine.MaxLength);
			Job.SP_ParentGuid = ZGuid.NewZGuid();
			Job.SP_ParentTableName = "SomeTable";
			Job.SP_GS_NKJobSubmittedBy = "HFE";
			Job.SP_FaxDestination = "+61 (2) 9025-1199";

			Job.CreateLogOnParent(Events.DocumentDelivered);
			AssertEquals("+61 (2) 9025-1199 - " + Job.SP_EmailSubjectLine, Job.LastLogCreatedOnParent.SL_Reference);
		}

		public void TestCreateJobWith256Characters()
		{
			AssertNoExceptionThrown(() => { Job.SP_EmailSubjectLine = new string('x', 256); });
		}

		public void TestAbbreviatedEmailSubjectLine()
		{
			GlbCompany company = Factory.New(typeof(GlbCompany)) as GlbCompany;
			GlbCompany anotherCompany = Factory.New(typeof(GlbCompany)) as GlbCompany;
			GlbBranch correctBranch = Factory.New(typeof(GlbBranch)) as GlbBranch;
			GlbBranch otherBranch = Factory.New(typeof(GlbBranch)) as GlbBranch;

			company.GC_Name = "My Company Pty. Ltd.";
			company.GC_Code = "MCP";

			anotherCompany.GC_Name = "Another Company Pty. Ltd.";
			anotherCompany.GC_Code = "ACO";

			correctBranch.GB_BranchName = "Head Office";
			correctBranch.GB_Code = "HOM";
			correctBranch.GB_GC = company.PK;

			otherBranch.GB_BranchName = "Head Office";
			otherBranch.GB_Code = "XXX";
			otherBranch.GB_GC = anotherCompany.PK;

			Factory.Save();

			Job.ClearCompanyAndBranchInformationForTesting();
			Job.SP_EmailSubjectLine = company.GC_Name + " (" + correctBranch.GB_BranchName + ") - Some Document";
			string expected = company.GC_Code + " (" + correctBranch.GB_Code + ") - Some Document";
			string companyInfos = "";

			foreach (CodeElement companyInfo in StmPrintJob.CompanyInformation)
			{
				companyInfos += companyInfo.Description + System.Environment.NewLine;
			}
			AssertEquals("Abbreviated email subject line; company info contains: \n" + companyInfos, expected, Job.SP_AbbreviatedEmailSubjectLine);
		}

		public void TestEmailFaxCoverNote()
		{
			AssertEquals("EmailFaxCoverNote", "", Job.EmailFaxCoverNote);

			var note = Factory.New<StmNote>();
			note.ST_NoteType = nameof(StmNoteVisibility.PRV);
			note.ST_ParentID = Job.PK;
			note.ST_Table = StmPrintJob.Schema.TableName;
			note.ST_NoteText = "Cover Note";

			AssertEquals("EmailFaxCoverNote", note.ST_NoteText, Job.EmailFaxCoverNote);
		}

		public void TestIsEmailJob()
		{
			Job.SP_JobType = nameof(PrintType.PRN);
			Assert("not an email job", !Job.IsEmailJob);
			Job.SP_JobType = "ABC";
			Assert("not a valid type - still not an email job", !Job.IsEmailJob);
			Job.SP_JobType = nameof(PrintType.EML);
			Assert("this is an email job", Job.IsEmailJob);
		}

		public void TestIsFaxJob()
		{
			Job.SP_JobType = nameof(PrintType.EML);
			Assert("not a fax job", !Job.IsFaxJob);
			Job.SP_JobType = "ABC";
			Assert("not a valid type - still not a fax job", !Job.IsFaxJob);
			Job.SP_JobType = nameof(PrintType.FAX);
			Assert("this is a fax job", Job.IsFaxJob);
		}

		public void TestIsPrintJob()
		{
			Job.SP_JobType = nameof(PrintType.EML);
			Assert("not a print job", !Job.IsPrintJob);
			Job.SP_JobType = "ABC";
			Assert("not a valid type - still not a print job", !Job.IsPrintJob);
			Job.SP_JobType = nameof(PrintType.PRN);
			Assert("this is a print job", Job.IsPrintJob);
			Job.SP_JobType = nameof(PrintJobType.PRS);
			Assert("this is a print job", Job.IsPrintJob);
		}

		public void TestIsDeliveredExternally()
		{
			Job.SP_JobType = nameof(PrintType.EML);
			Assert("an email job type, should be delivered externally", Job.IsDeliveredExternally);

			Job.SP_JobType = nameof(PrintType.FAX);
			Assert("a fax job type, should be delivered externally", Job.IsDeliveredExternally);

			Job.SP_JobType = nameof(PrintType.PRN);
			Assert("a print job type, should be delivered externally", Job.IsDeliveredExternally);

			Job.SP_JobType = nameof(PrintType.FTP);
			Assert("a ftp job type, should be delivered externally", Job.IsDeliveredExternally);

			Job.SP_JobType = nameof(PrintType.DDS);
			Assert("a DocManager type, should not be sent", !Job.IsDeliveredExternally);
		}

		public void TestAttachmentRequiresConversionForDOS()
		{
			Job.SP_JobType = nameof(PrintType.DDS);
			Job.SP_EmailAttachments = "test.XLS";
			Job.SP_SignBy = "DOS";
			Assert("Any DOS job requires a conversion if types match", Job.AttachmentRequiresConversion);
		}

		public void TestAttachmentRequiresConversionForPrint()
		{
			Job.SP_JobType = nameof(PrintType.PRN);
			Job.SP_EmailAttachments = "test.XLS";
			Assert("Print job type, doesn't matter what the format is, attachment does not require conversion", !Job.AttachmentRequiresConversion);
		}

		public void TestAttachmentRequiresConversionForEmail()
		{
			Job.SP_JobType = nameof(PrintType.EML);
			Job.SP_EmailAttachments = "test.PDF";
			Job.SP_EmailAttachmentFormat = "PDF";
			Assert("blob is already in format to be delivered via email, attachment does not require conversion", !Job.AttachmentRequiresConversion);

			Job.SP_EmailAttachments = "test.XLS";
			Assert("Blob is not in format to be delivered via email, so attachment requires conversion", Job.AttachmentRequiresConversion);
		}

		public void TestAttachmentRequiresConversionForFax()
		{
			Job.SP_JobType = nameof(PrintType.FAX);
			Job.SP_EmailAttachments = "test.TIF";
			Assert("Fax jobs always require conversion, even if the blob is already a TIF (to make sure the image is converted to 1bbp indexed)", Job.AttachmentRequiresConversion);

			Job.SP_EmailAttachments = "test.XLS";
			Assert("Fax jobs always require conversion", Job.AttachmentRequiresConversion);
		}

		public void TestAttachmentRequiresConversionForFtp()
		{
			Job.SP_JobType = nameof(PrintType.FTP);
			Job.SP_EmailAttachments = "test.PDF";
			Job.SP_EmailAttachmentFormat = "PDF";
			Assert("blob is already in format to be delivered to ftp, attachment does not require conversion", !Job.AttachmentRequiresConversion);

			Job.SP_EmailAttachments = "test.XLS";
			Assert("Blob is not in format to be delivered to ftp, so attachment requires conversion", Job.AttachmentRequiresConversion);
		}

		public void TestAttachmentRequiresConversionForFile()
		{
			Job.SP_EmailAttachments = "test.DOC";
			Job.SP_EmailAttachmentFormat = OrgConstants.AttachmentType.FIL;
			AssertEquals("AttachmentRequiresConversion", false, Job.AttachmentRequiresConversion);
		}

		public void TestAttachmentRequiresConversionForDragAndDrop()
		{
			Job.SP_JobType = nameof(PrintType.DDS);
			Assert("DDS jobs don't require conversion (attachments aren't used anywhere)", !Job.AttachmentRequiresConversion);
		}

		public void TestAttachmentRequiresConversionForPdfc()
		{
			Job.SP_JobType = nameof(PrintType.EML);
			Job.SP_EmailAttachments = "test.TXT";
			Job.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdfc;
			Assert(!Job.AttachmentRequiresConversion);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStoredAttachmentFilename()
		{
			var attachFile = UnitTestingConstants.TestFilesDir + "NewStyleTemplate.xls";
			Job.StoredAttachmentFilename = attachFile;
			AssertEquals("filename is correct", attachFile, Job.StoredAttachmentFilename);
			Assert("file size should be > 0", Job.StoredAttachmentSizeKB > 0);

			Job.StoredAttachmentFilename = string.Empty;
			AssertEquals("Filename should now be empty", true, Job.StoredAttachmentSizeKB.IsEmpty);
			AssertEquals("File size should be 0", 0, Job.StoredAttachmentSizeKB);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStoredAttachmentFileSize()
		{
			var attachFile = UnitTestingConstants.TestFilesDir + "NewStyleTemplate.xls";
			Job.StoredAttachmentFilename = attachFile;

			AssertEquals(@"Using the file NewStyleTemplate.xls to test file size is correct (expected 36Kb)", 36, Job.StoredAttachmentSizeKB);
		}

		public void TestIsInSameMergedJobForSimilarEmails()
		{
			var deliveryGroupID = ZGuid.NewZGuid();

			var firstPrintJob = CreateNewPrintJobWithEmails("test@example.com, test@test.com, i.like@dogs.com, potatos.gonna@potate.com", deliveryGroupID);
			var matchingFirstPrintJob = CreateNewPrintJobWithEmails("test@example.com, test@test.com, i.like@dogs.com, potatos.gonna@potate.com", deliveryGroupID);
			var similarToFirstButNotEqualPrintJob = CreateNewPrintJobWithEmails("example@test.com, test@test.com, i.like@dogs.com, potatos.gonna@potate.com", deliveryGroupID);
			var extremelySimilarToFirstButNotEqualPrintJob = CreateNewPrintJobWithEmails("test@example.co, test@test.com, i.like@dogs.com, potatos.gonna@potate.com", deliveryGroupID);
			var sameAsFirstButInDifferentOrderPrintJob = CreateNewPrintJobWithEmails("test@test.com, test@example.com, potatos.gonna@potate.com, i.like@dogs.com", deliveryGroupID);

			var singleEmailPrintJobFirst = CreateNewPrintJobWithEmails("foobar@gmail.com", deliveryGroupID);
			var singleEmailPrintJobSecond = CreateNewPrintJobWithEmails("bar@gmail.com", deliveryGroupID);

			Assert("Should not be in same merged job", !singleEmailPrintJobFirst.IsInSameMergedJob(singleEmailPrintJobSecond));
			Assert("Should not be in same merged job", !singleEmailPrintJobSecond.IsInSameMergedJob(singleEmailPrintJobFirst));
			Assert("Should be in same merged job", firstPrintJob.IsInSameMergedJob(matchingFirstPrintJob));
			Assert("Should not be in same merged job", !firstPrintJob.IsInSameMergedJob(similarToFirstButNotEqualPrintJob));
			Assert("Should not be in same merged job", !firstPrintJob.IsInSameMergedJob(extremelySimilarToFirstButNotEqualPrintJob));
			Assert("Should be in same merged job", firstPrintJob.IsInSameMergedJob(sameAsFirstButInDifferentOrderPrintJob));
		}

		public void TestIsInSameMergedJob()
		{
			ZGuid deliveryGroupID = ZGuid.NewZGuid();

			StmPrintJob printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_JobType = "EML";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_SB_DeliveryGroup = deliveryGroupID;

			StmPrintJob printJob2 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob2.SP_JobType = "PRN";
			printJob2.SP_Destination = "Test Printer"; // Was being set to SP_FaxDestination before.
			printJob2.SP_SB_DeliveryGroup = deliveryGroupID;

			StmPrintJob printJob3 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob3.SP_JobType = "EML";
			printJob3.SP_Destination = "test@example.com";
			printJob3.SP_SB_DeliveryGroup = deliveryGroupID;

			Assert("Print Jobs 1 and 3 belong in the same merged Job", printJob.IsInSameMergedJob(printJob3));
			Assert("Print Jobs 1 and 3 belong in the same merged Job", printJob3.IsInSameMergedJob(printJob));

			Assert("Print jobs 1 and 2 don't belong in the same merged job", !printJob.IsInSameMergedJob(printJob2));
			Assert("Print jobs 1 and 2 don't belong in the same merged job", !printJob2.IsInSameMergedJob(printJob));
		}

		public void TestIsInSameMergedJobForCCAndBCCEmails()
		{
			ZGuid deliveryGroupID = ZGuid.NewZGuid();

			StmPrintJob firstPrintJob = Factory.NewWithValidTestData<StmPrintJob>();
			firstPrintJob.SP_JobType = "EML";
			firstPrintJob.SP_Destination = "test@example.com";
			firstPrintJob.SP_SB_DeliveryGroup = deliveryGroupID;

			StmPrintJob printJobWithCCs = Factory.NewWithValidTestData<StmPrintJob>();
			printJobWithCCs.SP_JobType = "EML";
			printJobWithCCs.SP_Destination = "test@example.com";
			printJobWithCCs.CarbonCopyRecipients.AddNew().SPR_EmailAddress = "cc1@example.com";
			printJobWithCCs.SP_SB_DeliveryGroup = deliveryGroupID;

			StmPrintJob printJobWithBCCs = Factory.NewWithValidTestData<StmPrintJob>();
			printJobWithBCCs.SP_JobType = "EML";
			printJobWithBCCs.SP_Destination = "test@example.com";
			printJobWithBCCs.BlindCarbonCopyRecipients.AddNew().SPR_EmailAddress = "bcc1@example.com";
			printJobWithBCCs.SP_SB_DeliveryGroup = deliveryGroupID;

			StmPrintJob printJobWithCCsAndBCCs = Factory.NewWithValidTestData<StmPrintJob>();
			printJobWithCCsAndBCCs.SP_JobType = "EML";
			printJobWithCCsAndBCCs.SP_Destination = "test@example.com";
			printJobWithCCsAndBCCs.CarbonCopyRecipients.AddNew().SPR_EmailAddress = "cc1@example.com";
			printJobWithCCsAndBCCs.BlindCarbonCopyRecipients.AddNew().SPR_EmailAddress = "bcc1@example.com";
			printJobWithCCsAndBCCs.SP_SB_DeliveryGroup = deliveryGroupID;

			Assert("Print job 1 shouldn't be merged with print job with different CCs.", !firstPrintJob.IsInSameMergedJob(printJobWithCCs));
			Assert("Print job 1 shouldn't be merged with print job with different BCCs.", !firstPrintJob.IsInSameMergedJob(printJobWithBCCs));
			Assert("Print job 1 shouldn't be merged with print job with different CCs and BCCs.", !firstPrintJob.IsInSameMergedJob(printJobWithCCsAndBCCs));

			Assert("Print job with CCs shouldn't be merged with print job 1.", !printJobWithCCs.IsInSameMergedJob(firstPrintJob));
			Assert("Print job with CCs shouldn't be merged with print job with BCCs.", !printJobWithCCs.IsInSameMergedJob(printJobWithBCCs));
			Assert("Print job with CCs shouldn't be merged with print job that has the same CCs but also has BCCs.", !printJobWithCCs.IsInSameMergedJob(printJobWithCCsAndBCCs));

			Assert("Print job with BCCs shouldn't be merged with print job 1", !printJobWithBCCs.IsInSameMergedJob(firstPrintJob));
			Assert("Print job with BCCs shouldn't be merged with print job with CCs", !printJobWithBCCs.IsInSameMergedJob(printJobWithCCs));
			Assert("Print job with BCCs shouldn't be merged with print job that has the same BCCs but also has CCs.", !printJobWithBCCs.IsInSameMergedJob(printJobWithCCsAndBCCs));

			Assert("Print job with CCs and BCCs shouldn't be merged with print job 1", !printJobWithCCsAndBCCs.IsInSameMergedJob(firstPrintJob));
			Assert("Print job with CCs and BCCs shouldn't be merged with print job with only CCs", !printJobWithCCsAndBCCs.IsInSameMergedJob(printJobWithCCs));
			Assert("Print job with CCs and BCCs shouldn't be merged with print job with only BCCs", !printJobWithCCsAndBCCs.IsInSameMergedJob(printJobWithBCCs));
		}

		public void TestIsInSameMergedJob_DifferentEDocsProcessed()
		{
			var deliveryGroupID = ZGuid.NewZGuid();

			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_JobType = "EML";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_SB_DeliveryGroup = deliveryGroupID;
			printJob.SP_EDocsProcessed = true;
			printJob.SP_RelatedBusinessContext = ZString.Empty;

			var printJob2 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob2.SP_JobType = "EML";
			printJob2.SP_Destination = "test@example.com";
			printJob2.SP_SB_DeliveryGroup = deliveryGroupID;
			printJob2.SP_EDocsProcessed = false;
			printJob2.SP_RelatedBusinessContext = ZString.Empty;

			var printJob3 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob3.SP_JobType = "EML";
			printJob3.SP_Destination = "test@example.com";
			printJob3.SP_SB_DeliveryGroup = deliveryGroupID;
			printJob3.SP_EDocsProcessed = true;
			printJob3.SP_RelatedBusinessContext = ZString.Empty;

			var printJob4 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob4.SP_JobType = "EML";
			printJob4.SP_Destination = "test@example.com";
			printJob4.SP_SB_DeliveryGroup = deliveryGroupID;
			printJob4.SP_EDocsProcessed = true;
			printJob4.SP_RelatedBusinessContext = Core.Constants.DocManagerCodes.Shipment;

			CombineAssertions(() =>
			{
				Assert("Print Jobs 1 and 3 belong in the same merged Job", printJob.IsInSameMergedJob(printJob3));
				Assert("Print Jobs 1 and 3 belong in the same merged Job", printJob3.IsInSameMergedJob(printJob));

				Assert("Print jobs 1 and 2 belong in the same merged job", printJob.IsInSameMergedJob(printJob2));
				Assert("Print jobs 1 and 2 belong in the same merged job", printJob2.IsInSameMergedJob(printJob));

				Assert("Print jobs 1 and 4 belong in the same merged job", printJob.IsInSameMergedJob(printJob4));
				Assert("Print jobs 1 and 4 belong in the same merged job", printJob4.IsInSameMergedJob(printJob));
			});
		}

		public void TestIsInSameMergedJobForPRNShouldNotMerge()
		{
			TestCaseHelper.ClearTable(StmPrintQueueSchema.Constants.TableName);
			ZGuid deliveryGroupID = ZGuid.NewZGuid();

			StmPrintQueue queue = Factory.New<StmPrintQueue>();
			queue.SQ_ServerName = System.Environment.MachineName;
			queue.SQ_QueueName = "EDI Image Printer";
			queue.SQ_DisplayName = "EDI Image Printer";
			queue.SQ_AllowPrinting = true;

			StmPrintJob printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_JobType = "PRN";
			printJob.SP_SB_DeliveryGroup = deliveryGroupID;
			printJob.SP_SQ = queue.PK;

			StmPrintJob printJob2 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob2.SP_JobType = "PRN";
			printJob2.SP_SB_DeliveryGroup = deliveryGroupID;
			printJob2.SP_SQ = queue.PK;

			StmPrintJob printJob3 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob3.SP_JobType = "PRN";
			printJob3.SP_SB_DeliveryGroup = deliveryGroupID;
			printJob3.SP_SQ = queue.PK;

			Assert("Print jobs should not be merged because they're PRN jobs", !printJob.IsInSameMergedJob(printJob2));
			Assert("Print jobs should not be merged because they're PRN jobs", !printJob2.IsInSameMergedJob(printJob3));
			Assert("Print jobs should not be merged because they're PRN jobs", !printJob3.IsInSameMergedJob(printJob));
		}

		public void TestIsInSameMergedJobForEMLShouldMerge()
		{
			ZGuid deliveryGroupID = ZGuid.NewZGuid();

			StmPrintJob printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_JobType = "EML";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_SB_DeliveryGroup = deliveryGroupID;

			StmPrintJob printJob2 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob2.SP_JobType = "EML";
			printJob2.SP_Destination = "test@example.com";
			printJob2.SP_SB_DeliveryGroup = deliveryGroupID;

			StmPrintJob printJob3 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob3.SP_JobType = "EML";
			printJob3.SP_Destination = "test@example.com";
			printJob3.SP_SB_DeliveryGroup = deliveryGroupID;

			Assert("Print jobs should be merged because they're EML jobs", printJob.IsInSameMergedJob(printJob2));
			Assert("Print jobs should be merged because they're EML jobs", printJob2.IsInSameMergedJob(printJob3));
			Assert("Print jobs should be merged because they're EML jobs", printJob3.IsInSameMergedJob(printJob));
		}

		public void TestIsInSameMergedJobForFAXShouldNOTMerge()
		{
			ZGuid deliveryGroupID = ZGuid.NewZGuid();

			StmPrintJob printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_JobType = "FAX";
			printJob.SP_Destination = "90251199";
			printJob.SP_SB_DeliveryGroup = deliveryGroupID;

			StmPrintJob printJob2 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob2.SP_JobType = "FAX";
			printJob2.SP_Destination = "90251199";
			printJob2.SP_SB_DeliveryGroup = deliveryGroupID;

			StmPrintJob printJob3 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob3.SP_JobType = "FAX";
			printJob3.SP_Destination = "90251199";
			printJob3.SP_SB_DeliveryGroup = deliveryGroupID;

			Assert("Print jobs should not be merged because they're FAX jobs", !printJob.IsInSameMergedJob(printJob2));
			Assert("Print jobs should not be merged because they're FAX jobs", !printJob2.IsInSameMergedJob(printJob3));
			Assert("Print jobs should not be merged because they're FAX jobs", !printJob3.IsInSameMergedJob(printJob));
		}

		public void TestGetSerialisablePrintJob()
		{
			var stamp = ZGuid.NewZGuid();

			var queue = Factory.New<StmPrintQueue>();
			queue.SQ_ServerName = System.Environment.MachineName;
			queue.SQ_QueueName = "TestPrinter";
			queue.SQ_DisplayName = "TestPrinter";
			queue.SQ_AllowPrinting = true;
			queue.SQ_PrintQueueStateChanged = stamp;

			using (var directory = new TempDirectory())
			{
				var tempFile = Temp.GetTempFileName(directory.DirectoryName, "XLS");
				var printJob = Factory.NewWithValidTestData<StmPrintJob>();
				printJob.SP_EmailAttachments = "test.TIF";
				printJob.SP_Copies = 3;
				printJob.SP_EscapeSequence = new byte[] { 6, 7, 8, 9, 0 };
				printJob.SP_SQ = queue.PK;
				printJob.SP_WatermarkText = "watermark";
				printJob.SP_FlexCelLineSpacing = 3.9m;
				printJob.StoredAttachmentFilename = tempFile;

				var serialisableJob = printJob.GetSerialisablePrintJob(new byte[] { 1, 2, 3, 4, 5 });
				AssertEquals("BlobType", "TIF", serialisableJob.BlobType);
				AssertEquals("Contents", new byte[] { 1, 2, 3, 4, 5 }, serialisableJob.Contents);
				AssertEquals("Copies", 3, serialisableJob.Copies);
				AssertEquals("EmailSubjectLine", tempFile, serialisableJob.EmailSubjectLine);
				AssertEquals("EscapeSequence", new byte[] { 6, 7, 8, 9, 0 }, serialisableJob.EscapeSequence);
				AssertEquals("PK", printJob.PK, serialisableJob.JobPk);
				AssertEquals("QueueName", queue.SQ_QueueName, serialisableJob.QueueName);
				AssertEquals("Queue stamp", stamp.ToGuid(), serialisableJob.QueueStateChangedStamp);
				AssertEquals("HasWatermark", true, serialisableJob.HasWatermark);

				var printEngineJob = printJob.GetPrintEngineJob(new byte[] { 1, 2, 3, 4, 5 });
				AssertEquals(nameof(printEngineJob.LineSpacing), 3.9m, printEngineJob.LineSpacing);
			}
		}

		public void TestGetSerialisablePrintJob_EmailSubjectLineCantBeEmpty()
		{
			var stamp = ZGuid.NewZGuid();

			var queue = Factory.New<StmPrintQueue>();
			queue.SQ_ServerName = System.Environment.MachineName;
			queue.SQ_QueueName = "TestPrinter";
			queue.SQ_DisplayName = "TestPrinter";
			queue.SQ_AllowPrinting = true;
			queue.SQ_PrintQueueStateChanged = stamp;

			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_EmailAttachments = "test.TIF";
			printJob.SP_Copies = 3;
			printJob.SP_EscapeSequence = new byte[] { 6, 7, 8, 9, 0 };
			printJob.SP_SQ = queue.PK;
			printJob.SP_WatermarkText = "watermark";
			printJob.SP_FlexCelLineSpacing = 3.9m;

			printJob.SP_EmailSubjectLine = ZString.Empty;
			AssertEquals("(No Subject)", printJob.GetSerialisablePrintJob(new byte[] { 1, 2, 3, 4, 5 }).EmailSubjectLine);
		}

		public void TestWatermark()
		{
			var previousValue = DocumentsDataRegistry.Instance.Watermark.Value;
			try
			{
				var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
				registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
				{
					using (var image = Image.FromFile(resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.TransparentPNG.png")))
					{
						var newRegistry = new DocumentEngineCore.Registry.Watermark();
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
					}

					ZGuid stamp = ZGuid.NewZGuid();
					StmPrintQueue queue = Factory.New<StmPrintQueue>();
					queue.SQ_ServerName = System.Environment.MachineName;
					queue.SQ_QueueName = "TestPrinter";
					queue.SQ_DisplayName = "TestPrinter";
					queue.SQ_AllowPrinting = true;
					queue.SQ_PrintQueueStateChanged = stamp;

					StmPrintJob printJob = Factory.NewWithValidTestData<StmPrintJob>();
					printJob.SP_EmailAttachments = "test.TIF";
					printJob.SP_CustomProperties = new byte[] { 1, 2, 3, 4, 5 };
					printJob.SP_Copies = 3;
					printJob.SP_EmailSubjectLine = "hello";
					printJob.SP_EscapeSequence = new byte[] { 6, 7, 8, 9, 0 };
					printJob.SP_SQ = queue.PK;
					printJob.SP_WatermarkText = "watermark";
					printJob.SP_FlexCelLineSpacing = 3.9m;

					TextWatermark textWatermark = printJob.Watermark as TextWatermark;
					AssertNotNull("Watermark should be text watermark", textWatermark);
					AssertEquals("Text should be the value from SP_WatermarkText", "watermark", textWatermark.AsText);
					AssertEquals("Should get value from registry", WatermarkHorizontalAlign.Left, textWatermark.HorizontalAlign);
					AssertEquals("Should get value from registry", WatermarkVerticalAlign.Top, textWatermark.VerticalAlign);
					AssertEquals("Should get value from registry", 10f, textWatermark.HorizontalOffset);
					AssertEquals("Should get value from registry", 20f, textWatermark.VerticalOffset);
					AssertEquals("Should get value from registry", 65, textWatermark.Rotation);
					AssertEquals("Should get value from registry", 90, textWatermark.FontSize);
					AssertEquals("Should get value from registry", Color.FromArgb(30, 0, 0, 0), textWatermark.TextColor);
				}

				registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
				{
					StmPrintJob printJob = Factory.NewWithValidTestData<StmPrintJob>();
					TextWatermark nonCommercialUseWatermark = printJob.Watermark as TextWatermark;
					AssertNotNull("Watermark should be text watermark", nonCommercialUseWatermark);
					AssertEquals("Text should be default value for non commercial use watermark", WatermarkHelper.NonCommercialUseWatermarkText, nonCommercialUseWatermark.AsText);
					AssertEquals(printJob.Watermark.AsImage, null);
					AssertEquals("Should get default value for non commercial use watermark", WatermarkHorizontalAlign.Centre, nonCommercialUseWatermark.HorizontalAlign);
					AssertEquals("Should get default value for non commercial use watermark", WatermarkVerticalAlign.Middle, nonCommercialUseWatermark.VerticalAlign);
					AssertEquals("Should get default value for non commercial use watermark", 0f, nonCommercialUseWatermark.HorizontalOffset);
					AssertEquals("Should get default value for non commercial use watermark", 0f, nonCommercialUseWatermark.VerticalOffset);
					AssertEquals("Should get default value for non commercial use watermark", 45, nonCommercialUseWatermark.Rotation);
					AssertEquals("Should get default value for non commercial use watermark", 52, nonCommercialUseWatermark.FontSize);
					AssertEquals("Should get default value for non commercial use watermark", Color.FromArgb(60, 0, 0, 0), nonCommercialUseWatermark.TextColor);
				}

				DocumentsDataRegistry.Instance.TestWatermarkOpacity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 21);
				{
					StmPrintJob printJob = Factory.New<StmPrintJob>();
					TextWatermark nonCommercialUseWatermark = printJob.Watermark as TextWatermark;
					AssertNotNull("Watermark should be text watermark", nonCommercialUseWatermark);
					AssertEquals("Text should be default value for non commercial use watermark", WatermarkHelper.NonCommercialUseWatermarkText, nonCommercialUseWatermark.AsText);
					AssertEquals(printJob.Watermark.AsImage, null);
					AssertEquals("Should get default value for non commercial use watermark", WatermarkHorizontalAlign.Centre, nonCommercialUseWatermark.HorizontalAlign);
					AssertEquals("Should get default value for non commercial use watermark", WatermarkVerticalAlign.Middle, nonCommercialUseWatermark.VerticalAlign);
					AssertEquals("Should get default value for non commercial use watermark", 0f, nonCommercialUseWatermark.HorizontalOffset);
					AssertEquals("Should get default value for non commercial use watermark", 0f, nonCommercialUseWatermark.VerticalOffset);
					AssertEquals("Should get default value for non commercial use watermark", 45, nonCommercialUseWatermark.Rotation);
					AssertEquals("Should get default value for non commercial use watermark", 52, nonCommercialUseWatermark.FontSize);
					AssertEquals("Should get registry value for non commercial use watermark", Color.FromArgb(21, 0, 0, 0), nonCommercialUseWatermark.TextColor);
				}
			}
			finally
			{
				DocumentsDataRegistry.Instance.Watermark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, previousValue);
			}
		}

		[UseSnapshotProtection]
		public void TestDoesntSaveWhenLocksAreLost()
		{
			//Make some jobs, save them, grab locks, modify jobs, kill connection, save jobs, assert the changes aren't saved
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				using (var mutexs = new DisposableList(10))
				{
					var otherFactory = new BusinessObjectFactory(connection);

					var jobs = Enumerable.Range(0, 10).Select(i => otherFactory.NewWithValidTestData<StmPrintJob>()).ToArray();
					SetSP_Copies(69, jobs);
					otherFactory.Save();

					var lockedJobs = LockJobs(jobs, connection, mutexs);

					Assert("Precondition: Some jobs were locked", lockedJobs.Any());

					connection.CloseConnection();

					SetSP_Copies(13, lockedJobs);

					AssertExceptionThrown<SqlLockLostException>("We should not be able to save with undisposed locks", otherFactory.Save);

					var newFactory = new BusinessObjectFactory(Db.Connection);
					var reloadedJobs = lockedJobs.Select(job => newFactory.Load<StmPrintJob>(job.PK));
					Assert("Since the mutexes all died before saving the jobs, their changes should not be reflected in the db", reloadedJobs.All(job => job.SP_Copies == 69));
				}
			}
		}

		public void TestIsAutoLogged()
		{
			var job = Factory.New<PrintJobTest>();
			AssertEquals("PrintJob should not be auto logged", false, job.IsAutoLoggedForTest);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAttachmentToFilesystemXLStoXLS()
		{
			var filePath = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestXLSFileName;
			AssertSaveAttachmentToFilesystem(filePath, AttachmentTypeList.Codes.Xls, "test.XLS", ".XLS");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAttachmentToFilesystemXLSXtoXLSX()
		{
			var filePath = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestXLSXFileName;
			AssertSaveAttachmentToFilesystem(filePath, AttachmentTypeList.Codes.Xlsx, "test.XLSX", ".XLSX");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAttachmentToFilesystemTIFtoTIF()
		{
			var filePath = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.Test8bppTifFileName;
			AssertSaveAttachmentToFilesystem(filePath, AttachmentTypeList.Codes.Tif, "test.TIF", ".TIF");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAttachmentToFilesystemXLStoPDF()
		{
			var filePath = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestXLSFileName;
			AssertSaveAttachmentToFilesystem(filePath, AttachmentTypeList.Codes.Pdf, "test.XLS", ".PDF");
			AssertSaveAttachmentToFilesystem(filePath, AttachmentTypeList.Codes.Pdfa, "test.XLS", ".PDF");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAttachmentToFilesystemXLSXtoPDF()
		{
			var filePath = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestXLSXFileName;
			AssertSaveAttachmentToFilesystem(filePath, AttachmentTypeList.Codes.Pdf, "test.XLSX", ".PDF");
			AssertSaveAttachmentToFilesystem(filePath, AttachmentTypeList.Codes.Pdfa, "test.XLSX", ".PDF");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAttachmentToFilesystemXLStoTIF()
		{
			var filePath = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestXLSXFileName;
			AssertSaveAttachmentToFilesystem(filePath, AttachmentTypeList.Codes.Tif, "test.XLSX", ".TIF");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAttachmentToFilesystemTIFtoPDF()
		{
			var filePath = PrintProcessingConstants.TestMixedbppTifFullPath;
			AssertSaveAttachmentToFilesystem(filePath, AttachmentTypeList.Codes.Pdfc, "test.TIF", ".PDF");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestSaveAttachmentToFilesystemWithoutExceptionForPDFCAndOtherTypesOfFiles()
		{
			var filePath = PrintProcessingConstants.TestTxtFileFullPath;
			AssertSaveAttachmentToFilesystem(filePath, AttachmentTypeList.Codes.Pdfc, "Test.txt", ".TXT");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestRequiresAdministrativePrivileges("read the private key of X509 Certificate")]
		public void TestSaveAttachmentToFilesystemXLStoSignedPDF()
		{
			AssertSaveAttachmentToFilesystemXLStoSignedPDF(DigitalSignatureTestHelper.GetDigitalSignatureRegistry());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestRequiresAdministrativePrivileges("read the private key of X509 Certificate")]
		public void TestSaveAttachmentToFilesystemXLStoSignedPDF_WithPaswordProtectedCertificate()
		{
			AssertSaveAttachmentToFilesystemXLStoSignedPDF(DigitalSignatureTestHelper.GetDigitalSignatureRegistry_WithPassword());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAttachmentToFilesystemXLStoSignedPDF_Placeholder()
		{
			DocumentsDataRegistry.Instance.DigitalSignature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DigitalSignatureTestHelper.GetDigitalSignatureRegistry());

			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdf;
			printJob.SP_Destination = "test@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(Path.Combine(UnitTestingConstants.TestDocumentDigitalSignatureFilePath, "TestSignXlsDocument.xls"));
			printJob.SP_EmailAttachments = "test.xls";
			printJob.SP_EmailSubjectLine = "email subject";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = ZGuid.NewZGuid();
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SQ = ZGuid.Empty;
			printJob.SP_SignBy = "DOS";

			Factory.Save();

			try
			{
				printJob.SaveAttachmentToFilesystem(0);
				AssertEquals("File extention type of SavedFilename", ".PDF", Path.GetExtension(printJob.StoredAttachmentFilename).ToUpper());

				var signedConvertedPDF = File.ReadAllBytes(printJob.StoredAttachmentFilename);
				var expectedFileName = "testFilesystemXLStoSignedPDF.pdf";
				var expectedPdfData = File.ReadAllBytes(Path.Combine(UnitTestingConstants.TestDocumentDigitalSignatureFilePath, expectedFileName));

				// Uncomment the following line if this test is failing and you want to see what the actual output is.
				//File.WriteAllBytes(@"c:\tmp\" + expectedFileName, signedConvertedPDF);

				var expectedSignature = new List<(string key, string value)>()
				{
					(("Location", "")),
					(("Reason", $@"{CompanyBrandingName} \(WTG\) has provided the feature to generate and sign this PDF document using the data verified by the application users and available in {ProductName} at the time of signing. WTG or any of its employees shall not be held responsible for any discrepancies observed in the data contained in this document.")),
					(("ContactInfo", ""))
				};

				var actualSignature = ImageToPDFConverterTest.GetSignatureFromPDF(signedConvertedPDF);

				AssertContainsExactElementsInAnyOrder(expectedSignature, actualSignature);
			}
			finally
			{
				printJob.DeleteStoredAttachment();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAttachmentToFilesystemTwoPrintJobsSameName()
		{
			var deliveryGroup = CreateNewDeliveryGroup(null);

			var tempFilesBefore = Directory.GetFiles(Temp.TempPath);
			var xLSFile = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestXLSFileName;
			var testParentGuid = ZGuid.NewZGuid();
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(xLSFile);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_EmailSubjectLine = "email subject";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SQ = ZGuid.Empty;
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;

			var printJob2 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob2.SP_JobType = "EML";
			printJob2.SP_EmailAttachmentFormat = "XLS";
			printJob2.SP_Destination = "test@example.com";
			printJob2.SP_DocumentName = "Test";
			printJob2.SP_CustomProperties = File.ReadAllBytes(xLSFile);
			printJob2.SP_EmailAttachments = "test.XLS";
			printJob2.SP_EmailSubjectLine = "email subject";
			printJob2.SP_ParentTableName = "JobShipment";
			printJob2.SP_ParentGuid = testParentGuid;
			printJob2.SP_RunDateTime = ZDateTime.UtcNow;
			printJob2.SP_SQ = ZGuid.Empty;
			printJob2.SP_SB_DeliveryGroup = deliveryGroup.PK;

			Factory.Save();

			try
			{
				printJob.SaveAttachmentToFilesystem(0);
				printJob2.SaveAttachmentToFilesystem(0);
				Assert("Even though print jobs had exactly the same data, the two StoredAttachmentFilenames should be different - so they don't clash when attached to the email", printJob.StoredAttachmentFilename != printJob2.StoredAttachmentFilename);
				Assert("StoredAttachmentFilename exists", File.Exists(printJob.StoredAttachmentFilename));
				Assert("StoredAttachmentFilename exists", File.Exists(printJob2.StoredAttachmentFilename));
				Assert("StoredAttachmentSize should be filled in now", !printJob.StoredAttachmentSizeKB.IsEmpty);
				Assert("StoredAttachmentSize should be filled in now", !printJob2.StoredAttachmentSizeKB.IsEmpty);
				AssertEquals("File extention type of SavedFilename", ".XLS", Path.GetExtension(printJob.StoredAttachmentFilename).ToUpper());
				AssertEquals("File extention type of SavedFilename", ".XLS", Path.GetExtension(printJob2.StoredAttachmentFilename).ToUpper());

				var tempFilesAfter = Directory.GetFiles(Temp.TempPath);
				AssertEquals("Two more files in the temp path", tempFilesBefore.Length + 2, tempFilesAfter.Length);
			}
			finally
			{
				File.Delete(printJob.StoredAttachmentFilename);
				File.Delete(printJob2.StoredAttachmentFilename);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConvertAttachmentsForDeliveryTwoPrintJobsSameNameLoggingToDocManager()
		{
			var deliveryGroup = CreateNewDeliveryGroup("This is a test email subject line");

			var xLSFile = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestXLSFileName;
			var testParentGuid = ZGuid.NewZGuid();
			var printJob1 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob1.SP_JobType = "EML";
			printJob1.SP_EmailAttachmentFormat = "XLS";
			printJob1.SP_Destination = "test@example.com";
			printJob1.SP_DocumentName = "Test";
			printJob1.SP_CustomProperties = File.ReadAllBytes(xLSFile);
			printJob1.SP_EmailAttachments = "test.XLS";
			printJob1.SP_EmailSubjectLine = "email subject";
			printJob1.SP_ParentTableName = "JobShipment";
			printJob1.SP_RelatedBusinessContext = "SHP";
			printJob1.SP_ParentGuid = testParentGuid;
			printJob1.SP_RunDateTime = ZDateTime.UtcNow;
			printJob1.SP_SQ = ZGuid.Empty;
			printJob1.SP_SB_DeliveryGroup = deliveryGroup.PK;

			var printJob2 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob2.SP_JobType = "EML";
			printJob2.SP_EmailAttachmentFormat = "XLS";
			printJob2.SP_Destination = "test@example.com";
			printJob2.SP_DocumentName = "Test";
			printJob2.SP_CustomProperties = File.ReadAllBytes(xLSFile);
			printJob2.SP_EmailAttachments = "test.XLS";
			printJob2.SP_EmailSubjectLine = "email subject";
			printJob2.SP_ParentTableName = "JobShipment";
			printJob2.SP_RelatedBusinessContext = "SHP";
			printJob2.SP_ParentGuid = testParentGuid;
			printJob2.SP_RunDateTime = ZDateTime.UtcNow;
			printJob2.SP_SQ = ZGuid.Empty;
			printJob2.SP_SB_DeliveryGroup = deliveryGroup.PK;
			Factory.Save();

			var collection = new StmPrintJobMergedCollection(Factory);
			collection.Add(printJob1);
			collection.Add(printJob2);

			foreach (StmPrintJob printJob in collection)
			{
				printJob.SaveAttachmentToFilesystem(0);
			}

			Assert("Even though print jobs had exactly the same data, the two StoredAttachmentFilenames should be different - so they don't clash when attached to the email",
				printJob1.StoredAttachmentFilename != printJob2.StoredAttachmentFilename);

			foreach (StmPrintJob job in collection)
			{
				File.Delete(job.StoredAttachmentFilename);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAttachmentToFilesystemWithAWholeBunchOfInvalidChars()
		{
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = "example@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestGeneratedReport);
			printJob.SP_EmailAttachments = "blah ? <> * \\\" |.XLS";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			Factory.Save();

			try
			{
				printJob.SaveAttachmentToFilesystem(0);
				Assert("Saved file shouldn't have any invalid characters that were in the subject line", printJob.StoredAttachmentFilename.ContainsAnyChar("?<>*\\\"|"));
				AssertEquals("The new stored attachment filename should have replaced invalid chars with an underscore", "blah _ __ _ __ _", Path.GetFileNameWithoutExtension(printJob.StoredAttachmentFilename));
			}
			finally
			{
				if (File.Exists(printJob.StoredAttachmentFilename))
				{
					File.Delete(printJob.StoredAttachmentFilename);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAttachmentToFileSystemThrowExcelInterfaceException()
		{
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			try
			{
				printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
				printJob.SP_JobType = nameof(PrintType.EML);
				printJob.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdfc;
				printJob.SP_Destination = "test@example.com";
				printJob.SP_DocumentName = "Test.xls";
				printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.CorruptFileFullPath);
				printJob.SP_EmailAttachments = "Test.xls";
				printJob.SP_EmailSubjectLine = "email subject";
				printJob.SP_ParentTableName = "JobShipment";
				printJob.SP_ParentGuid = ZGuid.NewZGuid();
				printJob.SP_RunDateTime = ZDateTime.UtcNow;
				printJob.SP_SQ = ZGuid.Empty;
				printJob.SaveAttachmentToFilesystem(0);

				AssertEquals(1, printJob.ErrorsWhenConverting.Count);

				var error = printJob.ErrorsWhenConverting.First();
				AssertEquals("An error occurred when converting file Test.xls to format PDFC. The original format was retained.", error.message);
				Assert(error.exception is ExcelInterfaceException);
				AssertEquals(ExcelInterfaceExceptionBase.FileCorruptedMessage, error.exception.Message);

				AssertEquals("Test.xls", Path.GetFileName(printJob.StoredAttachmentFilename));
			}
			finally
			{
				File.Delete(printJob.StoredAttachmentFilename);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAttachmentToFileSystemAttachmentDontRequireConversionWhenFileFormatIsInteger()
		{
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			try
			{
				printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
				printJob.SP_JobType = nameof(PrintType.EML);
				printJob.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdfc;
				printJob.SP_Destination = "test@example.com";
				printJob.SP_DocumentName = "Test.7803";
				printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.CorruptFileFullPath);
				printJob.SP_EmailAttachments = "Test.7803";
				printJob.SP_EmailSubjectLine = "email subject";
				printJob.SP_ParentTableName = "JobShipment";
				printJob.SP_ParentGuid = ZGuid.NewZGuid();
				printJob.SP_RunDateTime = ZDateTime.UtcNow;
				printJob.SP_SQ = ZGuid.Empty;
				printJob.SaveAttachmentToFilesystem(0);

				AssertEquals(0, printJob.ErrorsWhenConverting.Count);
				AssertEquals(false, printJob.AttachmentRequiresConversion);
				AssertEquals("Test.7803", Path.GetFileName(printJob.StoredAttachmentFilename));
			}
			finally
			{
				File.Delete(printJob.StoredAttachmentFilename);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAttachmentToFileSystemThrowExcelInterfaceException_FileFormatNotSupported()
		{
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			try
			{
				printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
				printJob.SP_JobType = nameof(PrintType.EML);
				printJob.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdfc;
				printJob.SP_Destination = "test@example.com";
				printJob.SP_DocumentName = "Test.xls";
				printJob.SP_CustomProperties = File.ReadAllBytes(UnitTestingConstants.TestFilesDir + "BadExcelFormatTest.xls");
				printJob.SP_EmailAttachments = "Test.xls";
				printJob.SP_EmailSubjectLine = "email subject";
				printJob.SP_ParentTableName = "JobShipment";
				printJob.SP_ParentGuid = ZGuid.NewZGuid();
				printJob.SP_RunDateTime = ZDateTime.UtcNow;
				printJob.SP_SQ = ZGuid.Empty;
				printJob.SaveAttachmentToFilesystem(0);
			}
			catch (ExcelInterfaceException e) when (e.Type == ExcelInterfaceExceptionType.FileFormatNotSupported)
			{
				Assert(!ErrorReporter.HasBeenReported("Corrupted stream could not be loaded."));
				Assert(!ErrorReporter.HasBeenReported("Corrupted stream could not be loaded. Print Job informations :"));
				ErrorReporter.Clear();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAttachmentToFilesystemReportCustomPropertiesIsEmptyError()
		{
			var deliveryGroup = CreateNewDeliveryGroup("This is a test email subject line");

			var xLSFile = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestXLSFileName;
			var testParentGuid = ZGuid.NewZGuid();
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "PDF";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(xLSFile);
			printJob.SP_EmailAttachments = "test.xls";
			printJob.SP_EmailSubjectLine = "email subject";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_RelatedBusinessContext = "SHP";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SQ = ZGuid.Empty;
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;
			Factory.Save();

			var oldValueLength = printJob.SP_CustomProperties.Length;
			printJob.SP_CustomProperties = new ZBlob(Array.Empty<Byte>());
			try
			{
				printJob.SaveAttachmentToFilesystem(0);
			}
			finally
			{
				AssertEquals("Last Key Reported", "TryToOpenEmptyXlsStream", ErrorReporter.LastKeyReported);
				AssertContains("Last Error Message Reported", "DocumentName: Test", ErrorReporter.LastMessageReported);
				AssertContains("Last Error Message Reported", "AttachmentType: PDF", ErrorReporter.LastMessageReported);
				AssertContains("Last Error Message Reported", "EmailSubject: email subject", ErrorReporter.LastMessageReported);
				AssertContains("Last Error Message Reported", "ParentTableName: JobShipment", ErrorReporter.LastMessageReported);
				AssertContains("Last Error Message Reported", "LatestSetToEmptyStackTrace: ", ErrorReporter.LastMessageReported);
				AssertContains("Last Error Message Reported", $"OldValueLength: {oldValueLength}", ErrorReporter.LastMessageReported);

				File.Delete(printJob.StoredAttachmentFilename);
				ErrorReporter.Clear();
			}
		}

		public void TestStmALogShouldRemovedIfExceptionThrown()
		{
			var printJob = Factory.New<PrintJobForTestStmALog>();

			printJob.SP_EmailSubjectLine = GlbCompany.CurrentCompany.GC_Name + " (" + GlbBranch.CurrentBranch.GB_BranchName + ") - Some Doc";
			printJob.SP_ParentGuid = ZGuid.NewZGuid();
			printJob.SP_ParentTableName = "SomeTable";
			printJob.SP_GS_NKJobSubmittedBy = "HFE";
			printJob.SP_FaxDestination = "+61 (2) 9025-1199";
			var r1 = printJob.CarbonCopyRecipients.AddNew();
			r1.SPR_EmailAddress = "a@b.com";
			var r2 = printJob.CarbonCopyRecipients.AddNew();
			r2.SPR_EmailAddress = "b@c.com";
			var r3 = printJob.BlindCarbonCopyRecipients.AddNew();
			r3.SPR_EmailAddress = "c@d.com";
			var r4 = printJob.BlindCarbonCopyRecipients.AddNew();
			r4.SPR_EmailAddress = "d@e.com";

			printJob.ThrowExceptionWhenGetCarbonCopyRecipients = true;
			AssertExceptionThrown<Exception>(() =>
			{
				printJob.CreateLogOnParent(Events.DocumentDelivered);
			});

			AssertNoExceptionThrown(() =>
			{
				Factory.Save();
			});
		}

		[TestUtcOffset(+10, 0, 0)]
		public void TestSP_RunDateTimeLocal()
		{
			var job = Factory.New<PrintJobTest>();

			job.SP_RunDateTime = new ZDateTime(2011, 1, 20, 3, 42, 15);
			AssertEquals(new ZDateTime(2011, 1, 20, 13, 42, 15), job.SP_RunDateTimeLocal);

			job.SP_RunDateTime = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, job.SP_RunDateTimeLocal);
		}

		public void TestDelete_ShouldCascadeDeleteRecipients()
		{
			var printJob = Factory.New<StmPrintJob>();
			printJob.EmailToRecipients.Value = "a@b.com";
			printJob.CarbonCopyRecipients.Value = "a@b.com";
			printJob.BlindCarbonCopyRecipients.Value = "a@b.com";
			Factory.Save();

			printJob.Delete();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertNull("Job should have been deleted.", newFactory.Load<StmPrintJob>(printJob.PK));
			printJob.EmailToRecipients.ForEach(recipient => AssertNull("EmailToRecipients should have been deleted.", newFactory.Load<StmPrintJobCopyRecipient>(recipient.PK)));
			printJob.CarbonCopyRecipients.ForEach(recipient => AssertNull("CarbonCopyRecipients should have been deleted.", newFactory.Load<StmPrintJobCopyRecipient>(recipient.PK)));
			printJob.BlindCarbonCopyRecipients.ForEach(recipient => AssertNull("BlindCarbonCopyRecipients should have been deleted.", newFactory.Load<StmPrintJobCopyRecipient>(recipient.PK)));
		}

		public void TestSavingFailedJobSetsFailureReason()
		{
			Job.SP_Status = nameof(PrintJobStatus.FAL);
			Job.SP_FailureReason = ZString.Empty;
			Factory.Save();

			AssertStartsWith("Failure reason should not be empty", "Failed at ", Job.SP_FailureReason);
			AssertContains("Stack trace should be used if other reason not provided", nameof(TestSavingFailedJobSetsFailureReason), Job.SP_FailureReason);
		}

		public void TestSavingFailedJobDontOverrideFailureReason()
		{
			Job.SP_Status = nameof(PrintJobStatus.FAL);
			Job.SP_FailureReason = "Some failure reason";
			Factory.Save();

			AssertEquals("Existing failure reason should not be overridden", "Some failure reason", Job.SP_FailureReason);
		}

		public void TestSavingPreviouslyFailedJobDontChangeFailureReason()
		{
			Job.SP_Status = nameof(PrintJobStatus.FAL);
			Job.SP_FailureReason = ZString.Empty;
			Factory.Save();

			Assert("Failure reason should be set if SP_Status property has changes", !Job.SP_FailureReason.IsEmpty);

			Job.SP_FailureReason = ZString.Empty;
			Factory.Save();

			Assert("Failure reason should not be set if SP_Status property has no changes", Job.SP_FailureReason.IsEmpty);
		}

		StmPrintJob Job;

		protected override void SetUp()
		{
			base.SetUp();
			Job = Factory.NewWithValidTestData<StmPrintJob>();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		void AddPrintJobToQueue(StmPrintJob printJob)
		{
			var printJobQueue = printJob.Factory.New<StmPrintJobQueue>();
			printJobQueue.SPQ_JobType = printJob.SP_JobType;
			printJobQueue.SPQ_SP_PrintJob = printJob.PK;
			printJobQueue.SPQ_SB_DeliveryGroup = printJob.SP_SB_DeliveryGroup;
			printJobQueue.SPQ_EDocsProcessed = printJob.SP_EDocsProcessed;
			printJobQueue.SPQ_Sequence = 0;
		}

		void AssertPrintJobIsDeletedFromQueue(StmPrintJob printJob, Action<StmPrintJob> updateOnPrintJob, bool shouldRemainInDb = false)
		{
			AddPrintJobToQueue(printJob);

			Factory.Save();

			AssertEquals(true, Factory.ExistsInDatabase(StmPrintJobQueueSchema.Constants.TableName, new ZQuery(StmPrintJobQueueSchema.SPQ_SP_PrintJob, printJob.PK)));

			updateOnPrintJob?.Invoke(printJob);

			Factory.Save();

			AssertEquals(shouldRemainInDb, Factory.ExistsInDatabase(StmPrintJobQueueSchema.Constants.TableName, new ZQuery(StmPrintJobQueueSchema.SPQ_SP_PrintJob, printJob.PK)));
		}

		void AssertExcelFileIsEncrypted(string file, string password = null)
		{
			try
			{
				using (var excelInterface = new ExcelInterface())
				{
					if (string.IsNullOrEmpty(password))
					{
						AssertNoExceptionThrown(message: "Excel can be opened.", codeToRun: () =>
						{
							excelInterface.LoadExcelFile(file);
						});
					}
					else
					{
						AssertExceptionThrown<ExcelInterfaceException>(message: "Encrypted Excel can not be opened without password", codeToRun: () => { excelInterface.LoadExcelFile(file); });

						excelInterface.Xls.Protection.OpenPassword = password;
						AssertNoExceptionThrown(message: "Encrypted Excel can be opened with password", codeToRun: () => { excelInterface.LoadExcelFile(file); });
					}
				}
			}
			finally
			{
				File.Delete(file);
			}
		}

		void AssertPDFFileIsEncrypted(string file, string password = null)
		{
			try
			{
				if (string.IsNullOrEmpty(password))
				{
					AssertNoExceptionThrown(message: "PDF can be opened.", codeToRun: () =>
					{
						using (PdfReader.Open(file))
						{ }
					});
				}
				else
				{
					AssertExceptionThrown<PdfReaderException>(message: "Encrypted PDF can not be opened without password",
											expectedExceptionMessage: "A password is required to open the PDF document.",
											codeToRun: () =>
											{
												using (PdfReader.Open(file))
												{ }
											});

					var decryptedPassword = TwoWayEncoder.NewWithStandardInitialisationVector().Decrypt(password);
					AssertNoExceptionThrown(message: "Encrypted PDF can be opened with password", codeToRun: () =>
					{
						using (PdfReader.Open(file, decryptedPassword))
						{ }
					});
				}
			}
			finally
			{
				File.Delete(file);
			}
		}

		StmALog CreateDDVLog(DummyBusinessObject logParent, string referenceText, IEnumerable<KeyValuePair<string, string>> parameters = null)
		{
			var log = Factory.New<StmALog>();

			using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
			{
				log.SL_SE_NKEvent = Events.DocumentDeliveredCode;

				log.SL_Reference = StmALog.GenerateEventReferenceToFitInReferenceMaxLength(referenceText, parameters);
				log.SL_Parent = logParent.PK;
				log.SL_Table = DummyBusinessObject.Schema.TableName;
				log.SL_EventTime = ZDateTime.Now;
			}

			return log;
		}

		List<StmPrintJob> LockJobs(IEnumerable<StmPrintJob> jobs, DbConnection connection, DisposableList mutexs)
		{
			var lockedJobs = new List<StmPrintJob>(jobs.Count());
			foreach (var job in jobs)
			{
				SqlApplicationLock mutex;
				if (connection.TryGetLock("PrintJob:" + job.PK.ToString().ToLowerInvariant(), out mutex))
				{
					mutexs.Add(mutex);
					lockedJobs.Add(job);
				}
			}

			return lockedJobs;
		}

		void SetSP_Copies(short value, IEnumerable<StmPrintJob> jobs)
		{
			foreach (var job in jobs.WhereNotNull())
			{
				job.SP_Copies = value;
			}
		}

		void AssertSaveAttachmentToFilesystem(string filePath, string attachmentFormat, string emailAttachments, string expectedStoredAttachmentExtension)
		{
			string[] tempFilesBefore = Directory.GetFiles(Temp.TempPath);

			var testParentGuid = ZGuid.NewZGuid();
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = attachmentFormat;
			printJob.SP_Destination = "test@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(filePath);
			printJob.SP_EmailAttachments = emailAttachments;
			printJob.SP_EmailSubjectLine = "email subject";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SQ = ZGuid.Empty;
			Assert("Precondition: StoredAttachmentFilename is empty", printJob.StoredAttachmentFilename.IsEmpty);
			Factory.Save();

			try
			{
				printJob.SaveAttachmentToFilesystem(0);
				Assert("StoredAttachmentFilename should be filled in now", !printJob.StoredAttachmentFilename.IsEmpty);
				Assert("StoredAttachmentFilename exists", File.Exists(printJob.StoredAttachmentFilename));
				Assert("StoredAttachmentSize should be filled in now", !printJob.StoredAttachmentSizeKB.IsEmpty);
				AssertEquals("File extention type of SavedFilename", expectedStoredAttachmentExtension, Path.GetExtension(printJob.StoredAttachmentFilename).ToUpper());

				if ((attachmentFormat == AttachmentTypeList.Codes.Pdf || attachmentFormat == AttachmentTypeList.Codes.Pdfc) && Path.GetExtension(emailAttachments) == ".TIF")
				{
					return;
				}

				string[] tempFilesAfter = Directory.GetFiles(Temp.TempPath);
				AssertEquals("Only one more file in the temp path", tempFilesBefore.Length + 1, tempFilesAfter.Length);
			}
			finally
			{
				File.Delete(printJob.StoredAttachmentFilename);
			}
		}

		StmDeliveryGroup CreateNewDeliveryGroup(string subjectLine)
		{
			var deliveryGroup = Factory.New<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;
			if (!string.IsNullOrEmpty(subjectLine))
			{
				deliveryGroup.SB_EmailSubjectLine = subjectLine;
			}
			return deliveryGroup;
		}

		StmPrintJob CreateNewPrintJobWithEmails(string emails, ZGuid deliveryGroupId)
		{
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_JobType = "EML";
			printJob.SP_Destination = emails;
			printJob.SP_SB_DeliveryGroup = deliveryGroupId;

			return printJob;
		}

		void AssertSaveAttachmentToFilesystemXLStoSignedPDF(DigitalSignatureRegistry signatureRegistry)
		{
			DocumentsDataRegistry.Instance.DigitalSignature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, signatureRegistry);

			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdf;
			printJob.SP_Destination = "test@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(Path.Combine(UnitTestingConstants.TestDocumentDigitalSignatureFilePath, "TestSignXlsDocument.xls"));
			printJob.SP_EmailAttachments = "test.xls";
			printJob.SP_EmailSubjectLine = "email subject";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = ZGuid.NewZGuid();
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SQ = ZGuid.Empty;
			printJob.SP_SignBy = DocumentsSignBy.PFX;

			Factory.Save();

			try
			{
				printJob.SaveAttachmentToFilesystem(0);
				AssertEquals("File extention type of SavedFilename", ".PDF", Path.GetExtension(printJob.StoredAttachmentFilename).ToUpper());

				var signedConvertedPDF = File.ReadAllBytes(printJob.StoredAttachmentFilename);
				var expectedFileName = "testFilesystemXLStoSignedPDF.pdf";
				var expectedPdfData = File.ReadAllBytes(Path.Combine(UnitTestingConstants.TestDocumentDigitalSignatureFilePath, expectedFileName));

				// Uncomment the following line if this test is failing and you want to see what the actual output is.
				//File.WriteAllBytes(@"c:\tmp\" + expectedFileName, signedConvertedPDF);

				AssertMultilineASCIIEquals("Output PDF should be the same.", ImageToPDFConverterTest.GetStringForPDFComparison(expectedPdfData), ImageToPDFConverterTest.GetStringForPDFComparison(signedConvertedPDF));

				var expectedSignature = new List<(string key, string value)>()
				{
					(("Location", "Sango")),
					(("Reason", $@"{BrandingFactory.Instance.CompanyBrandingName} \(WTG\) has provided the feature to generate and sign this PDF document using the data verified by the application users and available in {Core.Constants.ProductName} at the time of signing. WTG or any of its employees shall not be held responsible for any discrepancies observed in the data contained in this document.")),
					(("ContactInfo", "Sango@Sango.com"))
				};

				var actualSignature = ImageToPDFConverterTest.GetSignatureFromPDF(signedConvertedPDF);

				AssertContainsExactElementsInAnyOrder(expectedSignature, actualSignature);
			}
			finally
			{
				printJob.DeleteStoredAttachment();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		byte[] ReportTestXls => resourceRetriever.Value.GetBytes("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.test.xls");

		byte[] DocumentTestXls => resourceRetriever.Value.GetBytes("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.Test.xls");

		sealed class PrintJobTest : StmPrintJob
		{
			public PrintJobTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			internal bool IsAutoLoggedForTest => base.IsAutoLogged;
		}

		sealed class PrintJobForTestStmALog : StmPrintJob
		{
			public PrintJobForTestStmALog(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override StmPrintJobCopyRecipientCollection CarbonCopyRecipients
			{
				get
				{
					if (ThrowExceptionWhenGetCarbonCopyRecipients)
					{
						throw new Exception("Exception When Create StmALog");
					}

					return base.CarbonCopyRecipients;
				}
			}

			public bool ThrowExceptionWhenGetCarbonCopyRecipients;
		}
	}
}
