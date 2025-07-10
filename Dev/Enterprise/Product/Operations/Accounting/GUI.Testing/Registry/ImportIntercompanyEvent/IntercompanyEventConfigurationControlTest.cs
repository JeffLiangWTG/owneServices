using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Registry.GUI;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(IntercompanyEventConfigurationControl))]
	public class IntercompanyEventConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new IntercompanyEventConfiguration();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((IntercompanyEventConfigurationControl)control).IntercompanyEventSettingGrid.ReadOnly;
		}

		protected override bool ShouldIgnoreMissingBindingMember(System.Windows.Forms.Control control)
		{
			if (control.Name == "enableEventConfiguration" || control.Name == "disableEventConfiguration")
			{
				return true;
			}
			return base.ShouldIgnoreMissingBindingMember(control);
		}
	}
}
