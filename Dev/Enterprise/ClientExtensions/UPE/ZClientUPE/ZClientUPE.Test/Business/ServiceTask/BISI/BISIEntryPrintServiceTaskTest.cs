using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.Ftp;
using Enterprise.Client.UPE.Business.Ftp.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.UPE.ServiceTask.Testing
{
	[TestedType(typeof(BISIEntryPrintServiceTask))]
	internal class BISIEntryPrintServiceTaskTest : ServiceTaskTestCase<BISIEntryPrintServiceTask>
	{
		public void TestHumanReadableName()
		{
			var attributes = GetHostedServiceAttributes();
			AssertEquals("Entry Print Upload", attributes[0].Description);
		}

		[TestDate(2005, 1, 2)]
		public void TestFtpUploadOfEntryPrint()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			AssertEquals(0, Logger.Count);
			BatchProcessor.TryUploadPublic();
			AssertEquals(0, Logger.Count);
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			JobDeclaration testDec1 = JobDeclarationTest.CreateSendableDeclaration(Factory);
			testDec1.JE_EntrySubmittedDate = new ZDateTime(2003, 3, 1);
			JobComInvoiceLine invoiceLine = testDec1.InvoiceLines[0];
			invoiceLine.AddInfo.ZA_GSTE = ZString.Empty;
			testDec1.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].AddInfo.ZA_GSTE = "1234";
			testDec1.DoMerge();
			JobDeclaration testDec2 = JobDeclarationTest.CreateSendableDeclaration(Factory);
			testDec2.DoMerge();
			testDec1.JE_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
			testDec2.JE_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
			Factory.Save();
			BatchProcessor.TryUploadPublic();
			string[] uploadedFiles = Directory.GetFiles(BatchProcessor.UploadBatchProcessorTestDir);
			AssertEquals(1, uploadedFiles.Length);
			string uploadedFile = uploadedFiles[0];
			ArrayList lines = new ArrayList();
			string line = null;
			using (StreamReader reader = new StreamReader(uploadedFile))
			{
				do
				{
					line = reader.ReadLine();
					if (line != null)
					{
						lines.Add(line);
					}
				}
				while (line != null);
			}

			AssertEquals("First line should contain the header", "HDR-00010002AU2005-01-02-00.00.00.000000", lines[0]);
			AssertEquals("First line of first entry", "ENTRY PRINT                              X  XXXXXX    ******************************          PAGE 1", lines[1].ToString().Trim());
			AssertEquals("First line of second entry", "ENTRY PRINT                              X  XXXXXX    ******************************          PAGE 1", lines[68].ToString().Trim());
			AssertEquals("Last line should contain the trailer", "FTR-00010002AU2005-01-02-00.00.00.000000", lines[lines.Count - 1]);
			AssertContains(string.Format("Start Entry Print Uploading for Company {0}", Env.CurrentCompany.Name), Logger.ToString());
			AssertContains(string.Format("Finish Entry Print Uploading (2 Uploaded) for Company {0}", Env.CurrentCompany.Name), Logger.ToString());
			AssertNotEquals(0, Logger.Count);
		}

		[TestDate(2005, 1, 2)]
		public void TestFtpUploadOfEntryPrint_TwoCompanies()
		{
			BatchProcessor.NewUploader = true;
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			AssertEquals(0, Logger.Count);
			BatchProcessor.TryUploadPublic();
			AssertEquals(0, Logger.Count);
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			JobDeclaration testDec1 = JobDeclarationTest.CreateSendableDeclaration(Factory);
			testDec1.JE_EntrySubmittedDate = new ZDateTime(2003, 3, 1);
			JobComInvoiceLine invoiceLine = testDec1.InvoiceLines[0];
			invoiceLine.AddInfo.ZA_GSTE = ZString.Empty;
			testDec1.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].AddInfo.ZA_GSTE = "1234";
			testDec1.DoMerge();
			JobDeclaration testDec2;
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var branch = company.Branches.AddNew();
			Factory.Save();
			using (branch.SetAsTemporaryContext())
			{
				UPEDataRegistry.Instance.EnableUPECustomisations = true;
				testDec2 = JobDeclarationTest.CreateSendableDeclaration(Factory);
				testDec2.DoMerge();
				testDec2.JE_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
				Factory.Save();
			}

			testDec1.JE_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
			Factory.Save();
			BatchProcessor.TryUploadPublic();
			string[] uploadedFiles = Directory.GetFiles(BatchProcessor.UploadBatchProcessorTestDir);
			AssertEquals(2, uploadedFiles.Length);
			AssertContains(string.Format("Start Entry Print Uploading for Company {0}", Env.CurrentCompany.Name), Logger.ToString());
			AssertContains(string.Format("Finish Entry Print Uploading (1 Uploaded) for Company {0}", Env.CurrentCompany.Name), Logger.ToString());
			AssertContains(string.Format("Start Entry Print Uploading for Company {0}", company.CompanyName), Logger.ToString());
			AssertContains(string.Format("Finish Entry Print Uploading (1 Uploaded) for Company {0}", company.CompanyName), Logger.ToString());
			AssertNotEquals(0, Logger.Count);
		}

		public void TestFtpUploadOfEntryPrint_ProcessesOnly1000DeclarationsAtATime()
		{
			AssertEquals("Default MaximumDeclarationsToProcessPerDay", 1000, BatchProcessor.MaximumDeclarationsToProcessPerDayPublic);
			BatchProcessor.MaximumDeclarationsToProcessPerDayPublic = 10;
			for (int i = 0; i < 15; i++)
			{
				UPEJobDeclaration declaration = CreateDeclaration();
				declaration.IsEntryPrintToBISIPending = true;
				declaration.CurrentQueue.P4_CustomAttrib9 = "test";
				Factory.Save();
			}

			BatchProcessor.TryUploadPublic();
			ZQuery query = new ZQuery();
			query.AddToFilter(ProcessQueueSchema.P4_CustomAttrib9, "test");
			query.AddToFilter(UPEJobDeclaration.IsEntryPrintToBISIPendingProcessQueueColumn, ZBool.False);
			AssertEquals("Number of declarations processed at a time should be limited to prevent an OutOfMemoryException", 10, Factory.GetDatabaseCount(typeof(ProcessQueue), query));
		}

		public void TestFtpUploadOfEntryPrint_SavedToArchiveDirectory()
		{
			UPEJobDeclaration testDec = (UPEJobDeclaration)JobDeclarationTest.CreateSendableDeclaration(Factory);
			testDec.JE_EntrySubmittedDate = new ZDateTime(2003, 3, 1);
			JobComInvoiceLine invoiceLine = testDec.InvoiceLines[0];
			invoiceLine.AddInfo.ZA_GSTE = ZString.Empty;
			testDec.DoMerge();
			testDec.JE_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
			Factory.Save();
			DirectoryInfo archiveDirectory = new DirectoryInfo(Path.Combine(Env.TempPath, "TestArchiveDirectory"));
			string oldEntryPrintFtpArchiveDirectory = UPEDataRegistry.Instance.EntryPrintSftpArchiveDirectory;
			UPEDataRegistry.Instance.EntryPrintSftpArchiveDirectory = archiveDirectory.FullName;
			archiveDirectory.Create();
			try
			{
				BatchProcessor.TryUploadPublic();
				AssertEquals("1 file should be copied to the archive directory", 1, archiveDirectory.GetFiles().Length);
				AssertEquals("Archived file should be populated with data", true, archiveDirectory.GetFiles()[0].Length > 0);
			}
			finally
			{
				archiveDirectory.Delete(true);
				UPEDataRegistry.Instance.EntryPrintSftpArchiveDirectory = oldEntryPrintFtpArchiveDirectory;
			}
		}

		public void TestSecondFtpUploadOfEntryPrintDoesntUploadAnything()
		{
			UPEJobDeclaration testDec = (UPEJobDeclaration)JobDeclarationTest.CreateSendableDeclaration(Factory);
			testDec.JE_EntrySubmittedDate = new ZDateTime(2003, 3, 1);
			JobComInvoiceLine invoiceLine = testDec.InvoiceLines[0];
			invoiceLine.AddInfo.ZA_GSTE = ZString.Empty;
			testDec.DoMerge();
			testDec.JE_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
			Factory.Save();
			AssertEquals("Upload of declaration entry print should be pending", true, testDec.IsEntryPrintToBISIPending);
			BatchProcessor.TryUploadPublic();
			AssertEquals("Upload of declaration entry print no longer pending", false, testDec.IsEntryPrintToBISIPending);
			new DirectoryInfo(BatchProcessor.UploadBatchProcessorTestDir).GetFiles()[0].Delete();
			BatchProcessor.TryUploadPublic();
			string[] uploadedFiles = Directory.GetFiles(BatchProcessor.UploadBatchProcessorTestDir);
			AssertEquals("Already processed declarations shouldn't be processed a second time", 0, uploadedFiles.Length);
		}

		public void TestDontAttemptUploadIfCantUpload()
		{
			UPEJobDeclaration testDec1 = (UPEJobDeclaration)JobDeclarationTest.CreateSendableDeclaration(Factory);
			testDec1.DoMerge();
			testDec1.JE_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
			Factory.Save();
			AssertEquals("Upload of declaration entry print should be pending", true, testDec1.IsEntryPrintToBISIPending);
			BatchProcessor.DummyFtpUploader.TestCanUpload = false;
			BatchProcessor.TryUploadPublic();
			string[] uploadedFiles = Directory.GetFiles(BatchProcessor.UploadBatchProcessorTestDir);
			AssertEquals("Should not attempt an upload", 0, uploadedFiles.Length);
			AssertEquals("Upload of declaration entry print should still be pending", true, testDec1.IsEntryPrintToBISIPending);
		}

		public void TestErrorInUpload()
		{
			UPEJobDeclaration testDec1 = (UPEJobDeclaration)JobDeclarationTest.CreateSendableDeclaration(Factory);
			testDec1.DoMerge();
			testDec1.JE_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
			Factory.Save();
			AssertEquals("Upload of declaration entry print should be pending", true, testDec1.IsEntryPrintToBISIPending);
			BatchProcessor.DummyFtpUploader.TestCanUpload = true;
			BatchProcessor.DummyFtpUploader.UploadToFtpServerShouldFail = true;
			BatchProcessor.TryUploadPublic();
			AssertEquals("Should 3 times before failing", 3, BatchProcessor.DummyFtpUploader.DoUploadCalledCounter);
			AssertEquals("There should be a notification indicating the retries", true, ((TestServiceLogger)BatchProcessor.ServiceLogger).ToString().Contains("Retrying..."));
			string[] uploadedFiles = Directory.GetFiles(BatchProcessor.UploadBatchProcessorTestDir);
			AssertEquals("No file should be uploaded", 0, uploadedFiles.Length);
			AssertEquals("No entries should be indicated as uploaded to the user", true, ((TestServiceLogger)BatchProcessor.ServiceLogger).ToString().Contains("Finish Entry Print Uploading (0 Uploaded)"));
		}

		public void TestDontUploadIfFactorySaveFails()
		{
			UPEJobDeclaration testDec1 = (UPEJobDeclaration)JobDeclarationTest.CreateSendableDeclaration(Factory);
			testDec1.DoMerge();
			testDec1.JE_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
			Factory.Save();
			AssertEquals("Upload of declaration entry print should be pending", true, testDec1.IsEntryPrintToBISIPending);
			BatchProcessor.FactorySaveShouldFail = true;
			BatchProcessor.RunTask();
			string[] uploadedFiles = Directory.GetFiles(BatchProcessor.UploadBatchProcessorTestDir);
			AssertEquals("Should not have attempted an upload", 0, uploadedFiles.Length);
			AssertEquals("Upload of declaration entry print should still be pending", true, testDec1.IsEntryPrintToBISIPending);
		}

		public void TestExecute_Timeout()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "JLI";
			staff.GS_EmailAddress = "jay.li@whatever.com";
			UPEDataRegistry.Instance.SftpServerTimeoutItemNotificationGroup = group.PK.ToGuid();
			Factory.Save();
			UPEDataRegistry.Instance.SftpServerTimeout = 5;
			new TestBISIEntryPrintBatchProcessorTimeout(Logger).RunTask();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Service Task 'ZU2' has timed out", Env.OutgoingMailManager.EmailsCreated[0].Subject);
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1hour", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		#region Implementation
		TestBISIEntryPrintBatchProcessor BatchProcessor;
		UPEJobDeclaration CreateDeclaration()
		{
			UPEJobDeclaration declaration = (UPEJobDeclaration)JobDeclarationTest.CreateSendableDeclaration(Factory);
			declaration.JE_EntrySubmittedDate = new ZDateTime(2003, 3, 1);
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.AddInfo.ZA_GSTE = ZString.Empty;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].AddInfo.ZA_GSTE = "1234";
			declaration.DoMerge();
			return declaration;
		}

		TestServiceLogger Logger
		{
			get
			{
				return logger ?? (logger = new TestServiceLogger());
			}
		}

		TestServiceLogger logger;
		protected override void SetUpCore()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUpCore();
			BatchProcessor = new TestBISIEntryPrintBatchProcessor(Logger);
			Directory.CreateDirectory(BatchProcessor.UploadBatchProcessorTestDir);
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			if (Directory.Exists(BatchProcessor.UploadBatchProcessorTestDir))
			{
				Directory.Delete(BatchProcessor.UploadBatchProcessorTestDir, true);
			}
		}

		#endregion
		#region Test Classes
		public class TestBISIEntryPrintBatchProcessor : BISIEntryPrintServiceTask
		{
			public TestBISIEntryPrintBatchProcessor(ILogger logger) : base(logger)
			{
			}

			public void TryUploadPublic()
			{
				TryUpload(CancellationToken.None);
			}

			public int MaximumDeclarationsToProcessPerDayPublic
			{
				get
				{
					return MaximumDeclarationsToProcessPerDay;
				}

				set
				{
					fMaximumDeclarationsToProcessPerDay = value;
				}
			}

			protected override int MaximumDeclarationsToProcessPerDay
			{
				get
				{
					return fMaximumDeclarationsToProcessPerDay != null ? fMaximumDeclarationsToProcessPerDay.Value : base.MaximumDeclarationsToProcessPerDay;
				}
			}

			int? fMaximumDeclarationsToProcessPerDay;
			#region Simulating Failing Factory.Save()
			public bool FactorySaveShouldFail;
			protected override BusinessObjectFactory NewFactory()
			{
				BusinessObjectFactory result = base.NewFactory();
				if (FactorySaveShouldFail)
				{
					result.New(typeof(BusinessObjectWithFailedSave));
				}

				return result;
			}

			class BusinessObjectWithFailedSave : OrgHeader
			{
				public BusinessObjectWithFailedSave(BusinessObjectFactory factory, DataRow row) : base(factory, row)
				{
				}

				public override void OnSaving()
				{
					throw new InvalidOperationException();
				}
			}

			#endregion
			#region DummyFtpUploader
			protected override FtpUploader GetNewBISIUploader()
			{
				if (NewUploader)
				{
					fDummyFtpUploader = null;
				}

				DummyFtpUploader.FileName = Path.GetRandomFileName();
				return DummyFtpUploader;
			}

			public bool NewUploader { get; set; }

			public DummyFtpUploader DummyFtpUploader
			{
				get
				{
					if (fDummyFtpUploader == null)
					{
						fDummyFtpUploader = new DummyFtpUploader(UploadBatchProcessorTestDir, UPEDataRegistry.Instance.EntryPrintSftpArchiveDirectory, Notifications);
					}

					return fDummyFtpUploader;
				}
			}

			DummyFtpUploader fDummyFtpUploader;
			public string UploadBatchProcessorTestDir
			{
				get
				{
					return Path.Combine(Env.TempPath, "EntryPrintUploadTesting");
				}
			}
			#endregion
		}
		public class TestBISIEntryPrintBatchProcessorTimeout : TestBISIEntryPrintBatchProcessor
		{
			public TestBISIEntryPrintBatchProcessorTimeout(ILogger logger) : base(logger)
			{
			}

			protected override FtpUploader GetNewBISIUploader()
			{
				Thread.Sleep(10000);
				return new BISIEntryPrintUploader(Notifications);
			}
		}
		#endregion
	}
}
