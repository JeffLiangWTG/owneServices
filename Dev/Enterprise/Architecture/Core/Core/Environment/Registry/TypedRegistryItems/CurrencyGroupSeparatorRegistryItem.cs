using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class CurrencyGroupSeparatorRegistryItem : StringRegistryItem, IRegistryItemWithOtherChangedItems
	{
		public CurrencyGroupSeparatorRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new CurrencyGroupSeparatorImpl(name, category, caption, hint, storage, options))
		{
		}

		class CurrencyGroupSeparatorImpl : RegionNumberFormatRegistryItemImpl
		{
			public CurrencyGroupSeparatorImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new RegionNumberFormatStringRegistryDataType(() => EnvProxy.Instance.Registry.RawRegistry.CurrencyDecimalSeparator), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				return GetOriginalCulture(companyPK).NumberFormat.CurrencyGroupSeparator;
			}
		}

		public IEnumerable<IRegistryItemInternals> OtherChangedItems { get; set; }
	}
}
