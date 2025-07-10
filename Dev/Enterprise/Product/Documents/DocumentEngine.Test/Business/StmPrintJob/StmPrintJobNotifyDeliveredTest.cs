using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	sealed class StmPrintJobNotifyDeliveredTest : TestCaseWithFactory
	{
		public void TestNotifyDeliveredEML()
		{
			SetupAndNotifyDelivered(PrintType.EML, TestEmailAddress, ZGuid.Empty, true);
			AssertChangedToDocManager();
		}

		public void TestNotifyDeliveredEMLWithoutDocManagerInfo()
		{
			SetupAndNotifyDelivered(PrintType.EML, TestEmailAddress, ZGuid.Empty, false);
			AssertDeleted();
		}

		public void TestNotifyDeliveredPRN()
		{
			SetupAndNotifyDelivered(PrintType.PRN, "", ZGuid.NewZGuid(), true);
			AssertChangedToDocManager();
		}

		public void TestNotifyDeliveredPRNWithoutDocManagerInfo()
		{
			SetupAndNotifyDelivered(PrintType.PRN, "", ZGuid.NewZGuid(), false);
			AssertDeleted();
		}

		public void TestNotifyDeliveredFAX()
		{
			SetupAndNotifyDelivered(PrintType.FAX, TestFaxNumber, ZGuid.Empty, true);
			AssertChangedToDocManager();
		}

		public void TestNotifyDeliveredFAXWithoutDocManagerInfo()
		{
			SetupAndNotifyDelivered(PrintType.FAX, TestFaxNumber, ZGuid.Empty, false);
			AssertDeleted();
		}

		public void TestNotifyDeliveredDDS()
		{
			SetupAndNotifyDelivered(PrintType.DDS, "", ZGuid.Empty, false);
			AssertDeleted();
		}

		public void TestNotifyDeliveredDDSNotSavedToEDocs()
		{
			SetupAndNotifyDelivered(PrintType.DDS, "", ZGuid.Empty, true);
			AssertNotDeleted();

			Job.SP_EDocsProcessed = true;
			Job.NotifyDelivered();
			AssertDeleted();
		}

		public void TestMarkAsProcessedDDS()
		{
			SetupAndNotifyDelivered(PrintType.DDS, "", ZGuid.Empty, true);
			AssertNotDeleted();
			AssertEquals(nameof(PrintType.DDS), Job.SP_JobType);

			Job.MarkEDocsProcessed();
			AssertDeleted();
		}

		public void TestMarkAsProcessedForEdocs()
		{
			SetupAndNotifyDelivered(PrintType.EML, "", ZGuid.Empty, true);
			Job.SP_RelatedBusinessContext = "";
			Job.SP_JobType = nameof(PrintType.EML);
			Job.MarkEDocsProcessed();
			Assert("Job has been marked as processed", Job.SP_EDocsProcessed);
		}

		public void TestMarkEDocsProcessedEML()
		{
			SetupAndNotifyDelivered(PrintType.EML, "", ZGuid.Empty, true);
			AssertNotDeleted();
			AssertEquals(nameof(PrintType.DDS), Job.SP_JobType);

			Job.MarkEDocsProcessed();
			AssertDeleted();
		}

		public void TestMarkEDocsProcessedFAX()
		{
			SetupAndNotifyDelivered(PrintType.FAX, "", ZGuid.Empty, true);
			AssertNotDeleted();
			AssertEquals(nameof(PrintType.DDS), Job.SP_JobType);

			Job.MarkEDocsProcessed();
			AssertDeleted();
		}

		public void TestMarkEDocsProcessedFTP()
		{
			SetupAndNotifyDelivered(PrintType.FTP, "", ZGuid.Empty, true);
			AssertNotDeleted();
			AssertEquals(nameof(PrintType.DDS), Job.SP_JobType);

			Job.MarkEDocsProcessed();
			AssertDeleted();
		}

		public void TestMarkEDocsProcessedPRN()
		{
			SetupAndNotifyDelivered(PrintType.PRN, "", ZGuid.Empty, true);
			AssertNotDeleted();
			AssertEquals(nameof(PrintType.DDS), Job.SP_JobType);

			Job.MarkEDocsProcessed();
			AssertDeleted();
		}

		public void TestMarkEDocsProcessed_DocManagerDoesNotSupportBusinessContext()
		{
			Setup(PrintType.EML, "", ZGuid.Empty, false);

			Job.MarkEDocsProcessed();
			AssertMarkedAsEdocProcessed();
		}

		public void TestSaveAttachmentToFileSystemDoNotTruncateFileExtension()
		{
			try
			{
				StmDeliveryGroup deliveryGroup = Factory.New<StmDeliveryGroup>();
				deliveryGroup.SB_IsProcessed = true;

				Job = Factory.NewWithValidTestData<StmPrintJob>();
				Job.SP_JobType = nameof(PrintType.EML);
				Job.SP_EmailAttachmentFormat = "FIL";
				Job.SP_Destination = TestEmailAddress;
				Job.SP_SB_DeliveryGroup = deliveryGroup.PK;
				Job.SP_EmailAttachments = "test.xlsx";

				Job.SaveAttachmentToFilesystem(0);
				Assert("Shouldn't truncate file extension", Job.StoredAttachmentFilename.EndsWith("test.xlsx"));
			}
			finally
			{
				Job.DeleteStoredAttachment();
			}
		}

		StmDeliveryGroup DeliveryGroup;
		StmPrintJob Job;

		void Setup(PrintType type, ZString emailFaxDestination, ZGuid queue, bool docManagerSupportedBusinessContext)
		{
			DeliveryGroup = Factory.New<StmDeliveryGroup>();
			DeliveryGroup.SB_IsProcessed = true;

			Job = Factory.NewWithValidTestData<StmPrintJob>();
			Job.SP_JobType = type.ToString();
			Job.SP_EmailAttachmentFormat = "XLS";
			Job.SP_Destination = emailFaxDestination;
			Job.SP_DocumentName = "Test";
			Job.SP_EmailAttachments = "test.XLS";
			Job.SP_EmailSubjectLine = "Subject";
			Job.SP_ParentTableName = "JobShipment";
			Job.SP_ParentGuid = ZGuid.NewZGuid();
			Job.SP_RelatedBusinessContext = docManagerSupportedBusinessContext ? Core.Constants.DocManagerCodes.Shipment : "";
			Job.SP_RunDateTime = ZDateTime.Now;
			Job.SP_RetryAttempts = 2;
			Job.SP_SB_DeliveryGroup = DeliveryGroup.PK;
			Job.SP_EDocsProcessed = false;
		}

		void SetupAndNotifyDelivered(PrintType type, ZString emailFaxDestination, ZGuid queue, bool docManagerSupportedBusinessContext)
		{
			Setup(type, emailFaxDestination, queue, docManagerSupportedBusinessContext);
			Job.NotifyDelivered();
		}

		void AssertDeleted()
		{
			Assert("Job should have been deleted", Job.IsDeleted);
		}

		void AssertNotDeleted()
		{
			Assert("Job should not have been deleted", !Job.IsDeleted);
		}

		void AssertMarkedAsEdocProcessed()
		{
			Assert("Job should have been marked", Job.SP_EDocsProcessed);
		}

		void AssertChangedToDocManager()
		{
			AssertEquals("PrintJob should be a DDS job", nameof(PrintType.DDS), Job.SP_JobType);
			AssertEquals("Retry attempts should be reset to 0", 0, (int)Job.SP_RetryAttempts);
			Assert("Should have a different delivery group to the original print job", Job.SP_SB_DeliveryGroup != DeliveryGroup.PK);
			AssertEquals("Should not have a print queue PK", true, Job.SP_SQ.IsEmpty);
		}

		const string TestEmailAddress = "example@example.example";
		const string TestFaxNumber = "299";
	}
}
