using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class PeriodReopenLevelsControl : RegistryZUserControl
	{
		public PeriodReopenLevelsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			PeriodReopenLevelsGrid.ReadOnly = readOnly;
		}
	}
}

