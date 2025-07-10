using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(HBLDeliveryModeControl))]
	sealed class HBLDeliveryModeControl_Test : Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new HBLDeliveryModes();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((HBLDeliveryModeControl)control).HBLDeliveryModeGrid.ReadOnly;
		}
	}
}
