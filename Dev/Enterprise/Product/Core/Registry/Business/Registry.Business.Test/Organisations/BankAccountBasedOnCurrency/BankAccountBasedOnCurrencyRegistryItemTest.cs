using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BankAccountBasedOnCurrencyRegistryItem))]
	sealed class BankAccountBasedOnCurrencyRegistryItemTest : StronglyTypedRegistryItemTestCase<BankAccountBasedOnCurrencyCollection>
	{
		protected override StronglyTypedRegistryItem<BankAccountBasedOnCurrencyCollection, BankAccountBasedOnCurrencyCollection> GetNewRegistryItem()
		{
			return new BankAccountBasedOnCurrencyRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
