namespace Enterprise.Registry.GUI
{
	public partial class VerboseLoggingControl : RegistryZUserControl
	{
		public VerboseLoggingControl()
		{
			InitializeComponent();
		}

		internal ZArchitecture.ZGrid Grid => codeDescriptionTimeGrid;

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			codeDescriptionTimeGrid.ReadOnly = readOnly;
		}
	}
}
