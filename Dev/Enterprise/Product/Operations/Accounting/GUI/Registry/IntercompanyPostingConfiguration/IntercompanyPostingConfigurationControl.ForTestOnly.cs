#if DEBUG

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class IntercompanyPostingConfigurationControl
	{
		public ZArchitecture.ZGrid IntercompanyPostingConfigurationGrid_ForTestOnly
		{
			get { return IntercompanyPostingConfigurationGrid; }
			set { IntercompanyPostingConfigurationGrid = value; }
		}
	}
}

#endif
