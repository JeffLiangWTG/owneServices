#if DEBUG

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class BackDateAPInvoicesConfigurationControl
	{
		public ZArchitecture.ZGrid PostDateConfigurationGrid_ForTestOnly
		{
			get { return PostDateConfigurationGrid; }
			set { PostDateConfigurationGrid = value; }
		}
	}
}

#endif
