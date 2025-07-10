using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBRD72Sender : MessageSender<CusEntryHeader, ImportD72Header>
	{
		public GOVCBRD72Sender(IEnumerable<ExtendReExportDateMessageSendingObject> sendingObjects, BusinessObjectFactory factory)
			: base(sendingObjects.Select(x => x.Header).Distinct(), factory)
		{
			this.sendingObjects = sendingObjects;
		}
		readonly IEnumerable<ExtendReExportDateMessageSendingObject> sendingObjects;

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._D72;

		protected override IMessageBuilder GetMessageBuilder(ImportD72Header messageDataProvider) => new GOVCBRD72MessageBuilder(messageDataProvider);

		protected override ZString GetMessageID(EDIMessage message) => message.EM_ApplicationReference;

		protected override ImportD72Header GetMessageDataProvider(CusEntryHeader parent, ZString messageID)
		{
			var versionNumber = ZShort.ParseSafe(messageID, 0);
			return new ImportD72Creator().Create(parent, GetSendingObject(versionNumber), versionNumber);
		}

		ExtendReExportDateMessageSendingObject GetSendingObject(short versionNumber)
		{
			SendingObjectsByVersionNumber.TryGetValue(versionNumber, out var sendingObject);
			return sendingObject;
		}

		protected override IEnumerable<EDIMessage> CreateMessages(CusEntryHeader parent)
		{
			SendingObjectsByVersionNumber.Clear();

			var messages = new List<EDIMessage>();
			parent.EntryNumbers.GetOrCreateCusEntryNum(MessageType);
			var maxVersionNumber = parent.EntryNumbers.Cast<CusEntryNumber>().Where(x => x.CE_EntryType == MessageType).Max(x => ZShort.ParseSafe(x.CE_EntryLineReference, 0));

			foreach (var sendingObject in sendingObjects.Where(x => x.Header.PK == parent.PK))
			{
				var message = Factory.New<EDIMessage>();
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_MessageType = MessageType;
				message.EM_ApplicationReference = (++maxVersionNumber).ToString();
				SendingObjectsByVersionNumber.Add(maxVersionNumber, sendingObject);
				messages.Add(message);
			}
			return messages;
		}

		Dictionary<short, ExtendReExportDateMessageSendingObject> SendingObjectsByVersionNumber => sendingObjectsByVersionNumber ?? (sendingObjectsByVersionNumber = new Dictionary<short, ExtendReExportDateMessageSendingObject>());
		Dictionary<short, ExtendReExportDateMessageSendingObject> sendingObjectsByVersionNumber;

		protected override void UpdateMessageStatus(CusEntryHeader parent) { }
	}
}
