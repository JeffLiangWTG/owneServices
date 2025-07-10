using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ChargeableFactorRegistryControl))]
	sealed class ChargeableFactorRegistryControlTestCase : RegistryBusinessObjectTemplateZUserControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ChargeableFactor();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			ChargeableFactorRegistryControl chargeableFactorControl = (ChargeableFactorRegistryControl)control;

			return chargeableFactorControl.zMetricFactorDropEdit.ReadOnly &&
				chargeableFactorControl.zImperialFactorDropEdit.ReadOnly;
		}
	}
}
