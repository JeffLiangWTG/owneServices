namespace Enterprise.Registry.GUI
{
	public partial class UOMPackTypeControl : RegistryZUserControl
	{
		public UOMPackTypeControl()
		{
			InitializeComponent();
		}

		#region SetControlOrBusinessEntityReadOnly

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			PackTypeGrid.ReadOnly = readOnly;
		}

		#endregion
	}
}
