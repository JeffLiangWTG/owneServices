using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(CashFlowCategoryBasedOnCreditorGroupControl))]
	class CashFlowCategoryBasedOnCreditorGroupControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CashFlowCategoryBasedOnCreditorGroupCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CashFlowCategoryBasedOnCreditorGroupControl)control).CashFlowCategoryBasedOnCreditorGrid_ForTestOnly.ReadOnly;
		}
	}
}
