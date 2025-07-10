#if DEBUG

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class IntercompanyClearingConfigurationControl
	{
		public ZArchitecture.ZGrid IntercompanyClearingConfigurationGrid_ForTestOnly
		{
			get { return IntercompanyClearingConfigurationGrid; }
			set { IntercompanyClearingConfigurationGrid = value; }
		}
	}
}

#endif
