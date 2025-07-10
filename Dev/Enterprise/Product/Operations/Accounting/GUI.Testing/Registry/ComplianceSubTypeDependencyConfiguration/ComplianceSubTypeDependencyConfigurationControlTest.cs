using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ComplianceSubTypeDependencyConfigurationControl))]
	class ComplianceSubTypeDependencyConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ComplianceSubTypeDependencyConfigurationCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ComplianceSubTypeDependencyConfigurationControl)control).ComplianceSubTypeDependencyConfigurationGrid_ForTestOnly.ReadOnly;
		}
	}
}
