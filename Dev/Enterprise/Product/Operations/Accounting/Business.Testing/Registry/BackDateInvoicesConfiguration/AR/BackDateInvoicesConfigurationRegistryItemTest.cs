using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(BackDateInvoicesConfigurationRegistryItem))]
	class BackDateInvoicesConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<BackDateInvoicesConfiguration>
	{
		protected override StronglyTypedRegistryItem<BackDateInvoicesConfiguration, BackDateInvoicesConfiguration> GetNewRegistryItem()
		{
			return new BackDateInvoicesConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
