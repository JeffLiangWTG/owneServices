using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR99SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._5SC, ElectronicDocumentTypeList.Codes._DHR)]
	class GOVCBRR99_Import5SCDHRSupporter : IGOVCBRR99Supporter
	{
		ZString IGOVCBRR99Supporter.EntryType => SharedJobMessageTypeList.Codes.Import;

		void IGOVCBRR99Supporter.UpdateParent(EDIMessage message, CusEntryHeader entry, ZString typeCode, IGOVCBRR99MessageData messageData, ZStringBuilder emailBuilder)
		{
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(typeCode);
			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			entryNum.CE_IssueDate = messageData.NoticeDateTime;
			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, typeCode);
			message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;
			entryNum.CE_EntryLineReference = outgoingMessage?.EM_ApplicationReference ?? "1";
		}
	}
}
