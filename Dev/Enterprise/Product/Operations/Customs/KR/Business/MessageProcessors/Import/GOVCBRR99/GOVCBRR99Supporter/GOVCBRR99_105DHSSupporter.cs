using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR99SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._105, ElectronicDocumentTypeList.Codes._DHS)]
	class GOVCBRR99_105DHSSupporter : IGOVCBRR99Supporter
	{
		ZString IGOVCBRR99Supporter.EntryType => SharedJobMessageTypeList.Codes.Import;

		void IGOVCBRR99Supporter.UpdateParent(EDIMessage message, CusEntryHeader entry, ZString typeCode, IGOVCBRR99MessageData messageData, ZStringBuilder emailBuilder)
		{
			var originalMessageType = typeCode == ElectronicDocumentTypeList.Codes._105
							? ElectronicDocumentTypeList.Codes._5SC
							: ElectronicDocumentTypeList.Codes._DHR;
			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, typeCode);
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(originalMessageType);
			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			entryNum.CE_EntryLineReference = outgoingMessage?.EM_ApplicationReference ?? ZString.Empty;
			message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;
		}
	}
}
