using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR38SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._5DR, ElectronicDocumentTypeList.Codes._5DS)]
	class GOVCBRR385DR5DSSupporter : IGOVCBRR38Supporter
	{
		ZString IGOVCBRR38Supporter.EntryType => KRJobMessageTypeList.Codes.LocalExport;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Untranslatable string")]
		void IGOVCBRR38Supporter.UpdateParent(CusEntryHeader entry, ZString typeCode, IGOVCBRR38MessageData messageData)
		{
			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, typeCode);

			if (outgoingMessage != null)
			{
				entry.CH_Status = outgoingMessage.EM_MessageSubType == MessageSubTypeLocalExport.Amendment ?
				CustomsMessageStatusTypeList.Codes.AmendmentAccepted :
				CustomsMessageStatusTypeList.Codes.CancellationAccepted;
			}

			if (messageData.ContentDescription.Contains("\uc11c\ub958\uc81c\ucd9c\ub300\uc0c1"))
			{
				entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.NDC;
			}
		}
	}
}
