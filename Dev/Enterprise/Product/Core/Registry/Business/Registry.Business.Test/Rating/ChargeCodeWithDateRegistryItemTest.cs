using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ChargeCodeWithDateRegistryItem))]
	sealed class ChargeCodeWithDateRegistryItemTest : StronglyTypedRegistryItemTestCase<ChargeCodeWithDate>
	{
		protected override StronglyTypedRegistryItem<ChargeCodeWithDate, ChargeCodeWithDate> GetNewRegistryItem()
		{
			return new ChargeCodeWithDateRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}
	}
}
