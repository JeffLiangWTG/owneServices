#if DEBUG

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class CrossTradeDebtorConfigurationControl
	{
		public ZArchitecture.ZGrid CrossTradeDebtorConfigGrid_ForTestOnly
		{
			get { return CrossTradeDebtorConfigGrid; }
			set { CrossTradeDebtorConfigGrid = value; }
		}
	}
}

#endif
