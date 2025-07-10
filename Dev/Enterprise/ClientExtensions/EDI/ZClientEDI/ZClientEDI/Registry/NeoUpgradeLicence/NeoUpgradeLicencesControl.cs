using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class NeoUpgradeLicencesControl : RegistryZUserControl
	{
		public NeoUpgradeLicencesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.NeoUpgradeLicencesGrid.ReadOnly = readOnly;
		}
	}
}
