using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class DirectDebitFileCreationURLsControl : RegistryZUserControl
	{
		public DirectDebitFileCreationURLsControl()
		{
			InitializeComponent();
		}

		#region Overriden

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			DirectDebitFileURLGrid.ReadOnly = readOnly;
		}

		#endregion
	}
}

