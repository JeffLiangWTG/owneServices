using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DecimalArrayRegistryItem))]
	sealed class DecimalArrayRegistryItemTest : StronglyTypedRegistryItemTestCase<decimal[]>
	{
		protected override StronglyTypedRegistryItem<decimal[], decimal[]> GetNewRegistryItem()
		{
			return new DecimalArrayRegistryItem("a", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System);
		}
	}
}
