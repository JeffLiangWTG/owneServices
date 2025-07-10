using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using CargoWise.IO.Testing.SharpFtpServer;
using NUnit.Framework;

namespace CargoWise.IO.Testing
{
	public class FtpProcessorTest : TestCase
	{
		public void TestFtpProcess_PentantExplicitFtp()
		{
			var ftpesProcessor = new FtpProcessorForTest("ftpes://1244EDE2211B4786MarcusJiang94422A3BB3AFEACC", "aaa", "bbb", readTimeout, connectTimeout, usePassive: true);
			AssertEquals(false, ftpesProcessor.UseSecure);
			AssertNoExceptionThrown("", () => ftpesProcessor.AffixProtocolToServerNameAndAppendForwardSlashIfNeeded());
			AssertEquals(true, ftpesProcessor.UseSecure);
			var request = ftpesProcessor.GetNewFtpRequest("ftp://1244EDE2211B4786MarcusJiang94422A3BB3AFEACC", WebRequestMethods.Ftp.UploadFile);
			AssertEquals(true, request.EnableSsl);

			var ftpsProcessor = new FtpProcessorForTest("ftps://1244EDE2211B4786MarcusJiang94422A3BB3AFEACC", "aaa", "bbb", readTimeout, connectTimeout, usePassive: true);
			AssertEquals(false, ftpsProcessor.UseSecure);
			AssertNoExceptionThrown(() => ftpsProcessor.AffixProtocolToServerNameAndAppendForwardSlashIfNeeded());
			AssertEquals(true, ftpsProcessor.UseSecure);
			var request2 = ftpsProcessor.GetNewFtpRequest("ftp://1244EDE2211B4786MarcusJiang94422A3BB3AFEACC", WebRequestMethods.Ftp.UploadFile);
			AssertEquals(true, request2.EnableSsl);
		}

		public void TestFtpProcess_NoErrorLogConstructor_UsePassiveDefault()
		{
			var ftpProcessor = new FtpProcessor(ServerName, userName, password, readTimeout, connectTimeout);
			AssertUsePassive(ftpProcessor, true);
		}

		public void TestFtpProcess_NoErrorLogConstructor_NotUsePassive()
		{
			var ftpProcessor = new FtpProcessor(ServerName, userName, password, readTimeout, connectTimeout, false);
			AssertUsePassive(ftpProcessor, false);
		}

		public void TestFtpProcess_ErrorLogConstructor_UsePassiveDefault()
		{
			var ftpProcessor = new FtpProcessor(ServerName, userName, password, null, readTimeout, connectTimeout);
			AssertUsePassive(ftpProcessor, true);
		}

		public void TestFtpProcess_ErrorLogConstructor_NotUsePassive()
		{
			var ftpProcessor = new FtpProcessor(ServerName, userName, password, null, readTimeout, connectTimeout, false);
			AssertUsePassive(ftpProcessor, false);
		}

		#region List Directory

		protected override void SetUp()
		{
			base.SetUp();
			ftpTestHelper = new FtpTestHelper(userName, password);
			ftpTestHelper.Start();
			Directory.CreateDirectory(Path.Combine(ftpTestHelper.LocalDirectory, ftpFolderName));
		}

		protected override void TearDown()
		{
			ftpTestHelper.Dispose();
			base.TearDown();
		}

		FtpTestHelper ftpTestHelper;
		const string ftpFolderName = "Test";
		const string userName = "testuser";
		const string password = "testpwd";
		static readonly TimeSpan readTimeout = new TimeSpan(0, 5, 0);
		static readonly TimeSpan connectTimeout = new TimeSpan(0, 6, 0);

		string ServerName
		{
			get { return "localhost:" + ftpTestHelper.Port; }
		}

		public void TestListDirectory()
		{
			string remoteFile = RemoteFile;
			string remoteFileName = Path.GetFileName(remoteFile);

			CreateFile(remoteFile, "download from remote file");
			try
			{
				string[] result = Processor.ListDirectory(ftpFolderName);
				AssertCollectionContains(remoteFileName, result);
				string[] result2 = Processor.ListDirectory("/" + ftpFolderName);
				AssertCollectionContains(remoteFileName, result2);
			}
			finally
			{
				DeleteIfExists(remoteFile);
			}

			FtpProcessor exProcessor = new FtpProcessorWithException("sadfasd", ServerName, userName, password, FtpStatusCode.ActionNotTakenFileUnavailable, "550 outbound/*: No such file or directory.");
			try
			{
				string[] result = exProcessor.ListDirectory(ftpFolderName);
				AssertEquals(0, result.Length);
			}
			catch
			{
				Fail("Should be no exception");
			}

			exProcessor = new FtpProcessorWithException("sadfasd", FtpStatusCode.ActionAbortedUnknownPageType, "550 outbound/*: No such file or directory.");
			try
			{
				string[] result = exProcessor.ListDirectory(ftpFolderName);
				Fail("Exception must be thrown");
			}
			catch
			{
			}

			exProcessor = new FtpProcessorWithException("sadfasd", FtpStatusCode.ActionNotTakenFileUnavailable, "550 outbound/*: No1 such file or directory.");
			try
			{
				string[] result = exProcessor.ListDirectory(ftpFolderName);
				Fail("Exception must be thrown");
			}
			catch
			{
			}
		}

		#endregion

		#region Download

		public void TestTimeoutIsSet()
		{
			AssertEquals("Read Timeout in milliseconds is incorrect", 300000, Processor.ReadTimeout.TotalMilliseconds, 100);
			AssertEquals("Connection Timeout in milliseconds is incorrect", 360000, Processor.ConnectTimeout.TotalMilliseconds, 100);
			AssertEquals("Read Timeout in minutes is incorrect", (double)5, Processor.ReadTimeout.TotalMinutes);
			AssertEquals("Connection Timeout in minutes is incorrect", (double)6, Processor.ConnectTimeout.TotalMinutes);
		}

		public void TestDownloadFileWithSpecialCharacters()
		{
			string[] invalidChars = { "#", "%", "'" };
			string localFile = "";
			string remoteFile = "";

			localFile = Path.Combine(Temp.TempPath, invalidChars[0] + Guid.NewGuid().ToString() + invalidChars[1] + ".txt");
			remoteFile = Path.Combine(Path.Combine(ftpTestHelper.LocalDirectory, ftpFolderName), invalidChars[0] + Guid.NewGuid().ToString() + invalidChars[1] + ".txt");

			string remoteFileName = Path.GetFileName(remoteFile);

			CreateFile(remoteFile, "download from remote file");
			Assert("Local file should not exist", !File.Exists(localFile));
			try
			{
				Processor.DownloadFile(localFile, Path.Combine(ftpFolderName, remoteFileName));
				Assert("File should have been downloaded", File.Exists(localFile));
				AssertASCIIFileSameAsString(remoteFile, "download from remote file");
			}
			finally
			{
				DeleteIfExists(remoteFile);
				DeleteIfExists(localFile);
			}
		}

		public void TestDownload()
		{
			string localFile = LocalFile;
			string remoteFile = RemoteFile;
			string remoteFileName = Path.GetFileName(remoteFile);
			const string fileContent = "download from remote file";
			CreateFile(remoteFile, fileContent);
			Assert("Local file should not exist", !File.Exists(localFile));
			try
			{
				var bytesDownloaded = Processor.DownloadFile(localFile, Path.Combine(ftpFolderName, remoteFileName));
				Assert("File should have been downloaded", File.Exists(localFile));
				AssertASCIIFileSameAsString(remoteFile, fileContent);
				AssertEquals(nameof(bytesDownloaded), fileContent.Length, bytesDownloaded);
			}
			finally
			{
				DeleteIfExists(remoteFile);
				DeleteIfExists(localFile);
			}
		}

		#endregion

		#region Append

		public void TestAppendToExistingFile()
		{
			string remoteFile = RemoteFile;
			string localFile = LocalFile;
			CreateFile(remoteFile, "remote");
			CreateFile(localFile, "local");
			try
			{
				Processor.AppendToFile(localFile, Path.Combine(ftpFolderName, Path.GetFileName(remoteFile)));
				AssertASCIIFileSameAsString(remoteFile, "remotelocal");
			}
			finally
			{
				DeleteIfExists(remoteFile);
				DeleteIfExists(localFile);
			}
		}

		public void TestAppendToNonExistingFile()
		{
			string localFile = LocalFile;
			string remoteFile = RemoteFile;
			CreateFile(localFile, "from local only");
			Assert("Remote file does not exist", !File.Exists(remoteFile));
			try
			{
				Processor.AppendToFile(localFile, Path.Combine(ftpFolderName, Path.GetFileName(remoteFile)));
				Thread.Sleep(2000);
				AssertFileExists("Remote file should habe been created", remoteFile);
				AssertASCIIFileSameAsString(remoteFile, "from local only");
			}
			finally
			{
				DeleteIfExists(localFile);
				DeleteIfExists(remoteFile);
			}
		}

		public void TestAppendToExistingFileWhenTempFileAlreadyExists()
		{
			string localFileName = LocalFile;
			string remoteFileName = RemoteFile;
			string tempRemoteFile = Path.Combine(ftpTestHelper.LocalDirectory, Path.Combine(ftpFolderName, Path.GetFileNameWithoutExtension(remoteFileName)) + ".tmp");

			CreateFile(localFileName, "local ");
			CreateFile(remoteFileName, "remote ");
			CreateFile(tempRemoteFile, "temp ");
			try
			{
				Processor.AppendToFile(localFileName, Path.Combine(ftpFolderName, Path.GetFileName(remoteFileName)));
				AssertASCIIFileSameAsString(tempRemoteFile, "temp local ");
				File.Delete(remoteFileName);

				Processor.AppendToFile(localFileName, Path.Combine(ftpFolderName, Path.GetFileName(remoteFileName)));
				AssertASCIIFileSameAsString(remoteFileName, "temp local local ");
			}
			finally
			{
				DeleteIfExists(localFileName);
				DeleteIfExists(remoteFileName);
				DeleteIfExists(tempRemoteFile);
			}
		}

		public void TestAppendToNonExistingFileWithUnicodeChar()
		{
			string localFile = LocalFile;
			string remoteFile = RemoteFile;
			CreateFile(localFile, "€€€€€€€€");
			try
			{
				Processor.AppendToFile(localFile, Path.Combine(ftpFolderName, Path.GetFileName(remoteFile)));
				AssertFileExists("Remote file should habe been created", remoteFile);
				AssertASCIIFileSameAsString(remoteFile, "€€€€€€€€");
			}
			finally
			{
				DeleteIfExists(localFile);
				DeleteIfExists(remoteFile);
			}
		}

		[ExpectNoExceptions()]
		public void TestAppendWhenWebExceptionWithSuccess()
		{
			RunWithException("WebWithSuccess");
		}

		[ExpectException(typeof(FtpException))]
		public void TestAppendWhenWebExceptionWithError()
		{
			RunWithException("WebWithError");
		}

		[ExpectException(typeof(FtpException))]
		public void TestAppendWhenNoWebException()
		{
			RunWithException("NoWeb");
		}

		[ExpectNoExceptions()]
		public void TestAppendWhenAPPEWarningWithSuccess()
		{
			RunWithException("APPE_Success");
		}

		[ExpectNoExceptions()]
		public void TestAppendWhenAPPEWarningWithError()
		{
			RunWithException("APPE_Error");
		}

		void RunWithException(string exceptionString)
		{
			string localFile = LocalFile;
			string remoteFile = RemoteFile;
			FtpProcessorWithException processor = new FtpProcessorWithException(exceptionString, ServerName, userName, password);
			try
			{
				processor.AppendToFile(localFile, Path.GetFileName(remoteFile));
			}
			finally
			{
				DeleteIfExists(localFile);
				DeleteIfExists(remoteFile);
			}
		}

		#endregion

		#region Rename

		public void TestRenameRemoteFile()
		{
			string localFile = LocalFile;
			string remoteFile = RemoteFile;
			CreateFile(remoteFile, "remote");
			string expectedFileName = Path.Combine(Path.Combine(ftpTestHelper.LocalDirectory, ftpFolderName), Guid.NewGuid() + ".txt");
			try
			{
				Processor.RenameRemoteFile(Path.Combine(ftpFolderName, Path.GetFileName(remoteFile)), Path.GetFileName(expectedFileName));
				AssertFileNotExists("Original file should not exist anymore", remoteFile);
				AssertFileExists("The renamed file should exist", expectedFileName);
			}
			finally
			{
				DeleteIfExists(remoteFile);
				DeleteIfExists(expectedFileName);
			}
		}

		#endregion

		public void TestTimeoutDoesntAllowInvalidValues()
		{
			var ftp = new FtpProcessor();
			Assert("ReadTimeout must be > 0 zero by default", ftp.ReadTimeout.TotalMilliseconds > 0);
			AssertExceptionThrown<ArgumentOutOfRangeException>("You should not be allowed to set ReadTimeout to an invalid value", () => ftp.ReadTimeout = new TimeSpan());
			Assert("ReadTimeout should have kept the valid value", ftp.ReadTimeout.TotalMilliseconds > 0);

			Assert("ConnectionTimeout must be >= 0 zero by default", ftp.ConnectTimeout.TotalMilliseconds >= 0);
			AssertExceptionThrown<ArgumentOutOfRangeException>("You should not be allowed to set the ConnectionTimeout to an invalid value", () => ftp.ConnectTimeout = TimeSpan.FromSeconds(-1));
			Assert("ConnectionTimeout should have kept the valid value", ftp.ConnectTimeout.TotalMilliseconds >= 0);
		}

		#region Delete

		public void TestConnectionToServerOnTimeoutDoesntDeleteFiles()
		{
			string localFile = LocalFile;
			string remoteFile = RemoteFile;
			string remoteFileName = Path.GetFileName(remoteFile);

			using (StreamWriter writer = new StreamWriter(remoteFile))
			{
				for (int i = 0; i < 100000; i++)
				{
					writer.Write("download from remote file");
				}
			}
			Assert("Local file should not exist", !File.Exists(localFile));

			try
			{
				var ftp = new FtpProcessor(ServerName, userName, password, new TimeSpan(0, 0, 30), new TimeSpan(1));
				AssertExceptionThrown(typeof(FtpException), () => ftp.DownloadFile(localFile, remoteFile));
				Assert("Local file should not exist", !File.Exists(localFile));
				Assert("Remote file", File.Exists(remoteFile));

				ftp.ReadTimeout = new TimeSpan(0, 0, 1);
				ftp.ConnectTimeout = new TimeSpan(0, 0, 30);
				ftp.DownloadFile(localFile, remoteFile);
				Assert("Local file should exist", File.Exists(localFile));
				Assert("Remote file", File.Exists(remoteFile));
			}
			finally
			{
				DeleteIfExists(remoteFile);
				DeleteIfExists(localFile);
			}
		}

		public void TestDeleteRemoteFileWithInvalidChars()
		{
			string[] invalidChars = { "#", "%", "'" };
			string remoteFile = "";

			remoteFile = Path.Combine(Path.Combine(ftpTestHelper.LocalDirectory, ftpFolderName), invalidChars[0] + Guid.NewGuid().ToString() + invalidChars[1] + ".txt");

			string remoteFileName = Path.GetFileName(remoteFile);

			CreateFile(remoteFile, "download from remote file");

			try
			{
				AssertEquals(true, Processor.DeleteRemoteFile(Path.Combine(ftpFolderName, Path.GetFileName(remoteFile))));
				AssertFileNotExists("File should have been deleted", remoteFile);
			}
			finally
			{
				DeleteIfExists(remoteFile);
			}
		}

		public void TestDeleteRemoteFile()
		{
			string remoteFile = RemoteFile;
			CreateFile(remoteFile, "remote");
			try
			{
				AssertEquals(true, Processor.DeleteRemoteFile(Path.Combine(ftpFolderName, Path.GetFileName(remoteFile))));
				AssertFileNotExists("File should have been deleted", remoteFile);
			}
			finally
			{
				DeleteIfExists(remoteFile);
			}
		}

		string deleteErrorLogResult;
		void LogMethodForDeleteRemoteFileWithErrorAndLogging(string msg)
		{
			deleteErrorLogResult = msg;
		}

		public void TestDeleteRemoteFileWithErrorAndLogging()
		{
			string remoteFile = RemoteFile;
			DeleteIfExists(remoteFile);
			var secondProcessorWithLogging = new FtpProcessor(ServerName, userName, password, LogMethodForDeleteRemoteFileWithErrorAndLogging, readTimeout, connectTimeout);
			try
			{
				AssertEquals(false, secondProcessorWithLogging.DeleteRemoteFile(Path.Combine(ftpFolderName, Path.GetFileName(remoteFile))));
				Assert(deleteErrorLogResult, deleteErrorLogResult.Contains("550"));  // 550 means "Requested action not taken. File unavailable (e.g., file not found, no access)"
			}
			catch (Exception ex)
			{
				Fail("Should not see an exception, " + ex.Message);
			}
		}

		#endregion

		#region Upload unique

		public void TestUploadUnique()
		{
			string testingFolderName = "SubFolderForUnique" + Guid.NewGuid();
			string testingFolderNameWithWindowsDelimters = @"\" + testingFolderName + @"\";
			string testingFolderNameWithWebDelimters = @"/" + testingFolderName + @"/";

			string localFile = LocalFile;
			string remotePath = Path.GetDirectoryName(RemoteFile) + testingFolderNameWithWindowsDelimters;
			Directory.CreateDirectory(remotePath);
			string remoteFile = remotePath + Path.GetFileName(RemoteFile);

			CreateFile(localFile, "local ");
			Assert("Local file should exist", File.Exists(localFile));

			try
			{
				Processor.UploadFileUnique(localFile, "/" + ftpFolderName + "/" + testingFolderNameWithWebDelimters);
				string[] filesFound = Directory.GetFiles(remotePath);
				AssertEquals("Should have one file created in the target folder", 1, filesFound.Length);
			}
			finally
			{
				foreach (string fileName in Directory.GetFiles(remotePath))
				{
					DeleteIfExists(fileName);
				}
				Directory.Delete(remotePath);
				DeleteIfExists(localFile);
			}
		}

		#endregion

		#region Upload specific

		public void TestUploadSpecific()
		{
			string localFile = LocalFile;
			string remoteFile = RemoteFile;
			string remoteFileName = Path.GetFileName(remoteFile);

			CreateFile(localFile, "local ");
			Assert("Local file should exist", File.Exists(localFile));
			Assert("Remote file should not exist", !File.Exists(remoteFile));

			try
			{
				Processor.UploadFile(localFile, Path.Combine(ftpFolderName, remoteFileName));
				AssertFileExists("File should have been uploaded", remoteFile);
			}
			finally
			{
				DeleteIfExists(remoteFile);
				DeleteIfExists(localFile);
			}
		}

		public void TestUploadSecure()
		{
			var localFile = LocalFile;
			var remoteFile = RemoteFile;
			var remoteFileName = Path.GetFileName(remoteFile);

			CreateFile(localFile, "local ");
			Assert("Local file should exist", File.Exists(localFile));
			Assert("Remote file should not exist", !File.Exists(remoteFile));

			var initialProtocol = ServicePointManager.SecurityProtocol;

			try
			{
				ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => BitConverter.ToString(cert.GetCertHash()) == "9E-34-CE-FF-F8-8D-E0-73-15-3C-7B-F0-C5-A4-1E-47-47-E5-AB-27";
#pragma warning disable CA5386 // Avoid hardcoding SecurityProtocolType value
				// This is a temporary solution and will be removed after WI00887123.
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.SystemDefault;
#pragma warning restore CA5386 // Avoid hardcoding SecurityProtocolType value
				var secureProcessor = new FtpProcessor(ServerName, userName, password, readTimeout, connectTimeout, usePassive: true, useSecureConnection: true);
				secureProcessor.UploadFile(localFile, Path.Combine(ftpFolderName, remoteFileName));
				AssertFileExists("File should have been uploaded", remoteFile);
			}
			finally
			{
				ServicePointManager.SecurityProtocol = initialProtocol;
				DeleteIfExists(remoteFile);
				DeleteIfExists(localFile);
			}
		}

		public void TestUploadSpecialCharacter()
		{
			string localFile = Path.Combine(Temp.TempPath, Guid.NewGuid().ToString() + "%" + "#" + ".txt");
			string remoteFile = Path.Combine(Path.Combine(ftpTestHelper.LocalDirectory, ftpFolderName), Guid.NewGuid().ToString() + "%" + "#" + ".txt");
			string remoteFileName = Path.GetFileName(remoteFile);

			CreateFile(localFile, "#%local ");
			Assert("Local file should exist", File.Exists(localFile));
			Assert("Remote file should not exist", !File.Exists(remoteFile));

			try
			{
				Processor.UploadFile(localFile, Path.Combine(ftpFolderName, remoteFileName));
				AssertFileExists("File should have been uploaded", remoteFile);
			}
			finally
			{
				DeleteIfExists(remoteFile);
				DeleteIfExists(localFile);
			}
		}

		public void TestUploadSpecificWithFakeRemoteDirectory()
		{
			AssertExceptionThrown(typeof(FtpException), TestUploadSpecificWithFakeRemoteDirectoryRunner);
		}

		void TestUploadSpecificWithFakeRemoteDirectoryRunner()
		{
			string localFile = LocalFile;
			string remoteFile = Path.GetDirectoryName(RemoteFile) + @"\SubFolderShitDoesntExist\" + Path.GetFileName(RemoteFile);

			string remoteFileName = @"/SubFolderShitDoesntExist/" + Path.GetFileName(remoteFile);

			CreateFile(localFile, "local ");

			Assert("Local file should exist", File.Exists(localFile));
			Assert("Remote file should not exist", !File.Exists(remoteFile));
			try
			{
				Processor.UploadFile(localFile, remoteFileName);
			}
			finally
			{
				DeleteIfExists(remoteFile);
				DeleteIfExists(localFile);
			}
		}

		public void TestUploadSpecificWithGoodRemoteDirectory()
		{
			string localFile = LocalFile;
			string remotePath = Path.GetDirectoryName(RemoteFile);
			Directory.CreateDirectory(remotePath);
			string remoteFile = remotePath + Path.GetFileName(RemoteFile);
			string remotePartialFileNameOnServer = Path.GetFileName(remoteFile);

			CreateFile(localFile, "local ");

			Assert("Local file should exist", File.Exists(localFile));
			Assert("Remote file should not exist", !File.Exists(remoteFile));
			try
			{
				Processor.UploadFile(localFile, remotePartialFileNameOnServer);
				AssertFileExists("Remote file should now exist", remoteFile);
				AssertEquals("local ", File.ReadAllText(remoteFile));
			}
			finally
			{
				DeleteIfExists(remoteFile);
				DeleteIfExists(localFile);
			}
		}

		public void TestUploadSpecificWhereDirectoryIsPartOfUri()
		{
			string goodSubFolder = @"/" + ftpFolderName + "/";
			processor = new FtpProcessor(ServerName + "/" + goodSubFolder, userName, password, readTimeout, connectTimeout);
			CreateFile(LocalFile, "local ");
			string remotePartialFileNameOnServer = Path.GetFileName(LocalFile);
			string remoteFile = Path.Combine(Path.GetDirectoryName(RemoteFile), remotePartialFileNameOnServer);

			Directory.CreateDirectory(Path.GetDirectoryName(remoteFile));
			Assert("Remote folder should exist", Directory.Exists(Path.GetDirectoryName(remoteFile)));
			Assert("Local file should exist", File.Exists(LocalFile));
			Assert("Remote file should not exist", !File.Exists(remoteFile));
			try
			{
				Processor.UploadFile(LocalFile, remotePartialFileNameOnServer);
				AssertFileExists("Remote file should now exist", remoteFile);
			}
			finally
			{
				DeleteIfExists(remoteFile);
				DeleteIfExists(LocalFile);
			}
		}

		#endregion

		#region AffixProtocolToServerNameAndAppendForwardSlashIfNeeded

		public void TestAffixProtocolToServerNameAndAppendForwardSlashIfNeeded()
		{
			var processor = new FtpProcessor();
			processor.ServerName = "ftp.server.com";
			AssertEquals("FTP Protocol is affixed.", "ftp://ftp.server.com/", processor.AffixProtocolToServerNameAndAppendForwardSlashIfNeeded());
			processor.ServerName = "ftp://ftp.server.com";
			AssertEquals("Forward slash appended.", "ftp://ftp.server.com/", processor.AffixProtocolToServerNameAndAppendForwardSlashIfNeeded());
			processor.ServerName = "ftp://ftp.server.com/";
			AssertEquals("No forward slash is appended.", "ftp://ftp.server.com/", processor.AffixProtocolToServerNameAndAppendForwardSlashIfNeeded());
			processor.ServerName = "http://ftp.server.com/";
			AssertExceptionThrown("Exception is thrown for invalid address", typeof(NotSupportedException), () => { processor.AffixProtocolToServerNameAndAppendForwardSlashIfNeeded(); });
		}

		#endregion

		#region Implementation

		void AssertFileExists(string message, string fileName)
		{
			//Assert(message, File.Exists(fileName));	// some weird behaviour of the FTP server causes the file to not be visible immediately
			AssertFileExists(message, fileName, 0);
		}

		void AssertFileExists(string message, string fileName, int count)
		{
			if (File.Exists(fileName))
			{
				Assert(true);
			}
			else
			{
				if (count == FileExistsRetryCount)
				{
					Fail(message);
				}
				else
				{
					count++;
					Thread.Sleep(FileExistsSleepRetryCount);
					AssertFileExists(message, fileName, count);
				}
			}
		}

		void AssertFileNotExists(string message, string fileName)
		{
			//Assert(message, !File.Exists(fileName));	// some weird behaviour of the FTP server causes the file to not be visible immediately
			AssertFileNotExists(message, fileName, 0);
		}

		void AssertFileNotExists(string message, string fileName, int count)
		{
			if (!File.Exists(fileName))
			{
				Assert(true);
			}
			else
			{
				if (count == FileExistsRetryCount)
				{
					Fail(message);
				}
				else
				{
					count++;
					Thread.Sleep(FileExistsSleepRetryCount);
					AssertFileNotExists(message, fileName, count);
				}
			}
		}

		void AssertUsePassive(FtpProcessor ftpProcessor, bool expectedValue)
		{
			var request = ftpProcessor.GetNewFtpRequestForPublicContract("ftp://localhost:2121", WebRequestMethods.Ftp.DownloadFile);
			AssertEquals(expectedValue, request.UsePassive);
		}

		const int FileExistsRetryCount = 10;
		const int FileExistsSleepRetryCount = 1000;

		FtpProcessor Processor
		{
			get { return processor ?? (processor = new FtpProcessor(ServerName, userName, password, readTimeout, connectTimeout)); }
		}
		FtpProcessor processor;

		string _localFile;
		string LocalFile
		{
			get { return _localFile ?? (_localFile = Path.Combine(Temp.TempPath, Guid.NewGuid().ToString() + ".txt")); }
		}

		string _remoteFile;
		string RemoteFile
		{
			get { return _remoteFile ?? (_remoteFile = Path.Combine(Path.Combine(ftpTestHelper.LocalDirectory, ftpFolderName), Guid.NewGuid().ToString() + ".txt")); }
		}

		void CreateFile(string pathToFile, string text)
		{
			using (StreamWriter writer = new StreamWriter(pathToFile))
			{
				writer.Write(text);
			}
		}

		public class FtpProcessorForTest : FtpProcessor
		{
			public FtpProcessorForTest(string serverName, string username, string password, TimeSpan readTimeout, TimeSpan connectTimeout, bool usePassive = true, bool useSecureConnection = false)
			: base(serverName, username, password, null, readTimeout, connectTimeout, usePassive, useSecureConnection)
			{
			}

			public new bool UseSecure => base.UseSecure;

			public new FtpWebRequest GetNewFtpRequest(string url, string method) => base.GetNewFtpRequest(url, method);
		}

		class FtpProcessorWithException : FtpProcessor
		{
			public FtpProcessorWithException(string exceptionString, string serverName, string username, string password)
				: base(serverName, username, password, readTimeout, connectTimeout)
			{
				ExceptionString = exceptionString;
			}

			public FtpProcessorWithException(string exceptionString, FtpStatusCode statusCode, string statusDescription)
				: base("", "", "", readTimeout, connectTimeout)
			{
				ExceptionString = exceptionString;
				StatusCode = statusCode;
				StatusDescription = statusDescription;
			}

			public FtpProcessorWithException(string exceptionString, string serverName, string username, string password, FtpStatusCode statusCode, string statusDescription)
				: base(serverName, username, password, readTimeout, connectTimeout)
			{
				ExceptionString = exceptionString;
				StatusCode = statusCode;
				StatusDescription = statusDescription;
			}

			protected override FtpWebRequest GetNewFtpRequest(string url, string method)
			{
				if (url.Length - 3 < 0)
				{
					throw new ArgumentException("Invalid argument.", nameof(url));
				}

				if (StatusCode != FtpStatusCode.Undefined)
				{
					ConstructorInfo constr =
						typeof(FtpWebResponse).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
						new Type[] { typeof(Stream), typeof(long), typeof(Uri), typeof(FtpStatusCode), typeof(string), typeof(DateTime), typeof(string), typeof(string), typeof(string) }, null);
					if (constr != null)
					{
						FtpWebResponse response = (FtpWebResponse)constr.Invoke(new object[] { null, 0, null, StatusCode, StatusDescription, DateTime.Now, "", "", "" });
						throw new WebException(ExceptionString, null, WebExceptionStatus.UnknownError, response);
					}
				}

				return base.GetNewFtpRequest(url, method);
			}

			protected override void AppendToFileCore(string localFilePath, string remoteFilePath)
			{
				switch (ExceptionString)
				{
					case "NoWeb":
						throw new Exception();
					case "WebWithSuccess":
						throw new WebException("WebWithSuccess", WebExceptionStatus.Success);
					case "WebWithError":
						throw new WebException("WebWithError", WebExceptionStatus.ProtocolError);
					case "APPE_Success":
						throw new WebException("150 APPE supported", WebExceptionStatus.Success);
					case "APPE_Error":
						throw new WebException("150 APPE supported", WebExceptionStatus.ProtocolError);
				}
			}

			readonly string ExceptionString;
			readonly FtpStatusCode StatusCode;
			readonly string StatusDescription;
		}

		#endregion
	}

	public class FtpTestHelper : IDisposable
	{
		public FtpTestHelper()
			: this("ftpUserName", "ftpPassword")
		{ }

		public FtpTestHelper(string ftpUserName, string ftpUserPassword)
		{
			UserName = ftpUserName;
			Password = ftpUserPassword;
		}

		[SuppressMessage("Microsoft.Contracts", "TestAlwaysEvaluatingToAConstant")] // cause CC loves evaluating constants from try catch
		public void Start()
		{
			LocalDirectory = Temp.GetNewTempSubdirectory();
			do
			{
				var userStore = new UserStore();
				userStore.Users.Clear();
				userStore.Users.Add(new User() { Username = UserName, Password = Password, HomeDir = LocalDirectory });
				try
				{
					Port = new Random().Next(49152, 65535);
					ftpServer = new FtpServer(IPAddress.Any, Port, userStore);
					ftpServer.Start();
				}
				catch (SocketException)
				{
					ftpServer = null;
				}
			} while (ftpServer == null);
		}

		void WaitForStartListenerResult()
		{
			if (ftpServer.startListenerResult != null && !ftpServer.startListenerResult.IsCompleted)
			{
				ftpServer.startListenerResult.AsyncWaitHandle?.WaitOne();
				ftpServer.startListenerResult = null;
			}
		}

		public void Dispose()
		{
			if (ftpServer != null)
			{
				ftpServer.Dispose();
				WaitForStartListenerResult();
				ftpServer = null;
			}
			try
			{
				Directory.Delete(LocalDirectory, true);
			}
			catch (IOException)
			{
				Thread.Sleep(1000);
				Directory.Delete(LocalDirectory, true);
			}
		}

		public string LocalDirectory
		{
			get;
			private set;
		}

		public int Port
		{
			get;
			private set;
		}

		public Uri ServerAddress
		{
			get { return new Uri("ftp://localhost:" + Port); }
		}

		public string UserName
		{
			get;
			private set;
		}

		public string Password
		{
			get;
			private set;
		}

		FtpServer ftpServer;
	}
}
