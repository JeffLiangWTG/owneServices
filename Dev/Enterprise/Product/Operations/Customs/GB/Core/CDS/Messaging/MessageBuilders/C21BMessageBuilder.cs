using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class C21BMessageBuilder : MessageBuilder
	{
		public C21BMessageBuilder(BusinessObject messagingParent, ErrorCollector errorCollector, string functionCodeNewAmendDelete)
			: base(messagingParent, errorCollector, functionCodeNewAmendDelete)
		{
		}

		protected override void PopulateDestination()
		{
		}

		protected override void PopulateDecAdditionalDocuments()
		{
		}

		protected override void PopulateWareHouse()
		{
		}

		protected override void PopulateImporter()
		{
			decShipment.Importer = new DeclarationGoodsShipmentImporter
			{
				ID = new ImporterIdentificationIDType { Value = Importer?.ID }
			};
		}

		protected override void PopulateDeclarant()
		{
			decMessage.Declarant = new DeclarationDeclarant
			{
				ID = new DeclarantIdentificationIDType { Value = Declarant?.ID }
			};
		}

		protected override void PopulateAEOMutualRecognitionParties()
		{
		}

		protected override void PopulateAEOMutualRecognitionParties(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateTradeTerms()
		{
		}

		protected override void PopulateDutyTaxFee(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity)
		{
		}

		protected override void PopulateCustomsValuations()
		{
		}

		protected override void PopulateCustomsValuationFreightChargeAmount(DeclarationGoodsShipmentCustomsValuation customsValuation)
		{
		}

		protected override void PopulateCustomsValuations(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateInvoiceAmount()
		{
		}

		protected override void PopulateValuationAdjustmentAdditionCode(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateInvoiceLineItemChargeAmount(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity)
		{
			if (!giCommodity.InvoiceLineItemCharge.Currency.IsEmpty)
			{
				base.PopulateInvoiceLineItemChargeAmount(commodity, giCommodity);
			}
		}

		protected override void PopulateCurrencyExchanges()
		{
		}

		protected override void PopulateDutyRegimeCode(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
		}

		protected override void PopulateCustomsValueAmount(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateDestinationCountryCode(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateOrigins(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateLoadingLocationID(DeclarationGoodsShipmentConsignment consignment, IConsignment consignmentData)
		{
		}

		protected override void PopulatePresentationOffice()
		{
		}

		protected override void PopulateNetNetWeightMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure goodsMeasure, ICommodity giCommodity)
		{
		}

		protected override void PopulateClassification(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity)
		{
		}

		protected override void PopulateConsignmentTransportEquipment(DeclarationGoodsShipmentConsignment consignment)
		{
		}

		protected override void PopulateBorderTransportMeans()
		{
			decMessage.BorderTransportMeans = new DeclarationBorderTransportMeans
			{
				ModeCode = new BorderTransportMeansModeCodeType { Value = BorderTransportMeans?.ModeCode },
			};
		}

		protected override void PopulateObligationGuarantees()
		{
		}

		protected override void PopulateTransactionNatureCode()
		{
		}

		protected override void PopulateTransactionNatureCode(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateItemConsignor(IGovernmentAgencyGoodsItem goodsItem)
		{
			// Note: only require in header
		}

		protected override void PopulateStatisticalValue(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateDomesticDutyTaxParties()
		{
		}

		protected override void PopulateDomesticDutyTaxParties(IGovernmentAgencyGoodsItem goodsItem)
		{
		}
	}
}
