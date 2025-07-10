using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ContainerPenaltyFreeDaysOptionsRegistryItemControl))]
	sealed class ContainerPenaltyFreeDaysOptionsRegistryItemControlTest : RegistryBusinessObjectTemplateZUserControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ContainerPenaltyFreeDaysOptions();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return (control as ContainerPenaltyFreeDaysOptionsRegistryItemControl).ReadOnly;
		}
	}
}
