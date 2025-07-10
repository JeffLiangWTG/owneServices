using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.H7.Business;

namespace Enterprise.Customs.GB.H7.Messaging
{
	public class C21BMessageBuilder : CDS.Messaging.MessageBuilders.C21BMessageBuilder
	{
		public C21BMessageBuilder(BusinessObject sendingObject, ErrorCollector errorCollector, string functionCode) : base(sendingObject, errorCollector, functionCode)
		{
			this.bill = (AsycudaBill)((MessageSendingObject)messagingParent).Bill;
		}

		readonly AsycudaBill bill;

		protected override IDeclaration GetCdsDeclarationFromEntry()
		{
			return new GbCDSH7ImportDeclarationWrapper((MessageSendingObject)messagingParent);
		}

		protected override void PopulateConsignmentContainerCode(DeclarationGoodsShipmentConsignment consignment)
		{
			consignment.ContainerCode = new ConsignmentContainerCodeType { Value = bill.ContainerNumber.IsEmpty ? "0" : "1" };
		}

		protected override void PopulateConsignmentTransportEquipment(DeclarationGoodsShipmentConsignment consignment)
		{
			consignment.TransportEquipment = bill.ContainerNumber.IsEmpty ? null :
			[
				new()
				{
					SequenceNumeric = 1,
					ID = new TransportEquipmentIdentificationIDType { Value = bill.ContainerNumber },
				}
			];
		}

		protected override void PopulateNetNetWeightMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure goodsMeasure, ICommodity giCommodity)
		{
			if (giCommodity.NetWeight != default)
			{
				goodsMeasure.NetNetWeightMeasure = new GoodsMeasureNetNetWeightMeasureType { Value = giCommodity.NetWeight };
			}
		}

		protected override bool IsArrivalTransportMeansDetailsRequiredCore => true;

		protected override void PopulateExportCountryID(IGovernmentAgencyGoodsItem goodsItem)
		{
			var itemExportCountryID = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemExportCountry();
			if (!ExportCountryID.IsEmpty)
			{
				itemExportCountryID.ID = new ExportCountryCountryCodeType { Value = ExportCountryID };
			}

			decGoodsItem.ExportCountry = itemExportCountryID;
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
	}
}
