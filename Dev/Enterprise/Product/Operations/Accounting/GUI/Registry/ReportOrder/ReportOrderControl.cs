using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ReportOrderControl : RegistryZUserControl
	{
		public ReportOrderControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ReportOrderGrid.ReadOnly = readOnly;
		}
	}
}

