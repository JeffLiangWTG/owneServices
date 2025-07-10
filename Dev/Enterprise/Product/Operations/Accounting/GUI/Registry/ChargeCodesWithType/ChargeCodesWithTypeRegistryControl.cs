using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ChargeCodesWithTypeRegistryControl : RegistryZUserControl
	{
		public ChargeCodesWithTypeRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ChargeCodesWithTypeGrid.ReadOnly = readOnly;
		}
	}
}
