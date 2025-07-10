using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5DP5DQSender : CusEntryHeaderOriginalMessageSender<LocalExportEntryHeader>
	{
		public GOVCBR5DP5DQSender(IEnumerable<CusEntryHeader> entries, string messageType, BusinessObjectFactory factory)
			: base(entries, factory)
		{
			localExportMessageType = messageType;
		}

		readonly ZString localExportMessageType;

		public override ZString MessageType => localExportMessageType;

		protected override IMessageBuilder GetMessageBuilder(LocalExportEntryHeader messageDataProvider)
		{
			IMessageBuilder result = null;
			if (localExportMessageType == ElectronicDocumentTypeList.Codes._5DP)
			{
				result = new GOVCBR5DPMessageBuilder(messageDataProvider);
			}
			else if (localExportMessageType == ElectronicDocumentTypeList.Codes._5DQ)
			{
				result = new GOVCBR5DQMessageBuilder(messageDataProvider);
			}
			return result;
		}

		protected override LocalExportEntryHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => localExportMessageType == ElectronicDocumentTypeList.Codes._5DP
			? new LocalExport5DPEntryHeaderCreator().Create(parent) : new LocalExport5DQEntryHeaderCreator().Create(parent);

		protected override void UpdateMessageStatus(CusEntryHeader parent)
		{
			parent.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
		}

		protected override IEnumerable<EDIMessage> CreateMessages(CusEntryHeader parent)
		{
			var messages = base.CreateMessages(parent);
			var message = messages.First();
			message.RecordVersionNumberFromCH_VersionID(parent);
			return messages;
		}

		protected override void OnSentCore(CusEntryHeader parent)
		{
			base.OnSentCore(parent);
			parent.PopulateEntrySubmittedDateIfRequired();
		}
	}
}
