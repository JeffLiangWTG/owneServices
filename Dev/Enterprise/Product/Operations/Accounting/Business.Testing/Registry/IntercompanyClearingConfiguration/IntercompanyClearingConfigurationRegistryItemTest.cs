using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(IntercompanyClearingConfigurationRegistryItem))]
	class IntercompanyClearingConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<IntercompanyClearingConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<IntercompanyClearingConfigurationCollection, IntercompanyClearingConfigurationCollection> GetNewRegistryItem()
		{
			return new IntercompanyClearingConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}
	}
}
