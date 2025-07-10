namespace Enterprise.Registry.GUI
{
	public partial class IndexShardRegistryControl : RegistryZUserControl
	{
		public IndexShardRegistryControl()
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
