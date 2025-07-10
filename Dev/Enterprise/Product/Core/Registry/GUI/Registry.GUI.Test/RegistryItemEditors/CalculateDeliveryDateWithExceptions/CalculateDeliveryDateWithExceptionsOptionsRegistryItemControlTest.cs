using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CalculateDeliveryDateWithExceptionsOptionsRegistryItemControl))]
	sealed class CalculateDeliveryDateWithExceptionsOptionsRegistryItemControlTest : RegistryBusinessObjectTemplateZUserControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CalculateDeliveryDateWithExceptionsOptions();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return (control as CalculateDeliveryDateWithExceptionsOptionsRegistryItemControl).ReadOnly;
		}
	}
}
