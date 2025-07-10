using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class AzureOpenIDConnectConfigurationRegistryItem : StronglyTypedRegistryItem<AzureOpenIDConnectConfigurationCollection>
	{
		public AzureOpenIDConnectConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new AzureOpenIDConnectConfigurationRegistryDataType(), storage, options))
		{
		}
	}
}
