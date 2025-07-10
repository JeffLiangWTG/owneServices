#if DEBUG

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class CashFlowActivityConfigurationControl
	{
		public ZArchitecture.ZGrid CashFlowActivityConfigurationGrid_ForTestOnly
		{
			get { return CashFlowActivityConfigurationGrid; }
			set { CashFlowActivityConfigurationGrid = value; }
		}
	}
}

#endif
