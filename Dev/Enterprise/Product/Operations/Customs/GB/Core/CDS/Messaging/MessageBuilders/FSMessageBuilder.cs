using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class FSMessageBuilder : MessageBuilder
	{
		public FSMessageBuilder(CusEntryHeader cusEntryHeader, ErrorCollector errorCollector, string functionCodeNewAmendDelete) : base(cusEntryHeader, errorCollector, functionCodeNewAmendDelete)
		{
		}

		protected override void PopulatePreviousDocuments(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateHeaderAddionalDocuments()
		{
		}

		protected override void PopulateAdditionalDocuments(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateDecAdditionalDocuments()
		{
		}

		protected override void PopulateWareHouse()
		{
		}

		protected override void PopulateExporter()
		{
		}

		protected override void PopulateItemConsignor(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateImporter()
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
			if (agent != null)
			{
				var orgAgent = agent?.Agent;
				var hasEORI = !orgAgent?.ID.IsEmpty ?? false;
				var hasFunctionCode = !agent.FunctionCode.IsEmpty;

				if (hasEORI)
				{
					decMessage.Agent = new DeclarationAgent
					{
						ID = new AgentIdentificationIDType { Value = orgAgent.ID }
					};
				}

				if (hasFunctionCode)
				{
					var decMessageAgent = decMessage?.Agent ?? (decMessage.Agent = new DeclarationAgent());
					decMessageAgent.FunctionCode = new AgentFunctionCodeType { Value = agent.FunctionCode };
				}
			}
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

		protected override void PopulateDomesticDutyTaxParties()
		{
		}

		protected override void PopulateDomesticDutyTaxParties(IGovernmentAgencyGoodsItem goodsItem)
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

		protected override void PopulateCustomsValuations(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateInvoiceAmount()
		{
		}

		protected override void PopulateValuationAdjustmentAdditionCode(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateCustomsValueAmount(IGovernmentAgencyGoodsItem goodsItemWrapper)
		{
		}

		protected override void PopulateCustomsValuationFreightChargeAmount(DeclarationGoodsShipmentCustomsValuation customsValuation)
		{
		}

		protected override void PopulateDestination()
		{
		}

		protected override void PopulateDestinationCountryCode(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateExportCountryID()
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

		protected override void PopulatePackagings(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateCommodity(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateTotalPackageQuantity()
		{
		}

		protected override void PopulateConsignmentContainerCode(DeclarationGoodsShipmentConsignment consignment)
		{
		}

		protected override void PopulateBorderTransportMeans()
		{
		}

		protected override void PopulateArrivalTransportMeans(DeclarationGoodsShipmentConsignment consignment, IConsignment consignmentData)
		{
		}

		protected override void PopulateConsignmentTransportEquipment(DeclarationGoodsShipmentConsignment consignment)
		{
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
