using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR20SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._929)]
	class GOVCBRR20_929Supporter : GOVCBRR20Supporter
	{
		public override ZString EntryType => SharedJobMessageTypeList.Codes.Import;

		public override void UpdateParent(BusinessObject header, ZString typeCode, ZString keyToFindOutgoingMessageOrEntryNum)
		{
			CusEntryHeader entry = (CusEntryHeader)header;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
		}

		public override EDIMessage GetOutgoingMessage(BusinessObject header, ZString typeCode, ZString keyToFindOutgoingMessageOrEntryNum)
		{
			var outgoingMessage = base.GetOutgoingMessage(header, typeCode, keyToFindOutgoingMessageOrEntryNum);
			if (outgoingMessage != null)
			{
				((CusEntryHeader)header).ResetChargesAndLineFeesVersionAndAmount(outgoingMessage.EM_ApplicationReference);
			}
			return outgoingMessage;
		}
	}
}
