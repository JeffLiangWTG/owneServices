using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class InvoiceRollupAndGroupDescriptionControl : RegistryZUserControl
	{
		public InvoiceRollupAndGroupDescriptionControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			InvoiceRollupAndGroupDescriptionGrid.ReadOnly = readOnly;
		}

		void InvoiceRollupAndGroupDescriptionGrid_RowDeleting(object sender, ZArchitecture.RowsDeletingEventArgs e)
		{
			e.Cancel = true;
		}
	}
}

