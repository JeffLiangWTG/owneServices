using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(EnforceZeroBalanceDisbursementsRegistryItem))]
	class EnforceZeroBalanceDisbursementsRegistryItemTest : StronglyTypedRegistryItemTestCase<EnforceZeroBalanceDisbursementsConfiguration>
	{
		protected override StronglyTypedRegistryItem<EnforceZeroBalanceDisbursementsConfiguration, EnforceZeroBalanceDisbursementsConfiguration> GetNewRegistryItem()
		{
			return new EnforceZeroBalanceDisbursementsRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}
	}
}
