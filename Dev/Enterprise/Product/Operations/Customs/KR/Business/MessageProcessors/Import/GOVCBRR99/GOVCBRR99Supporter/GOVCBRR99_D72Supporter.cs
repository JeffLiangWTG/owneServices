using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR99SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._D72)]
	class GOVCBRR99_D72Supporter : IGOVCBRR99Supporter
	{
		ZString IGOVCBRR99Supporter.EntryType => SharedJobMessageTypeList.Codes.Import;

		void IGOVCBRR99Supporter.UpdateParent(EDIMessage message, CusEntryHeader entry, ZString typeCode, IGOVCBRR99MessageData messageData, ZStringBuilder emailBuilder)
		{
			var outgoingMessage = entry.Messages.GetLastMessageWithMatchingVersionNumber(typeCode, messageData.AmendSequence.ToString());
			message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;

			var entryNum_D72 = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._D72);
			if (entryNum_D72 != null)
			{
				if (messageData.AmendSequence > ZInt.ParseSafe(entryNum_D72.CE_EntryLineReference, 0))
				{
					entryNum_D72.CE_EntryLineReference = messageData.AmendSequence.ToString();
				}
			}
		}
	}
}
