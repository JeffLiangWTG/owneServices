using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(RatesServiceUrlRegistryItem))]
	sealed class RatesServiceUrlRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new RatesServiceUrlRegistryItem("a", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System, string.Empty);
		}
	}
}
