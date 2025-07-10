using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5DSCancellationSenderTest : TestCaseWithFactory
	{
		IEnumerable<JobDeclarationMiscMessageSendingObject> GetMessageParents() => MessageSendingObjects;

		MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR5DSCancellationSenderForTest(MessageSendingObjects, ElectronicDocumentTypeList.Codes._5DS, Factory) : new GOVCBR5DR5DSCancellationSender(MessageSendingObjects, ElectronicDocumentTypeList.Codes._5DS, Factory);

		IEnumerable<JobDeclarationMiscMessageSendingObject> Parents => parents ?? (parents = GetMessageParents());
		IEnumerable<JobDeclarationMiscMessageSendingObject> parents;

		IEnumerable<JobDeclarationMiscMessageSendingObject> MessageSendingObjects
		{
			get
			{
				if (messageSendingObjects == null)
				{
					var entry = new TestDataSetupHelper(Factory).GetEntryHas5DQSnapshot();
					entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._5DQ;
					var sendingObjectParent = new JobDeclarationMiscMessageSendingObjectParent(entry.Declaration, MessageType, MessageFunctions.MessageFunctionCode.Cancellation);
					messageSendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationMiscMessageSendingObject>();
				}
				return messageSendingObjects;
			}
		}

		ZString MessageType => ElectronicDocumentTypeList.Codes._5DS;

		IEnumerable<JobDeclarationMiscMessageSendingObject> messageSendingObjects;

		public void TestCancelSendMessage()
		{
			var sendingObject = Parents.Single();
			GetMessageSender().Send();

			var entry = sendingObject.Header;
			var messages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == MessageType);
			var message = messages.LastOrDefault();
			AssertEquals(LocalExportAmendmentTypeList.Codes.Cancellation, message.EM_MessageSubType);
			AssertEquals("1", message.EM_ApplicationReference);
		}

		public void TestStatusIsUpdated()
		{
			GetMessageSender().Send();
			foreach (JobDeclarationMiscMessageSendingObjectCore parent in Parents)
			{
				AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationSent, GetStatusField(parent.Header));
			}
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

		class GOVCBR5DSCancellationSenderForTest : GOVCBR5DR5DSCancellationSender
		{
			public GOVCBR5DSCancellationSenderForTest(IEnumerable<JobDeclarationMiscMessageSendingObject> sendingObjects, string messageType, BusinessObjectFactory factory) : base(sendingObjects, messageType, factory)
			{
			}
			protected override LocalExportAmendEntryHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => throw new Exception();
		}
	}
}
