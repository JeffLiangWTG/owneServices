using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(EventsVisibilityRegistryControl))]
	sealed class EventsVisibilityRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return null;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((EventsVisibilityRegistryControl)control).ReadOnly;
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new EventsVisibilityRegistryControl();
		}
	}
}
