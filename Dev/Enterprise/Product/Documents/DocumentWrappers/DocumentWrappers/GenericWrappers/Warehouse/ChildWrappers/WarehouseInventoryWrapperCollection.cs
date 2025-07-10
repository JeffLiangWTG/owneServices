using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseInventoryWrapperCollection : WarehouseGenericWrapperCollection<WarehouseInventoryWrapper>
	{
		public WarehouseInventoryWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WarehouseInventoryWrapperCollection(WhsInventoryViewCollection collection, BusinessObjectFactory factory)
			: base(factory)
		{
			if (collection != null)
			{
				AddLinesFrom(collection);
			}
		}

		#region Implementation

		void AddLinesFrom(IEnumerable inventoryLines)
		{
			foreach (WhsInventoryView line in inventoryLines)
			{
				Add(new WarehouseInventoryWrapper(line.InDocketLine, Factory));
			}
		}

		#endregion
	}
}
