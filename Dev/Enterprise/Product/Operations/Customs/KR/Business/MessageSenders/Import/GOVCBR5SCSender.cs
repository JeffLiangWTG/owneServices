using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5SCSender : CusEntryHeaderOriginalMessageSender<ImportFTAHeader>
	{
		public GOVCBR5SCSender(IEnumerable<CusEntryHeader> entries, BusinessObjectFactory factory, ZString messageType)
			: base(entries, factory)
		{
			if (messageType != ElectronicDocumentTypeList.Codes._5SC)
			{
				throw new ArgumentException($"Param messageType should be '5SC'. But the current value is {MessageType}");
			}
			this.messageType = messageType;
		}

		public override ZString MessageType => messageType;
		readonly ZString messageType;
		protected override IMessageBuilder GetMessageBuilder(ImportFTAHeader dataProvider) => new GOVCBR5SCMessageBuilder(dataProvider);
		protected override ImportFTAHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => new ImportFTACreator().Create(parent);

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
