using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehousePackingSlipLineWrapperCollection : WarehouseDocketLineWrapperCollection<WarehousePackingSlipLineWrapper>, IPackingSlipWrapperCollection<WarehousePackingSlipLineWrapper>
	{
		#region Constructors

		public WarehousePackingSlipLineWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WarehousePackingSlipLineWrapperCollection(IEnumerable<KeyValuePair<WhsReleaseLine, WhsPickableDocketLine>> collection, BusinessObjectFactory factory)
			: base(factory)
		{
			if (collection != null)
			{
				AddLinesFrom(collection);
			}
		}

		#endregion

		#region Implementation

		void AddLinesFrom(IEnumerable<KeyValuePair<WhsReleaseLine, WhsPickableDocketLine>> packLines)
		{
			foreach (var line in packLines)
			{
				Add(new WarehousePackingSlipLineWrapper(line.Key, line.Value, Factory));
			}
		}

		#endregion

		// interfaces

		#region IPackingSlipWrapperCollection Members

		void IPackingSlipWrapperCollection<WarehousePackingSlipLineWrapper>.Add(WarehousePackingSlipLineWrapper packingSlipWrapper)
		{
			Add(packingSlipWrapper);
		}

		IEnumerable<WarehousePackingSlipLineWrapper> IPackingSlipWrapperCollection<WarehousePackingSlipLineWrapper>.Wrappers
		{
			get
			{
				foreach (WarehousePackingSlipLineWrapper wrapper in this)
				{
					yield return wrapper;
				}
			}
		}

		#endregion
	}
}
