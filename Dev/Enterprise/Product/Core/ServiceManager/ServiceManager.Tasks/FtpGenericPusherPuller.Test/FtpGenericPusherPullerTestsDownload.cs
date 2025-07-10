using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.FTP.Testing
{
	[TestedType(typeof(FtpGenericPusherPullerServiceTask))]
	sealed class FtpGenericPusherPullerTestsDownload : ServiceTaskTestCase<FtpGenericPusherPullerServiceTask>
	{
		public void TestMinimumPeriod()
		{
			AssertEquals("5minutes", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestDownload_FTPES()
		{
			using (var tempLocalDir = new TempDirectory())
			{
				helper.SetUpDownloadConfigs(tempLocalDir, Factory, FtpClobberingOptions.Codes.Clobber, prefix: "ftpes");
				var existingLocalTarget = Helper.MakeLocalSourceFile("DownLoad_Daniel.xml", "Destination", tempLocalDir);
				var goodRemoteSource = helper.MakeExistingRemoteFile("DownLoad_Daniel.xml", "Source");
				var ignoredRemoteSource = helper.MakeExistingRemoteFile("DownLoad_Something.xml", "SourceIgnore");
				var ftpTask = new FtpGenericPusherPullerServiceTask();
				var log = InitialiseAndRunTaskSchedule(ftpTask);
				AssertContains("CargoWise.IO.FtpException: Fail list directory", log[1]);
			}
		}

		public void TestDownloadClobberAndMask()
		{
			AssertDownloadClobberAndMask("ftp");
		}

		void AssertDownloadClobberAndMask(string prefix)
		{
			using (var tempLocalDir = new TempDirectory())
			{
				helper.SetUpDownloadConfigs(tempLocalDir, Factory, FtpClobberingOptions.Codes.Clobber, prefix: prefix);
				var existingLocalTarget = Helper.MakeLocalSourceFile("DownLoad_Daniel.xml", "Destination", tempLocalDir);
				var goodRemoteSource = helper.MakeExistingRemoteFile("DownLoad_Daniel.xml", "Source");
				var ignoredRemoteSource = helper.MakeExistingRemoteFile("DownLoad_Something.xml", "SourceIgnore");
				var ftpTask = new FtpGenericPusherPullerServiceTask();
				var log = InitialiseAndRunTaskSchedule(ftpTask);
				Helper.AssertFileExistsWithContent("File now exists on local folder", existingLocalTarget, "Source");
				Helper.AssertFileExistsWithContent("File should still exist on source, it was not touched", ignoredRemoteSource, "SourceIgnore");
				Helper.AssertFileNotExists("Source file no longer exists, it has been deleted after pulling", goodRemoteSource);
				AssertContains("Enumerating files ", log[0]);
				AssertContains("Downloading ", log[1]);
				AssertContains("Downloaded '" + Helper.RemoteSubFolder + "/DownLoad_Daniel.xml' to ", log[2]);
				AssertContains(@"tmp\DownLoad_Daniel.xml. Profile p123", log[2]);
				AssertContains("Deleted remote file " + Helper.RemoteSubFolder + "/DownLoad_Daniel.xml. Profile p123", log[3]);
			}
		}

		public void TestDownloadUniqueCheckKeep()
		{
			AssertDownloadUniqueCheckKeep("ftp");
		}

		void AssertDownloadUniqueCheckKeep(string prefix)
		{
			using (var tempLocalDir = new TempDirectory())
			{
				helper.SetUpDownloadConfigs(tempLocalDir, Factory, FtpClobberingOptions.Codes.UniqueCheckKeep, prefix: prefix);
				var existingLocalTarget = Helper.MakeLocalSourceFile("DownLoad_Daniel.xml", "Destination", tempLocalDir);
				var goodRemoteSource = helper.MakeExistingRemoteFile("DownLoad_Daniel.xml", "Source");
				var ftpTask = new FtpGenericPusherPullerServiceTask();
				var log = InitialiseAndRunTaskSchedule(ftpTask);
				Helper.AssertFileExistsWithContentPartialMatch("File now exists on local folder", existingLocalTarget + "_", "Source");
				Helper.AssertFileExistsWithContentPartialMatch("File now exists on local folder", existingLocalTarget, "Destination");
				Helper.AssertFileNotExists("Source file no longer exists, it has been deleted after pulling", goodRemoteSource);
				AssertContains("Downloaded '" + Helper.RemoteSubFolder + "/DownLoad_Daniel.xml' to ", log[2]);
				AssertContains("Deleted remote file " + Helper.RemoteSubFolder + "/DownLoad_Daniel.xml. Profile p123", log[3]);
			}
		}

		public void TestDownloadUniqueCheckNew()
		{
			AssertDownloadUniqueCheckNew("ftp");
		}

		void AssertDownloadUniqueCheckNew(string prefix)
		{
			using (var tempLocalDir = new TempDirectory())
			{
				helper.SetUpDownloadConfigs(tempLocalDir, Factory, FtpClobberingOptions.Codes.UniqueCheckNew, prefix: prefix);
				var existingLocalTarget = Helper.MakeLocalSourceFile("DownLoad_Daniel.xml", "Destination", tempLocalDir);
				var goodRemoteSource = helper.MakeExistingRemoteFile("DownLoad_Daniel.xml", "Source");
				var ftpTask = new FtpGenericPusherPullerServiceTask();
				var log = InitialiseAndRunTaskSchedule(ftpTask);
				var newLocalFileJustCreatedName = Path.Combine(tempLocalDir, "Transfer_");
				Helper.AssertFileExistsWithContentPartialMatch("File now exists on local folder", newLocalFileJustCreatedName, "Source");
				Helper.AssertFileNotExists("Source file no longer exists, it has been deleted after pulling", goodRemoteSource);
				Helper.AssertFileExistsWithContent("File should still exist on local folder", existingLocalTarget, "Destination");
				AssertContains("Downloaded '" + Helper.RemoteSubFolder + "/DownLoad_Daniel.xml' to ", log[2]);
				AssertContains(@".tmp\Transfer_", log[2]);
				AssertContains("Deleted remote file " + Helper.RemoteSubFolder + "/DownLoad_Daniel.xml. Profile p123", log[3]);
			}
		}

		public void TestDownloadUniqueNocheckKeep()
		{
			AssertDownloadUniqueNocheckKeep("ftp");
		}

		void AssertDownloadUniqueNocheckKeep(string prefix)
		{
			using (var tempLocalDir = new TempDirectory())
			{
				helper.SetUpDownloadConfigs(tempLocalDir, Factory, FtpClobberingOptions.Codes.UniqueNocheckKeep, prefix: prefix);
				var goodRemoteSource = helper.MakeExistingRemoteFile("DownLoad_Daniel.xml", "Source");
				var ftpTask = new FtpGenericPusherPullerServiceTask();
				var log = InitialiseAndRunTaskSchedule(ftpTask);
				Helper.AssertFileExistsWithContentPartialMatch("File now exists on local folder", Path.Combine(tempLocalDir, "DownLoad_Daniel.xml_"), "Source");
				Helper.AssertFileNotExists("Source file no longer exists, it has been deleted after pulling", goodRemoteSource);
				AssertContains("Downloaded '" + Helper.RemoteSubFolder + "/DownLoad_Daniel.xml' to ", log[2]);
				AssertContains(@".tmp\DownLoad_Daniel.xml_", log[2]);
				AssertContains("Deleted remote file " + Helper.RemoteSubFolder + "/DownLoad_Daniel.xml. Profile p123", log[3]);
			}
		}

		public void TestDownloadUniqueNocheckNew()
		{
			AssertDownloadUniqueNocheckNew("ftp");
		}

		void AssertDownloadUniqueNocheckNew(string prefix)
		{
			using (var tempLocalDir = new TempDirectory())
			{
				helper.SetUpDownloadConfigs(tempLocalDir, Factory, FtpClobberingOptions.Codes.UniqueNocheckNew, prefix: prefix);
				var goodRemoteSource = helper.MakeExistingRemoteFile("DownLoad_Daniel.xml", "Source");
				var ftpTask = new FtpGenericPusherPullerServiceTask();
				var log = InitialiseAndRunTaskSchedule(ftpTask);
				Helper.AssertFileExistsWithContentPartialMatch("File now exists on local folder", Path.Combine(tempLocalDir, "Transfer_"), "Source");
				Helper.AssertFileNotExists("Source file no longer exists, it has been deleted after pulling", goodRemoteSource);
				AssertContains("Downloaded '" + Helper.RemoteSubFolder + "/DownLoad_Daniel.xml' to ", log[2]);
				AssertContains(@".tmp\Transfer_", log[2]);
				AssertContains("Deleted remote file " + Helper.RemoteSubFolder + "/DownLoad_Daniel.xml. Profile p123", log[3]);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetUpCore()
		{
			base.SetUpCore();
			helper = new Helper();
			helper.Start();
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			helper.Dispose();
		}

		Helper helper;
	}
}
