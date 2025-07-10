using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class CASSChargeCodesControl : RegistryZUserControl
	{
		public CASSChargeCodesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CASSComponentsGrid.ReadOnly = readOnly;
		}
	}
}
