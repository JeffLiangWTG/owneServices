using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class BoleroEBLForOrganisationConfigurationRegistryItem : StronglyTypedRegistryItem<BoleroEBLForOrganisationConfiguration>
	{
		public BoleroEBLForOrganisationConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, BoleroEBLForOrganisationConfiguration defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new BoleroEBLForOrganisationConfigurationRegistryDataType(), storage, options, defaultValue))
		{
		}
	}
}
