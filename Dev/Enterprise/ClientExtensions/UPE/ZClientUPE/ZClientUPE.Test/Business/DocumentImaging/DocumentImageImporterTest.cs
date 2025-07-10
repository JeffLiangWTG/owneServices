using System;
using System.IO;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Client.UPE.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.UPE.DocumentImaging.Testing
{
	sealed class DocumentImageImporterTest : DocumentImageImportingTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[SnailTest]
		public void TestImport_AttachAndDeleteFiles()
		{
			CopyTestFileToTempRepository("W41G0RYZ.*");
			AssertEquals("Files should be in the repository initially for the test", 3, Directory.GetFiles(Env.TempPath, "W41G0RYZ.*").Length);
			Thread.Sleep(0);

			JobDeclaration.Factory.Save();
			Thread.Sleep(0);

			UPETestHelper.Delay(delay);  // Caters for -5 seconds for "From" date time.

			if (DocumentImporter.ExecuteBatchForTest() == 1)
			{
				// Execute batch should was sucessful.
				AssertEquals("1 multi-page document should be imported", 1, JobDeclaration.DocManagerInfo.Documents.Count);
				AssertEquals("Files should be deleted from the repository afterwards", 0, Directory.GetFiles(Env.TempPath, "W41G0RYZ.*").Length);
			}
			else
			{
				// Execute batch should was unsucessful (also a valid result).
				AssertEquals("No multi-page document should be imported", 0, JobDeclaration.DocManagerInfo.Documents.Count);
				AssertEquals("Files should remain in repository afterwards", 3, Directory.GetFiles(Env.TempPath, "W41G0RYZ.*").Length);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[SnailTest]
		[TestDate]
		public void TestImport_WhenAttachFailsRetry()
		{
			CopyTestFileToTempRepository("W41G0RYZ.*");
			AssertEquals("Files should be in the repository initially for the test", 3, Directory.GetFiles(Env.TempPath, "W41G0RYZ.*").Length);
			UPETestHelper.Delay(delay);  // Caters for -5 seconds for "From" date time.

			AssertEquals("No documents should be imported", 0, DocumentImporter.ExecuteBatchForTest());
			AssertEquals("Files should NOT be deleted from the repository if they can't be attached", 3, Directory.GetFiles(Env.TempPath, "W41G0RYZ.*").Length);

			TestDateAttribute.Date = DateTime.Now.AddHours(1);

			JobDeclaration.Factory.Save(); // save a job that has a matching house bill
			Assert("Import should be retried", !DocumentImporter.ExecuteBatchForTest().Equals(0));
			AssertEquals("Files should be deleted after they are imported", 0, Directory.GetFiles(Env.TempPath, "W41G0RYZ.*").Length);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2050, 1, 1)]
		[SnailTest]
		public void TestImport_Notifications()
		{
			CopyTestFileToTempRepository("W41G0RYZ.*");
			UPETestHelper.Delay(delay);  // Caters for -5 seconds for "From" date time.

			DocumentImporter.ExecuteBatchForTest();
			AssertMultilineASCIIEquals("Notifications",
@"Importing document with index file '" + Path.Combine(Env.TempPath, "W41G0RYZ.000") + @"'
Could not find job with house bill 'M1302370459' for index file 'W41G0RYZ.000'
".Trim(), TestHelper.Buffer.AsString.Trim());

			AssertSentEmail("CargoWise One Document Imaging Notifications",
@"CargoWise One Document Imaging Notifications; Generated 01-Jan-50 00:00:00

Could not find job with house bill 'M1302370459' for index file 'W41G0RYZ.000'

CargoWise One Document Imaging
");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[SnailTest]
		[TestDate]
		public void TestNotifyUserIfNoDocumentsImportedInLast4Hours()
		{
			DocumentImporter.ExecuteBatchForTest();
			AssertEquals("No notifications initially", "", TestHelper.Buffer.AsString.Trim());

			TestDateAttribute.Date = ZDateTime.Now.AddHours(5).ToDateTime();

			CopyTestFileToTempRepository("W41G0RYZ.*");
			UPETestHelper.Delay(delay);  // Caters for -5 seconds for "From" date time.
			CusHAWB.Factory.Save();
			DocumentImporter.ExecuteBatchForTest();
			AssertEquals("Warning should not appear while documents are being imported successfully", -1, TestHelper.Buffer.AsString.IndexOf("past 4 hours"));
			TestHelper.Buffer.Clear();

			TestDateAttribute.Date = ZDateTime.Now.AddHours(4).AddMinutes(1).ToDateTime();

			DocumentImporter.ExecuteBatchForTest();
			AssertEquals("When no documents have been successfully imported for over 4 hours", "Warning: *** No documents have been successfully imported for the past 4 hours ***", TestHelper.Buffer.AsString.Trim());
		}

		protected override void SetUp()
		{
			base.SetUp();
			UPETestHelper.DeleteFiles(Env.TempPath, "W41G0RYZ");
		}

		protected override void TearDown()
		{
			try
			{
				UPETestHelper.DeleteFiles(Env.TempPath, "W41G0RYZ");
			}
			catch (IOException)
			{
				UPETestHelper.Delay(2); // Wait for 2 seconds and delete again
				UPETestHelper.DeleteFiles(Env.TempPath, "W41G0RYZ");
			}

			base.TearDown();
		}

		const int delay = 6;

		#region Test Classes

		class TestDocumentImporter : DocumentImageImporter
		{
			public TestDocumentImporter(INotifications notifications)
				: base(notifications)
			{
			}
		}

		#endregion

		TestDocumentImporter DocumentImporter
		{
			get { return fDocumentImporter ?? (fDocumentImporter = new TestDocumentImporter(TestHelper.Buffer)); }
		}
		TestDocumentImporter fDocumentImporter;

		void AssertSentEmail(ZString expectedSubject, ZString expectedBody)
		{
			EmailDef sentEmail = Env.OutgoingMailManager.EmailsCreated[Env.OutgoingMailManager.EmailsCreated.Count - 1];
			AssertEquals("Subject", expectedSubject, sentEmail.Subject);
			AssertEquals("Body", expectedBody, sentEmail.Body);
			AssertEquals("To", "bob@edi.com.au", sentEmail.Recipients[0]);
		}

		UPETestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new UPETestHelper(Factory)); }
		}
		UPETestHelper testHelper;
	}
}
