using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	public class McpIslCredentialsSettingCollectionRegistryItem : StronglyTypedRegistryItem<McpIslCredentialsSettingCollection>
	{
		public McpIslCredentialsSettingCollectionRegistryItem(string name, MultilingualString category, string caption, string hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, (NoResString)caption, (NoResString)hint, new McpIslCredentialsSettingRegistryDataType(), storage))
		{
		}

		public McpIslCredentialsSettingCollectionRegistryItem(string name, MultilingualString category, string caption, string hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, (NoResString)caption, (NoResString)hint, new McpIslCredentialsSettingRegistryDataType(), storage, options))
		{
		}
	}
}
