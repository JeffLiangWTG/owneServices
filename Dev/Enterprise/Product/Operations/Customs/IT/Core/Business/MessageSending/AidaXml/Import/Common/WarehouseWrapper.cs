using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

sealed class WarehouseWrapper : IWarehouse
{
	public WarehouseWrapper(ZString warehouseType, ZString warehouseID)
	{
		this.warehouseType = Argument.NotNullOrEmpty(warehouseType, nameof(warehouseType));
		this.warehouseID = Argument.NotNullOrEmpty(warehouseID, nameof(warehouseID));
	}

	readonly ZString warehouseID;
	readonly ZString warehouseType;

	string IWarehouse.IdentificationNumber => warehouseID;

	string IWarehouse.WarehouseType => warehouseType;
}
