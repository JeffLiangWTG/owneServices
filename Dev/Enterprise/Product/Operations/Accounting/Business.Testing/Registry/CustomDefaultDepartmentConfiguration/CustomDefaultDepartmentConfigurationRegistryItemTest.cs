using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CustomDefaultDepartmentConfigurationRegistryItem))]
	class CustomDefaultDepartmentConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<CustomDefaultDepartmentConfiguration>
	{
		protected override StronglyTypedRegistryItem<CustomDefaultDepartmentConfiguration, CustomDefaultDepartmentConfiguration> GetNewRegistryItem()
		{
			return new CustomDefaultDepartmentConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default);
		}
	}
}
