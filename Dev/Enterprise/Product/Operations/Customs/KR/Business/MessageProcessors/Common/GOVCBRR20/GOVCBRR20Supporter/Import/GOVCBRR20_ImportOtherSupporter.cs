using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR20SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._934, ElectronicDocumentTypeList.Codes._5SC, ElectronicDocumentTypeList.Codes._DHR,
											ElectronicDocumentTypeList.Codes._5SI, ElectronicDocumentTypeList.Codes._5BA, ElectronicDocumentTypeList.Codes._5TE)]
	class GOVCBRR20_ImportOtherSupporter : GOVCBRR20Supporter
	{
		public override ZString EntryType => SharedJobMessageTypeList.Codes.Import;

		public override void UpdateParent(BusinessObject header, ZString typeCode, ZString keyToFindOutgoingMessageOrEntryNum)
		{
			CusEntryHeader entry = (CusEntryHeader)header;
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(typeCode);
			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalRejected;
		}
	}
}
