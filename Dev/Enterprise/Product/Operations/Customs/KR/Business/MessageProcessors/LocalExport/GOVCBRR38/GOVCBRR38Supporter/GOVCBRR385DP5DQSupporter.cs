using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR38SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._5DP, ElectronicDocumentTypeList.Codes._5DQ)]
	class GOVCBRR385DP5DQSupporter : IGOVCBRR38Supporter
	{
		ZString IGOVCBRR38Supporter.EntryType => KRJobMessageTypeList.Codes.LocalExport;

		void IGOVCBRR38Supporter.UpdateParent(CusEntryHeader entry, ZString typeCode, IGOVCBRR38MessageData messageData)
		{
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			entry.CH_BGMReference = messageData.ConfirmNumber;

			entry.EntryNumbers.UpdateCusEntryNumIfExists(KRJobMessageTypeList.Codes.LocalExport, CusEntryNumber.Schema.CE_IssueDate, messageData.AcceptDateTime);

			if (!messageData.ContentDescription.IsEmpty)
			{
				var remarks = messageData.NoticeDateTime.ToString(DateFormatType.DateTime) + "\r\n" + messageData.ContentDescription;

				entry.CH_CustomsMessageRemarks = remarks + (!entry.CH_CustomsMessageRemarks.IsEmpty ? "\r\n\r\n" : "") + entry.CH_CustomsMessageRemarks;
			}

			if (messageData.ContentDescription.Contains((NoResString)"\uc11c\ub958\uc81c\ucd9c\ub300\uc0c1"))
			{
				entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.NDC;
			}
		}
	}
}
