using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class FindWindowQueryCostsRegistryItem : StronglyTypedRegistryItem<FindWindowQueryCosts>
	{
		public FindWindowQueryCostsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, FindWindowQueryCosts defaultValue)
			: base(new FindWindowQueryCostsRegistryItemImpl(name, category, caption, hint, storage, defaultValue))
		{
		}

		class FindWindowQueryCostsRegistryItemImpl : RegistryItemImpl
		{
			public FindWindowQueryCostsRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, FindWindowQueryCosts defaultValue)
				: base(name, category, caption, hint, new FindWindowQueryCostsRegistryDataType(), storage, defaultValue)
			{
			}
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.FindWindowQueryCostsRegistryItemEditor, Enterprise.Registry.GUI")]
	public class FindWindowQueryCostsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<FindWindowQueryCosts>
	{
	}
}
