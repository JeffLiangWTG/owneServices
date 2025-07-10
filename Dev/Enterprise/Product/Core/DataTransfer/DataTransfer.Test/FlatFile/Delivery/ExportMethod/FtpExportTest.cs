using System;
using System.IO;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.IO.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Moq;
using Moq.Protected;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class FtpExportTest : TestCaseWithFactory
	{
		public void TestExportType()
		{
			ExportInstructions instructions = new ExportInstructions();
			FtpExport export = new FtpExport(instructions, Buffer);
			AssertEquals("Should ALWAYS be Ftp delivery type", ExportType.Ftp, export.ExportType);
		}

		public void TestCanDeliver()
		{
			ExportInstructions instructions = new ExportInstructions();
			FtpExport export = new FtpExport(instructions, Buffer);
			FtpExportInstructions ftpProperties = instructions.FtpProperties;
			ftpProperties.RunPreSaveValidation();
			Assert("PreConditon: FtpProperties has error", ftpProperties.HasErrors);
			AssertEquals("Prerequisite conditions not met - ftp setting not correct", false, export.CanDeliver);

			ftpProperties.ServerAddress = "ftp.updates.edi.com.au";
			ftpProperties.Username = "Anonymous";
			ftpProperties.Password = "Anonymouse@test.com.au";
			ftpProperties.RunPreSaveValidation();
			AssertEquals("PreConditon: FtpProperties has no error", false, ftpProperties.HasErrors);
			AssertEquals("Prerequisite conditions not met - ftp setting not correct", true, export.CanDeliver);
		}

		public void TestDeliver_HandlingException()
		{
			UnitTestFtp.Instance.ResetFtpNotTransferBecauseInTestMode();
			var instructions = new ExportInstructions();
			var mockExport = new Mock<FtpExport>(new object[] { instructions, Buffer });
			mockExport.CallBase = true;
			var export = mockExport.Object;
			AssertNotNull("Export", export);

			var ftpProperties = instructions.FtpProperties;
			ftpProperties.RunPreSaveValidation();
			Assert("PreConditon: FtpProperties has error", ftpProperties.HasErrors);
			AttemptToDeliverWithFile(export);
			AssertEquals("Buffer should have errors as FtpProperties have errors. Buffer contain: " + System.Environment.NewLine + Buffer.AsString, true, Buffer.HasErrors);
			var callBeginUploadMockExceptionString = "New CallBeginUploadMock Error Exception";
			AssertEquals(string.Format("Buffer should not contain error '{0}'. Buffer contain: {1}{2}",
				callBeginUploadMockExceptionString,
				System.Environment.NewLine,
				Buffer.AsString), false, Buffer.AsString.Contains(callBeginUploadMockExceptionString));

			UnitTestFtp.Instance.StatusDescriptionForTest = "Dummy Status Description";
			ftpProperties.ServerAddress = "ftp.updates.edi.com.au";
			ftpProperties.Username = "Anonymous";
			ftpProperties.Password = "Anonymouse@test.com.au";
			AssertEquals("PreConditon: Instructions has no error", false, ftpProperties.HasErrors);
			Buffer.Clear();
			AttemptToDeliverWithFile(export);
			AssertEquals("Buffer should not have errors. Buffer contain: " + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
			AssertEquals(string.Format("Buffer should contain 'Data Ftp Completed - Dummy Status Description'. Buffer contain: {0}{1}",
				System.Environment.NewLine,
				Buffer.AsString), false, Buffer.AsString.IndexOf("Data Ftp Completed - Dummy Status Description") > 0);

			var mockException = new Exception(callBeginUploadMockExceptionString);
			mockExport.Protected()
				.Setup("CallBeginUploadMock", ItExpr.IsAny<FtpState>())
				.Throws(mockException);
			Buffer.Clear();
			AttemptToDeliverWithFile(export);
			AssertEquals(string.Format("Buffer should contain error '{0}'. Buffer contain: {1}{2}",
				callBeginUploadMockExceptionString,
				System.Environment.NewLine,
				Buffer.AsString), true, Buffer.AsString.Contains(callBeginUploadMockExceptionString));

			mockExport.VerifyAll();
			mockExport.Reset();
			UnitTestFtp.Instance.ResetFtpNotTransferBecauseInTestMode();
			UnitTestFtp.Instance.OperationExceptionForTest = new Exception("Exception from State");
			Buffer.Clear();
			AttemptToDeliverWithFile(export);
			AssertEquals(string.Format("Buffer should not contain error '{0}'. Buffer contain: {1}{2}",
				callBeginUploadMockExceptionString,
				System.Environment.NewLine,
				Buffer.AsString), false, Buffer.AsString.Contains(callBeginUploadMockExceptionString));
			AssertEquals(string.Format("Buffer should not contain error 'Exception from State'. Buffer contain: {0}{1}",
				System.Environment.NewLine,
				Buffer.AsString), true, Buffer.AsString.Contains("Exception from State"));
		}

		public void TestDeliver_WithLiveServer()
		{
			var instructions = new ExportInstructions();
			using (var ftpTestHelper = new FtpTestHelper())
			{
				ftpTestHelper.Start();
				instructions.FtpProperties.ServerAddress = ftpTestHelper.ServerAddress.ToString();
				instructions.FtpProperties.Username = ftpTestHelper.UserName;
				instructions.FtpProperties.Password = ftpTestHelper.Password;
				var destinationFile = Path.Combine(ftpTestHelper.LocalDirectory, Path.GetFileName(TempFileToDoThings));
				try
				{
					var export = new FtpExport(instructions, Buffer);
					AssertEquals(string.Format("PreCondition: File '{0}' should not exist on server", destinationFile), false, File.Exists(destinationFile));
					AttemptToDeliverWithFile(export);
					AssertEquals("Buffer should not have errors. Buffer contain: " + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
					AssertEquals(string.Format("Buffer should contain 'Data Ftp Completed - '. Buffer contain: {0}{1}",
						System.Environment.NewLine,
						Buffer.AsString), false, Buffer.AsString.IndexOf("Data Ftp Completed - ") > 0);
					AssertEquals(string.Format("File '{0}' should not exist on server for standard test mode", destinationFile), false, File.Exists(destinationFile));

					Buffer.Clear();
					export = new FtpExportLiveTest(instructions, Buffer);
					AttemptToDeliverWithFile(export);
					AssertEquals("Buffer should not have errors. Buffer contain: " + System.Environment.NewLine + Buffer.AsString, false, Buffer.HasErrors);
					AssertEquals(string.Format("Buffer should contain 'Data Ftp Completed - '. Buffer contain: {0}{1}",
						System.Environment.NewLine,
						Buffer.AsString), false, Buffer.AsString.IndexOf("Data Ftp Completed - ") > 0);
					AssertFileExists(string.Format("File '{0}' should exist on server", destinationFile), destinationFile);
				}
				finally
				{
					DeleteIfExists(TempFileToDoThings);
				}
			}
		}

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
				if (count == 10)
				{
					Fail(message);
				}
				else
				{
					count++;
					Thread.Sleep(1000);
					AssertFileExists(message, fileName, count);
				}
			}
		}

		void AttemptToDeliverWithFile(FtpExport export)
		{
			var expectedOutputFilestream = resourceRetriever.Value.GetStream("Enterprise.DataTransfer.Test.FlatFile.Delivery.TestFiles.File1.txt");
			using (var fileStream = new FileStream(TempFileToDoThings, FileMode.Create, FileAccess.Write))
			{
				expectedOutputFilestream.CopyTo(fileStream);
			}
			File.SetAttributes(TempFileToDoThings, FileAttributes.Normal);
			export.Deliver(TempFileToDoThings);
		}

		ZString TempFileToDoThings
		{
			get
			{
				if (fTempFileToDoThings.IsEmpty)
				{
					fTempFileToDoThings = Temp.GetTempFileName();
				}
				return fTempFileToDoThings;
			}
		}
		ZString fTempFileToDoThings;

		NotificationBuffer Buffer
		{
			get
			{
				if (fBuffer == null)
				{
					fBuffer = new NotificationBuffer();
				}
				return fBuffer;
			}
		}

		NotificationBuffer fBuffer;

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		protected override void SetUp()
		{
			base.SetUp();
			UnitTestFtp.Instance.ResetFtpNotTransferBecauseInTestMode();
		}

		protected override void TearDown()
		{
			UnitTestFtp.Instance.ResetFtpNotTransferBecauseInTestMode();
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		sealed class FtpExportLiveTest : FtpExport
		{
			public FtpExportLiveTest(ExportInstructions instructions, INotifications notifications)
				: base(instructions, notifications)
			{
			}

			protected override void CallBeginUploadMock(FtpState state)
			{
				BeginUpload(state);
			}
		}
	}
}
