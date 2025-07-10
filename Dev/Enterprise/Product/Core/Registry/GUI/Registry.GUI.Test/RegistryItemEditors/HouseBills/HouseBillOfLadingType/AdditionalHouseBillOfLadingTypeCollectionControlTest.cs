using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(AdditionalHouseBillOfLadingTypeCollectionControl))]
	sealed class AdditionalHouseBillOfLadingTypeCollectionControlTest : Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new AdditionalHouseBillOfLadingTypeCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((AdditionalHouseBillOfLadingTypeCollectionControl)control).Grid.ReadOnly;
		}
	}
}
