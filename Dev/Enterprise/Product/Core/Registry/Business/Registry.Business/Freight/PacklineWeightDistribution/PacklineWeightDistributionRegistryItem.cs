using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class PacklineWeightDistributionRegistryItem : StronglyTypedRegistryItem<PacklineWeightDistributionConfiguration>
	{
		public PacklineWeightDistributionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new PacklineWeightDistributionRegistryDataType(), storage))
		{
		}

		public PacklineWeightDistributionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, PacklineWeightDistributionConfiguration defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new PacklineWeightDistributionRegistryDataType(), storage, options, defaultValue))
		{
		}
	}
}
