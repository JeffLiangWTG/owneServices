namespace Enterprise.Registry.GUI
{
	public partial class AutoratingViaPortControl : RegistryZUserControl
	{
		public AutoratingViaPortControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			viaConfigurationGrid.ReadOnly = readOnly;
			viaSettingsGrid.ReadOnly = readOnly;
		}

#if DEBUG

		public bool IsControlOrBusinessEntityReadOnly
			=> viaConfigurationGrid.ReadOnly;

#endif
	}
}
