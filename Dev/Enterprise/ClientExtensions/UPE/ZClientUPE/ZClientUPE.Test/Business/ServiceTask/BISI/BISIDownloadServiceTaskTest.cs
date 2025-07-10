using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.Business.Ftp;
using Enterprise.Client.UPE.Business.Ftp.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Client.UPE.ServiceTask.Testing
{
	[TestedType(typeof(BISIDownloadServiceTask))]
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class BISIDownloadServiceTaskTest : ServiceTaskTestCase<BISIDownloadServiceTask>
	{
		public void TestCleanupOldPreDownloadedCharges()
		{
			#region setup pre-downloaded charges
			ZDateTime testDate = ZDateTime.Now;
			ClientPWSHeader header = Factory.NewWithValidTestData<ClientPWSHeader>();
			header.U1_BillableWeight = 6.6m;
			header.U1_BillToAccount = "BOB";
			header.U1_ImportedDate = testDate;
			header.U1_InvoiceNumber = "12345";
			header.U1_WayBillNumber = "678";
			header.U1_WayBillShortNumber = "910";
			ClientPWSCharge charge = header.Charges.AddNew();
			charge.U2_ChargeDescription = "charge 1";
			charge.U2_Discount = 2.2m;
			charge.U2_NetAmount = 3.3m;
			charge.U2_NonTaxableAmount = 4.4m;
			charge.U2_TaxableAmount = 5.5m;
			Factory.Save();
			#endregion
			ClientPWSHeader[] headers = Factory.Load<ClientPWSHeader>(new ZQuery());
			Assert("sanity check", headers.Length == 1);
			Processor.RunTask();
			headers = Factory.Load<ClientPWSHeader>(new ZQuery());
			Assert("still good", headers.Length == 1);
			header.U1_ImportedDate = testDate.AddDays(-6);
			Factory.Save();
			Processor.DummyBISIDownloader.PrepareZipFilesForTest();
			Processor.RunTask();
			headers = Factory.Load<ClientPWSHeader>(new ZQuery());
			Assert("still good", headers.Length == 1);
			header.U1_ImportedDate = testDate.AddDays(-7);
			Factory.Save();
			Processor.DummyBISIDownloader.PrepareZipFilesForTest();
			Processor.RunTask();
			string query = String.Format("SELECT COUNT(*) FROM {0}", ClientPWSHeaderSchema.PK.TableName);
			int headerCount = Convert.ToInt32(Db.Connection.ExecuteScalar(query));
			Assert("it's old so it was deleted", headerCount == 0);
		}

		public void TestScanAndMatchPreDownloadedCharges()
		{
			#region setup pre-downloaded charges
			ZDateTime testDate = ZDateTime.Now;
			ClientPWSHeader header = Factory.NewWithValidTestData<ClientPWSHeader>();
			header.U1_BillableWeight = 6.6m;
			header.U1_BillToAccount = "BOB";
			header.U1_ImportedDate = testDate;
			header.U1_InvoiceNumber = "12345";
			header.U1_WayBillNumber = "678";
			header.U1_WayBillShortNumber = "910";
			ClientPWSCharge charge = header.Charges.AddNew();
			charge.U2_ChargeDescription = "charge 1";
			charge.U2_Discount = 2.2m;
			charge.U2_NetAmount = 3.3m;
			charge.U2_NonTaxableAmount = 4.4m;
			charge.U2_TaxableAmount = 5.5m;
			ClientPWSHeader header2 = Factory.NewWithValidTestData<ClientPWSHeader>();
			header2.U1_BillableWeight = 6.6m;
			header2.U1_BillToAccount = "BOB";
			header2.U1_ImportedDate = testDate;
			header2.U1_InvoiceNumber = "55555";
			header2.U1_WayBillNumber = "999";
			header2.U1_WayBillShortNumber = "888";
			charge = header2.Charges.AddNew();
			charge.U2_ChargeDescription = "charge 2";
			charge.U2_Discount = 1.1m;
			charge.U2_NetAmount = 2.2m;
			charge.U2_NonTaxableAmount = 3.4m;
			charge.U2_TaxableAmount = 4.4m;
			Factory.Save();
			#endregion
			ClientPWSHeader[] headers = Factory.Load<ClientPWSHeader>(new ZQuery());
			Assert("sanity check", headers.Length == 2);
			Processor.RunTask();
			headers = Factory.Load<ClientPWSHeader>(new ZQuery());
			Assert("pre-download charges should not have been processed because of no shipments yet", headers.Length == 2);
			Callout calloutWithLongTrackId = Factory.NewWithValidTestData<Callout>();
			calloutWithLongTrackId.CS_HAWB = "678";
			Factory.Save();
			AssertNull(calloutWithLongTrackId.JobHeader);
			Callout calloutWithShortTrackId = Factory.NewWithValidTestData<Callout>();
			calloutWithShortTrackId.CS_HAWB = "888";
			Factory.Save();
			AssertNull(calloutWithShortTrackId.JobHeader);
			Processor.IsExpectingOneOfTheImportsToFail = false;
			Processor.IsExpectingOneOfTheUnzipsToFail = false;
			Processor.DummyBISIDownloader.PrepareZipFilesForTest();
			Processor.RunTask();
			AssertNull(calloutWithLongTrackId.JobHeader);
			AssertNull(calloutWithShortTrackId.JobHeader);
			Processor.DummyBISIDownloader.PrepareZipFilesForTest();
			Processor.RunTask();
			AssertNull(calloutWithLongTrackId.JobHeader);
			AssertNull(calloutWithShortTrackId.JobHeader);
			calloutWithLongTrackId.BisiUploadDate = testDate;
			calloutWithShortTrackId.BisiUploadDate = testDate;
			var mawb = Factory.NewWithValidTestData<UPECusMAWB>();
			calloutWithLongTrackId.CS_CM = mawb.PK;
			calloutWithShortTrackId.CS_CM = mawb.PK;
			Factory.Save();
			Processor.DummyBISIDownloader.PrepareZipFilesForTest();
			Processor.RunTask();
			var factoryReloaded = new BusinessObjectFactory();
			calloutWithLongTrackId = factoryReloaded.Load<Callout>(calloutWithLongTrackId.PK);
			calloutWithShortTrackId = factoryReloaded.Load<Callout>(calloutWithShortTrackId.PK);
			AssertChargesInfo(header, calloutWithLongTrackId);
			AssertChargesInfo(header2, calloutWithShortTrackId);
		}

		public void TestScanAndMatchPreDownloadedCharges_NoMatchOnNonUPEBranches()
		{
			#region setup pre-downloaded charges
			ZDateTime testDate = ZDateTime.Now;
			ClientPWSHeader header = Factory.NewWithValidTestData<ClientPWSHeader>();
			header.U1_BillableWeight = 6.6m;
			header.U1_BillToAccount = "BOB";
			header.U1_ImportedDate = testDate;
			header.U1_InvoiceNumber = "12345";
			header.U1_WayBillNumber = "678";
			header.U1_WayBillShortNumber = "910";
			ClientPWSCharge charge = header.Charges.AddNew();
			charge.U2_ChargeDescription = "charge 1";
			charge.U2_Discount = 2.2m;
			charge.U2_NetAmount = 3.3m;
			charge.U2_NonTaxableAmount = 4.4m;
			charge.U2_TaxableAmount = 5.5m;
			ClientPWSHeader header2 = Factory.NewWithValidTestData<ClientPWSHeader>();
			header2.U1_BillableWeight = 6.6m;
			header2.U1_BillToAccount = "BOB";
			header2.U1_ImportedDate = testDate;
			header2.U1_InvoiceNumber = "55555";
			header2.U1_WayBillNumber = "999";
			header2.U1_WayBillShortNumber = "888";
			charge = header2.Charges.AddNew();
			charge.U2_ChargeDescription = "charge 2";
			charge.U2_Discount = 1.1m;
			charge.U2_NetAmount = 2.2m;
			charge.U2_NonTaxableAmount = 3.4m;
			charge.U2_TaxableAmount = 4.4m;
			Factory.Save();
			#endregion
			ClientPWSHeader[] headers = Factory.Load<ClientPWSHeader>(new ZQuery());
			Assert("sanity check", headers.Length == 2);
			Processor.RunTask();
			headers = Factory.Load<ClientPWSHeader>(new ZQuery());
			Assert("pre-download charges should not have been processed because of no shipments yet", headers.Length == 2);
			Callout calloutWithLongTrackId = Factory.NewWithValidTestData<Callout>();
			calloutWithLongTrackId.CS_HAWB = "678";
			Factory.Save();
			AssertNull(calloutWithLongTrackId.JobHeader);
			Callout calloutWithShortTrackId = Factory.NewWithValidTestData<Callout>();
			calloutWithShortTrackId.CS_HAWB = "888";
			Factory.Save();
			AssertNull(calloutWithShortTrackId.JobHeader);
			Processor.IsExpectingOneOfTheImportsToFail = false;
			Processor.IsExpectingOneOfTheUnzipsToFail = false;
			Processor.DummyBISIDownloader.PrepareZipFilesForTest();
			Processor.RunTask();
			AssertNull(calloutWithLongTrackId.JobHeader);
			AssertNull(calloutWithShortTrackId.JobHeader);
			Processor.DummyBISIDownloader.PrepareZipFilesForTest();
			Processor.RunTask();
			AssertNull(calloutWithLongTrackId.JobHeader);
			AssertNull(calloutWithShortTrackId.JobHeader);
			calloutWithLongTrackId.BisiUploadDate = testDate;
			calloutWithShortTrackId.BisiUploadDate = testDate;
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "ABC";
			Factory.Save();
			var mawb = Factory.NewWithValidTestData<UPECusMAWB>();
			mawb.CM_GB = branch.PK;
			calloutWithLongTrackId.CS_CM = mawb.PK;
			calloutWithShortTrackId.CS_CM = mawb.PK;
			Factory.Save();
			Processor.DummyBISIDownloader.PrepareZipFilesForTest();
			Processor.RunTask();
			var factoryReloaded = new BusinessObjectFactory();
			calloutWithLongTrackId = factoryReloaded.Load<Callout>(calloutWithLongTrackId.PK);
			calloutWithShortTrackId = factoryReloaded.Load<Callout>(calloutWithShortTrackId.PK);
			AssertNull(calloutWithLongTrackId.JobHeader);
			AssertNull(calloutWithShortTrackId.JobHeader);
		}

		void AssertChargesInfo(ClientPWSHeader header, Callout callOut)
		{
			AssertNotNull(callOut.JobHeader);
			Assert("should have charges", callOut.JobHeader.Charges.Count == 1);
			Assert("CS_ChargableWeight", callOut.CS_ChargableWeight == header.U1_BillableWeight);
			Assert("bill to", callOut.BillToAccountNumber == header.U1_BillToAccount);
			Assert("invoice number", callOut.InvoiceNumber == header.U1_InvoiceNumber);
			header.Reload();
			ZDateTime expectedDate = header.U1_ImportedDate.AddSeconds(-header.U1_ImportedDate.Second).AddMilliseconds(-header.U1_ImportedDate.Millisecond);
			Assert("transfered date", ((IBisiDownload)callOut).TransferredDateTime == expectedDate);
		}

		public void TestHumanReadableName()
		{
			var attributes = GetHostedServiceAttributes();
			AssertEquals("BISI COD Download", attributes[0].Description);
		}

		public void TestDefaultSchedule()
		{
			AssertEquals("30minutes", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestExecute()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			AssertEquals(0, Logger.Count);
			Processor.RunTask();
			AssertEquals(0, Logger.Count);
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			AssertEquals("Pre-condition. There should be nothing in the Archive directory", 0, Directory.GetFiles(Processor.DummyBISIDownloader.ArchiveDir).Length);
			AssertEquals("Pre-condition. Shipment has not been downloaded. Invoice number should be empty", "", UPECusHAWB1.InvoiceNumber);
			AssertEquals("Pre-condition. Shipment has not been downloaded. Invoice number should be empty", "", UPECusHAWB2.InvoiceNumber);
			Processor.RunTask();
			AssertEquals("Source file should be removed on successful import", false, File.Exists(Processor.DummyBISIDownloader.SourceFile1Path));
			AssertEquals("Source file should be removed on successful import", false, File.Exists(Processor.DummyBISIDownloader.SourceFile2Path));
			AssertEquals("The two files being imported should be moved to the archive directory.", 2, Directory.GetFiles(Processor.DummyBISIDownloader.ArchiveDir).Length);
			AssertEquals("Temporary file should be deleted", false, File.Exists(Processor.DummyBISIDownloader.DownloadedFileName));
			UPECusHAWB1.CurrentQueue.Reload();
			UPECusHAWB2.CurrentQueue.Reload();
			AssertEquals("Invoice should be populated now", "000000333333", UPECusHAWB1.InvoiceNumber);
			AssertEquals("Invoice should be populated now", "000000111111", UPECusHAWB2.InvoiceNumber);
			AssertNotEquals(0, Logger.Count);
		}

		[TestDate(2005, 11, 2)]
		public void TestRunTask_NotAU()
		{
			foreach (var br in Factory.Load<GlbBranch>(new ZQuery()))
			{
				br.SetCountry(CountryCodes.Taiwan);
			}
			Factory.Save();

			Processor.RunTask();

			AssertEquals("", Processor.Logger.ToString());
		}

		public void TestExecute_DownloadFails()
		{
			AssertEquals("Pre-condition. There should be nothing in the Archive directory", 0, Directory.GetFiles(Processor.DummyBISIDownloader.ArchiveDir).Length);
			AssertEquals("Pre-condition. Shipment has not been downloaded. Invoice number should be empty", "", UPECusHAWB1.InvoiceNumber);
			AssertEquals("Pre-condition. Shipment has not been downloaded. Invoice number should be empty", "", UPECusHAWB2.InvoiceNumber);
			Processor.DummyBISIDownloader.IsExpectingDownloadFailure = true;
			Processor.RunTask();
			AssertEquals("Source file should not be removed if import is unsuccessful", true, File.Exists(Processor.DummyBISIDownloader.SourceFile1Path));
			AssertEquals("Source file should not be removed if import is unsuccessful", true, File.Exists(Processor.DummyBISIDownloader.SourceFile2Path));
			AssertEquals("There should be nothing in the archive directory.", 0, Directory.GetFiles(Processor.DummyBISIDownloader.ArchiveDir).Length);
			AssertEquals("Temporary file should be deleted", false, File.Exists(Processor.DummyBISIDownloader.DownloadedFileName));
			AssertEquals("Import fails. Invoice should not be populated.", "", UPECusHAWB1.InvoiceNumber);
			AssertEquals("Import fails. Invoice should not be populated.", "", UPECusHAWB2.InvoiceNumber);
		}

		public void TestExecute_EmptyDirectory()
		{
			Processor.DummyBISIDownloader.IsExpectingEmptyZipDirectory = true;
			Processor.RunTask();
			AssertEquals("Nothing Imported. Archive directory should be empty.", 0, Directory.GetFiles(Processor.DummyBISIDownloader.ArchiveDir).Length);
		}

		public void TestExecute_ImportFails()
		{
			AssertEquals("Pre-condition. There should be nothing in the Archive directory", 0, Directory.GetFiles(Processor.DummyBISIDownloader.ArchiveDir).Length);
			AssertEquals("Pre-condition. Shipment has not been downloaded. Invoice number should be empty", "", UPECusHAWB1.InvoiceNumber);
			AssertEquals("Pre-condition. Shipment has not been downloaded. Invoice number should be empty", "", UPECusHAWB2.InvoiceNumber);
			Processor.IsExpectingOneOfTheImportsToFail = true;
			Processor.RunTask();
			AssertEquals("Source file should not be removed if import is unsuccessful", true, File.Exists(Processor.DummyBISIDownloader.SourceFile1Path));
			AssertEquals("Source file should be removed if import is successful", false, File.Exists(Processor.DummyBISIDownloader.SourceFile2Path));
			AssertEquals("CODZipFile1 should be moved to the archive directory.", 1, Directory.GetFiles(Processor.DummyBISIDownloader.ArchiveDir).Length);
			AssertEquals("Temporary file should be deleted", false, File.Exists(Processor.DummyBISIDownloader.DownloadedFileName));
			UPECusHAWB1.CurrentQueue.Reload();
			UPECusHAWB2.CurrentQueue.Reload();
			AssertEquals("Import successful. Invoice should be populated.", "000000333333", UPECusHAWB1.InvoiceNumber);
			AssertEquals("Import fails. Invoice should not be populated.", "", UPECusHAWB2.InvoiceNumber);
		}

		public void TestExecute_UnzipFails()
		{
			AssertEquals("Pre-condition. There should be nothing in the Archive directory", 0, Directory.GetFiles(Processor.DummyBISIDownloader.ArchiveDir).Length);
			AssertEquals("Pre-condition. Shipment has not been downloaded. Invoice number should be empty", "", UPECusHAWB1.InvoiceNumber);
			AssertEquals("Pre-condition. Shipment has not been downloaded. Invoice number should be empty", "", UPECusHAWB2.InvoiceNumber);
			Processor.IsExpectingOneOfTheUnzipsToFail = true;
			Processor.RunTask();
			AssertEquals("Source file should be removed if import is successful", false, File.Exists(Processor.DummyBISIDownloader.SourceFile1Path));
			AssertEquals("Source file should not be removed if import is unsuccessful", true, File.Exists(Processor.DummyBISIDownloader.SourceFile2Path));
			AssertEquals("CODZipFile2 should be moved to the archive directory", 1, Directory.GetFiles(Processor.DummyBISIDownloader.ArchiveDir).Length);
			AssertEquals("Temporary file should be deleted", false, File.Exists(Processor.DummyBISIDownloader.DownloadedFileName));
			UPECusHAWB1.CurrentQueue.Reload();
			UPECusHAWB2.CurrentQueue.Reload();
			AssertEquals("Import fails. Invoice should not be populated", "", UPECusHAWB1.InvoiceNumber);
			AssertEquals("Import successful. Invoice should be populated", "000000111111", UPECusHAWB2.InvoiceNumber);
			string expectedUnzipErrorNotification = "Cannot unzip file '" + Processor.DummyBISIDownloader.SourceFile2Path + "' downloaded from the ftp server";
			AssertEquals("Error notification should indicate the unzip failed and the file may be corrupt", true, Logger.ToString().Contains(expectedUnzipErrorNotification));
		}

		public void TestExecute_SendWarningReportAfterChargesDownloaded()
		{
			ZDateTime initialTime = new ZDateTime(2005, 1, 1, 00, 00, 00);
			Processor.DummyBISIDownloader.IsExpectingEmptyZipDirectory = true;
			Processor.RunTask();
			AssertEquals("Should not be called until a file has been downloaded", false, Processor.BISIUploadWarningReporter.SendWarningEmailIfRequiredCalled);
			Processor.DummyBISIDownloader.IsExpectingEmptyZipDirectory = false;
			Processor.RunTask();
			AssertEquals("Should be called when at least 1 file was downloaded", true, Processor.BISIUploadWarningReporter.SendWarningEmailIfRequiredCalled);
			Processor.BISIUploadWarningReporter.SendWarningEmailIfRequiredCalled = false;
		}

		public void TestExecute_WhenFtpExceptionOccurs()
		{
			Processor.DummyBISIDownloader.IsExpectingEmptyZipDirectory = false;
			Processor.DummyBISIDownloader.ThrowExceptionInDeleteRemoteFile = true;
			AssertEquals("No errors initially for the test", 0, Logger.Count);
			Processor.RunTask();
			AssertEquals("An FTP error should be reported", true, Logger.ToString().Contains("Could not delete an FTP file"));
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
			ProcessorTimeout.RunTask();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Service Task 'ZU1' has timed out", Env.OutgoingMailManager.EmailsCreated[0].Subject);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		#region Implementation
		protected override void SetUpCore()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUpCore();
			Processor.DummyBISIDownloader.PrepareZipFilesForTest();
			CreateHouseBills();
		}

		protected override void TearDownCore()
		{
			CargoWise.IO.TempDirectory.DeleteDirectory(Processor.DummyBISIDownloader.TempPath);
			if (Directory.Exists(Env.TempPath + "BISIUnzippedFiles"))
			{
				CargoWise.IO.TempDirectory.DeleteDirectory(Env.TempPath + "BISIUnzippedFiles");
			}

			base.TearDownCore();
		}

		BISIDownloadBatchProcessorForTest Processor
		{
			get
			{
				if (fProcessor == null)
				{
					fProcessor = new BISIDownloadBatchProcessorForTest(Logger);
				}

				return fProcessor;
			}
		}

		BISIDownloadBatchProcessorForTimeoutTest ProcessorTimeout
		{
			get
			{
				return new BISIDownloadBatchProcessorForTimeoutTest(Logger);
			}
		}

		TestServiceLogger Logger
		{
			get
			{
				return logger ?? (logger = new TestServiceLogger());
			}
		}

		TestServiceLogger logger;
		void CreateHouseBills()
		{
			UPECusMAWB = Factory.New<UPECusMAWB>();
			UPECusHAWB1 = (UPECusHAWB)UPECusMAWB.ChildBills.AddNew();
			UPECusHAWB1.CS_HAWB = "W6625665589";
			UPECusHAWB2 = (UPECusHAWB)UPECusMAWB.ChildBills.AddNew();
			UPECusHAWB2.CS_HAWB = "1Z298E686640623521";
			Factory.Save();
		}

		BISIDownloadBatchProcessorForTest fProcessor;
		UPECusMAWB UPECusMAWB;
		UPECusHAWB UPECusHAWB1;
		UPECusHAWB UPECusHAWB2;
		#region BISIDownloadBatchProcessorForTest
		class BISIDownloadBatchProcessorForTest : BISIDownloadServiceTask
		{
			public BISIDownloadBatchProcessorForTest(ILogger logger) : base(logger)
			{
			}

			public bool IsExpectingOneOfTheImportsToFail;
			public bool IsExpectingOneOfTheUnzipsToFail;
			protected override bool ImportCODFile(PWSFileImporter importer, string cODFile, INotifications notifications)
			{
				bool result;
				if (IsExpectingOneOfTheImportsToFail && cODFile.ToUpper().IndexOf("CODFILEINZIP1.TXT") > -1)
				{
					result = false;
				}
				else
				{
					result = base.ImportCODFile(importer, cODFile, notifications);
				}

				return result;
			}

			protected override bool ExtractZipFileCore(string zipFile)
			{
				bool result;
				if (IsExpectingOneOfTheUnzipsToFail && zipFile.ToUpper().IndexOf("CODZIPFILE2.ZIP") > -1)
				{
					result = false;
				}
				else
				{
					result = base.ExtractZipFileCore(zipFile);
				}

				return result;
			}

			protected override IBISIDownloader GetBISIDownloader()
			{
				return DummyBISIDownloader;
			}

			public DummyBISIDownloader DummyBISIDownloader
			{
				get
				{
					if (fDummyBISIDownloader == null)
					{
						fDummyBISIDownloader = new DummyBISIDownloader();
					}

					return fDummyBISIDownloader;
				}
			}

			DummyBISIDownloader fDummyBISIDownloader;
			#region BISIUploadWarningReporter
			internal TestBISIUploadWarningReporter BISIUploadWarningReporter
			{
				get
				{
					if (fBISIUploadWarningReporter == null)
					{
						fBISIUploadWarningReporter = new TestBISIUploadWarningReporter();
					}

					return fBISIUploadWarningReporter;
				}
			}

			TestBISIUploadWarningReporter fBISIUploadWarningReporter;
			protected override BISIUploadWarningReporter GetBISIUploadWarningReporter()
			{
				if (fBISIUploadWarningReporter == null)
				{
					fBISIUploadWarningReporter = new TestBISIUploadWarningReporter();
				}

				return fBISIUploadWarningReporter;
			}

			internal class TestBISIUploadWarningReporter : BISIUploadWarningReporter
			{
				public bool SendWarningEmailIfRequiredCalled;
				public void SendWarningEmailIfRequiredPublic(ZDateTime now, INotifications notifications)
				{
					base.SendWarningEmailIfRequired(now, notifications);
				}

				protected override void SendWarningEmailIfRequired(ZDateTime now, INotifications notifications)
				{
					SendWarningEmailIfRequiredCalled = true;
				}
			}
			#endregion
		}
		#endregion

		#region BISIDownloadBatchProcessorForTimeoutTest
		class  BISIDownloadBatchProcessorForTimeoutTest : BISIDownloadBatchProcessorForTest
		{
			public BISIDownloadBatchProcessorForTimeoutTest(ILogger logger) : base(logger)
			{
			}
			protected override IBISIDownloader GetBISIDownloader()
			{
				Thread.Sleep(10000);
				return DummyBISIDownloader;
			}
		}

		#endregion

		#endregion
		
	}
}
