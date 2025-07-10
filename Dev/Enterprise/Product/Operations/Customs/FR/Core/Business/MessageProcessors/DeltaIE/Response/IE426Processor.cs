using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE426;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class IE426Processor : DeltaIEBaseProcessor<CC426BType>
	{
		public IE426Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetNewMessageStatus() => MessageStatusCodeList.Codes.OK;

		protected override ZString GetLRNFromResponseMessage(CC426BType messageObject) => messageObject.ImportOperation?.LRN;

		protected override ZString GetNewCRN(CC426BType messageObject) => messageObject.ImportOperation?.CustomsRegistrationNumber;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.PrelodgedDeclarationAcceptance;

		protected override ZString GetNewEntryStatus() => DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered;

		protected override ZString GetEntryNumber(CC426BType messageObject) => messageObject.ImportOperation?.CustomsRegistrationNumber ?? ZString.Empty;

		protected override ZDateTime GetNewCustomsEntryIssueDate(CC426BType messageObject) => ParseUtcDateTimeString(messageObject.ImportOperation?.DeclarationRegistrationDateAndTime ?? ZString.Empty);

		protected override ZString GetEntryStatusChangedTimeString(CC426BType messageObject) => messageObject.DeclarationStatus?.StateDateTime;

		protected override ZDateTime GetNewIssueDate(CC426BType messageObject)
			=> ParseUtcDateTimeString(messageObject.DeclarationStatus?.StateDateTime ?? ZString.Empty);

		protected override ZDateTime GetNewExpiryDate(CC426BType messageObject)
			=> ParseUtcDateTimeString(messageObject.ImportOperation?.PresentationNotificationDueDate ?? ZString.Empty);

		protected override void UpdateFees(CusEntryHeader entryHeader, CC426BType messageObject)
		{
			DeleteConfirmedFees(entryHeader);
			UpdateEntryLineFees(entryHeader, messageObject);

			var dutiesAndTaxesSummaries = messageObject.GeneralTaxation?.DutiesAndTaxesSummaries;
			UpdateEntryCharges(entryHeader, dutiesAndTaxesSummaries, s => s.TaxType, s => s.PayableTaxAmount, s => s.NationalTaxType, s => s.TaxationStatus);

			var guaranteedAmount = messageObject.GeneralTaxation?.TotalPayableTaxAmount?.AmountUsed?.GuaranteedAmount;
			UpdateEntryGuaranteeAmount(entryHeader, guaranteedAmount);
		}

		protected void UpdateEntryLineFees(CusEntryHeader entryHeader, CC426BType messageObject)
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
