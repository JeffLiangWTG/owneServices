using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR99SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._5FN)]
	class GOVCBRR99_5FNSupporter : IGOVCBRR99Supporter
	{
		ZString IGOVCBRR99Supporter.EntryType => ElectronicDocumentTypeList.Codes._5FN;

		void IGOVCBRR99Supporter.UpdateParent(EDIMessage message, CusEntryHeader entry, ZString typeCode, IGOVCBRR99MessageData messageData, ZStringBuilder emailBuilder)
		{
			var lineReference = ZInt.ParseEmptyAsZero(messageData.EntryLineNo5FN);
			if (lineReference > 0)
			{
				var entryNumber = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5FN && ZInt.ParseEmptyAsZero(x.CE_EntryLineReference) == lineReference);
				if (entryNumber != null)
				{
					entryNumber.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
					entryNumber.CE_IssueDate = messageData.AcceptDateTime;
				}
				var outgoingMessage = entry.Messages.GetLastMessageWithMatchingVersionNumber(ElectronicDocumentTypeList.Codes._5FN, lineReference.ToString());
				message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;
			}
		}
	}
}
