using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(UsageBillingSettingsRegistryControl))]
	class UsageBillingSettingsRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new UsageBillingSettings();
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			var control = (UsageBillingSettingsRegistryControl)control1;
			return control.PriceListGrid.ReadOnly && control.BranchGrid.ReadOnly;
		}
	}
}
