namespace Enterprise.Registry.GUI
{
	public partial class IndexDurationRegistryControl : RegistryZUserControl
	{
		public IndexDurationRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			RetentionTimeGrid.ReadOnly = readOnly;
		}
	}
}
