using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR20SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._5UL)]
	class GOVCBRR20_5ULSupporter : GOVCBRR20Supporter
	{
		public override ZString EntryType => ElectronicDocumentTypeList.Codes._5UL;
		public override void UpdateParent(BusinessObject header, ZString typeCode, ZString refundNumber)
		{
			var entryNumFilter = new ZQuery(CusEntryNumSchema.CE_EntryNum, refundNumber);
			entryNumFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, typeCode);
			entryNumFilter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.KoreaSouth);
			entryNumFilter.AddToFilter(CusEntryNumSchema.CE_ParentTable, new string[] { CusEntryHeader.Schema.TableName, CusReconDeclaration.Schema.TableName });
			var entryNum = header.Factory.LoadTop1<CusEntryNumber>(entryNumFilter);
			if (entryNum != null)
			{
				entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			}

			var reconDeclaration = header as CusReconDeclaration;
			if (reconDeclaration != null)
			{
				reconDeclaration.CRD_MessageStatus = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			}
		}

		public override BusinessObject LoadParent(BusinessObjectFactory factory, GlbCompany company, ZString refundNumber, ZString entryType)
		{
			BusinessObject result = null;
			var (entryNum5UL, entry, refundDeclaration, outgoingMessage) = MessageLinkedObjectManager.GetLinkedRefundObject(factory, company, refundNumber, entryType);
			if (entry != null)
			{
				result = entry;
			}
			else if (refundDeclaration != null)
			{
				result = refundDeclaration;
			}
			return result;
		}

		public override EDIMessage GetOutgoingMessage(BusinessObject header, ZString typeCode, ZString refundNumber)
		{
			EDIMessage messages = null;
			var entry = header as CusEntryHeader;
			if (entry != null)
			{
				messages = entry.Messages.GetLastMessageMatching(x => x.EM_MessageType == typeCode && x.EM_MessageOwner == refundNumber);
			}

			var reconDeclaration = header as CusReconDeclaration;
			if (reconDeclaration != null)
			{
				messages = (EDIMessage)reconDeclaration.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, typeCode);
			}
			return messages;
		}
		public override ZString GetKeyToFindOutgoingMessageOrEntryNum(BusinessObject header, IGOVCBRR20MessageData messageData)
		{
			return messageData.ApplicationNumber;
		}
	}
}
