using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehousePickOrderSummaryWrapper : WarehousePickingSlipWrapper
	{
		#region Constructor

		public WarehousePickOrderSummaryWrapper(WhsPick pick, BusinessObjectFactory factoryToWrap)
			: base(pick, factoryToWrap)
		{
		}

		#endregion

		#region Static Memebers

		public static WarehousePickOrderSummaryWrapper New(WhsPick pick, BusinessObjectFactory factoryToWrap)
		{
			WarehousePickOrderSummaryWrapper result = null;
			if (pick != null)
			{
				result = new WarehousePickOrderSummaryWrapper(pick, factoryToWrap);
			}
			return result;
		}

		#endregion

		#region DocumentTitle

		protected override ZString DocumentTitleCore
		{
			get { return Res.GetString("1bd033c1-f119-42e8-bb50-da4a70c3fd27", "Pick Order Summary"); }
		}

		#endregion

		#region SecondaryHeading

		protected override ZString SecondaryHeadingCore
		{
			get { return Res.GetString("538469fb-a05e-44e5-ab50-0cb4e21d58c7", "Pick Details"); }
		}

		#endregion

		#region GetJobLines

		protected override WarehouseGenericWrapperCollection GetJobLines()
		{
			var orderedInventories = Pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var wrappers = new List<WarehousePickOrderedInventoryWrapper>();

			WhsPickByBOMHelper.AddFetchHintsForIsPickByBOMKitPickLine(Factory, Pick.GetAllPickLines());
			foreach (var orderedInventory in orderedInventories)
			{
				var pickByBOMQty = orderedInventory.Owners.SelectMany(o => o.PickLines).Where(pl => pl.IsPickByBOMKitPickLine()).Sum(pl => pl.WZ_Units);
				var orderedQtyWithoutPickByBOM = orderedInventory.QuantityOrdered - pickByBOMQty;
				if (orderedQtyWithoutPickByBOM > 0)
				{
					var wrapper = new WarehousePickOrderedInventoryWrapper(orderedInventory, Factory, unitOrderedOverride: orderedQtyWithoutPickByBOM, unitsPickedOverride: orderedInventory.PickLineQuantity - pickByBOMQty);
					wrappers.Add(wrapper);
				}
			}

			return new WarehousePickOrderedInventoryWrapperCollection(wrappers, Factory);
		}

		#endregion

		#region ProductLinesCount

		protected override ZInt ProductLinesCountCore
		{
			get
			{
				return JobLines.Cast<WarehousePickOrderedInventoryWrapper>().DistinctBy(w => w.ProductCode)
					.Count(w => !string.IsNullOrEmpty(w.ProductCode));
			}
		}

		#endregion

		#region GetJobs

		protected override WarehouseJobGenericWrapperCollection GetJobs()
		{
			return new WarehousePickableDocketWrapperCollection(Pick.Orders, Factory);
		}

		#endregion

		#region Pick

		WhsPick Pick
		{
			get { return (WhsPick)WrappedObject; }
		}

		#endregion
	}
}
