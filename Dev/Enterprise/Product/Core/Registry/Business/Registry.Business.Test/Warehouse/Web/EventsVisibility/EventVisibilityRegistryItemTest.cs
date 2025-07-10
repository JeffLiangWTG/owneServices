using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EventVisibilityRegistryItem))]
	sealed class EventVisibilityRegistryItemTest : StronglyTypedRegistryItemTestCase<EventVisibilityCollection>
	{
		protected override StronglyTypedRegistryItem<EventVisibilityCollection, EventVisibilityCollection> GetNewRegistryItem()
		{
			return new EventVisibilityRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, new EventVisibilityCollection());
		}
	}
}
