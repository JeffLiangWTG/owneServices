using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class NumberGroupSeparatorRegistryItem : StringRegistryItem, IRegistryItemWithOtherChangedItems
	{
		public NumberGroupSeparatorRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new NumberGroupSeparatorImpl(name, category, caption, hint, storage, options))
		{
		}

		class NumberGroupSeparatorImpl : RegionNumberFormatRegistryItemImpl
		{
			public NumberGroupSeparatorImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new RegionNumberFormatStringRegistryDataType(() => EnvProxy.Instance.Registry.RawRegistry.NumberDecimalSeparator), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				return GetOriginalCulture(companyPK).NumberFormat.NumberGroupSeparator;
			}
		}

		public IEnumerable<IRegistryItemInternals> OtherChangedItems { get; set; }
	}
}
