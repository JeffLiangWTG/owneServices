namespace Enterprise.Registry.GUI
{
	public partial class DefaultNumberOfDecimalsControl : RegistryZUserControl
	{
		public DefaultNumberOfDecimalsControl()
			: this(true)
		{
		}

		public DefaultNumberOfDecimalsControl(bool showTransportMode)
		{
			InitializeComponent();

			if (!showTransportMode)
			{
				this.DefaultNumberOfDecimalsGrid.RemoveFromAvailableColumns("TransportMode");
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			DefaultNumberOfDecimalsGrid.ReadOnly = readOnly;
		}
	}
}
