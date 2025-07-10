using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5ASAmendmentSenderTest : CusEntryHeaderAmendmentMessageSenderTest<GOVCBR5ASAmendmentSender>
	{
		protected override IEnumerable<JobDeclarationAmendmentMessageSendingObject> GetMessageParents() => MessageSendingObjects;

		protected override MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR5ASAmendmentSenderForTest(MessageSendingObjects, Factory) : new GOVCBR5ASAmendmentSender(MessageSendingObjects, Factory);

		IEnumerable<JobDeclarationAmendmentMessageSendingObject> Parents => parents ?? (parents = GetMessageParents());
		IEnumerable<JobDeclarationAmendmentMessageSendingObject> parents;

		IEnumerable<JobDeclarationAmendmentMessageSendingObject> MessageSendingObjects
		{
			get
			{
				if (messageSendingObjects == null)
				{
					var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
					entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._830;
					var sendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, MessageType);
					messageSendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationAmendmentMessageSendingObject>();
				}
				return messageSendingObjects;
			}
		}

		protected override ZString MessageType => ElectronicDocumentTypeList.Codes._5AS;

		IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects;

		public void TestAmendSendMessage()
		{
			MessageSender.Send();

			var sendingObject = Parents.Single();
			var entry = sendingObject.Header;
			var messages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == MessageType);
			var message = messages.LastOrDefault();
			AssertEquals(_5ASAmendmentType.Codes.Amendment, message.EM_MessageSubType);
			AssertEquals("1", message.EM_ApplicationReference);
		}

		protected override ZString GetStatusField(CusEntryHeader entry) => entry.CH_Status;

		class GOVCBR5ASAmendmentSenderForTest : GOVCBR5ASAmendmentSender
		{
			public GOVCBR5ASAmendmentSenderForTest(IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects, BusinessObjectFactory factory)
			: base(messageSendingObjects, factory)
			{
			}

			protected override ExportAmendmentHeader GetMessageDataProvider(CusEntryHeader parent, ExportEntryHeader currentSnapshot, AmendedItemCollection amendedItems) => throw new System.Exception();
		}
	}
}
