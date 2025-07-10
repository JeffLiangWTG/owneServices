using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5DRAmendmentSenderTest : CusEntryHeaderAmendmentMessageSenderTest<GOVCBR5DS5DRAmendmentSender>
	{
		protected override IEnumerable<JobDeclarationAmendmentMessageSendingObject> GetMessageParents() => MessageSendingObjects;

		protected override MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR5DS5DRAmendmentSenderForTest(MessageSendingObjects, ElectronicDocumentTypeList.Codes._5DR, Factory) : new GOVCBR5DS5DRAmendmentSender(MessageSendingObjects, ElectronicDocumentTypeList.Codes._5DR, Factory);

		IEnumerable<JobDeclarationAmendmentMessageSendingObject> Parents => parents ?? (parents = GetMessageParents());
		IEnumerable<JobDeclarationAmendmentMessageSendingObject> parents;

		IEnumerable<JobDeclarationAmendmentMessageSendingObject> MessageSendingObjects
		{
			get
			{
				if (messageSendingObjects == null)
				{
					var entry = new TestDataSetupHelper(Factory).GetEntryHas5DPSnapshot();
					entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._5DP;
					var sendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, MessageType);
					messageSendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationAmendmentMessageSendingObject>();
				}
				return messageSendingObjects;
			}
		}

		protected override ZString MessageType => ElectronicDocumentTypeList.Codes._5DR;

		IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects;

		public void TestAmendSendMessage()
		{
			MessageSender.Send();

			var sendingObject = Parents.Single();
			var entry = sendingObject.Header;
			var messages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == MessageType);
			var message = messages.LastOrDefault();
			AssertEquals(LocalExportAmendmentTypeList.Codes.Amendment, message.EM_MessageSubType);
			AssertEquals("1", message.EM_ApplicationReference);
		}

		protected override ZString GetStatusField(CusEntryHeader entry) => entry.CH_Status;

		class GOVCBR5DS5DRAmendmentSenderForTest : GOVCBR5DS5DRAmendmentSender
		{
			public GOVCBR5DS5DRAmendmentSenderForTest(IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects, ZString messageType, BusinessObjectFactory factory)
			: base(messageSendingObjects, messageType, factory)
			{
			}

			protected override LocalExportAmendEntryHeader GetMessageDataProvider(CusEntryHeader parent, LocalExportEntryHeader currentSnapshot, AmendedItemCollection amendedItems) => throw new System.Exception();
		}
	}
}
