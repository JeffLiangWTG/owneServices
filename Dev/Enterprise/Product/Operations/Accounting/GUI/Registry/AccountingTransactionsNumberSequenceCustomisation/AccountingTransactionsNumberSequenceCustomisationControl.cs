using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	internal partial class AccountingTransactionsNumberSequenceCustomisationControl : RegistryBusinessObjectTemplateZUserControl
	{
		public AccountingTransactionsNumberSequenceCustomisationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			elementsGrid.ReadOnly = readOnly;
		}
	}
}
