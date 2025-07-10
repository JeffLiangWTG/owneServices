using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(BillingUnitCountAdjustmentRegistryControl))]
	class BillingUnitCountAdjustmentRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new BillingUnitCountAdjustmentCollection();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			var control = (BillingUnitCountAdjustmentRegistryControl)control1;
			return control.PriceCodeGrid.ReadOnly && control.AdjustmentGrid.ReadOnly;
		}
	}
}
