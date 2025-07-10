using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR20SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._5SM)]
	class GOVCBRR20_5SMSupporter : GOVCBRR20Supporter
	{
		public override ZString EntryType => ElectronicDocumentTypeList.Codes._5SM;

		public override void UpdateParent(BusinessObject header, ZString typeCode, ZString keyToFindOutgoingMessageOrEntryNum)
		{
			CusEntryHeader entry = (CusEntryHeader)header;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
		}
	}
}
