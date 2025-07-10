using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR99SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._5BB)]
	class GOVCBRR99_5BBSupporter : IGOVCBRR99Supporter
	{
		ZString IGOVCBRR99Supporter.EntryType => SharedJobMessageTypeList.Codes.Import;

		void IGOVCBRR99Supporter.UpdateParent(EDIMessage message, CusEntryHeader entry, ZString typeCode, IGOVCBRR99MessageData messageData, ZStringBuilder emailBuilder)
		{
			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, typeCode);

			var entryNum_5BA = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5BA);
			entryNum_5BA.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			entryNum_5BA.CE_EntryLineReference = outgoingMessage?.EM_ApplicationReference ?? ZString.Empty;
			message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;
		}
	}
}
