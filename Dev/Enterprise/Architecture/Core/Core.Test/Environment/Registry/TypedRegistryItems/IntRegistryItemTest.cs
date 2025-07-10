using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(IntRegistryItem))]
	public class IntRegistryItemTest : StronglyTypedRegistryItemTestCase<int>
	{
		protected override StronglyTypedRegistryItem<int, int> GetNewRegistryItem()
		{
			return new IntRegistryItem("", null, null, null, RegistryStorageFlags.All);
		}
	}
}
