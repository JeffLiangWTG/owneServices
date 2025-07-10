using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationWarehouseProvider : IDeclarationWarehouse
	{
		public DeclarationWarehouseProvider(WarehouseArea warehouseData)
		{
			this.warehouseData = Argument.NotNull(warehouseData, nameof(warehouseData));
		}

		readonly WarehouseArea warehouseData;

		public static DeclarationWarehouseProvider New(WarehouseArea warehouseData)
		{
			return warehouseData == null ? null : new DeclarationWarehouseProvider(warehouseData);
		}

		public string WarehouseCode => warehouseData.CY_Code;
	}
}
