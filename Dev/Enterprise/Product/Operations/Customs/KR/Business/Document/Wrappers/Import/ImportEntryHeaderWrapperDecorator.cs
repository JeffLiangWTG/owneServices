using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public static class ImportEntryHeaderWrapperDecorator
	{
		public static void Decorate(this ImportEntryHeaderWrapper wrapper, CusEntryHeader entry)
		{
			wrapper.MessageStatus = entry.CH_Status;
			wrapper.EntryReleaseDate = entry.CH_EntryReleaseDate;
			wrapper.DeclarationDate = entry.CusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
			wrapper.CancellationDecisionDate = entry.CusEntryNumber?.CE_ExpiryDate ?? ZDateTime.Empty;
			wrapper.IsImportCancellationDeclinedByCustoms = GetImportCancellationDeclinedByCustoms(entry);
			wrapper.CustomsOfficers5BF = entry.CustomsOfficers.Cast<CustomsOfficer>().FirstOrDefault(x => x.CY_Code == ElectronicDocumentTypeList.Codes._5BF);
			wrapper.CustomsOfficerName5BD = entry.CustomsOfficers.Cast<CustomsOfficer>().FirstOrDefault(x => x.CY_Code == CustomsOfficerTypeList.Codes._5BDResponsibleCustomsOfficer)?.CY_Data ?? ZString.Empty;

			wrapper.EntryNumIMP = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == KRJobMessageTypeList.Codes.Import);
			wrapper.EntryNum5BA = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5BA);
			wrapper.EntryNumD72 = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._D72);
			wrapper.EntryNum5GU = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5GU);
			wrapper.EntryNum5UA = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UA);
			wrapper.EntryNum5BD = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5BD);
			wrapper.EntryNum5TM = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5TM);

			wrapper.MessageData5TV = GetMessageWrapper(entry, ElectronicDocumentTypeList.Codes._5TV)?.MessageData5TV;
			wrapper.MessageData5UO = GetMessageWrapper(entry, ElectronicDocumentTypeList.Codes._5UO)?.MessageData5UO;
			wrapper.MessageData5WN = GetMessageWrapper(entry, ElectronicDocumentTypeList.Codes._5WN)?.MessageData5WN;
			wrapper.MessageData5TW = GetMessageWrapper(entry, ElectronicDocumentTypeList.Codes._5TW)?.MessageData5TW;
			wrapper.MessageSendingObject5BF = GetMessageWrapper(entry, ElectronicDocumentTypeList.Codes._5BF)?.MessageSendingObject5BF;
			wrapper.MessageData5GU = GetMessageWrapper(entry, ElectronicDocumentTypeList.Codes._5GU)?.MessageData5GU;
			wrapper.MessageData5UB = GetMessageWrapper(entry, ElectronicDocumentTypeList.Codes._5UB)?.MessageData5UB;
			wrapper.MessageSendingObject5BD = GetMessageWrapper(entry, ElectronicDocumentTypeList.Codes._5BD)?.MessageSendingObject5BD;
			wrapper.MessageData5GV = GetMessageWrapper(entry, ElectronicDocumentTypeList.Codes._5GV)?.MessageData5GV;
			wrapper.MessageData5BE = GetMessageWrapper(entry, ElectronicDocumentTypeList.Codes._5BE)?.MessageData5BE;

			wrapper.EntryInstruction5BA = entry.EntryInstruction;
			wrapper.BrokerBusinessRegNo = entry.Declaration.BrokerAddress.GetRegistrationNumber(IdentificationType.BusinessRegNo);

			wrapper.HasAny5FNRejection = entry.HasAny5FNRejection;
			wrapper.ImporterTypeOfBusiness = OrgHeaderWrapper.New(entry.Declaration?.ImporterAddress?.Header)?.ZO_TypeOfBusiness ?? ZString.Empty;
			wrapper.PayerFormattedCorporationCode = new OrganizationDocWrapper(entry.Declaration.DutyPayer).FormattedCorporationCode;
		}

		static EDIMessage GetLastMessage(CusEntryHeader entry, string messageType)
		{
			return entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == messageType).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
		}

		static EDIMessageWrapper GetMessageWrapper(CusEntryHeader entry, string messageType)
		{
			EDIMessageWrapper result = null;
			var lastMessage = GetLastMessage(entry, messageType);
			if (lastMessage != null)
			{
				if (lastMessage.EM_MessageType == ElectronicDocumentTypeList.Codes._5TW)
				{
					if (lastMessage.EM_MessageSubType != OneOrMultiple.ONE)
					{
						var messageFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.KRCustoms);
						messageFilter.AddToFilter(EDIMessageSchema.EM_MessageType, ElectronicDocumentTypeList.Codes._5TW);
						messageFilter.AddToFilter(EDIMessageSchema.EM_MessageSubType, OneOrMultiple.MUL);
						messageFilter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
						messageFilter.AddToFilter(EDIMessageSchema.EM_MessageNum, lastMessage.EM_ApplicationReference);
						messageFilter.AddToFilter(EDIMessageSchema.EM_GB, entry.Declaration.Company.Branches.Select(x => x.PK));
						lastMessage = entry.Factory.LoadTop1<EDIMessage>(messageFilter);
					}
				}
				result = new EDIMessageWrapper(lastMessage);
			}
			return result;
		}

		static ZBool GetImportCancellationDeclinedByCustoms(CusEntryHeader entry)
		{
			var result = false;
			var message5BG = GetLastMessage(entry, ElectronicDocumentTypeList.Codes._5BG);
			if (message5BG != null && entry.CusEntryNumber?.CE_ExpiryDate == ZDateTime.Empty)
			{
				result = true;
			}
			return result;
		}
	}
}
