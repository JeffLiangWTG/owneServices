using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class H3MessageBuilder : H5MessageBuilder
	{
		public H3MessageBuilder(CusEntryHeader cusEntryHeader, ErrorCollector errorCollector, string functionCode) : base(cusEntryHeader, errorCollector, functionCode)
		{
		}

		protected override void PopulateValuationAdjustmentAdditionCode(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateNetNetWeightMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure goodsMeasure, ICommodity giCommodity)
		{
			if (GBCustomsDataRegistry.Instance.SendNetMassForH3Declarations.Value)
			{
				base.PopulateNetNetWeightMeasure(goodsMeasure, giCommodity);
			}
		}

		protected override void PopulateObligationGuarantees()
		{
			PopulateObligationGuaranteesBase();
		}

		protected override void PopulateLoadingLocationID(DeclarationGoodsShipmentConsignment consignment, IConsignment consignmentData)
		{
			PopulateLoadingLocationIDBase(consignment, consignmentData);
		}

		protected override void PopulateWriteOff(DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument result, IAdditionalDocument additionalDocument)
		{
		}
	}
}
