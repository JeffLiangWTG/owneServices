using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PaymentThreeLevelAuthorisationSettingsRegistryItem))]
	sealed class PaymentThreeLevelAuthorisationSettingsRegistryItemTest : StronglyTypedRegistryItemTestCase<PaymentThreeLevelAuthorisationSettingsCollection>
	{
		protected override StronglyTypedRegistryItem<PaymentThreeLevelAuthorisationSettingsCollection, PaymentThreeLevelAuthorisationSettingsCollection> GetNewRegistryItem()
		{
			return new PaymentThreeLevelAuthorisationSettingsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
