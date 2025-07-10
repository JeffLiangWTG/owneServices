using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR20SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._5GW)]
	class GOVCBRR20_5GWSupporter : GOVCBRR20Supporter
	{
		public override ZString EntryType => ElectronicDocumentTypeList.Codes._5GW;

		public override void UpdateParent(BusinessObject header, ZString typeCode, ZString keyToFindOutgoingMessageOrEntryNum)
		{
			CusMiscRequestHeader requestHeader = (CusMiscRequestHeader)header;
			requestHeader.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
		}

		public override BusinessObject LoadParent(BusinessObjectFactory factory, GlbCompany company, ZString entryNumber, ZString entryType)
		{
			return MessageLinkedObjectManager.GetLinkedMiscRequestHeader(factory, company, entryNumber, entryType);
		}
	}
}
