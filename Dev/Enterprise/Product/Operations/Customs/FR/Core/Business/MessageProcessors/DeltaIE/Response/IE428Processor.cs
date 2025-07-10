using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE428;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class IE428Processor : DeltaIEBaseProcessor<CC428BType>
	{
		public IE428Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetNewMessageStatus() => MessageStatusCodeList.Codes.OK;

		protected override ZString GetLRNFromResponseMessage(CC428BType messageObject) => messageObject.ImportOperation?.LRN;

		protected override ZString GetNewCRN(CC428BType messageObject) => messageObject.ImportOperation?.CustomsRegistrationNumber;

		protected override ZString GetNewMRN(CC428BType messageObject) => messageObject.ImportOperation?.MRN;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.DeclarationAcceptance;

		protected override ZString GetNewEntryStatus() => DeltaIEImportCusEntryStatusList.Codes.DeclarationAcceptedMrnAllocated;

		protected override ZString GetEntryNumber(CC428BType messageObject) => messageObject.ImportOperation?.CustomsRegistrationNumber ?? ZString.Empty;

		protected override ZDateTime GetNewCustomsEntryIssueDate(CC428BType messageObject) => ParseUtcDateTimeString(messageObject.ImportOperation?.DeclarationAcceptanceDateAndTime ?? ZString.Empty);

		protected override ZString GetEntryStatusChangedTimeString(CC428BType messageObject) => messageObject.DeclarationStatus?.StateDateTime;

		protected override ZDateTime GetNewIssueDate(CC428BType messageObject)
			=> ParseUtcDateTimeString(messageObject.DeclarationStatus?.StateDateTime ?? ZString.Empty);

		protected override void UpdateFees(CusEntryHeader entryHeader, CC428BType messageObject)
		{
			DeleteConfirmedFees(entryHeader);
			UpdateEntryLineFees(entryHeader, messageObject);

			var dutiesAndTaxesSummaries = messageObject.GeneralTaxation?.DutiesAndTaxesSummaries;
			UpdateEntryCharges(entryHeader, dutiesAndTaxesSummaries, s => s.TaxType, s => s.PayableTaxAmount, s => s.NationalTaxType, s => s.TaxationStatus);

			var guaranteedAmount = messageObject.GeneralTaxation?.TotalPayableTaxAmount?.AmountUsed?.GuaranteedAmount;
			UpdateEntryGuaranteeAmount(entryHeader, guaranteedAmount);
		}

		protected void UpdateEntryLineFees(CusEntryHeader entryHeader, CC428BType messageObject)
		{
			var detailedTaxationGoodsShipment = messageObject.DetailedTaxation?.GoodsShipment;
			var rootGoodsShipment = messageObject.GoodsShipment;

			if (detailedTaxationGoodsShipment != null || rootGoodsShipment != null)
			{
				foreach (CusEntryLine entryLine in entryHeader.AllEntryLines)
				{
					var taxationShipmentItem = detailedTaxationGoodsShipment?.GoodsShipmentItem?.FirstOrDefault(gsi => gsi.DeclarationGoodsItemNumber == entryLine.CL_LineNumber.ToString());
					if (taxationShipmentItem != null)
					{
						taxationShipmentItem.DutiesAndTaxes?.ForEach(x => CreateEntryLineFee(entryLine, x));
					}

					var rootShipmentItem = rootGoodsShipment?.GoodsShipmentItem?.FirstOrDefault(gsi => gsi.DeclarationGoodsItemNumber == entryLine.CL_LineNumber.ToString());
					if (rootShipmentItem != null)
					{
						UpdateEntryLine(entryLine, rootShipmentItem);
					}
				}
			}
		}

		void UpdateEntryLine(CusEntryLine entryLine, MGoodsShipmentItemType05FR goodsShipmentItem)
		{
			entryLine.CL_ConfirmedCustomsValue = goodsShipmentItem.CustomsValue;
			entryLine.CL_ConfirmedStatisticalValue = goodsShipmentItem.StatisticalValue;
			entryLine.CL_ConfirmedValueForVAT = goodsShipmentItem.VATBase;
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
