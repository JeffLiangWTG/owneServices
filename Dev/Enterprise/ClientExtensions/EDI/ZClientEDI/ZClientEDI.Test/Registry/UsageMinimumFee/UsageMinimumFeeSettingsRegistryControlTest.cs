using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(UsageMinimumFeeSettingsRegistryControl))]
	public class UsageMinimumFeeSettingsRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new UsageMinimumFeeSettings();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var usageMinimumFeeSettingsRegistryControl = (UsageMinimumFeeSettingsRegistryControl)control;
			return usageMinimumFeeSettingsRegistryControl.ExposedMinimumFeeUsageGridForTesting.ReadOnly;
		}
	}
}
