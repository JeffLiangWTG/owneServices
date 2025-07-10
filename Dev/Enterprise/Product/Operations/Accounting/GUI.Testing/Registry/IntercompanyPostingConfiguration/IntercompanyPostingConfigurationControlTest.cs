using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(IntercompanyPostingConfigurationControl))]
	class IntercompanyPostingConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new IntercompanyPostingConfigurationCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((IntercompanyPostingConfigurationControl)control).IntercompanyPostingConfigurationGrid_ForTestOnly.ReadOnly;
		}
	}
}
