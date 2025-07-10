using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(GenericChargeConfigurationRegistryItem))]
	public class GenericChargeConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<GenericChargeConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<GenericChargeConfigurationCollection, GenericChargeConfigurationCollection> GetNewRegistryItem()
		{
			return new GenericChargeConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}
	}
}
