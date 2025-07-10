using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ZeroAmountTaxTypesDescriptionsRegistryItem))]
	class ZeroAmountTaxTypesDescriptionsRegistryItemTest : StronglyTypedRegistryItemTestCase<ZeroAmountTaxTypesDescriptionsCollection>
	{
		protected override StronglyTypedRegistryItem<ZeroAmountTaxTypesDescriptionsCollection, ZeroAmountTaxTypesDescriptionsCollection> GetNewRegistryItem()
		{
			return new ZeroAmountTaxTypesDescriptionsRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.Company, new ZeroAmountTaxTypesDescriptionsCollection());
		}
	}
}
