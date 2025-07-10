using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class BackDateAPInvoicesConfigurationControl : RegistryZUserControl
	{
		ZArchitecture.ZGrid PostDateConfigurationGrid;
		ZArchitecture.GUI.ZGroupBox PostDateGroupBox;

		public BackDateAPInvoicesConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			PostDateConfigurationGrid.ReadOnly = readOnly;
		}
	}
}

