namespace Enterprise.Registry.GUI
{
	public partial class SupportEdwDataSourceReportRegistryControl : RegistryZUserControl
	{
		public SupportEdwDataSourceReportRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ReportListGrid.ReadOnly = readOnly;
		}
	}
}
