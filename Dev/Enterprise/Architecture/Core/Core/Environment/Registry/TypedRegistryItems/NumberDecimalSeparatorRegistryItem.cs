using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class NumberDecimalSeparatorRegistryItem : StringRegistryItem, IRegistryItemWithOtherChangedItems
	{
		public NumberDecimalSeparatorRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new NumberDecimalSeparatorImpl(name, category, caption, hint, storage, options))
		{
		}

		class NumberDecimalSeparatorImpl : RegionNumberFormatRegistryItemImpl
		{
			public NumberDecimalSeparatorImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new RegionNumberFormatStringRegistryDataType(() => EnvProxy.Instance.Registry.RawRegistry.NumberGroupSeparator), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				return GetOriginalCulture(companyPK).NumberFormat.NumberDecimalSeparator;
			}
		}

		public IEnumerable<IRegistryItemInternals> OtherChangedItems { get; set; }
	}
}
