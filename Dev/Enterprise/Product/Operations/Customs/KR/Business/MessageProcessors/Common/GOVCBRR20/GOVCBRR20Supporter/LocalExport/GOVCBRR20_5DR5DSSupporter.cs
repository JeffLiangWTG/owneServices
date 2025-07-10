using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR20SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._5DR, ElectronicDocumentTypeList.Codes._5DS)]
	class GOVCBRR20_5DR5DSSupporter : GOVCBRR20Supporter
	{
		public override ZString EntryType => KRJobMessageTypeList.Codes.LocalExport;

		public override void UpdateParent(BusinessObject header, ZString typeCode, ZString keyToFindOutgoingMessageOrEntryNum)
		{
			CusEntryHeader entry = (CusEntryHeader)header;
			var message = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, typeCode);

			entry.CH_Status = message.EM_MessageSubType == Constants.MessageSubTypeLocalExport.Amendment ?
				CustomsMessageStatusTypeList.Codes.AmendmentRejected :
				CustomsMessageStatusTypeList.Codes.CancellationRejected;
		}
	}
}
