using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(GatewayChargeDefaultInvoiceTargetJobConfigurationControl))]
	class GatewayChargeDefaultInvoiceTargetJobConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new GatewayChargeDefaultInvoiceTargetJobConfigurationCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((GatewayChargeDefaultInvoiceTargetJobConfigurationControl)control).GatewayChargeDefaultInvoiceTargetJobConfigurationGrid_ForTestOnly.ReadOnly;
		}
	}
}
