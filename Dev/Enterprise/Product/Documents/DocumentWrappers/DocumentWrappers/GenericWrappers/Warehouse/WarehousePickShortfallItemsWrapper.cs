using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehousePickShortfallItemsWrapper : WarehousePickingSlipWrapper
	{
		#region Constructor

		public WarehousePickShortfallItemsWrapper(WhsPick pick, BusinessObjectFactory factoryToWrap)
			: base(pick, factoryToWrap)
		{
		}

		#endregion

		#region Static Memebers

		public static WarehousePickShortfallItemsWrapper New(WhsPick pick, BusinessObjectFactory factoryToWrap)
		{
			WarehousePickShortfallItemsWrapper result = null;
			if (pick != null)
			{
				result = new WarehousePickShortfallItemsWrapper(pick, factoryToWrap);
			}
			return result;
		}

		#endregion

		#region SecondaryHeadingCore

		protected override ZString SecondaryHeadingCore
		{
			get { return Res.GetString("b5289f6c-2c4b-46f2-b762-15817845f5ce", "Pick Details"); }
		}

		#endregion

		#region DocumentTitle

		protected override ZString DocumentTitleCore
		{
			get { return Res.GetString("73ef2538-404a-4134-9358-887acb6a6a50", "Shortfall Items"); }
		}

		#endregion

		#region GetShortfallItems

		protected override WarehouseGenericWrapperCollection GetJobLines()
		{
			var result = new WarehousePickOrderedInventoryWrapperCollection(Factory);

			foreach (WhsPickOrderedInventory orderedInventory in Pick.OrderedInventories)
			{
				if (orderedInventory.QuantityShort != 0)
				{
					result.Add(new WarehousePickOrderedInventoryWrapper(orderedInventory, Factory));
				}
			}

			return result;
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
