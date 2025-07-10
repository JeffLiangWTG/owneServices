using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR20SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._105, ElectronicDocumentTypeList.Codes._DHS, ElectronicDocumentTypeList.Codes._5BB)]
	class GOVCBRR20_ImportOtherAmendmentSupporter : GOVCBRR20Supporter
	{
		public override ZString EntryType => SharedJobMessageTypeList.Codes.Import;

		public override void UpdateParent(BusinessObject header, ZString typeCode, ZString keyToFindOutgoingMessageOrEntryNum)
		{
			CusEntryHeader entry = (CusEntryHeader)header;
			var originalTypeCode = ElectronicDocumentTypeList.GetOriginalType(typeCode);
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(originalTypeCode);
			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentRejected;
		}
	}
}
