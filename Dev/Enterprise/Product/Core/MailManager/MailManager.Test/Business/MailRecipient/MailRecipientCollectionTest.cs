using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MailManager.Business
{
	[TestedType(typeof(MailRecipientCollection))]
	sealed class MailRecipientCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestMailRecipientCollection()
		{
			AssertNotNull("Collection", new MailRecipientCollection(MyItem, MyItem.Factory));
		}

		public void TestGetMailRecipient()
		{
			MyCollection.Add(Recipient1);
			AssertEquals("MyCollection[0]", Recipient1, MyCollection[0]);
		}

		public void TestSetMailRecipient()
		{
			TestGetMailRecipient();
		}

		public void TestHasRecipientsWithSendAttemptsRemaining()
		{
			Recipient1.MR_AckAttempt = 1;
			Recipient2.MR_AckAttempt = 1;
			Recipient3.MR_AckAttempt = 1;
			AssertEquals("HasRecipientsWithSendAttemptsRemaining", true, MyCollection.HasRecipientsWithSendAttemptsRemaining);
			Recipient1.MR_AckAttempt = 0;
			Recipient2.MR_AckAttempt = 100;
			Recipient3.MR_AckAttempt = 0;
			AssertEquals("HasRecipientsWithSendAttemptsRemaining", true, MyCollection.HasRecipientsWithSendAttemptsRemaining);
			Recipient1.MR_AckAttempt = 0;
			Recipient2.MR_AckAttempt = 0;
			Recipient3.MR_AckAttempt = 0;
			AssertEquals("HasRecipientsWithSendAttemptsRemaining", false, MyCollection.HasRecipientsWithSendAttemptsRemaining);
		}

		public override void TestAddNew()
		{
			MyCollection.AddNew("a@b.com", MailRecipient.RecipientTypes.TO);
			AssertEquals("Count", 4, MyCollection.Count);
			AssertEquals("Address", "a@b.com", MyCollection[3].EmailAddress);
			AssertEquals("Type", nameof(MailRecipient.RecipientTypes.TO), MyCollection[3].MR_RecipientType);

			var emailAddress = "TestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalid@example.com";

			AssertExceptionThrown<InvalidMailFormatException>(() => { MyCollection.AddNew(emailAddress, MailRecipient.RecipientTypes.TO); });
		}

		public void TestContainsFailedRecipients()
		{
			AssertEquals("No failed", false, MyCollection.ContainsFailedRecipients);
			MyCollection[0].MR_LastAttempt = ZDateTime.UtcNow.AddMinutes(-MailAcknowledgement.AcknowledgmentTimeout - 1);
			MyCollection[0].MR_AckAttempt = MailAcknowledgement.MaxAttempts;
			Assert("Failed", MyCollection.ContainsFailedRecipients);
		}

		public void TestAllRecipientsSuccessfullyDelivered()
		{
			AssertEquals("No Succesfull", false, MyCollection.AllRecipientsSuccessfullyDelivered);
			MyCollection[0].MR_DeliveredTime = ZDateTime.UtcNow;
			AssertEquals("Not all Succesfull", false, MyCollection.AllRecipientsSuccessfullyDelivered);
			MyCollection[1].MR_DeliveredTime = ZDateTime.UtcNow;
			MyCollection[2].MR_DeliveredTime = ZDateTime.UtcNow;
			Assert("All Succesfull", MyCollection.AllRecipientsSuccessfullyDelivered);
		}

		public void TestContainsWaitingForACKRecipients()
		{
			MyItem.MI_Status = MailStatus.QueuedWithAck;
			AssertEquals("No waiting", false, MyCollection.ContainsWaitingForACKRecipients);
			MyCollection[0].MR_LastAttempt = ZDateTime.UtcNow.AddMinutes(-MailAcknowledgement.AcknowledgmentTimeout + 1);
			MyCollection[0].MR_AckAttempt = MailAcknowledgement.MaxAttempts;
			Assert("Waiting", MyCollection.ContainsWaitingForACKRecipients);
		}

		#region Implemementation
		protected override void SetUp()
		{
			base.SetUp();
			MyItem = Factory.New<DummyMailItem>();
			Recipient1 = Factory.New<MailRecipient>();
			Recipient2 = Factory.New<MailRecipient>();
			Recipient3 = Factory.New<MailRecipient>();

			Recipient1.MR_MI = MyItem.PK;
			Recipient2.MR_MI = MyItem.PK;
			Recipient3.MR_MI = MyItem.PK;

			MyCollection = MyItem.MailRecipientsCore_Exposed;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			MyItem = Factory.New<DummyMailItem>();
			return MyItem.MailRecipientsCore_Exposed;
		}

		MailRecipientCollection MyCollection;
		DummyMailItem MyItem;
		MailRecipient Recipient1;
		MailRecipient Recipient2;
		MailRecipient Recipient3;
		#endregion
	}
}
