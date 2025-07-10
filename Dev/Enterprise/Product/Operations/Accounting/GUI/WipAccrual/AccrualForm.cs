namespace Enterprise.Accounting.GUI.WipAccrual
{
	public partial class AccrualForm : WIPAccrualForm
	{
		public AccrualForm()
		{
		}

		public AccrualForm(Business.WIPAccrual.Accrual accrual)
			: base(accrual)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}

