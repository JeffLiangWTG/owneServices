using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ElectronicProcessingChargeConfigurationRegistryItem))]
	public class ElectronicProcessingChargeConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<ElectronicProcessingChargeConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<ElectronicProcessingChargeConfigurationCollection, ElectronicProcessingChargeConfigurationCollection> GetNewRegistryItem()
		{
			return new ElectronicProcessingChargeConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new ElectronicProcessingChargeConfigurationCollection());
		}
	}
}
