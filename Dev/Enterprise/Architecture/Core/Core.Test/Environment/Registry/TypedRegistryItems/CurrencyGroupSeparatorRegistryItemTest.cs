using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(CurrencyGroupSeparatorRegistryItem))]
	public class CurrencyGroupSeparatorRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new CurrencyGroupSeparatorRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsValueMandatory);
		}
	}
}
