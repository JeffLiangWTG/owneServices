using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(BranchLevelPostingConfigurationControl))]
	class BranchLevelPostingConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new BranchLevelPostingConfiguration();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((BranchLevelPostingConfigurationControl)control).BranchGroupSettingsGrid_ForTestOnly.ReadOnly;
		}

		protected override bool ShouldIgnoreMissingBindingMember(System.Windows.Forms.Control control)
		{
			if (control.Name == "enableBranchLevelPosting" || control.Name == "disableBranchLevelPosting")
			{
				return true;
			}
			return base.ShouldIgnoreMissingBindingMember(control);
		}
	}
}
