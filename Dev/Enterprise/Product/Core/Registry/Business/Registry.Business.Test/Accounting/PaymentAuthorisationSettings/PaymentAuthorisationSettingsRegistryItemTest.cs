using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PaymentAuthorisationSettingsRegistryItem))]
	sealed class PaymentAuthorisationSettingsRegistryItemTest : StronglyTypedRegistryItemTestCase<PaymentAuthorisationSettingsCollection>
	{
		protected override StronglyTypedRegistryItem<PaymentAuthorisationSettingsCollection, PaymentAuthorisationSettingsCollection> GetNewRegistryItem()
		{
			return new PaymentAuthorisationSettingsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
