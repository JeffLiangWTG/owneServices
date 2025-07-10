using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ComplianceSubTypeAllocationOverrideConfigurationControl))]
	class ComplianceSubTypeAllocationOverrideConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ComplianceSubTypeAllocationOverrideConfigurationCollection(null, null, CountryCodes.Brazil);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var grid = (ZArchitecture.ZGrid)control.Controls.Find("ComplianceSubTypeAllocationOverrideConfigurationGrid", false)[0];
			return grid.ReadOnly;
		}
	}
}
