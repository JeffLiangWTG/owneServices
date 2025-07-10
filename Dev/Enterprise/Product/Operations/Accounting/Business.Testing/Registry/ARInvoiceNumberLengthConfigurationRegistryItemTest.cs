using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ARInvoiceNumberLengthConfigurationRegistryItem))]
	class ARInvoiceNumberLengthConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<int>
	{
		protected override StronglyTypedRegistryItem<int, int> GetNewRegistryItem()
		{
			return new ARInvoiceNumberLengthConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default, 8, 5, 8);
		}
	}
}
