using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class AYCTriggerTypeSettingsControl : RegistryZUserControl
	{
		public AYCTriggerTypeSettingsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			TextBoxPrimaryChargeCode.ReadOnly = readOnly;
			TextBoxSecondaryChargeCode.ReadOnly = readOnly;
		}
	}
}
