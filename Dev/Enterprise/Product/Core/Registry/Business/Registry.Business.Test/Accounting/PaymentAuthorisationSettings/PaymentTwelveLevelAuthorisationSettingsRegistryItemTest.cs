using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PaymentTwelveLevelAuthorisationSettingsRegistryItem))]
	sealed class PaymentTwelveLevelAuthorisationSettingsRegistryItemTest : StronglyTypedRegistryItemTestCase<PaymentTwelveLevelAuthorisationSettingsCollection>
	{
		protected override StronglyTypedRegistryItem<PaymentTwelveLevelAuthorisationSettingsCollection, PaymentTwelveLevelAuthorisationSettingsCollection> GetNewRegistryItem()
		{
			return new PaymentTwelveLevelAuthorisationSettingsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
