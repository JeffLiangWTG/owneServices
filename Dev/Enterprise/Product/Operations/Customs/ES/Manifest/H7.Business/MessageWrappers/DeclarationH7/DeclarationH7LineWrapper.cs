using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Manifest.H7.Business;

public class DeclarationH7LineWrapper(AsycudaPackedItem item, ZInt numberOfPackages) : IDeclarationH7Line
{
	public ZInt LineNumber => item.SequenceNumber;

	public IH7Value Value => new LineValueWrapper(item);

	public ZString GoodsDescription => item.API_GoodsDescription;

	public ZString CommodityCode => item.API_Tariff.Length >= 8 ? item.API_Tariff.Left(8) : item.API_Tariff.Left(6);

	public ZDecimal GrossWeight =>
		Constants.Weight.Convert(item.API_GrossWeight, item.API_GrossWeightUQ, Constants.Weight.Kilograms);

	public ZDecimal ComplementaryUnitsQty => item.API_CustomsQty2;

	public ZInt NumberOfPackages => numberOfPackages;
}
