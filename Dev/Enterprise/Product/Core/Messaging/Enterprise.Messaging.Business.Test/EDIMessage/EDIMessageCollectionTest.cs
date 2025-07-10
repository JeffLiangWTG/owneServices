using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Messaging.Business
{
	[TestedType(typeof(EDIMessageCollection))]
	public class EDIMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestFirstOutgoingMessage()
		{
			SetupCollectionWithOutgoingAndIncoming();
			AssertEquals("FirstOutgoingMessage", Outgoing1, MessageCollection.FirstOutgoingMessage);
		}

		public void TestAllowNew()
		{
			var parent = Factory.New(typeof(TestHelperEDIMessage));

			var collection = new EDIMessageCollection(parent, Factory);
			AssertEquals("Doesn't allow new", false, collection.AllowNew);
		}

		public void TestUpdateStatusOfXXXMessagesTo()
		{
			var parent = (EDIMessage)Factory.New(typeof(TestHelperEDIMessage));

			var messages = new EDIMessageCollection(parent, Factory);

			var sent = messages.AddNew();
			sent.EM_Status = EDIMessage.Status.Sent;

			var pending = messages.AddNew();
			pending.EM_Status = EDIMessage.Status.Pending;

			EDIMessage[] result = messages.UpdateStatusOfMatchingMessagesTo(EDIMessage.Status.Cancelled, delegate(EDIMessage msg)
			{ return msg.IsPending; });
			AssertEquals(1, result.Length);
			AssertEquals(pending, result[0]);
			AssertEquals(EDIMessage.Status.Cancelled, result[0].EM_Status);
			AssertEquals(EDIMessage.Status.Sent, sent.EM_Status);  // unchanged

			result = messages.UpdateStatusOfMatchingMessagesTo(EDIMessage.Status.Cancelled, delegate(EDIMessage msg)
			{ return true; });
			AssertEquals(2, result.Length);
			AssertContainsExactElementsInAnyOrder(messages, result);
		}

		public void TestNumberOfOutgoingMessages()
		{
			var parent = Factory.New(typeof(TestHelperEDIMessage));

			var collection = new EDIMessageCollection(parent, Factory);
			var message1 = collection.AddNew();
			var message2 = collection.AddNew();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertEquals("NumberOfOutgoingMessages", 0, collection.NumberOfOutgoingMessages);
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertEquals("NumberOfOutgoingMessages", 1, collection.NumberOfOutgoingMessages);
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertEquals("NumberOfOutgoingMessages", 2, collection.NumberOfOutgoingMessages);
		}

		public void TestGetMatchingMessage()
		{
			var parent = (EDIMessage)Factory.New(typeof(TestHelperEDIMessage));
			var collection = new EDIMessageCollection(parent);

			var message1 = collection.AddNew();
			message1.EM_ApplicationCode = "AAA";
			message1.EM_MessageType = "CCC";
			message1.EM_MessageNum = "1";
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var message2 = collection.AddNew();
			message2.EM_ApplicationCode = "BBB";
			message2.EM_MessageType = "CCC";
			message2.EM_MessageNum = "1";
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var message3 = collection.AddNew();
			message3.EM_ApplicationCode = "BBB";
			message3.EM_MessageType = "DDD";
			message3.EM_MessageNum = "1";
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var message4 = collection.AddNew();
			message4.EM_ApplicationCode = "BBB";
			message4.EM_MessageType = "DDD";
			message4.EM_MessageNum = "2";
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var message5 = collection.AddNew();
			message5.EM_ApplicationCode = "BBB";
			message5.EM_MessageType = "DDD";
			message5.EM_MessageNum = "2";
			message5.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var result = collection.GetMatchingMessage("AAA", "CCC", "1", EDIMessage.Direction.Transmit);
			AssertEquals("Find by 4 combinations", message1, result);

			result = collection.GetMatchingMessage("BBB", "CCC", "1", EDIMessage.Direction.Transmit);
			AssertEquals("Find by 4 combinations", message2, result);

			result = collection.GetMatchingMessage("BBB", "DDD", "1", EDIMessage.Direction.Transmit);
			AssertEquals("Find by 4 combinations", message3, result);

			result = collection.GetMatchingMessage("BBB", "DDD", "2", EDIMessage.Direction.Transmit);
			AssertEquals("Find by 4 combinations", message4, result);

			result = collection.GetMatchingMessage("BBB", "DDD", "2", EDIMessage.Direction.Receive);
			AssertEquals("Find by 4 combinations", message5, result);

			result = collection.GetMatchingMessage("BBB", "DDD", "3", EDIMessage.Direction.Receive);
			AssertEquals("Find by 4 combinations", null, result);
		}

		public void TestLastOutgoingMessage()
		{
			SetupCollectionWithOutgoingAndIncoming();
			AssertEquals("LastOutgoingMessage", Outgoing2, MessageCollection.LastOutgoingMessage);
		}

		public void TestLastIncomingMessage()
		{
			SetupCollectionWithOutgoingAndIncoming();
			AssertEquals("LastIncomingMessage", Incoming2, MessageCollection.LastIncomingMessage);
		}

		public void TestLastSentOutgoingMessage()
		{
			var parent = (EDIMessage)Factory.New(typeof(TestHelperEDIMessage));
			MessageCollection = new EDIMessageCollection(parent, Factory);

			var message = MessageCollection.AddNew();
			AssertEquals("LastSentOutgoingMessage", null, MessageCollection.LastSentOutgoingMessage);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertEquals("LastSentOutgoingMessage", null, MessageCollection.LastSentOutgoingMessage);
			message.EM_Status = EDIMessage.Status.Sent;
			AssertEquals("LastSentOutgoingMessage", message, MessageCollection.LastSentOutgoingMessage);
		}

		public void TestGetLastMessageWithTRXorRCV()
		{
			SetupCollectionWithOutgoingAndIncoming();
			var outgoing3 = MessageCollection.AddNew(typeof(TestHelperEDIMessage));
			outgoing3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing3.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.InterchangeReady);
			System.Threading.Thread.Sleep(1000);

			var incoming3 = MessageCollection.AddNew(typeof(TestHelperEDIMessage));
			incoming3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			System.Threading.Thread.Sleep(1000);

			var outgoing4 = MessageCollection.AddNew(typeof(TestHelperEDIMessage));
			outgoing4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing4.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.InterchangeReady);
			System.Threading.Thread.Sleep(1000);

			var incoming4 = MessageCollection.AddNew(typeof(TestHelperEDIMessage));
			incoming4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			Outgoing1.EM_ApplicationCode = "AP1";
			Incoming1.EM_ApplicationCode = "AP1";
			outgoing3.EM_ApplicationCode = "AP1";
			incoming3.EM_ApplicationCode = "AP1";

			Outgoing2.EM_ApplicationCode = "AP2";
			Incoming2.EM_ApplicationCode = "AP2";
			outgoing4.EM_ApplicationCode = "AP2";
			incoming4.EM_ApplicationCode = "AP2";

			AssertEquals("Last outgoing message for application1", outgoing3, MessageCollection.GetLastMessage("AP1", ZString.Empty, EDIMessage.Direction.Transmit));
			AssertEquals("Last outgoing message for application2", outgoing4, MessageCollection.GetLastMessage("AP2", ZString.Empty, EDIMessage.Direction.Transmit));
			AssertEquals("Last incoming message for application1", incoming3, MessageCollection.GetLastMessage("AP1", ZString.Empty, EDIMessage.Direction.Receive));
			AssertEquals("Last incoming message for application2", incoming4, MessageCollection.GetLastMessage("AP2", ZString.Empty, EDIMessage.Direction.Receive));
		}

		public void TestLastMessage()
		{
			SetupCollectionWithOutgoingAndIncoming();
			AssertEquals("LastOutgoingMessage", Incoming2, MessageCollection.LastMessage);
		}

		public void TestGetLastMessageWithApplicationCode()
		{
			SetupCollectionWithOutgoingAndIncoming();
			Outgoing2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			Outgoing1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			Incoming1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			AssertEquals("LastOutgoingMessage", Outgoing2, MessageCollection.GetLastMessage(EDIMessage.ApplicationCodes.CMR));
		}

		public void TestGetLastMessageWithApplicationCodeAndMessageType()
		{
			ZString messageType = "TST";
			SetupCollectionWithOutgoingAndIncoming();
			Outgoing2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			Outgoing1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			Incoming1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			Outgoing1.EM_MessageType = messageType;
			AssertEquals("LastOutgoingMessage", Outgoing1, MessageCollection.GetLastMessage(EDIMessage.ApplicationCodes.CMR, messageType));
		}

		public void TestGetLastMessageWithMessageTypeAndMessageSubType()
		{
			ZString messageType = "TST";
			ZString messageSubType = "SUB";
			SetupCollectionWithOutgoingAndIncoming();
			Outgoing2.EM_MessageSubType = "ITF";
			Outgoing1.EM_MessageSubType = messageSubType;
			Outgoing1.EM_MessageType = messageType;
			AssertEquals("LastOutgoingMessage for MessageSubType", Outgoing1, MessageCollection.GetLastMessage("", messageType, "", "", messageSubType));
		}

		public void TestGetLastMessageWithMessageTypeAndMessageSubTypes()
		{
			SetupCollectionWithOutgoingAndIncoming();
			Outgoing2.EM_MessageSubType = "ITF";
			Outgoing2.EM_SystemCreateTimeUtc = new ZDateTime(2009, 2, 1);
			Outgoing1.EM_MessageSubType = "SUB";
			Outgoing1.EM_SystemCreateTimeUtc = new ZDateTime(2009, 1, 1);
			Factory.Save();
			AssertEquals("LastOutgoingMessage for MessageSubType", Outgoing2, MessageCollection.GetLastMessage("", "", EDIMessage.Direction.Transmit, "", new ZString[] { "SUB", "ITF" }));
		}

		public void TestGetLastMessageWithExcludedStatuses()
		{
			SetupCollectionWithOutgoingAndIncoming();
			Outgoing2.EM_SystemCreateTimeUtc = new ZDateTime(2009, 2, 1);
			Outgoing1.EM_SystemCreateTimeUtc = new ZDateTime(2009, 1, 1);
			Outgoing2.EM_Status = "ABC";
			Outgoing1.EM_Status = "DEF";
			Factory.Save();
			IList<ZString> excludeList = new List<ZString> { "ABC" };
			AssertEquals("LastOutgoingMessage for exclude list, pre-req", Outgoing2, MessageCollection.GetLastMessage("", "", "TRX", System.Array.Empty<ZString>(), System.Array.Empty<ZString>(), System.Array.Empty<ZString>(), System.Array.Empty<ZString>()));
			AssertEquals("LastOutgoingMessage for exclude list, Outgoing2 is excluded leaving Outgoing1", Outgoing1, MessageCollection.GetLastMessage("", "", "TRX", System.Array.Empty<ZString>(), System.Array.Empty<ZString>(), excludeList, System.Array.Empty<ZString>()));
		}

		public void TestGetLastMessageWithExcludedSubTypes()
		{
			SetupCollectionWithOutgoingAndIncoming();
			Outgoing2.EM_SystemCreateTimeUtc = new ZDateTime(2009, 2, 1);
			Outgoing1.EM_SystemCreateTimeUtc = new ZDateTime(2009, 1, 1);
			Outgoing2.EM_MessageSubType = "ABC";
			Outgoing1.EM_MessageSubType = "DEF";
			Factory.Save();
			IList<ZString> excludeList = new List<ZString> { "ABC" };
			AssertEquals("LastOutgoingMessage for exclude list, pre-req", Outgoing2, MessageCollection.GetLastMessage("", "", "TRX", System.Array.Empty<ZString>(), System.Array.Empty<ZString>(), System.Array.Empty<ZString>(), System.Array.Empty<ZString>()));
			AssertEquals("LastOutgoingMessage for exclude list, Outgoing2 is excluded leaving Outgoing1", Outgoing1, MessageCollection.GetLastMessage("", "", "TRX", System.Array.Empty<ZString>(), System.Array.Empty<ZString>(), System.Array.Empty<ZString>(), excludeList));
		}

		public void TestGetLastMessageWithExcludedSystemUsers()
		{
			SetupCollectionWithOutgoingAndIncomingWithLatestMessageQueuedBySystemAccount();
			IList<ZString> excludeList = new List<ZString> { "ABC" };
			AssertEquals("LastOutgoingNonSystemAndNonNullUserMessage for excluded systems user Outgoing3 is excluded leaving Outgoing2", Outgoing2, MessageCollection.LastOutgoingNonSystemAndNonNullUserMessage);
		}

		public void TestGetLastMessageWithExcludedBlankUsers()
		{
			SetupCollectionWithOutgoingAndIncomingWithLatestMessageQueuedByBlankUser();
			AssertEquals("LastOutgoingNonSystemAndNonNullUserMessage for excluded blank user Outgoing3 is excluded leaving Outgoing2", Outgoing2, MessageCollection.LastOutgoingNonSystemAndNonNullUserMessage);
		}

		public void TestRelationshipSetForNewChild()
		{
			var parent = Factory.New(typeof(TestHelperEDIMessage));

			MessageCollection = new EDIMessageCollection(parent, Factory);
			var newMessage = MessageCollection.AddNew();
			AssertEquals(parent.TableName, newMessage.EM_LinkTable);
			AssertEquals(parent.PK, newMessage.EM_LinkUniqueID);
		}

		public void TestRelationshipSetForOldChild()
		{
			var parent = (EDIMessage)Factory.New(typeof(TestHelperEDIMessage));

			MessageCollection = new EDIMessageCollection(parent, Factory);
			var newMessage = Factory.New<EDIMessage>();
			MessageCollection.Add(newMessage);
			AssertEquals(parent.TableName, newMessage.EM_LinkTable);
			AssertEquals(parent.PK, newMessage.EM_LinkUniqueID);
		}

		public void TestMessageFlaggedAsHavingMessageErrorsWhenAdded()
		{
			var parent = Factory.New<TestHelperEDIMessage>();
			MessageCollection = new EDIMessageCollection(parent, Factory);
			var firstMessage = MessageCollection.AddNew(typeof(EDIMessage));
			Assert("NotSentWithMessageErrors", !firstMessage.EM_SendWithMessageErrors);
			parent.FlagError = true;
			parent.Validation.ValidateEM_MessageText();
			var secondMessage = MessageCollection.AddNew(typeof(EDIMessage));
			Assert("SentWithMessageErrors", secondMessage.EM_SendWithMessageErrors);
		}

		public void TestMessagesIncludingInterchangeRejections()
		{
			var parent = Factory.New(typeof(TestHelperEDIMessage));
			MessageCollection = new EDIMessageCollection(parent, Factory);
			AssertNotNull(MessageCollection.MessagesIncludingInterchangeRejections(ZString.Empty));
		}

		public void TestIsWaitingForAResponse()
		{
			var parent = Factory.New(typeof(TestHelperEDIMessage));

			MessageCollection = new EDIMessageCollection(parent, Factory);
			var message = MessageCollection.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Assert(MessageCollection.IsWaitingForAResponse);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Assert(!MessageCollection.IsWaitingForAResponse);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Rejected;
			Assert(!MessageCollection.IsWaitingForAResponse);
			message.EM_Status = EDIMessage.Status.Cancelled;
			Assert(!MessageCollection.IsWaitingForAResponse);
		}

		public void TestWeAreNoLongerWaitingIfWeHaveReceivedAnAcknowledgementAndDontExpectToReceiveAResponse()
		{
			var parent = Factory.New(typeof(TestHelperEDIMessage));

			MessageCollection = new EDIMessageCollection(parent, Factory);
			var message = (TestHelperEDIMessage)MessageCollection.AddNew(typeof(TestHelperEDIMessage));
			message.fAssumeMessageClearIfAcknowledgedAndNoResponse = true;
			var interchange = Factory.New<EDIInterchange>();
			message.EM_EI = interchange.PK;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Assert(MessageCollection.IsWaitingForAResponse);
			SimulateAcknowledgement(interchange);
			Assert(!MessageCollection.IsWaitingForAResponse);
		}

		public void TestIsMatchingMessages()
		{
			var parent = Factory.New<TestHelperEDIMessage>();
			MessageCollection = new EDIMessageCollection(parent, Factory);
			var message = MessageCollection.AddNew();
			message.EM_ApplicationCode = "AAA";
			message.EM_MessageType = "CCC";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Discarded;
			Assert(MessageCollection.IsMatchingMessages("AAA", new ZString[] { "CCC" }, EDIMessage.Direction.Transmit, false));
			Assert(!MessageCollection.IsMatchingMessages("BBB", new ZString[] { "CCC" }, EDIMessage.Direction.Transmit, false));
			Assert(!MessageCollection.IsMatchingMessages("AAA", new ZString[] { "DDD" }, EDIMessage.Direction.Transmit, false));
			Assert(!MessageCollection.IsMatchingMessages("AAA", new ZString[] { "CCC" }, EDIMessage.Direction.Receive, false));
			Assert(!MessageCollection.IsMatchingMessages("AAA", new ZString[] { "CCC" }, EDIMessage.Direction.Transmit, true));
			Assert(!MessageCollection.IsMatchingMessages("", System.Array.Empty<ZString>(), "", true));
			Assert(MessageCollection.IsMatchingMessages("", System.Array.Empty<ZString>(), "", false));
			message.EM_Status = EDIMessage.Status.Queued;
			Assert(MessageCollection.IsMatchingMessages("", System.Array.Empty<ZString>(), "", false));
			Assert(MessageCollection.IsMatchingMessages("", System.Array.Empty<ZString>(), "", true));
			message.Delete();
			Assert(!MessageCollection.IsMatchingMessages("", System.Array.Empty<ZString>(), "", false));
			Assert(!MessageCollection.IsMatchingMessages("", System.Array.Empty<ZString>(), "", true));
		}

		public void TestHasNonDiscardedMessage()
		{
			var parent = Factory.New<TestHelperEDIMessage>();
			MessageCollection = new EDIMessageCollection(parent, Factory);
			Assert(!MessageCollection.HasNonDiscardedMessage());
			var message = MessageCollection.AddNew();
			Assert(MessageCollection.HasNonDiscardedMessage());
			message.EM_Status = EDIMessage.Status.Discarded;
			Assert(!MessageCollection.HasNonDiscardedMessage());
			message = MessageCollection.AddNew();
			Assert(MessageCollection.HasNonDiscardedMessage());
			message.EM_Status = EDIMessage.Status.Discarded;
			Assert(!MessageCollection.HasNonDiscardedMessage());
		}

		#region Implementation

		void SimulateAcknowledgement(EDIInterchange interchange)
		{
			var acknowledgement = interchange.InterchangeAcknowledgementMessages.AddNew();
			acknowledgement.EM_MessageSubType = nameof(Core.Constants.AUCTLMessageSubType.ACK);
		}

		void SetupCollectionWithOutgoingAndIncoming(bool createWithoutSystemsUser = false)
		{
			var userWhoCreatedRecord = GlbStaff.CurrentUser;

			if (createWithoutSystemsUser)
			{
				userWhoCreatedRecord = Factory.New<GlbStaff>();
				userWhoCreatedRecord.GS_Code = "AAA";
				userWhoCreatedRecord.GS_IsSystemAccount = false;
				Factory.Save();
			}

			var parent = Factory.New(typeof(TestHelperEDIMessage));

			MessageCollection = new EDIMessageCollection(parent, Factory);
			Outgoing1 = MessageCollection.AddNew(typeof(TestHelperEDIMessage));
			Outgoing1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Outgoing1.EM_SystemCreateUser = userWhoCreatedRecord.GS_Code;
			Outgoing1.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.InterchangeReady);
			Factory.Save();
			System.Threading.Thread.Sleep(1000);

			Incoming1 = MessageCollection.AddNew(typeof(TestHelperEDIMessage));
			Incoming1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();
			System.Threading.Thread.Sleep(1000);

			Outgoing2 = MessageCollection.AddNew(typeof(TestHelperEDIMessage));
			Outgoing2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Outgoing2.EM_SystemCreateUser = userWhoCreatedRecord.GS_Code;
			Outgoing2.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.InterchangeReady);
			Factory.Save();
			System.Threading.Thread.Sleep(1000);

			Incoming2 = MessageCollection.AddNew(typeof(TestHelperEDIMessage));
			Incoming2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();
		}

		void SetupCollectionWithOutgoingAndIncomingWithLatestMessageQueuedBySystemAccount()
		{
			SetupCollectionWithOutgoingAndIncoming(true);
			System.Threading.Thread.Sleep(1000);

			Outgoing3 = MessageCollection.AddNew(typeof(TestHelperEDIMessage));
			Outgoing3.EM_ApplicationCode = "USI";
			Outgoing3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Outgoing3.EM_MessageNum = "21533";
			Outgoing3.EM_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			Outgoing3.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.InterchangeReady);
			Factory.Save();
		}

		void SetupCollectionWithOutgoingAndIncomingWithLatestMessageQueuedByBlankUser()
		{
			SetupCollectionWithOutgoingAndIncoming(true);
			System.Threading.Thread.Sleep(1000);

			Outgoing3 = MessageCollection.AddNew(typeof(TestHelperEDIMessage));
			Outgoing3.EM_ApplicationCode = "USI";
			Outgoing3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Outgoing3.EM_MessageNum = "21533";
			Outgoing3.EM_SystemCreateUser = "";
			Outgoing3.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.InterchangeReady);
			// Do not save - keep EM_SystemCreateUser blank
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = (EDIMessage)Factory.New(typeof(TestHelperEDIMessage));
			return new EDIMessageCollection(parent, Factory);
		}

		EDIMessageCollection MessageCollection;
		EDIMessage Outgoing1;
		EDIMessage Outgoing2;
		EDIMessage Outgoing3;
		EDIMessage Incoming1;
		EDIMessage Incoming2;

		#region TestHelper EDIMessage
		public class TestHelperEDIMessage : EDIMessage, IObsoleteValidation
		{
			public TestHelperEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public bool fAssumeMessageClearIfAcknowledgedAndNoResponse;
			public override bool AssumeMessageClearIfAcknowledgedAndNoResponse
			{
				get
				{
					return fAssumeMessageClearIfAcknowledgedAndNoResponse;
				}
			}

			protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
			{
				//do nothing
			}

			protected override EDIMessageValidation GetNewValidation()
			{
				return new TestHelperEDIMessageValidation(this);
			}

			public bool FlagError;

			class TestHelperEDIMessageValidation : EDIMessageValidation
			{
				public TestHelperEDIMessageValidation(TestHelperEDIMessage message)
					: base(message)
				{
					this.message = message;
				}
				readonly TestHelperEDIMessage message;

				protected override void CheckEM_MessageText()
				{
					base.CheckEM_MessageText();
					if (message.FlagError)
					{
						Parent.EM_MessageTextInfo.AddMessageError("invalid message");
					}
				}
			}
		}
		#endregion
		#endregion

	}
}
