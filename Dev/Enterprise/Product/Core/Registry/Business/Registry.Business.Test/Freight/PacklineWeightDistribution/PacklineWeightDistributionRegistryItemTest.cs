using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.Freight.PacklineWeightDistribution
{
	[TestedType(typeof(PacklineWeightDistributionRegistryItem))]
	sealed class PacklineWeightDistributionRegistryItemTest : StronglyTypedRegistryItemTestCase<PacklineWeightDistributionConfiguration>
	{
		protected override StronglyTypedRegistryItem<PacklineWeightDistributionConfiguration, PacklineWeightDistributionConfiguration> GetNewRegistryItem()
		{
			return new PacklineWeightDistributionRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new PacklineWeightDistributionConfiguration() { EnablePacklineWeightDistribution = true, EnableActualWeightDistribution = true, EnableVolumetricWeightDistribution = true });
		}
	}
}
