using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(SystemDefinableRegistryImageCollectionControl))]
	sealed class SystemDefinableRegistryImageCollectionControl_Test : Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new SystemDefinableRegistryImageCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			SystemDefinableRegistryImageCollectionControl registryImageCollectionControl = (SystemDefinableRegistryImageCollectionControl)control;
			return registryImageCollectionControl.RegistryImageGrid.ReadOnly &&
				registryImageCollectionControl.RegistryImageSelectionControl.ReadOnly;
		}
	}
}
