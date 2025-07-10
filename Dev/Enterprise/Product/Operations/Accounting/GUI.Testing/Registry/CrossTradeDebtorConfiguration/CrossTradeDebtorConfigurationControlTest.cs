using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(CrossTradeDebtorConfigurationControl))]
	class CrossTradeDebtorConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CrossTradeDebtorConfigurationHeader();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CrossTradeDebtorConfigurationControl)control).CrossTradeDebtorConfigGrid_ForTestOnly.ReadOnly;
		}
	}
}
