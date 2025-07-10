using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Warehouse.ChildWrappers
{
	public class WarehouseStocktakeLineWrapperCollection : WarehouseGenericWrapperCollection<WarehouseStocktakeLineWrapper>
	{
		public WarehouseStocktakeLineWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WarehouseStocktakeLineWrapperCollection(WhsStocktakeLineCollection collection, BusinessObjectFactory factory)
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
			foreach (WhsStocktakeLine line in inventoryLines)
			{
				Add(new WarehouseStocktakeLineWrapper(line, Factory));
			}
		}

		#endregion
	}
}
