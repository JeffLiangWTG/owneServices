using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADDeclarationGoodsShipmentCommodityPreviousDocument : ICADMessageDeclarationGoodsShipmentCommodityPreviousDocument
{
	public CADDeclarationGoodsShipmentCommodityPreviousDocument(ZString lineNumeric, ZString typeCode)
	{
		this.lineNumeric = lineNumeric;
		this.typeCode = typeCode;
	}

	readonly ZString lineNumeric;
	readonly ZString typeCode;

	#region ICADMessageDeclarationGoodsShipmentCommodityPreviousDocument

	string ICADMessageDeclarationGoodsShipmentCommodityPreviousDocument.LineNumeric => lineNumeric;

	string ICADMessageDeclarationGoodsShipmentCommodityPreviousDocument.TypeCode => typeCode;

	#endregion
}
