namespace Enterprise.Registry.GUI
{
	public partial class BankAccountBasedOnCurrencyControl : RegistryZUserControl
	{
		public BankAccountBasedOnCurrencyControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			BankAccountBasedOnCurrencyGrid.ReadOnly = readOnly;
		}
	}
}
