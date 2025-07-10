using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.IO.Testing;
using CargoWise.Types;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.FTP.Testing
{
	sealed class Helper : FtpTestHelper
	{
		public Helper()
			: base(FtpUserName, FtpUserPassword)
		{ }

		internal static void AssertFileExistsWithContent(string message, string targetFileName, string expectedContent)
		{
			message = message + " " + targetFileName;
			TestCaseWithFactory.AssertEquals(message, true, File.Exists(targetFileName));
			TestCaseWithFactory.AssertEquals(message, expectedContent, File.ReadAllText(targetFileName));
		}

		internal static void AssertFileExistsWithContentPartialMatch(string message, string targetFilename, string expectedContent)
		{
			message = message + " " + targetFilename;
			var fileInfo = new FileInfo(targetFilename);
			var matchingFiles = fileInfo.Directory.GetFiles(fileInfo.Name + "*");
			TestCaseWithFactory.AssertEquals(true, matchingFiles.Length > 0);
			TestCaseWithFactory.AssertEquals(message, expectedContent, File.ReadAllText(matchingFiles[0].FullName));
		}

		internal static void AssertFileNotExists(string message, string localSource, TestServiceLogger logger = null)
		{
			message = message + " " + localSource;

			if (logger != null)
			{
				var allLogs = new ZStringBuilder();
				for (int i = 0; i < logger.Count; i++)
				{
					allLogs.AppendIfNotEmpty(logger[i]);
				}
				message += allLogs.ToStringWithNewLineBetweenAppends();
			}

			// The below is an expensive way of doing this, but File.Exists(localSource) incorrectly reports true sometimes.  Ghey. 
			var fileExists = true;
			try
			{
				var throwAway = File.ReadAllText(localSource);
			}
			catch (FileNotFoundException)
			{
				fileExists = false;
			}

			TestCaseWithFactory.AssertEquals(message, false, fileExists);
		}

		internal string MakeExistingRemoteFile(string fileName, string content)
		{
			var remoteDir = Path.Combine(LocalDirectory, RemoteSubFolder);
			var dir = new DirectoryInfo(remoteDir);
			if (!dir.Exists)
			{
				dir.Create();
			}
			var remotePath = Path.Combine(dir.FullName, fileName);
			File.WriteAllText(remotePath, content);
			return remotePath;
		}

		internal static string MakeLocalSourceFile(string fileName, string content, TempDirectory sourceDir)
		{
			var sourceFileName = Path.Combine(sourceDir.DirectoryName, fileName);
			File.WriteAllText(sourceFileName, content);
			return sourceFileName;
		}

		internal void SetUpUploadConfigs(string tempLocalDirName, BusinessObjectFactory factory, string clobberOrUnique, string deleteSourceOption = FtpArchiveOptions.Codes.Delete, string prefix = "ftp")
		{
			SetUpConfigs(tempLocalDirName, factory, FtpDirection.Codes.Push, clobberOrUnique, deleteSourceOption: deleteSourceOption, prefix: prefix);
		}

		internal void SetUpDownloadConfigs(string tempLocalDirName, BusinessObjectFactory factory, string clobberOrUnique, string deleteSourceOption = FtpArchiveOptions.Codes.Delete, string prefix = "ftp")
		{
			SetUpConfigs(tempLocalDirName, factory, FtpDirection.Codes.Pull, clobberOrUnique, deleteSourceOption: deleteSourceOption, prefix: prefix);
		}

		internal void SetUpConfigs(string tempLocalDirName, BusinessObjectFactory factory, string pushOrPull, string clobberOrUnique, string deleteSourceOption = FtpArchiveOptions.Codes.Delete, string emailAddress = "test@foo.com", bool useSubFolder = true, string prefix = "ftp")
		{
			Directory.CreateDirectory(Path.Combine(LocalDirectory, RemoteSubFolder));
			var profile = new FtpProfile(factory);
			profile.FriendlyName = "p123";
			profile.LocalFolder = tempLocalDirName;
			profile.RemoteLocation = prefix + "://localhost:" + Port;
			if (useSubFolder)
			{
				profile.RemoteLocation += "/" + RemoteSubFolder;
			}

			profile.RemotePassword = FtpUserPassword;
			profile.RemoteUsername = FtpUserName;
			profile.ClobberOrMakeUnique = clobberOrUnique;
			profile.FindFileMask = @"D.NI[aeiou]L\.xml";
			profile.DeleteSourceOption = deleteSourceOption;
			profile.PushOrPull = pushOrPull;
			profile.LastRunUtc = ZDateTime.Now.AddDays(-1);
			profile.RunPeriodSeconds = 600;
			profile.EmailAlertOnFailure = emailAddress;

			var configCollection = FtpRegistry.Instance.Profiles.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			configCollection.Add(profile);
			FtpRegistry.Instance.Profiles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configCollection);
			factory.Save();
		}

		internal static string RemoteSubFolder
		{
			get { return "UnitTesting.ServiceManagerTasksFtp_" + System.Environment.MachineName; }
		}

		const string FtpUserName = "ftpuser";
		const string FtpUserPassword = "ftppass";
	}
}
