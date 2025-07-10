using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class C21NMessageBuilder : C21IMessageBuilder
	{
		public C21NMessageBuilder(CusEntryHeader cusEntryHeader, ErrorCollector errorCollector, string functionCode) : base(cusEntryHeader, errorCollector, functionCode)
		{
		}

		protected override void PopulateWriteOff(DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument result, IAdditionalDocument additionalDocument) { }

		protected override void PopulateDecAdditionalDocuments() { }

		protected override void PopulateItemConsignor(IGovernmentAgencyGoodsItem goodsItem) { }

		protected override void PopulateExporter() { }

		protected override void PopulateImporter()
		{
			if (Importer != null && !Importer.ID.IsEmpty)
			{
				var importer = new DeclarationGoodsShipmentImporter
				{
					ID = new ImporterIdentificationIDType { Value = Importer.ID },
				};
				decShipment.Importer = importer;
			}
		}

		protected override void PopulateDeclarant()
		{
			if (Declarant != null && !Declarant.ID.IsEmpty)
			{
				var declarant = new DeclarationDeclarant
				{
					ID = new DeclarantIdentificationIDType { Value = Declarant.ID }
				};
				decMessage.Declarant = declarant;
			}
		}

		protected override void PopulateAgent()
		{
			var agent = Agent;
			if (agent != null && !agent.FunctionCode.IsEmpty && agent.Agent != null && agent.Agent is IOrganisation orgAgent && !orgAgent.ID.IsEmpty)
			{
				decMessage.Agent = new DeclarationAgent
				{
					ID = new AgentIdentificationIDType { Value = orgAgent.ID },
					FunctionCode = new AgentFunctionCodeType { Value = agent.FunctionCode }
				};
			}
		}

		protected override void PopulateInvoiceLineItemChargeAmount(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity) { }

		protected override void PopulateExportCountryID() { }

		protected override void PopulateExportCountryID(IGovernmentAgencyGoodsItem goodsItem) { }

		protected override void PopulateClassification(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity) { }

		protected override void PopulateGoodsMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity)
		{
			commodity.GoodsMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure();
			PopulateGrossMassMeasure(commodity.GoodsMeasure, giCommodity);
			PopulateNetNetWeightMeasure(commodity.GoodsMeasure, giCommodity);
		}

		protected override void PopulateNetNetWeightMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure goodsMeasure, ICommodity giCommodity)
		{
			if (giCommodity.NetWeight > 0)
			{
				goodsMeasure.NetNetWeightMeasure = new GoodsMeasureNetNetWeightMeasureType { Value = giCommodity.NetWeight };
			}
		}

		protected override void PopulateArrivalTransportMeans(DeclarationGoodsShipmentConsignment consignment, IConsignment consignmentData)
		{
			var arrivalTransportMeans = consignmentData.ArrivalTransportMeans;
			if (IsArrivalTransportMeansDetailsRequired)
			{
				consignment.ArrivalTransportMeans = new DeclarationGoodsShipmentConsignmentArrivalTransportMeans
				{
					ID = new ArrivalTransportMeansIdentificationIDType { Value = arrivalTransportMeans?.ID },
					IdentificationTypeCode = new ArrivalTransportMeansIdentificationTypeCodeType { Value = arrivalTransportMeans?.IdentificationTypeCode },
				};
			}
		}

		protected override void PopulateObligationGuarantees() { }

		protected override void PopulateTaxTypeCode(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee) { }

		protected override void PopulateSpecificTaxBaseQuantity(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee) { }

		protected override void PopulateTaxAssessedAmount(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee) { }

		protected override void PopulatePaymentAmount(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
			var paymentAmount = dutyTaxFee.Payment?.PaymentAmount?.Amount ?? ZDecimal.Zero;
			if (!paymentAmount.IsEmpty)
			{
				result.Payment.PaymentAmount = new PaymentPaymentAmountType
				{
					Value = paymentAmount,
					currencyID = dutyTaxFee.Payment?.PaymentAmount?.Currency ?? ZString.Empty
				};
			}
		}

		protected override void PopulateGrossMassMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure goodsMeasure, ICommodity giCommodity)
		{
			goodsMeasure.GrossMassMeasure = new GoodsMeasureGrossMassMeasureType { Value = giCommodity.GrossWeight };
		}
	}
}
