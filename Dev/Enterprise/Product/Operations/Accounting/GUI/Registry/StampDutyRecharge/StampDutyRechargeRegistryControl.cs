using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class StampDutyRechargeRegistryControl : RegistryZUserControl
	{
		public StampDutyRechargeRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			StampDutyRechargeOrganizationTypeDropEdit.Enabled = !readOnly;
			StampDutyRechargeTransactionTypeDropEdit.Enabled = !readOnly;
		}
	}
}
