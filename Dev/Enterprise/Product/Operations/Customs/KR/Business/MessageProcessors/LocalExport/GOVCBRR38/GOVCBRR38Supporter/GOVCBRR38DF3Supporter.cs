using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR38SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._DF3)]
	class GOVCBRR38DF3Supporter : IGOVCBRR38Supporter
	{
		ZString IGOVCBRR38Supporter.EntryType => ElectronicDocumentTypeList.Codes._DF3;

		void IGOVCBRR38Supporter.UpdateParent(CusEntryHeader entry, ZString typeCode, IGOVCBRR38MessageData messageData)
		{
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(typeCode);
			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
		}
	}
}
