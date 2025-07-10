using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(ConsolidatedBillingSettingsRegistryUserControl))]
	public class ConsolidatedBillingSettingsRegistryUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new ConsolidatedBillingSettingCollection();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ConsolidatedBillingSettingsRegistryUserControl)control).grid.ReadOnly;
		}
	}
}
