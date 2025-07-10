using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseGroupedLinesForVarianceWrapperCollection : GenericWrapperCollection<WarehouseGroupedLinesForVarianceWrapper>
	{
		public WarehouseGroupedLinesForVarianceWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WarehouseGroupedLinesForVarianceWrapperCollection(WhsDocketLineCollection collection, BusinessObjectFactory factory)
			: base(factory)
		{
			if (collection != null)
			{
				AddLinesFrom(collection);
			}
		}

		void AddLinesFrom(IEnumerable docketLines)
		{
			var keyToWrapperPairs = new Dictionary<string, WarehouseGroupedLinesForVarianceWrapper>();
			foreach (WhsDocketLine line in docketLines)
			{
				var key = GetKey(line);
				WarehouseGroupedLinesForVarianceWrapper wrapper;
				if (keyToWrapperPairs.TryGetValue(key, out wrapper))
				{
					wrapper.IncrementTotalValues(line);
				}
				else
				{
					wrapper = new WarehouseGroupedLinesForVarianceWrapper(line, Factory);
					keyToWrapperPairs.Add(key, wrapper);
					Add(wrapper);
				}
			}
		}

		string GetKey(WhsDocketLine docketLine)
		{
			var key = new ZStringBuilder();
			key.Append(docketLine.ProductCode);
			key.Append(docketLine.WE_F3_NKPackType);
			key.Append(docketLine.SupplierPart?.OP_WeightUQ ?? ZString.Empty);
			key.Append(docketLine.WE_PartAttrib1);
			key.Append(docketLine.WE_PartAttrib2);
			key.Append(docketLine.WE_PartAttrib3);
			key.Append(docketLine.WE_PackingDate.ToShortDateString());
			key.Append(docketLine.WE_ExpiryDate.ToShortDateString());
			key.Append(docketLine.WE_CurrentInventoryStatus);
			return key.ToStringWithDelimiterBetweenAppends(",");
		}
	}
}
