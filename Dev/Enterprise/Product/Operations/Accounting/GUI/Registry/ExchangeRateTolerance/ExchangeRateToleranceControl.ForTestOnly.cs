#if DEBUG
namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ExchangeRateToleranceControl
	{
		public ZArchitecture.ZGrid ExchangeRateToleranceGrid_ForTestOnly
		{
			get { return ExchangeRateToleranceGrid; }
			set { ExchangeRateToleranceGrid = value; }
		}
	}
}
#endif
