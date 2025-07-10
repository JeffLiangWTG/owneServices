using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Warehouse.ChildWrappers
{
	public class WarehouseWorkOrderLineWrapperCollection : WarehouseDocketLineWrapperCollection<WarehouseWorkOrderLineWrapper>
	{
		public WarehouseWorkOrderLineWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WarehouseWorkOrderLineWrapperCollection(WhsWorkOrderLineCollection collection, BusinessObjectFactory factory)
			: base(factory)
		{
			if (collection != null)
			{
				AddLinesFrom(collection);
			}
		}

		#region Implementation

		void AddLinesFrom(IEnumerable workOrderLines)
		{
			foreach (WhsWorkOrderLine line in workOrderLines)
			{
				Add(new WarehouseWorkOrderLineWrapper(line, Factory));
			}
		}

		#endregion
	}
}
