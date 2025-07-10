using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public partial class DeleteExpiredRatesUserControl : RegistryZUserControl
	{
		public DeleteExpiredRatesUserControl(DeleteExpiredRatesWrapper wrapper)
		{
			InitializeComponent();
			DeleteExpiredRatesWrapper = wrapper;

			BatchSizeCalcEdit.MaxValue = 5000;
		}

		public DeleteExpiredRatesWrapper DeleteExpiredRatesWrapper { get; }

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			ExpiredPeriodInYearsCalcEdit.Enabled = !readOnly;
			BatchSizeCalcEdit.Enabled = !readOnly;
		}

		public void SetBusinessEntityValue(DeleteExpiredRates deleteExpiredRates)
		{
			DeleteExpiredRatesWrapper.ExpiredRates = deleteExpiredRates;
			SetDataBinding(DeleteExpiredRatesWrapper, "");
		}
	}
}
