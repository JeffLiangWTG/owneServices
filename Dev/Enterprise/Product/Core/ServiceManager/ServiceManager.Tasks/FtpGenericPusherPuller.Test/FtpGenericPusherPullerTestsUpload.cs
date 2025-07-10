using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.FTP.Testing
{
	[TestedType(typeof(FtpGenericPusherPullerServiceTask))]
	sealed class FtpGenericPusherPullerTestsUpload : ServiceTaskTestCase<FtpGenericPusherPullerServiceTask>
	{
		public void TestMinimumPeriod()
		{
			AssertEquals("5minutes", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestUploadClobber()
		{
			using (var tempLocalDir = new TempDirectory())
			{
				helper.SetUpUploadConfigs(tempLocalDir, Factory, FtpClobberingOptions.Codes.Clobber);
				var goodLocalSource = Helper.MakeLocalSourceFile("Upload_Daniel.xml", "Source", tempLocalDir);
				var ignoredLocalSource = Helper.MakeLocalSourceFile("Upload_Something.xml", "SourceIgnore", tempLocalDir);
				var remoteExistingTarget = helper.MakeExistingRemoteFile("Upload_Daniel.xml", "Destination");
				var ftpTask = new FtpGenericPusherPullerServiceTask();
				var log = InitialiseAndRunTaskSchedule(ftpTask);
				Helper.AssertFileExistsWithContent("File should now exist on remote server", remoteExistingTarget, "Source");
				Helper.AssertFileExistsWithContent("File shoudl still exist on source, it was not touched", ignoredLocalSource, "SourceIgnore");
				Helper.AssertFileNotExists("Source file should no longer exist, it has been deleted after upload", goodLocalSource);
				AssertContains("Deleted local file Upload_Daniel.xml", log[1]);
			}
		}

		/// <summary>
		/// FTP account can be set to use a subfolder as default folder after log on although the FTP address is the root address. E.g. FTP host address is ftp.abc.com, but after log on
		/// it automatically goes to ftp.abc.com/subfolder/. In this case it will return error 550 when there are multiple forward slash in the path(E.g. ftp.abc.com///a.txt), which is
		/// OK when the FTP account is not set to this way. In this case it only works when the path is ftp.abc.com/a.txt
		/// </summary>
		public void TestUploadWhenUploadingToRootFolder()
		{
			using (var tempLocalDir = new TempDirectory())
			{
				helper.SetUpConfigs(tempLocalDir, Factory, FtpDirection.Codes.Push, FtpClobberingOptions.Codes.Clobber, FtpArchiveOptions.Codes.Delete, "test@foo.com", false);
				var goodLocalSource = Helper.MakeLocalSourceFile("Upload_Daniel.xml", "Source", tempLocalDir);
				var remoteFileTarget = Path.Combine(helper.LocalDirectory, "Upload_Daniel.xml");
				var ftpTask = new FtpGenericPusherPullerServiceTask();
				var log = InitialiseAndRunTaskSchedule(ftpTask);

				Helper.AssertFileNotExists("Source file should no longer exist, it has been deleted after upload", goodLocalSource);
				Helper.AssertFileExistsWithContent("File should now exist on remote server", remoteFileTarget, "Source");
				AssertContains("Information|Uploaded Upload_Daniel.xml as Upload_Daniel.xml.", log[0]);

				File.Delete(remoteFileTarget);//Clean up the test file.
			}
		}

		public void TestUploadFindFilenamesAndArchive()
		{
			using (var tempLocalDir = new TempDirectory())
			{
				helper.SetUpUploadConfigs(tempLocalDir, Factory, FtpClobberingOptions.Codes.Clobber, FtpArchiveOptions.Codes.Move);
				var goodLocalSource = Helper.MakeLocalSourceFile("Upload_Daniel.xml", "Source", tempLocalDir);
				var ignoredLocalSource = Helper.MakeLocalSourceFile("Upload_Something.xml", "SourceIgnore", tempLocalDir);
				var ftpTask = new FtpGenericPusherPullerServiceTask();
				var log = InitialiseAndRunTaskSchedule(ftpTask);
				Helper.AssertFileExistsWithContent("File should still exist on source, it was not touched", ignoredLocalSource, "SourceIgnore");
				Helper.AssertFileNotExists("Source file should no longer exist, it has been move after upload", goodLocalSource);
				Helper.AssertFileExistsWithContent("Source file archived", Path.Combine(tempLocalDir, "archive", "Upload_Daniel.xml"), "Source");
				AssertContains("Uploaded Upload_Daniel.xml as " + Helper.RemoteSubFolder + "/Upload_Daniel.xml. Profile p123", log[0]);
				AssertContains("Archived local file Upload_Daniel.xml. Profile p123", log[1]);
			}
		}

		public void TestUploadUniqueNocheckKeep()
		{
			using (var tempLocalDir = new TempDirectory())
			{
				helper.SetUpUploadConfigs(tempLocalDir, Factory, FtpClobberingOptions.Codes.UniqueNocheckKeep);
				var goodLocalSource = Helper.MakeLocalSourceFile("Upload_Daniel.xml", "Source", tempLocalDir);
				var ftpTask = new FtpGenericPusherPullerServiceTask();
				var log = InitialiseAndRunTaskSchedule(ftpTask);
				Helper.AssertFileNotExists("Source file should no longer exist, it has been deleted after upload", goodLocalSource);
				var newRemoteFileJustCreatedName = Path.Combine(helper.LocalDirectory, Helper.RemoteSubFolder, "Upload_Daniel.xml") + "_";
				Helper.AssertFileExistsWithContentPartialMatch("File now exists on remote server", newRemoteFileJustCreatedName, "Source");
				AssertContains("Uploaded Upload_Daniel.xml as " + Helper.RemoteSubFolder + "/Upload_Daniel.xml", log[0]);
				AssertNotContains("Uploaded Upload_Daniel.xml as /" + Helper.RemoteSubFolder + "/Upload_Daniel.xml. ", log[0]);
			}
		}

		public void TestUploaUniqueNocheckNew()
		{
			using (var tempLocalDir = new TempDirectory())
			{
				helper.SetUpUploadConfigs(tempLocalDir, Factory, FtpClobberingOptions.Codes.UniqueNocheckNew);
				var goodLocalSource = Helper.MakeLocalSourceFile("Upload_Daniel.xml", "Source", tempLocalDir);
				var ftpTask = new FtpGenericPusherPullerServiceTask();
				var log = InitialiseAndRunTaskSchedule(ftpTask);
				Helper.AssertFileNotExists("Source file should no longer exist, it has been deleted after upload", goodLocalSource);
				var newRemoteFileJustCreated = new DirectoryInfo(Path.Combine(helper.LocalDirectory, Helper.RemoteSubFolder)).GetFiles().Single(file => file.Name != "Upload_Daniel.xml");
				Helper.AssertFileExistsWithContentPartialMatch("File now exists on remote server", newRemoteFileJustCreated.FullName, "Source");
				AssertContains("Uploaded Upload_Daniel.xml to folder /" + Helper.RemoteSubFolder + " uniquely.", log[0]);
			}
		}

		public void TestUploadUniqueCheckKeep()
		{
			using (var tempLocalDir = new TempDirectory())
			{
				helper.SetUpUploadConfigs(tempLocalDir, Factory, FtpClobberingOptions.Codes.UniqueCheckKeep);
				var goodLocalSource = Helper.MakeLocalSourceFile("Upload_Daniel.xml", "Source", tempLocalDir);
				var remoteExistingTarget = helper.MakeExistingRemoteFile("Upload_Daniel.xml", "Destination");
				var ftpTask = new FtpGenericPusherPullerServiceTask();
				var log = InitialiseAndRunTaskSchedule(ftpTask);
				Helper.AssertFileNotExists("Source file should no longer exist, it has been deleted after upload", goodLocalSource);
				Helper.AssertFileExistsWithContent("Existing destination file untouched on remote server", remoteExistingTarget, "Destination");
				Helper.AssertFileExistsWithContentPartialMatch("File now exists on remote server", remoteExistingTarget + "_", "Source");
				AssertContains("Checked remote path /" + Helper.RemoteSubFolder + " for existing file Upload_Daniel.xml, found.", log[0]);
				AssertContains("Uploaded Upload_Daniel.xml as " + Helper.RemoteSubFolder + "/Upload_Daniel.xml_", log[1]);
				AssertContains("Deleted local file Upload_Daniel.xml", log[2]);
			}
		}

		public void TestUploadUniqueCheckNew()
		{
			using (var tempLocalDir = new TempDirectory())
			{
				helper.SetUpUploadConfigs(tempLocalDir, Factory, FtpClobberingOptions.Codes.UniqueCheckNew);
				var goodLocalSource = Helper.MakeLocalSourceFile("Upload_Daniel.xml", "Source", tempLocalDir);
				var remoteExistingTarget = helper.MakeExistingRemoteFile("Upload_Daniel.xml", "Destination");
				var ftpTask = new FtpGenericPusherPullerServiceTask();
				var log = InitialiseAndRunTaskSchedule(ftpTask);
				Helper.AssertFileNotExists("Source file should no longer exist, it has been deleted after upload", goodLocalSource);
				Helper.AssertFileExistsWithContent("Existing destination file untouched on remote server", remoteExistingTarget, "Destination");
				var newRemoteFileJustCreated = new DirectoryInfo(Path.Combine(helper.LocalDirectory, Helper.RemoteSubFolder)).GetFiles().Single(file => file.Name != "Upload_Daniel.xml");
				Helper.AssertFileExistsWithContentPartialMatch("File now exists on remote server", newRemoteFileJustCreated.FullName, "Source");
				AssertContains("Checked remote path /" + Helper.RemoteSubFolder + " for existing file Upload_Daniel.xml, found.", log[0]);
				AssertContains("Uploaded Upload_Daniel.xml to folder /" + Helper.RemoteSubFolder + " uniquely", log[1]);
				AssertContains("Deleted local file Upload_Daniel.xml", log[2]);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();

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
