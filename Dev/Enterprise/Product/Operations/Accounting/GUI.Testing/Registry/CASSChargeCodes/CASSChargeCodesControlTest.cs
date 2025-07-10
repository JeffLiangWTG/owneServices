using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(CASSChargeCodesControl))]
	class CASSChargeCodesControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return AccountingConfigurationRegistry.Instance.CASSChargeCodes.DefaultValue;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CASSChargeCodesControl)control).CASSComponentsGrid_ForTestOnly.ReadOnly;
		}
	}
}
