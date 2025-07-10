using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(ChargeableWeightRoundingRegistryControl))]
	sealed class ChargeableWeightRoundingRegistryControl_Test : Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ChargeableWeightRoundingCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ChargeableWeightRoundingRegistryControl)control).ChargeableWeightRoundingGrid.ReadOnly;
		}
	}
}
