using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ChargeableWeightRoundingRegistryItem))]
	sealed class ChargeableWeightRoundingRegistryItemTest : StronglyTypedRegistryItemTestCase<ChargeableWeightRoundingCollection>
	{
		protected override StronglyTypedRegistryItem<ChargeableWeightRoundingCollection, ChargeableWeightRoundingCollection> GetNewRegistryItem()
		{
			return new ChargeableWeightRoundingRegistryItem(string.Empty,
					null, null, null, RegistryStorageFlags.System, new ChargeableWeightRoundingCollection());
		}
	}
}
