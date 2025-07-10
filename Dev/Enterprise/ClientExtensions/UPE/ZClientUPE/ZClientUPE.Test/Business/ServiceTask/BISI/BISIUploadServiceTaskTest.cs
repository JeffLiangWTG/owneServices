using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.Business.BISI.Testing;
using Enterprise.Client.UPE.Business.Ftp;
using Enterprise.Client.UPE.Business.Ftp.Testing;
using Enterprise.Client.UPE.ServiceTask;
using Enterprise.Client.UPE.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.UPE.Business.ServiceTask
{
	[TestedType(typeof(BISIUploadServiceTask))]
	sealed class BISIUploadServiceTaskTest : ServiceTaskTestCase<BISIUploadServiceTask>
	{
		public void TestHumanReadableName()
		{
			var attributes = GetHostedServiceAttributes();
			AssertEquals("BISI Upload", attributes[0].Description);
		}

		public void TestDefaultSchedule()
		{
			AssertEquals("30minutes", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		[TestDate(2005, 01, 01, 05, 00, 00)]
		public void TestExecuteCore()
		{
			UPETestHelper.Rego.BISIUploadEverydayHWM = new ZDateTime(2005, 01, 01, 04, 30, 00);
			string archiveFolder = BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadCurrentBatchNumber = 200;
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccess;
			Assert("Pre-condition. There should be nothing in the Virtual FTP directory", !File.Exists(Processor.DummyFtpUploader.TargetFileFullPath));
			AssertEquals("Pre-condition. There should be nothing in the archive directory", 0, Directory.GetFiles(archiveFolder).Length);
			Assert("Pre-condition. SaveDateUploadedAndBISIUploadData method should not be called yet", !Processor.DummyBISIFileExporter.SaveDateUploadedAndBISIUploadDataCalled);

			Processor.RunTask();
			Assert("Temporary file should be deleted", !File.Exists(Processor.DummyBISIFileExporter.LastTargetFileName));
			AssertUploadedFileContent(200, new ZDateTime(2005, 01, 01, 04, 30, 00), ZDateTime.Now.AddSeconds(-60));
			AssertEquals("There should be one file archived", 1, Directory.GetFiles(archiveFolder).Length);
			Assert("SaveDateUploadedAndBISIUploadData method should be called", Processor.DummyBISIFileExporter.SaveDateUploadedAndBISIUploadDataCalled);
			AssertEquals("Batch number should be increased", 201, UPETestHelper.Rego.BISIUploadCurrentBatchNumber);
			AssertEquals("High water mark should be increased", ZDateTime.Now.AddSeconds(-60), UPETestHelper.Rego.BISIUploadCompletedHWM);

			Assert("Log starts with PID", TestHelper.Logger.ToString().StartsWith(string.Format("Information|Start BISI Uploading - PID: {0}", System.Diagnostics.Process.GetCurrentProcess().Id)));
		}

		[TestDate(2005, 01, 01, 05, 00, 00)]
		public void TestExecuteCore_ExportFails()
		{
			UPEDataRegistry.Instance.BISIUploadEverydayHWM = new ZDateTime(2005, 01, 01, 04, 30, 00);
			UPEDataRegistry.Instance.BISIUploadCurrentBatchNumber = 200;
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportFails;

			Processor.RunTask();
			Assert("Temporary file should be deleted", !File.Exists(Processor.DummyBISIFileExporter.LastTargetFileName));
			Assert("Export fails, file should not exist", !File.Exists(Processor.DummyFtpUploader.TargetFileFullPath));
			Assert("SaveDateUploadedAndBISIUploadData method should not be called if export fails", !Processor.DummyBISIFileExporter.SaveDateUploadedAndBISIUploadDataCalled);
			AssertEquals("Batch number should not be increased if export fails", 200, UPEDataRegistry.Instance.BISIUploadCurrentBatchNumber);
			AssertEquals("High water mark should not be increased if export fails", new ZDateTime(2005, 01, 01, 04, 30, 00), UPEDataRegistry.Instance.BISIUploadEverydayHWM);

			Assert("Log starts with PID", TestHelper.Logger.ToString().StartsWith(string.Format("Information|Start BISI Uploading - PID: {0}", System.Diagnostics.Process.GetCurrentProcess().Id)));
		}

		[TestDate(2005, 01, 01, 05, 00, 00)]
		public void TestExecuteCore_UploadFails()
		{
			UPETestHelper.Rego.BISIUploadEverydayHWM = new ZDateTime(2005, 01, 01, 04, 30, 00);
			UPETestHelper.Rego.BISIUploadCurrentBatchNumber = 200;
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccess;
			Processor.DummyFtpUploader.UploadToFtpServerShouldFail = true;

			Processor.RunTask();
			AssertEquals("Should retry twice before failing", 3, Processor.DummyFtpUploader.DoUploadCalledCounter);
			Assert("Temporary file should be deleted", !File.Exists(Processor.DummyBISIFileExporter.LastTargetFileName));
			Assert("Upload fails, file should not exist", !File.Exists(Processor.DummyFtpUploader.TargetFileFullPath));
			AssertEquals("Batch number should not be increased if upload fails", 200, UPETestHelper.Rego.BISIUploadCurrentBatchNumber);
			AssertEquals("High water mark should not be increased if upload fails", new ZDateTime(2005, 01, 01, 04, 30, 00), UPETestHelper.Rego.BISIUploadEverydayHWM);

			Assert("Log starts with PID", TestHelper.Logger.ToString().StartsWith(string.Format("Information|Start BISI Uploading - PID: {0}", System.Diagnostics.Process.GetCurrentProcess().Id)));
		}

		[TestDate(2005, 01, 01, 05, 00, 00)]
		public void TestExecuteCore_ExportSuccessButNoData()
		{
			UPETestHelper.Rego.BISIUploadEverydayHWM = new ZDateTime(2005, 01, 01, 04, 30, 00);
			UPETestHelper.Rego.BISIUploadCurrentBatchNumber = 200;
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccessButNoData;

			Processor.RunTask();
			Assert("Temporary file should be deleted", !File.Exists(Processor.DummyBISIFileExporter.LastTargetFileName));
			Assert("File should not exist, there is no data exported", !File.Exists(Processor.DummyFtpUploader.TargetFileFullPath));
			Assert("SaveDateUploadedAndBISIUploadData method should be called regardless of whether there is data exported", Processor.DummyBISIFileExporter.SaveDateUploadedAndBISIUploadDataCalled);
			AssertEquals("Batch number should not be increased if no data is exported", 200, UPETestHelper.Rego.BISIUploadCurrentBatchNumber);
			AssertEquals("High water mark should be increased if export sucess regardless of whether there is data exported", ZDateTime.Now.AddSeconds(-60), UPETestHelper.Rego.BISIUploadCompletedHWM);

			Assert("Log starts with PID", TestHelper.Logger.ToString().StartsWith(string.Format("Information|Start BISI Uploading - PID: {0}", System.Diagnostics.Process.GetCurrentProcess().Id)));
		}

		[TestDate(2005, 01, 01, 09, 31, 00)]
		public void TestExecuteCore_TimeChunkIsGreaterThanTheAllowedTimeBuffer()
		{
			UPETestHelper.Rego.BISIUploadTimeBuffer = 4;
			UPETestHelper.Rego.BISIUploadEverydayHWM = new ZDateTime(2005, 01, 01, 04, 30, 00);
			string archiveFolder = BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadCurrentBatchNumber = 200;
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccess;
			Assert("Pre-condition. There should be nothing in the Virtual FTP directory", !File.Exists(Processor.DummyFtpUploader.TargetFileFullPath));
			AssertEquals("Pre-condition. There should be nothing in the archive directory", 0, Directory.GetFiles(archiveFolder).Length);
			Assert("Pre-condition. SaveDateUploadedAndBISIUploadData method should not be called yet", !Processor.DummyBISIFileExporter.SaveDateUploadedAndBISIUploadDataCalled);

			UPETestHelper.Rego.ForceAllXPLDsToBeUploadedEveryTime = true;
			Processor.RunTask();
			Assert("Temporary file should be deleted", !File.Exists(Processor.DummyBISIFileExporter.LastTargetFileName));
			AssertUploadedFileContent(200, new ZDateTime(2005, 01, 01, 04, 30, 00), new ZDateTime(2005, 01, 01, 08, 30, 00));
			AssertEquals("There should be one file archived", 1, Directory.GetFiles(archiveFolder).Length);
			Assert("SaveDateUploadedAndBISIUploadData method should be called", Processor.DummyBISIFileExporter.SaveDateUploadedAndBISIUploadDataCalled);
			AssertEquals("Batch number should be increased", 201, UPETestHelper.Rego.BISIUploadCurrentBatchNumber);
			AssertEquals("High water mark should be increased", new ZDateTime(2005, 01, 01, 08, 30, 00), UPETestHelper.Rego.BISIUploadEverydayHWM);
		}

		public void TestExecuteCore_ShouldNotContinueProcessingIfCannotUpload()
		{
			Processor.DummyFtpUploader.TestCanUpload = false;
			Processor.RunTask();
			AssertNull("Should not be exporting file if FTP server is not ready", Processor.DummyBISIFileExporter.LastTargetFileName);
		}

		[TestDate(2005, 01, 01, 05, 00, 00)]
		public void TestRunTask_SavingFails()
		{
			string archiveFolder = BISIUploadArchiveDirectory();
			UPEDataRegistry.Instance.BISIUploadEverydayHWM = new ZDateTime(2005, 01, 01, 04, 30, 00);
			UPEDataRegistry.Instance.BISIUploadCurrentBatchNumber = 200;
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccess;
			Processor.DummyBISIFileExporter.SaveDateUploadedAndBISIUploadDataShouldFail = true;

			Processor.RunTask();
			Assert("Temporary file should be deleted", !File.Exists(Processor.DummyBISIFileExporter.LastTargetFileName));
			AssertUploadedFileContent(200, new ZDateTime(2005, 01, 01, 04, 30, 00), ZDateTime.Now.AddSeconds(-60));
			AssertEquals("There should be one file archived", 1, Directory.GetFiles(archiveFolder).Length);
			AssertEquals("Batch number should be increased even if saving fails (BISI does not accept two uploads with same batch number)", 201, UPEDataRegistry.Instance.BISIUploadCurrentBatchNumber);
			AssertEquals("High water mark should not be increased if saving fails", new ZDateTime(2005, 01, 01, 04, 30, 00), UPEDataRegistry.Instance.BISIUploadEverydayHWM);
			ErrorReporter.Clear();
		}

		[TestDate(2005, 01, 01, 05, 00, 00)]
		public void TestExecuteCore_ReportUploadedShipments()
		{
			UPETestHelper.Rego.BISIUploadEverydayHWM = new ZDateTime(2005, 01, 01, 04, 30, 00);
			BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadCurrentBatchNumber = 200;
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccess;

			Processor.RunTask();
			AssertEquals("2 shipments should have been reported as uploaded", true, Processor.LastShipmentsUploadedReporter.SendEmailIfRequiredCalled);
			AssertEquals("2 shipments should have been reported as uploaded", 2, Processor.LastShipmentsUploadedReporter.UploadedShipments.Count);
		}

		public void TestBranchUsedInUploadIsFromRegistry()
		{
			var defaultCompany = Factory.Load<GlbCompany>(Env.CurrentCompany.PK);
			var branchSYD = defaultCompany.Branches.First(x => x.GB_Code == "SYD");
			using (branchSYD.SetAsTemporaryContext())
			{
				var branchADL = defaultCompany.Branches.AddNew();
				branchADL.GB_Code = "ADL";
				branchADL.GB_RL_NKHomePort = "AUADL";
				var branchPER = defaultCompany.Branches.AddNew();
				branchPER.GB_Code = "PER";
				branchPER.GB_RL_NKHomePort = "AUPER";
				Factory.Save();

				var everydayHWM = SetEveryDayHWMWithBranchTime(branchPER);
				AssertZDatesWithin5Minutes("Check registry was set to Perth time", branchPER.HomePort.LocationDateTime, everydayHWM);

				UPEDataRegistry.Instance.BranchToUseForUPECustomisations = branchSYD.PK.ToGuid();
				BISIUploadArchiveDirectory();
				UPETestHelper.Rego.BISIUploadCurrentBatchNumber = 200;
				Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccess;
				Processor.RunTask();
				AssertZDatesWithin5Minutes("Registry now set as Sydney time", ZDateTime.Now, UPETestHelper.Rego.BISIUploadEverydayHWM);
				AssertContains("for company EDI and branch SYD in country AU", Processor.Logger.ToString());

				everydayHWM = SetEveryDayHWMWithBranchTime(branchPER);
				AssertZDatesWithin5Minutes("Check registry was set to Perth time", branchPER.HomePort.LocationDateTime, everydayHWM);
				UPEDataRegistry.Instance.BranchToUseForUPECustomisations = branchADL.PK.ToGuid();
				var directoryInfo = new DirectoryInfo(Processor.UploadBatchProcessorTestDir);
				foreach (var file in directoryInfo.GetFiles())
				{
					file.Delete();
				}
				Processor.RunTask();
				AssertZDatesWithin5Minutes("Check the registry is saved as Adelaide time", branchADL.HomePort.LocationDateTime, UPETestHelper.Rego.BISIUploadEverydayHWM);
				AssertContains("for company EDI and branch ADL in country AU", Processor.Logger.ToString());
			}
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
			TimeoutProcessor.RunTask();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Service Task 'ZU3' has timed out", Env.OutgoingMailManager.EmailsCreated[0].Subject);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();

		ZDateTime SetEveryDayHWMWithBranchTime(GlbBranch branch)
		{
			ZDateTime result;
			using (branch.SetAsTemporaryContext())
			{
				result = ZDateTime.Now;
				UPETestHelper.Rego.BISIUploadEverydayHWM = result;
			}
			return result;
		}

		#region Registry Values Get Updated after successful run

		#region Completed Run Tests

		[TestDate(2005, 1, 1, 5, 0, 0)]
		public void TestBISIUploadCompletedHWMCore_GetsUpdatedAfterRun()
		{
			BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadCompletedHWM = new ZDateTime(2005, 1, 1, 4, 30, 0);
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccessButNoData;

			UPETestHelper.Rego.ForceAllXPLDsToBeUploadedEveryTime = false;
			Processor.RunTask();
			AssertEquals(ZDateTime.Now.AddSeconds(-60), UPETestHelper.Rego.BISIUploadEverydayHWM);
		}
		#endregion

		#region Everyday Run Tests

		[TestDate(2005, 1, 1, 5, 0, 0)]
		public void TestBISIUploadEverydayHWMCore_GetsUpdatedAfterRun()
		{
			BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadEverydayHWM = new ZDateTime(2005, 1, 1, 4, 30, 0);
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccessButNoData;

			UPETestHelper.Rego.ForceAllXPLDsToBeUploadedEveryTime = false;
			Processor.RunTask();
			AssertEquals(ZDateTime.Now.AddSeconds(-60), UPETestHelper.Rego.BISIUploadEverydayHWM);
		}

		[TestDate(2005, 1, 2, 0, 1, 0)]
		public void TestBISIUploadEverydayDateOfArrivalHWMCore_GetsUpdatedAfterRun()
		{
			BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadEverydayDateOfArrivalHWM = new ZDateTime(2005, 1, 1);
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccessButNoData;

			UPETestHelper.Rego.ForceAllXPLDsToBeUploadedEveryTime = false;
			Processor.RunTask();
			AssertEquals(new ZDateTime(2005, 1, 2), UPETestHelper.Rego.BISIUploadEverydayDateOfArrivalHWM);
		}

		[TestDate(2005, 1, 2)]
		public void TestBISIUploadEverydayHWMCore_DoesntGetUpdatedAfterRun()
		{
			BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadEverydayDateOfArrivalHWM = new ZDateTime(2005, 1, 2);
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccessButNoData;

			UPETestHelper.Rego.ForceAllXPLDsToBeUploadedEveryTime = false;
			Processor.RunTask();
			AssertEquals(new ZDateTime(2005, 1, 2), UPETestHelper.Rego.BISIUploadEverydayDateOfArrivalHWM);
		}
		#endregion

		#region 1030 Run Tests

		[TestDate(2006, 2, 14, 9, 0, 0)]
		public void Test1030UploadCore_DoesntGetUpdatedBefore1030()
		{
			BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadWorkingDayMetroHWM = new ZDateTime(2006, 2, 13, 22, 1, 0);
			UPETestHelper.Rego.BISIUploadWorkingDayDateOfArrivalMetroHWM = new ZDateTime(2006, 2, 13);
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccessButNoData;

			UPETestHelper.Rego.ForceAllXPLDsToBeUploadedEveryTime = false;
			Processor.RunTask();
			AssertEquals(new ZDateTime(2006, 2, 13, 22, 1, 0), UPETestHelper.Rego.BISIUploadWorkingDayMetroHWM);
			AssertEquals(new ZDateTime(2006, 2, 13), UPETestHelper.Rego.BISIUploadWorkingDayDateOfArrivalMetroHWM);
		}

		[TestDate(2006, 2, 14, 11, 20, 1)]
		public void Test1030UploadCore_GetsUpdatedAfter1030()
		{
			BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadWorkingDayMetroHWM = new ZDateTime(2006, 2, 13, 22, 1, 0);
			UPETestHelper.Rego.BISIUploadWorkingDayDateOfArrivalMetroHWM = new ZDateTime(2006, 2, 13);
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccessButNoData;
			UPETestHelper.Rego.ForceAllXPLDsToBeUploadedEveryTime = false;
			Processor.RunTask();
			AssertEquals(ZDateTime.Now.AddSeconds(-60), UPETestHelper.Rego.BISIUploadWorkingDayMetroHWM);
			AssertEquals(ZDateTime.Now.Date, UPETestHelper.Rego.BISIUploadWorkingDayDateOfArrivalMetroHWM);
		}

		[TestDate(2006, 2, 14, 12, 31, 1)]
		public void Test1030UploadCore_DoesntGetUpdatedAfter1030_IfAlreadyRun()
		{
			BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadWorkingDayMetroHWM = new ZDateTime(2006, 2, 14, 10, 30, 0);
			UPETestHelper.Rego.BISIUploadWorkingDayDateOfArrivalMetroHWM = new ZDateTime(2006, 2, 14);
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccessButNoData;

			UPETestHelper.Rego.ForceAllXPLDsToBeUploadedEveryTime = false;
			Processor.RunTask();
			AssertEquals(new ZDateTime(2006, 2, 14, 10, 30, 0), UPETestHelper.Rego.BISIUploadWorkingDayMetroHWM);
			AssertEquals(new ZDateTime(2006, 2, 14), UPETestHelper.Rego.BISIUploadWorkingDayDateOfArrivalMetroHWM);
		}

		[TestDate(2006, 2, 11, 12, 31, 1)]
		public void Test1030UploadCore_DoesntGetUpdatedOnAWeekendOrHoliday()
		{
			BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadWorkingDayMetroHWM = new ZDateTime(2006, 2, 10, 22, 1, 0);
			UPETestHelper.Rego.BISIUploadWorkingDayDateOfArrivalMetroHWM = new ZDateTime(2006, 2, 10);
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccessButNoData;

			UPETestHelper.Rego.ForceAllXPLDsToBeUploadedEveryTime = false;
			Processor.RunTask();
			AssertEquals(new ZDateTime(2006, 2, 10, 22, 1, 0), UPETestHelper.Rego.BISIUploadWorkingDayMetroHWM);
			AssertEquals(new ZDateTime(2006, 2, 10), UPETestHelper.Rego.BISIUploadWorkingDayDateOfArrivalMetroHWM);
		}

		#endregion

		#region 1630 Run Tests

		[TestDate(2006, 2, 14, 9, 0, 0)]
		public void Test1630UploadCore_DoesntGetUpdatedBefore1630()
		{
			BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadWorkingDayOtherHWM = new ZDateTime(2006, 2, 13, 22, 1, 0);
			UPETestHelper.Rego.BISIUploadWorkingDayDateOfArrivalOtherHWM = new ZDateTime(2006, 2, 13);
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccessButNoData;

			UPETestHelper.Rego.ForceAllXPLDsToBeUploadedEveryTime = false;
			Processor.RunTask();
			AssertEquals(new ZDateTime(2006, 2, 13, 22, 1, 0), UPETestHelper.Rego.BISIUploadWorkingDayOtherHWM);
			AssertEquals(new ZDateTime(2006, 2, 13), UPETestHelper.Rego.BISIUploadWorkingDayDateOfArrivalOtherHWM);
		}

		[TestDate(2006, 2, 14, 17, 1, 1)]
		public void Test1630UploadCore_GetsUpdatedAfter1630()
		{
			BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadWorkingDayOtherHWM = new ZDateTime(2006, 2, 13, 22, 1, 0);
			UPETestHelper.Rego.BISIUploadWorkingDayDateOfArrivalOtherHWM = new ZDateTime(2006, 2, 13);
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccessButNoData;

			UPETestHelper.Rego.ForceAllXPLDsToBeUploadedEveryTime = false;
			Processor.RunTask();
			AssertEquals(ZDateTime.Now.AddSeconds(-60), UPETestHelper.Rego.BISIUploadWorkingDayOtherHWM);
			AssertEquals(ZDateTime.Now.Date, UPETestHelper.Rego.BISIUploadWorkingDayDateOfArrivalOtherHWM);
		}

		[TestDate(2006, 2, 14, 12, 30, 1)]
		public void Test1630UploadCore_DoesntGetUpdatedAfter1630_IfAlreadyRun()
		{
			BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadWorkingDayOtherHWM = new ZDateTime(2006, 2, 14, 16, 30, 0);
			UPETestHelper.Rego.BISIUploadWorkingDayDateOfArrivalOtherHWM = new ZDateTime(2006, 2, 13);
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccessButNoData;

			UPETestHelper.Rego.ForceAllXPLDsToBeUploadedEveryTime = false;
			Processor.RunTask();
			AssertEquals(new ZDateTime(2006, 2, 14, 16, 30, 0), UPETestHelper.Rego.BISIUploadWorkingDayOtherHWM);
			AssertEquals(new ZDateTime(2006, 2, 13), UPETestHelper.Rego.BISIUploadWorkingDayDateOfArrivalOtherHWM);
		}

		[TestDate(2006, 2, 11, 12, 30, 1)]
		public void Test1630UploadCore_DoesntGetUpdatedOnAWeekendOrHoliday()
		{
			BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadWorkingDayOtherHWM = new ZDateTime(2006, 2, 10, 22, 1, 0);
			UPETestHelper.Rego.BISIUploadWorkingDayDateOfArrivalOtherHWM = new ZDateTime(2006, 2, 10);
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccessButNoData;

			UPETestHelper.Rego.ForceAllXPLDsToBeUploadedEveryTime = false;
			Processor.RunTask();
			AssertEquals(new ZDateTime(2006, 2, 10, 22, 1, 0), UPETestHelper.Rego.BISIUploadWorkingDayOtherHWM);
			AssertEquals(new ZDateTime(2006, 2, 10), UPETestHelper.Rego.BISIUploadWorkingDayDateOfArrivalOtherHWM);
		}

		#endregion

		#region 2200 Run Tests

		[TestDate(2006, 2, 14, 18, 0, 0)]
		public void Test2200UploadCore_DoesntGetUpdatedBefore2200()
		{
			BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadWorkingDayMetroHWM = new ZDateTime(2006, 2, 14, 10, 32, 0);
			UPETestHelper.Rego.BISIUploadWorkingDayOtherHWM = new ZDateTime(2006, 2, 14, 16, 32, 0);
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccessButNoData;

			UPETestHelper.Rego.ForceAllXPLDsToBeUploadedEveryTime = false;
			Processor.RunTask();
			AssertEquals(new ZDateTime(2006, 2, 14, 10, 32, 0), UPETestHelper.Rego.BISIUploadWorkingDayMetroHWM);
			AssertEquals(new ZDateTime(2006, 2, 14, 16, 32, 0), UPETestHelper.Rego.BISIUploadWorkingDayOtherHWM);
		}

		[TestDate(2006, 2, 14, 22, 01, 0)]
		public void Test2200UploadCore_GetsUpdatedAfter2200()
		{
			BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadWorkingDayMetroHWM = new ZDateTime(2006, 2, 13, 10, 31, 0);
			UPETestHelper.Rego.BISIUploadWorkingDayOtherHWM = new ZDateTime(2006, 2, 13, 16, 31, 0);
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccessButNoData;

			UPETestHelper.Rego.ForceAllXPLDsToBeUploadedEveryTime = false;
			Processor.RunTask();
			AssertEquals(ZDateTime.Now.AddSeconds(-60), UPETestHelper.Rego.BISIUploadWorkingDayMetroHWM);
			AssertEquals(ZDateTime.Now.AddSeconds(-60), UPETestHelper.Rego.BISIUploadWorkingDayOtherHWM);
		}

		[TestDate(2006, 2, 14, 22, 30, 1)]
		public void Test2200UploadCore_DoesntGetUpdatedAfter2200_IfAlreadyRun()
		{
			BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadWorkingDayMetroHWM = new ZDateTime(2006, 2, 14, 22, 1, 0);
			UPETestHelper.Rego.BISIUploadWorkingDayOtherHWM = new ZDateTime(2006, 2, 14, 22, 1, 0);
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccessButNoData;

			UPETestHelper.Rego.ForceAllXPLDsToBeUploadedEveryTime = false;
			Processor.RunTask();
			AssertEquals(new ZDateTime(2006, 2, 14, 22, 1, 0), UPETestHelper.Rego.BISIUploadWorkingDayMetroHWM);
			AssertEquals(new ZDateTime(2006, 2, 14, 22, 1, 0), UPETestHelper.Rego.BISIUploadWorkingDayOtherHWM);
		}

		[TestDate(2006, 2, 11, 22, 1, 0)]
		public void Test2200UploadCore_DoesntGetUpdatedOnAWeekendOrHoliday()
		{
			BISIUploadArchiveDirectory();
			UPETestHelper.Rego.BISIUploadWorkingDayMetroHWM = new ZDateTime(2006, 2, 13, 10, 31, 0);
			UPETestHelper.Rego.BISIUploadWorkingDayOtherHWM = new ZDateTime(2006, 2, 13, 16, 31, 0);
			Processor.DummyBISIFileExporter.ExpectedExportResult = BISIExportResult.ExportSuccessButNoData;

			UPETestHelper.Rego.ForceAllXPLDsToBeUploadedEveryTime = false;
			Processor.RunTask();
			AssertEquals(new ZDateTime(2006, 2, 13, 10, 31, 0), UPETestHelper.Rego.BISIUploadWorkingDayMetroHWM);
			AssertEquals(new ZDateTime(2006, 2, 13, 16, 31, 0), UPETestHelper.Rego.BISIUploadWorkingDayOtherHWM);
		}

		#endregion

		string BISIUploadArchiveDirectory()
		{
			string result = Path.Combine(Processor.UploadBatchProcessorTestDir, "ARCHIVE");
			Directory.CreateDirectory(result);
			UPETestHelper.Rego.BISIUploadArchiveDirectory = result;
			return result;
		}

		#endregion

		#region Test Classes

		 class BISIUploadBatchProcessorForTest : BISIUploadServiceTask
		{
			public BISIUploadBatchProcessorForTest(ILogger logger)
				: base(logger)
			{
			}

			protected override IBISIFileExporter BISIFileExporter
			{
				get { return DummyBISIFileExporter; }
			}

			protected override FtpUploader BISIUploader
			{
				get { return DummyFtpUploader; }
			}

			public DummyBISIFileExporter DummyBISIFileExporter
			{
				get { return dummyBISIFileExporter ?? (dummyBISIFileExporter = new DummyBISIFileExporter()); }
			}
			DummyBISIFileExporter dummyBISIFileExporter;

			public DummyFtpUploader DummyFtpUploader
			{
				get { return dummyFtpUploader ?? (dummyFtpUploader = new DummyFtpUploader(UploadBatchProcessorTestDir, UPETestHelper.Rego.BISIUploadArchiveDirectory, ServiceLogger.GetTaskNotificationSubscriber())); }
			}
			DummyFtpUploader dummyFtpUploader;

			public string UploadBatchProcessorTestDir
			{
				get { return Path.Combine(Env.TempPath, uploadBatchTestFolder); }
			}
			const string uploadBatchTestFolder = "UploadBatchTesting";

			protected override BISIShipmentsUploadedReporter NewShipmentsUploadedReporter(IReadOnlyList<IShipmentData> uploadedShipments, int bISIUploadCurrentBatchNumber)
			{
				LastShipmentsUploadedReporter = new TestBISIShipmentsUploadedReporter(uploadedShipments, bISIUploadCurrentBatchNumber);
				return LastShipmentsUploadedReporter;
			}

			public TestBISIShipmentsUploadedReporter LastShipmentsUploadedReporter;
		}

		sealed class  BISIUploadBatchProcessorForTimeoutTest : BISIUploadBatchProcessorForTest
		{
			public BISIUploadBatchProcessorForTimeoutTest(ILogger logger) : base(logger)
			{
			}

			protected override FtpUploader BISIUploader
			{
				get
				{
					Thread.Sleep(10000);
					return DummyFtpUploader;
				}
			}
		}

		sealed class TestBISIShipmentsUploadedReporter : BISIShipmentsUploadedReporter
		{
			public TestBISIShipmentsUploadedReporter(IReadOnlyList<IShipmentData> uploadedShipments, int bISIUploadCurrentBatchNumber)
				: base(uploadedShipments, bISIUploadCurrentBatchNumber)
			{
			}

			public bool SendEmailIfRequiredCalled;

			public override void SendEmailIfRequired(INotifications notifications)
			{
				SendEmailIfRequiredCalled = true;
			}
		}

		#endregion

		protected override void SetUpCore()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			UPEDataRegistry.Instance.BranchToUseForUPECustomisations = Env.CurrentBranchPK;

			base.SetUpCore();
			PrepareTempDir();
		}

		protected override void TearDownCore()
		{
			CleanTempDir();
			base.TearDownCore();
		}

		void PrepareTempDir()
		{
			if (!Directory.Exists(Processor.UploadBatchProcessorTestDir))
			{
				Directory.CreateDirectory(Processor.UploadBatchProcessorTestDir);
			}
			DeleteDirectoryContentRecursively(Processor.UploadBatchProcessorTestDir);
		}

		void DeleteDirectoryContentRecursively(string path)
		{
			if (Directory.Exists(path))
			{
				foreach (string fileName in Directory.GetFiles(path))
				{
					File.SetAttributes(fileName, ~FileAttributes.ReadOnly);
					File.Delete(fileName);
				}

				foreach (string directoryName in Directory.GetDirectories(path))
				{
					TempDirectory.DeleteDirectory(directoryName);
				}
			}
		}

		void CleanTempDir()
		{
			TempDirectory.DeleteDirectory(Processor.UploadBatchProcessorTestDir);
		}

		void AssertUploadedFileContent(int batchNumber, ZDateTime startTime, ZDateTime endTime)
		{
			Assert("Target file should exist in the virtual FTP directory", File.Exists(Processor.DummyFtpUploader.TargetFileFullPath));
			using (StreamReader reader = File.OpenText(Processor.DummyFtpUploader.TargetFileFullPath))
			{
				string[] content = reader.ReadToEnd().Trim().Split(',');
				AssertEquals("Invalid test file content", 3, content.Length);
				AssertEquals(batchNumber, int.Parse(content[0]));
				ZDateTime generatedStartTime;
				ZDateTime generatedEndTime;
				ZDateTime.TryParseExact(content[1], out generatedStartTime, "yyyyMMddHHmmss");
				ZDateTime.TryParseExact(content[2], out generatedEndTime, "yyyyMMddHHmmss");
				AssertEquals(startTime, generatedStartTime);
				AssertEquals(endTime, generatedEndTime);
			}
		}

		BISIUploadBatchProcessorForTest Processor
		{
			get { return processor ?? (processor = new BISIUploadBatchProcessorForTest(TestHelper.Logger)); }
		}
		BISIUploadBatchProcessorForTest processor;

		BISIUploadBatchProcessorForTimeoutTest TimeoutProcessor
		{
			get { return timeoutProcessor ?? (timeoutProcessor = new BISIUploadBatchProcessorForTimeoutTest(TestHelper.Logger)); }
		}
		BISIUploadBatchProcessorForTimeoutTest timeoutProcessor;

		UPETestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new UPETestHelper()); }
		}
		UPETestHelper testHelper;
	}
}
