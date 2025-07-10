using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(EventLogsListProvidersAndTheirCapacityRegistryItem))]
	class EventLogsListProvidersAndTheirCapacityRegistryItemTest : StronglyTypedRegistryItemTestCase<EventLogsListProvidersAndTheirCapacityCollection>
	{
		protected override StronglyTypedRegistryItem<EventLogsListProvidersAndTheirCapacityCollection, EventLogsListProvidersAndTheirCapacityCollection> GetNewRegistryItem()
		{
			return new EventLogsListProvidersAndTheirCapacityRegistryItem("EventLogsListProvidersAndTheirCapacity", (NoResString)"Dummy Category", (NoResString)"Dummy Caption", null);
		}
	}
}
