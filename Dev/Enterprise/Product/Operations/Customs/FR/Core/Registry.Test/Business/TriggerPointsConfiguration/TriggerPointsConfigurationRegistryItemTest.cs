using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Registry.Testing
{
	[TestedType(typeof(TriggerPointsConfigurationRegistryItem))]
	class TriggerPointsConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<TriggerPointsConfiguration>
	{
		protected override ZArchitecture.Environment.StronglyTypedRegistryItem<TriggerPointsConfiguration, TriggerPointsConfiguration> GetNewRegistryItem()
		{
			return new TriggerPointsConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}
	}
}
