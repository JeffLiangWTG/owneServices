using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MailManager.Business
{
	sealed class MailRecipientTest : TransactionedTestCase
	{
		public void TestMailRecipient()
		{
			AssertNotNull("MyRecipient", MyRecipient);
		}

		public void TestMailItem()
		{
			AssertEquals("MailItem.PK", MyItem.PK, MyRecipient.MailItem.PK);
		}

		public void TestDelivered()
		{
			AssertEquals("Not delivered", false, MyRecipient.IsDelivered);

			MyRecipient.MR_DeliveredTime = ZDateTime.UtcNow;
			AssertEquals("Delivered", true, MyRecipient.IsDelivered);
		}

		public void TestWaitingForAcknowledgement()
		{
			MyRecipient.MR_LastAttempt = ZDateTime.UtcNow.AddMinutes(-MailAcknowledgement.AcknowledgmentTimeout - 1);
			AssertEquals("Not WaitingForAcknowledgement", false, MyRecipient.IsWaitingForAcknowledgement);

			MyRecipient.MR_LastAttempt = ZDateTime.UtcNow.AddMinutes(-MailAcknowledgement.AcknowledgmentTimeout + 1);
			MyItem.MI_Status = MailStatus.Queued;
			AssertEquals("Not WaitingForAcknowledgement with QUE status", false, MyRecipient.IsWaitingForAcknowledgement);

			MyItem.MI_Status = MailStatus.QueuedWithAck;
			AssertEquals("WaitingForAcknowledgement with QWA status", true, MyRecipient.IsWaitingForAcknowledgement);
		}

		public void TestFailed()
		{
			AssertEquals("Not failed", false, MyRecipient.IsFailed);

			MyRecipient.MR_AckAttempt = MailAcknowledgement.MaxAttempts;
			MyRecipient.MR_LastAttempt = ZDateTime.UtcNow.AddMinutes(-MailAcknowledgement.AcknowledgmentTimeout - 1);
			AssertEquals("Failed", true, MyRecipient.IsFailed);
		}

		public void TestSuspended()
		{
			MyItem.MI_Status = MailStatus.Queued;
			AssertEquals("Not suspended when status is QUE", false, MyRecipient.IsSuspended);

			MyItem.MI_Status = MailStatus.QueuedWithAck;
			MyRecipient.MR_LastAttempt = ZDateTime.UtcNow;
			AssertEquals("Suspended", true, MyRecipient.IsSuspended);

			MyRecipient.MR_AckAttempt = (ZByte)(MailAcknowledgement.MaxAttempts - 1) >= 0 ? (ZByte)(MailAcknowledgement.MaxAttempts - 1) : (ZByte)0;
			MyRecipient.MR_LastAttempt = ZDateTime.UtcNow.AddMinutes(-MailAcknowledgement.AcknowledgmentTimeout - 1);
			AssertEquals("Not suspended", false, MyRecipient.IsSuspended);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			MyItem = factory.New<DummyMailItem>();
			MyRecipient = MyItem.MailRecipientsCore_Exposed.AddNew();
		}

		DummyMailItem MyItem;
		MailRecipient MyRecipient;
		#endregion
	}
}
