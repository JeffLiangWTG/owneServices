using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBRDF3MessageSender : CusEntryHeaderOriginalMessageSender<LocalExportAmendEntryHeader>
	{
		public GOVCBRDF3MessageSender(IEnumerable<CusEntryHeader> entries, BusinessObjectFactory factory)
			: base(entries, factory)
		{
		}

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._DF3;

		protected override IMessageBuilder GetMessageBuilder(LocalExportAmendEntryHeader messageDataProvider) => new GOVCBRDF3MessageBuilder(messageDataProvider);

		protected override LocalExportAmendEntryHeader GetMessageDataProvider(CusEntryHeader entry, ZString messageID) => new LocalExportAmendDF3Creator().Create(entry);

		protected override void UpdateMessageStatus(CusEntryHeader entry)
		{
			var cusEntryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(MessageType);
			cusEntryNum.CE_EntryNum = entry.CH_BGMReference;
			cusEntryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
		}
	}
}
