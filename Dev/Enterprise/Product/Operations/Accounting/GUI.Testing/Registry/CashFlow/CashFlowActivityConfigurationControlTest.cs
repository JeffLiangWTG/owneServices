using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(CashFlowActivityConfigurationControl))]
	class CashFlowActivityConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CashFlowActivityConfigurationCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CashFlowActivityConfigurationControl)control).CashFlowActivityConfigurationGrid_ForTestOnly.ReadOnly;
		}

		protected override void BashForDescriptionColumn(Control controlToBash)
		{
		}
	}
}
