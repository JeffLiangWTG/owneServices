using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRDHSAmendmentSenderTest : CusEntryHeaderAmendmentMessageSenderTest<GOVCBRDHSAmendmentSender>
	{
		protected override IEnumerable<JobDeclarationAmendmentMessageSendingObject> GetMessageParents() => MessageSendingObjects;

		protected override MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBRDHSAmendmentSenderForTest(MessageSendingObjects, Factory) : new GOVCBRDHSAmendmentSender(MessageSendingObjects, Factory, MessageType);

		IEnumerable<JobDeclarationAmendmentMessageSendingObject> Parents => parents ?? (parents = GetMessageParents());
		IEnumerable<JobDeclarationAmendmentMessageSendingObject> parents;

		IEnumerable<JobDeclarationAmendmentMessageSendingObject> MessageSendingObjects
		{
			get
			{
				if (messageSendingObjects == null)
				{
					var entry = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot();
					entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._DHR;
					var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._DHR);
					entryNum.CE_EntryLineReference = "2";
					var sendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, MessageType);
					messageSendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationAmendmentMessageSendingObject>();
				}
				return messageSendingObjects;
			}
		}

		protected override ZString MessageType => ElectronicDocumentTypeList.Codes._DHS;

		IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects;

		public override void TestSendMessage()
		{
			MessageSender.Send();

			var sendingObject = Parents.Single();
			var entry = sendingObject.Header;
			var messages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == MessageType);
			var message = messages.LastOrDefault();
			AssertEquals(FTAAmendmentType.Codes.UXX, message.EM_MessageSubType);
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._DHR);
			AssertEquals("2", entryNum.CE_EntryLineReference);
			AssertEquals("3", message.EM_ApplicationReference);
		}

		protected override ZString GetStatusField(CusEntryHeader entry)
		{
			return entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.GetOriginalType(MessageSender.MessageType)).CE_EntryStatus;
		}

		class GOVCBRDHSAmendmentSenderForTest : GOVCBRDHSAmendmentSender
		{
			public GOVCBRDHSAmendmentSenderForTest(IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects, BusinessObjectFactory factory)
			: base(messageSendingObjects, factory, ElectronicDocumentTypeList.Codes._DHS)
			{
			}

			protected override ImportFTAAmendmentHeader GetMessageDataProvider(CusEntryHeader parent, ImportDHRHeader currentSnapshot, AmendedItemCollection amendedItems) => throw new System.Exception();
		}
	}
}
