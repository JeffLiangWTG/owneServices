using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsSADHeaderWarehouseIdentificationWrapper : IWarehouseIdentification
{
	public NctsSADHeaderWarehouseIdentificationWrapper(NctsDepartureMovementHeader movementHeader)
	{
		Argument.NotNull(movementHeader, nameof(movementHeader));
		warehouseCode = movementHeader.WarehouseCode;
	}
	readonly ZString warehouseCode;

	public ZString Type => warehouseCode.SubstringSafe(0, 1);

	public ZString Identification => warehouseCode.SubstringSafe(1, 6);

	public ZString CinIdentification => warehouseCode.SubstringSafe(7, 1);

	public ZString AuthorizingCountry => warehouseCode.SubstringSafe(8, 2);
}
