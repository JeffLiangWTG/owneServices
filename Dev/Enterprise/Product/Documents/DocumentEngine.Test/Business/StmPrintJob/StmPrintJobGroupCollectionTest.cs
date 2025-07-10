using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	sealed class StmPrintJobGroupCollectionTest : TestCaseWithFactory
	{
		public void TestAdd()
		{
			AssertEquals("No elements in array", 0, groupCollection.Count);
			groupCollection.Add(contents1);
			AssertEquals("One element in array", 1, groupCollection.Count);
			AssertEquals("Same element", contents1, groupCollection[0]);
			groupCollection.Add(contents2);
			AssertEquals("Two elements in array", 2, groupCollection.Count);
			AssertEquals("Same element in correct order", contents2, groupCollection[1]);
		}

		public void TestRemove()
		{
			AssertEquals("No elements in array", 0, groupCollection.Count);
			groupCollection.Add(contents1);
			AssertEquals("One element in array", 1, groupCollection.Count);
			groupCollection.Remove(contents1);
			AssertEquals("No elements in array", 0, groupCollection.Count);
		}

		public void TestCount()
		{
			AssertEquals("No elements in array", 0, groupCollection.Count);
			groupCollection.Add(contents1);
			AssertEquals("One element in array", 1, groupCollection.Count);
			groupCollection.Add(contents2);
			AssertEquals("Two elements in array", 2, groupCollection.Count);
		}

		public void TestContains()
		{
			Assert("Element not in collection", !groupCollection.Contains(contents1));
			Assert("Element not in collection", !groupCollection.Contains(contents2));
			groupCollection.Add(contents1);
			Assert("Element is now in collection", groupCollection.Contains(contents1));
			Assert("Element not in collection", !groupCollection.Contains(contents2));
		}

		public void TestIndexOf()
		{
			AssertEquals("Index of element not in there yet: ", -1, groupCollection.IndexOf(contents1));
			groupCollection.Add(contents1);
			groupCollection.Add(contents2);
			AssertEquals("Index of element should be 0", 0, groupCollection.IndexOf(contents1));
			AssertEquals("Index of element should be 1", 1, groupCollection.IndexOf(contents2));
		}

		public void TestIndexer()
		{
			groupCollection.Add(contents1);
			groupCollection.Add(contents2);

			AssertEquals("Indexer should return correct collection element in 0 position", contents1, groupCollection[0]);
			AssertEquals("Indexer should return correct collection element in 1 position", contents2, groupCollection[1]);
		}

		public void TestGetMergedPrintCollection()
		{
			ZGuid deliveryGroupID = ZGuid.NewZGuid();
			groupCollection.Add(contents1);
			groupCollection.Add(contents2);

			contents1PrintJob1.SP_JobType = "EML";
			contents1PrintJob1.SP_Destination = "test@example.com";
			contents1PrintJob1.SP_SB_DeliveryGroup = deliveryGroupID;

			StmPrintJob printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = "EML";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_SB_DeliveryGroup = deliveryGroupID;

			StmPrintJobMergedCollection returnCollection = groupCollection.GetMergedPrintCollection(printJob);

			AssertEquals("Should return the existing collection ", contents1, returnCollection);
			AssertEquals("GroupCollection's count should remain at 2 (unchanged)", 2, groupCollection.Count);

			StmPrintJob printJobThatDoesNotMerge = Factory.New<StmPrintJob>();
			printJobThatDoesNotMerge.SP_JobType = "PRN";
			printJobThatDoesNotMerge.SP_SB_DeliveryGroup = deliveryGroupID;

			returnCollection = groupCollection.GetMergedPrintCollection(printJobThatDoesNotMerge);
			AssertNull("Should return null - no collection matched", returnCollection);
			AssertEquals("Group collections count should stay the same at 2", 2, groupCollection.Count);
		}

		public void TestAddNewMergedPrintCollection()
		{
			ZGuid deliveryGroupID = ZGuid.NewZGuid();
			groupCollection.Add(contents1);
			groupCollection.Add(contents2);

			contents1PrintJob1.SP_JobType = "EML";
			contents1PrintJob1.SP_Destination = "test@example.com";
			contents1PrintJob1.SP_SB_DeliveryGroup = deliveryGroupID;

			StmPrintJobMergedCollection returnCollection = groupCollection.AddNewMergedPrintCollection(contents1PrintJob1);

			AssertEquals("GroupCollection count should increase by 1", 3, groupCollection.Count);
			Assert("GroupCollection should contain the collection returned", groupCollection.Contains(returnCollection));
			Assert("the collection just added should have the PrintJob in it", returnCollection.Contains(contents1PrintJob1.PK));
		}

		public void TestGetCollectionToFit()
		{
			AssertNull("No elements in the group collection yet, so GetCollectionToFit will return null", groupCollection.GetCollectionToFit(50, 500));

			groupCollection.Add(contents1);
			contents1PrintJob1.StoredAttachmentSizeKB = 50;
			contents1PrintJob2.StoredAttachmentSizeKB = 50;
			contents1PrintJob3.StoredAttachmentSizeKB = 50;
			AssertNull("all elements are full to capacity, so GetCollectionToFit will return null", groupCollection.GetCollectionToFit(30, 150));

			groupCollection.Add(contents2);
			AssertEquals("second collection will be returned, because it has room to fit", contents2, groupCollection.GetCollectionToFit(30, 150));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergePrintJobsIntoDeliveryGroupsUsesSameFactoryPassedIn()
		{
			ZString xLSFile = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestXLSFileName;

			StmPrintQueue queue = Factory.New<StmPrintQueue>();
			queue.SQ_ServerName = System.Environment.MachineName;
			queue.SQ_QueueName = "EDI Image Printer";
			queue.SQ_DisplayName = "EDI Image Printer";
			queue.SQ_AllowPrinting = true;

			StmPrintJob printJob = Factory.New<StmPrintJob>();
			printJob.SP_SB_DeliveryGroup = CreateNewDeliveryGroup(null).PK;
			printJob.SP_JobType = "PRN";
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = "";
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_CustomProperties = File.ReadAllBytes(xLSFile);
			printJob.SP_SQ = queue.PK;

			StmPrintJobCollection collection = new StmPrintJobCollection(Factory);
			collection.Add(printJob);

			StmPrintJobGroupCollection printJobsByRecipient = StmPrintJobGroupCollection.MergePrintJobsByRecipient((StmPrintJob[])collection.ToArray(typeof(StmPrintJob)));
			AssertEquals("DeliveryGroupCollection element's factory is the same one that was passed in from the print job", printJobsByRecipient[0].Factory, printJob.Factory);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergePrintJobsIntoDeliveryGroupsPRNJobsNOMerge()
		{
			ZString xLSFile = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestXLSFileName;

			StmDeliveryGroup deliveryGroup1 = CreateNewDeliveryGroup("testing");

			StmPrintQueue queue = Factory.New<StmPrintQueue>();
			queue.SQ_ServerName = System.Environment.MachineName;
			queue.SQ_QueueName = "EDI Image Printer";
			queue.SQ_DisplayName = "EDI Image Printer";
			queue.SQ_AllowPrinting = true;

			StmPrintJob printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = "PRN";
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = "";
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_SB_DeliveryGroup = deliveryGroup1.PK;
			printJob.SP_SQ = queue.PK;

			StmPrintJob printJob2 = Factory.New<StmPrintJob>();
			printJob2.SP_JobType = "PRN";
			printJob2.SP_EmailAttachmentFormat = "XLS";
			printJob2.SP_FaxDestination = "";
			printJob2.SP_EmailAttachments = "test.XLS";
			printJob2.SP_SB_DeliveryGroup = deliveryGroup1.PK;
			printJob2.SP_SQ = queue.PK;

			StmPrintJob printJob3 = Factory.New<StmPrintJob>();
			printJob3.SP_JobType = "PRN";
			printJob3.SP_EmailAttachmentFormat = "XLS";
			printJob3.SP_Destination = "";
			printJob3.SP_EmailAttachments = "test.XLS";
			printJob3.SP_SB_DeliveryGroup = deliveryGroup1.PK;
			printJob3.SP_SQ = queue.PK;

			StmPrintJobCollection collection = new StmPrintJobCollection(Factory);
			collection.Add(printJob);
			collection.Add(printJob2);
			collection.Add(printJob3);

			StmPrintJobGroupCollection printJobsByRecipient = StmPrintJobGroupCollection.MergePrintJobsByRecipient((StmPrintJob[])collection.ToArray(typeof(StmPrintJob)));
			AssertEquals("Three collections in the group collection for DeliveryGroupID 1", 3, printJobsByRecipient.Count);
			AssertEquals("Each collection in the group collection has just one print job", 1, printJobsByRecipient[0].Count);
			AssertEquals("Each collection in the group collection has just one print job", 1, printJobsByRecipient[1].Count);
			AssertEquals("Each collection in the group collection has just one print job", 1, printJobsByRecipient[2].Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergePrintJobsIntoDeliveryGroupsEMLJobs()
		{
			ZString xLSFile = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestXLSFileName;

			StmDeliveryGroup deliveryGroup1 = CreateNewDeliveryGroup("testing");

			StmPrintJob printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_SB_DeliveryGroup = deliveryGroup1.PK;

			StmPrintJob printJob2 = Factory.New<StmPrintJob>();
			printJob2.SP_JobType = "EML";
			printJob2.SP_EmailAttachmentFormat = "XLS";
			printJob2.SP_Destination = "test@example.com";
			printJob2.SP_EmailAttachments = "test.XLS";
			printJob2.SP_SB_DeliveryGroup = deliveryGroup1.PK;

			StmPrintJob printJob3 = Factory.New<StmPrintJob>();
			printJob3.SP_JobType = "EML";
			printJob3.SP_EmailAttachmentFormat = "XLS";
			printJob3.SP_Destination = "testing@example.com";
			printJob3.SP_EmailAttachments = "test.XLS";
			printJob3.SP_SB_DeliveryGroup = deliveryGroup1.PK;

			StmPrintJobCollection collection = new StmPrintJobCollection(Factory);
			collection.Add(printJob);
			collection.Add(printJob2);
			collection.Add(printJob3);

			StmPrintJobGroupCollection printJobsByRecipient = StmPrintJobGroupCollection.MergePrintJobsByRecipient((StmPrintJob[])collection.ToArray(typeof(StmPrintJob)));
			AssertEquals("two collections in the group collection for DeliveryGroupID 1", 2, printJobsByRecipient.Count);
			AssertEquals("two print jobs in the 1st collection, because they should be merged and sent together (test@example.com)", 2, printJobsByRecipient[0].Count);
			AssertEquals("one print job in the 2nd collection, different recipient(testing@example.com)", 1, printJobsByRecipient[1].Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergePrintJobsIntoDeliveryGroupsFAXJobs()
		{
			ZString xLSFile = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestXLSFileName;

			StmDeliveryGroup deliveryGroup1 = CreateNewDeliveryGroup("testing");

			StmPrintJob printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = "FAX";
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = "90251199";
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_SB_DeliveryGroup = deliveryGroup1.PK;

			StmPrintJob printJob2 = Factory.New<StmPrintJob>();
			printJob2.SP_JobType = "FAX";
			printJob2.SP_EmailAttachmentFormat = "XLS";
			printJob2.SP_Destination = "90251199";
			printJob2.SP_EmailAttachments = "test.XLS";
			printJob2.SP_SB_DeliveryGroup = deliveryGroup1.PK;

			StmPrintJobCollection collection = new StmPrintJobCollection(Factory);
			collection.Add(printJob);
			collection.Add(printJob2);

			StmPrintJobGroupCollection printJobsByRecipient = StmPrintJobGroupCollection.MergePrintJobsByRecipient((StmPrintJob[])collection.ToArray(typeof(StmPrintJob)));
			AssertEquals("Two collections in the group collection for DeliveryGroupID 1", 2, printJobsByRecipient.Count);
			AssertEquals("Each collection in the group collection has just one print job", 1, printJobsByRecipient[0].Count);
			AssertEquals("Each collection in the group collection has just one print job", 1, printJobsByRecipient[1].Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			groupCollection = new StmPrintJobGroupCollection();
			contents1 = new StmPrintJobMergedCollection(Factory);
			contents1PrintJob1 = contents1.AddNew();
			contents1PrintJob2 = contents1.AddNew();
			contents1PrintJob3 = contents1.AddNew();

			contents2 = new StmPrintJobMergedCollection(Factory);
			contents2.AddNew();
			contents2.AddNew();
			contents2.AddNew();
		}

		StmPrintJobGroupCollection groupCollection;
		StmPrintJobMergedCollection contents1;
		StmPrintJobMergedCollection contents2;

		StmPrintJob contents1PrintJob1;
		StmPrintJob contents1PrintJob2;
		StmPrintJob contents1PrintJob3;

		StmDeliveryGroup CreateNewDeliveryGroup(string subjectLine)
		{
			var deliveryGroup = Factory.New<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;
			if (!string.IsNullOrEmpty(subjectLine))
			{
				deliveryGroup.SB_EmailSubjectLine = subjectLine;
			}
			return deliveryGroup;
		}
	}
}
