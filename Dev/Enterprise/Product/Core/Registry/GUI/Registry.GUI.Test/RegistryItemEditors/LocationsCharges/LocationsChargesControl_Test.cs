using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(LocationsChargesControl))]
	sealed class LocationsChargesControl_Test : Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new LocationsChargesCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			LocationsChargesControl locationsChargesControl = (LocationsChargesControl)control;

			return locationsChargesControl.LocationsGrid.ReadOnly &&
				locationsChargesControl.ChargesGrid.ReadOnly;
		}
	}
}
