using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.Test;

namespace Enterprise.MailManager.Testing
{
	sealed class MailManagerEmailAcknowledgementTest : TestCaseWithFactory
	{
		public void TestProcessAnyAcknowledgementEmailsSuccessfully()
		{
			MailItem item = GetSystemUpdateMailItem();
			MailItem ackItem = GetAcknowledgementMailItem(item);
			Factory.Save();

			var helper = new MessageFilterTestHelper<MailManagerEmailAcknowledgement, MailItem>();
			bool result = helper.Process(ackItem);

			AssertEquals("Procesed email", true, result);

			MailItem item2 = GetSystemUpdateMailItem();
			MailItem ackItem2 = GetAcknowledgementMailItem(item2);
			MailItem ackItem1 = GetAcknowledgementMailItem1(item);
			Factory.Save();

			result = helper.Process(ackItem2);
			AssertEquals("Procesed emails", true, result);
		}

		public void TestProcessAnyAcknowledgementEmailsFailure()
		{
			MailItem item = GetSystemUpdateMailItem();
			MailItem ackItem = GetAcknowledgementMailItemFail(item);
			Factory.Save();

			var helper = new MessageFilterTestHelper<MailManagerEmailAcknowledgement, MailItem>();
			bool result = helper.Process(ackItem);
			AssertEquals("Procesed no ACK email", false, result);

			MailItem ackItem1 = GetAcknowledgementMailItemFail1(item);
			Factory.Save();

			result = helper.Process(ackItem1);
			AssertEquals("Procesed wrong GUID email", false, result);

			MailItem ackItem3 = GetAcknowledgementMailItem1(item);
			ackItem3.MI_From = "1@2.com";
			Factory.Save();

			result = helper.Process(ackItem3);
			AssertEquals("Procesed no emails", false, result);
		}

		public void TestProcessAnyAcknowledgementEmailsEx()
		{
			MailItem item1 = GetSystemUpdateMailItem();
			MailItem ackItem1 = GetAcknowledgementMailItem(item1);
			ackItem1.MI_Subject = "ACK Something";

			MailItem item2 = GetSystemUpdateMailItem();
			MailItem ackItem2 = GetAcknowledgementMailItem(item2);
			ackItem2.MI_Subject = "ACK GUID:0123456789012345678901234567890123456";

			MailItem item3 = GetSystemUpdateMailItem();
			MailItem ackItem3 = GetAcknowledgementMailItem(item3);
			ackItem3.MI_Subject = "ACK Package20041023_170651_1_1_1757_32169.edp from	system_update@edi.net.au GUID:" + item3.PK.ToString() + "[Some crap at the end]";

			Factory.Save();

			var helper = new MessageFilterTestHelper<MailManagerEmailAcknowledgement, MailItem>();
			bool result1 = helper.Process(ackItem1);
			AssertEquals(false, result1);

			bool result2 = helper.Process(ackItem2);
			AssertEquals(false, result2);

			string expectedMessage = "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).";
			AssertEquals("Expected message was not reported", true, ErrorReporter.LastMessageReported.StartsWith(expectedMessage));
			ErrorReporter.Clear();

			bool result3 = helper.Process(ackItem3);
			AssertEquals(true, result3);
		}

		public void TestProcessAcknowledgementEmailMessageFilter()
		{
			MailItem item1 = GetSystemUpdateMailItem();
			MailItem item2 = GetSystemUpdateMailItem();
			Factory.Save();

			var helper = new MessageFilterTestHelper<MailManagerEmailAcknowledgement, MailItem>();

			MailItem ackItem1 = GetAcknowledgementMailItem(item1);
			ackItem1.MI_Subject = "ACK Something";
			Assert(!helper.Process(ackItem1));
			AssertEquals(0, helper.Log.Count);
			AssertEquals(MailStatus.QueuedWithAck, item1.MI_Status);

			MailItem ackItem2a = GetAcknowledgementMailItem(item2);
			Assert(helper.Process(ackItem2a));
			AssertEquals(1, helper.Log.Count);
			AssertEquals("Debug|Acknowledgement email processed.", helper.Log[0]);
			AssertEquals(MailStatus.QueuedWithAck, item2.MI_Status);

			MailItem ackItem2b = GetAcknowledgementMailItem1(item2);
			Assert(helper.Process(ackItem2b));
			AssertEquals(2, helper.Log.Count);
			AssertEquals("Debug|Acknowledgement email processed.", helper.Log[1]);
			AssertEquals(MailStatus.Sent, item2.MI_Status);
		}

		#region Implementation

		MailItem GetSystemUpdateMailItem()
		{
			MailItem result = Factory.New<MailItem>();
			result.MI_Status = MailStatus.QueuedWithAck;
			result.MI_Direction = MailDirection.Transmit;
			result.MI_SendDateTime = ZDateTime.UtcNow;
			result.MI_ReceivedDateTime = ZDateTime.UtcNow;
			result.AddRecipientForUserCommunication("Deliverance Update <client-address@example.com>", MailRecipient.RecipientTypes.TO);
			result.AddRecipientForUserCommunication("Deliverance Update <client-address1@example.com>", MailRecipient.RecipientTypes.CC);
			result.MI_From = "address@edi.com.au";
			result.MI_Subject = "filename.ext from address@edi.com.au GUID:" + result.PK.ToString();

			return result;
		}

		MailItem GetAcknowledgementMailItem(MailItem item)
		{
			MailItem result = Factory.New<MailItem>();
			result.MI_Status = MailStatus.Queued;
			result.MI_Direction = MailDirection.Receive;
			result.MI_SendDateTime = ZDateTime.UtcNow;
			result.MI_ReceivedDateTime = ZDateTime.UtcNow;
			result.AddRecipientForUserCommunication("address@edi.com.au", MailRecipient.RecipientTypes.TO);
			result.MI_From = "Client <client-address@example.com>";
			result.MI_Subject = "ACK " + item.MI_Subject;

			return result;
		}

		MailItem GetAcknowledgementMailItem1(MailItem item)
		{
			MailItem result = Factory.New<MailItem>();
			result.MI_Status = MailStatus.Queued;
			result.MI_Direction = MailDirection.Receive;
			result.MI_SendDateTime = ZDateTime.UtcNow;
			result.MI_ReceivedDateTime = ZDateTime.UtcNow;
			result.AddRecipientForUserCommunication("address@edi.com.au", MailRecipient.RecipientTypes.TO);
			result.MI_From = "client-address1@example.com";
			result.MI_Subject = "ACK " + item.MI_Subject;

			return result;
		}

		MailItem GetAcknowledgementMailItemFail(MailItem item)
		{
			MailItem result = Factory.New<MailItem>();
			result.MI_Status = MailStatus.Queued;
			result.MI_Direction = MailDirection.Receive;
			result.MI_SendDateTime = ZDateTime.UtcNow;
			result.MI_ReceivedDateTime = ZDateTime.UtcNow;
			result.AddRecipientForUserCommunication("address@edi.com.au", MailRecipient.RecipientTypes.TO);
			result.MI_From = "Deliverance Update <client-address@example.com>";
			result.MI_Subject = item.MI_Subject;

			return result;
		}

		MailItem GetAcknowledgementMailItemFail1(MailItem item)
		{
			MailItem result = Factory.New<MailItem>();
			result.MI_Status = MailStatus.Queued;
			result.MI_Direction = MailDirection.Receive;
			result.MI_SendDateTime = ZDateTime.UtcNow;
			result.MI_ReceivedDateTime = ZDateTime.UtcNow;
			result.AddRecipientForUserCommunication("address@edi.com.au", MailRecipient.RecipientTypes.TO);
			result.MI_From = "Deliverance Update <client-address@example.com>";
			result.MI_Subject = "ACK filename.ext from address@edi.com.au GUID:" + ZGuid.NewZGuid().ToString();

			return result;
		}

		#endregion
	}
}
