using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class NumberGroupSizesStringListRegistryItem : StringRegistryItem
	{
		public NumberGroupSizesStringListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new NumberGroupSizesStringListImpl(name, category, caption, hint, storage, options))
		{
		}

		class NumberGroupSizesStringListImpl : RegionNumberFormatRegistryItemImpl
		{
			public NumberGroupSizesStringListImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new NumberGroupSizesStringListRegistryDataType(), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				return GetDefaultGroupSizes(GetOriginalCulture(companyPK).NumberFormat.NumberGroupSizes);
			}
		}
	}
}
