using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5FNSender : MessageSender<CusEntryHeader, Import5FNLine>
	{
		public override ZString MessageType => ElectronicDocumentTypeList.Codes._5FN;

		public GOVCBR5FNSender(IEnumerable<JobDeclarationMiscMessageSendingObject> messageSendingObjects, BusinessObjectFactory factory) : base(messageSendingObjects.Select(x => x.Header).Distinct(), factory)
		{
			this.messageSendingObjects = messageSendingObjects;
		}

		readonly IEnumerable<JobDeclarationMiscMessageSendingObject> messageSendingObjects;

		protected override void UpdateMessageStatus(CusEntryHeader parent)
		{
			var messageSendingObject = GetMessageSendingObject(parent);

			foreach (MessageSendingEntryLineObject entryLineObject in messageSendingObject.MessageSendingEntryLines)
			{
				if (entryLineObject.ShouldSend)
				{
					var entryNum = parent.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5FN && ZInt.ParseEmptyAsZero(x.CE_EntryLineReference) == entryLineObject.EntryLineNo);
					if (entryNum == null)
					{
						entryNum = parent.EntryNumbers.AddNew();
						entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
						entryNum.CE_EntryNum = parent.EntryNumber;
						entryNum.CE_EntryLineReference = entryLineObject.EntryLineNo.ToString();
					}
					entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
				}
			}
		}

		JobDeclarationMiscMessageSendingObject GetMessageSendingObject(CusEntryHeader parent) => messageSendingObjects.First(x => x.Header == parent);

		protected override Import5FNLine GetMessageDataProvider(CusEntryHeader parent, ZString messageID)
		{
			var messageIDInt = ZInt.ParseEmptyAsZero(messageID);
			CusEntryLine entryLine = parent.MergedLines.First(x => x.CL_LineNumber == messageIDInt);
			return new Import5FNLineCreator().Create(entryLine);
		}

		protected override IMessageBuilder GetMessageBuilder(Import5FNLine messageDataProvider)
		{
			return new GOVCBR5FNMessageBuilder(messageDataProvider);
		}

		protected override ZString GetMessageID(EDIMessage message) => message.EM_ApplicationReference;

		protected override IEnumerable<EDIMessage> CreateMessages(CusEntryHeader parent)
		{
			var messages = new List<EDIMessage>();
			var messageSendingObject = GetMessageSendingObject(parent);

			foreach (MessageSendingEntryLineObject entryLineObject in messageSendingObject.MessageSendingEntryLines)
			{
				if (entryLineObject.ShouldSend)
				{
					var message = Factory.New<EDIMessage>();
					message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
					message.EM_MessageType = MessageType;
					message.EM_ApplicationReference = entryLineObject.EntryLineNo.ToString();
					messages.Add(message);
				}
			}
			return messages;
		}
	}
}
