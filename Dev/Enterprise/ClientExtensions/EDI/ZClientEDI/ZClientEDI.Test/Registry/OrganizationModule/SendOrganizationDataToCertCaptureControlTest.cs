using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(SendOrganizationDataToCertCaptureControl))]
	class SendOrganizationDataToCertCaptureControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new SendOrganizationDataToCertCapture();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var sendControl = (SendOrganizationDataToCertCaptureControl)control;
			return !sendControl.yesRadioButton.Enabled && !sendControl.noRadioButton.Enabled && !sendControl.SendAllOrganizationsButton.Enabled;
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control.Name == "yesRadioButton" || control.Name == "noRadioButton")
			{
				return true;
			}
			else
			{
				return base.ShouldIgnoreMissingBindingMember(control);
			}
		}
	}
}
