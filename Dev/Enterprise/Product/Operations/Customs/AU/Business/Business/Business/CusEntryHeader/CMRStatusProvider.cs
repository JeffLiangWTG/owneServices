using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRStatusProvider : ICusEntryHeaderStatusProvider
	{
		public CMRStatusProvider(CusEntryHeader entryHeader)
		{
			this.entryHeader = entryHeader;
		}

		public ZString MessageStatusDescription
		{
			get { return MessageStatusList.GetDescriptionFromCode(entryHeader.CH_Status); }
		}

		public ZString CargoStatusDescription
		{
			get { return EntryAdviceList.GetDescriptionFromCode(entryHeader.CH_EntryStatus); }
		}

		public ZString PaymentStatusDescription
		{
			get { return PaymentStatusList.GetDescriptionFromCode(entryHeader.AddInfo.ZA_PaymentStatus_Hidden); }
		}

		CMREntryPaymentStatusList PaymentStatusList
		{
			get
			{
				if (fPaymentStatusList == null)
				{
					fPaymentStatusList = new CMREntryPaymentStatusList();
				}
				return fPaymentStatusList;
			}
		}
		CMREntryPaymentStatusList fPaymentStatusList;

		public ZString ATDSecurityCode
		{
			get
			{
				CMRATDMessage aTDResponse = GetMostRecentMessage(new ZString[] { CMRMessage.CMRMessageTypes.ATD }) as CMRATDMessage;
				return aTDResponse != null ? aTDResponse.GetATDSecurityCode() : ZString.Empty;
			}
		}

		readonly CusEntryHeader entryHeader;

		#region EntryAdviceList

		CMRImportEntryAdviceList EntryAdviceList
		{
			get
			{
				if (fEntryAdviceList == null)
				{
					fEntryAdviceList = new CMRImportEntryAdviceList();
				}
				return fEntryAdviceList;
			}
		}
		CMRImportEntryAdviceList fEntryAdviceList;

		#endregion

		#region MessageStatusList

		internal CMRImportMessageStatusList MessageStatusList
		{
			get
			{
				if (fMessageStatusList == null)
				{
					fMessageStatusList = new CMRImportMessageStatusList();
				}
				return fMessageStatusList;
			}
		}
		CMRImportMessageStatusList fMessageStatusList;

		#endregion

		#region Most Recent Message

		CMRCUSRESMessage GetMostRecentMessage(ZString[] messageTypes)
		{
			var statusProvider = (IStatusNeedsRecalculationProvider)entryHeader;
			var lastResponse = statusProvider.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.CMR, messageTypes, EDIMessage.Direction.Receive);
			return lastResponse.Length > 0 ? (CMRCUSRESMessage)lastResponse[0] : null;
		}

		#endregion
	}
}
