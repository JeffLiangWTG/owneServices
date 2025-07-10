using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class CurrencyGroupSizesStringListRegistryItem : StringRegistryItem
	{
		public CurrencyGroupSizesStringListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new CurrencyGroupSizesStringListImpl(name, category, caption, hint, storage, options))
		{
		}

		class CurrencyGroupSizesStringListImpl : RegionNumberFormatRegistryItemImpl
		{
			public CurrencyGroupSizesStringListImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new NumberGroupSizesStringListRegistryDataType(), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				return GetDefaultGroupSizes(GetOriginalCulture(companyPK).NumberFormat.CurrencyGroupSizes);
			}
		}
	}
}
