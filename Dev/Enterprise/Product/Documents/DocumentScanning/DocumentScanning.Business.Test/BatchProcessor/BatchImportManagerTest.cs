using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.IO.Testing;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MailManager.MailFilters.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Testing
{
	sealed class BatchImportManagerTest : TestCaseWithDocumentFactory
	{
		public void TestImportFileContent_WhenAllocationInformationIsConfiguredInPrefix()
		{
			var branch = Factory.Load<GlbBranch>(new ZQuery())[0];
			var company = Factory.Load<GlbCompany>(branch.GB_GC);
			var initialDepartment = Factory.Load<GlbDepartment>(new ZQuery())[0];
			var fileName = $"[SHP MSC S00001000 C@{company.GC_Code} B@{branch.GB_Code}] small.tif";

			var messageList = new List<string>();
			var batchImportManagerInternals = new BatchImportManager();
			batchImportManagerInternals.LogProgress += args =>
			{
				messageList.Add(args.EventType + " - " + args.Message);
			};
			((IBatchImportManagerInternals)batchImportManagerInternals).ImportFileContent(MasterFactory, SmallTifBytes, fileName, "INV", Guid.Empty, Guid.Empty, initialDepartment.PK.ToGuid());

			MasterFactory.Save();

			var factoryOne = MasterFactory.GetFactory(1);

			AssertEquals("Should import one file and allocate because it has all allocation info", 1, factoryOne.GetDatabaseCount(typeof(StorageDocs)));

			var document = factoryOne.LoadTop1<StorageDocs>(new ZQuery());

			AssertEquals("Company should be same as the one in prefix", company.PK, document.SC_GC_Company);
			AssertEquals("Branch should be same as the one in prefix", branch.PK, document.SC_GB_Branch);
			AssertEquals("Department should be empty", true, document.SC_GE_Department.IsEmpty);
			AssertEquals("Warning - The visibility parameters specified in the FileName element for file '[SHP MSC S00001000 C@EDI B@BNE] small.tif' will be used in place of the VisibleCompanyCode, VisibleBranchCode and VisibleDepartmentCode elements.", messageList[0]);
		}

		public void TestImportFileContent_WhenAllocationInformationIsNotConfiguredInPrefix()
		{
			var initialBranch = Factory.Load<GlbBranch>(new ZQuery())[0];
			var initialCompany = Factory.Load<GlbCompany>(initialBranch.GB_GC);
			var fileName = "[SHP MSC S00001000] small.tif";

			IBatchImportManagerInternals batchImportManagerInternals = new BatchImportManager();
			batchImportManagerInternals.ImportFileContent(MasterFactory, new byte[] { 1, 2, 3 }, fileName, "INV", initialCompany.PK.ToGuid(), initialBranch.PK.ToGuid(), Guid.Empty);
			MasterFactory.Save();

			var factoryOne = MasterFactory.GetFactory(1);

			AssertEquals("Should import one file and allocate because it has all allocation info", 1, factoryOne.GetDatabaseCount(typeof(StorageDocs)));

			var document = factoryOne.LoadTop1<StorageDocs>(new ZQuery());

			AssertEquals("Company should keep the initial company", initialCompany.PK, document.SC_GC_Company);
			AssertEquals("Branch should keep the initial branch", initialBranch.PK, document.SC_GB_Branch);
			AssertEquals("Department should be empty", true, document.SC_GE_Department.IsEmpty);
		}

		public void TestBatchImportToBookingWithDocumentTracking()
		{
			try
			{
				var viewQuotedBooking = Factory.New<ViewQuotedBooking>();
				var booking = QuotedBooking.CreateNewBooking(Factory);
				booking.JS_UniqueConsignRef = "S00001111";
				viewQuotedBooking.VB_JS = booking.PK;
				Factory.Save();

				Directory.CreateDirectory(TestDirectory);
				var fileName = "[DocManager BKG CIV S00001111] small.txt";
				var fileFullPath = Path.Combine(TestDirectory, fileName);
				File.Create(fileFullPath).Dispose();
				File.WriteAllText(fileFullPath, "123123123");

				var manager = new BatchImportManager();
				manager.ImportFilesAndEmails(TestDirectory);

				var storageMain = new DbBackendDocumentFactory(Factory).LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, booking.PK));
				AssertEquals("SM_Type for ViewQuotedBooking Should be SHP instead of BKG.", "SHP", storageMain.SM_Type);

				var jobDocsAndCartagePK = booking.DocsAndCartage.PK;
				var requiredDocuments = Factory.Load<JobRequiredDocument>(new ZQuery(JobRequiredDocumentSchema.EQ_ParentID, jobDocsAndCartagePK));
				AssertNotNull("Should have value.", requiredDocuments.First().EQ_DateReceived);
			}
			finally
			{
				if (Directory.Exists(TestDirectory))
				{
					Directory.Delete(TestDirectory, true);
				}
			}
		}

		public void TestLogProgressSetForGetDocumentFactory()
		{
			List<string> messageList = new List<string>();
			var manager = new BatchImportManager();
			manager.LogProgress += args => { messageList.Add(args.EventType + " - " + args.Message); };
			var documentFactory = manager.GetDocumentFactory();
			documentFactory.OnLog(TraceEventType.Information, "Blah blah");
			AssertEquals(1, messageList.Count);
			AssertEquals("Information - Blah blah", messageList[0]);
		}

		public void TestImportEmailWithJobRequiredDocument_HandleZCannotSaveException()
		{
			try
			{
				var email = CreateEmailBySubject(string.Format("[ediDocManager SHP PKL S00001000]"));
				email.MI_Body = "JobRequiredDocument ZCannotSaveException";

				AssertEquals(0, (ShipmentS00001000 as IDocManagerSupport).DocManagerInfo.AllEDocs.Count);

				var requiredDocument = (ShipmentS00001000 as IDocsAndCartageParent).RequiredDocumentsProvider.RequiredDocuments.AddNew();
				requiredDocument.EQ_DocType = "PKL";
				requiredDocument.EQ_DocCategory = "SCL";
				requiredDocument.EQ_DocDescription = "Packing List";
				requiredDocument.EQ_DocPeriod = "SHP";
				requiredDocument.EQ_DocUsage = "BTH";
				MasterFactory.Save();

				var manager = new BatchImportManager();
				var throwZCannotSaveException = true;
				var changeRequiredDocument = true;
				BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
				{
					if (throwZCannotSaveException)
					{
						throwZCannotSaveException = false;
						throw new ZCannotSaveException("While you're saving the required document with type PKL, there's another user is saving the same document with same type, please try again later.", "Cannot Save Required Document");
					}

					if (changeRequiredDocument)
					{
						changeRequiredDocument = false;
						var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
						var shipmentReloaded = newFactory.Load<Enterprise.Integration.Forwarding.IForwardingShipment>(ShipmentS00001000.PK);
						var requiredDocumentReloaded = (shipmentReloaded as IDocsAndCartageParent).RequiredDocumentsProvider.RequiredDocuments[0];
						requiredDocumentReloaded.EQ_DateReceived = ZDateTimeOffset.Now.AddMinutes(-5);
						newFactory.Save();
					}
				});
				AssertNoExceptionThrown(() => { manager.ImportFilesAndEmails(TestDirectory); });
				var shipment = MasterFactory.Load<Enterprise.Integration.Forwarding.IForwardingShipment>(ShipmentS00001000.PK);
				AssertEquals(1, (shipment as IDocManagerSupport).DocManagerInfo.AllEDocs.Count);
			}
			finally
			{
				if (Directory.Exists(TestDirectory))
				{
					Directory.Delete(TestDirectory, true);
				}
			}
		}

		public void TestImportWithEmptyDataImageFileInFolder()
		{
			try
			{
				var apInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
				apInvoice.AH_TransactionType = "INV";
				apInvoice.AH_TransactionNum = "BCR0720";

				Factory.Save();

				Directory.CreateDirectory(TestDirectory);
				var fileName = "[EdiDocManager ACP PIN BCR0720] small.tif";
				var fileFullPath = Path.Combine(TestDirectory, fileName);
				File.Create(fileFullPath).Dispose();

				BatchImportManager manager = new BatchImportManager();
				List<string> logs = new List<string>();
				manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);
				manager.ImportFilesAndEmails(TestDirectory);

				var expectedLog = $@"Information - Found 1 DocManager Files to import in {TestDirectory}:
{fileFullPath}
Information - Importing file '{fileFullPath}'
Information - Importing from File system.
Full filename: {fileFullPath};
Filename only: {fileName}.
Error - File '{fileName}' could not be imported. Error message: File is empty.
Information - File '{fileFullPath}' has been moved to unsuccessful folder.
Information - File '{fileFullPath}' is processed.
Information - Import Run Completed, processed 1 file(s), ignored 0 file(s) processed by another process.";

				AssertEquals(expectedLog, logs.ToStringWithNewLineBetweenStrings());
			}
			finally
			{
				if (Directory.Exists(TestDirectory))
				{
					Directory.Delete(TestDirectory, true);
				}
			}
		}

		public void TestImportWithEmptyDataImageFileOnEmail()
		{
			try
			{
				var staff = MasterFactory.NewWithValidTestData<GlbStaff>();
				staff.GS_FullName = "Jerry Feng";
				staff.GS_EmailAddress = "jerry@test.com";

				var email1 = MasterFactory.New<MailItem>();
				email1.MI_Body = "Test email";
				email1.MI_Subject = "[ediDocManager]";
				email1.MI_From = staff.GS_EmailAddress;
				email1.MI_Direction = MailDirection.Receive;
				email1.MI_Status = MailStatus.Queued;
				email1.MI_LastAttemptDateTime = new ZDateTime(2021, 6, 2);
				email1.MI_SendDateTime = new ZDateTime(2021, 6, 2);
				email1.MI_ReceivedDateTime = new ZDateTime(2021, 6, 2);
				MailFilterLocatorTestHelper.SetApplication(email1, MailFilterCodes.DocumentImportManager);

				var attachment1_1 = email1.MailAttachments.AddNew();
				attachment1_1.MA_FileName = "emptyImage1.png";
				attachment1_1.MA_Data = null;
				attachment1_1.MA_MI = email1.PK;

				var attachment1_2 = email1.MailAttachments.AddNew();
				attachment1_2.MA_FileName = "emptyImage2.png";
				attachment1_2.MA_Data = Array.Empty<byte>();
				attachment1_2.MA_MI = email1.PK;

				var attachment1_3 = email1.MailAttachments.AddNew();
				attachment1_3.MA_FileName = "small.gif";
				attachment1_3.MA_Data = SmallGifBytes;
				attachment1_3.MA_MI = email1.PK;

				MasterFactory.Save();

				var securityInstance = SecurityTestHelper.CreateSecurityInstance(MasterFactory, staff);
				securityInstance.AllocateDocuments.IsAllowed = true;
				using var disposable = Env.SetTemporarySecurityInstanceForTest(securityInstance);

				var manager = new BatchImportManagerWithTimestamp();
				var logs = new List<string>();
				manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);

				manager.ImportFilesAndEmails(TestDirectory);

				var expectedLog = @"Information - Importing emails in batch of size 1.
Information - Importing email from 'jerry@test.com' with subject '[ediDocManager]'.
Error - File 'emptyImage1.png' could not be imported. Error message: File is empty.
Error - File 'emptyImage2.png' could not be imported. Error message: File is empty.
Information - Imported file 'small.gif'.
Information - An email has been sent to Jerry Feng explaining why the import failed.
Error - File 'emptyImage1.png' could not be imported. Error message: File is empty.
Error - File 'emptyImage2.png' could not be imported. Error message: File is empty.
Information - Imported file 'small.gif'.
Information - An email has been sent to Jerry Feng explaining why the import failed.
Error - File 'emptyImage1.png' could not be imported. Error message: File is empty.
Error - File 'emptyImage2.png' could not be imported. Error message: File is empty.
Information - Imported file 'small.gif'.
Information - An email has been sent to Jerry Feng explaining why the import failed.
Information - Email '[ediDocManager]' is processed.
Information - Import Run Completed, processed 1 email(s), ignored 0 email(s) processed by another process.";
				AssertEquals(expectedLog, logs.ToStringWithNewLineBetweenStrings());
				AssertEquals(MailStatus.Failed, email1.MI_Status);
			}
			finally
			{
				if (Directory.Exists(TestDirectory))
				{
					Directory.Delete(TestDirectory, true);
				}
			}
		}

		void TestEmailImport_WithSecurityCheckpoint(bool allocateAllowed, bool docTypeAllowed, string expectedLog)
		{
			var staff = MasterFactory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Test User";
			staff.GS_EmailAddress = "test@test.com";

			var email = MasterFactory.New<MailItem>();
			email.MI_Body = "Test email";
			email.MI_Subject = "[ediDocManager SHP CIV S00001000]";
			email.MI_From = staff.GS_EmailAddress;
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_ReceivedDateTime = new ZDateTime(2021, 6, 2);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			var attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "file.pdf";
			attachment.MA_Data = new byte[] { 1, 2, 3 };
			attachment.MA_MI = email.PK;

			MasterFactory.Save();

			var securityInstance = SecurityTestHelper.CreateSecurityInstance(MasterFactory, staff);
			securityInstance.AllocateDocuments.IsAllowed = allocateAllowed;
			securityInstance.GetDocumentTypeUploadCheckPoint("CIV").IsAllowed = docTypeAllowed;
			Env.SetTemporarySecurityInstanceForTest(securityInstance);

			var manager = new BatchImportManagerWithTimestamp();
			var logs = new List<string>();
			manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);

			manager.ImportFilesAndEmails("");
			email.Reload();

			AssertEquals(expectedLog, logs.ToStringWithNewLineBetweenStrings());
			AssertEquals(allocateAllowed && docTypeAllowed ? MailStatus.Processed : MailStatus.Failed, email.MI_Status);
		}

		public void TestEmailImport_AllocateDocumentsCheckpoint_NotAllowed()
		{
			var expectedLog = @"Information - Importing emails in batch of size 1.
Information - Importing email from 'test@test.com' with subject '[ediDocManager SHP CIV S00001000]'.
Error - Unable to allocate document. Allocation has been rejected because the staff user does not have appropriate security rights enabled for the security setting at Manage -> DocManager -> Allocate eDocs.
Error - Unable to allocate document. Allocation has been rejected because the staff user does not have appropriate security rights enabled for the security setting at Manage -> DocManager -> Allocate eDocs.
Error - Unable to allocate document. Allocation has been rejected because the staff user does not have appropriate security rights enabled for the security setting at Manage -> DocManager -> Allocate eDocs.
Information - Email '[ediDocManager SHP CIV S00001000]' is processed.
Information - Import Run Completed, processed 1 email(s), ignored 0 email(s) processed by another process.";
			TestEmailImport_WithSecurityCheckpoint(allocateAllowed: false, docTypeAllowed: false, expectedLog);
		}

		public void TestEmailImport_AllocateDocumentsCheckpoint_DocType_NotAllowed()
		{
			var expectedLog = @"Information - Importing emails in batch of size 1.
Information - Importing email from 'test@test.com' with subject '[ediDocManager SHP CIV S00001000]'.
Error - Unable to allocate document. Allocation has been rejected because the staff user does not have appropriate security rights enabled for the security setting at Manage -> DocManager -> Allocate eDocs -> eDocs Tab - Upload Specific Document Type -> CIV.
Error - Unable to allocate document. Allocation has been rejected because the staff user does not have appropriate security rights enabled for the security setting at Manage -> DocManager -> Allocate eDocs -> eDocs Tab - Upload Specific Document Type -> CIV.
Error - Unable to allocate document. Allocation has been rejected because the staff user does not have appropriate security rights enabled for the security setting at Manage -> DocManager -> Allocate eDocs -> eDocs Tab - Upload Specific Document Type -> CIV.
Information - Email '[ediDocManager SHP CIV S00001000]' is processed.
Information - Import Run Completed, processed 1 email(s), ignored 0 email(s) processed by another process.";
			TestEmailImport_WithSecurityCheckpoint(allocateAllowed: true, docTypeAllowed: false, expectedLog);
		}

		public void TestEmailImport_AllocateDocumentsCheckpoint_DocType_Allowed()
		{
			var expectedLog = @"Information - Importing emails in batch of size 1.
Information - Importing email from 'test@test.com' with subject '[ediDocManager SHP CIV S00001000]'.
Information - Imported file 'file.pdf'.
Information - Email '[ediDocManager SHP CIV S00001000]' is processed.
Information - Import Run Completed, processed 1 email(s), ignored 0 email(s) processed by another process.";
			TestEmailImport_WithSecurityCheckpoint(allocateAllowed: true, docTypeAllowed: true, expectedLog);
		}

		sealed class SecurityTestHelper
		{
			public static SecurityCore CreateSecurityInstance(BusinessObjectFactory factory, GlbStaff staff)
			{
				var securityCollection = new GlbSecurityCollection(factory);
				securityCollection.Load();

				return new SecurityCore(securityCollection, staff, Guid.Empty, Guid.Empty, Guid.Empty);
			}
		}

		public void TestImportFilesWhenFilePathLengthMoreThanLockKeyLength()
		{
			var apInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			apInvoice.AH_TransactionType = "INV";
			apInvoice.AH_TransactionNum = "BCR0720";
			Factory.Save();

			var manager = new BatchImportManager();
			var logs = new List<string>();
			manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);
			var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif", "[EdiDocManager ACP PIN BCR0720] small.tif");
			var testDirectory = Path.GetDirectoryName(testFile);

			using (var mutexes = new DisposableList(0))
			using (var anotherConnection = Db.NewExtraConnectionToMainDb())
			{
				AssertNoExceptionThrown(() => manager.ImportFilesAndEmails(testDirectory));

				var expectedLog = $@"Information - Found 1 DocManager Files to import in {testDirectory}:
{testFile}
Information - Importing file '{testFile}'
Information - Importing from File system.
Full filename: {testFile};
Filename only: [EdiDocManager ACP PIN BCR0720] small.tif.
Information - Imported file '[EdiDocManager ACP PIN BCR0720] small.TIF'.
Information - File '{testFile}' is processed.
Information - Import Run Completed, processed 1 file(s), ignored 0 file(s) processed by another process.";

				AssertEquals(expectedLog, logs.ToStringWithNewLineBetweenStrings());
			}
		}

		public void TestImportFilesAndEmailsWithMutex()
		{
			var email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email";
			email.MI_Subject = "[ediDocManager CNT MSC 00001]";
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2020, 9, 18);
			email.MI_SendDateTime = new ZDateTime(2020, 9, 18);
			email.MI_ReceivedDateTime = new ZDateTime(2020, 9, 18);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			var container = MasterFactory.NewWithValidTestData<CommonContainer>();
			container.JC_ContainerNum = "00001";

			var apInvoice = MasterFactory.NewWithValidTestData<AccTransactionHeader>();
			apInvoice.AH_TransactionType = "INV";
			apInvoice.AH_TransactionNum = "BCR0720";
			MasterFactory.Save();

			var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif", "[EdiDocManager ACP PIN BCR0720] small.tif");
			var testDirectory = Path.GetDirectoryName(testFile);

			var manager = new BatchImportManager();
			var logs = new List<string>();
			manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);

			using (var mutexes = new DisposableList(0))
			using (var anotherConnection = Db.NewExtraConnectionToMainDb())
			{
				anotherConnection.TryGetLock(manager.GetSafeLockKey(testFile), out var fileLock);
				anotherConnection.TryGetLock($"DMI:ImportingEmail:{email.PK}", out var emailLock);
				mutexes.Add(fileLock);
				mutexes.Add(emailLock);

				manager.ImportFilesAndEmails(testDirectory);

				var expectedLog = $@"Information - Found 1 DocManager Files to import in {testDirectory}:
{testFile}
Verbose - File '{testFile}' is being processed by another process, ignoring it.
Information - Import Run Completed, processed 0 file(s), ignored 1 file(s) processed by another process.
Information - Importing emails in batch of size 1.
Information - Importing email from 'test@test.com' with subject '[ediDocManager CNT MSC 00001]'.
Verbose - Email '[ediDocManager CNT MSC 00001]' is being processed by another process, ignoring it.
Information - Import Run Completed, processed 0 email(s), ignored 1 email(s) processed by another process.";

				AssertEquals(expectedLog, logs.ToStringWithNewLineBetweenStrings());
			}
		}

		public void TestImportFileWithUnUniqueID()
		{
			var apCredit = Factory.NewWithValidTestData<AccTransactionHeader>();
			apCredit.AH_TransactionType = "CRD";
			apCredit.AH_TransactionNum = "BCR0720";

			var apInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			apInvoice.AH_TransactionType = "INV";
			apInvoice.AH_TransactionNum = "BCR0720";

			Factory.Save();

			var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif", "[EdiDocManager ACP PIN BCR0720] small.tif");
			var testDirectory = Path.GetDirectoryName(testFile);

			BatchImportManager manager = new BatchImportManager();
			List<string> logs = new List<string>();
			manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);
			manager.ImportFilesAndEmails(testDirectory);

			AssertEquals(7, logs.Count);

			AssertEquals($"Information - Found 1 DocManager Files to import in {testDirectory}:\r\n{testFile}", logs[0]);
			AssertEquals($"Information - Importing file '{testFile}'", logs[1]);
			AssertEquals($"Information - Importing from File system.\r\nFull filename: {testFile};\r\nFilename only: [EdiDocManager ACP PIN BCR0720] small.tif.", logs[2]);
			AssertEquals($"Error - File '[EdiDocManager ACP PIN BCR0720] small.tif' could not be imported. The unique ID BCR0720 you have supplied for Reference Type ACP exists in multiple records.", logs[3]);
			AssertEquals($"Information - File '{testFile}' has been moved to unsuccessful folder.", logs[4]);
			AssertEquals($"Information - File '{testFile}' is processed.", logs[5]);
			AssertEquals($"Information - Import Run Completed, processed 1 file(s), ignored 0 file(s) processed by another process.", logs[6]);
		}

		public void TestImportEmailWithErrorOnSaveShouldSaveFailedEmailWithNewFactory()
		{
			try
			{
				var email = CreateEmailBySubject(string.Format("[ediDocManager SHP MSC S00001000]"));
				email.MI_Body = "Email Factory Cannot Save";
				MasterFactory.Save();

				var emailReloadedCount = 0;

				email.Reloaded += (sender, e) => { emailReloadedCount++; };

				var manager = new BatchImportManager();
				AssertNoExceptionThrown(() => { manager.ImportFilesAndEmails(TestDirectory); });

				email.Reload();
				AssertEquals("Status FAL should be saved to database", MailStatus.Failed, email.MI_Status);
				AssertEquals("Status FAL should be synced to the processing email", MailStatus.Failed, manager.ProcessingEmail.MI_Status);
				AssertEquals("New factory should call save once", 1, emailReloadedCount);
			}
			finally
			{
				if (Directory.Exists(TestDirectory))
				{
					Directory.Delete(TestDirectory, true);
				}
			}
		}

		public void TestImportEmailWithErrorOnSaveShouldLogErrorMessage()
		{
			try
			{
				var email = CreateEmailBySubject(string.Format("[ediDocManager SHP MSC S00001000]"));
				email.MI_Body = "Test deadlock exception";

				MasterFactory.Save();

				BatchImportManagerWithTimestamp manager = new BatchImportManagerWithTimestamp();
				List<string> logs = new List<string>();
				manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);

				AssertNoExceptionThrown(() => { manager.ImportFilesAndEmails(TestDirectory); });

				var actual = string.Join("\r\n", logs);

				AssertContains(@"An error occurred while importing the email.
Caption: Cannot Save...
Message: Server cancelled the operation due to deadlock with another operation. Please try again.", actual);
			}
			finally
			{
				if (Directory.Exists(TestDirectory))
				{
					Directory.Delete(TestDirectory, true);
				}
			}
		}

		public void TestImportEmail_BranchUsedForImport()
		{
			try
			{
				// Create job user, and a branch for the user
				var staff = Factory.New(typeof(GlbStaff)) as GlbStaff;
				staff.GS_Code = "PRJ";
				staff.GS_FullName = "Jane Doe";
				staff.GS_EmailAddress = "jane@example";
				staff.GS_LoginName = "janedoe";

				var company1 = Factory.New(typeof(GlbCompany)) as GlbCompany;
				company1.GC_Code = "CP1";
				company1.GC_Name = "A Company";

				var userBranch = Factory.New(typeof(GlbBranch)) as GlbBranch;
				userBranch.GB_Code = "BP1";
				userBranch.GB_GC = company1.PK;
				userBranch.GB_BranchName = "A Branch";
				staff.GS_GB_HomeBranch = userBranch.PK;

				var email = CreateEmailBySubject(string.Format("[ediDocManager SHP MSC S00001000]"));
				email.MI_Body = "Email Factory Cannot Save";
				email.MI_From = staff.GS_EmailAddress;
				MasterFactory.Save();

				var manager = new BatchImportManager();
				AssertNoExceptionThrown(() => { manager.ImportFilesAndEmails(TestDirectory); });
				AssertEquals("Current branch is used to import email if it's not null", GlbBranch.CurrentBranch.PK, manager.BranchUsedForImportEmail.PK);
			}
			finally
			{
				if (Directory.Exists(TestDirectory))
				{
					Directory.Delete(TestDirectory, true);
				}
			}
		}

		public void TestFolderThatHasSecurityRights()
		{
			DirectoryInfo testDirectory = Directory.CreateDirectory(TestDirectory);
			DirectorySecurity testDirectorySecurity = GetDirectoryAccessControl(testDirectory.FullName);
			SecurityIdentifier allUsers = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
			FileSystemAccessRule accessRule = new FileSystemAccessRule(allUsers, FileSystemRights.Read | FileSystemRights.Synchronize, InheritanceFlags.None, PropagationFlags.NoPropagateInherit, AccessControlType.Deny); // the test doesn't work bcoz 
			testDirectorySecurity.AddAccessRule(accessRule);

			BatchImportManager manager = new BatchImportManager();

			AssertEquals(true, manager.CheckSecuritySettings(testDirectory.FullName));

			if (Directory.Exists(testDirectory.FullName))
			{
				string[] files = Directory.GetFiles(testDirectory.FullName);

				foreach (string file in files)
				{
					File.SetAttributes(file, FileAttributes.Normal);
					File.Delete(file);
				}

				Directory.Delete(testDirectory.FullName);
			}
			if (Directory.Exists(TestDirectory))
			{
				string[] files = Directory.GetFiles(TestDirectory);

				foreach (string file in files)
				{
					File.SetAttributes(file, FileAttributes.Normal);
					File.Delete(file);
				}

				Directory.Delete(TestDirectory);
			}
		}

		public void TestImportWithBadImageFileOnEmail()
		{
			try
			{
				var staff = MasterFactory.NewWithValidTestData<GlbStaff>();
				staff.GS_FullName = "Juan Manuel Fangio";
				staff.GS_EmailAddress = "fangio@fastestmanontheplanet.com";

				var email1 = MasterFactory.New<MailItem>();
				email1.MI_Body = "This is the first test email";
				email1.MI_Subject = "[ediDocManager]";
				email1.MI_From = staff.GS_EmailAddress;
				email1.MI_Direction = MailDirection.Receive;
				email1.MI_Status = MailStatus.Queued;
				email1.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
				email1.MI_SendDateTime = new ZDateTime(2005, 12, 1);
				email1.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
				MailFilterLocatorTestHelper.SetApplication(email1, MailFilterCodes.DocumentImportManager);

				var attachment1_1 = email1.MailAttachments.AddNew();
				attachment1_1.MA_FileName = "badImage1.png";
				attachment1_1.MA_Data = BadImagePngBytes;
				attachment1_1.MA_MI = email1.PK;

				var attachment1_2 = email1.MailAttachments.AddNew();
				attachment1_2.MA_FileName = "small.gif";
				attachment1_2.MA_Data = SmallGifBytes;
				attachment1_2.MA_MI = email1.PK;

				var attachment1_3 = email1.MailAttachments.AddNew();
				attachment1_3.MA_FileName = "badImage2.png";
				attachment1_3.MA_Data = BadImagePngBytes;
				attachment1_3.MA_MI = email1.PK;

				var email2 = MasterFactory.New<MailItem>();
				email2.MI_Body = "This is second test email";
				email2.MI_Subject = "[ediDocManager]";
				email2.MI_From = "someotherperson@elsewhere.com";
				email2.MI_Direction = MailDirection.Receive;
				email2.MI_Status = MailStatus.Queued;
				email2.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
				email2.MI_SendDateTime = new ZDateTime(2005, 12, 1);
				email2.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
				MailFilterLocatorTestHelper.SetApplication(email2, MailFilterCodes.DocumentImportManager);

				var attachment2_1 = email2.MailAttachments.AddNew();
				attachment2_1.MA_FileName = "small.gif";
				attachment2_1.MA_Data = SmallGifBytes;
				attachment2_1.MA_MI = email2.PK;

				var attachment2_2 = email2.MailAttachments.AddNew();
				attachment2_2.MA_FileName = "small.JPG";
				attachment2_2.MA_Data = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.JPG");
				attachment2_2.MA_MI = email2.PK;

				MasterFactory.Save();

				var securityInstance = SecurityTestHelper.CreateSecurityInstance(MasterFactory, staff);
				securityInstance.AllocateDocuments.IsAllowed = true;
				Env.SetTemporarySecurityInstanceForTest(securityInstance);

				var manager = new BatchImportManagerWithTimestamp();
				var logs = new List<string>();
				manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);

				AssertNoExceptionThrown(delegate
				{ manager.ImportFilesAndEmails(TestDirectory); });
				email1.Reload();
				email2.Reload();

				var expected = @"Information - Importing emails in batch of size 2.
Information - Importing email from 'fangio@fastestmanontheplanet.com' with subject '[ediDocManager]'.
Warning - File 'badImage1.png' could not be processed as an image due to invalid file content. The image data stream supplied does not contain a valid image format.
Error - Exception of type 'Enterprise.DocumentEngine.Exceptions.CorruptedDocumentException' was thrown.
Information - Imported file 'small.gif'.
Warning - File 'badImage2.png' could not be processed as an image due to invalid file content. The image data stream supplied does not contain a valid image format.
Error - Exception of type 'Enterprise.DocumentEngine.Exceptions.CorruptedDocumentException' was thrown.
Information - An email has been sent to Juan Manuel Fangio explaining why the import failed.
Warning - File 'badImage1.png' could not be processed as an image due to invalid file content. The image data stream supplied does not contain a valid image format.
Error - Exception of type 'Enterprise.DocumentEngine.Exceptions.CorruptedDocumentException' was thrown.
Information - Imported file 'small.gif'.
Warning - File 'badImage2.png' could not be processed as an image due to invalid file content. The image data stream supplied does not contain a valid image format.
Error - Exception of type 'Enterprise.DocumentEngine.Exceptions.CorruptedDocumentException' was thrown.
Information - An email has been sent to Juan Manuel Fangio explaining why the import failed.
Warning - File 'badImage1.png' could not be processed as an image due to invalid file content. The image data stream supplied does not contain a valid image format.
Error - Exception of type 'Enterprise.DocumentEngine.Exceptions.CorruptedDocumentException' was thrown.
Information - Imported file 'small.gif'.
Warning - File 'badImage2.png' could not be processed as an image due to invalid file content. The image data stream supplied does not contain a valid image format.
Error - Exception of type 'Enterprise.DocumentEngine.Exceptions.CorruptedDocumentException' was thrown.
Information - An email has been sent to Juan Manuel Fangio explaining why the import failed.
Information - Email '[ediDocManager]' is processed.
Information - Importing email from 'someotherperson@elsewhere.com' with subject '[ediDocManager]'.
Information - Imported file 'small.gif'.
Information - Imported file 'small.JPG'.
Information - Email '[ediDocManager]' is processed.
Information - Import Run Completed, processed 2 email(s), ignored 0 email(s) processed by another process.";

				var actual = string.Join("\r\n", logs);
				AssertEquals("LogProgress Events", expected, actual);
				AssertEquals(MailStatus.Failed, email1.MI_Status);
				AssertEquals(MailStatus.Processed, email2.MI_Status);

				var emailDefs = Env.OutgoingMailManager.EmailsCreated.FindAll(mailItem => { return mailItem.Subject.StartsWith("DocManager Service Task Import Failed"); });
				foreach (var emailDef in emailDefs)
				{
					AssertNotNull("Email with Subject Starting with 'DocManager Service Task Import Failed'", emailDef);
					AssertMultilineASCIIEquals("emailDef.Body", Res.GetString("2f135b64-ecbb-4a7f-a1cc-95232493f5a0", @"The DocManager Service Task could not import the attachments sent on {0}
			
{1}

Are you trying to add non-image files?
The DocManager Service Task will recognize these file types: {2}. If the files you are trying to import are not image files, or cannot be converted to image files, you need to specify the 3-letter Reference, 3 or 4 letters Document Type and the unique code in the subject line of your email for the system to automatically allocate your file to the correct place.

	[ediDocManager (3-letter Reference) (3 or 4 letters Document Type) (Unique Code)]

For example, assuming you have created a SALES PROFILE document type under Config > Reference Files > Document Type with a 3-letter code of PRO, and you want to allocate your documents to the organization ABCSYD, put this in the email subject:

	[ediDocManager ORG PRO ABCSYD]

You can optionally specify a company code to ensure the documents are allocated to the correct record:

	[ediDocManager CRT CRS AASDRA C:DEM]

Where DEM is the company code with which this Client Rates record is associated.

The codes to use in the email subject are the same as if you were using the Allocate eDocs form under Operations > DocManager > Allocate eDocs.", "01-Dec-05 00:00", "badImage1.png\t\nbadImage2.png", string.Join(",", FileImporter.SupportedFileFormats)), emailDef.Body);
				}
			}
			finally
			{
				if (Directory.Exists(TestDirectory))
				{
					Directory.Delete(TestDirectory, true);
				}
			}
		}

		public void TestImportWithBadImageFileInFolder()
		{
			var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.BadImage.png", "badImage.png");
			var testDirectory = Path.GetDirectoryName(testFile);

			var manager = new BatchImportManagerWithTimestamp();
			var logs = new List<string>();
			manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);

			AssertNoExceptionThrown(delegate
			{ manager.ImportFilesAndEmails(testDirectory); });

			AssertMultilineASCIIEquals("LogProgress Events", string.Format(@"
Information - Found 1 DocManager Files to import in {0}:
{1}
Information - Importing file '{1}'
Information - Importing from File system.
Full filename: {1};
Filename only: badImage.png.
Warning - File 'badImage.png' could not be processed as an image due to invalid file content. The image data stream supplied does not contain a valid image format.
Error - Exception of type 'Enterprise.DocumentEngine.Exceptions.CorruptedDocumentException' was thrown.
Information - File '{1}' has been moved to unsuccessful folder.
Information - File '{1}' is processed.
Information - Import Run Completed, processed 1 file(s), ignored 0 file(s) processed by another process.".Trim(), testDirectory, testFile), string.Join("\n", logs.ToArray()));
		}

		public void TestImportWithFileSizeBiggerThanRegistryLimitInFolder()
		{
			SystemDataRegistry.Instance.eDocsMaximumFilesize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			var twoMbDatPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.2MB.dat", "2MB.dat");
			var testDirectory = Path.GetDirectoryName(twoMbDatPath);
			var manager = new BatchImportManagerWithTimestamp();
			var logs = new List<string>();
			manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);

			AssertNoExceptionThrown(delegate
			{ manager.ImportFilesAndEmails(testDirectory); });

			AssertMultilineASCIIEquals("LogProgress Events", string.Format(@"
Information - Found 1 DocManager Files to import in {0}:
{1}
Information - Importing file '{1}'
Information - Importing from File system.
Full filename: {1};
Filename only: {2}.
Error - The file size of '{2}' exceeds the 1MB maximum file size allowed for eDocs.
Information - File '{1}' has been moved to unsuccessful folder.
Information - File '{1}' is processed.
Information - Import Run Completed, processed 1 file(s), ignored 0 file(s) processed by another process.
".Trim(), testDirectory, twoMbDatPath, "2MB.dat"), string.Join("\n", logs.ToArray()));
		}

		public void TestImportFromFolderWithErrorOnSave()
		{
			var originalIsInterative = Globals.IsUserInteractive;
			using (new DisposableAction(() => Globals.IsUserInteractive = originalIsInterative))
			{
				Globals.IsUserInteractive = false;
				var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.MultipageTestDocument.tif", "MultipageTestDocument.tif");
				var testDirectory = Path.GetDirectoryName(testFile);
				BatchImportManagerWithTimestamp manager = new BatchImportManagerWithTimestamp();
				List<string> logs = new List<string>();
				manager.LogProgress +=
					args =>
					{
						logs.Add(args.EventType + " - " + args.Message);

						if (args.Message.StartsWith("Imported file"))
						{
							// Create and save duplicate StorageMain record before it is saved by BatchImportManager.
							StorageMain storageMain = MasterFactory.New<StorageMain>();
							storageMain.SM_ParentFK = ShipmentS00001000.PK;
							storageMain.SM_DB = 1;
							MasterFactory.Save();
						}
					};

				AssertNoExceptionThrown(() => manager.ImportFilesAndEmails(testDirectory));

				string expectedLog = string.Format(
@"Information - Found 1 DocManager Files to import in {0}:
{1}
Information - Importing file '{1}'
Information - Importing from File system.
Full filename: {1};
Filename only: MultipageTestDocument.tif.
Information - Imported file 'MultipageTestDocument.TIF'.
Information - Importing email information.
Caption: eDocs Reload Required
Message: While you were working, the eDocs for this record were modified. The system will now need to merge this information.
Information - File '{1}' is processed.
Information - Import Run Completed, processed 1 file(s), ignored 0 file(s) processed by another process."
					.Trim(), testDirectory, testFile);

				string actualLog = string.Join("\r\n", logs.ToArray());

				AssertMultilineASCIIEquals("LogProgress Events", expectedLog, actualLog);
			}

			string unsuccessfulDirectory = Path.Combine(TestDirectory, BatchImportManager.UnsuccessfulDirectoryName);
			AssertEquals("There should not be a file in the unsuccessful folder, the file with failed import", false, Directory.Exists(unsuccessfulDirectory));
		}

		public void TestImportEmailWithErrorOnSave()
		{
			try
			{
				ZQuery query = new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001002");
				var shipmentS00001002 = (BusinessObject)MasterFactory.LoadTop1<Enterprise.Integration.Freight.ICommonShipment>(query);

				if (shipmentS00001002 == null)
				{
					shipmentS00001002 = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
					shipmentS00001002[JobShipmentSchema.JS_UniqueConsignRef] = "S00001002";
				}

				var email = CreateEmailBySubject(string.Format("[ediDocManager SHP MSC S00001000]"));
				var email2 = CreateEmailBySubject(string.Format("[ediDocManager SHP MSC S00001001]"));
				email2.MI_Body = "Test unhandled ZSaveException";
				var email3 = CreateEmailBySubject(string.Format("[ediDocManager SHP MSC S00001002]"));

				MasterFactory.Save();

				var originalIsInterative = Globals.IsUserInteractive;
				using (new DisposableAction(() => Globals.IsUserInteractive = originalIsInterative))
				{
					Globals.IsUserInteractive = false;

					var manager = new BatchImportManagerWithTimestamp();
					var logs = new List<string>();
					var isDulicateStorageMainSaved = false;
					manager.LogProgress +=
						args =>
						{
							logs.Add(args.EventType + " - " + args.Message);

							if (args.Message.StartsWith("Imported file") && !isDulicateStorageMainSaved) //Only do this once to improve performance
							{
								// Create and save duplicate StorageMain record before it is saved by BatchImportManager.
								var storageMain = MasterFactory.New<StorageMain>();
								storageMain.SM_ParentFK = ShipmentS00001000.PK;
								storageMain.SM_DB = 1;
								MasterFactory.Save();
								isDulicateStorageMainSaved = true;
							}
						};

					AssertNoExceptionThrown(() => manager.ImportFilesAndEmails(TestDirectory));

					var simplifiedErrorLog = "PlaceHolderForComplicatedErrorMessage";
					var expectedLog = FormattableString.Invariant(
	$@"Information - Importing emails in batch of size 3.
Information - Importing email from 'test@test.com' with subject '[ediDocManager SHP MSC S00001000]'.
Information - Imported file 'Email with Image.msg'.
Information - Importing email information.
Caption: eDocs Reload Required
Message: While you were working, the eDocs for this record were modified. The system will now need to merge this information.
Information - Email '[ediDocManager SHP MSC S00001000]' is processed.
Information - Importing email from 'test@test.com' with subject '[ediDocManager SHP MSC S00001001]'.
Information - Imported file 'Email with Image.msg'.
Information - Imported file 'Email with Image.msg'.
Information - Imported file 'Email with Image.msg'.
PlaceHolderForComplicatedErrorMessage
Information - Email '[ediDocManager SHP MSC S00001001]' is processed.
Information - Importing email from 'test@test.com' with subject '[ediDocManager SHP MSC S00001002]'.
Information - Imported file 'Email with Image.msg'.
Information - Email '[ediDocManager SHP MSC S00001002]' is processed.
Information - Import Run Completed, processed 3 email(s), ignored 0 email(s) processed by another process.").Trim();

					AssertContains("We should report the failed mail item", "Error - Error while saving email to database. Email '[ediDocManager SHP MSC S00001001]' could not be imported.", logs[9]);
					logs[9] = simplifiedErrorLog;

					var actualLog = string.Join("\r\n", logs.ToArray());
					AssertEquals("LogProgress Events", expectedLog, actualLog);
				}

				var otherFactory = new BusinessObjectFactory();
				var mailInDb = otherFactory.Load<MailItem>(email.PK);
				var mail2InDb = otherFactory.Load<MailItem>(email2.PK);
				var mail3InDb = otherFactory.Load<MailItem>(email3.PK);

				CombineAssertions(() =>
				{
					AssertEquals("First Email status should be set to PRS", MailStatus.Processed, mailInDb.MI_Status);
					AssertEquals("Second Email status should be set to FAL", MailStatus.Failed, mail2InDb.MI_Status);
					AssertEquals("Third Email status should be set to PRS", MailStatus.Processed, mail3InDb.MI_Status);
				});
			}
			finally
			{
				if (Directory.Exists(TestDirectory))
				{
					Directory.Delete(TestDirectory, true);
				}
			}
		}

		public void TestImportWithMockVirusFileOnEmail()
		{
			using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var email = CreateEmailBySubject(string.Format("[ediDocManager SHP MSC S00001000]"));
				MasterFactory.Save();

				var manager = new BatchImportManagerWithTimestamp();
				var logs = new List<string>();
				manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);

				var moqAmsiContext = new Mock<IAmsiContext>();
				var moqAmsiSession = new Mock<IAmsiSession>();
				moqAmsiSession.Setup(session => session.IsMalware(It.IsAny<byte[]>(), It.IsAny<string>())).Returns(() => true);
				moqAmsiContext.Setup(context => context.CreateSession()).Returns(moqAmsiSession.Object);
				using (ObjectFactory.Substitute(moqAmsiContext.Object))
				{
					AssertNoExceptionThrown(() => manager.ImportFilesAndEmails(""));
				}

				var actual = string.Join("\r\n", logs);
				AssertContains("The file \"Email with Image.msg\" has been detected with virus and therefore cannot be saved or opened.", actual);

				var factoryOne = MasterFactory.GetFactory(1);
				AssertEquals("Should be no documents imported", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			}
		}

		public void TestImportWithMockVirusFileOnFiles()
		{
			using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif", "[SHP MSC S00001000] small.tif");
				var testDirectory = Path.GetDirectoryName(testFile);

				var manager = new BatchImportManagerWithTimestamp();
				var logs = new List<string>();
				manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);

				var moqAmsiContext = new Mock<IAmsiContext>();
				var moqAmsiSession = new Mock<IAmsiSession>();
				moqAmsiSession.Setup(session => session.IsMalware(It.IsAny<byte[]>(), It.IsAny<string>())).Returns(() => true);
				moqAmsiContext.Setup(context => context.CreateSession()).Returns(moqAmsiSession.Object);
				using (ObjectFactory.Substitute(moqAmsiContext.Object))
				{
					AssertNoExceptionThrown(() => manager.ImportFilesAndEmails(testDirectory));
				}

				var actual = string.Join("\r\n", logs);
				AssertContains("The file \"small.tif\" has been detected with virus and therefore cannot be saved or opened.", actual);

				var factoryOne = MasterFactory.GetFactory(1);
				AssertEquals("Should be no documents imported", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			}
		}

		public void TestImportFileWithNoExtension()
		{
			using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var email = CreateEmailBySubject(string.Format("[ediDocManager SHP MSC S00001000]"));
				email.MailAttachments[0].MA_FileName = "test";
				MasterFactory.Save();

				var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif", "[ediDocManager SHP MSC S00001000] small");
				var testDirectory = Path.GetDirectoryName(testFile);

				var manager = new BatchImportManagerWithTimestamp();
				var logs = new List<string>();
				manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);

				var moqAmsiContext = new Mock<IAmsiContext>();
				var moqAmsiSession = new Mock<IAmsiSession>();
				moqAmsiSession.Setup(session => session.IsMalware(It.IsAny<byte[]>(), It.IsAny<string>())).Returns(() => true);
				moqAmsiContext.Setup(context => context.CreateSession()).Returns(moqAmsiSession.Object);
				using (ObjectFactory.Substitute(moqAmsiContext.Object))
				{
					AssertNoExceptionThrown(() => manager.ImportFilesAndEmails(testDirectory));
				}

				var expectedLog = FormattableString.Invariant(
$@"Information - Found 1 DocManager Files to import in {testDirectory}:
{testFile}
Information - Importing file '{testFile}'
Information - Importing from File system.
Full filename: {testFile};
Filename only: [ediDocManager SHP MSC S00001000] small.
Error - File '[ediDocManager SHP MSC S00001000] small' could not be imported. Error message: File extension is missing.
Information - File '{testFile}' has been moved to unsuccessful folder.
Information - File '{testFile}' is processed.
Information - Import Run Completed, processed 1 file(s), ignored 0 file(s) processed by another process.
Information - Importing emails in batch of size 1.
Information - Importing email from 'test@test.com' with subject '[ediDocManager SHP MSC S00001000]'.
Error - File 'test' could not be imported. Error message: File extension is missing.
Error - File 'test' could not be imported. Error message: File extension is missing.
Error - File 'test' could not be imported. Error message: File extension is missing.
Information - Email '[ediDocManager SHP MSC S00001000]' is processed.
Information - Import Run Completed, processed 1 email(s), ignored 0 email(s) processed by another process.");

				var actual = string.Join("\r\n", logs);
				AssertContains(expectedLog, actual);

				var factoryOne = MasterFactory.GetFactory(1);
				AssertEquals("Should be no documents imported", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
				AssertEquals("Email tatus should be marked as failed", MailStatus.Failed, email.MI_Status);
			}
		}

		[DeveloperOnlyTest]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWithBadImageFileThatThrowsExternalException()
		{
			try
			{
				Directory.CreateDirectory(TestDirectory);
				CopyFile(Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"badImageExternalException.tif"), TestDirectory);

				BatchImportManagerWithTimestamp manager = new BatchImportManagerWithTimestamp();
				List<string> logs = new List<string>();
				manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);

				AssertNoExceptionThrown(delegate
				{ manager.ImportFilesAndEmails(TestDirectory); });

				string expected = string.Format(@"Information - Found 1 DocManager Files to import in {0}:
{1}
Information - Importing file '{1}'
Information - Importing from File system.
Full filename: {1};
Filename only: {2}.
Information - Imported file 'badImageExternalException.TIF'.
Information - File '{1}' is processed.
Information - Import Run Completed, processed 1 file(s), ignored 0 file(s) processed by another process.", TestDirectory.Trim(), Path.Combine(TestDirectory, "badImageExternalException.tif"), "badImageExternalException.tif");
				string actual = string.Join("\n", logs.ToArray());
				AssertMultilineASCIIEquals("LogProgress Events", expected.Trim(), actual.Trim());
			}
			finally
			{
				if (Directory.Exists(TestDirectory))
				{
					Directory.Delete(TestDirectory, true);
				}
			}
		}

		public void TestImportWithFiles()
		{
			var testFile1 = MultipageTestDocumentTifPath;
			var testFile2 = SmallGifPath;

			BatchImportManager manager = new BatchImportManager();
			manager.ImportFilesAndEmails(Path.GetDirectoryName(testFile1));

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("Should get two documents in factory one - they've been automatically allocated from the multipage document", 2, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("Should get three documents in factory 0 - two from the multipage document and one from small.gif", 3, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("TestFile 1 should have been deleted because it got successfully imported", false, File.Exists(testFile1));
			AssertEquals("TestFile 2 should have been deleted because it got successfully imported", false, File.Exists(testFile2));
		}

		public void TestUserContextChangeDetection()
		{
			IDisposable tempUserContext = null;
			try
			{
				var shipmentWithWorkflow = MasterFactory.Load<Enterprise.Integration.Forwarding.IForwardingShipment>(ShipmentS00001000.PK) as IWorkflowProvider;
				var milestone = shipmentWithWorkflow.WorkflowItems.Milestones.AddNew();
				milestone.P9_Description = "Trigger template application";
				milestone.TriggerConditions.TriggerEventCode = Events.DocumentImportedCode;
				var action = milestone.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
				action.PQ_FieldName = "<JS_GoodsDescription>";
				action.PQ_FieldValue = "Test";

				GlbCompany companyMEL = MasterFactory.New<GlbCompany>();
				companyMEL.GC_Code = "MEL";
				GlbBranch branchMel = MasterFactory.New<GlbBranch>();
				branchMel.GB_GC = companyMEL.PK;
				MasterFactory.Save();

				var testFile1 = MultipageTestDocumentTifPath;
				var testFile2 = SmallGifPath;

				BatchImportManager manager = new BatchImportManager();

				// Hooking the logging to mess with the user context
				manager.LogProgress += args =>
				{
					if (args.Message.StartsWith("Importing from File system"))
					{
						tempUserContext = Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchMel.PK.ToGuid(), Env.CurrentDepartment.PK);
					}

					if (args.Message.StartsWith("Importing file") && tempUserContext != null)
					{
						tempUserContext.Dispose();
						tempUserContext = null;
					}
				};

				manager.ImportFilesAndEmails(Path.GetDirectoryName(testFile1));
				Assert(ErrorReporter.TotalErrorCount > 0);
				ErrorReporter.Clear();
			}
			finally
			{
				tempUserContext?.Dispose();
			}
		}

		DirectorySecurity GetDirectoryAccessControl(string directoryFullPath)
		{
#if NETFRAMEWORK
			return Directory.GetAccessControl(directoryFullPath);
#else
			DirectoryInfo directoryInfo = new DirectoryInfo(directoryFullPath);
			return directoryInfo.GetAccessControl();
#endif
		}

		void SetDirectoryAccessControl(string directoryFullPath, DirectorySecurity directorySecurity)
		{
#if NETFRAMEWORK
			Directory.SetAccessControl(directoryFullPath, directorySecurity);
#else
			DirectoryInfo directoryInfo = new DirectoryInfo(directoryFullPath);
			directoryInfo.SetAccessControl(directorySecurity);
#endif
		}

		public void TestImportFileWithSecurityGroup()
		{
			var testDirectory = Directory.CreateDirectory(TestDirectory);
			var testDirectorySecurity = GetDirectoryAccessControl(testDirectory.FullName);
			testDirectorySecurity.SetAccessRuleProtection(true, true);

			var allUsers = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
			var accessRule = new FileSystemAccessRule(allUsers, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.NoPropagateInherit, AccessControlType.Allow);
			testDirectorySecurity.AddAccessRule(accessRule);
			SetDirectoryAccessControl(testDirectory.FullName, testDirectorySecurity);

			var currentUser = WindowsIdentity.GetCurrent().User;
			var currentUserAccessRule = GetDirectoryAccessControl(TestDirectory)
				.GetAccessRules(true, true, typeof(NTAccount))
				.OfType<FileSystemAccessRule>()
				.Where(rule => rule.IdentityReference.Value == currentUser.Translate(typeof(NTAccount)).Value);
			if (!currentUserAccessRule.Any())
			{
				currentUserAccessRule.ForEach((FileSystemAccessRule rule) => { testDirectorySecurity.RemoveAccessRule(rule); });
				testDirectorySecurity.SetAccessRuleProtection(true, false);
				SetDirectoryAccessControl(testDirectory.FullName, testDirectorySecurity);
			}

			var deniedAccessRule = new FileSystemAccessRule(currentUser, FileSystemRights.Delete, AccessControlType.Deny);

			var manager = new BatchImportManager();
			try
			{
				Assert(manager.CheckSecuritySettings(testDirectory.FullName));

				testDirectorySecurity.AddAccessRule(deniedAccessRule);
				testDirectorySecurity.SetAccessRuleProtection(true, false);
				SetDirectoryAccessControl(testDirectory.FullName, testDirectorySecurity);

				if (manager.CheckSecuritySettings(testDirectory.FullName))
				{
					var collection = GetDirectoryAccessControl(testDirectory.FullName)
						.GetAccessRules(true, true, typeof(NTAccount))
						.OfType<FileSystemAccessRule>();

					var failedMsg = new StringBuilder();
					failedMsg.AppendLine("Access Collection:");
					collection.ForEach(a => failedMsg.AppendLine($"Identity: {a.IdentityReference.Value}; Rights: {a.FileSystemRights}."));

					var currentIdentity = WindowsIdentity.GetCurrent().User.Translate(typeof(NTAccount)).Value;
					Assert($"Failed assert found. Current identity:{currentIdentity}; {failedMsg}", false);
				}

				Assert(!manager.CheckSecuritySettings(testDirectory.FullName));
			}
			finally
			{
				testDirectorySecurity.RemoveAccessRule(deniedAccessRule);
				if (Directory.Exists(TestDirectory))
				{
					Directory.Delete(TestDirectory, true);
				}
			}
		}

		public void TestImportFileWithFalseSecurityReference()
		{
			FileSystemAccessControlUtilsTestHelper.CreateFolderWithBlockedPermissionsForTest(TestDirectory);

			var manager = new BatchImportManager();
			try
			{
				bool check = true;
				AssertNoExceptionThrown(() => check = manager.CheckSecuritySettings(TestDirectory));
				Assert(!check);
			}
			finally
			{
				if (Directory.Exists(TestDirectory))
				{
					Directory.Delete(TestDirectory, true);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFileWithoutDeleteAuthority()
		{
			var testDirectory = Directory.CreateDirectory(TestDirectory);
			var testDirectorySecurity = GetDirectoryAccessControl(testDirectory.FullName);
			var currentUser = WindowsIdentity.GetCurrent().User;
			var writeRule = new FileSystemAccessRule(currentUser, FileSystemRights.Write, AccessControlType.Deny);
			try
			{
				var testFile1 = CopyFile(Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"MultipageTestDocument.TIF"), "[SHP CIV S0000001] test.tif", TestDirectory);

				testDirectorySecurity.AddAccessRule(writeRule);
				testDirectory.SetAccessControl(testDirectorySecurity);

				var manager = new BatchImportManager();
				var logs = new List<string>();
				manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);
				manager.ImportFilesAndEmails(TestDirectory);

				AssertContains("Log error for permissions accessing to DMI import folder."
					, string.Format(@"DMI Service Task doesn't have modification rights for {0} directory. Please check security settings and run service task again", TestDirectory)
					, string.Join(";", logs));
			}
			finally
			{
				testDirectorySecurity.RemoveAccessRule(writeRule);
				testDirectory.SetAccessControl(testDirectorySecurity);
				if (Directory.Exists(TestDirectory))
				{
					Directory.Delete(TestDirectory, true);
				}
			}
		}

		public void TestImportFileWithOpenFiles()
		{
			var testFile1 = MultipageTestDocumentTifPath;
			var testFile2 = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.gif", "small.gif");

			List<string> logs = new List<string>();
			using (var stream = new FileStream(testFile1, FileMode.Open))
			{
				BatchImportManager manager = new BatchImportManager();
				manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);
				manager.ImportFilesAndEmails(Path.GetDirectoryName(MultipageTestDocumentTifPath));
			}

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("Should get no documents in factory one - the multipage doc was in use and thus was not imported", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals(string.Format("Should get 1 document in factory 0 - one from small.gif only, the multipage doc was in use and thus not imported. Logs: {0}", string.Join("\n", logs.ToArray())), 1, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			StorageDocsUnallocated document = MasterFactory.LoadTop1<StorageDocsUnallocated>(new ZQuery());
			AssertDocumentProperties(document, "small", ZString.Empty, Core.Constants.DocManagerCodes.Unallocated, ZGuid.Empty);
			AssertEquals("TestFile 1 should NOT have been deleted because it was open", true, File.Exists(testFile1));
			AssertEquals("TestFile 2 should have been deleted because it got successfully imported", false, File.Exists(testFile2));
			AssertEquals("I/O Exception when accessing file to import will only be logged in service task logs and it is customer's responsibility to investigate.", string.Empty, ErrorReporter.LastMessageReported.Trim());
			ErrorReporter.Clear();
		}

		public void TestImportFileWhenFileIsInUse()
		{
			using (ManualResetEvent mainThreadManualEvent = new ManualResetEvent(false))
			{
				var smallGifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.gif", "small.gif");
				var testDirectory = Path.GetDirectoryName(smallGifPath);
				using (ManualResetEvent manualEvent = new ManualResetEvent(false))
				{
					ThreadPool.QueueUserWorkItem(new WaitCallback((a) =>
					{
						using (FileStream stream = new FileStream(smallGifPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
						{
							stream.Flush();
							manualEvent.WaitOne();
							stream.Close();
							mainThreadManualEvent.Set();
						}
					}));

					try
					{
						BatchImportManagerWithTimestamp manager = new BatchImportManagerWithTimestamp();
						List<string> logs = new List<string>();
						manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);

						manager.ImportFilesAndEmails(testDirectory);
						AssertEquals("I/O Exception when accessing file to import will only be logged in service task logs and it is customer's responsibility to investigate.", string.Empty, ErrorReporter.LastMessageReported.Trim());
						ErrorReporter.Clear();

						string expected = string.Format(@"Information - Found 1 DocManager Files to import in {0}:
{1}
Information - Importing file '{1}'
Information - Importing from File system.
Full filename: {1};
Filename only: small.gif.
Information - File '{1}' is processed.
Information - Importing skipped files.
Information - Importing file '{1}'
Information - Importing from File system.
Full filename: {1};
Filename only: small.gif.
Information - Importing from File system.
Full filename: {1};
Filename only: small.gif.
Information - Importing from File system.
Full filename: {1};
Filename only: small.gif.
Information - Importing from File system.
Full filename: {1};
Filename only: small.gif.
Information - Importing from File system.
Full filename: {1};
Filename only: small.gif.
Warning - File 'small.gif' could not be imported. Get I/O Exception when accessing file to import: The file is currently in use. Please close the file and try again.
Import file folder: {0}

Information - File '{1}' is processed.
Information - Import Run Completed, processed 1 file(s), ignored 0 file(s) processed by another process.
", testDirectory, smallGifPath);

						string actual = string.Join("\n", logs.ToArray());
						AssertMultilineASCIIEquals("Tried 6 times when file is in use and show error message as warning. ", expected.Trim(), actual.Trim());
					}

					finally
					{
						manualEvent.Set();
						mainThreadManualEvent.WaitOne();
					}
				}
			}
		}

		public void TestImportWithXLSFile()
		{
			var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.xls", "Test.xls");
			var testDirectory = Path.GetDirectoryName(testFile);
			BatchImportManagerWithTimestamp manager = new BatchImportManagerWithTimestamp();
			manager.ImportFilesAndEmails(testDirectory);

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should be no documents - xls shouldn't be imported", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("should be no documents - xls shouldn't be imported", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("TestFile should have been moved to unsuccessful folder - no longer in the import folder", false, File.Exists(testFile));

			string unsuccessfulDirectory = Path.Combine(testDirectory, BatchImportManager.UnsuccessfulDirectoryName);
			string[] filenamesInUnsuccessfulDirectory = Directory.GetFiles(unsuccessfulDirectory);
			AssertEquals("There should be a file in the unsuccessful folder, the file with failed import", 1, filenamesInUnsuccessfulDirectory.Length);
			string unsuccessfulFilename = Path.GetFileName(filenamesInUnsuccessfulDirectory[0]);
			AssertEquals("Filename should start with the same filename as the original doc, but has a timestamp appended to the end of it", "Test02-Jan-05.xls", Path.GetFileName(unsuccessfulFilename));
		}

		public void TestImportWithGIFFile()
		{
			ImportImageFile(SmallGifPath, false);
		}

		public void TestImportWithTIFFile()
		{
			ImportImageFile(resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif"), true);
		}

		void ImportImageFile(string fileToImport, bool shouldConvert)
		{
			BatchImportManagerWithTimestamp manager = new BatchImportManagerWithTimestamp();
			List<string> logs = new List<string>();
			manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);
			manager.ImportFilesAndEmails(Path.GetDirectoryName(fileToImport));

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should be no documents - doesn't have barcode info in file, or allocation info in filename, so shouldn't get allocated", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals(string.Format("should be 1 document - should end up in unallocated documents. Logs: {0}", string.Join("\n", logs.ToArray())), 1, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("TestFile should have been deleted", false, File.Exists(fileToImport));

			StorageDocsUnallocated document = MasterFactory.LoadTop1<StorageDocsUnallocated>(new ZQuery());
			AssertDocumentProperties(document, Path.GetFileNameWithoutExtension(fileToImport), ZString.Empty, Core.Constants.DocManagerCodes.Unallocated, ZGuid.Empty);

			string savedDoc = document.SaveToTempFile();
			using (Image image = Image.FromFile(savedDoc))
			{
				if (shouldConvert)
				{
					AssertEquals("Image should be converted to TIF", ImageFormat.Tiff, image.RawFormat);
				}
				else
				{
					AssertNotEquals("Image should not be converted to TIF", ImageFormat.Tiff, image.RawFormat);
				}
			}
			File.Delete(savedDoc);
		}

		public void TestImportWithUnsupportedFileDuplicateFilenamesDoesntCauseConflict()
		{
			var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.xls", "Test.xls");
			var testDirectory = Path.GetDirectoryName(testFile);

			BatchImportManagerWithTimestamp manager = new BatchImportManagerWithTimestamp();
			manager.ImportFilesAndEmails(testDirectory); // import will fail, no allocation info in the filename of the pdf

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should be no documents - xls shouldn't be imported", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("should be no documents - xls shouldn't be imported", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("TestFile should have been moved to unsuccessful folder - no longer in the import folder", false, File.Exists(testFile));

			string unsuccessfulDirectory = Path.Combine(testDirectory, BatchImportManager.UnsuccessfulDirectoryName);
			string[] filenamesInUnsuccessfulDirectory = Directory.GetFiles(unsuccessfulDirectory);
			AssertEquals("There should be a file in the unsuccessful folder, the file with failed import", 1, filenamesInUnsuccessfulDirectory.Length);
			string unsuccessfulFilename = Path.GetFileName(filenamesInUnsuccessfulDirectory[0]);
			AssertEquals("Filename should start with the same filename as the original doc, but has a timestamp appended to the end of it", "Test02-Jan-05.xls", Path.GetFileName(unsuccessfulFilename));

			testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.xls", "Test.xls");
			manager.ImportFilesAndEmails(testDirectory); // import will fail, no allocation info in the filename of the pdf
			AssertEquals("TestFile should have been moved to unsuccessful folder - no longer in the import folder", false, File.Exists(testFile));
			filenamesInUnsuccessfulDirectory = Directory.GetFiles(unsuccessfulDirectory);
			AssertEquals("There should be two files in the unsuccessful folder", 2, filenamesInUnsuccessfulDirectory.Length);
		}

		class BatchImportManagerWithTimestamp : BatchImportManager
		{
			protected override ZDateTime GetTimestamp()
			{
				return new ZDateTime(2005, 01, 02, 03, 04, 05);
			}
		}

		public void TestImportEmailWithEmbeddedAttachment()
		{
			var email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email";
			email.MI_Subject = "[ediDocManager SHP MSC S00001000]";
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
			email.MI_SendDateTime = new ZDateTime(2005, 12, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			MailAttachment attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "Email with Image.msg";
			attachment.MA_Data = EmailWithImageBytes;
			attachment.MA_MI = email.PK;

			MasterFactory.Save();

			BatchImportManager manager = new BatchImportManager();
			manager.ImportFilesAndEmails(TestDirectory);

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should import one file and allocate because it has all allocation info", 1, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			StorageDocs document = factoryOne.LoadTop1<StorageDocs>(new ZQuery());
			AssertEquals("DocType", "MSC", document.SC_DocType);
			AssertEquals("Parent type", Core.Constants.DocManagerCodes.Shipment, document.ParentMain.SM_Type);
			AssertEquals("ParentFK", ShipmentS00001000.PK, document.ParentMain.SM_ParentFK);

			AssertEquals("should be no documents unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
		}

		public void TestImportEmailWithEmbeddedAttachment_OneOffQuote()
		{
			var company1 = MasterFactory.New(typeof(GlbCompany)) as GlbCompany;
			company1.GC_Code = "CP1";
			company1.GC_Name = "A Company";

			var userBranch1 = MasterFactory.New(typeof(GlbBranch)) as GlbBranch;
			userBranch1.GB_Code = "BP1";
			userBranch1.GB_GC = company1.PK;
			userBranch1.GB_BranchName = "A Branch";

			var company2 = MasterFactory.New(typeof(GlbCompany)) as GlbCompany;
			company2.GC_Code = "CP2";
			company2.GC_Name = "B Company";

			var userBranch2 = MasterFactory.New(typeof(GlbBranch)) as GlbBranch;
			userBranch2.GB_Code = "BP2";
			userBranch2.GB_GC = company2.PK;
			userBranch2.GB_BranchName = "B Branch";

			var quote1 = QuotedBooking.CreateNewQuote(MasterFactory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			quote1.TH_GC = company1.PK;
			quote1.SetQuoteNumber();

			var quote2 = QuotedBooking.CreateNewQuote(MasterFactory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			quote2.TH_GC = company2.PK;
			quote2.SetQuoteNumber();

			var oneOffQuote1 = QuotedBooking.New(quote1.PK, Guid.Empty, MasterFactory);
			oneOffQuote1.ClientPK = MasterFactory.NewWithValidTestData<OrgHeader>().PK;
			oneOffQuote1.Mode = Core.Constants.RateMode.FCL;
			oneOffQuote1.Origin = "AUSYD";
			oneOffQuote1.Destination = "USLAX";

			var oneOffQuote2 = QuotedBooking.New(quote2.PK, Guid.Empty, MasterFactory);
			oneOffQuote2.ClientPK = MasterFactory.NewWithValidTestData<OrgHeader>().PK;
			oneOffQuote2.Mode = Core.Constants.RateMode.FCL;
			oneOffQuote2.Origin = "AUSYD";
			oneOffQuote2.Destination = "USLAX";

			var email1 = MasterFactory.New<MailItem>();
			email1.MI_Body = "This is a test email";
			email1.MI_Subject = $"[ediDocManager QU1 MSC {quote1.TH_QuoteNumber}]";
			email1.MI_From = "test@test.com";
			email1.MI_Direction = MailDirection.Receive;
			email1.MI_Status = MailStatus.Queued;
			email1.MI_LastAttemptDateTime = email1.MI_SendDateTime = email1.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
			MailFilterLocatorTestHelper.SetApplication(email1, MailFilterCodes.DocumentImportManager);

			var email2 = MasterFactory.New<MailItem>();
			email2.MI_Body = "This is a test email";
			email2.MI_Subject = $"[ediDocManager QU1 MSC {quote2.TH_QuoteNumber}]";
			email2.MI_From = "test@test.com";
			email2.MI_Direction = MailDirection.Receive;
			email2.MI_Status = MailStatus.Queued;
			email2.MI_LastAttemptDateTime = email2.MI_SendDateTime = email2.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
			MailFilterLocatorTestHelper.SetApplication(email2, MailFilterCodes.DocumentImportManager);

			var attachment1 = email1.MailAttachments.AddNew();
			attachment1.MA_FileName = "Quote1.msg";
			attachment1.MA_Data = EmailWithImageBytes;
			attachment1.MA_MI = email1.PK;

			var attachment2 = email2.MailAttachments.AddNew();
			attachment2.MA_FileName = "Quote2.msg";
			attachment2.MA_Data = EmailWithImageBytes;
			attachment2.MA_MI = email2.PK;

			MasterFactory.Save();

			using (Globals.SetIsUserInteractiveForTest(false)) // this would simulate the value when service tasks are triggered
			{
				var manager = new BatchImportManager();
				manager.ImportFilesAndEmails(TestDirectory);
			}

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("Should import 2 files and allocate because it has all allocation info", 2, factoryOne.GetDatabaseCount(typeof(StorageDocs)));

			var documents = factoryOne.Load<StorageDocs>(new ZQuery());
			var doc1 = documents.FirstOrDefault(doc => doc.ParentMain.SM_ParentFK == oneOffQuote1.PK);
			var doc2 = documents.FirstOrDefault(doc => doc.ParentMain.SM_ParentFK == oneOffQuote2.PK);

			AssertNotNull(doc1);
			AssertNotNull(doc2);
			AssertEquals("File Name for Quote1", "Quote1", doc1.GetFileNameOnlyWithoutExtension());
			AssertEquals("File Name for Quote2", "Quote2", doc2.GetFileNameOnlyWithoutExtension());

			AssertEquals("Should be no documents unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
		}

		public void TestImportEmailWithCaseInsensitiveSubject()
		{
			var email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email";
			email.MI_Subject = "[Docmanager SHP MSC S00001000]";
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
			email.MI_SendDateTime = new ZDateTime(2005, 12, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			var attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "Email with Image.msg";
			attachment.MA_Data = EmailWithImageBytes;
			attachment.MA_MI = email.PK;

			MasterFactory.Save();

			var manager = new BatchImportManager();
			manager.ImportFilesAndEmails(TestDirectory);

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should import one file and allocate because it has all allocation info", 1, factoryOne.GetDatabaseCount(typeof(StorageDocs)));

			var document = factoryOne.LoadTop1<StorageDocs>(new ZQuery());
			AssertEquals("FileName", "Email with Image", document.SC_FileName);
			AssertEquals("DocType", "MSC", document.SC_DocType);
		}

		public void TestImportEmailWithNoSubjectAndAttachmentWithBarcode()
		{
			TestImportEmailWithNoSubjectAndAttachmentWithBarcodeWithRestrictEmailAllocationOption(false);
		}

		public void TestImportEmailWithNoSubjectAndAttachmentWithBarcodeAndRestrictEmailAllocation()
		{
			TestImportEmailWithNoSubjectAndAttachmentWithBarcodeWithRestrictEmailAllocationOption(true);
		}

		void TestImportEmailWithNoSubjectAndAttachmentWithBarcodeWithRestrictEmailAllocationOption(bool isRestrictEmailAllocationForOrgContacts)
		{
			DocManagerRegistry.Instance.RestrictEmailAllocationForOrgContacts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRestrictEmailAllocationForOrgContacts);

			var acceptedEmails = new CodeDescriptionPairList();
			acceptedEmails.AddPair("*@domain.com", "The test address");

			SystemDataRegistry.Instance.EmailAddressesAllowedForImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, acceptedEmails);

			var email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email for eDocs";
			email.MI_Subject = "";
			email.MI_From = "test@domain.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2018, 3, 1);
			email.MI_SendDateTime = new ZDateTime(2018, 3, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2018, 3, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			var attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "ShipmentScannedDocs.pdf";
			attachment.MA_Data = ShipmentScannedDocsPdfBytes;
			attachment.MA_MI = email.PK;

			MasterFactory.Save();

			var manager = new BatchImportManager();
			manager.ImportFilesAndEmails(TestDirectory);

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should import two files and allocate them because they have all allocation info", 2, factoryOne.GetDatabaseCount(typeof(StorageDocs)));

			var query = new ZQuery();
			query.OrderBy = StorageDocsSchema.SC_DocType.Name;
			var documents = factoryOne.Load<StorageDocs>(query);
			AssertEquals("We should load exactly 2 documents", 2, documents.Length);

			// first doc should be CIV for Shipment S00001001
			var document = documents[0];
			AssertEquals("first doc DocType should be CIV", "CIV", document.SC_DocType);
			AssertEquals("first doc Parent type should be Shipment", Core.Constants.DocManagerCodes.Shipment, document.ParentMain.SM_Type);
			AssertEquals("first doc ParentFK should be Shipment S00001001 as per Barcode", ShipmentS00001001.PK, document.ParentMain.SM_ParentFK);

			// second doc should be DGF for Shipment S00001001
			var document2 = documents[1];
			AssertEquals("second doc DocType should be DGF as per attached doc and Barcode", "DGF", document2.SC_DocType);
			AssertEquals("second doc Parent type should be Shipment", Core.Constants.DocManagerCodes.Shipment, document2.ParentMain.SM_Type);
			AssertEquals("second doc ParentFK should be Shipment S00001001 as per Barcode", ShipmentS00001001.PK, document2.ParentMain.SM_ParentFK);

			AssertEquals("should be no documents unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
		}

		public void TestImportEmailWithRandomSubjectAndAttachmentWithBarcode()
		{
			TestImportEmailWithRandomSubjectAndAttachmentWithBarcodeWithRestrictEmailAllocationOption(false);
		}

		public void TestImportEmailWithRandomSubjectAndAttachmentWithBarcodeAndRestrictEmailAllocation()
		{
			TestImportEmailWithRandomSubjectAndAttachmentWithBarcodeWithRestrictEmailAllocationOption(true);
		}

		void TestImportEmailWithRandomSubjectAndAttachmentWithBarcodeWithRestrictEmailAllocationOption(bool isRestrictEmailAllocationForOrgContacts)
		{
			DocManagerRegistry.Instance.RestrictEmailAllocationForOrgContacts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRestrictEmailAllocationForOrgContacts);

			var acceptedEmails = new CodeDescriptionPairList();
			acceptedEmails.AddPair("*@domain.com", "The test address");

			SystemDataRegistry.Instance.EmailAddressesAllowedForImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, acceptedEmails);
			var email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email for eDocs";
			email.MI_Subject = "test";
			email.MI_From = "test@domain.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2018, 3, 1);
			email.MI_SendDateTime = new ZDateTime(2018, 3, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2018, 3, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			var attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "ShipmentScannedDocs.pdf";
			attachment.MA_Data = ShipmentScannedDocsPdfBytes;
			attachment.MA_MI = email.PK;

			MasterFactory.Save();

			var manager = new BatchImportManager();
			manager.ImportFilesAndEmails(TestDirectory);

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should import two files and allocate them because they have all allocation info", 2, factoryOne.GetDatabaseCount(typeof(StorageDocs)));

			var query = new ZQuery();
			query.OrderBy = StorageDocsSchema.SC_DocType.Name;
			var documents = factoryOne.Load<StorageDocs>(query);
			AssertEquals("We should load exactly 2 documents", 2, documents.Length);

			// first doc should be CIV for Shipment S00001001
			var document = documents[0];
			AssertEquals("first doc DocType should be CIV", "CIV", document.SC_DocType);
			AssertEquals("first doc Parent type should be Shipment", Core.Constants.DocManagerCodes.Shipment, document.ParentMain.SM_Type);
			AssertEquals("first doc ParentFK should be Shipment S00001001 as per Barcode", ShipmentS00001001.PK, document.ParentMain.SM_ParentFK);

			// second doc should be DGF for Shipment S00001001
			var document2 = documents[1];
			AssertEquals("second doc DocType should be DGF as per attached doc and Barcode", "DGF", document2.SC_DocType);
			AssertEquals("second doc Parent type should be Shipment", Core.Constants.DocManagerCodes.Shipment, document2.ParentMain.SM_Type);
			AssertEquals("second doc ParentFK should be Shipment S00001001 as per Barcode", ShipmentS00001001.PK, document2.ParentMain.SM_ParentFK);

			AssertEquals("should be no documents unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
		}

		public void TestImportEmailWithSubjectAndAttachmentWithBarcode()
		{
			var email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email for eDocs";
			email.MI_Subject = "[ediDocManager SHP MSC S00001000]";
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2018, 3, 1);
			email.MI_SendDateTime = new ZDateTime(2018, 3, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2018, 3, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			var attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "ShipmentScannedDocs.pdf";
			attachment.MA_Data = ShipmentScannedDocsPdfBytes;
			attachment.MA_MI = email.PK;

			MasterFactory.Save();

			var manager = new BatchImportManager();
			manager.ImportFilesAndEmails(TestDirectory);

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should import one file and allocate because it has all allocation info", 1, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			var document = factoryOne.LoadTop1<StorageDocs>(new ZQuery());
			AssertEquals("Assert DocType is MSC despite Barcodes insite the document", "MSC", document.SC_DocType);
			AssertEquals("Parent type", Core.Constants.DocManagerCodes.Shipment, document.ParentMain.SM_Type);
			AssertEquals("ParentFK should be the Shipment specified in email subject, not the one read from the Barcode", ShipmentS00001000.PK, document.ParentMain.SM_ParentFK);

			AssertEquals("should be no documents unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
		}

		public void TestImportEmailFromNotAcceptedEmailAddress()
		{
			TestImportEmailFromNotAcceptedEmailAddressWithRestrictEmailAllocationOption(false);
		}

		public void TestImportEmailFromNotAcceptedEmailAddressAndRestrictEmailAllocation()
		{
			TestImportEmailFromNotAcceptedEmailAddressWithRestrictEmailAllocationOption(true);
		}

		void TestImportEmailFromNotAcceptedEmailAddressWithRestrictEmailAllocationOption(bool isRestrictEmailAllocationForOrgContacts)
		{
			DocManagerRegistry.Instance.RestrictEmailAllocationForOrgContacts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRestrictEmailAllocationForOrgContacts);

			var acceptedEmails = new CodeDescriptionPairList();
			acceptedEmails.AddPair("*@domain.com", "The test address");

			SystemDataRegistry.Instance.EmailAddressesAllowedForImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, acceptedEmails);

			var email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email for eDocs";
			email.MI_Subject = "";
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2018, 3, 1);
			email.MI_SendDateTime = new ZDateTime(2018, 3, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2018, 3, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager, false);

			var attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "ShipmentScannedDocs.pdf";
			attachment.MA_Data = ShipmentScannedDocsPdfBytes;
			attachment.MA_MI = email.PK;

			MasterFactory.Save();

			var manager = new BatchImportManager();
			manager.ImportFilesAndEmails(TestDirectory);

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should not have imported anything", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));

			acceptedEmails.AddPair("test@test.com", "The correct test address");

			SystemDataRegistry.Instance.EmailAddressesAllowedForImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, acceptedEmails);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);
			MasterFactory.Save();
			manager = new BatchImportManager();
			manager.ImportFilesAndEmails(TestDirectory);

			AssertEquals("should have now imported 2 documents", 2, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
		}

		public void TestImportEmailFromAcceptedEmailAddress()
		{
			TestImportEmailFromAcceptedEmailAddressWithRestrictEmailAllocationOption(false);
		}

		public void TestImportEmailFromAcceptedEmailAddressAndRestrictEmailAllocation()
		{
			TestImportEmailFromAcceptedEmailAddressWithRestrictEmailAllocationOption(true);
		}

		void TestImportEmailFromAcceptedEmailAddressWithRestrictEmailAllocationOption(bool isRestrictEmailAllocationForOrgContacts)
		{
			DocManagerRegistry.Instance.RestrictEmailAllocationForOrgContacts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRestrictEmailAllocationForOrgContacts);

			var acceptedEmails = new CodeDescriptionPairList();
			acceptedEmails.AddPair("*@test.com", "The test domain");
			acceptedEmails.AddPair("user?@cw1.com", "The test user wildcard");
			acceptedEmails.AddPair("funny.guy@wtg.com", "The test address");
			SystemDataRegistry.Instance.EmailAddressesAllowedForImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, acceptedEmails);

			// email1 - perfect match
			var email1 = MasterFactory.New<MailItem>();
			email1.MI_Body = "This is a test email for eDocs, Regards Funny One";
			email1.MI_Subject = "";
			email1.MI_From = "funny.guy@wtg.com";
			email1.MI_Direction = MailDirection.Receive;
			email1.MI_Status = MailStatus.Queued;
			email1.MI_LastAttemptDateTime = new ZDateTime(2019, 12, 1);
			email1.MI_SendDateTime = new ZDateTime(2019, 12, 1);
			email1.MI_ReceivedDateTime = new ZDateTime(2019, 12, 1);
			MailFilterLocatorTestHelper.SetApplication(email1, MailFilterCodes.DocumentImportManager);

			// email2 - email within quotes
			var email2 = MasterFactory.New<MailItem>();
			email2.MI_Body = "This is a test email for eDocs, Regards, funny guy";
			email2.MI_Subject = "";
			email2.MI_From = "\"Some One Funny\" <funny.guy@wtg.com>";
			email2.MI_Direction = MailDirection.Receive;
			email2.MI_Status = MailStatus.Queued;
			email2.MI_LastAttemptDateTime = new ZDateTime(2019, 12, 2);
			email2.MI_SendDateTime = new ZDateTime(2019, 12, 2);
			email2.MI_ReceivedDateTime = new ZDateTime(2019, 12, 2);
			MailFilterLocatorTestHelper.SetApplication(email2, MailFilterCodes.DocumentImportManager);

			// email3 - match domain
			var email3 = MasterFactory.New<MailItem>();
			email3.MI_Body = "This is a test email for eDocs, Regards, random guy";
			email3.MI_Subject = "";
			email3.MI_From = "random@test.com";
			email3.MI_Direction = MailDirection.Receive;
			email3.MI_Status = MailStatus.Queued;
			email3.MI_LastAttemptDateTime = new ZDateTime(2019, 12, 3);
			email3.MI_SendDateTime = new ZDateTime(2019, 12, 3);
			email3.MI_ReceivedDateTime = new ZDateTime(2019, 12, 3);
			MailFilterLocatorTestHelper.SetApplication(email3, MailFilterCodes.DocumentImportManager);

			// email4 - email with quote but match domain
			var email4 = MasterFactory.New<MailItem>();
			email4.MI_Body = "This is a test email for eDocs, Regards, another guy";
			email4.MI_Subject = "";
			email4.MI_From = "\"Another guy\" <another.guy@test.com>";
			email4.MI_Direction = MailDirection.Receive;
			email4.MI_Status = MailStatus.Queued;
			email4.MI_LastAttemptDateTime = new ZDateTime(2019, 12, 4);
			email4.MI_SendDateTime = new ZDateTime(2019, 12, 4);
			email4.MI_ReceivedDateTime = new ZDateTime(2019, 12, 4);
			MailFilterLocatorTestHelper.SetApplication(email4, MailFilterCodes.DocumentImportManager);

			// email5 - match by single wildcard
			var email5 = MasterFactory.New<MailItem>();
			email5.MI_Body = "This is a test email for eDocs, Regards, user 5";
			email5.MI_Subject = "";
			email5.MI_From = "user5@cw1.com";
			email5.MI_Direction = MailDirection.Receive;
			email5.MI_Status = MailStatus.Queued;
			email5.MI_LastAttemptDateTime = new ZDateTime(2019, 12, 4);
			email5.MI_SendDateTime = new ZDateTime(2019, 12, 4);
			email5.MI_ReceivedDateTime = new ZDateTime(2019, 12, 4);
			MailFilterLocatorTestHelper.SetApplication(email5, MailFilterCodes.DocumentImportManager);

			// emailX - unmatch
			var emailX = MasterFactory.New<MailItem>();
			emailX.MI_Body = "This is a test email for eDocs, Regards, bad guy";
			emailX.MI_Subject = "";
			emailX.MI_From = "\"Another guy\" <bad.guy@test.wtg.com>";
			emailX.MI_Direction = MailDirection.Receive;
			emailX.MI_Status = MailStatus.Queued;
			emailX.MI_LastAttemptDateTime = new ZDateTime(2019, 12, 2);
			emailX.MI_SendDateTime = new ZDateTime(2019, 12, 2);
			emailX.MI_ReceivedDateTime = new ZDateTime(2019, 12, 2);
			MailFilterLocatorTestHelper.SetApplication(emailX, MailFilterCodes.DocumentImportManager, false);

			MasterFactory.Save();

			var manager = new BatchImportManager();
			manager.ImportFilesAndEmails(TestDirectory);

			email1.Reload();
			AssertEquals("Email1 should be processed", MailStatus.Processed, email1.MI_Status);
			email2.Reload();
			AssertEquals("Email2 should be processed", MailStatus.Processed, email2.MI_Status);
			email3.Reload();
			AssertEquals("Email3 should be processed", MailStatus.Processed, email3.MI_Status);
			email4.Reload();
			AssertEquals("Email4 should be processed", MailStatus.Processed, email4.MI_Status);
			email5.Reload();
			AssertEquals("Email5 should be processed", MailStatus.Processed, email5.MI_Status);
			emailX.Reload();
			AssertEquals("EmailX should not be processed", MailStatus.Queued, emailX.MI_Status);
		}

		public void TestImportEmailFromAcceptedEmailAddressWithSingleCharWildcard()
		{
			var acceptedEmails = new CodeDescriptionPairList();
			acceptedEmails.AddPair("user?@cw1.com", "The test user wildcard");
			SystemDataRegistry.Instance.EmailAddressesAllowedForImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, acceptedEmails);
			var email1 = MasterFactory.New<MailItem>();
			email1.MI_Body = "This is a test email for eDocs, Regards, user 1";
			email1.MI_Subject = "";
			email1.MI_From = "user1@cw1.com";
			email1.MI_Direction = MailDirection.Receive;
			email1.MI_Status = MailStatus.Queued;
			email1.MI_LastAttemptDateTime = new ZDateTime(2019, 12, 4);
			email1.MI_SendDateTime = new ZDateTime(2019, 12, 4);
			email1.MI_ReceivedDateTime = new ZDateTime(2019, 12, 4);
			MailFilterLocatorTestHelper.SetApplication(email1, MailFilterCodes.DocumentImportManager);

			var email2 = MasterFactory.New<MailItem>();
			email2.MI_Body = "This is a test email for eDocs, Regards, user 2";
			email2.MI_Subject = "";
			email2.MI_From = "\"Uses TWO\" <user2@cw1.com>";
			email2.MI_Direction = MailDirection.Receive;
			email2.MI_Status = MailStatus.Queued;
			email2.MI_LastAttemptDateTime = new ZDateTime(2019, 12, 4);
			email2.MI_SendDateTime = new ZDateTime(2019, 12, 4);
			email2.MI_ReceivedDateTime = new ZDateTime(2019, 12, 4);
			MailFilterLocatorTestHelper.SetApplication(email2, MailFilterCodes.DocumentImportManager);
			MasterFactory.Save();

			var manager = new BatchImportManager();
			manager.ImportFilesAndEmails(TestDirectory);

			email1.Reload();
			AssertEquals("Email5 should be processed", MailStatus.Processed, email1.MI_Status);
			email2.Reload();
			AssertEquals("Email5 should be processed", MailStatus.Processed, email2.MI_Status);
		}

		public void TestImportEmailWhenNoAcceptedEmailAddressDefined()
		{
			TestImportEmailWhenNoAcceptedEmailAddressDefinedWithRestrictEmailAllocationOption(false);
		}

		public void TestImportEmailWhenNoAcceptedEmailAddressDefinedAndRestrictEmailAllocation()
		{
			TestImportEmailWhenNoAcceptedEmailAddressDefinedWithRestrictEmailAllocationOption(true);
		}

		void TestImportEmailWhenNoAcceptedEmailAddressDefinedWithRestrictEmailAllocationOption(bool isRestrictEmailAllocationForOrgContacts)
		{
			DocManagerRegistry.Instance.RestrictEmailAllocationForOrgContacts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRestrictEmailAllocationForOrgContacts);
			SystemDataRegistry.Instance.EmailAddressesAllowedForImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ReadOnlyCodeDescriptionPairList());

			var email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email for eDocs";
			email.MI_Subject = "";
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2018, 3, 1);
			email.MI_SendDateTime = new ZDateTime(2018, 3, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2018, 3, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager, false);

			var attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "ShipmentScannedDocs.pdf";
			attachment.MA_Data = ShipmentScannedDocsPdfBytes;
			attachment.MA_MI = email.PK;

			MasterFactory.Save();

			var manager = new BatchImportManager();
			manager.ImportFilesAndEmails(TestDirectory);

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should not have imported anything", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
		}

		public void TestImportEmailWithNoSubjectAndIncorrectDocumentGoesToUnallocated()
		{
			TestImportEmailWithNoSubjectAndIncorrectDocumentGoesToUnallocatedWithRestrictEmailAllocationOption(false);
		}

		public void TestImportEmailWithNoSubjectAndIncorrectDocumentGoesToUnallocatedAndRestrictEmailAllocation()
		{
			TestImportEmailWithNoSubjectAndIncorrectDocumentGoesToUnallocatedWithRestrictEmailAllocationOption(true);
		}

		void TestImportEmailWithNoSubjectAndIncorrectDocumentGoesToUnallocatedWithRestrictEmailAllocationOption(bool isRestrictEmailAllocationForOrgContacts)
		{
			DocManagerRegistry.Instance.RestrictEmailAllocationForOrgContacts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRestrictEmailAllocationForOrgContacts);

			var acceptedEmails = new CodeDescriptionPairList();
			acceptedEmails.AddPair("*@test.com", "The test address");

			SystemDataRegistry.Instance.EmailAddressesAllowedForImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, acceptedEmails);

			var email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email for eDocs";
			email.MI_Subject = "";
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2018, 3, 1);
			email.MI_SendDateTime = new ZDateTime(2018, 3, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2018, 3, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			var attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "Sample.PDF";
			attachment.MA_Data = SamplePdfBytes;
			attachment.MA_MI = email.PK;

			MasterFactory.Save();

			var manager = new BatchImportManager();
			manager.ImportFilesAndEmails(TestDirectory);

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("Should not have imported any file", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));

			AssertEquals("There should be one document unallocated", 1, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
		}

		public void TestImportEmailWithXLSFileInAttachment()
		{
			TestImportEmailWithXLSFileInAttachmentWithRestrictEmailAllocationOption(false);
		}

		public void TestImportEmailWithXLSFileInAttachmentAndRestrictEmailAllocation()
		{
			TestImportEmailWithXLSFileInAttachmentWithRestrictEmailAllocationOption(true);
		}

		void TestImportEmailWithXLSFileInAttachmentWithRestrictEmailAllocationOption(bool isRestrictEmailAllocationForOrgContacts)
		{
			DocManagerRegistry.Instance.RestrictEmailAllocationForOrgContacts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRestrictEmailAllocationForOrgContacts);

			var acceptedEmails = new CodeDescriptionPairList();
			acceptedEmails.AddPair("*@test.com", "The test address");

			SystemDataRegistry.Instance.EmailAddressesAllowedForImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, acceptedEmails);
			var email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email for eDocs";
			email.MI_Subject = "";
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2018, 3, 1);
			email.MI_SendDateTime = new ZDateTime(2018, 3, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2018, 3, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			var attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "Test.xls";
			attachment.MA_Data = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.xls");
			attachment.MA_MI = email.PK;

			MasterFactory.Save();

			var manager = new BatchImportManager();
			manager.ImportFilesAndEmails(TestDirectory);

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should not import file", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
		}

		public void TestImportEmailsWithAttachmentFileNameHasSquareBrackets()
		{
			var email = MasterFactory.New<MailItem>();
			email.MI_Body = "Jerry test email attachment file";
			email.MI_Subject = "[ediDocManager SHP MSC S00001000]";
			email.MI_From = "jerry@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2022, 12, 19);
			email.MI_SendDateTime = new ZDateTime(2022, 12, 19);
			email.MI_ReceivedDateTime = new ZDateTime(2022, 12, 19);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			var attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "Email with [].msg";//invalid char '/' in file name
			attachment.MA_Data = EmailWithImageBytes;
			attachment.MA_MI = email.PK;

			MasterFactory.Save();

			AssertEquals(0, (ShipmentS00001000 as IDocManagerSupport).DocManagerInfo.AllEDocs.Count);

			var manager = new BatchImportManager();
			manager.ImportFilesAndEmails(TestDirectory);

			var shipment = MasterFactory.Load<Enterprise.Integration.Forwarding.IForwardingShipment>(ShipmentS00001000.PK);
			AssertEquals(1, (shipment as IDocManagerSupport).DocManagerInfo.AllEDocs.Count);
			AssertEquals("Email with [].msg", (shipment as IDocManagerSupport).DocManagerInfo.AllEDocs[0].FileName);
		}

		public void TestImportEmailsWithIllegalChar()
		{
			var email1 = MasterFactory.New<MailItem>();
			email1.MI_Body = "This is a test email";
			email1.MI_Subject = "[ediDocManager SHP MSC S00001000]";
			email1.MI_From = "test@test.com";
			email1.MI_Direction = MailDirection.Receive;
			email1.MI_Status = MailStatus.Queued;
			email1.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
			email1.MI_SendDateTime = new ZDateTime(2005, 12, 1);
			email1.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
			MailFilterLocatorTestHelper.SetApplication(email1, MailFilterCodes.DocumentImportManager);

			var attachment1 = email1.MailAttachments.AddNew();
			attachment1.MA_FileName = "Email with <> Image.msg";//invalid char '/' in file name
			attachment1.MA_Data = EmailWithImageBytes;
			attachment1.MA_MI = email1.PK;

			var attachment2 = email1.MailAttachments.AddNew();
			attachment2.MA_FileName = "Email with Image.msg";//valid file name
			attachment2.MA_Data = EmailWithImageBytes;
			attachment2.MA_MI = email1.PK;

			var email2 = MasterFactory.New<MailItem>();
			email2.MI_Body = "This is a test email";
			email2.MI_Subject = "[ediDocManager SHP MSC S00001000]";
			email2.MI_From = "test@test.com";
			email2.MI_Direction = MailDirection.Receive;
			email2.MI_Status = MailStatus.Queued;
			email2.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
			email2.MI_SendDateTime = new ZDateTime(2005, 12, 1);
			email2.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
			MailFilterLocatorTestHelper.SetApplication(email2, MailFilterCodes.DocumentImportManager);

			var attachment3 = email1.MailAttachments.AddNew();
			attachment3.MA_FileName = "Email with Image.msg";//valid file name
			attachment3.MA_Data = EmailWithImageBytes;
			attachment3.MA_MI = email2.PK;

			MasterFactory.Save();

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals(0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));

			BatchImportManager manager = new BatchImportManager();
			List<string> logs = new List<string>();
			manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);
			manager.ImportFilesAndEmails(TestDirectory);

			var emailWithImageIsProcessedFirst = @"Information - Importing emails in batch of size 2.
Information - Importing email from 'test@test.com' with subject '[ediDocManager SHP MSC S00001000]'.
Information - Imported file 'Email with __ Image.msg'.
Information - Imported file 'Email with Image.msg'.
Information - Email '[ediDocManager SHP MSC S00001000]' is processed.
Information - Importing email from 'test@test.com' with subject '[ediDocManager SHP MSC S00001000]'.
Information - Imported file 'Email with Image.msg'.
Information - Email '[ediDocManager SHP MSC S00001000]' is processed.
Information - Import Run Completed, processed 2 email(s), ignored 0 email(s) processed by another process.";

			var emailWith__ImageIsProcessedFirst = @"Information - Importing emails in batch of size 2.
Information - Importing email from 'test@test.com' with subject '[ediDocManager SHP MSC S00001000]'.
Information - Imported file 'Email with __ Image.msg'.
Information - Imported file 'Email with Image.msg'.
Information - Email '[ediDocManager SHP MSC S00001000]' is processed.
Information - Importing email from 'test@test.com' with subject '[ediDocManager SHP MSC S00001000]'.
Information - Imported file 'Email with Image.msg'.
Information - Email '[ediDocManager SHP MSC S00001000]' is processed.
Information - Import Run Completed, processed 2 email(s), ignored 0 email(s) processed by another process.";

			var actual = string.Join("\r\n", logs);
			AssertEquals("LogProgress Events.", actual == emailWithImageIsProcessedFirst ? emailWithImageIsProcessedFirst : emailWith__ImageIsProcessedFirst, actual);

			AssertEquals("should import three files and allocate", 3, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("should be no documents unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
		}

		MailItem CreateEmailBySubject(string subject)
		{
			var email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email";
			email.MI_Subject = subject;
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
			email.MI_SendDateTime = new ZDateTime(2005, 12, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			var attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "Email with Image.msg";
			attachment.MA_Data = EmailWithImageBytes;
			attachment.MA_MI = email.PK;

			return email;
		}

		public void TestImportEmailWithCompanyBranchDepartmentSpecificDocument()
		{
			var branch = Factory.Load<GlbBranch>(new ZQuery())[0];
			var company = Factory.Load<GlbCompany>(branch.GB_GC);
			var department = Factory.Load<GlbDepartment>(new ZQuery())[0];

			var email = CreateEmailBySubject(string.Format("[ediDocManager SHP MSC S00001000 C:{0} B:{1} D:{2}]", company.GC_Code, branch.GB_Code, department.GE_Code));
			MasterFactory.Save();

			var manager = new BatchImportManager();
			manager.ImportFilesAndEmails(TestDirectory);

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should import one file and allocate because it has all allocation info", 1, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			StorageDocs document = factoryOne.LoadTop1<StorageDocs>(new ZQuery());
			AssertEquals("DocType", "MSC", document.SC_DocType);
			AssertEquals("Parent type", Core.Constants.DocManagerCodes.Shipment, document.ParentMain.SM_Type);
			AssertEquals("ParentFK", ShipmentS00001000.PK, document.ParentMain.SM_ParentFK);
			AssertEquals("Company PK", company.PK, document.SC_GC_Company);
			AssertEquals("Branch PK", branch.PK, document.SC_GB_Branch);
			AssertEquals("Department PK", department.PK, document.SC_GE_Department);

			AssertEquals("should be no documents unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
		}

		public void TestImportEmailWithDuplicatedJobs()
		{
			var email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email";
			email.MI_Subject = string.Format("[ediDocManager CNT MSC 00001]");
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
			email.MI_SendDateTime = new ZDateTime(2005, 12, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			var container1 = MasterFactory.NewWithValidTestData<CommonContainer>();
			container1.JC_ContainerNum = "00001";

			var container2 = MasterFactory.NewWithValidTestData<CommonContainer>();
			container2.JC_ContainerNum = "00001";

			MasterFactory.Save();

			var manager = new BatchImportManager();
			List<string> logs = new List<string>();
			manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);
			manager.ImportFilesAndEmails(TestDirectory);

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should import no file since it has incorrect allocation info", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));

			AssertEquals("LogProgress Events", $@"Information - Importing emails in batch of size 1.
Information - Importing email from 'test@test.com' with subject '[ediDocManager CNT MSC 00001]'.
Error - Failed to allocate document. Error message: The unique ID 00001 you have supplied for Reference Type CNT exists in multiple records.
Error - Failed to allocate document. Error message: The unique ID 00001 you have supplied for Reference Type CNT exists in multiple records.
Error - Failed to allocate document. Error message: The unique ID 00001 you have supplied for Reference Type CNT exists in multiple records.
Information - Email '[ediDocManager CNT MSC 00001]' is processed.
Information - Import Run Completed, processed 1 email(s), ignored 0 email(s) processed by another process.", string.Join("\r\n", logs));
			AssertEquals(MailStatus.Failed, email.MI_Status);
		}

		public void TestImportEmailWithCompanyBranchDepartmentSpecificDocument_IncorrectSubject()
		{
			var branch = Factory.Load<GlbBranch>(new ZQuery())[0];
			var company = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, branch.GB_GC))[0];

			var email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email";
			email.MI_Subject = string.Format("[ediDocManager SHP MSC S00001000 C:{0} B:{1} D:{2}]", company.GC_Code, branch.GB_Code, "@@@");
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
			email.MI_SendDateTime = new ZDateTime(2005, 12, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			var attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "Email with Image.msg";
			attachment.MA_Data = EmailWithImageBytes;
			attachment.MA_MI = email.PK;

			MasterFactory.Save();

			var manager = new BatchImportManager();
			List<string> logs = new List<string>();
			manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);
			manager.ImportFilesAndEmails(TestDirectory);

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should import no file since it has incorrect allocation info", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));

			AssertEquals("LogProgress Events", $@"
Information - Importing emails in batch of size 1.
Information - Importing email from 'test@test.com' with subject '[ediDocManager SHP MSC S00001000 C:DEM B:BNE D:@@@]'.
Error - Failed to allocate document. Error message: The visible branch code 'BNE' does not belong to the visible company specified in email subject.
The visible department code '@@@' specified in email subject is invalid.
Error - Failed to allocate document. Error message: The visible branch code 'BNE' does not belong to the visible company specified in email subject.
The visible department code '@@@' specified in email subject is invalid.
Error - Failed to allocate document. Error message: The visible branch code 'BNE' does not belong to the visible company specified in email subject.
The visible department code '@@@' specified in email subject is invalid.
Information - Email '[ediDocManager SHP MSC S00001000 C:DEM B:BNE D:@@@]' is processed.
Information - Import Run Completed, processed 1 email(s), ignored 0 email(s) processed by another process.".Trim(), string.Join("\r\n", logs));
			AssertEquals(MailStatus.Failed, email.MI_Status);
		}

		public void TestImportWithFileWithAllocationInformation()
		{
			var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif", "[SHP CIV S00001000] small.tif");

			BatchImportManager manager = new BatchImportManager();
			manager.ImportFilesAndEmails(Path.GetDirectoryName(testFile));

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should import one file and allocate because it has all allocation info", 1, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			StorageDocs document = factoryOne.LoadTop1<StorageDocs>(new ZQuery());
			AssertEquals("DocType", "CIV", document.SC_DocType);
			AssertEquals("Parent type", Core.Constants.DocManagerCodes.Shipment, document.ParentMain.SM_Type);
			AssertEquals("ParentFK", ShipmentS00001000.PK, document.ParentMain.SM_ParentFK);

			AssertEquals("should be no documents unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("TestFile should have been deleted", false, File.Exists(testFile));
		}

		public void TestImportWithFileWithCompanySpecificAllocationInformation()
		{
			var branch = Factory.Load<GlbBranch>(new ZQuery())[0];
			var company = Factory.Load<GlbCompany>(branch.GB_GC);
			var department = Factory.Load<GlbDepartment>(new ZQuery())[0];

			var fileName = string.Format("[SHP MSC S00001000 C@{0} B@{1} D@{2}] small.tif", company.GC_Code, branch.GB_Code, department.GE_Code);
			var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif", fileName);

			BatchImportManager manager = new BatchImportManager();
			manager.ImportFilesAndEmails(Path.GetDirectoryName(testFile));

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should import one file and allocate because it has all allocation info", 1, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			StorageDocs document = factoryOne.LoadTop1<StorageDocs>(new ZQuery());
			AssertEquals("DocType", "MSC", document.SC_DocType);
			AssertEquals("Parent type", Core.Constants.DocManagerCodes.Shipment, document.ParentMain.SM_Type);
			AssertEquals("ParentFK", ShipmentS00001000.PK, document.ParentMain.SM_ParentFK);
			AssertEquals("Company PK", company.PK, document.SC_GC_Company);
			AssertEquals("Branch PK", branch.PK, document.SC_GB_Branch);
			AssertEquals("Department PK", department.PK, document.SC_GE_Department);

			AssertEquals("should be no documents unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("TestFile should have been deleted", false, File.Exists(testFile));
		}

		public void TestImportWithFileWithCompanySpecificAllocationInformation_IncorrectSubject()
		{
			var branch = Factory.Load<GlbBranch>(new ZQuery())[0];
			var company = Factory.Load<GlbCompany>(branch.GB_GC);

			var fileName = string.Format("[SHP MSC S00001000 C@{0} B@{1} D@{2}] small.tif", company.GC_Code, branch.GB_Code, "###");
			var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif", fileName);
			var testDirectory = Path.GetDirectoryName(testFile);

			BatchImportManager manager = new BatchImportManager();
			List<string> logs = new List<string>();
			manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);
			manager.ImportFilesAndEmails(testDirectory);

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should import no file since it has incorrect allocation info", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("TestFile should not been deleted", false, File.Exists(testFile));

			AssertEquals("LogProgress Events", $@"Information - Found 1 DocManager Files to import in {testDirectory}:
{testFile}
Information - Importing file '{testFile}'
Information - Importing from File system.
Full filename: {testFile};
Filename only: {fileName}.
Error - File '{fileName}' could not be imported. The visible department code '###' specified in email subject is invalid.
Information - File '{testFile}' has been moved to unsuccessful folder.
Information - File '{testFile}' is processed.
Information - Import Run Completed, processed 1 file(s), ignored 0 file(s) processed by another process.", string.Join("\r\n", logs));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWithFileWithAllocationInformationInBarcodesAndWrongInFileName()
		{
			try
			{
				Directory.CreateDirectory(TestDirectory);
				string testFile = CopyFile(Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"MultipageWithShipmentBarcodes.tif"), "[muhaha] MultipageWithShipmentBarcodes.tif", TestDirectory);

				BatchImportManager manager = new BatchImportManager();
				manager.ImportFilesAndEmails(TestDirectory);

				NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
				AssertEquals("should import one file and allocate because it has all allocation info in barcodes", 1, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
				StorageDocs doc = factoryOne.LoadTop1<StorageDocs>(new ZQuery());
				AssertEquals("Parent type", Core.Constants.DocManagerCodes.Shipment, doc.ParentMain.SM_Type);
				AssertEquals("ParentFK", ShipmentS00001000.PK, doc.ParentMain.SM_ParentFK);

				AssertEquals("should be no documents unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
				AssertEquals("TestFile should have been deleted", false, File.Exists(testFile));
			}
			finally
			{
				if (Directory.Exists(TestDirectory))
				{
					Directory.Delete(TestDirectory, true);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWithFileWithAllocationInformationInBarcodesAndJobFromFilenameNotExists()
		{
			try
			{
				Directory.CreateDirectory(TestDirectory);
				string testFile = CopyFile(Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"MultipageWithShipmentBarcodes.tif"), "[SHP CIV S00001999] MultipageWithShipmentBarcodes.tif", TestDirectory);

				BatchImportManager manager = new BatchImportManager();
				manager.ImportFilesAndEmails(TestDirectory);

				NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
				AssertEquals("should import one file and allocate because it has all allocation info in barcodes", 1, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
				StorageDocs doc = factoryOne.LoadTop1<StorageDocs>(new ZQuery());
				AssertEquals("Parent type", Core.Constants.DocManagerCodes.Shipment, doc.ParentMain.SM_Type);
				AssertEquals("ParentFK", ShipmentS00001000.PK, doc.ParentMain.SM_ParentFK);

				AssertEquals("should be no documents unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
				AssertEquals("TestFile should have been deleted", false, File.Exists(testFile));
			}
			finally
			{
				if (Directory.Exists(TestDirectory))
				{
					Directory.Delete(TestDirectory, true);
				}
			}
		}

		public void TestImportWithFileWithExtraSpaceInAllocationInformation()
		{
			var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif", "[SHP CIV ] small.tif");
			BatchImportManager manager = new BatchImportManager();
			manager.ImportFilesAndEmails(Path.GetDirectoryName(testFile));

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should import one file but unallocated because not all information is there", 1, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			StorageDocs document = MasterFactory.LoadTop1<StorageDocs>(new ZQuery());
			AssertDocumentProperties(document, "[SHP CIV ] small", "CIV", Core.Constants.DocManagerCodes.Shipment, ZGuid.Empty);
			AssertEquals("should be no documents allocated", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("TestFile should have been deleted", false, File.Exists(testFile));
		}

		public void TestImportWithFileWithExtraSpaceInAllocationInformationAtStart()
		{
			var collection = new CodeSelectionCollection(SystemDataRegistry.Instance.DocumentTypesRestrictedListProvider);
			collection.AddNew().Code = "CIV";

			using (SystemDataRegistry.Instance.DocumentTypesRestrictedForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif", "[ SHP CIV ] small.tif");
				BatchImportManager manager = new BatchImportManager();
				manager.ImportFilesAndEmails(Path.GetDirectoryName(testFile));

				NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
				AssertEquals("should be denied due to registry setting", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
				AssertEquals("TestFile should have been deleted", false, File.Exists(testFile));
			}
		}

		public void TestImportWithFileWithExtraSpaceInAllocationInformationAtStart_DocumentTypeRestricted()
		{
			var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif", "[ SHP CIV ] small.tif");
			BatchImportManager manager = new BatchImportManager();
			manager.ImportFilesAndEmails(Path.GetDirectoryName(testFile));

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should import one file but unallocated because not all information is there", 1, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			StorageDocs document = MasterFactory.LoadTop1<StorageDocs>(new ZQuery());
			AssertDocumentProperties(document, "[ SHP CIV ] small", "CIV", Core.Constants.DocManagerCodes.Shipment, ZGuid.Empty);
			AssertEquals("should be no documents allocated", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("TestFile should have been deleted", false, File.Exists(testFile));
		}

		public void TestImportFileWithOnlyBarcodeSheets()
		{
			var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.TwoBarcodes.tif");
			var testDirectory = Path.GetDirectoryName(testFile);

			BatchImportManager manager = new BatchImportManager();
			manager.ImportFilesAndEmails(testDirectory);

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should have no documents added in factory one", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("should have no documents added in main factory", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			string unsuccessfulDirectory = Path.Combine(testDirectory, BatchImportManager.UnsuccessfulDirectoryName);
			AssertEquals("TestFile should have been moved to unsuccessful folder - no longer in the import folder", false, File.Exists(testFile));
			string[] filenamesInUnsuccessfulDirectory = Directory.GetFiles(unsuccessfulDirectory);
			AssertEquals("There should be a new file in the unsuccessful folder", 1, filenamesInUnsuccessfulDirectory.Length);

			string fileNameOnly = Path.GetFileNameWithoutExtension(testFile);
			Assert("The filename should exist with", Path.GetFileNameWithoutExtension(filenamesInUnsuccessfulDirectory[0]).Contains(fileNameOnly));
		}

		public void TestImportWithEmailsWithIncorrectSubject()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "test@test.com";
			staff.GS_FullName = "Test Person";

			MailItem email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email";
			email.MI_Subject = "subject without keyword";
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
			email.MI_SendDateTime = new ZDateTime(2005, 12, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager, false);

			MailAttachment attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "test.tif";
			attachment.MA_Data = SmallGifBytes;
			attachment.MA_MI = email.PK;

			MasterFactory.Save();

			BatchImportManager importManager = new BatchImportManager();
			importManager.ImportFilesAndEmails(TestDirectory);

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should be no documents - email subject wasn't correct", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("should be no documents - email subject wasn't correct", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			email.MI_Subject = "[ediDocManager]";
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);
			MasterFactory.Save();
			importManager.ImportFilesAndEmails(TestDirectory);
			AssertEquals("should be no documents - not allocated", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("should be 1 document - unallocated", 1, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			StorageDocsUnallocated document = MasterFactory.LoadTop1<StorageDocsUnallocated>(new ZQuery());
			AssertDocumentProperties(document, "test", ZString.Empty, Core.Constants.DocManagerCodes.Unallocated, ZGuid.Empty);

			email.Reload();
			AssertEquals("Email's status should now be processed", MailStatus.Processed, email.MI_Status);
		}

		public void TestImportWithEmailsWithProcessedStatus()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "test@test.com";
			staff.GS_FullName = "Test Person";

			MailItem email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email";
			email.MI_Subject = "[ediDocManager]";
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Processed;
			email.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
			email.MI_SendDateTime = new ZDateTime(2005, 12, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager, false);

			MailAttachment attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "test.tif";
			attachment.MA_Data = SmallGifBytes;
			attachment.MA_MI = email.PK;

			MasterFactory.Save();

			BatchImportManager importManager = new BatchImportManager();
			importManager.ImportFilesAndEmails(TestDirectory);

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should be no documents - email was processed already", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("should be no documents - email was processed already", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			email.MI_Status = MailStatus.Queued;
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);
			MasterFactory.Save();
			importManager.ImportFilesAndEmails(TestDirectory);
			AssertEquals("should be no documents - not allocated", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("should be 1 document - unallocated", 1, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			StorageDocsUnallocated document = MasterFactory.LoadTop1<StorageDocsUnallocated>(new ZQuery());
			AssertDocumentProperties(document, "test", ZString.Empty, Core.Constants.DocManagerCodes.Unallocated, ZGuid.Empty);

			email.Reload();
			AssertEquals("Email's status should now be processed", MailStatus.Processed, email.MI_Status);
		}

		public void TestImportWithEmailsWithIncorrectDirection()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "test@test.com";
			staff.GS_FullName = "Test Person";

			MailItem email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email";
			email.MI_Subject = "[ediDocManager]";
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Transmit;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
			email.MI_SendDateTime = new ZDateTime(2005, 12, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager, false);

			MailAttachment attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "test.tif";
			attachment.MA_Data = SmallGifBytes;
			attachment.MA_MI = email.PK;

			MasterFactory.Save();

			BatchImportManager importManager = new BatchImportManager();
			importManager.ImportFilesAndEmails(TestDirectory);

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should be no documents - email is status of transmitted not received", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("should be no documents - email is status of  transmitted not received", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			email.MI_Direction = MailDirection.Receive;
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);
			MasterFactory.Save();
			importManager.ImportFilesAndEmails(TestDirectory);
			AssertEquals("should be no documents - not allocated", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("should be 1 document - unallocated", 1, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			StorageDocsUnallocated document = MasterFactory.LoadTop1<StorageDocsUnallocated>(new ZQuery());
			AssertDocumentProperties(document, "test", ZString.Empty, Core.Constants.DocManagerCodes.Unallocated, ZGuid.Empty);

			email.Reload();
			AssertEquals("Email's status should now be processed", MailStatus.Processed, email.MI_Status);
		}

		public void TestImportWithEmailsWithMultipleAttachments()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "test@test.com";
			staff.GS_FullName = "Test Person";

			MailItem email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email";
			email.MI_Subject = "[ediDocManager]";
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
			email.MI_SendDateTime = new ZDateTime(2005, 12, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			MailAttachment attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "test.tif";
			attachment.MA_Data = SmallGifBytes;
			attachment.MA_MI = email.PK;

			MailAttachment attachment2 = email.MailAttachments.AddNew();
			attachment2.MA_FileName = "test2.tif";
			attachment2.MA_Data = SmallGifBytes;
			attachment2.MA_MI = email.PK;

			MasterFactory.Save();

			BatchImportManager importManager = new BatchImportManager();
			importManager.ImportFilesAndEmails(TestDirectory);

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should be no documents - documents wouldnt have been allocated", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("should be 2 documents - one for each attachment", 2, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			var documents = MasterFactory.Load<StorageDocsUnallocated>(new ZQuery()).OrderBy(doc => doc.SC_FileName).ToList();
			AssertEquals("There should be 2 unallocated documents", 2, documents.Count);
			AssertDocumentProperties(documents[0], "test", ZString.Empty, Core.Constants.DocManagerCodes.Unallocated, ZGuid.Empty);
			AssertDocumentProperties(documents[1], "test2", ZString.Empty, Core.Constants.DocManagerCodes.Unallocated, ZGuid.Empty);

			email.Reload();
			AssertEquals("Email's status should now be processed", MailStatus.Processed, email.MI_Status);
		}

		public void TestImportWithEmailsWithAttachmentsSizeBiggerThanRegistryLimit()
		{
			SystemDataRegistry.Instance.eDocsMaximumFilesize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			try
			{
				GlbStaff staff = Factory.New<GlbStaff>();
				staff.GS_Code = "KXU";
				staff.GS_EmailAddress = "test@test.com";
				staff.GS_FullName = "Test Person";

				MailItem email = MasterFactory.New<MailItem>();
				email.MI_Body = "This is a test email";
				email.MI_Subject = "[ediDocManager]";
				email.MI_From = "test@test.com";
				email.MI_Direction = MailDirection.Receive;
				email.MI_Status = MailStatus.Queued;
				email.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
				email.MI_SendDateTime = new ZDateTime(2005, 12, 1);
				email.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
				MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

				MailAttachment attachment = email.MailAttachments.AddNew();
				attachment.MA_FileName = "test.dat";
				attachment.MA_Data = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.2MB.dat");
				attachment.MA_MI = email.PK;

				MasterFactory.Save();

				BatchImportManager importManager = new BatchImportManager();
				List<string> logs = new List<string>();
				importManager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);

				AssertNoExceptionThrown(delegate
				{ importManager.ImportFilesAndEmails(TestDirectory); });

				email.Reload();
				AssertEquals("Email's status should now be marked as failed", MailStatus.Failed, email.MI_Status);

				AssertMultilineASCIIEquals("LogProgress Events", @"
Information - Importing emails in batch of size 1.
Information - Importing email from 'test@test.com' with subject '[ediDocManager]'.
Error - The file size of 'test.dat' exceeds the 1MB maximum file size allowed for eDocs.
Error - The file size of 'test.dat' exceeds the 1MB maximum file size allowed for eDocs.
Error - The file size of 'test.dat' exceeds the 1MB maximum file size allowed for eDocs.
Information - Email '[ediDocManager]' is processed.
Information - Import Run Completed, processed 1 email(s), ignored 0 email(s) processed by another process.".Trim(), string.Join("\n", logs.ToArray()));
			}
			finally
			{
				if (Directory.Exists(TestDirectory))
				{
					Directory.Delete(TestDirectory, true);
				}
			}
		}

		public void TestImportConcurrencyError_Email_SM_DB()
		{
			// Create SD002 if not existed
			if (!DBHelper.DatabaseExists(2))
			{
				_ = DBHelper.CreateDatabase(2);
			}

			MasterFactory.RefreshEnabled = false;
			MasterFactory.FactoryForEverythingExceptEDocs.RefreshEnabled = false;

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "test@test.com";
			staff.GS_FullName = "Test Person";

			var email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email";
			email.MI_Subject = "[ediDocManager SHP CIV S00001000]";
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
			email.MI_SendDateTime = new ZDateTime(2005, 12, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			var attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "[SHP PKL S00001000] test.tif";
			attachment.MA_Data = SmallTifBytes;
			attachment.MA_MI = email.PK;

			// Create and save StorageMain record so it already exists
			var storageMain = MasterFactory.New<StorageMain>();
			storageMain.SM_ParentFK = ShipmentS00001000.PK;
			storageMain.SM_DB = 2;
			storageMain.SM_CD1 = 2;
			var storageDoc = storageMain.Documents.AddNew();
			storageDoc.SC_SystemLastEditTimeUtc = ZDateTime.Now;

			MasterFactory.Save();

			var manager = new BatchImportManagerWithTimestamp();
			var logs = new List<string>();
			manager.LogProgress +=
				args =>
				{
					logs.Add(args.EventType + " - " + args.Message);

					if (args.Message.StartsWith("Imported file"))
					{
						//modify database values of storage main
						var storageMain2 = new DbBackendDocumentFactory(new BusinessObjectFactory()).LoadTop1<StorageMain>(
						new ZQuery(StorageMainSchema.SM_ParentFK, ShipmentS00001000.PK));
						storageMain2.SM_DB = 1;
						var storageDoc2 = storageMain2.Documents.AddNew();
						storageDoc.SC_SystemLastEditTimeUtc = ZDateTime.Now.AddHours(1);
						storageMain2.Factory.RefreshEnabled = false;
						storageMain2.MasterFactory.RefreshEnabled = false;
						storageMain2.MasterFactory.Save();
						storageMain2.Factory.Save();
						AssertEquals(2, storageMain.SM_DB);
						AssertEquals(1, storageMain2.SM_DB);
						AssertEquals(1, new DbBackendDocumentFactory(new BusinessObjectFactory()).LoadTop1<StorageMain>(
						new ZQuery(StorageMainSchema.SM_ParentFK, ShipmentS00001000.PK)).SM_DB);
					}
				};

			AssertNoExceptionThrown(() => manager.ImportFilesAndEmails(TestDirectory));

			AssertEquals("should be 0 documents - documents shouldn't be left unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should be 1 documents in factory one - documents have been allocated", 1, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
		}

		public void TestImportConcurrencyError_File_SM_ParentFK()
		{
			MasterFactory.RefreshEnabled = false;
			MasterFactory.FactoryForEverythingExceptEDocs.RefreshEnabled = false;

			var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif", "[ediDocManager SHP CIV S00001000] small.tif");
			var manager = new BatchImportManagerWithTimestamp();
			var logs = new List<string>();
			manager.LogProgress +=
				args =>
				{
					logs.Add(args.EventType + " - " + args.Message);

					if (args.Message.StartsWith("Imported file"))
					{
						//Add StorageMain to database now
						AssertNull(new DbBackendDocumentFactory(new BusinessObjectFactory()).LoadTop1<StorageMain>(
						new ZQuery(StorageMainSchema.SM_ParentFK, ShipmentS00001000.PK)));
						var storageMain = new DbBackendDocumentFactory(new BusinessObjectFactory()).New<StorageMain>();
						storageMain.SM_ParentFK = ShipmentS00001000.PK;
						storageMain.SM_DB = 1;
						storageMain.SM_CD1 = 1;
						var storageDoc = storageMain.Documents.AddNew();
						storageDoc.SC_SystemLastEditTimeUtc = ZDateTime.Now.AddHours(1);
						storageMain.Factory.RefreshEnabled = false;
						storageMain.MasterFactory.RefreshEnabled = false;
						storageMain.MasterFactory.Save();
						storageMain.Factory.Save();
						AssertNotNull(new DbBackendDocumentFactory(new BusinessObjectFactory()).LoadTop1<StorageMain>(
						new ZQuery(StorageMainSchema.SM_ParentFK, ShipmentS00001000.PK)));
					}
				};

			AssertNoExceptionThrown(() => manager.ImportFilesAndEmails(Path.GetDirectoryName(testFile)));

			AssertEquals("should be 0 documents - documents shouldn't be left unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should be 2 documents in factory one - documents have been allocated", 2, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
		}

		public void TestImportConcurrencyError_File_SM_DB()
		{
			// Create SD002 if not existed
			if (!DBHelper.DatabaseExists(2))
			{
				_ = DBHelper.CreateDatabase(2);
			}

			MasterFactory.RefreshEnabled = false;
			MasterFactory.FactoryForEverythingExceptEDocs.RefreshEnabled = false;

			// Create and save StorageMain record so it already exists
			var storageMain = MasterFactory.New<StorageMain>();
			storageMain.SM_ParentFK = ShipmentS00001000.PK;
			storageMain.SM_DB = 2;
			storageMain.SM_CD1 = 2;
			var storageDoc = storageMain.Documents.AddNew();
			storageDoc.SC_SystemLastEditTimeUtc = ZDateTime.Now;

			MasterFactory.Save();

			var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif", "[ediDocManager SHP CIV S00001000] small.tif");
			var manager = new BatchImportManagerWithTimestamp();
			var logs = new List<string>();
			manager.LogProgress +=
				args =>
				{
					logs.Add(args.EventType + " - " + args.Message);

					if (args.Message.StartsWith("Imported file"))
					{
						//modify database values of storage main
						var storageMain2 = new DbBackendDocumentFactory(new BusinessObjectFactory()).LoadTop1<StorageMain>(
						new ZQuery(StorageMainSchema.SM_ParentFK, ShipmentS00001000.PK));
						storageMain2.SM_DB = 1;
						var storageDoc2 = storageMain2.Documents.AddNew();
						storageDoc2.SC_SystemLastEditTimeUtc = ZDateTime.Now.AddHours(1);
						storageMain2.Factory.RefreshEnabled = false;
						storageMain2.MasterFactory.RefreshEnabled = false;
						storageMain2.MasterFactory.Save();
						storageMain2.Factory.Save();
						AssertEquals(2, storageMain.SM_DB);
						AssertEquals(1, storageMain2.SM_DB);
						AssertEquals(1, new DbBackendDocumentFactory(new BusinessObjectFactory()).LoadTop1<StorageMain>(
						new ZQuery(StorageMainSchema.SM_ParentFK, ShipmentS00001000.PK)).SM_DB);
					}
				};

			AssertNoExceptionThrown(() => manager.ImportFilesAndEmails(Path.GetDirectoryName(testFile)));

			AssertEquals("should be 0 documents - documents shouldn't be left unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should be 1 documents in factory one - documents have been allocated", 1, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
		}

		public void TestImportWithEmailsUsesSubjectLineRatherThanAttachmentNames()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "test@test.com";
			staff.GS_FullName = "Test Person";

			var email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email";
			email.MI_Subject = "[ediDocManager SHP CIV S00001000]";
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
			email.MI_SendDateTime = new ZDateTime(2005, 12, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			var attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "[SHP PKL S00001000] test.tif";
			attachment.MA_Data = SmallTifBytes;
			attachment.MA_MI = email.PK;

			var attachment2 = email.MailAttachments.AddNew();
			attachment2.MA_FileName = "[SHP HBL S00001000] test2.gif";
			attachment2.MA_Data = SmallGifBytes;
			attachment2.MA_MI = email.PK;

			var attachment3 = email.MailAttachments.AddNew();
			attachment3.MA_FileName = "[SHP ARN S00001000] Sample.PDF";
			attachment3.MA_Data = SamplePdfBytes;
			attachment3.MA_MI = email.PK;

			MasterFactory.Save();

			var importManager = new BatchImportManager();
			importManager.ImportFilesAndEmails(TestDirectory);

			AssertEquals("should be 0 documents - documents shouldn't be left unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should be 3 documents in factory one - documents have been allocated", 3, factoryOne.GetDatabaseCount(typeof(StorageDocs)));

			var parent = MasterFactory.LoadTop1<StorageMain>(new ZQuery());
			var docs = parent.Documents.Cast<StorageDocsBase>().OrderBy(doc => doc.SC_FileName).ToList();
			AssertEquals("Parent should have two documents allocated correctly", 2, docs.Count);
			AssertDocumentProperties(docs[0], "test", "CIV", Core.Constants.DocManagerCodes.Shipment, ShipmentS00001000.PK);
			AssertDocumentProperties(docs[1], "test2", "CIV", Core.Constants.DocManagerCodes.Shipment, ShipmentS00001000.PK);

			AssertEquals("Parent should have one file allocated correctly", 1, parent.Files.Count);
			AssertDocumentProperties(parent.Files[0], "Sample", "CIV", Core.Constants.DocManagerCodes.Shipment, ShipmentS00001000.PK);

			email.Reload();
			AssertEquals("Email's status should now be processed", MailStatus.Processed, email.MI_Status);

			StmALog[] logs = (ShipmentS00001000 as EnterpriseBusinessObject).Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentImported.Code));
			AssertEquals(string.Join("\r\n", logs.Select(l => l.SL_Reference)), 3, logs.Length);

			Assert(Array.Exists(logs, x => x.SL_Reference.Contains(parent.Documents[0].PK.ToString())));
			Assert(Array.Exists(logs, x => x.SL_Reference.Contains(parent.Documents[1].PK.ToString())));
			Assert(Array.Exists(logs, x => x.SL_Reference.Contains(parent.Files[0].PK.ToString())));
		}

		public void TestImportWithEmailsWithCurlyBraceAtEndOfSubjectLine()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "test@test.com";
			staff.GS_FullName = "Test Person";

			MailItem email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email";
			email.MI_Subject = "[ediDocManager SHP CIV S00001000}";
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
			email.MI_SendDateTime = new ZDateTime(2005, 12, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			MailAttachment attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "[SHP PKL S00001000] test.tif";
			attachment.MA_Data = SmallTifBytes;
			attachment.MA_MI = email.PK;

			MasterFactory.Save();

			BatchImportManager importManager = new BatchImportManager();
			importManager.ImportFilesAndEmails(TestDirectory);

			AssertEquals("should be 0 documents - due to bad subject line nothing will get imported and an email will be sent to user instead", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should be 0 documents in factory one - due to bad subject line nothing will get imported and an email will be sent to user instead", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));

			email.Reload();
			AssertEquals("Email's status should be marked as failed", MailStatus.Failed, email.MI_Status);
		}

		public void TestImportWithEmailsWithCurlyBraceInAttachmentName()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "test@test.com";
			staff.GS_FullName = "Test Person";

			MailItem email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email";
			email.MI_Subject = "[ediDocManager SHP CIV S00001000]";
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
			email.MI_SendDateTime = new ZDateTime(2005, 12, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			MailAttachment attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "[SHP PKL S00001000} test.tif";
			attachment.MA_Data = SmallTifBytes;
			attachment.MA_MI = email.PK;

			MasterFactory.Save();

			BatchImportManager importManager = new BatchImportManager();
			importManager.ImportFilesAndEmails(TestDirectory);

			AssertEquals("should be 0 documents - documents shouldn't be left unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should be 1 documents in factory one - document has been allocated despite malformed attachment name", 1, factoryOne.GetDatabaseCount(typeof(StorageDocs)));

			StorageMain parent = MasterFactory.LoadTop1<StorageMain>(new ZQuery());
			AssertEquals("Parent should have one document allocated correctly", 1, parent.Documents.Count);
			AssertDocumentProperties(parent.Documents[0], "[SHP PKL S00001000} test", "CIV", Core.Constants.DocManagerCodes.Shipment, ShipmentS00001000.PK);

			email.Reload();
			AssertEquals("Email's status should now be processed", MailStatus.Processed, email.MI_Status);
		}

		public void TestImportWithEmailsWithCurlyBraceAtStartOfSubjectLine()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "test@test.com";
			staff.GS_FullName = "Test Person";

			MailItem email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email";
			email.MI_Subject = "{ediDocManager SHP CIV S00001000";
			email.MI_From = "test@test.com";
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2005, 12, 1);
			email.MI_SendDateTime = new ZDateTime(2005, 12, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2005, 12, 1);

			MailAttachment attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "[SHP PKL S00001000] test.tif";
			attachment.MA_Data = SmallTifBytes;
			attachment.MA_MI = email.PK;

			MasterFactory.Save();

			BatchImportManager importManager = new BatchImportManager();
			importManager.ImportFilesAndEmails(TestDirectory);

			AssertEquals("should be 0 documents - nothing would be picked up by incorrect subject line", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			NumberedBusinessObjectFactory factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("should be 0 documents - nothing would be picked up by incorrect subject line", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));

			AssertEquals("Email's status should remain unchanged", MailStatus.Queued, email.MI_Status);
		}

		public void TestIncrementalSaveOnEmails()
		{
			for (int i = 0; i < 25; i++)
			{
				var email = MasterFactory.New<MailItem>();
				email.MI_Body = "Email " + i;
				email.MI_Subject = "[ediDocManager]";
				email.MI_From = "someone@somewhere.com";
				email.MI_Direction = MailDirection.Receive;
				email.MI_Status = MailStatus.Queued;
				email.MI_LastAttemptDateTime = ZDateTime.UtcNow;
				email.MI_SendDateTime = ZDateTime.UtcNow;
				email.MI_ReceivedDateTime = ZDateTime.UtcNow;
				MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);
				var attachment = email.MailAttachments.AddNew();
				attachment.MA_FileName = "small.gif";
				attachment.MA_Data = SmallGifBytes;
				attachment.MA_MI = email.PK;
			}
			MasterFactory.Save();

			int numberImportedSoFar = 0;
			BatchImportManager manager = new BatchImportManager();
			manager.LogProgress += args =>
			{
				if (args.Message.StartsWith("Importing email") && ++numberImportedSoFar == 4)
				{
					throw new OutOfMemoryException();
				}
			};
			AssertExceptionThrown(typeof(OutOfMemoryException), delegate
			{
				manager.ImportFilesAndEmails(TestDirectory);
			});

			AssertEquals("some documents should be saved", 2, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			manager = new BatchImportManager();
			manager.ImportFilesAndEmails(TestDirectory);

			AssertEquals("all documents should be saved", 25, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
		}

		public void TestIncrementalSaveOnFiles()
		{
			var testDirectory = Path.GetDirectoryName(resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif", "[SHP CIV S00001000] small-0.tif"));
			for (var i = 1; i < 25; i++)
			{
				resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif", "[SHP CIV S00001000] small-" + i + ".tif");
			}

			int numberImportedSoFar = 0;
			BatchImportManager manager = new BatchImportManager();
			manager.LogProgress += args =>
			{
				if (args.Message.StartsWith("Importing file") && ++numberImportedSoFar == 4)
				{
					throw new OutOfMemoryException();
				}
			};
			AssertExceptionThrown(typeof(OutOfMemoryException), delegate
			{
				manager.ImportFilesAndEmails(testDirectory);
			});

			AssertEquals("some documents should be saved", MasterFactory.GetFactory(1).GetDatabaseCount(typeof(StorageDocs)), 3);

			manager = new BatchImportManager();
			manager.ImportFilesAndEmails(testDirectory);

			AssertEquals("all documents should be saved", MasterFactory.GetFactory(1).GetDatabaseCount(typeof(StorageDocs)), 25);
		}

		public void TestFileMovedToUnsuccessfulFolderIfImportFails()
		{
			var apInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			apInvoice.AH_TransactionType = "INV";
			apInvoice.AH_TransactionNum = "BCR0720";

			Factory.Save();

			var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif", "[EdiDocManager ACP PIN BCR0720] small.tif");
			var testDirectory = Path.GetDirectoryName(testFile);
			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				throw new ApplicationException("Exception occurs on factory save.");
			});

			BatchImportManager manager = new BatchImportManager();
			List<string> logs = new List<string>();
			manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);
			AssertNoExceptionThrown(() => manager.ImportFilesAndEmails(testDirectory));

			AssertEquals(8, logs.Count);
			AssertEquals($"Information - Found 1 DocManager Files to import in {testDirectory}:\r\n{testFile}", logs[0]);
			AssertEquals($"Information - Importing file '{testFile}'", logs[1]);
			AssertEquals($"Information - Importing from File system.\r\nFull filename: {testFile};\r\nFilename only: [EdiDocManager ACP PIN BCR0720] small.tif.", logs[2]);
			AssertEquals($"Information - Imported file '[EdiDocManager ACP PIN BCR0720] small.TIF'.", logs[3]);
			AssertEquals("Error - File '[EdiDocManager ACP PIN BCR0720] small.tif' could not be imported. Exception occurs on factory save.", logs[4]);
			AssertEquals($"Information - File '{testFile}' has been moved to unsuccessful folder.", logs[5]);
			AssertEquals($"Information - File '{testFile}' is processed.", logs[6]);
			AssertEquals($"Information - Import Run Completed, processed 1 file(s), ignored 0 file(s) processed by another process.", logs[7]);
		}

		public void TestImportEmailWithValidSubjectAndFullEmailInOrgContactListWithRestrictEmailAllocationOn()
		{
			DocManagerRegistry.Instance.RestrictEmailAllocationForOrgContacts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetUpOrgContacts();

			var email = SetUpEmailWithValidSubject("A valid user<valid@test.com>");
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			MasterFactory.Save();

			var manager = new BatchImportManager();
			var logs = new List<string>();
			manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);
			manager.ImportFilesAndEmails("");

			var expectedLog = "Warning - Unable to allocate document. Allocation has been restricted to only accept Organization Contact email addresses";
			var actual = string.Join("\r\n", logs);
			AssertNotContains("Expect a warning log", expectedLog, actual);

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("Should import and allocate 1 file", 1, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("Should be no documents unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
		}

		public void TestImportEmailWithValidSubjectAndEmailInOrgContactListWithRestrictEmailAllocationOn()
		{
			DocManagerRegistry.Instance.RestrictEmailAllocationForOrgContacts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetUpOrgContacts();

			var email = SetUpEmailWithValidSubject("valid@test.com");
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			MasterFactory.Save();

			var manager = new BatchImportManager();
			manager.ImportFilesAndEmails("");

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("Should import and allocate 1 file", 1, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("Should be no documents unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
		}

		public void TestImportEmailWithValidSubjectAndEmailNotInOrgContactListButInWhiteListWithRestrictEmailAllocationOn()
		{
			DocManagerRegistry.Instance.RestrictEmailAllocationForOrgContacts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetUpOrgContacts();

			var acceptedEmails = new CodeDescriptionPairList();
			acceptedEmails.AddPair("*@domain.com", "The test address");

			SystemDataRegistry.Instance.EmailAddressesAllowedForImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, acceptedEmails);
			var email = SetUpEmailWithValidSubject("valid@domain.com");
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager);

			MasterFactory.Save();

			var manager = new BatchImportManager();
			manager.ImportFilesAndEmails("");

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("Should import and allocate 1 file", 1, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("Should be no documents unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
		}

		public void TestImportEmailWithValidSubjectAndEmailNotInOrgContactListNorWhiteListWithRestrictEmailAllocationOn()
		{
			DocManagerRegistry.Instance.RestrictEmailAllocationForOrgContacts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetUpOrgContacts();

			var acceptedEmails = new CodeDescriptionPairList();
			acceptedEmails.AddPair("*@domain.com", "The test address");

			SystemDataRegistry.Instance.EmailAddressesAllowedForImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, acceptedEmails);

			var email = SetUpEmailWithValidSubject("invalid@test.com");
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager, false);

			MasterFactory.Save();

			var manager = new BatchImportManager();
			var logs = new List<string>();
			manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);
			manager.ImportFilesAndEmails("");

			var expectedLog = "Warning - Unable to allocate document. Allocation has been restricted to only accept Organization Contact email addresses and invalid@test.com was not found in the Organization Contacts.";
			var actual = string.Join("\r\n", logs);
			AssertContains("Expect a warning log", expectedLog, actual);

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("Should be no documents imported", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("Should be no documents unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			email.Reload();
			AssertEquals("Email should not be processed", MailStatus.Failed, email.MI_Status);
		}

		public void TestImportEmailWithEmptySubjectAndEmailInOrgContactListWithRestrictEmailAllocationOn()
		{
			DocManagerRegistry.Instance.RestrictEmailAllocationForOrgContacts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetUpOrgContacts();

			var email = SetUpEmailWithValidSubject("valid@cw1.com");
			email.MI_Subject = "";
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager, false);

			MasterFactory.Save();

			var manager = new BatchImportManager();
			var logs = new List<string>();
			manager.LogProgress += args => logs.Add(args.EventType + " - " + args.Message);
			manager.ImportFilesAndEmails("");

			var expectedLog = "Warning - Unable to allocate document. Allocation has been restricted to only accept Organization Contact email addresses and invalid@test.com was not found in the Organization Contacts.";
			var actual = string.Join("\r\n", logs);
			AssertNotContains("Expect no warning log", expectedLog, actual);

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("Should be no documents imported", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("Should be no documents unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			email.Reload();
			AssertEquals("Email should not be processed", MailStatus.Queued, email.MI_Status);
		}

		public void TestImportEmailWithEmptySubjectAndEmailNotInOrgContactListWithRestrictEmailAllocationOn()
		{
			DocManagerRegistry.Instance.RestrictEmailAllocationForOrgContacts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetUpOrgContacts();

			var email = SetUpEmailWithValidSubject("invalid@test.com");
			email.MI_Subject = "";
			MailFilterLocatorTestHelper.SetApplication(email, MailFilterCodes.DocumentImportManager, false);

			MasterFactory.Save();

			var manager = new BatchImportManager();
			manager.ImportFilesAndEmails("");

			var factoryOne = MasterFactory.GetFactory(1);
			AssertEquals("Should be no documents imported", 0, factoryOne.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("Should be no documents unallocated", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));

			email.Reload();
			AssertEquals("Email should not be processed", MailStatus.Queued, email.MI_Status);
		}

		void SetUpOrgContacts()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_Email = "valid@test.com";

			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_Email = "valid@cw1.com";

			Factory.Save();
		}

		MailItem SetUpEmailWithValidSubject(string fromEmail)
		{
			var email = MasterFactory.New<MailItem>();
			email.MI_Body = "This is a test email for eDocs";
			email.MI_Subject = "[ediDocManager SHP MSC S00001000]";
			email.MI_From = fromEmail;
			email.MI_Direction = MailDirection.Receive;
			email.MI_Status = MailStatus.Queued;
			email.MI_LastAttemptDateTime = new ZDateTime(2018, 3, 1);
			email.MI_SendDateTime = new ZDateTime(2018, 3, 1);
			email.MI_ReceivedDateTime = new ZDateTime(2018, 3, 1);

			var attachment = email.MailAttachments.AddNew();
			attachment.MA_FileName = "ShipmentScannedDocs.pdf";
			attachment.MA_Data = ShipmentScannedDocsPdfBytes;
			attachment.MA_MI = email.PK;

			return email;
		}

		readonly string TestDirectory = Path.Combine(Env.TempPath, "TestImportDirectory");
		readonly string db2Name = new DocManagerDBHelper().GetDatabaseName(2);
		readonly DocManagerDBHelperTestClass DBHelper = new ();

		protected override void SetUp()
		{
			base.SetUp();
			if (!DBHelper.DatabaseExists(1))
			{
				DBHelper.CreateDatabase(1);
			}
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(DBQueryHelper.GetTableNameWithDatabasePrefix(1, StorageDocsSchema.Constants.TableName));
			SetupShipmentObjects();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		protected override void OnAfterBaseTestCaseRunBare()
		{
			if (DBHelper.DatabaseExists(2))
			{
				DBHelper.DropDatabase(db2Name);
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		byte[] BadImagePngBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.BadImage.png");

		byte[] SmallTifBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");

		byte[] ShipmentScannedDocsPdfBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.ShipmentScannedDocs.pdf");

		byte[] SamplePdfBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");

		string multipageTestDocumentTifPath;
		string MultipageTestDocumentTifPath
		{
			get
			{
				if (string.IsNullOrWhiteSpace(multipageTestDocumentTifPath))
				{
					multipageTestDocumentTifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.MultipageTestDocument.tif");
				}
				return multipageTestDocumentTifPath;
			}
		}

		byte[] SmallGifBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.gif");

		string SmallGifPath
		{
			get
			{
				if (string.IsNullOrEmpty(smallGifPath))
				{
					smallGifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.gif");
				}
				return smallGifPath;
			}
		}
		string smallGifPath;

		byte[] EmailWithImageBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Email with Image.msg");

		void SetupShipmentObjects()
		{
			ZQuery query1 = new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001000");
			ShipmentS00001000 = (BusinessObject)MasterFactory.LoadTop1<Enterprise.Integration.Freight.ICommonShipment>(query1);

			if (ShipmentS00001000 == null)
			{
				ShipmentS00001000 = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
				ShipmentS00001000[JobShipmentSchema.JS_UniqueConsignRef] = "S00001000";
			}

			query1 = new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001001");
			ShipmentWithHouseBill = (BusinessObject)MasterFactory.LoadTop1<Enterprise.Integration.Freight.ICommonShipment>(query1);

			if (ShipmentWithHouseBill == null)
			{
				ShipmentWithHouseBill = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
				ShipmentWithHouseBill[JobShipmentSchema.JS_UniqueConsignRef] = "S00001001";
			}

			ShipmentWithHouseBill[JobShipmentSchema.JS_HouseBill] = "12345678901234567890";
			ShipmentWithHouseBill[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
			ShipmentWithHouseBill[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";

			MasterFactory.Save();

			var query = new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001001");
			ShipmentS00001001 = (BusinessObject)MasterFactory.LoadTop1<Enterprise.Integration.Freight.ICommonShipment>(query);
		}

		BusinessObject ShipmentS00001000;
		BusinessObject ShipmentS00001001;
		BusinessObject ShipmentWithHouseBill;
	}
}
