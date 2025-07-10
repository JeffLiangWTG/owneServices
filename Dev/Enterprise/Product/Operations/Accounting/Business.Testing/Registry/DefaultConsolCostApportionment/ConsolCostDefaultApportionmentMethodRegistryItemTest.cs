using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ConsolCostDefaultApportionmentMethodRegistryItem))]
	class ConsolCostDefaultApportionmentMethodRegistryItemTest : StronglyTypedRegistryItemTestCase<ConsolCostDefaultApportionmentMethodConfiguration>
	{
		protected override StronglyTypedRegistryItem<ConsolCostDefaultApportionmentMethodConfiguration, ConsolCostDefaultApportionmentMethodConfiguration> GetNewRegistryItem()
		{
			return new ConsolCostDefaultApportionmentMethodRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
