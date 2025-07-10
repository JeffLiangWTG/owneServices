using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(RegistryImageCollectionControl))]
	sealed class RegistryImageCollectionControl_Test : Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new RegistryImageCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			RegistryImageCollectionControl registryImageCollectionControl = (RegistryImageCollectionControl)control;
			return registryImageCollectionControl.RegistryImageGrid.ReadOnly &&
				registryImageCollectionControl.RegistryImageSelectionControl.ReadOnly;
		}
	}
}
