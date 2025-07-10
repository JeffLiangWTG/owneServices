using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADDeclarationGoodsShipmentCommodityClassification : ICADMessageDeclarationGoodsShipmentCommodityClassification
{
	public CADDeclarationGoodsShipmentCommodityClassification(ZString tariff, bool isClassification = true)
	{
		this.tariff = tariff;
		this.isClassification = isClassification;
	}

	readonly ZString tariff;
	readonly bool isClassification;

	#region ICADDeclarationGoodsShipmentCommodityClassification

	string ICADMessageDeclarationGoodsShipmentCommodityClassification.ID => tariff;

	string ICADMessageDeclarationGoodsShipmentCommodityClassification.BindingTariffReferenceID
	{
		get
		{
			if (isClassification)
			{
				return "10";
			}
			else
			{
				return "06";
			}
		}
	}

	#endregion
}
