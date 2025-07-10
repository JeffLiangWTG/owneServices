using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class PackingLineWrapperCollectionHelperLegacy : PackingSlipLineWrapperCollectionHelper<DocWhsPackingSlipLine, DocWhsPackingSlipLineCollection>
	{
		protected override DocWhsPackingSlipLineCollection GetNewPackingSlipWrapperCollection(IEnumerable<KeyValuePair<WhsReleaseLine, WhsPickableDocketLine>> releaseLines, BusinessObjectFactory factory)
		{
			return new DocWhsPackingSlipLineCollection(releaseLines, factory);
		}

		protected override DocWhsPackingSlipLine GetNewPackingSlipWrapper(WhsReleaseLine releaseLine, WhsPickableDocketLine orderLine, BusinessObjectFactory factory)
		{
			return DocWhsPackingSlipLine.New(releaseLine, orderLine, factory);
		}
	}
}
