namespace Enterprise.Registry.GUI
{
	public partial class PerformanceReportingControl : RegistryZUserControl
	{
		public PerformanceReportingControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			MetricGrid.ReadOnly = readOnly;
			CategoryGrid.ReadOnly = readOnly;
		}
	}
}
