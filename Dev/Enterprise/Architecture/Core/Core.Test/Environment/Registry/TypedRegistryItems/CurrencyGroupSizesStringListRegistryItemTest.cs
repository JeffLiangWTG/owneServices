using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(CurrencyGroupSizesStringListRegistryItem))]
	public class CurrencyGroupSizesStringListRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new CurrencyGroupSizesStringListRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}
	}
}
