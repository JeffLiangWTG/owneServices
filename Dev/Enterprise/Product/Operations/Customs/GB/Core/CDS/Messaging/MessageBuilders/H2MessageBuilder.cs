using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class H2MessageBuilder : H5MessageBuilder
	{
		public H2MessageBuilder(CusEntryHeader cusEntryHeader, ErrorCollector errorCollector, string functionCode) : base(cusEntryHeader, errorCollector, functionCode)
		{
		}

		protected override void PopulateDecAdditionalDocuments()
		{
		}

		protected override void PopulateExporter()
		{
		}

		protected override void PopulateItemConsignor(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateTradeTerms()
		{
		}

		protected override void PopulateSpecificTaxBaseQuantity(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
		}

		protected override void PopulatePaymentAmount(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
		}

		protected override void PopulateTaxAssessedAmount(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
		}

		protected override void PopulatePayment(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
		}

		protected override void PopulateCustomsValuations()
		{
		}

		protected override void PopulateCustomsValuations(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateValuationAdjustmentAdditionCode(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateNetNetWeightMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure goodsMeasure, ICommodity giCommodity)
		{
			if (GBCustomsDataRegistry.Instance.SendNetMassForH2Declarations.Value)
			{
				base.PopulateNetNetWeightMeasure(goodsMeasure, giCommodity);
			}
		}

		protected override void PopulateObligationGuarantees()
		{
			PopulateObligationGuaranteesBase();
		}

		protected override void PopulateInvoiceLineItemChargeAmount(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity)
		{
		}

		protected override void PopulateCurrencyExchanges()
		{
		}

		protected override bool IsArrivalTransportMeansDetailsRequiredCore => false;

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

		protected override void PopulateWriteOff(DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument result, IAdditionalDocument additionalDocument)
		{
		}
	}
}
