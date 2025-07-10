using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(LandedCostingPreferencesControl))]
	sealed class LandedCostingPreferencesControl_Test : Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new LandedCostingGroupCollection(null, Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			LandedCostingPreferencesControl landedCostingPreferencesControl = (LandedCostingPreferencesControl)control;

			return landedCostingPreferencesControl.LandedCostingGroupsGrid.ReadOnly &&
				landedCostingPreferencesControl.ChargeGroupsAndChargeCodesGrid.ReadOnly;
		}
	}
}
