using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(EventsRegistryItem))]
	class EventsRegistryItemTest : StronglyTypedRegistryItemTestCase<EventRegistryBusinessObjectCollection>
	{
		protected override StronglyTypedRegistryItem<EventRegistryBusinessObjectCollection, EventRegistryBusinessObjectCollection> GetNewRegistryItem()
		{
			return new EventsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
