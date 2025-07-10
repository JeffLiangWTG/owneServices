using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EventVisibilityOverrideRegistryItem))]
	sealed class EventVisibilityRegistryOverrideItemTest : StronglyTypedRegistryItemTestCase<EventVisibilityOverrideCollection>
	{
		protected override StronglyTypedRegistryItem<EventVisibilityOverrideCollection, EventVisibilityOverrideCollection> GetNewRegistryItem()
		{
			return new EventVisibilityOverrideRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new EventVisibilityOverrideCollection());
		}
	}
}
