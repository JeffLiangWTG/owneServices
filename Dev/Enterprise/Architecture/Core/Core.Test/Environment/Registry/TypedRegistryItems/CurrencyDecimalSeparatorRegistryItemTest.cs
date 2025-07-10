using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(CurrencyDecimalSeparatorRegistryItem))]
	public class CurrencyDecimalSeparatorRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new CurrencyDecimalSeparatorRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsValueMandatory);
		}
	}
}
