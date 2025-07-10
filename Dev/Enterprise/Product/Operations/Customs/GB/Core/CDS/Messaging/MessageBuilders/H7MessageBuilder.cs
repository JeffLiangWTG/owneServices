using System;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class H7MessageBuilder : MessageBuilder
	{
		public H7MessageBuilder(BusinessObject messagingParent, EU.Business.ErrorCollector errorCollector, string functionCode)
			: base(messagingParent, errorCollector, functionCode)
		{
		}

		protected override void PopulateTotalPackageQuantity()
		{
			if (TotalPackageQuantity > 0 && IsMovementThroughAnInventoryLinkingLocation())
			{
				decMessage.TotalPackageQuantity = new DeclarationTotalPackageQuantityType
				{
					Value = TotalPackageQuantity
				};
			}
		}

		protected override void PopulateDecAdditionalDocuments()
		{
		}

		protected override void PopulateWareHouse()
		{
		}

		protected override void PopulateAEOMutualRecognitionParties()
		{
		}

		protected override void PopulateAEOMutualRecognitionParties(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateAuthorisationHolders()
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
			decShipment.CustomsValuation = new DeclarationGoodsShipmentCustomsValuation();
			PopulateCustomsValuationFreightChargeAmount(decShipment.CustomsValuation);
		}

		protected override void PopulateCustomsValuationFreightChargeAmount(DeclarationGoodsShipmentCustomsValuation customsValuation)
		{
			var freightCharges = Wrapper.FreightChargeAmount;
			customsValuation.FreightChargeAmount = new CustomsValuationFreightChargeAmountType { currencyID = freightCharges.Currency, Value = freightCharges.Amount };
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
		}

		protected override void PopulateCurrencyExchanges()
		{
		}

		protected override void PopulateDutyRegimeCode(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
		}

		protected override void PopulateCustomsValueAmount(IGovernmentAgencyGoodsItem goodsItem)
		{
			const string requiredCurrencyCode = Core.Constants.CurrencyCodes.UnitedKingdom;
			var amountAndCurrency = (goodsItem as ICommodity)?.InvoiceLineItemCharge;
			if (amountAndCurrency != null)
			{
				var currency = RefCurrency.LoadFromCurrencyCode(cusEntryHeader.Factory, amountAndCurrency.Currency);
				var money = new Money(amountAndCurrency.Amount, currency);
				if (money.Currency?.Code != requiredCurrencyCode)
				{
					var requiredCurrency = RefCurrency.LoadFromCurrencyCode(cusEntryHeader.Factory, requiredCurrencyCode);
					money = ((ICurrencyConverterProvider)cusEntryHeader.Declaration).CurrencyConverter.ConvertRounded(money, requiredCurrency);
				}
				decGoodsItem.CustomsValueAmount = new GovernmentAgencyGoodsItemCustomsValueAmountType { currencyID = money.Currency?.Code, Value = money.Amount };
			}
		}

		protected override void PopulateDestinationCountryCode(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateExportCountryID(IGovernmentAgencyGoodsItem goodsItem)
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

		protected override void PopulateSupervisingOffice()
		{
		}

		protected override void PopulateNetNetWeightMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure goodsMeasure, ICommodity giCommodity)
		{
		}

		protected override void PopulatePackagings(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateClassification(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity)
		{
			var classificatonsWithoutIdentificationTypeCodeTRC = giCommodity.Classifications.Where(x => x.TypeCode != Constants.Classification.IdentificationTypeCodes.TRC);
			commodity.Classification = classificatonsWithoutIdentificationTypeCodeTRC?.Select(classification => new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification
			{
				ID = new ClassificationIdentificationIDType { Value = classification.ID },
				IdentificationTypeCode = new ClassificationIdentificationTypeCodeType { Value = classification.TypeCode }
			}).ToArray() ?? Array.Empty<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification>();
		}

		protected override void PopulateConsignmentContainerCode(DeclarationGoodsShipmentConsignment consignment)
		{
		}

		protected override void PopulateArrivalTransportMeans(DeclarationGoodsShipmentConsignment consignment, IConsignment consignmentData)
		{
		}

		protected override void PopulateConsignmentTransportEquipment(DeclarationGoodsShipmentConsignment consignment)
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

		protected override void PopulateObligationGuarantees()
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
	}
}
