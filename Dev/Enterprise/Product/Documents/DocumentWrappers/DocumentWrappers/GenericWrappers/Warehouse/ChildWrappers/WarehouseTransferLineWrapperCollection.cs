using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Warehouse
{
	public class WarehouseTransferLineWrapperCollection : WarehouseDocketLineWrapperCollection<WarehouseTransferLineWrapper>
	{
		public WarehouseTransferLineWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WarehouseTransferLineWrapperCollection(WhsTransferLineCollection collection, BusinessObjectFactory factory)
			: base(factory)
		{
			if (collection != null)
			{
				AddLinesFrom(collection);
			}
		}

		#region Implementation

		void AddLinesFrom(IEnumerable transferLine)
		{
			foreach (WhsTransferLine line in transferLine)
			{
				Add(new WarehouseTransferLineWrapper(line, Factory));
			}
		}

		#endregion
	}
}
