using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(EventLogsListProvidersAndTheirCapacityControl))]
	class EventLogsListProvidersAndTheirCapacityControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new EventLogsListProvidersAndTheirCapacityCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((EventLogsListProvidersAndTheirCapacityControl)control).Grid.ReadOnly;
		}
	}
}
