using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehousePickNonPickedItemsWrapper : WarehousePickingSlipWrapper
	{
		#region Constructor

		public WarehousePickNonPickedItemsWrapper(WhsPick pick, BusinessObjectFactory factoryToWrap)
			: base(pick, factoryToWrap)
		{
		}

		#endregion

		#region Static Memebers

		public static WarehousePickNonPickedItemsWrapper New(WhsPick pick, BusinessObjectFactory factoryToWrap)
		{
			WarehousePickNonPickedItemsWrapper result = null;
			if (pick != null)
			{
				result = new WarehousePickNonPickedItemsWrapper(pick, factoryToWrap);
			}
			return result;
		}

		#endregion

		#region SecondaryHeadingCore

		protected override ZString SecondaryHeadingCore
		{
			get { return Res.GetString("f9a1d180-b81d-4a0e-b079-42aae887e6f3", "Pick Details"); }
		}

		#endregion

		#region DocumentTitle

		protected override ZString DocumentTitleCore
		{
			get { return Res.GetString("d2231122-3b5c-4d86-8c62-9d4f307b674e", "Non Picked Items"); }
		}

		#endregion

		#region GetNonPickedItems

		protected override WarehouseGenericWrapperCollection GetJobLines()
		{
			var result = new WarehousePickOrderedInventoryWrapperCollection(Factory);

			foreach (WhsPickOrderedInventory orderedInventory in Pick.OrderedInventories)
			{
				if (orderedInventory.PickLineQuantity == 0)
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
