using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CreditTemporaryIncreaseAuthorisationSettingsRegistryItem))]
	sealed class CreditTemporaryIncreaseAuthorisationSettingsRegistryItemTest : StronglyTypedRegistryItemTestCase<CreditTemporaryIncreaseAuthorisationSettingsCollection>
	{
		protected override StronglyTypedRegistryItem<CreditTemporaryIncreaseAuthorisationSettingsCollection, CreditTemporaryIncreaseAuthorisationSettingsCollection> GetNewRegistryItem()
		{
			return new CreditTemporaryIncreaseAuthorisationSettingsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
