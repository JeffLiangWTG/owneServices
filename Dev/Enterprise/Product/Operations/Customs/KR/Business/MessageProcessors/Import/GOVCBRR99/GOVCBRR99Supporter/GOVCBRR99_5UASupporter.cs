using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR99SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._5UA)]
	class GOVCBRR99_5UASupporter : IGOVCBRR99Supporter
	{
		ZString IGOVCBRR99Supporter.EntryType => SharedJobMessageTypeList.Codes.Import;

		void IGOVCBRR99Supporter.UpdateParent(EDIMessage message, CusEntryHeader entry, ZString typeCode, IGOVCBRR99MessageData messageData, ZStringBuilder emailBuilder)
		{
			var outgoingMessage = entry.Messages.GetLastMessageWithMatchingVersionNumber(ElectronicDocumentTypeList.Codes._5UA, messageData.AmendSequence.ToString());
			message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;

			var entryNum = entry.EntryNumbers.GetCusEntryNumWithMatchingVersionNumber(typeCode, messageData.AmendSequence.ToString());
			if (entryNum != null)
			{
				entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
				entryNum.CE_IssueDate = messageData.AcceptDateTime;
			}

			var penaltyExemptionSessionalData = entry.LoadPenaltyExemptionSessionalDataRelated5UA((ZShort)messageData.AmendSequence);
			if (penaltyExemptionSessionalData != null)
			{
				penaltyExemptionSessionalData.CSI_DateOfIssue = messageData.AcceptDateTime;
			}
		}
	}
}
