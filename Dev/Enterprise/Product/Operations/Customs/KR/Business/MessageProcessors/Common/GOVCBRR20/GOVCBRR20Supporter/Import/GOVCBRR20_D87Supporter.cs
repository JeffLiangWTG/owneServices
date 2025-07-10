using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR20SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._D87)]
	class GOVCBRR20_D87Supporter : GOVCBRR20Supporter
	{
		public override ZString EntryType => ElectronicDocumentTypeList.Codes._D87;

		public override void UpdateParent(BusinessObject header, ZString typeCode, ZString keyToFindOutgoingMessageOrEntryNum)
		{
			CusEntryHeader entry = (CusEntryHeader)header;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
		}

		public override BusinessObject LoadParent(BusinessObjectFactory factory, GlbCompany company, ZString entryNumber, ZString entryType)
		{
			return MessageLinkedObjectManager.GetLinkedJobDeclarationD87(factory, company.PK, entryNumber);
		}
	}
}
