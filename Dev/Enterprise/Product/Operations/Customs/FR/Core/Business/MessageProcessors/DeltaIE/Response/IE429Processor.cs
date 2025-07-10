using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE429;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class IE429Processor : DeltaIEBaseProcessor<CC429BType>
	{
		public IE429Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetNewMessageStatus() => MessageStatusCodeList.Codes.OK;

		protected override ZString GetLRNFromResponseMessage(CC429BType messageObject) => messageObject.ImportOperation?.LRN;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.ReleaseNotification;

		protected override ZString GetNewEntryStatus() => DeltaIEImportCusEntryStatusList.Codes.Released;

		protected override ZString GetEntryStatusChangedTimeString(CC429BType messageObject) => messageObject.DeclarationStatus?.StateDateTime;

		protected override ZDateTime GetNewCustomsEntryIssueDate(CC429BType messageObject) => ParseUtcDateTimeString(messageObject.ImportOperation?.DeclarationAcceptanceDate ?? ZString.Empty);

		protected override ZDateTime GetNewReleaseDate(CC429BType messageObject) => ParseUtcDateTimeString(messageObject.ImportOperation?.ReleaseDate ?? ZString.Empty);

		protected override void DoExtraProcessing(CusEntryHeader entryHeader, EDIMessage inboundMessage, bool isFirstTimeProcessing)
		{
			base.DoExtraProcessing(entryHeader, inboundMessage, isFirstTimeProcessing);

			UpdateAdditionalDeclarationType(entryHeader);

			UpdateTemporaryStorageRegisterIfApplicable(entryHeader, inboundMessage, isFirstTimeProcessing);
		}

		protected void UpdateAdditionalDeclarationType(CusEntryHeader entryHeader)
		{
			var entryInstruction = entryHeader?.EntryInstruction;
			if (entryInstruction != null)
			{
				if (entryInstruction.IsSimplified)
				{
					if (entryInstruction.CEI_SubStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC)
					{
						entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
					}
				}
				else
				{
					if (entryInstruction.CEI_SubStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA)
					{
						entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
					}
				}
			}
		}

		protected void UpdateTemporaryStorageRegisterIfApplicable(CusEntryHeader entryHeader, EDIMessage inboundMessage, bool isFirstTimeProcessing)
		{
			if (isFirstTimeProcessing)
			{
				var processor = new CusTempStorageRegisterProcessor<EDIMessage>(entryHeader, Logger, notifierWhenInsufficient: Notifier);
				try
				{
					processor.CalculateTransactionsAndLockMutexIfNeededForAddingTransaction();
					processor.AddTransactionsWhenSaving(inboundMessage);
				}
				finally
				{
					processor.UnlockRegistersMutexes();
				}

				void Notifier(ZString failingRegister)
				{
					Logger.LogWarning((NoResString)$"Warning: Processing received message #{inboundMessage.EM_MessageNum}, type {inboundMessage.EM_MessageType} {inboundMessage.EM_MessageSubType}. Register {failingRegister} has not been updated.");
					entryHeader.Logs.AddNew(Events.DeclarationHasErrors, $"Warning: Register {failingRegister} has not been updated.", ZDateTimeOffset.UtcNow);

					var declarationNumber = entryHeader.Declaration?.JobNumber;
					var body = Res.GetString("F4066F49-9A41-4F16-8A54-735DC9DA7256", "Temporary Storage: {0} has not been updated by Declaration: {1}. There are not enough packages remaining.", failingRegister, declarationNumber);
					ProcessorHelper.SendEmail(entryHeader.Factory, entryHeader.CH_SystemCreateUser, true, GetEmailBody(body), GetEmailsubject(declarationNumber, failingRegister), GetEmailGroupRegistryItem());
				}
			}
		}

		protected override void UpdateFees(CusEntryHeader entryHeader, CC429BType messageObject)
		{
			DeleteConfirmedFees(entryHeader);
			UpdateEntryLineFees(entryHeader, messageObject);

			var dutiesAndTaxesSummaries = messageObject.GeneralTaxation?.DutiesAndTaxesSummaries;
			UpdateEntryCharges(entryHeader, dutiesAndTaxesSummaries, s => s.TaxType, s => s.PayableTaxAmount, s => s.NationalTaxType, s => s.TaxationStatus);

			var guaranteedAmount = messageObject.GeneralTaxation?.TotalPayableTaxAmount?.AmountUsed?.GuaranteedAmount;
			UpdateEntryGuaranteeAmount(entryHeader, guaranteedAmount);
		}

		protected void UpdateEntryLineFees(CusEntryHeader entryHeader, CC429BType messageObject)
		{
			var goodsShipment = messageObject.DetailedTaxation?.GoodsShipment;

			if (goodsShipment != null)
			{
				foreach (CusEntryLine entryLine in entryHeader.AllEntryLines)
				{
					var goodsShipmentItem = goodsShipment.GoodsShipmentItem?.Where(gsi => gsi.DeclarationGoodsItemNumber == entryLine.CL_LineNumber.ToString())?.FirstOrDefault();
					if (goodsShipmentItem != null)
					{
						goodsShipmentItem.DutiesAndTaxes?.ForEach(x => CreateEntryLineFee(entryLine, x));
					}
				}
			}
		}

		void CreateEntryLineFee(CusEntryLine entryLine, DutiesAndTaxesType fee)
		{
			var entryLineFee = entryLine.ConfirmedFees.AddNew();
			entryLineFee.CF_ChargeType = fee.TaxType;
			entryLineFee.CF_MethodOfCalculation = fee.TaxBase?.FirstOrDefault()?.MeasurementUnitAndQualifier ?? ZString.Empty;
			entryLineFee.CF_BaseValue = (decimal)(fee.TaxBase?.FirstOrDefault()?.Amount ?? 0d);
			entryLineFee.CF_Rate = (decimal)(fee.TaxBase?.FirstOrDefault()?.TaxRate ?? 0d);
			entryLineFee.CF_ChargeAmount = (decimal)(fee.PayableTaxAmount);
			entryLineFee.CF_MethodOfPayment = fee.MethodOfPayment;
			entryLineFee.NationalFeeTypeCode = fee.NationalTaxType;
			entryLineFee.CF_Source = Enterprise.Customs.Business.CusEntryLineFeeSourceCodeList.Codes.CUS;
			entryLineFee.G4_RateOverride = ZString.Empty;
		}
	}
}
