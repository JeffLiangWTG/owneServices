using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(JobClosureConfigurationControl))]
	class JobClosureConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new JobClosureConfigurationHeader();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((JobClosureConfigurationControl)control).JobClosureConfigGrid_ForTestOnly.ReadOnly;
		}
	}
}
