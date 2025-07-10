namespace Enterprise.Registry.GUI
{
	partial class InterchangeSenderProxyUserControl : RegistryZUserControl
	{
		public InterchangeSenderProxyUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.ParentGrid.ReadOnly = readOnly;
		}
	}
}
