using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using AUCusEntryHeader = Enterprise.Customs.AU.Declaration.Business.CusEntryHeader;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class REFACCMessageProcessor : BaseImportDeclarationMessageProcessor
	{
		public REFACCMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.REFACC, "Refund Acknowledgement Response (REFACC)")
		{
		}

		protected override bool ShouldSendAcknowledgementReport
		{
			get { return true; }
		}

		protected override bool DoAdditionalProcessing()
		{
			bool result = base.DoAdditionalProcessing();
			if (result)
			{
				var responseMessage = (CMRREFACCMessage)incomingMessage;
				if (entryHeader != null)
				{
					AssignValuesToCusEntryPayInfo(entryHeader, responseMessage);
				}
			}
			return result;
		}

		void AssignValuesToCusEntryPayInfo(AUCusEntryHeader entryHeader, CMRREFACCMessage responseMessage)
		{
			var payInfos = entryHeader.EntryPayInfos.GetPendingItems();
			CusEntryPayInfo payInfo = null;
			foreach (var onePayInfo in payInfos)
			{
				if (onePayInfo.C9_CusResReceived && !onePayInfo.C9_RemAdvReceived)
				{
					payInfo = onePayInfo;
					break;
				}
			}

			if (payInfo == null)
			{
				payInfo = entryHeader.EntryPayInfos.AddNew();
			}

			payInfo.C9_RemAdvReceived = true;
			payInfo.C9_TransactionType = PaymentTransactionTypeList.Codes.Refund;
			payInfo.C9_PaymentDate = responseMessage.REFACCInfoProvider.PaymentFinalisedDate;
			payInfo.C9_PaymentReference = responseMessage.REFACCInfoProvider.EFTRunNumber;
			payInfo.C9_PaymentParty = entryHeader.Declaration.JE_PaymentMethod;
			payInfo.C9_PaymentStatus = payInfo.C9_CusResReceived ? CusEntryPayInfoStatusList.Codes.Clear : CusEntryPayInfoStatusList.Codes.Pending;
		}
	}
}
