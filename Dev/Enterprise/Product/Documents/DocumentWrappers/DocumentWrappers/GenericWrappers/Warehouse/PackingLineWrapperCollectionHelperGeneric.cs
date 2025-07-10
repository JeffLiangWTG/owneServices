using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class PackingLineWrapperCollectionHelperGeneric : PackingSlipLineWrapperCollectionHelper<WarehousePackingSlipLineWrapper, WarehousePackingSlipLineWrapperCollection>
	{
		protected override WarehousePackingSlipLineWrapperCollection GetNewPackingSlipWrapperCollection(IEnumerable<KeyValuePair<WhsReleaseLine, WhsPickableDocketLine>> releaseLines, BusinessObjectFactory factory)
		{
			return new WarehousePackingSlipLineWrapperCollection(releaseLines, factory);
		}

		protected override WarehousePackingSlipLineWrapper GetNewPackingSlipWrapper(WhsReleaseLine releaseLine, WhsPickableDocketLine orderLine, BusinessObjectFactory factory)
		{
			return new WarehousePackingSlipLineWrapper(releaseLine, orderLine, factory);
		}
	}
}
