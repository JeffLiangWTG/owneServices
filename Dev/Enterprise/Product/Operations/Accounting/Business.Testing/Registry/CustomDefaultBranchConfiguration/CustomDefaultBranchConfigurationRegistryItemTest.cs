using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CustomDefaultBranchConfigurationRegistryItem))]
	class CustomDefaultBranchConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<CustomDefaultBranchConfiguration>
	{
		protected override StronglyTypedRegistryItem<CustomDefaultBranchConfiguration, CustomDefaultBranchConfiguration> GetNewRegistryItem()
		{
			return new CustomDefaultBranchConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default);
		}
	}
}
