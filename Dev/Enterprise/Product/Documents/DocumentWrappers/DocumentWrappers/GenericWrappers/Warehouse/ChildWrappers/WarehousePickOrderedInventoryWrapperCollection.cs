using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehousePickOrderedInventoryWrapperCollection : WarehouseGenericWrapperCollection<WarehousePickOrderedInventoryWrapper>
	{
		public WarehousePickOrderedInventoryWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WarehousePickOrderedInventoryWrapperCollection(WhsPickOrderedInventoryCollection collection, BusinessObjectFactory factory)
			: base(factory)
		{
			if (collection != null)
			{
				AddLinesFrom(collection);
			}
		}

		public WarehousePickOrderedInventoryWrapperCollection(IEnumerable<WarehousePickOrderedInventoryWrapper> orderedInventoryLines, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (var line in orderedInventoryLines)
			{
				Add(line);
			}
		}

		#region Implementation 

		void AddLinesFrom(WhsPickOrderedInventoryCollection orderedInventoryLines)
		{
			foreach (var line in orderedInventoryLines.Cast<WhsPickOrderedInventory>())
			{
				Add(new WarehousePickOrderedInventoryWrapper(line, Factory));
			}
		}

		#endregion
	}
}
