using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5ASExtendOfPeriodSenderTest : TestCaseWithFactory
	{
		IEnumerable<JobDeclarationMiscMessageSendingObject> GetMessageParents() => MessageSendingObjects;

		MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR5ASExtendOfPeriodSenderForTest(MessageSendingObjects, Factory) : new GOVCBR5ASExtendOfPeriodSender(MessageSendingObjects, Factory);

		IEnumerable<JobDeclarationMiscMessageSendingObject> Parents => parents ?? (parents = GetMessageParents());
		IEnumerable<JobDeclarationMiscMessageSendingObject> parents;

		IEnumerable<JobDeclarationMiscMessageSendingObject> MessageSendingObjects
		{
			get
			{
				if (messageSendingObjects == null)
				{
					var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
					entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._830;
					var sendingObjectParent = new JobDeclarationMiscMessageSendingObjectParent(entry.Declaration, MessageType, MessageFunctions.MessageFunctionCode.Extend);
					messageSendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationMiscMessageSendingObject>();
				}
				return messageSendingObjects;
			}
		}

		ZString MessageType => ElectronicDocumentTypeList.Codes._5AS;

		IEnumerable<JobDeclarationMiscMessageSendingObject> messageSendingObjects;

		public void TestExtendSendMessage()
		{
			var sendingObject = Parents.Single();
			sendingObject.NewDate = ZDate.Today;
			GetMessageSender().Send();

			var entry = sendingObject.Header;
			var messages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == MessageType);
			var message = messages.LastOrDefault();
			AssertEquals(_5ASAmendmentType.Codes.Extension, message.EM_MessageSubType);
			AssertEquals("1", message.EM_ApplicationReference);
		}

		public void TestStatusIsUpdated()
		{
			GetMessageSender().Send();
			foreach (JobDeclarationMiscMessageSendingObjectCore parent in Parents)
			{
				AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentSent, GetStatusField(parent.Header));
			}
		}

		public void TestAmendmentTypeIsNull()
		{
			GetMessageSender().Send();
			var sendingObject = Parents.Single();
			var entry = sendingObject.Header;

			var messages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == MessageType);
			var message = messages.LastOrDefault();
			var messageData = MessageEncoding.UTF8WithoutBOM.GetString(message.EM_MessageData);
			AssertContains("<TagID>A608</TagID>", messageData);
			AssertNotContains("<ChangeReasonCode>", messageData);
		}

		public void TestSendException()
		{
			IsExceptionTest = ZBool.True;
			var messages = Parents.Single().Header.Messages;
			AssertEquals("One message exits before sending a message.", 1, messages.Count);
			AssertExceptionThrown<Exception>(() => GetMessageSender().Send());
			AssertEquals("Exception occurred when sending a message, and no new message has been created.", 1, messages.Count);
		}
		public ZBool IsExceptionTest;

		ZString GetStatusField(CusEntryHeader entry) => entry.CH_Status;

		class GOVCBR5ASExtendOfPeriodSenderForTest : GOVCBR5ASExtendOfPeriodSender
		{
			public GOVCBR5ASExtendOfPeriodSenderForTest(IEnumerable<JobDeclarationMiscMessageSendingObject> sendingObjects, BusinessObjectFactory factory) : base(sendingObjects, factory)
			{
			}

			protected override ExportAmendmentHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => throw new Exception();
		}
	}
}
