using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class TestRigRegistryControl : RegistryZUserControl
	{
		public TestRigRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			OptionsGrid.ReadOnly = readOnly;
		}

		internal ZGrid Grid => OptionsGrid;
	}
}
