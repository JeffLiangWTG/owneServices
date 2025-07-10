using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class EnableAddEditAndDeleteLogsItemCollectionRegistryItem : StronglyTypedRegistryItem<EnableAddEditAndDeleteLogsItemCollection>
	{
		public EnableAddEditAndDeleteLogsItemCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, EnableAddEditAndDeleteLogsItemCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new EnableAddEditAndDeleteLogsItemRegistryDataType(), storage, options, defaultValue))
		{
			EqualityComparer = Enumerable.Empty<EnableAddEditAndDeleteLogsItem>()
				.CreateComparerForElements((x, y) => string.Equals(x.Table, y.Table, StringComparison.OrdinalIgnoreCase), x => x.Table.GetHashCode());
		}

		protected override object GetValueWithoutFallbackCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var items = (EnableAddEditAndDeleteLogsItemCollection)base.GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK);

			var missingItems = DefaultValue.Except(items, EqualityComparer).ToArray();
			if (missingItems.Length > 0)
			{
				items.AddRange(missingItems);
			}

			return items;
		}

		readonly IEqualityComparer<EnableAddEditAndDeleteLogsItem> EqualityComparer;
	}
}
