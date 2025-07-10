using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR20SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._D72)]
	class GOVCBRR20_D72Supporter : GOVCBRR20Supporter
	{
		public override ZString EntryType => SharedJobMessageTypeList.Codes.Import;

		public override void UpdateParent(BusinessObject header, ZString typeCode, ZString keyToFindOutgoingMessageOrEntryNum)
		{
		}

		public override EDIMessage GetOutgoingMessage(BusinessObject header, ZString typeCode, ZString versionNumber)
		{
			return this.GetLastMessageWithMatchingVersionNumber(header, typeCode, versionNumber);
		}
	}
}
