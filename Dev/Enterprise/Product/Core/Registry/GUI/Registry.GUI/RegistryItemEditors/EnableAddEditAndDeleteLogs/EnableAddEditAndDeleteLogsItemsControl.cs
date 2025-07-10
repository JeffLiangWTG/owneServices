namespace Enterprise.Registry.GUI
{
	public partial class EnableAddEditAndDeleteLogsItemsControl : RegistryZUserControl
	{
		public EnableAddEditAndDeleteLogsItemsControl()
		{
			InitializeComponent();
			EnableAddEditAndDeleteLogsItemsGrid.AllowSorting = false;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			EnableAddEditAndDeleteLogsItemsGrid.ReadOnly = readOnly;
		}
	}
}
