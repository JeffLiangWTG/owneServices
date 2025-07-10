using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureWrapper : IDeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure
	{
		DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureWrapper(ZDecimal quantity, ZString unitQty, ZString measureQualifier)
		{
			this.quantity = quantity;
			this.unitQty = unitQty;
			this.measureQualifier = measureQualifier;
		}

		public static IDeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure NewOrNull(ZDecimal quantity, ZString unitQty, ZString measureQualifier)
			=> quantity.IsEmpty || unitQty.IsEmpty || measureQualifier.IsEmpty
			? null
			: new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureWrapper(quantity, unitQty, measureQualifier);

		IDeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureDmExtensions IDeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure.DmExtensions => new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureDmExtensionsWrapper(measureQualifier);

		IQuantityType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure.TariffQuantity => QuantityTypeWrapper.NewOrNull(quantity.Round(3), unitQty);

		readonly ZDecimal quantity;
		readonly ZString unitQty;
		readonly ZString measureQualifier;
	}
}
