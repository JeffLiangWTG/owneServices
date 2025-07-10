using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class GuidArrayRegistryItem : StronglyTypedRegistryItem<Guid[]>
	{
		public GuidArrayRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, Array.Empty<Guid>())
		{
		}

		public GuidArrayRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, Guid[] defaultValue)
			: this(name, category, caption, hint, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public GuidArrayRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, Guid[] defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new GuidArrayRegistryDataType(defaultValue), null, storage, options, defaultValue))
		{
		}
	}
}
