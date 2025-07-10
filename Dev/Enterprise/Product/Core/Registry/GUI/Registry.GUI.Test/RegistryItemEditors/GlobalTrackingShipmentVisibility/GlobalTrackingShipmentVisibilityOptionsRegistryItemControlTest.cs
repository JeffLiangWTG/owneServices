using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(GlobalTrackingShipmentVisibilityOptionsRegistryItemControl))]
	sealed class GlobalTrackingShipmentVisibilityOptionsRegistryItemControlTest : RegistryBusinessObjectTemplateZUserControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new GlobalTrackingShipmentVisibilityOptions();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return (control as GlobalTrackingShipmentVisibilityOptionsRegistryItemControl).ReadOnly;
		}
	}
}
