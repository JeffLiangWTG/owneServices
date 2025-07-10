using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR99SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._5FE)]
	class GOVCBRR99_5FESupporter : IGOVCBRR99Supporter
	{
		ZString IGOVCBRR99Supporter.EntryType => SharedJobMessageTypeList.Codes.Import;

		void IGOVCBRR99Supporter.UpdateParent(EDIMessage message, CusEntryHeader entry, ZString typeCode, IGOVCBRR99MessageData messageData, ZStringBuilder emailBuilder)
		{
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			var versionNumber = entry.GetCW1VersionNumberFromCustoms5FEVersionNumber((ZShort)messageData.AmendSequence, messageData.DeclarationDate);
			var originalMessage = entry.Messages.GetLastMessageWithMatchingVersionNumber(ElectronicDocumentTypeList.Codes._5FE, versionNumber.ToString());
			message.EM_ApplicationReference = originalMessage?.EM_MessageNum ?? ZString.Empty;
			var versionNumber5UA = originalMessage?.EM_MessageOwner ?? ZString.Empty;

			if (!versionNumber5UA.IsEmpty)
			{
				var entryNumber5UA = entry.EntryNumbers.GetCusEntryNumWithMatchingVersionNumber(ElectronicDocumentTypeList.Codes._5UA, versionNumber5UA);
				if (entryNumber5UA != null)
				{
					entryNumber5UA.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
				}
			}

			var amendmentSessionalData = entry.GetAmendmentSessionalDataWithMatchingVersionNumber(versionNumber);
			if (amendmentSessionalData != null)
			{
				amendmentSessionalData.CSI_DateOfIssue = messageData.AcceptDateTime;
				if (amendmentSessionalData.ValidPenaltyExemptionSessionalData != null)
				{
					amendmentSessionalData.ValidPenaltyExemptionSessionalData.CSI_DateOfIssue = messageData.AcceptDateTime;
				}
			}
			entry.UpdateEntryVersionID(originalMessage);
		}
	}
}
