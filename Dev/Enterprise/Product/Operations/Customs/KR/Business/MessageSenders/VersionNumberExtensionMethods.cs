using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public static class VersionNumberExtensionMethods
	{
		public static void RecordVersionNumberFromCH_VersionID(this EDIMessage message, CusEntryHeader entry)
		{
			message.EM_ApplicationReference = (entry.CH_VersionID + 1).ToString();
		}

		public static void RecordVersionNumberFromCusEntryNum(this EDIMessage message, CusEntryHeader entry, string entryType)
		{
			var cusEntryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(entryType);
			message.EM_ApplicationReference = (ZInt.ParseSafe(cusEntryNum.CE_EntryLineReference, 0) + 1).ToString();
		}

		public static void UpdateEntryVersionID(this CusEntryHeader entry, EDIMessage outgoingMessage)
		{
			var versionNumber = ZShort.ParseSafe(outgoingMessage?.EM_ApplicationReference ?? ZString.Empty, 0);
			if (versionNumber == ZShort.Zero)
			{
				versionNumber = entry.CH_VersionID + 1;
			}

			entry.CH_VersionID = versionNumber;
		}

		public static ZShort CalculateNextCustoms5FEVersionNumber(this CusEntryHeader entry)
		{
			var last5FEMessageVersion = ZString.Empty;
			var sortedMessages = entry.Messages.Cast<EDIMessage>().OrderBy(x => x.EM_SystemCreateTimeUtc);
			var lastR99MessageReceivedYesterday = sortedMessages.LastOrDefault(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._R99 &&
													x.EM_MessageSubType == ElectronicDocumentTypeList.Codes._5FE &&
													x.EM_SystemCreateTimeUtc.ToLocalBranchTime() < ZDate.Today);

			if (lastR99MessageReceivedYesterday != null)
			{
				last5FEMessageVersion = sortedMessages.FirstOrDefault(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5FE && x.EM_MessageNum == lastR99MessageReceivedYesterday.EM_ApplicationReference).EM_ApplicationReference;
			}

			return entry.CH_VersionID + 1 - ZShort.ParseSafe(last5FEMessageVersion, 1);
		}
		public static ZShort GetCustoms5FEVersionNumber(this CusEntryHeader entry, EDIMessage current5FEMessage)
		{
			var current5FEApplicationReference = ZShort.ParseSafe(current5FEMessage.EM_ApplicationReference, 1);
			var current5FESentDate = current5FEMessage.EM_SystemCreateTimeUtc.ToLocalBranchTime().Date;
			return GetCustoms5FEVersionNumber(entry, current5FEApplicationReference, current5FESentDate);
		}
		public static ZShort GetCustoms5FEVersionNumber(this CusEntryHeader entry, ZShort current5FECW1VersionNo, ZDateTime current5FESentDate)
		{
			var totalCountsOfPreviousAccepted5FE = entry.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._R99 &&
														x.EM_MessageSubType == ElectronicDocumentTypeList.Codes._5FE &&
														x.EM_SystemCreateTimeUtc.ToLocalBranchTime() < current5FESentDate);
			return (ZShort)(current5FECW1VersionNo - totalCountsOfPreviousAccepted5FE - 1);
		}

		public static ZShort GetCW1VersionNumberFromCustoms5FEVersionNumber(this CusEntryHeader entry, ZShort customsVersionNumber, ZDate submissionDate)
		{
			var totalCountsOfPreviousAccepted5FE = entry.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._R99 &&
														x.EM_MessageSubType == ElectronicDocumentTypeList.Codes._5FE &&
														x.EM_SystemCreateTimeUtc.ToLocalBranchTime() < submissionDate);
			return (ZShort)(customsVersionNumber + totalCountsOfPreviousAccepted5FE + 1);
		}
	}
}
