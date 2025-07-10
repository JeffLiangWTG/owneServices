using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class H5MessageBuilder : H1MessageBuilder
	{
		public H5MessageBuilder(CusEntryHeader cusEntryHeader, ErrorCollector errorCollector, string functionCode) : base(cusEntryHeader, errorCollector, functionCode)
		{
		}

		protected override void PopulateSubmitter(DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument result, IAdditionalDocument additionalDocument)
		{
		}

		protected override void PopulateEffectiveDateTime(DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument result, IAdditionalDocument additionalDocument)
		{
		}

		protected override void PopulateSeller()
		{
		}

		protected override void PopulateBuyer()
		{
		}

		protected override void PopulateDomesticDutyTaxParties()
		{
		}

		protected override void PopulateDomesticDutyTaxParties(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateLoadingLocationID(DeclarationGoodsShipmentConsignment consignment, IConsignment consignmentData)
		{
		}

		protected override void PopulateObligationGuarantees()
		{
		}

		protected override void PopulateQuotaOrderID(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
		}

		protected override void PopulateSeller(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateBuyer(IGovernmentAgencyGoodsItem goodsItem)
		{
		}
	}
}
