using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR20SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._5UA)]
	class GOVCBRR20_5UASupporter : GOVCBRR20Supporter
	{
		public override ZString EntryType => SharedJobMessageTypeList.Codes.Import;

		public override void UpdateParent(BusinessObject header, ZString typeCode, ZString entryLineReference)
		{
			CusEntryHeader entry = (CusEntryHeader)header;

			var entryNum = entry.EntryNumbers.GetCusEntryNumWithMatchingVersionNumber(typeCode, entryLineReference);
			if (entryNum != null)
			{
				entry.EntryNumbers.RemoveAndDelete(entryNum);
			}

			var penaltyExemptionSessionalData = entry.LoadPenaltyExemptionSessionalDataRelated5UA(ZShort.Parse(entryLineReference));
			if (penaltyExemptionSessionalData != null)
			{
				penaltyExemptionSessionalData.ResetVersionAndIndicator();
			}
		}

		public override EDIMessage GetOutgoingMessage(BusinessObject header, ZString typeCode, ZString versionNumber)
		{
			return this.GetLastMessageWithMatchingVersionNumber(header, typeCode, versionNumber);
		}
	}
}
