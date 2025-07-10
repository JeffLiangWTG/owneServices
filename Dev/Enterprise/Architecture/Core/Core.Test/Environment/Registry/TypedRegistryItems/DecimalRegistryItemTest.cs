using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(DecimalRegistryItem))]
	sealed class DecimalRegistryItemTest : StronglyTypedRegistryItemTestCase<decimal>
	{
		protected override StronglyTypedRegistryItem<decimal, decimal> GetNewRegistryItem()
		{
			return new DecimalRegistryItem("", null, null, null, RegistryStorageFlags.All);
		}
	}
}
