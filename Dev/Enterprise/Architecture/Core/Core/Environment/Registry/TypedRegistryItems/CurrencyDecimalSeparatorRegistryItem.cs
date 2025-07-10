using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class CurrencyDecimalSeparatorRegistryItem : StringRegistryItem, IRegistryItemWithOtherChangedItems
	{
		public CurrencyDecimalSeparatorRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new CurrencyDecimalSeparatorImpl(name, category, caption, hint, storage, options))
		{
		}

		class CurrencyDecimalSeparatorImpl : RegionNumberFormatRegistryItemImpl
		{
			public CurrencyDecimalSeparatorImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new RegionNumberFormatStringRegistryDataType(() => EnvProxy.Instance.Registry.RawRegistry.CurrencyGroupSeparator), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				return GetOriginalCulture(companyPK).NumberFormat.CurrencyDecimalSeparator;
			}
		}

		public IEnumerable<IRegistryItemInternals> OtherChangedItems { get; set; }
	}
}
