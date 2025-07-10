using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5BFSenderTest : TestCaseWithFactory
	{
		IEnumerable<CancellationMessageSendingObject> GetMessageParents() => MessageSendingObjects;

		MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR5BFSenderForTest(MessageSendingObjects, Factory) : new GOVCBR5BFSender(MessageSendingObjects, Factory);

		IEnumerable<CancellationMessageSendingObject> Parents => parents ?? (parents = GetMessageParents());
		IEnumerable<CancellationMessageSendingObject> parents;

		IEnumerable<CancellationMessageSendingObject> MessageSendingObjects
		{
			get
			{
				if (messageSendingObjects == null)
				{
					var entry = new TestDataSetupHelper(Factory).GetEntry929FullData(ZString.Empty, ZBool.True);
					entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._929;
					var sendingObjectParent = new CancellationMessageSendingObjectParent(entry.Declaration);
					messageSendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<CancellationMessageSendingObject>();
				}
				return messageSendingObjects;
			}
		}

		IEnumerable<CancellationMessageSendingObject> messageSendingObjects;

		public void TestStatusIsUpdated()
		{
			GetMessageSender().Send();
			foreach (CancellationMessageSendingObject parent in Parents)
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

		class GOVCBR5BFSenderForTest : GOVCBR5BFSender
		{
			public GOVCBR5BFSenderForTest(IEnumerable<CancellationMessageSendingObject> sendingObjects, BusinessObjectFactory factory) : base(sendingObjects, factory)
			{
			}

			protected override Import5BFCancel GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => throw new Exception();
		}
	}
}
