using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(FreeWaitingTimeRegistryItem))]
	sealed class FreeWaitingTimeRegistryItemTest : StronglyTypedRegistryItemTestCase<FreeWaitingTimeCollection>
	{
		protected override StronglyTypedRegistryItem<FreeWaitingTimeCollection, FreeWaitingTimeCollection> GetNewRegistryItem()
		{
			return new FreeWaitingTimeRegistryItem("", null, null, null, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, new FreeWaitingTimeCollection());
		}
	}
}
