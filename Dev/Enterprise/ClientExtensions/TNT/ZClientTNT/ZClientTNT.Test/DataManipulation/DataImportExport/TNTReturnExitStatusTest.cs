using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.TNT
{
	public class TNTReturnExitStatusTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestDirectoryCreationFromRegistry()
		{
			ExportDirectory = "";
			TNTReturnExitStatus testFileReturn = new TNTReturnExitStatus();
			testFileReturn.SendEDNReply("SYD", "HB1", "SYD", "LAX", "EDN001", "CLEAR", "Y");
		}

		public void TestFileCreation()
		{
			ZString errorMessage = ResponseSender.SendEDNReply("SYD", "HB1", "SYD", "LAX", "EDN001", "CLEAR", "Y");
			AssertEquals("Error message should be empty", ZString.Empty, errorMessage);
			Assert(!ResponseSender.NotificationBuffer.HasErrors);
			ExportDirectory = ZString.Empty;
			errorMessage = ResponseSender.SendEDNReply("SYD", "HB1", "SYD", "LAX", "EDN001", "CLEAR", "Y");
			AssertEquals("Error message should not be empty", String.Format("{0} has not been set.", ExportDirectoryRegistry.Caption), errorMessage);
			Assert(ResponseSender.NotificationBuffer.HasErrors);
		}

		[TestDate(2005, 1, 2, 14, 30, 45, 28)]
		[TestDateIncremental]
		public void TestSubsequentCallDoesNotOverwritePreviousData()
		{
			TestDateIncrementalAttribute.Span = TimeSpan.Zero;
			string expectedFilename = (ResponseSender.PopulateFileNameInMilliSeconds) ? "TIES20050102143045028E.SYD" : "TIES20050102143045E.SYD";
			string expectedFileFullPath = Path.Combine(TempDirectory, expectedFilename + QuantumFile.Extension);
			AssertEquals(string.Format("PreCondition: File {0} should not exist", expectedFileFullPath), false, File.Exists(expectedFileFullPath));
			ZString errorMessage = ResponseSender.SendEDNReply("SYD", "HB1", "SYD", "LAX", "EDN001", "CLEAR", "Y");
			AssertEquals("Error message should be empty", ZString.Empty, errorMessage);
			AssertEquals(string.Format("File {0} should exist", expectedFileFullPath), true, File.Exists(expectedFileFullPath));
			string expectedData = "HB1            SYD  LAX  EDN001   CLEARY" + System.Environment.NewLine;
			AssertASCIIFileSameAsString(expectedFileFullPath, expectedData);
			errorMessage = ResponseSender.SendEDNReply("SYD", "HB2", "MEL", "SIN", "EDN002", "HOLD", "N");
			AssertEquals("Error message should be empty", ZString.Empty, errorMessage);
			AssertEquals(string.Format("File {0} should have been appended", expectedFileFullPath), true, File.Exists(expectedFileFullPath));
			expectedData += "HB2            MEL  SIN  EDN002   HOLD N" + System.Environment.NewLine;
			AssertASCIIFileSameAsString(expectedFileFullPath, expectedData);
			File.SetAttributes(expectedFileFullPath, FileAttributes.ReadOnly);
			errorMessage = ResponseSender.SendEDNReply("SYD", "HB3", "PER", "HKG", "EDN003", "HOLD", "N");
			AssertEquals("Error message should not be empty", ZString.Format("Access to the path '{0}' is denied.", expectedFileFullPath), errorMessage);
			TestDateAttribute.AddSeconds(-1);
			TestDateIncrementalAttribute.Span = TimeSpan.FromSeconds(1);
			errorMessage = ResponseSender.SendEDNReply("SYD", "HB3", "PER", "HKG", "EDN003", "HOLD", "N");
			AssertEquals("Error message should be empty", ZString.Empty, errorMessage);
			AssertContains("TIES20050102143046", ResponseSender.NotificationBuffer.AsString);
		}

		#region SetUp & TearDown
		string TempDirectory;
		protected override void SetUp()
		{
			base.SetUp();
			TempDirectory = Path.Combine(Env.TempPath, "TNTTesting");
			Directory.CreateDirectory(TempDirectory);
			ExportDirectory = TempDirectory;
			ResponseSender.NotificationBuffer.Clear();
		}

		protected override void TearDown()
		{
			base.TearDown();
			CargoWise.IO.TempDirectory.DeleteDirectory(TempDirectory);
		}

		protected TNTReturnExitStatus ResponseSender
		{
			get
			{
				return responseSender ?? (responseSender = GetResponseSender());
			}
		}

		TNTReturnExitStatus responseSender;
		protected virtual TNTReturnExitStatus GetResponseSender()
		{
			return new TNTReturnExitStatus();
		}

		protected ZString ExportDirectory { get => ExportDirectoryRegistry.Value; set => ExportDirectoryRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }

		protected virtual StringRegistryItem ExportDirectoryRegistry { get => TNTDataRegistry.Instance.TNTReplyDirectoryRaw; }
		#endregion
	}
}
