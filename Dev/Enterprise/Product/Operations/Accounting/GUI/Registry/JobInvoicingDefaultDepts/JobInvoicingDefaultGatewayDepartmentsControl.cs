using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class JobInvoicingDefaultGatewayDepartmentsControl : RegistryZUserControl
	{
		public JobInvoicingDefaultGatewayDepartmentsControl()
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
