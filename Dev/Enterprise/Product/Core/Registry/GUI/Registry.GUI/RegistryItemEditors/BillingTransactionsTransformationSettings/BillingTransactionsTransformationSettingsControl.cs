namespace Enterprise.Registry.GUI.eHub
{
	public partial class BillingTransactionsTransformationSettingsControl : RegistryZUserControl
	{
		public BillingTransactionsTransformationSettingsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(ReadOnly);
			currentFixIndexCalcEdit.ReadOnly = readOnly;
			currentFixStateCalcEdit.ReadOnly = readOnly;
		}
	}
}
