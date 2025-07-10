using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.ZArchitecture.Business;
using AUCusEntryHeader = Enterprise.Customs.AU.Declaration.Business.CusEntryHeader;
using CusEntryPayInfo = Enterprise.Customs.Business.CusEntryPayInfo;
using CusEntryPayInfoStatusList = Enterprise.Customs.Business.CusEntryPayInfoStatusList;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class IMDRMessageProcessor : CMRHeaderAndLineChargesResponseProcessor
	{
		public IMDRMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.IMD, "Import Declaration Response(IMDR)")
		{
		}

		protected override bool DoAdditionalProcessing()
		{
			bool result = base.DoAdditionalProcessing();
			if (result && entryHeader != null && incomingMessage is CMRIMDRMessage responseMessage)
			{
				var declaration = entryHeader.Declaration;
				if (declaration != null)
				{
					declaration.Logger = Logger;
				}

				if (responseMessage.IMDRInfoProvider.TotalPayable < 0)
				{
					AssignValuesToCusEntryPayInfo(entryHeader, responseMessage);
				}
			}
			return result;
		}

		void AssignValuesToCusEntryPayInfo(AUCusEntryHeader entryHeader, CMRIMDRMessage responseMessage)
		{
			CusEntryPayInfo[] payInfos = entryHeader.EntryPayInfos.GetPendingItems();
			CusEntryPayInfo payInfo = null;
			foreach (CusEntryPayInfo onePayInfo in payInfos)
			{
				if (!onePayInfo.C9_CusResReceived && onePayInfo.C9_RemAdvReceived)
				{
					payInfo = onePayInfo;
					break;
				}
			}

			if (payInfo == null)
			{
				payInfo = entryHeader.EntryPayInfos.AddNew();
			}

			payInfo.C9_CusResReceived = true;
			payInfo.C9_IncomingPayResponseNo = responseMessage.EM_MessageNum;
			payInfo.C9_TransactionType = PaymentTransactionTypeList.Codes.Refund;
			payInfo.C9_PaymentParty = entryHeader.Declaration.JE_PaymentMethod;
			payInfo.C9_PaymentAmount = responseMessage.IMDRInfoProvider.TotalPayable;
			payInfo.C9_PaymentStatus = payInfo.C9_RemAdvReceived ? CusEntryPayInfoStatusList.Codes.Clear : CusEntryPayInfoStatusList.Codes.Pending;
		}

		bool fGISLineBreakDownIndicatorPresent;
		bool fGISLineBreakDownIndicatorPresentSet;

		protected override bool GISLineBreakDownIndicatorPresent(SegmentGroup11 group11)
		{
			if (!fGISLineBreakDownIndicatorPresentSet)
			{
				fGISLineBreakDownIndicatorPresentSet = true;
				fGISLineBreakDownIndicatorPresent = false;
				if (cUSRES.FTX.Count > 0)
				{
					foreach (GISSegment gIS in cUSRES.GIS)
					{
						if (gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode == "LLB")
						{
							fGISLineBreakDownIndicatorPresent = true;
							break;
						}
					}
				}
				if (!fGISLineBreakDownIndicatorPresent)
				{
					entryHeader.Declaration.Logs.AddNew(Events.ManualMatchDone, "Reply to CI Amendment Received");
				}
			}
			return fGISLineBreakDownIndicatorPresent;
		}

		protected override void SetEntryHeaderStatus()
		{
			if (cUSRES != null && entryHeader != null)
			{
				var status = ZString.Empty;
				foreach (FTXSegment fTX in cUSRES.FTX)
				{
					if (fTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.StatusDetails)
					{
						status = GetStatus(fTX.TextLiteral.FreeTextValue2);
						if (status != CMRImportEntryAdvice.Finalised.Code || entryHeader.CH_EntryStatus != CMRImportEntryAdvice.ATDReceived.Code)
						{
							entryHeader.CH_EntryStatus = status;
						}
						break;
					}
				}

				SetIsSubjectToRedLineProcessing();
				var declaration = entryHeader.Declaration;
				declaration.JE_EntryStatus = entryHeader.Declaration.SummaryEntryStatusCalculator.SummaryEntryStatus;
				if (declaration.IsWHSUniversalXMLActive && !declaration.IsBondedWarehousingDisabled && declaration.SupportsBondedWarehousing
					&& (IsEntryStatusRelevantForWHSUpdate(declaration, status)
						|| (incomingMessage.IsRejected && IsStatusInterestingForWHSUpdate))
					&& (declaration.HasWHSTransaction || !entryHeader.IsNature10))
				{
					incomingMessage.Factory.Saved -= Factory_Saved;
					incomingMessage.Factory.Saved += Factory_Saved;
					delayEmailReport = true;
				}
			}
		}

		bool IsEntryStatusRelevantForWHSUpdate(JobDeclaration declaration, ZString status)
		{
			var result = status == CMRImportEntryAdvice.Withdrawn.Code;
			if (result)
			{
				var whsStatus = declaration.WarehouseTransactionStatus;
				result = whsStatus == WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal || declaration.IsExWarehouse;
			}
			else
			{
				if (declaration.IsExWarehouse)
				{
					result = status == CMRImportEntryAdvice.Finalised.Code || status == CMRImportEntryAdvice.Clear.Code;
				}
				else
				{
					result = ShouldUpdateInward(declaration.WarehouseTransactionStatus) && (status == CMRImportEntryAdvice.Finalised.Code || status == CMRImportEntryAdvice.Clear.Code || status == CMRImportEntryAdvice.Held.Code);
				}
			}
			return result;
		}

		bool IsStatusInterestingForWHSUpdate
		{
			get
			{
				var status = entryHeader.CH_Status;
				return status == CustomsEntryStatus.ClearAmendment.Code ||
					status == CustomsEntryStatus.FailAmendment.Code ||
					status == CustomsEntryStatus.AwaitingAmendment.Code ||
					status == CustomsEntryStatus.ClearFormalLodge.Code ||
					status == CustomsEntryStatus.FailFormalLodge.Code ||
					status == CustomsEntryStatus.AwaitingFormalLodge.Code ||
					status == CustomsEntryStatus.ClearWithdrawal.Code ||
					status == CustomsEntryStatus.FailWithdrawal.Code ||
					status == CustomsEntryStatus.AwaitingWithdrawal.Code;
			}
		}
	}
}
