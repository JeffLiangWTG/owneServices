using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(CashFlowCategoryBasedOnDebtorGroupControl))]
	class CashFlowCategoryBasedOnDebtorGroupControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CashFlowCategoryBasedOnDebtorGroupCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CashFlowCategoryBasedOnDebtorGroupControl)control).CashFlowCategoryBasedOnDebtorGrid_ForTestOnly.ReadOnly;
		}
	}
}
