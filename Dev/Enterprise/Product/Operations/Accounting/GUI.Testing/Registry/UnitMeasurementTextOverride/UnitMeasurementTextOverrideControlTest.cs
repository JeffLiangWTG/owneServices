using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(UnitMeasurementTextOverrideControl))]
	class UnitMeasurementTextOverrideControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new UnitMeasurementTextOverrideCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((UnitMeasurementTextOverrideControl)control).UnitMeasurementTextOverrideGrid_ForTestOnly.ReadOnly;
		}
	}
}
