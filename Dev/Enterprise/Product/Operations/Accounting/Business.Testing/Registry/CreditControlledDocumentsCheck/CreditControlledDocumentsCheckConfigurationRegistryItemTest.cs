using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CreditControlledDocumentsCheckConfigurationRegistryItem))]
	class CreditControlledDocumentsCheckConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<CreditControlledDocumentsCheckConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<CreditControlledDocumentsCheckConfigurationCollection, CreditControlledDocumentsCheckConfigurationCollection> GetNewRegistryItem()
		{
			return new CreditControlledDocumentsCheckConfigurationRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System);
		}
	}
}
