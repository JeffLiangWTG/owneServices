namespace Enterprise.Registry.GUI
{
	public partial class SecondarySMTPServerControl : RegistryZUserControl
	{
		public SecondarySMTPServerControl()
		{
			InitializeComponent();
		}

		#region Overriden

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			try
			{
				base.SetControlOrBusinessEntityReadOnly(readOnly);
				SecondarySMTPServersGrid.ReadOnly = readOnly;
			}
			finally
			{
				ResumeLayout(false);
			}
		}

		#endregion
	}
}
