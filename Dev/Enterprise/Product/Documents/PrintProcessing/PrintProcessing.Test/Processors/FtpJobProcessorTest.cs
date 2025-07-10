using System;
using System.IO;
using System.Net;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.IO.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.PrintProcessing.Testing
{
	sealed class FtpJobProcessorTest : TestCaseWithFactory
	{
		class FtpJobProcessorWithDummyProcessor : FtpJobProcessor
		{
			public FtpJobProcessorWithDummyProcessor(IFtpProcessor processor, StmPrintJobMergedCollection mergedPrintGroup, PrintJobManager.ProgressDelegate logProgress) : base(mergedPrintGroup, logProgress, null)
			{
				this.processor = processor;
			}

			protected override IFtpProcessor GetProcessor(string url, string user, string password)
			{
				return processor;
			}
			readonly IFtpProcessor processor;
		}

		public void TestCreateProcessorWithUrl()
		{
			var ftpUrl = "ftp://localhost";
			var sftpUrl = "sftp://localhost";
			var anotherUrl = "localhost";

			var logs = new StringBuilder();
			var processor = new FtpJobProcessorWithDummyProcessor(null, null, (type, message) => { logs.AppendLine(message); });

			AssertType<FtpProcessor>(processor.CreateProcessor(ftpUrl, "TestUser", "TestPassword"));
			AssertType<SftpProcessor>(processor.CreateProcessor(sftpUrl, "TestUser", "TestPassword"));
			AssertType<FtpProcessor>(processor.CreateProcessor(anotherUrl, "TestUser", "TestPassword"));
		}

		[TestDate(2020, 5, 15, 12, 13, 14)]
		public void TestUploadFileNotification()
		{
			var localFileName = Path.Combine(Temp.TempPath, "test.txt");
			var remoteFileName = Path.Combine(FtpFolderPath, "Test (2020-05-15 12-13-14).PDF");
			var remoteValidFileName = Path.Combine(FtpFolderPath, "Test1 _ Test2 (2020-05-15 12-13-14).PDF");
			var url = ServerName + "/" + FtpFolderName;

			void TestNotify(bool successful, string message)
			{
				var email = new EmailDef
				{
					Subject = successful.ToString(),
					Body = message
				};
				email.AddRecipientForUserCommunication("test@test.com");
				Env.OutgoingMailManager.CreateAndSave(email);
			}

			try
			{
				var collection = CreateCollectionWithFtpJob("Test", localFileName, url);

				AssertFileExists("Precondition - no file on ftp", remoteFileName, false);

				var logs = new StringBuilder();
				var processor = new FtpJobProcessor(collection, (type, message) => { logs.AppendLine(message); }, TestNotify);
				processor.UploadToFtpInUnitTests = true;
				processor.Process();

				AssertFileExists("File should have been uploaded", remoteFileName, true);

				AssertEquals(1, CountMailDBItems("true"));

				logs.Clear();
				var processor2 = new FtpJobProcessor(collection, (type, message) => { logs.AppendLine(message); }, TestNotify);
				processor2.UploadToFtpInUnitTests = true;
				processor2.ThrowExceptionWhenRenamingRemoteFile = true;
				processor2.Process();

				Assert("Partial file should be deleted when an exception is thrown during FTP upload.", processor2.IsRemoteFileDeleted);

				AssertEquals(1, CountMailDBItems("false"));
			}
			finally
			{
				DeleteIfExists(localFileName);
				DeleteIfExists(remoteFileName);
				DeleteIfExists(remoteValidFileName);
			}
		}

		int CountMailDBItems(string subject)
		{
			return (int)Db.Connection.ExecuteScalar(String.Format("SELECT COUNT(*) FROM dbo.MailDBItems WHERE MI_Subject = '{0}'", subject));
		}

		[TestDate(2016, 6, 21, 12, 13, 14)]
		public void TestUploadFile_RenamesWhenThereIsACollision()
		{
			var url = ServerName + "/" + FtpFolderName;
			var localFileName = Path.Combine(Temp.TempPath, "test.txt");
			var remoteFileNameWithoutAttatchment = "Test (2016-06-21 12-13-14)";
			var remoteFileName = remoteFileNameWithoutAttatchment + ".PDF";
			var remotePartialName = remoteFileName + ".partial";

			try
			{
				var collection = CreateCollectionWithFtpJob("Test", localFileName, url);

				var ftpProcessor = new Mock<IFtpProcessor>();
				ftpProcessor
					.Setup(m => m.RenameRemoteFileSeveralAttempts(remotePartialName, remoteFileName, It.IsAny<int>(), It.IsAny<int>()))
					.Throws(new FtpException(FtpException.FtpExceptionType.RenameFile, "Could not rename file after 2 tries.", new WebException("Internal exception message")));

				ftpProcessor.Setup(m => m.ListDirectory("")).Returns(new[] { remoteFileName, remoteFileNameWithoutAttatchment + " (1).PDF" });
				ftpProcessor.Setup(m => m.RenameRemoteFileSeveralAttempts(remotePartialName, remoteFileNameWithoutAttatchment + " (2).PDF", It.IsAny<int>(), It.IsAny<int>()));

				var logs = new StringBuilder();
				var processor = new FtpJobProcessorWithDummyProcessor(ftpProcessor.Object, collection, (type, message) => { logs.AppendLine(message); });

				processor.UploadToFtpInUnitTests = true;
				processor.Process();

				ftpProcessor.VerifyAll();

				AssertContains("Should log that we are saving a new name", "File already existed. Renaming file 'Test (2016-06-21 12-13-14).PDF.partial' to 'Test (2016-06-21 12-13-14).PDF' instead", logs.ToString());
			}
			finally
			{
				DeleteIfExists(localFileName);
			}
		}

		[TestDate(2013, 11, 21, 10, 03, 00)]
		public void TestProcess()
		{
			var localFileName = Path.Combine(Temp.TempPath, "test.txt");
			var remoteFileName = Path.Combine(FtpFolderPath, "Test (2013-11-21 10-03-00).PDF");
			var remoteValidFileName = Path.Combine(FtpFolderPath, "Test1 _ Test2 (2013-11-21 10-03-00).PDF");
			var url = ServerName + "/" + FtpFolderName;

			try
			{
				var collection = CreateCollectionWithFtpJob("Test", localFileName, url);

				AssertFileExists("Precondition - no file on ftp", remoteFileName, false);

				var logs = new StringBuilder();
				var processor = new FtpJobProcessor(collection, (type, message) => { logs.AppendLine(message); }, null);
				processor.UploadToFtpInUnitTests = true;
				processor.Process();

				Assert("Partial file should be created during FTP upload.", processor.IsPartialFileCreated);
				Assert("Partial file should be renamed after FTP upload.", processor.IsRemoteFileRenamed);
				AssertFileExists("File should have been uploaded", remoteFileName, true);

				var expectedLog =
@"Uploading file 'Test (2013-11-21 10-03-00).PDF.partial'
Renaming file 'Test (2013-11-21 10-03-00).PDF.partial' to 'Test (2013-11-21 10-03-00).PDF'";
				var actualLog = logs.ToString().Trim();
				AssertEquals("No warnings should be logged", expectedLog, actualLog);

				logs.Clear();
				var processor2 = new FtpJobProcessor(collection, (type, message) => { logs.AppendLine(message); }, null);
				processor2.UploadToFtpInUnitTests = true;
				processor2.ThrowExceptionWhenRenamingRemoteFile = true;
				processor2.Process();

				Assert("Partial file should be deleted when an exception is thrown during FTP upload.", processor2.IsRemoteFileDeleted);

				expectedLog =
$@"Uploading file 'Test (2013-11-21 10-03-00).PDF.partial'
Renaming file 'Test (2013-11-21 10-03-00).PDF.partial' to 'Test (2013-11-21 10-03-00).PDF'
Could not rename file 'Test (2013-11-21 10-03-00).PDF.partial' to 'Test (2013-11-21 10-03-00).PDF' at FTP location {url}. Please check that specified user has security rights to rename files on the FTP, and there is no other file with same name. Partial file will be deleted.

CargoWise.IO.FtpException: Could not rename file after 2 tries. ---> System.Net.WebException: Internal exception message";
				actualLog = logs.ToString();
				actualLog = actualLog.Substring(0, actualLog.IndexOf("--- End of inner exception stack trace ---")).Trim();
				AssertEquals(expectedLog, actualLog);

				var collection3 = CreateCollectionWithFtpJob("Test1 | Test2", localFileName, url);
				var processor3 = new FtpJobProcessor(collection3, (type, message) => { logs.AppendLine(message); }, null)
				{
					UploadToFtpInUnitTests = true
				};
				processor3.Process();

				AssertFileExists("File with valid name should have been uploaded", remoteValidFileName, true);
			}
			finally
			{
				DeleteIfExists(localFileName);
				DeleteIfExists(remoteFileName);
				DeleteIfExists(remoteValidFileName);
			}
		}

		void AssertFileExists(string message, string fileName, bool expected)
		{
			if (File.Exists(fileName) ^ expected)
			{
				Fail(message);
			}
			else
			{
				Assert(true);
			}
		}

		#region Implementation

		StmPrintJobMergedCollection CreateCollectionWithFtpJob(string documentName, string localFileName, string url)
		{
			var deliveryGroup = Factory.New<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;
			deliveryGroup.SB_EmailSubjectLine = "blah";

			CreateDummyFile(localFileName);

			var collection = new StmPrintJobMergedCollection(Factory);
			var printJob = collection.AddNew();
			printJob.SP_JobType = nameof(PrintType.FTP);
			printJob.SP_DocumentName = documentName;
			printJob.SP_EmailAttachmentFormat = "PDF";
			printJob.SP_FaxDestination = url;
			printJob.SP_EmailFromAddress = UserName + "\0" + Password;
			printJob.StoredAttachmentFilename = localFileName;
			printJob.StoredAttachmentSizeKB = 80;
			printJob.SP_EmailSignature = "Company Name";
			printJob.SP_EmailSubjectLine = "Subject Line";
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;

			return collection;
		}

		void CreateDummyFile(string fileName)
		{
			File.WriteAllBytes(fileName, new UTF8Encoding(true).GetBytes("Sample text"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			ftpTestHelper = new FtpTestHelper(UserName, Password);
			ftpTestHelper.Start();
			Directory.CreateDirectory(FtpFolderPath);
		}

		protected override void TearDown()
		{
			if (Directory.Exists(FtpFolderPath))
			{
				Directory.Delete(FtpFolderPath);
			}
			ftpTestHelper.Dispose();
			base.TearDown();
		}

		string ServerName
		{
			get { return "ftp://localhost:" + ftpTestHelper.Port; }
		}

		string FtpFolderPath
		{
			get { return Path.Combine(ftpTestHelper.LocalDirectory, FtpFolderName); }
		}

		FtpTestHelper ftpTestHelper;
		const string FtpFolderName = "Test";
		const string UserName = "testuser";
		const string Password = "testpwd";

		#endregion
	}
}
