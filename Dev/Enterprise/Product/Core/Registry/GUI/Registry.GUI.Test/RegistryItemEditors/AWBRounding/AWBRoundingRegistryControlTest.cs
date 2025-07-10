using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(AWBRoundingRegistryControl))]
	sealed class AWBRoundingRegistryControlTest : Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new AWBRoundingCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((AWBRoundingRegistryControl)control).ChargeableWeightRoundingGrid.ReadOnly;
		}
	}
}
