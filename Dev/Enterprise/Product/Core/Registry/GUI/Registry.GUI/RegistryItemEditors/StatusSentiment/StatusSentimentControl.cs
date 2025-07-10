namespace Enterprise.Registry.GUI
{
	public partial class StatusSentimentControl : RegistryZUserControl
	{
		public StatusSentimentControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			statusSentimentsGrid.ReadOnly = readOnly;
		}
	}
}
