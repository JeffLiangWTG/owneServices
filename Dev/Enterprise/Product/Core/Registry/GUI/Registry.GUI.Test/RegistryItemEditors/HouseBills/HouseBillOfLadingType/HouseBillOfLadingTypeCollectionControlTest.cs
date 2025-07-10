using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(HouseBillOfLadingTypeCollectionControl))]
	sealed class HouseBillOfLadingTypeCollectionControlTest : Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new HouseBillOfLadingTypeCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((HouseBillOfLadingTypeCollectionControl)control).Grid.ReadOnly;
		}
	}
}
