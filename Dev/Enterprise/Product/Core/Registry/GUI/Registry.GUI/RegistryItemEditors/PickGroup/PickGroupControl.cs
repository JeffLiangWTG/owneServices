namespace Enterprise.Registry.GUI
{
	partial class PickGroupControl : RegistryZUserControl
	{
		public PickGroupControl()
		{
			InitializeComponent();
		}

		#region ReadOnly

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			PickGroupGrid.ReadOnly = readOnly;
		}

		#endregion

		public object Data
		{
			get { return DataSource; }
			set { SetDataBinding(value, null); }
		}
	}
}
