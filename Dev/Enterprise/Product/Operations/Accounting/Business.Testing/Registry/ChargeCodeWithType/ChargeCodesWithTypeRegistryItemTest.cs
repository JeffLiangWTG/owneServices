using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ChargeCodeWithTypeRegistryItem))]
	class ChargeCodesWithTypeRegistryItemTest : StronglyTypedRegistryItemTestCase<ChargeCodeWithTypeCollection>
	{
		protected override StronglyTypedRegistryItem<ChargeCodeWithTypeCollection, ChargeCodeWithTypeCollection> GetNewRegistryItem()
		{
			return new ChargeCodeWithTypeRegistryItem(string.Empty,
					null, null, null, RegistryStorageFlags.System, new ChargeCodeWithTypeCollection());
		}
	}
}
