using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR99SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._5SM)]
	class GOVCBRR99_5SMSupporter : IGOVCBRR99Supporter
	{
		ZString IGOVCBRR99Supporter.EntryType => ElectronicDocumentTypeList.Codes._5SM;

		void IGOVCBRR99Supporter.UpdateParent(EDIMessage message, CusEntryHeader entry, ZString typeCode, IGOVCBRR99MessageData messageData, ZStringBuilder emailBuilder)
		{
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var entryNumber = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5SM);
			entryNumber.CE_IssueDate = messageData.AcceptDateTime;
			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5SM);
			message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;
		}
	}
}
