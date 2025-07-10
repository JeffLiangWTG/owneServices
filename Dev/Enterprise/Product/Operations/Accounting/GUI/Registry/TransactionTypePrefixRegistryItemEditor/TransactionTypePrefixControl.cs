using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class TransactionTypePrefixControl : RegistryZUserControl
	{
		public TransactionTypePrefixControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			TransactionTypePrefixGrid.ReadOnly = readOnly;
		}
	}
}

