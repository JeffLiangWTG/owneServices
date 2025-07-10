using Enterprise.Registry.GUI;

namespace Enterprise.Customs.FR.GUI.Registry
{
	public partial class TriggerPointsConfigurationControl : RegistryZUserControl
	{
		public TriggerPointsConfigurationControl()
		{
			InitializeComponent();
		}
		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			zTriggerPointConfigurationGroupbox.Enabled = !readOnly;
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
