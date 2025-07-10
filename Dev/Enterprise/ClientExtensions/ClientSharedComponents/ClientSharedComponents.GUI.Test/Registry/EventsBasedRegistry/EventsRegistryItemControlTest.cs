using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(EventsRegistryItemControl))]
	public class EventsRegistryItemControlTest : RegistryZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			EventsRegistryItemControl control = (EventsRegistryItemControl)control1;
			return control.ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new EventRegistryBusinessObjectCollection();
		}
	}
}
