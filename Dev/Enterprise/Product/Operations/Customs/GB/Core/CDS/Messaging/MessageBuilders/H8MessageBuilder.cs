using System;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class H8MessageBuilder : MessageBuilder
	{
		public H8MessageBuilder(CusEntryHeader cusEntryHeader, ErrorCollector errorCollector, string functionCode)
			: base(cusEntryHeader, errorCollector, functionCode)
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

		protected override void PopulateAcceptanceDateTime()
		{
		}

		protected override void PopulateDeclarant()
		{
			decMessage.Declarant = new DeclarationDeclarant
			{
				ID = new DeclarantIdentificationIDType { Value = Declarant?.ID }
			};
		}

		protected override void PopulateAgent()
		{
			var agent = Agent;
			if (agent != null && agent.Agent != null && agent.Agent is IOrganisation orgAgent && !orgAgent.ID.IsEmpty)
			{
				decMessage.Agent = new DeclarationAgent
				{
					ID = new AgentIdentificationIDType { Value = orgAgent.ID },
				};
			}
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
			decShipment.CustomsValuation = new DeclarationGoodsShipmentCustomsValuation();
			PopulateCustomsValuationFreightChargeAmount(decShipment.CustomsValuation);
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

		protected override void PopulateCurrencyExchanges()
		{
		}

		protected override void PopulateDutyRegimeCode(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
		}

		protected override void PopulateDestinationCountryCode(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateOrigins(IGovernmentAgencyGoodsItem goodsItem)
		{
			decGoodsItem.Origin = goodsItem.Origins?.Select(origin => new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin
			{
				CountryCode = new OriginCountryCodeType { Value = origin.CountryCode },
				TypeCode = new OriginTypeCodeType { Value = Constants.OriginTypeCodes.NonPreferential }
			}).ToArray() ?? Array.Empty<DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin>();
		}

		protected override void PopulateLoadingLocationID(DeclarationGoodsShipmentConsignment consignment, IConsignment consignmentData)
		{
		}

		protected override void PopulatePresentationOffice()
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

		protected override IOrganisation Importer
		{
			get
			{
				var declaration = cusEntryHeader.Declaration;
				var importer = declaration.ImporterDocumentaryAddress.Organisation;
				return !CDSExtensions.IsUnmatchedOrgHeader(importer)
					? OrganisationWrapper.New(declaration.ImporterDocumentaryAddress) : null;
			}
		}
	}
}
