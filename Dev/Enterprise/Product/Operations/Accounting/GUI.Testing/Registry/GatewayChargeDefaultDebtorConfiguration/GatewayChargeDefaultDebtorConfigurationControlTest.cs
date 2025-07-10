using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(GatewayChargeDefaultDebtorConfigurationControl))]
	class GatewayChargeDefaultDebtorConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new GatewayChargeDefaultDebtorConfigurationCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((GatewayChargeDefaultDebtorConfigurationControl)control).GatewayChargeDefaultDebtorConfigurationGrid_ForTestOnly.ReadOnly;
		}
	}
}
