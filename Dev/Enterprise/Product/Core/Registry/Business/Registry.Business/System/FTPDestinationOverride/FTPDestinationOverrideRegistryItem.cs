using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class FTPDestinationOverrideRegistryItem : StronglyTypedRegistryItem<FTPDestinationOverrideInfo, FTPDestinationOverrideInfo>
	{
		public FTPDestinationOverrideRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, FTPDestinationOverrideInfo defaultValue)
			: base(new FTPDestinationOverrideRegistryItemImpl(name, category, caption, hint, storage, options, defaultValue))
		{ }

		public void SetValue(FTPDestinationOverrideInfo value)
		{
			this.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		class FTPDestinationOverrideRegistryItemImpl : RegistryItemImpl
		{
			public FTPDestinationOverrideRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, FTPDestinationOverrideInfo defaultValue)
				: base(name, category, caption, hint, new FTPDestinationOverrideInfoRegistryDataType(), storage, options, defaultValue)
			{ }
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.FTPDestinationOverrideRegistryItemEditor, Enterprise.Registry.GUI")]
	public class FTPDestinationOverrideInfoRegistryDataType : NonPersistentBusinessObjectRegistryDataType<FTPDestinationOverrideInfo> { }
}
