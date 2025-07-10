using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ComplianceSubTypeAttributionRuleSetControl))]
	class ComplianceSubTypeAttributionRuleSetControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ComplianceSubTypeAttributionRuleSet();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ComplianceSubTypeAttributionRuleSetControl)control).ZDropEdit1_ForTestOnly.ReadOnly;
		}
	}
}
