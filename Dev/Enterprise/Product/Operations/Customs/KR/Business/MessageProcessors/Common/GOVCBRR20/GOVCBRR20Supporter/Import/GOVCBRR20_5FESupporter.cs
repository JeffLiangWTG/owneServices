using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR20SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._5FE)]
	class GOVCBRR20_5FESupporter : GOVCBRR20Supporter
	{
		public override ZString EntryType => SharedJobMessageTypeList.Codes.Import;

		public override void UpdateParent(BusinessObject header, ZString typeCode, ZString versionNumber)
		{
			CusEntryHeader entry = (CusEntryHeader)header;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentRejected;
			var message5FE = entry.Messages.GetLastMessageWithMatchingVersionNumber(ElectronicDocumentTypeList.Codes._5FE, versionNumber);
			var versionNumber5UA = message5FE?.EM_MessageOwner ?? ZString.Empty;

			if (!versionNumber5UA.IsEmpty)
			{
				var entryNumber5UA = entry.EntryNumbers.GetCusEntryNumWithMatchingVersionNumber(ElectronicDocumentTypeList.Codes._5UA, versionNumber5UA);
				if (entryNumber5UA != null)
				{
					entry.EntryNumbers.RemoveAndDelete(entryNumber5UA);
				}
			}

			var penaltyExemptionSessionalData = entry.LoadPenaltyExemptionSessionalDataRelated5FE(ZShort.Parse(versionNumber));
			if (penaltyExemptionSessionalData != null)
			{
				penaltyExemptionSessionalData.ResetVersionAndIndicator();
			}
		}

		public override EDIMessage GetOutgoingMessage(BusinessObject header, ZString typeCode, ZString versionNumber)
		{
			var outgoingMessage = this.GetLastMessageWithMatchingVersionNumber(header, typeCode, versionNumber);
			if (outgoingMessage != null)
			{
				((CusEntryHeader)header).ResetChargesAndLineFeesVersionAndAmount(outgoingMessage.EM_ApplicationReference);
			}
			return outgoingMessage;
		}
		public override ZString GetKeyToFindOutgoingMessageOrEntryNum(BusinessObject header, IGOVCBRR20MessageData messageData)
		{
			return ((CusEntryHeader)header).GetCW1VersionNumberFromCustoms5FEVersionNumber((ZShort)messageData.AmendSequence, (ZDate)messageData.AcceptDateTime).ToString();
		}
	}
}
