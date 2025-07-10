using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(HBLDeliveryPriorityControl))]
	sealed class HBLDeliveryPriorityControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
			=> new HBLDeliveryPriorityConfigCollection();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
			=> ((HBLDeliveryPriorityControl)control).IsControlOrBusinessEntityReadOnly;
	}
}
