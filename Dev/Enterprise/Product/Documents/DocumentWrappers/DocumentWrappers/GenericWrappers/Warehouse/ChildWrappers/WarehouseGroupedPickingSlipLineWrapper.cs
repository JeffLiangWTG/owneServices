using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseGroupedPickingSlipLineWrapper : WarehousePickingSlipLineWrapper
	{
		public WarehouseGroupedPickingSlipLineWrapper(WhsPickLine pickingLineBO, BusinessObjectFactory factory,
			List<GroupedQuantityItem> groupedQuantity)
			: base(pickingLineBO, factory)
		{
			this.groupedQuantity = groupedQuantity;
			this.Units = groupedQuantity.Sum(x => x.Qty);
		}

		readonly List<GroupedQuantityItem> groupedQuantity;

		#region Properties

		protected override ZString UnitsGroupedUOMTypeCore => string.Join(System.Environment.NewLine, groupedQuantity.Select(x => x.UOMType));

		protected override ZString UnitsGroupedPackQtyCore
		{
			get
			{
				return string.Join(System.Environment.NewLine, groupedQuantity.Select(x => x.PackQty.ToString("#0.###")));
			}
		}

		protected override ZString UnitsGroupedPackTypeCore
		{
			get
			{
				return string.Join(System.Environment.NewLine, groupedQuantity.Select(x => x.PackType));
			}
		}

		protected override ZString UnitsGroupedQtyCore
		{
			get
			{
				return string.Join(System.Environment.NewLine, groupedQuantity.Select(x => x.Qty.ToString("#0.###")));
			}
		}

		protected override ZString UnitsGroupedStockKeepingUnitCore
		{
			get
			{
				return string.Join(System.Environment.NewLine, groupedQuantity.Select(x => x.StockKeepingUnit));
			}
		}

		#endregion
	}
}
