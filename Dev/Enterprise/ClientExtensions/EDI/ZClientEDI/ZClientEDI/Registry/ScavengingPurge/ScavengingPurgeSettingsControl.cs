using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class ScavengingPurgeSettingsControl : RegistryZUserControl
	{
		public ScavengingPurgeSettingsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			gridScavengingItems.ReadOnly = readOnly;
		}
	}
}
