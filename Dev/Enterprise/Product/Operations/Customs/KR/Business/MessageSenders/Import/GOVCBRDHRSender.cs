using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBRDHRSender : CusEntryHeaderOriginalMessageSender<ImportDHRHeader>
	{
		public GOVCBRDHRSender(IEnumerable<CusEntryHeader> entries, BusinessObjectFactory factory, ZString messageType)
			: base(entries, factory)
		{
			if (messageType != ElectronicDocumentTypeList.Codes._DHR)
			{
				throw new ArgumentException($"Param messageType should be 'DHR'. But the current value is {MessageType}");
			}
			this.messageType = messageType;
		}

		public override ZString MessageType => messageType;
		readonly ZString messageType;
		protected override IMessageBuilder GetMessageBuilder(ImportDHRHeader dataProvider) => new GOVCBRDHRMessageBuilder(dataProvider);
		protected override ImportDHRHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) =>  new ImportDHRCreator().Create(parent);

		protected override IEnumerable<EDIMessage> CreateMessages(CusEntryHeader parent)
		{
			var messages = base.CreateMessages(parent);
			var message = messages.First();
			message.RecordVersionNumberFromCusEntryNum(parent, messageType);
			return messages;
		}

		protected override void UpdateMessageStatus(CusEntryHeader entry)
		{
			var cusEntryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(MessageType);
			cusEntryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
		}
	}
}
