using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(EventLogsListProvidersAndTheirCapacity))]
	public class EventLogsListProvidersAndTheirCapacityTest : RegistryBusinessObjectTemplateTestCase<EventLogsListProvidersAndTheirCapacity>
	{
		protected override EventLogsListProvidersAndTheirCapacity GetBusinessObjectToClone()
		{
			return new EventLogsListProvidersAndTheirCapacity();
		}

		protected override EventLogsListProvidersAndTheirCapacity GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
