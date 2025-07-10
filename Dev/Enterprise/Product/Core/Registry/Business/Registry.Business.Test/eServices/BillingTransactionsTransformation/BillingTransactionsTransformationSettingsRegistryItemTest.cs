using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.eHub.Testing
{
	[TestedType(typeof(BillingTransactionsTransformationSettingsRegistryItem))]
	sealed class BillingTransactionsTransformationSettingsRegistryItemTest : StronglyTypedRegistryItemTestCase<BillingTransactionsTransformationSettings>
	{
		protected override StronglyTypedRegistryItem<BillingTransactionsTransformationSettings, BillingTransactionsTransformationSettings> GetNewRegistryItem()
		{
			return new BillingTransactionsTransformationSettingsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers);
		}
	}
}
