using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class PickingLineWrapperCollectionHelperGeneric : PickingLineWrapperCollectionHelperBase
	{
		public PickingLineWrapperCollectionHelperGeneric(WhsPick pick, BusinessObjectFactory factory, bool isPickByBiggestEnabled)
			: base(pick, factory, isPickByBiggestEnabled)
		{ }

		protected override IPickingSlipLineWrapperCollection GetRolledUpLines(WhsPickLine[] pickLines)
		{
			pick.UpdateIsLocationEmptyAfterFinalisingPickOnPickLines(pickLines);
			return new WarehousePickingSlipLineWrapperCollection(Factory);
		}

		protected override IPickingSlipLineWrapper GetDocPickLine(WhsPickLine pickLine)
		{
			return WarehousePickingSlipLineWrapper.New(pickLine, Factory);
		}

		protected override IPickingSlipLineWrapperCollection SplitItems(IPickingSlipLineWrapperCollection rolledUpLines)
		{
			return (isPickByBiggestEnabled) ?
				SplitItemsByPackType(rolledUpLines as WarehousePickingSlipLineWrapperCollection) :
				rolledUpLines;
		}

		WarehousePickingSlipLineWrapperCollection SplitItemsByPackType(WarehousePickingSlipLineWrapperCollection items)
		{
			var result = new WarehousePickingSlipLineWrapperCollection(Factory);
			var conversionTables = new Dictionary<OrgSupplierPart, ConversionsToSKUTable>();
			foreach (WarehousePickingSlipLineWrapper pickingSlipLineWrapper in items)
			{
				var pickLine = pickingSlipLineWrapper.WrappedObject as WhsPickLine;
				if (pickLine != null)
				{
					ConversionsToSKUTable table;
					var part = pickLine.SupplierPart;
					if (!conversionTables.TryGetValue(part, out table))
					{
						conversionTables[part] = table = new ConversionsToSKUTable(part);
					}

					var grouped = BiggestPackTypeGrouper.GetGroups(part, pickingSlipLineWrapper.Units, table).ToList();
					result.Add(new WarehouseGroupedPickingSlipLineWrapper(pickLine, Factory, grouped));
				}
			}
			return result;
		}
	}
}
