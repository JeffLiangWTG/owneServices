using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(AutoCreateARAndAPOutstandingBalancesCurrencyAdjustmentsRegistryItem))]
	class AutoCreateARAndAPOutstandingBalancesCurrencyAdjustmentsRegistryItemTest : StronglyTypedRegistryItemTestCase<bool>
	{
		protected override StronglyTypedRegistryItem<bool, bool> GetNewRegistryItem()
		{
			return new AutoCreateARAndAPOutstandingBalancesCurrencyAdjustmentsRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, false);
		}
	}
}
