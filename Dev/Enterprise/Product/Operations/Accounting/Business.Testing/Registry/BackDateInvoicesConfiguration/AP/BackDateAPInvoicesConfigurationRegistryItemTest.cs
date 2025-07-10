using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(BackDateAPInvoicesConfigurationRegistryItem))]
	class BackDateAPInvoicesConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<BackDateAPInvoicesConfiguration>
	{
		protected override StronglyTypedRegistryItem<BackDateAPInvoicesConfiguration, BackDateAPInvoicesConfiguration> GetNewRegistryItem()
		{
			return new BackDateAPInvoicesConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
