using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(MilestoneEventUpdatesRegistryControl))]
	class MilestoneEventUpdatesRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return null;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((MilestoneEventUpdatesRegistryControl)control).ReadOnly;
		}
	}
}
