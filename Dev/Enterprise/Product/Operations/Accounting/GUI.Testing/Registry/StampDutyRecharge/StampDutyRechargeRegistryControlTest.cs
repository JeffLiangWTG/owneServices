using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(StampDutyRechargeRegistryControl))]
	class StampDutyRechargeRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new StampDutyRecharge();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return !((StampDutyRechargeRegistryControl)control).StampDutyRechargeOrganizationTypeDropEdit_ForTestOnly.Enabled && !((StampDutyRechargeRegistryControl)control).StampDutyRechargeTransactionTypeDropEdit_ForTestOnly.Enabled;
		}
	}
}
