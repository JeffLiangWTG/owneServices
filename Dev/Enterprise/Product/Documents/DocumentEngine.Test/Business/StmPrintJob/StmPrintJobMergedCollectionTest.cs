using System.Data;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestedType(typeof(StmPrintJobMergedCollection))]
	sealed class StmPrintJobMergedCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmPrintJobMergedCollection(Factory);
		}

		protected override void SetUp()
		{
			Collection = GetCollectionToTest() as StmPrintJobMergedCollection;
			base.SetUp();
		}

		new StmPrintJobMergedCollection Collection;

		class StmPrintJobDeleteOnDeliveryMock : StmPrintJob
		{
			public StmPrintJobDeleteOnDeliveryMock(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool DeliveryNotified;

			internal override void NotifyDelivered()
			{
				DeliveryNotified = true;

				// This tests the way the collection loops over jobs - can't use foreach because we may modify the collection
				Delete();
			}
		}

		public void TestNotifyAllJobsDelivered()
		{
			StmPrintJobDeleteOnDeliveryMock mock1, mock2;
			mock1 = Factory.New<StmPrintJobDeleteOnDeliveryMock>();
			mock2 = Factory.New<StmPrintJobDeleteOnDeliveryMock>();

			Collection.Add(mock1);
			Collection.Add(mock2);
			Collection.NotifyAllJobsDelivered();

			Assert(mock1.DeliveryNotified);
			Assert(mock2.DeliveryNotified);
		}

		public void TestMarkAllJobsAsProcessed()
		{
			var printJob1 = Factory.New<StmPrintJob>();
			printJob1.SP_RelatedBusinessContext = Core.Constants.DocManagerCodes.Shipment;
			var printJob2 = Factory.New<StmPrintJob>();
			printJob2.SP_RelatedBusinessContext = Core.Constants.DocManagerCodes.Shipment;

			Collection.Add(printJob1);
			Collection.Add(printJob2);
			Collection.MarkAllEDocsProcessed();

			Assert("The print job number 1 should be marked as EDocsProcessed", printJob1.SP_EDocsProcessed);
			Assert("The print job number 2 should be marked as EDocsProcessed", printJob2.SP_EDocsProcessed);
		}

		public void TestJobType()
		{
			AssertEquals("Collection's job type empty by default", ZString.Empty, Collection.JobType);

			StmPrintJob printJob1 = Collection.AddNew();
			printJob1.SP_JobType = "PRN";

			AssertEquals("Collection's Job Type", "PRN", Collection.JobType);

			printJob1.SP_JobType = "EML";
			AssertEquals("Collection's Job Type", "EML", Collection.JobType);
		}

		public void TestEDocsProcessed()
		{
			AssertEquals("Collection's job EDocsProcessed false by default", ZString.Empty, Collection.JobType);

			var printJob1 = Collection.AddNew();
			printJob1.SP_EDocsProcessed = false;

			AssertEquals("Collection's EDocsProcessed", false, Collection.EDocsProcessed);

			printJob1.SP_EDocsProcessed = true;
			AssertEquals("Collection's EDocsProcessed", true, Collection.EDocsProcessed);
		}

		public void TestShouldSaveToEDocs()
		{
			AssertEquals("Collection's job ShouldSaveToEDocs false by default", false, Collection.ShouldSaveToEDocs);

			var printJob1 = Collection.AddNew();
			printJob1.SP_RelatedBusinessContext = "BLA";

			AssertEquals("Collection should not save to eDocs", false, Collection.ShouldSaveToEDocs);

			printJob1.SP_RelatedBusinessContext = Core.Constants.DocManagerCodes.Shipment;
			AssertEquals("Collection should now save to eDocs", true, Collection.ShouldSaveToEDocs);
		}

		public void TestRetryAttempts()
		{
			StmPrintJob printJob1 = Collection.AddNew();
			AssertEquals("Collection's retry attempts by default", 0, Collection.RetryAttempts);

			printJob1.SP_RetryAttempts = 1;

			AssertEquals("Collection's retry attempts", 1, Collection.RetryAttempts);

			printJob1.SP_RetryAttempts = 2;
			AssertEquals("Collection's retry attempts", 2, Collection.RetryAttempts);
		}

		public void TestDeliveryGroupPK()
		{
			StmPrintJob printJob1 = Collection.AddNew();
			StmDeliveryGroup group = Factory.New<StmDeliveryGroup>();
			printJob1.SP_SB_DeliveryGroup = group.PK;
			AssertEquals("Collection's delivery group pk", group.PK, Collection.DeliveryGroupPK);

			printJob1.SP_SB_DeliveryGroup = ZGuid.Empty;
			AssertEquals("Collection's delivery group pk", ZGuid.Empty, Collection.DeliveryGroupPK);
		}

		public void TestDeliveryGroup()
		{
			StmPrintJob printJob1 = Collection.AddNew();
			StmDeliveryGroup group = Factory.New<StmDeliveryGroup>();
			printJob1.SP_SB_DeliveryGroup = group.PK;
			AssertEquals("Collection's delivery group same instance", group, Collection.DeliveryGroup);

			printJob1.SP_SB_DeliveryGroup = ZGuid.Empty;
			AssertEquals("Collection's delivery group should be null", null, Collection.DeliveryGroup);
		}

		public void TestSizeInKB()
		{
			StmPrintJob printJob1 = Factory.New<StmPrintJob>();
			printJob1.StoredAttachmentSizeKB = 500;
			Collection.Add(printJob1);

			AssertEquals("Size in KB should be sum of all print jobs in the collection", 500, Collection.SizeInKB);

			StmPrintJob printJob2 = Factory.New<StmPrintJob>();
			printJob2.StoredAttachmentSizeKB = 200;
			Collection.Add(printJob2);

			AssertEquals("Size in KB should be sum of all print jobs in the collection", 700, Collection.SizeInKB);
		}

		public void TestIsUnderSizeLimit()
		{
			StmPrintJob printJob1 = Factory.New<StmPrintJob>();
			printJob1.StoredAttachmentSizeKB = 500;
			StmPrintJob printJob2 = Factory.New<StmPrintJob>();
			printJob2.StoredAttachmentSizeKB = 200;

			Collection.Add(printJob1);
			Collection.Add(printJob2);

			StmPrintJob printJobToAdd = Factory.New<StmPrintJob>();
			printJobToAdd.StoredAttachmentSizeKB = 200;
			Assert("IsUnderSizeLimit should return true for size limit of 1000", Collection.IsUnderSizeLimit(printJobToAdd, 1000));

			printJobToAdd.StoredAttachmentSizeKB = 299;
			Assert("IsUnderSizeLimit should return true for size limit of 1000", Collection.IsUnderSizeLimit(printJobToAdd, 1000));

			printJobToAdd.StoredAttachmentSizeKB = 300;
			Assert("IsUnderSizeLimit should return false for size limit of 1000", !Collection.IsUnderSizeLimit(printJobToAdd, 1000));
		}

		public void TestIncrementRetryAttempts()
		{
			using (TempFile tempFile1 = TempFile.New())
			using (TempFile tempFile2 = TempFile.New())
			{
				string tempFilename1 = tempFile1.Filename;
				string tempFilename2 = tempFile2.Filename;

				StmPrintJob printJob1 = Collection.AddNew();
				printJob1.StoredAttachmentFilename = tempFilename1;
				StmPrintJob printJob2 = Collection.AddNew();
				printJob2.StoredAttachmentFilename = tempFilename2;

				AssertEquals("Precondition: Retry attempts at 0", 0, (int)printJob1.SP_RetryAttempts);
				AssertEquals("Precondition: Retry attempts at 0", 0, (int)printJob2.SP_RetryAttempts);
				Collection.IncrementRetryAttempts();

				AssertEquals("Retry attempts should be incremented", 1, (int)printJob1.SP_RetryAttempts);
				Assert("Stored filename should not exist anymore", !File.Exists(tempFilename1));
				AssertEquals("Retry attempts should be incremented", 1, (int)printJob2.SP_RetryAttempts);
				Assert("Stored filename should not exist anymore", !File.Exists(tempFilename2));

				Collection.IncrementRetryAttempts();
				AssertEquals(2, (int)printJob1.SP_RetryAttempts);
				AssertEquals(nameof(PrintJobStatus.QUE), printJob1.SP_Status);
				AssertEquals(2, (int)printJob2.SP_RetryAttempts);
				AssertEquals(nameof(PrintJobStatus.QUE), printJob2.SP_Status);

				Collection.IncrementRetryAttempts();
				AssertEquals(3, (int)printJob1.SP_RetryAttempts);
				AssertEquals(nameof(PrintJobStatus.FAL), printJob1.SP_Status);
				AssertEquals(3, (int)printJob2.SP_RetryAttempts);
				AssertEquals(nameof(PrintJobStatus.FAL), printJob2.SP_Status);
			}
		}

		public void TestDeleteStoredAttachments()
		{
			using (TempFile tempFile1 = TempFile.New())
			using (TempFile tempFile2 = TempFile.New())
			{
				string tempFilename1 = tempFile1.Filename;
				string tempFilename2 = tempFile2.Filename;

				StmPrintJob printJob1 = Collection.AddNew();
				printJob1.StoredAttachmentFilename = tempFilename1;
				StmPrintJob printJob2 = Collection.AddNew();
				printJob2.StoredAttachmentFilename = tempFilename2;

				Collection.DeleteStoredAttachments();
				Assert("Stored filename should not exist anymore", !File.Exists(tempFilename1));
				Assert("Stored filename should not exist anymore", !File.Exists(tempFilename2));
			}
		}
	}
}
