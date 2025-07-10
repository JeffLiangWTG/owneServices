using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CalculateDeliveryDueDateOptionsRegistryItemControl))]
	sealed class CalculateDeliveryDueDateOptionsRegistryItemControlTest : RegistryBusinessObjectTemplateZUserControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CalculateDeliveryDueDateOptions();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return (control as CalculateDeliveryDueDateOptionsRegistryItemControl).ReadOnly;
		}
	}
}
