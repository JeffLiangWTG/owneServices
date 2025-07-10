namespace Enterprise.Registry.GUI
{
	public partial class RatingTokenAuthenticationControl : RegistryZUserControl
	{
		public RatingTokenAuthenticationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			TokenAuthenticationGrid.ReadOnly = readOnly;
		}
	}
}
