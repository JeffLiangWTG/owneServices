using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class JobInvoicingDefaultDepartmentsControl : RegistryZUserControl
	{
		public JobInvoicingDefaultDepartmentsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			Grid.ReadOnly = readOnly;
		}
	}
}
