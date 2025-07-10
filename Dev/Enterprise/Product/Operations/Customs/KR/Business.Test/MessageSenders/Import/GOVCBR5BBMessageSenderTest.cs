using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5BBMessageSenderTest : CusEntryHeaderAmendmentMessageSenderTest<GOVCBR5BBSender>
	{
		public void TestValidationMode()
		{
			var declaration = Entry.Declaration;
			AssertEquals("Pre-condition", "Import", declaration.ValidationMode.ToString());
			var sender = GetMessageSender();
			AssertEquals("Import, AgreedRateForAllLines", declaration.ValidationMode.ToString());
			sender.Send();
			AssertEquals("Import", declaration.ValidationMode.ToString());
		}

		protected override IEnumerable<JobDeclarationAmendmentMessageSendingObject> GetMessageParents() => MessageSendingObjects;

		protected override MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR5BBSenderForTest(MessageSendingObjects, Factory) : new GOVCBR5BBSender(MessageSendingObjects, Factory);

		IEnumerable<JobDeclarationAmendmentMessageSendingObject> MessageSendingObjects
		{
			get
			{
				if (messageSendingObjects == null)
				{
					Entry.MergedLines[0].Delete();
					var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5BA);
					entryNum.CE_EntryLineReference = "2";
					var sendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(Entry.Declaration, MessageType);
					messageSendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationAmendmentMessageSendingObject>();
				}
				return messageSendingObjects;
			}
		}

		protected override ZString MessageType => ElectronicDocumentTypeList.Codes._5BB;

		IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects;
		IEnumerable<JobDeclarationAmendmentMessageSendingObject> Parents => parents ?? (parents = GetMessageParents());
		IEnumerable<JobDeclarationAmendmentMessageSendingObject> parents;

		protected override ZString GetStatusField(CusEntryHeader entry) => entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5BA).CE_EntryStatus;

		public override void TestSendMessage()
		{
			MessageSender.Send();

			foreach (JobDeclarationAmendmentMessageSendingObject parent in Parents)
			{
				var entry = parent.Header;
				var messages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == MessageType);
				AssertEquals(1, messages.Count());
				var message = messages.FirstOrDefault();
				var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5BA);
				AssertEquals("2", entryNum.CE_EntryLineReference);
				AssertEquals("3", message.EM_ApplicationReference);
			}
		}

		public override void TestStatusIsUpdated()
		{
			MessageSender.Send();
			foreach (JobDeclarationAmendmentMessageSendingObject parent in Parents)
			{
				var entry = parent.Header;
				var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5BA);
				AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentSent, entryNum.CE_EntryStatus);
			}
		}

		class GOVCBR5BBSenderForTest : GOVCBR5BBSender
		{
			public GOVCBR5BBSenderForTest(IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects, BusinessObjectFactory factory)
			: base(messageSendingObjects, factory)
			{
			}

			protected override Import5BBHeader GetMessageDataProvider(CusEntryHeader parent, Import5BAHeader currentSnapshot, AmendedItemCollection amendedItems) => throw new System.Exception();
		}

		CusEntryHeader Entry => entry ?? (entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot());
		CusEntryHeader entry;
	}
}
