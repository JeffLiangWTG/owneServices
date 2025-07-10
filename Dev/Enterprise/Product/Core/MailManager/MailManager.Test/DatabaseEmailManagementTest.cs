using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MailManager.Testing
{
	public class DatabaseEmailManagementTest : TransactionedTestCase
	{
		public void TestPurgeProcessedIncomingEmailOlderThan()
		{
			int numberDeleted = new DatabaseEmailManagement().PurgeProcessedIncomingEmailOlderThan(7);
			AssertEquals("NumberDeleted", 1, numberDeleted);
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			AssertNotNull(secondFactory.Load(typeof(MailItem), NewSentItem.PK));
			AssertNull(secondFactory.Load(typeof(MailItem), OldReceivedItem.PK));
		}

		public void TestPurgeProcessedOutgoingEmailOlderThan()
		{
			int numberDeleted = new DatabaseEmailManagement().PurgeProcessedOutgoingEmailOlderThan(7);
			AssertEquals("NumberDeleted", 2, numberDeleted);
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			AssertNotNull(secondFactory.Load(typeof(MailItem), NewSentItem.PK));
			AssertNull(secondFactory.Load(typeof(MailItem), OldSentItem.PK));
			AssertNotNull(secondFactory.Load(typeof(MailItem), NewFailedItem.PK));
			AssertNull(secondFactory.Load(typeof(MailItem), OldFailedItem.PK));
		}

		public void TestPurgeUnsentOutgoingEmailOlderThan()
		{
			int numberDeleted = new DatabaseEmailManagement().PurgeUnsentOutgoingEmailOlderThan(7);
			AssertEquals("NumberDeleted", 1, numberDeleted);
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			AssertNotNull(secondFactory.Load(typeof(MailItem), NewSentItem.PK));
			AssertNotNull(secondFactory.Load(typeof(MailItem), OldSentItem.PK));
			AssertNull(secondFactory.Load(typeof(MailItem), OldUnsentItem.PK));
		}

		public void TestPurgeIncomingEmailOlderThan()
		{
			int numberDeleted = new DatabaseEmailManagement().PurgeIncomingEmailOlderThan(7);
			AssertEquals("NumberDeleted", 2, numberDeleted);
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			AssertNotNull(secondFactory.Load(typeof(MailItem), NewSentItem.PK));
			AssertNull(secondFactory.Load(typeof(MailItem), OldReceivedItem.PK));
			AssertNull(secondFactory.Load(typeof(MailItem), OldUnreceivedItem.PK));
		}

		public void TestSetToFailedQueuedWithAckOutgoingEmailOlderThan()
		{
			int numberApplied = new DatabaseEmailManagement().SetToFailedQueuedWithAckOutgoingEmailOlderThan(7);
			AssertEquals("NumberApplied", 1, numberApplied);
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			MailItem newItem = secondFactory.Load<MailItem>(NewQueuedWithAckItem.PK);
			MailItem oldItem = secondFactory.Load<MailItem>(OldQueuedWithAckItem.PK);
			AssertNotNull(oldItem);
			AssertNotNull(newItem);
			AssertEquals("Set Failed", MailStatus.Failed, oldItem.MI_Status);
			AssertEquals("Still QueuedWithAck", MailStatus.QueuedWithAck, newItem.MI_Status);
		}

		public void TestMaximumBatchSizeAndCountLimitsTheRowsToBeCleaned()
		{
			var databaseEmailManagement = new DatabaseEmailManagement()
			{
				MaximumBatchSize = 1,
				MaximumBatchCount = 1,
			};
			int numberDeleted = databaseEmailManagement.PurgeProcessedOutgoingEmailOlderThan(7);
			AssertEquals("NumberDeleted", 1, numberDeleted);
			numberDeleted = databaseEmailManagement.PurgeProcessedOutgoingEmailOlderThan(7);
			AssertEquals("NumberDeleted", 1, numberDeleted);
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			AssertNotNull(secondFactory.Load(typeof(MailItem), NewSentItem.PK));
			AssertNull(secondFactory.Load(typeof(MailItem), OldSentItem.PK));
			AssertNotNull(secondFactory.Load(typeof(MailItem), NewFailedItem.PK));
			AssertNull(secondFactory.Load(typeof(MailItem), OldFailedItem.PK));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(MailDBItemsSchema.Constants.TableName);
			Factory = new BusinessObjectFactory();

			OldSentItem = Factory.New<MailItem>();
			NewSentItem = Factory.New<MailItem>();
			OldUnsentItem = Factory.New<MailItem>();
			NewUnsentItem = Factory.New<MailItem>();
			OldFailedItem = Factory.New<MailItem>();
			NewFailedItem = Factory.New<MailItem>();
			OldReceivedItem = Factory.New<MailItem>();
			NewReceivedItem = Factory.New<MailItem>();
			OldUnreceivedItem = Factory.New<MailItem>();
			NewUnreceivedItem = Factory.New<MailItem>();
			OldQueuedWithAckItem = Factory.New<MailItem>();
			NewQueuedWithAckItem = Factory.New<MailItem>();

			OldSentItem.MI_SendDateTime = ZDateTime.UtcNow.AddDays(-8);
			OldUnsentItem.MI_SendDateTime = ZDateTime.UtcNow.AddDays(-8);
			OldFailedItem.MI_SendDateTime = ZDateTime.UtcNow.AddDays(-8);
			OldReceivedItem.MI_ReceivedDateTime = ZDateTime.UtcNow.AddDays(-8);
			OldUnreceivedItem.MI_ReceivedDateTime = ZDateTime.UtcNow.AddDays(-8);
			OldQueuedWithAckItem.MI_SendDateTime = ZDateTime.UtcNow.AddDays(-8);

			OldSentItem.MI_ReceivedDateTime = ZDateTime.UtcNow.AddDays(-8);
			OldUnsentItem.MI_ReceivedDateTime = ZDateTime.UtcNow.AddDays(-8);
			OldFailedItem.MI_ReceivedDateTime = ZDateTime.UtcNow.AddDays(-8);
			OldReceivedItem.MI_SendDateTime = ZDateTime.UtcNow.AddDays(-8);
			OldUnreceivedItem.MI_SendDateTime = ZDateTime.UtcNow.AddDays(-8);
			OldQueuedWithAckItem.MI_ReceivedDateTime = ZDateTime.UtcNow.AddDays(-8);

			NewSentItem.MI_SendDateTime = ZDateTime.UtcNow.AddDays(-2);
			NewUnsentItem.MI_SendDateTime = ZDateTime.UtcNow.AddDays(-2);
			NewFailedItem.MI_SendDateTime = ZDateTime.UtcNow.AddDays(-2);
			NewReceivedItem.MI_ReceivedDateTime = ZDateTime.UtcNow.AddDays(-2);
			NewUnreceivedItem.MI_ReceivedDateTime = ZDateTime.UtcNow.AddDays(-2);
			NewQueuedWithAckItem.MI_SendDateTime = ZDateTime.UtcNow.AddDays(-2);

			NewSentItem.MI_ReceivedDateTime = ZDateTime.UtcNow.AddDays(-2);
			NewUnsentItem.MI_ReceivedDateTime = ZDateTime.UtcNow.AddDays(-2);
			NewFailedItem.MI_ReceivedDateTime = ZDateTime.UtcNow.AddDays(-2);
			NewReceivedItem.MI_SendDateTime = ZDateTime.UtcNow.AddDays(-2);
			NewUnreceivedItem.MI_SendDateTime = ZDateTime.UtcNow.AddDays(-2);
			NewQueuedWithAckItem.MI_ReceivedDateTime = ZDateTime.UtcNow.AddDays(-2);

			OldReceivedItem.MI_Direction = MailDirection.Receive;
			OldReceivedItem.MI_Status = MailStatus.Processed;

			NewReceivedItem.MI_Direction = MailDirection.Receive;
			NewReceivedItem.MI_Status = MailStatus.Processed;

			OldUnreceivedItem.MI_Direction = MailDirection.Receive;
			OldUnreceivedItem.MI_Status = MailStatus.Queued;

			NewUnreceivedItem.MI_Direction = MailDirection.Receive;
			NewUnreceivedItem.MI_Status = MailStatus.Queued;

			OldQueuedWithAckItem.MI_Direction = MailDirection.Transmit;
			OldQueuedWithAckItem.MI_Status = MailStatus.QueuedWithAck;

			NewQueuedWithAckItem.MI_Direction = MailDirection.Transmit;
			NewQueuedWithAckItem.MI_Status = MailStatus.QueuedWithAck;

			OldSentItem.MI_Direction = MailDirection.Transmit;
			OldSentItem.MI_Status = MailStatus.Sent;

			NewSentItem.MI_Direction = MailDirection.Transmit;
			NewSentItem.MI_Status = MailStatus.Sent;

			OldUnsentItem.MI_Direction = MailDirection.Transmit;
			OldUnsentItem.MI_Status = MailStatus.Queued;

			NewUnsentItem.MI_Direction = MailDirection.Transmit;
			NewUnsentItem.MI_Status = MailStatus.Queued;

			OldFailedItem.MI_Direction = MailDirection.Transmit;
			OldFailedItem.MI_Status = MailStatus.Failed;

			NewFailedItem.MI_Direction = MailDirection.Transmit;
			NewFailedItem.MI_Status = MailStatus.Failed;

			OldQueuedWithAckItem.MI_Subject = "Package20050909_081914_1_1_2078_16777.edp";
			NewQueuedWithAckItem.MI_Subject = "Package20050909_081914_1_1_2078_16777.edp";

			OldQueuedWithAckItem.MI_Body = FullUpgradeBody;
			NewQueuedWithAckItem.MI_Body = FullUpgradeBody;

			Factory.Save();
		}

		protected BusinessObjectFactory Factory;
		protected MailItem OldSentItem;
		protected MailItem NewSentItem;
		protected MailItem OldUnsentItem;
		protected MailItem NewUnsentItem;
		protected MailItem OldFailedItem;
		protected MailItem NewFailedItem;
		protected MailItem OldReceivedItem;
		protected MailItem NewReceivedItem;
		protected MailItem OldUnreceivedItem;
		protected MailItem NewUnreceivedItem;
		protected MailItem OldQueuedWithAckItem;
		protected MailItem NewQueuedWithAckItem;

		protected const string FullUpgradeBody = "EagleTransferEncode2=Package20050909_081914_1_1_2078_16777._52,500000    \r\ng/+*0+mn+%00x+*!";

		#endregion
	}
}
