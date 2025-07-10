using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class C1MessageBuilder : ExportsMessageBuilder
	{
		public C1MessageBuilder(CusEntryHeader cusEntryHeader, ErrorCollector errorCollector, string functionCodeNewAmendDelete)
			: base(cusEntryHeader, errorCollector, functionCodeNewAmendDelete)
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

		protected override void PopulateGrossMassMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure goodsMeasure, ICommodity giCommodity)
		{
			if (giCommodity == null)
			{
				return;
			}

			var grossWeight = giCommodity.GrossWeight;
			goodsMeasure.GrossMassMeasure = new GoodsMeasureGrossMassMeasureType { Value = grossWeight };
		}

		protected override void PopulateNetNetWeightMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure goodsMeasure, ICommodity giCommodity)
		{
			if (giCommodity == null)
			{
				return;
			}

			var netWeight = giCommodity.NetWeight;
			goodsMeasure.NetNetWeightMeasure = new GoodsMeasureNetNetWeightMeasureType { Value = netWeight };
		}
	}
}
