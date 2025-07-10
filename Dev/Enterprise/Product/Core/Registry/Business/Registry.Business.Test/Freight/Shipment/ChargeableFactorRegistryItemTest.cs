using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ChargeableFactorRegistryItem))]
	sealed class ChargeableFactorRegistryItemTest : StronglyTypedRegistryItemTestCase<ChargeableFactor>
	{
		protected override StronglyTypedRegistryItem<ChargeableFactor, ChargeableFactor> GetNewRegistryItem()
		{
			return new ChargeableFactorRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, new ChargeableFactor());
		}
	}
}
