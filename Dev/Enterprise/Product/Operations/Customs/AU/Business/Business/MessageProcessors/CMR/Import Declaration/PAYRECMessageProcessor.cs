using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;
using AUCusEntryHeader = Enterprise.Customs.AU.Declaration.Business.CusEntryHeader;
using CusEntryPayInfoStatusList = Enterprise.Customs.Business.CusEntryPayInfoStatusList;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PAYRECMessageProcessor : BaseImportDeclarationMessageProcessor
	{
		public PAYRECMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.PAYREC, "Payment Receipt Response (PAYREC)")
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
				if (entryHeader != null)
				{
					CMRPAYRECMessage responseMessage = (CMRPAYRECMessage)incomingMessage;
					entryHeader.AQISServicePaymentAmount += responseMessage.PAYRECInfoProvider.AQISServicePayment;
					entryHeader.CH_TotalPaid += responseMessage.PAYRECInfoProvider.AQISServicePayment;

					//PAYREC for Customs Charge for Original IMD
					if (!entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden)
					{
						entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = responseMessage.PAYRECInfoProvider.TotalPayable > responseMessage.PAYRECInfoProvider.AQISServicePayment;
					}

					if (entryHeader.CH_EntryStatus != CMRImportEntryAdvice.Processing.Code)
					{
						ZString entryNumber = responseMessage.EntryNumber;

						if (!entryNumber.IsEmpty)
						{
							entryHeader.EntryNumber = entryNumber;
						}
					}

					var payInfo = entryHeader.EntryPayInfos.GetItemByMessageNum(responseMessage.EM_MessageNum);
					if (payInfo == null)
					{
						AssignValuesToCusEntryPayInfo(entryHeader, responseMessage);
					}

					var declaration = entryHeader.Declaration;
					if (declaration.IsWHSUniversalXMLActive && declaration.IsInwardBondedWarehousingEnabled
						&& ShouldUpdateInward(declaration.WarehouseTransactionStatus) && HasPaidEntry(responseMessage.PAYRECInfoProvider))
					{
						incomingMessage.Factory.Saved -= Factory_Saved;
						incomingMessage.Factory.Saved += Factory_Saved;
						delayEmailReport = true;
					}

					if (responseMessage.PAYRECInfoProvider.AQISServicePayment > 0)
					{
						declaration.Logger = Logger;
						entryHeader.MarkNeedsAutoRateASPOnSaved();
					}
				}
			}
			return result;
		}

		bool HasPaidEntry(PAYRECInfoProvider provider)
		{
			return provider.TotalPayable != provider.AQISServicePayment;
		}

		void AssignValuesToCusEntryPayInfo(AUCusEntryHeader entryHeader, CMRPAYRECMessage responseMessage)
		{
			var payInfo = entryHeader.EntryPayInfos.AddNew();
			payInfo.C9_IncomingPayResponseNo = responseMessage.EM_MessageNum;
			payInfo.C9_RemAdvReceived = true;
			payInfo.C9_CusResReceived = true;

			ZDecimal aQISPaymentAmount = responseMessage.PAYRECInfoProvider.AQISServicePayment;
			ZDecimal totalPayable = responseMessage.PAYRECInfoProvider.TotalPayable;

			payInfo.C9_PaymentAmount = totalPayable;
			if (aQISPaymentAmount == 0m && totalPayable > 0m)
			{
				payInfo.C9_TransactionType = PaymentTransactionTypeList.Codes.CustomsChargePayment;
			}
			else if (aQISPaymentAmount > 0m && aQISPaymentAmount == totalPayable)
			{
				payInfo.C9_TransactionType = PaymentTransactionTypeList.Codes.AQISPayment;
			}
			else if (aQISPaymentAmount > 0m && aQISPaymentAmount < totalPayable)
			{
				payInfo.C9_TransactionType = PaymentTransactionTypeList.Codes.CustomsAQISPayment;
			}

			payInfo.C9_PaymentDate = responseMessage.PAYRECInfoProvider.PaymentFinalisedDate;
			payInfo.C9_PaymentReference = responseMessage.PAYRECInfoProvider.ICSReceiptNumber;
			payInfo.C9_PaymentStatus = CusEntryPayInfoStatusList.Codes.Clear;
			payInfo.C9_PaymentParty = entryHeader.Declaration.JE_PaymentMethod;
		}
	}
}
