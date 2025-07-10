#if DEBUG

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class PeriodSetUpForm
	{
		public Business.PeriodManagement.NewYearPeriodSettings NewYearSettings_ForTestOnly
		{
			get { return NewYearSettings; }
			set { NewYearSettings = value; }
		}

		public ZArchitecture.GUI.ZButton OKButton_ForTestOnly
		{
			get { return OKButton; }
			set { OKButton = value; }
		}
	}
}

#endif
