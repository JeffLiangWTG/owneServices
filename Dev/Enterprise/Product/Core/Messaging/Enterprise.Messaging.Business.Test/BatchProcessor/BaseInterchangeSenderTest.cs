using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Messaging.Business.Testing
{
	public abstract class BaseInterchangeSenderTest : TestCaseWithFactory
	{
		protected abstract EDIInterchange GetNewEDIInterchangeReadyToSend();
		protected abstract EDIMessage GetNewEDIMessageReadyToSend();
		protected abstract BaseInterchangeSender GetNewSender();

		#region TestSendInterchange
		public void TestSendInterchange()
		{
			EDIInterchange interchange = GetNewEDIInterchangeReadyToSend();
			Factory.Save();

			Sender.ExecuteBatch();

			interchange.Reload();
			AssertEquals("InterchangeState", EDIInterchange.Status.Sent, interchange.EI_Status);
		}
		#endregion

		#region TestSendMessage

		public virtual void TestSendMessage()
		{
			EDIMessage testMessage = GetNewEDIMessageReadyToSend();
			Factory.Save();

			Sender.ExecuteBatch();

			testMessage.Reload();
			AssertEquals("MessageState", EDIMessage.Status.Sent, testMessage.EM_Status);
			EDIInterchange interchange = testMessage.Interchange;
			AssertEquals("InterchangeState", EDIInterchange.Status.Sent, interchange.EI_Status);
		}

		#endregion

		#region TestSendMessage

		public virtual void TestProcess()
		{
			EDIMessage testMessage = GetNewEDIMessageReadyToSend();
			Factory.Save();

			NotificationBuffer buffer = new NotificationBuffer();
			((IProcessor)Sender).Process(buffer);

			testMessage.Reload();
			AssertEquals("MessageState", EDIMessage.Status.Sent, testMessage.EM_Status);
			AssertEquals("InterchangeState", EDIInterchange.Status.Sent, testMessage.Interchange.EI_Status);
			Assert("buffer should have been used for logging", buffer.AsString.Length > 0);
		}

		#endregion

		#region TestMessagesWithEM_HeldUntilDateNotSentUntilTheRightDate

		public virtual void TestMessagesWithEM_HeldUntilDateNotSentUntilTheRightDate()
		{
			EDIMessage testMessage = GetNewEDIMessageReadyToSend();
			testMessage.EM_HeldUntilDate = ZDateTime.Now.AddMinutes(3);
			Factory.Save();

			Sender.ExecuteBatch();

			testMessage.Reload();
			AssertEquals("MessageState", EDIMessage.Status.Queued, testMessage.EM_Status);
			AssertNull("TestMessage.Interchange", testMessage.Interchange);
		}

		#endregion

		#region TestMessagesWithoutEM_HeldUntilDateGetSentRightNow

		public virtual void TestMessagesWithoutEM_HeldUntilDateGetSentRightNow()
		{
			EDIMessage testMessage = GetNewEDIMessageReadyToSend();
			testMessage.EM_HeldUntilDate = ZDateTime.Now.AddMinutes(-3);
			Factory.Save();

			Sender.ExecuteBatch();

			testMessage.Reload();
			AssertEquals("MessageState", EDIMessage.Status.Sent, testMessage.EM_Status);
			EDIInterchange interchange = testMessage.Interchange;
			AssertEquals("InterchangeState", EDIInterchange.Status.Sent, interchange.EI_Status);
		}
		#endregion

		#region SetUp
		protected override void SetUp()
		{
			base.SetUp();
			Sender = GetNewSender();
		}
		protected BaseInterchangeSender Sender;
		#endregion
	}
}
