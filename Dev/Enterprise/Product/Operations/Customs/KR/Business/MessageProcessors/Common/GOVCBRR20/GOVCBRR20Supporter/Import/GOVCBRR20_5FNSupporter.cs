using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR20SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._5FN)]
	class GOVCBRR20_5FNSupporter : GOVCBRR20Supporter
	{
		public override ZString EntryType => ElectronicDocumentTypeList.Codes._5FN;

		public override void UpdateParent(BusinessObject header, ZString typeCode, ZString entryLineReference)
		{
			CusEntryHeader entry = (CusEntryHeader)header;
			var entryLineNumber = ZInt.ParseEmptyAsZero(entryLineReference);
			if (entryLineNumber != 0)
			{
				var entryNumber = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5FN && ZInt.ParseEmptyAsZero(x.CE_EntryLineReference) == entryLineNumber);
				if (entryNumber != null)
				{
					entryNumber.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalRejected;
				}
			}
		}

		public override EDIMessage GetOutgoingMessage(BusinessObject header, ZString typeCode, ZString versionNumber)
		{
			var lineReference = ZInt.ParseEmptyAsZero(versionNumber);
			return this.GetLastMessageWithMatchingVersionNumber(header, typeCode, lineReference.ToString());
		}
		public override ZString GetKeyToFindOutgoingMessageOrEntryNum(BusinessObject header, IGOVCBRR20MessageData messageData)
		{
			return messageData.EntryLineNo5FN;
		}
	}
}
