using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Common
{
	public class InventoryProvider : IInventory
	{
		public InventoryProvider(CusSupportingInfo info)
		{
			this.info = Argument.NotNull(info, nameof(info));
		}

		readonly CusSupportingInfo info;

		public string InventoryManagementNumber => info.CSI_Code;

		public decimal? MoveInQuantity => info.CSI_Quantity.Truncate();
	}
}
