using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseAdjustmentLineWrapperCollection : WarehouseDocketLineWrapperCollection<WarehouseAdjustmentLineWrapper>
	{
		#region Constructors

		public WarehouseAdjustmentLineWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WarehouseAdjustmentLineWrapperCollection(WhsAdjustmentLineCollection collection, BusinessObjectFactory factory)
			: base(factory)
		{
			if (collection != null)
			{
				AddLinesFrom(collection);
			}
		}

		public static WarehouseAdjustmentLineWrapperCollection New(WhsAdjustment adjustment, BusinessObjectFactory factory)
		{
			if (adjustment != null)
			{
				return new WarehouseAdjustmentLineWrapperCollection(adjustment.Lines, factory);
			}
			else
			{
				return new WarehouseAdjustmentLineWrapperCollection(factory);
			}
		}

		#endregion

		#region Implementation

		void AddLinesFrom(IEnumerable adjustmentLines)
		{
			foreach (WhsAdjustmentLine line in adjustmentLines)
			{
				Add(new WarehouseAdjustmentLineWrapper(line, Factory));
			}
		}

		#endregion
	}
}
