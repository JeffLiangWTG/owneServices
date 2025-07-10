using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class I1MessageBuilderControlledGoods : MessageBuilder
	{
		public I1MessageBuilderControlledGoods(CusEntryHeader cusEntryHeader, ErrorCollector errorCollector, string functionCode) : base(cusEntryHeader, errorCollector, functionCode)
		{
		}

		protected override void PopulateTaxAssessedAmount(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
		}

		protected override void PopulatePaymentAmount(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
		}

		protected override void PopulateValuationAdjustmentAdditionCode(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateCurrencyExchanges()
		{
		}

		protected override void PopulateBorderTransportMeans()
		{
			if (BorderTransportMeans != null)
			{
				var borderTransportMeans = BorderTransportMeans == null ? new DeclarationBorderTransportMeans() : new DeclarationBorderTransportMeans
				{
					ModeCode = new BorderTransportMeansModeCodeType { Value = BorderTransportMeans.ModeCode },
				};
				decMessage.BorderTransportMeans = borderTransportMeans;
			}
		}

		protected override void PopulateArrivalTransportMeans(DeclarationGoodsShipmentConsignment consignment, IConsignment consignmentData)
		{
		}

		protected override void PopulateTransactionNatureCode()
		{
		}

		protected override void PopulateTransactionNatureCode(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateStatisticalValue(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateAcceptanceDateTime()
		{
		}

		protected virtual bool ShouldSendInvoiceLineItemChargeAmount(ICommodity giCommodity) =>
			giCommodity.NoAdditionalProcedureCodesAreE01orE02 || !giCommodity.InvoiceLineItemCharge.Amount.IsEmpty;

		protected override void PopulateInvoiceLineItemChargeAmount(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity)
		{
			if (ShouldSendInvoiceLineItemChargeAmount(giCommodity))
			{
				base.PopulateInvoiceLineItemChargeAmount(commodity, giCommodity);
			}
		}
	}
}
