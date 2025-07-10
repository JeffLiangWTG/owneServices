using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(CostVarianceApprovalRegistryControl))]
	class CostVarianceApprovalRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CostVarianceApproval();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CostVarianceApprovalRegistryControl)control).AuthorisationRequirementsGrid_ForTestOnly.ReadOnly &&
				!((CostVarianceApprovalRegistryControl)control).VarianceCalculationStyleDropEdit_ForTestOnly.Enabled &&
				!((CostVarianceApprovalRegistryControl)control).VarianceComparisonOptionDropEdit_ForTestOnly.Enabled;
		}
	}
}
